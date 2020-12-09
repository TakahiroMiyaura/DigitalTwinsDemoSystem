// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using Assets.Main.Scripts;
using Com.Reseul.SpatialAnchors;
using UnityEngine;
#if !UNITY_EDITOR
using Microsoft.Azure.SpatialAnchors.Unity;
#endif

public class AnchorMaintenance : MonoBehaviour
{
    public async void AnchorDelete()
    {
        await AnchorModuleProxy.Instance.StartAzureSessionAsync();
#if !UNITY_EDITOR
        var cloudSpatialAnchor = GetComponent<CloudNativeAnchor>();
        var id = cloudSpatialAnchor.CloudAnchor.Identifier;
        await AnchorModuleProxy.Instance.DeleteAzureAnchorAsync(id);
#endif
        var sensorParam = new Dictionary<SensorParams, object>();
        sensorParam[SensorParams.IsPlacement] = false;
        await FunctionsAppsForADT.Instance.UpdateSensorStatusAsync(
            AnchorModuleProxy.Instance.KnownBeaconProximityUuids[0],
            sensorParam);
        DestroyImmediate(gameObject);
    }
}