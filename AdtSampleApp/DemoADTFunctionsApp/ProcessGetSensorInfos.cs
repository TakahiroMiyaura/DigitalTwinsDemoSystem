// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DemoADTFunctionsApp
{
    public static class ProcessGetSensorInfos
    {
        [FunctionName("ProcessGetSensorInfosById")]
        public static IActionResult GetSensorInfosById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "SensorInfos/uuid/{id}")]
            HttpRequest req,
            [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                PartitionKey ="Sensor",
                Id = "{id}")]
            Document document,
            ILogger log)
        {

            log.LogInformation("HTTP trigger function [ProcessGetSensorInfosById] processed a request.");
            // 入力ドキュメントをログ出力
            log.LogInformation("input document info");
            var msg = " description:" + document;
            log.LogInformation(msg);

            // Fetching the name from the path parameter in the request URL
            return new OkObjectResult(document);
        }

        [FunctionName("ProcessGetSensorLists")]
        public static IActionResult GetSensorLists(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "SensorInfos/lists")]
            HttpRequest req,
            [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                SqlQuery = "SELECT * FROM c WHERE c.type='Sensor'",
                PartitionKey = "Sensor")]
            IEnumerable<Document> document,
            ILogger log)
        {
            log.LogInformation("HTTP trigger function [ProcessGetSensorLists] processed a request.");
            // 入力ドキュメントをログ出力
            log.LogInformation("input document info");
            var msg = " description:" + document;
            log.LogInformation(msg);

            // Fetching the name from the path parameter in the request URL
            return new OkObjectResult(document);
        }
    }
}