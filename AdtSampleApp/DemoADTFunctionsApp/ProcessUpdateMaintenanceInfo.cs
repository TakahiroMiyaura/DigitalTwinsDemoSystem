using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DemoADTFunctionsApp
{
    public static class ProcessUpdateMaintenanceInfo
    {
        [FunctionName("ProcessUpdateMaintenanceInfo")]
        public static  IActionResult UpdateMaintenanceInfo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "MaintenanceInfo/update/uuid/{id}")] HttpRequest req,
            [CosmosDB(
                CosmosDBConst.DATABASE_NAME,
                CosmosDBConst.COLLECTION_NAME,
                ConnectionStringSetting = CosmosDBConst.CONNECTION_STRING_SETTING,
                PartitionKey = "Sensor",
                Id = "{id}")]
            Document query,
            ILogger log)
        {
            log.LogInformation("HTTP trigger function [ProcessUpdateMaintenanceInfo] processed a request.");
            Document doc = new Document();
            string bodyStr;
            using (var stream = new StreamReader(req.Body))
            {
                bodyStr = stream.ReadToEnd();
            }

            if (query != null && bodyStr.Length > 0)
            {
                var json = (JObject) JsonConvert.DeserializeObject(bodyStr);

                query.SetPropertyValue("MaintenanceInfo", json["MaintenanceInfo"]);
                query.SetPropertyValue("Status", json["Status"]);
            }

            return new OkObjectResult("");
        }
    }
}
