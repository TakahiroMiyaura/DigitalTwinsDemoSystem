// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using Assets.Main.Scripts;
using UnityEngine;

public class MaintenanceReport : MonoBehaviour
{
    private Dictionary<string, string> messageInfo = new Dictionary<string, string>
    {
        {"MaintenanceFinish", "There wasn't any problem."},
        {"RequestMaintenance", "Check the sensor and environments."},
        {"Pending", "Trying to find out the cause of the problem."},
        {"ChangeParts", "The issue has been solved by changing parts."}
    };

    private Dictionary<string, string> statusInfo = new Dictionary<string, string>
    {
        {"MaintenanceFinish", "Complete Work."},
        {"RequestMaintenance", "RequestMaintenance"},
        {"Pending", "Pending"},
        {"ChangeParts", "Complete Work(Change Parts)."}
    };

    private string uuid;

    private void Start()
    {
        uuid = GetComponentInParent<SensorDetailView>().Uuid;
    }

    public async void SendMaintenaceReport(string status)
    {
        await FunctionsAppsForADT.Instance.UpdateMaintenanceInfoAsync(uuid, messageInfo[status], statusInfo[status]);
    }
}