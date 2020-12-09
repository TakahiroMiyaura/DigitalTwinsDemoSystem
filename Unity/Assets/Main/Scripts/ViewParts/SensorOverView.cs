// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using TMPro;
using UnityEngine;

public class SensorOverView : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro maintenanceStatusText = null;

    [SerializeField]
    private TextMeshPro sensorNameText = null;

    [SerializeField]
    private TextMeshPro statusText = null;

    public string SensorName
    {
        set => sensorNameText.text = value;
        get => sensorNameText.text;
    }

    public string Status
    {
        set => statusText.text = value;
        get => statusText.text;
    }

    public string MaintenanceStatus
    {
        set => maintenanceStatusText.text = value;
        get => maintenanceStatusText.text;
    }

    public string Uuid { set; get; }

    public void SetStatusColor(Color color)
    {
        statusText.color = color;
    }


    public void SetMaintenanceStatusColor(Color color)
    {
        maintenanceStatusText.color = color;
    }
}