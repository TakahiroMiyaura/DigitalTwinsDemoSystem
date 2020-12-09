// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace DemoADTFunctionsApp
{
    public static class ProcessDTToCosmosDB
    {
        private const string adtAppId = "https://digitaltwins.azure.net";
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly string cosmosDBConnectionStrings = Environment.GetEnvironmentVariable(CosmosDBConst.CONNECTION_STRING_SETTING);
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("ProcessDTToCosmosDB")]
        public static void DTToCosmosDB([EventGridTrigger] EventGridEvent eventGridEvent, [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                CreateIfNotExists = true)]
            out object sensorInfos,
            ILogger log)
        {
            log.LogInformation("Event grid trigger function [ProcessDTToCosmosDB] processed a request.");

            sensorInfos = null;

            DigitalTwinsClient client;
            // Authenticate on ADT APIs
            try
            {
                var cred = new ManagedIdentityCredential(adtAppId);
                client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred,
                    new DigitalTwinsClientOptions {Transport = new HttpClientTransport(httpClient)});
                log.LogInformation("ADT service client connection created.");
            }
            catch (Exception e)
            {
                log.LogError($"ADT service client connection failed. {e}");
                return;
            }

            Document doc = null;
            if (client != null)
            {
                try
                {
                    if (eventGridEvent != null && eventGridEvent.Data != null)
                    {
                        var twinId = eventGridEvent.Subject;
                        JObject twins;

                        if (AdtUtilities.TryGetTwinData(client, twinId, "dtmi:demo:Sensor", out twins, log))
                        {
                            var message = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());

                            log.LogInformation(
                                $"Reading event from {twinId}: {eventGridEvent.EventType}: {message["data"]}");

                            doc = CreateDocument(twins, message,log).GetAwaiter().GetResult();
                        }
                        else
                        {
                            log.LogInformation($"TwinsId:{twinId} excluded this EventTrigger.");
                        }

                    }
                }
                catch (Exception e)
                {
                    doc = null;
                    log.LogError(e.ToString());
                }
            } 
            sensorInfos = doc;
        }

        private static async Task<Document> CreateDocument(JObject twins, JObject message,ILogger Log)
        {
            // Read properties which values have been changed in each operation

            CosmosClient cosmosClient = new CosmosClient(cosmosDBConnectionStrings, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct
            });
            Container container = cosmosClient.GetContainer(CosmosDBConst.DATABASE_NAME, CosmosDBConst.COLLECTION_NAME);
            Document doc = await container.ReadItemAsync<Document>(twins["UUID"].Value<string>(), new PartitionKey("Sensor")) ;
            if (doc == null)
            {
                doc = new Document();
                doc.SetPropertyValue("id", twins["UUID"]);
                doc.SetPropertyValue("type", "Sensor");
            }

            foreach (var operation in message["data"]["patch"])
            {
                var opValue = (string) operation["op"];
                if (opValue.Equals("replace"))
                {
                    var propertyPath = (string) operation["path"];

                    if (propertyPath.Equals("/AnchorsId"))
                    {
                        doc.SetPropertyValue("AnchorsId", operation["value"].Value<string>());
                    }
                    else if (propertyPath.Equals("/SensorName"))
                    {
                        doc.SetPropertyValue("SensorName", operation["value"].Value<string>());
                    }
                    else if (propertyPath.Equals("/Temperature"))
                    {
                        doc.SetPropertyValue("Temperature", operation["value"].Value<float>());
                    }
                    else if (propertyPath.Equals("/Pressure"))
                    {
                        doc.SetPropertyValue("Pressure", operation["value"].Value<float>());
                    }
                    else if (propertyPath.Equals("/Humidity"))
                    {
                        doc.SetPropertyValue("Humidity", operation["value"].Value<float>());
                    }
                    else if (propertyPath.Equals("/Light"))
                    {
                        doc.SetPropertyValue("Light", operation["value"].Value<float>());
                    }
                    else if (propertyPath.Equals("/Co2"))
                    {
                        doc.SetPropertyValue("Co2", operation["value"].Value<float>());
                    }
                    else if (propertyPath.Equals("/TVOC"))
                    {
                        doc.SetPropertyValue("TVOC", operation["value"].Value<float>());
                    }
                    else if (propertyPath.Equals("/IsAlert"))
                    {
                        doc.SetPropertyValue("IsAlert", operation["value"].Value<bool>());
                    }
                    else if (propertyPath.Equals("/StatusCode"))
                    {
                        doc.SetPropertyValue("StatusCode", operation["value"].Value<string>());
                    }
                }
            }

            return doc;
        }
    }
}