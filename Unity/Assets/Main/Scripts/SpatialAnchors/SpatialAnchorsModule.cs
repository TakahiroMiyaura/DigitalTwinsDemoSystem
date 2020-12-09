// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Main.Scripts;
using Com.Reseul.SpatialAnchors;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class SpatialAnchorsModule : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro messageText = null;

    [SerializeField]
    private GameObject pointerGameObject = null;

    [SerializeField]
    private ProgressIndicatorLoadingBar progresBar = null;

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

    [SerializeField]
    private GameObject Step4 = null;

    private float timer;
    private bool isAnchorSucceeded;

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

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1)
        {
            timer = 0;
            if (Step1.activeSelf)
            {
                messageText.text = "Push session start.";
                SensorStatusText.text = "Ready.";
            }
            else if (Step2.activeSelf)
            {
                messageText.text = "Placement Anchor and push placement.";
                SensorStatusText.text = AnchorModuleProxy.Instance.BluetoothStatus.ToString();
            }
            else if (Step3.activeSelf)
            {
                messageText.text = "Anchor Placement Succeeded!";
                SensorStatusText.text = AnchorModuleProxy.Instance.BluetoothStatus.ToString();
            }
            else
            {
                messageText.text = "";
                SensorStatusText.text = "";
            }
        }
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
            await progresOrbs.CloseAsync();
        }

        Step1.SetActive(false);
        Step2.SetActive(true);
        pointerGameObject.GetComponent<ObjectManipulator>().enabled = true;
    }

    public async void PlacementAnchor()
    {
        try
        {
            isAnchorSucceeded = false;
            progresBar.Progress = 0.01f;
            await progresBar.OpenAsync();
            var placementResult = AnchorModuleProxy.Instance.CreateAzureAnchorAsync(pointerGameObject, new Dictionary<string, string>());
            while (AnchorModuleProxy.Instance.RecommendedForCreateProgress < 1)
            {
                progresBar.Progress = AnchorModuleProxy.Instance.RecommendedForCreateProgress * 0.9f;

                await Task.Yield();
            }

            isAnchorSucceeded = !string.IsNullOrEmpty(await placementResult);
            var sensorParam = new Dictionary<SensorParams, object>();
            sensorParam[SensorParams.IsPlacement] = true;
            var result = FunctionsAppsForADT.Instance.UpdateSensorStatusAsync(
                AnchorModuleProxy.Instance.KnownBeaconProximityUuids[0],
                sensorParam);
            while (result.Status == TaskStatus.Running)
            {
                await Task.Yield();
            }

            progresBar.Progress = 1f;
            ;
        }
        finally
        {
            await progresBar.CloseAsync();
        }

        if (isAnchorSucceeded)
        {
            Step2.SetActive(false);
            Step3.SetActive(true);
        }
    }

    public void NextStep()
    {

        Step3.SetActive(false);
        StatusObj.SetActive(false);
        Step4.SetActive(true);
    }
}