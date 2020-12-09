﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace Com.Reseul.SpatialAnchors
{
    public enum SensorStatus
    {
        /// <summary>
        ///     Sensor is not initialized.
        /// </summary>
        NotInitialized,

        /// <summary>
        ///     GeoLocation data is available.
        /// </summary>
        Available,

        /// <summary>
        ///     GeoLocation was disabled in the SensorCapabilities.
        /// </summary>
        DisabledCapability,

        /// <summary>
        ///     No sensor fingerprint provider has been created.
        /// </summary>
        MissingSensorFingerprintProvider,

        /// <summary>
        ///     No GPS data has been received.
        /// </summary>
        NoData
    }
}
