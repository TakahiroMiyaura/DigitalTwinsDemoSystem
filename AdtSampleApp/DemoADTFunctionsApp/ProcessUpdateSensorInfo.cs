// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DemoADTFunctionsApp
{
    public static class ProcessUpdateSensorInfo
    {
        [FunctionName("ProcessCreateSensorInfo")]
        public static IActionResult CreateSensorInfo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SensorInfo/create")]
            HttpRequest req,
            [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                CreateIfNotExists = true)]
            out object sensorInfos,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function [ProcessCreateSensorInfo] processed a request.");

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            sensorInfos = JsonConvert.DeserializeObject(requestBody);

            return new OkObjectResult("Set Data Successfully.");
        }

        [FunctionName("ProcessUpdateSensorStatus")]
        public static IActionResult UpdateSensorStatus(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SensorInfo/update/uuid/{id}")]
            HttpRequest req,
            [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                CreateIfNotExists = true)]
            out object sensorInfos,
            [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                PartitionKey = "Sensor",
                Id = "{id}")]
            Document document,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function [ProcessUpdateSensorStatus] processed a request.");
            log.LogInformation($" -Update SensorInfo.Id:{document.Id}");
            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var status = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
            foreach (var data in status)
            {
                if (data.Key.Equals("Id")) continue;
                log.LogInformation($" >Update SensorInfo.{data.Key}:{data.Value}");
                document.SetPropertyValue(data.Key, data.Value);
            }

            sensorInfos = document;

            return new OkObjectResult("Update Data Successfully.");
        }
    }
}