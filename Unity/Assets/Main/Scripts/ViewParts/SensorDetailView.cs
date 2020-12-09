// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using Assets.Main.Scripts;
using Com.Reseul.SpatialAnchors;
using TMPro;
using UnityEngine;


public class SensorDetailView : MonoBehaviour
{
    public Action<SensorDetailView> OnSensorInfoUpdate = null;

    [SerializeField]
    private GameObject currentStep = null;

    [SerializeField]
    private float interval = 10;

    [SerializeField]
    private GameObject nextStep = null;

    [SerializeField]
    private TextMeshPro textMeshMaintenanceInfo = null;

    [SerializeField]
    private TextMeshPro textMeshMaintenanceStatus = null;

    [SerializeField]
    private TextMeshPro textMeshProCo2 = null;

    [SerializeField]
    private TextMeshPro textMeshProHumidity = null;

    [SerializeField]
    private TextMeshPro textMeshProLight = null;

    [SerializeField]
    private TextMeshPro textMeshProLocationStatus = null;

    [SerializeField]
    private TextMeshPro textMeshProPressure = null;

    [SerializeField]
    private TextMeshPro textMeshProSensorName = null;

    [SerializeField]
    private TextMeshPro textMeshProTemp = null;

    [SerializeField]
    private TextMeshPro textMeshProTVOC = null;

    [SerializeField]
    private TextMeshPro textMeshProUuid = null;

    [SerializeField]
    private string uuid = null;

    private string sensorName = null;
    private float temperature = 0;
    private float pressure = 0;
    private float humidity = 0;
    private float lightValue = 0;
    private float co2 = 0;
    private float tvoc = 0;
    private string status = null;
    private string[] statusCodes =null;
    private string maintenanceInfo = null;
    private string maintenanceStatus = null;
    private float timer = 0;

    public float Temperature
    {
        set
        {
            textMeshProTemp.text = value.ToString("0.0");
            temperature = value;
        }
        get => temperature;
    }

    public float Pressure
    {
        set
        {
            textMeshProPressure.text = value.ToString("0.0");
            pressure = value;
        }
        get => pressure;
    }

    public float Humidity
    {
        set
        {
            textMeshProHumidity.text = value.ToString("0.0");
            humidity = value;
        }
        get => humidity;
    }

    public float Light
    {
        set
        {
            textMeshProLight.text = value.ToString("0.0");
            lightValue = value;
        }
        get => lightValue;
    }

    public float Co2
    {
        set
        {
            textMeshProCo2.text = value.ToString("0.0");
            co2 = value;
        }
        get => co2;
    }

    public float TVOC
    {
        set
        {
            textMeshProTVOC.text = value.ToString("0.0");
            tvoc = value;
        }
        get => tvoc;
    }

    public string Uuid
    {
        set
        {
            textMeshProUuid.text = value;
            uuid = value;
            timer = interval + 1f;
        }
        get => uuid;
    }

    public string Status
    {
        set
        {
            textMeshProLocationStatus.text = value;
            status = value;
        }
        get => status;
    }

    public string MaintenanceStatus
    {
        set
        {
            textMeshMaintenanceStatus.text = value;
            maintenanceStatus = value;
        }
        get => maintenanceStatus;
    }


    public string MaintenanceInfo
    {
        set
        {
            textMeshMaintenanceInfo.text = value;
            maintenanceInfo = value;
        }
        get => maintenanceInfo;
    }

    public string SensorName
    {
        set
        {
            textMeshProSensorName.text = value;
            sensorName = value;
        }
        get => sensorName;
    }

    public string[] StatusCodes => statusCodes;

    // Update is called once per frame
    private async void Update()
    {
        timer += Time.deltaTime;

        if (timer > interval)
        {
            timer = 0;
            if (!string.IsNullOrEmpty(uuid))
                await FunctionsAppsForADT.Instance.GetSensorByIdAsync(uuid, SetParameters);
        }
    }

    private void SetParameters(SensorInfo obj)
    {
        Temperature = obj.Temperature;
        Pressure = obj.Pressure;
        Humidity = obj.Humidity;
        Light = obj.Light;
        Co2 = obj.Co2;
        TVOC = obj.TVOC;
        SensorName = obj.SensorName;
        Status = obj.IsPlacement ? "Placement" : "Not Placement";
        statusCodes = JsonUtility.FromJson<StatusCodes>("{\"Codes\":" + obj.StatusCode + "}").Codes;
        OnSensorInfoUpdate?.Invoke(this);
        MaintenanceStatus = obj.Status;
        MaintenanceInfo = obj.MaintenanceInfo;
    }

    public void SetUuidToSpatialAnchorManager()
    {
        Debug.Log($"Call SetUuidToSpatialAnchorManager.params->{true},{uuid}");
        AnchorModuleProxy.Instance.SetCoarseRelocationBluetooth(true, new[] {uuid});
        Debug.Log("Call nextStep");
        nextStep.SetActive(true);
        Debug.Log("Call currentStep");
        currentStep.SetActive(false);
    }
}