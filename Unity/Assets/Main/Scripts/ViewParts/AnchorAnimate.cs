// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;

public class AnchorAnimate : MonoBehaviour
{
    [SerializeField]
    private float angle = 1f;

    private void Update()
    {
        transform.rotation = transform.rotation * Quaternion.AngleAxis(angle, Vector3.up);
    }
}