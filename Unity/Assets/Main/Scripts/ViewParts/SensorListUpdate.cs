// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections;
using System.Collections.Generic;
using Assets.Main.Scripts;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class SensorListUpdate : MonoBehaviour
{
    [SerializeField]
    private GridObjectCollection container = null;

    private SensorInfo[] data = null;

    [SerializeField]
    private SensorDetailView detailView = null;

    [SerializeField]
    private GameObject deviceInfo = null;

    private bool isRecieved = false;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(Request());
    }


    private IEnumerator Request()
    {
#pragma warning disable 4014
        FunctionsAppsForADT.Instance.GetSensorListsAsync(x =>
        {
            data = x;
            isRecieved = true;
        });
#pragma warning restore 4014
        yield return null;
    }


    // Update is called once per frame
    private void Update()
    {
        if (isRecieved && data != null)
        {
            if (container == null) return;

            var view = container.GetComponentsInChildren<SensorOverView>();
            var hit = 0;
            var existsName = new List<string>();
            foreach (var sensorOverView in view)
            {
                foreach (var sensorInfo in data)
                {
                    if (sensorInfo.SensorName.Equals(sensorOverView.SensorName))
                    {
                        existsName.Add(sensorInfo.SensorName);
                        hit++;
                    }
                }
            }

            if (hit < data.Length)
            {
                foreach (var sensorInfo in data)
                {
                    var instantiate = Instantiate(deviceInfo);
                    var sensorView = instantiate.GetComponent<SensorOverView>();
                    sensorView.SensorName = sensorInfo.SensorName;
                    sensorView.Status = sensorInfo.IsPlacement ? "Placement" : "Not Placement";

                    sensorView.Uuid = sensorInfo.id;
                    sensorView.MaintenanceStatus = sensorInfo.Status;

                    var button = instantiate.GetComponentInChildren<ShowDetailInfoButton>();
                    button.Uuid = sensorInfo.id;
                    button.DetailView = detailView;
                    instantiate.transform.parent = container.transform;
                    instantiate.transform.localPosition = Vector3.zero;
                    instantiate.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    if (sensorInfo.IsPlacement)
                    {
                        sensorView.SetStatusColor(Color.white);
                    }
                    else
                    {
                        sensorView.SetStatusColor(new Color(1, 0, 1));
                    }

                    if (sensorView.MaintenanceStatus.Contains("RequestMaintenance")
                        || sensorView.MaintenanceStatus.Contains("Pending"))
                    {
                        sensorView.SetMaintenanceStatusColor(new Color(1, 0, 1));
                    }
                    else
                    {
                        sensorView.SetMaintenanceStatusColor(Color.white);
                    }
                }

                container.UpdateCollection();
                container.GetComponentInParent<ScrollingObjectCollection>().UpdateContent();
            }

            isRecieved = false;
        }
    }
}