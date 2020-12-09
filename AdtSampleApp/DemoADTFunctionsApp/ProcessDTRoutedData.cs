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
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DemoADTFunctionsApp
{
    public static class ProcessDTRoutedData
    {
        private const string adtAppId = "https://digitaltwins.azure.net";
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("ProcessDTRoutedData")]
        public static async Task DTRoutedData([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation("Event grid trigger function [ProcessDTRoutedData] processed a request.");

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

                            var message = (JObject) JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());


                            log.LogInformation(
                                $"Reading event from {twinId}: {eventGridEvent.EventType}: {message}");

                            //Find and update parent Twin
                            var parentId = await AdtUtilities.FindParentAsync(client, twinId, "contains", log);
                            if (parentId != null)
                            {
                                // Read properties which values have been changed in each operation
                                foreach (var operation in message["data"]["patch"])
                                {
                                    var opValue = (string) operation["op"];
                                    if (opValue.Equals("replace"))
                                    {
                                        var propertyPath = (string) operation["path"];

                                        if (propertyPath.Equals("/Temperature"))
                                        {
                                            await AdtUtilities.UpdateTwinPropertyAsync(client, parentId, propertyPath,
                                                operation["value"].Value<float>(), log);
                                        }

                                        if (propertyPath.Equals("/Humidity"))
                                        {
                                            await AdtUtilities.UpdateTwinPropertyAsync(client, parentId, propertyPath,
                                                operation["value"].Value<float>(), log);
                                        }

                                        if (propertyPath.Equals("/IsAlert"))
                                        {
                                            await AdtUtilities.UpdateTwinPropertyAsync(client, parentId, propertyPath,
                                                operation["value"].Value<bool>(), log);
                                        }

                                        if (propertyPath.Equals("/StatusCode"))
                                        {
                                            await AdtUtilities.UpdateTwinPropertyAsync(client, parentId, propertyPath,
                                                operation["value"].Value<string>(), log);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            log.LogInformation($"TwinsId:{twinId} excluded this EventTrigger.");
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogError(e.ToString());
                }
            }
        }
    }
}