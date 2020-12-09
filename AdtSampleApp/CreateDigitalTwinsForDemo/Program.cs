// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Serialization;
using Azure.Identity;
using ExcelDataReader;

namespace CreateDigitalTwinsForDemo
{
    internal class Program
    {
        const string clientId = "<your-application-ID>";
        const string tenantId = "<your-directory-ID>";
        const string adtInstanceUrl = "https://<your-Azure-Digital-Twins-instance-hostName>";
        private static async Task Main(string[] args)
        {
            // Connect to Azure Digital Twins.
            var credentials = new InteractiveBrowserCredential(tenantId, clientId);
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credentials);
            Console.WriteLine("Service client created – ready to go");

            Console.WriteLine();
            Console.WriteLine("Upload a model");
            // Read the model Folder,and create list of model data.
            var typeList = new List<string>();
            foreach (var path in Directory.GetFiles("Models"))
            {
                var dtdl = File.ReadAllText(path);
                typeList.Add(dtdl);
            }

            // Read a list of models back from the service, if the models already exist in list of model data, delete the model.
            var modelDataList = client.GetModelsAsync();
            await foreach (var md in modelDataList)
            {
                if (typeList.Count(x => x.Contains(md.Id)) > 0)
                {
                    await client.DeleteModelAsync(md.Id);
                    Console.WriteLine($"Delete model: {md.Id}");
                }
            }

            // Upload the model to the service
            try
            {
                await client.CreateModelsAsync(typeList);
            }
            catch (RequestFailedException rex)
            {
                Console.WriteLine($"Load model: {rex.Status}:{rex.Message}");
                Console.WriteLine(
                    "If Status is 409(Conflict), these models is already created.You can be ignore this error.");
            }

            // Read a list of models back from the service
            modelDataList = client.GetModelsAsync();
            await foreach (var md in modelDataList)
            {
                Console.WriteLine($"Type name: {md.DisplayName}: {md.Id}");
            }

            // Create Digital Twins for Demo system.
            // Read buildingScenario.xlsx
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = File.Open("buildingScenario.xlsx", FileMode.Open, FileAccess.Read))
            using (var excelReader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = excelReader.AsDataSet();
                var isHeader = true;
                foreach (DataRow datarow in result.Tables[0].Rows)
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    try
                    {
                        var twindId = datarow.ItemArray[1].ToString();

                        // if digital twins already added, delete the data. 
                        var data = client.QueryAsync($"SELECT * FROM DIGITALTWINS WHERE $dtId = '{twindId}'");
                        await foreach (var queryResult in data)
                        {
                            if (queryResult.Length == 0) continue;
                            var relationships = client.GetRelationshipsAsync(twindId);
                            await foreach (var relation in relationships)
                            {
                                var relationshipInfos =
                                    JsonSerializer.Deserialize<Dictionary<string, object>>(relation);
                                var relationId = relationshipInfos["$relationshipId"].ToString();
                                Console.WriteLine($"Detected relationship: {relationId}. Delete this relationship...");
                                await client.DeleteRelationshipAsync(twindId, relationId);
                            }

                            await client.DeleteDigitalTwinAsync(twindId);
                            Console.WriteLine($"Detected twin: {twindId}. Delete this data...");
                        }

                        // create Digital Twin data.
                        var twinData = new BasicDigitalTwin();
                        twinData.Metadata.ModelId = datarow.ItemArray[0].ToString();
                        twinData.Id = twindId;
                        var config =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(datarow.ItemArray[4].ToString());
                        foreach (var key in config.Keys)
                        {
                            twinData.CustomProperties.Add(key, config[key]);
                            Console.WriteLine($"twin: {twindId}.Custom Properties:key:{key},values:{config[key]}");
                        }

                        await client.CreateDigitalTwinAsync(twindId, JsonSerializer.Serialize(twinData));
                        Console.WriteLine($"Created twin: {twindId}");

                        // create relationship data.
                        var relationshipFrom = datarow.ItemArray[2].ToString();
                        if (relationshipFrom.Length > 0)
                        {
                            await CreateRelationship(client, relationshipFrom, twindId);
                        }
                    }
                    catch (RequestFailedException rex)
                    {
                        Console.WriteLine($"Create twin error: {rex.Status}:{rex.Message}");
                    }
                }
            }
        }

        /// <summary>
        ///     create relationship.
        /// </summary>
        /// <param name="client">Digital Twins Client</param>
        /// <param name="srcId">source digital twins Id </param>
        /// <param name="targetId">target digital twins Id</param>
        /// <returns></returns>
        public async static Task CreateRelationship(DigitalTwinsClient client, string srcId, string targetId)
        {
            var relationship = new BasicRelationship
            {
                TargetId = targetId,
                Name = "contains"
            };

            try
            {
                var relId = $"{srcId}-contains->{targetId}";
                await client.CreateRelationshipAsync(srcId, relId, JsonSerializer.Serialize(relationship));
                Console.WriteLine("Created relationship successfully");
            }
            catch (RequestFailedException rex)
            {
                Console.WriteLine($"Create relationship error: {rex.Status}:{rex.Message}");
            }
        }
    }
}