// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DemoADTFunctionsApp
{
    internal static class AdtUtilities
    {
        public static async Task<string> FindParentAsync(DigitalTwinsClient client, string child, string relname,
            ILogger log)
        {
            // Find parent using incoming relationships
            try
            {
                var rels = client.GetIncomingRelationshipsAsync(child);

                await foreach (var ie in rels)
                {
                    if (ie.RelationshipName == relname)
                        return ie.SourceId;
                }
            }
            catch (RequestFailedException exc)
            {
                log.LogInformation($"*** Error in retrieving parent:{exc.Status}:{exc.Message}");
            }

            return null;
        }



        public static bool TryGetTwinData(DigitalTwinsClient client, string twinsId,string targetModel, out JObject json,ILogger log)
        {
            json = null;

            // Find parent using incoming relationships
            try
            {
                var response = client.GetDigitalTwin(twinsId);
                var resData = response.Value;
                log.LogInformation($"Find twinsId:{twinsId} -> {resData}");
                var result = (JObject)JsonConvert.DeserializeObject(resData);
                if (result["$metadata"]["$model"].Value<string>().Contains(targetModel))
                {
                    json = result;
                    return true;
                }
                log.LogInformation($"TwinsId:{twinsId} excluded this EventTrigger.");
                return false;
            }
            catch (RequestFailedException exc)
            {
                log.LogInformation($"*** Error in retrieving parent:{exc.Status}:{exc.Message}");
            }

            return false;
        }
        public static async Task UpdateTwinPropertyAsync(DigitalTwinsClient client, string twinId, string propertyPath,
            object value, ILogger log)
        {
            // If the twin does not exist, this will log an error
            try
            {
                var uou = new UpdateOperationsUtility();
                uou.AppendReplaceOp(propertyPath, value);
                var patchPayload = uou.Serialize();
                log.LogInformation($"UpdateTwinPropertyAsync sending {patchPayload}");

                await client.UpdateDigitalTwinAsync(twinId, patchPayload);
            }
            catch (RequestFailedException exc)
            {
                log.LogInformation($"*** Error:{exc.Status}/{exc.Message}");
            }
        }
    }
}