// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;

[Serializable]
public class SensorInfo
{
    public string id;
    public string _rid;
    public string _self;
    public int _ts;
    public string _etag;
    public string type;
    public string SensorName;
    public float Temperature;
    public float Pressure;
    public float Humidity;
    public float Light;
    public float Co2;
    public float TVOC;
    public string StatusCode;
    public bool IsAlert;
    public string MaintenanceInfo;
    public string Status;
    public bool IsPlacement;
}