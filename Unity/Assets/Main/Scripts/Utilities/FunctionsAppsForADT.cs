// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Main.Scripts
{
    public class FunctionsAppsForADT
    {
        private static readonly object lockObj = new object();

        private static FunctionsAppsForADT instance;

        private static readonly HttpClient _httpClient;

        private string getSensorInfoById =
            "https://XXXXX.azurewebsites.net/api/SensorInfos/uuid/{0}?code=XXXX"; // TODO:DeployしたAzure FunctionsのURLに変更

        private string getSensorInfoUrl =
            "https://XXXXX.azurewebsites.net/api/SensorInfos/lists?code=XXXX"; // TODO:DeployしたAzure FunctionsのURLに変更

        private string getSensorMessages =
            "https://XXXXX.azurewebsites.net/api/GetSensorMessages?code=XXXX"; // TODO:DeployしたAzure FunctionsのURLに変更

        public string updateMaintenanceInfo =
            "https://XXXXX.azurewebsites.net/api/MaintenanceInfo/update/uuid/{0}?code=XXXX"; // TODO:DeployしたAzure FunctionsのURLに変更

        private string updateSensorStatus =
            "https://XXXXX.azurewebsites.net/api/SensorInfo/update/uuid/{0}?code=XXXX"; // TODO:DeployしたAzure FunctionsのURLに変更

        static FunctionsAppsForADT()
        {
            _httpClient = new HttpClient();
        }

        public static FunctionsAppsForADT Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new FunctionsAppsForADT();
                        }
                    }
                }

                return instance;
            }
        }

        public async Task UpdateMaintenanceInfoAsync(string uuid, string maintenanceInfo, string status,
            Action action = null)
        {
            var sendData = "{\"MaintenanceInfo\":\"" + maintenanceInfo + "\",\"Status\":\"" + status + "\"}";
            var content = new StringContent(sendData, Encoding.UTF8, @"application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(string.Format(updateMaintenanceInfo, uuid)),
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                action?.Invoke();
            }
            else
            {
                throw new InvalidOperationException("Status:" + response.StatusCode + ",message:" +
                                                    await response.Content.ReadAsStringAsync());
            }
        }

        public void UpdateMaintenanceInfo(string uuid, string maintenanceInfo, string status, Action action = null)
        {
            UpdateMaintenanceInfoAsync(uuid, maintenanceInfo, status, action).GetAwaiter().GetResult();
        }

        public async Task UpdateSensorStatusAsync(string uuid, Dictionary<SensorParams, object> sensorParams,
            Action action = null)
        {
            var sb = new StringBuilder();
            foreach (var param in sensorParams)
            {
                sb.Append($"\"{param.Key}\":");
                if (param.Value is string)
                {
                    sb.Append($"\"{param.Value}\"");
                }
                else if (param.Value is bool)
                {
                    sb.Append(param.Value.ToString().ToLower());
                }
                else
                {
                    sb.Append($"{param.Value}");
                }

                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1).Insert(0, "{").Append("}");

            Debug.Log("sendData : " + sb);
            Debug.Log("id : " + uuid);
            var content = new StringContent(sb.ToString(), Encoding.UTF8, @"application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(string.Format(updateSensorStatus, uuid)),
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                action?.Invoke();
            }
            else
            {
                throw new InvalidOperationException("Status:" + response.StatusCode + ",message:" +
                                                    await response.Content.ReadAsStringAsync());
            }
        }

        public void UpdateSensorStatus(string uuid, Dictionary<SensorParams, object> sensorParams, Action action = null)
        {
            UpdateSensorStatusAsync(uuid, sensorParams, action).GetAwaiter().GetResult();
        }

        public async Task<Dictionary<string, string>> GetSensorMessagesAsync(
            Action<Dictionary<string, string>> action = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(getSensorMessages)
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var jsonObject = DeserializeArray<KeyValueDataList>(json);

                var dic = new Dictionary<string, string>();
                foreach (var keyValueData in jsonObject.root)
                {
                    dic.Add(keyValueData.Key, keyValueData.Values);
                }

                action?.Invoke(dic);

                return dic;
            }

            throw new InvalidOperationException("Status:" + response.StatusCode + ",message:" +
                                                await response.Content.ReadAsStringAsync());
        }

        public Dictionary<string, string> GetSensorMessages(Action<Dictionary<string, string>> action = null)
        {
            return GetSensorMessagesAsync(action).GetAwaiter().GetResult();
        }

        public async Task<SensorInfo> GetSensorByIdAsync(string uuid, Action<SensorInfo> action = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(string.Format(getSensorInfoById, uuid))
            };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var jsonObject = Deserialize<SensorInfo>(json);
                jsonObject.id = jsonObject.id.Replace(" ", "");
                action?.Invoke(jsonObject);

                return jsonObject;
            }

            throw new InvalidOperationException("Status:" + response.StatusCode + ",message:" +
                                                await response.Content.ReadAsStringAsync());
        }

        public SensorInfo GetSensorById(string uuid, Action<SensorInfo> action = null)
        {
            return GetSensorByIdAsync(uuid, action).GetAwaiter().GetResult();
        }

        public async Task<SensorInfo[]> GetSensorListsAsync(Action<SensorInfo[]> action = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(getSensorInfoUrl)
            };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var jsonObject = DeserializeArray<SensorList>(json).root;
                foreach (var sensorInfo in jsonObject)
                {
                    sensorInfo.id = sensorInfo.id.Replace(" ", "");
                }

                action?.Invoke(jsonObject);

                return jsonObject;
            }

            throw new InvalidOperationException("Status:" + response.StatusCode + ",message:" +
                                                await response.Content.ReadAsStringAsync());
        }

        public SensorInfo[] GetSensorLists(Action<SensorInfo[]> action = null)
        {
            return GetSensorListsAsync(action).GetAwaiter().GetResult();
        }


        public static T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public static T DeserializeArray<T>(string json)
        {
            var _json = "{" + $"\"root\": {json}" + "}";
            return JsonUtility.FromJson<T>(_json);
        }

        public static string SerializeArray<T>(T jsonObject)
        {
            return JsonUtility.ToJson(jsonObject);
        }

        [Serializable]
        public class KeyValueData
        {
            public string Key;
            public string Values;
        }

        [Serializable]
        public class KeyValueDataList
        {
            public KeyValueData[] root;
        }
    }
}