// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Reseul.SpatialAnchors;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class SearchAnchorsModule : MonoBehaviour, IASACallBackManager
{
    [SerializeField]
    private TextMeshPro messageText = null;

    [SerializeField]
    private GameObject pointerGameObject = null;

    [SerializeField]
    private ProgressIndicatorOrbsRotator progresOrbs = null;

    [SerializeField]
    private TextMeshPro SensorStatusText = null;

    [SerializeField]
    private GameObject StatusObj = null;

    [SerializeField]
    private GameObject Step1 = null;

    [SerializeField]
    private GameObject Step2 = null;

    [SerializeField]
    private GameObject Step3 = null;

    private float visualTimer = 0;

    private float processTimer = 0;

    public bool OnLocatedAnchorObject(string identifier, IDictionary<string, string> appProperties,
        out GameObject gameObject)
    {
        gameObject = Instantiate(pointerGameObject);
        gameObject.GetComponentInChildren<SensorDetailView>(true).Uuid =
            AnchorModuleProxy.Instance.KnownBeaconProximityUuids[0];
        return true;
    }

    public void OnLocatedAnchorComplete()
    {
    }

    public async void ResetOperation()
    {
        AnchorModuleProxy.Instance.DeleteNativeAnchor(pointerGameObject);
        await AnchorModuleProxy.Instance.StopAzureSessionAsync();
        Step1.SetActive(true);
        Step2.SetActive(false);
        Step3.SetActive(false);
        StatusObj.SetActive(true);
        pointerGameObject.transform.localPosition = Vector3.zero;
    }

    private void Start()
    {
        AnchorModuleProxy.Instance.SetASACallBackManager(this);
    }

    public async void StartSession()
    {
        try
        {
            await progresOrbs.OpenAsync();
            var result = AnchorModuleProxy.Instance.SetSpatialAnchorModeAsync(ASAMode.CoarseRelocation);

            while (result.Status == TaskStatus.Running)
            {
                await Task.Yield();
            }
        }
        finally
        {
            progresOrbs.StopOrbs();
            await progresOrbs.CloseAsync();
        }

        Step1.SetActive(false);
        Step2.SetActive(true);
    }

    public async void SearchAnchor()
    {
        try
        {
            await progresOrbs.OpenAsync();
            AnchorModuleProxy.Instance.FindNearBySensors();
            while (processTimer < 3)
            {
                processTimer += Time.deltaTime;
                await Task.Yield();
            }
        }
        finally
        {
            progresOrbs.StopOrbs();
            await progresOrbs.CloseAsync();
        }

        Step2.SetActive(false);
        StatusObj.SetActive(false);
        Step3.SetActive(false);
        progresOrbs.gameObject.SetActive(false);
    }

    private void Update()
    {
        visualTimer += Time.deltaTime;
        if (visualTimer > 1)
        {
            if (Step1.activeSelf)
            {
                messageText.text = "Push session start.";
                SensorStatusText.text = "Ready.";
            }
            else if (Step2.activeSelf)
            {
                messageText.text = "Push placement.";
                SensorStatusText.text = AnchorModuleProxy.Instance.BluetoothStatus.ToString();
            }
            else
            {
                messageText.text = "";
                SensorStatusText.text = "";
            }
        }
    }
}