// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using Assets.Main.Scripts;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class StatusDetails : MonoBehaviour
{
    private static object lockObj = new object();

    [SerializeField]
    private GridObjectCollection container = null;

    [SerializeField]
    private SensorDetailView detailView = null;


    private Dictionary<string, string> sensorMessages = null;

    [SerializeField]
    private GameObject StatusInfoPrefab = null;

    public async void Start()
    {
        detailView.OnSensorInfoUpdate += SetStatusInfos;
        if (sensorMessages == null)
        {
            sensorMessages = await FunctionsAppsForADT.Instance.GetSensorMessagesAsync();
        }
    }

    public void SetStatusInfos(SensorDetailView data)
    {
        var view = container.GetComponentsInChildren<StatusView>();
        var hit = 0;
        var existsId = new List<string>();
        var DelObj = new List<StatusView>();
        var dataStatusCodes = data.StatusCodes;
        foreach (var statusView in view)
        {
            var exists = false;
            foreach (var id in dataStatusCodes)
            {
                if (id.Equals(statusView.Id))
                {
                    exists = true;
                    existsId.Add(id);
                    hit++;
                    break;
                }
            }

            if (!exists)
            {
                DelObj.Add(statusView);
            }
        }

        foreach (var statusView in DelObj)
        {
            DestroyImmediate(statusView.gameObject);
        }

        if (hit < dataStatusCodes.Length)
        {
            foreach (var statusId in dataStatusCodes)
            {
                if (existsId.Contains(statusId) || dataStatusCodes.Length > 1 && statusId.Equals("000000")) continue;

                var instantiate = Instantiate(StatusInfoPrefab);
                var sensorView = instantiate.GetComponent<StatusView>();
                sensorView.Id = statusId;
                sensorView.Message = sensorMessages[statusId];
                instantiate.transform.parent = container.transform;
                instantiate.transform.localRotation = StatusInfoPrefab.transform.localRotation;
                instantiate.transform.localPosition = StatusInfoPrefab.transform.localPosition;
                instantiate.transform.localScale = StatusInfoPrefab.transform.localScale;
            }

            container.UpdateCollection();
            GetComponentInChildren<ScrollingObjectCollection>(true).UpdateContent();
        }
    }
}