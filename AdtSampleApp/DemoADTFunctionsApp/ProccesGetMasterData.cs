// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DemoADTFunctionsApp
{
    public static class ProccesGetMasterData
    {
        [FunctionName("ProcessGetSensorMessages")]
        public static IActionResult GetSensorMessages(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetSensorMessages")]
            HttpRequest req,
            [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                PartitionKey = "MasterData",
                Id = "DemoADSensorMessages")]
            Document document,
            ILogger log)
        {
            log.LogInformation("HTTP trigger function [ProcessGetSensorMessages] processed a request.");
            // 入力ドキュメントをログ出力
            log.LogInformation("input document info");
            var messages = document.GetPropertyValue<object>("messages");
            var msg = " description:" + messages;
            log.LogInformation(msg);

            // Fetching the name from the path parameter in the request URL
            return new OkObjectResult(messages);
        }
    }
}