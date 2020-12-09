// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.Net.Http;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Serialization;
using Azure.Identity;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DemoADTFunctionsApp
{
    public class ProcessHubToDTEvents
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("ProcessHubToDTEvents")]
        public async void HubToDTEvents([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {

            log.LogInformation("Event grid trigger function [ProcessHubToDTEvents] processed a request.");

            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");

            try
            {
                //Authenticate with Digital Twins
                var cred = new ManagedIdentityCredential("https://digitaltwins.azure.net");
                var client = new DigitalTwinsClient(
                    new Uri(adtInstanceUrl), cred, new DigitalTwinsClientOptions
                        {Transport = new HttpClientTransport(httpClient)});
                log.LogInformation("ADT service client connection created.");

                
                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    log.LogInformation(eventGridEvent.Data.ToString());

                    // Reading deviceId and temperature for IoT Hub JSON
                    var deviceMessage = (JObject) JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    var deviceId = (string) deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    var sensorName = deviceMessage["body"]["sensorName"];
                    var uuid = deviceMessage["body"]["uuid"];
                    var temperature = deviceMessage["body"]["temperature"];
                    var pressure = deviceMessage["body"]["pressure"];
                    var humidity = deviceMessage["body"]["humidity"];
                    var light = deviceMessage["body"]["light"];
                    var Co2 = deviceMessage["body"]["Co2"];
                    var TVOC = deviceMessage["body"]["TVOC"];
                    var statusCode = JsonConvert.SerializeObject(deviceMessage["body"]["statusCode"]).Replace("\\","");
                    var isAlert = deviceMessage["body"]["isAlert"];
                    log.LogInformation(
                        $"Device:{deviceId} uuid is:{uuid} sensorName is:{sensorName} temperature is:{temperature} pressure is:{pressure} humidity is:{humidity} light is:{light} Co2 is:{Co2} TVOC is:{TVOC} isAlert is:{isAlert} statusCode is:{statusCode}");

                    //Update twin using device temperature
                    var uou = new UpdateOperationsUtility();
                    uou.AppendReplaceOp("/SensorName", sensorName.Value<string>());
                    uou.AppendReplaceOp("/Temperature", temperature.Value<double>());
                    uou.AppendReplaceOp("/Pressure", pressure.Value<double>());
                    uou.AppendReplaceOp("/Humidity", humidity.Value<double>());
                    uou.AppendReplaceOp("/Light", light.Value<double>());
                    uou.AppendReplaceOp("/Co2", Co2.Value<double>());
                    uou.AppendReplaceOp("/TVOC", TVOC.Value<double>());
                    uou.AppendReplaceOp("/UUID", uuid.Value<string>());
                    uou.AppendReplaceOp("/StatusCode", statusCode);
                    uou.AppendReplaceOp("/IsAlert", isAlert.Value<bool>());

                    await client.UpdateDigitalTwinAsync(deviceId, uou.Serialize());
                }
            }
            catch (Exception e)
            {
                log.LogError($"Error in ingest function: {e.Message}");
            }
        }
    }
}