// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;

public class OnEnableCameraForward : MonoBehaviour
{
    private bool isExecute;

    // Start is called before the first frame update
    private void OnEnable()
    {
        if (!isExecute)
        {
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
            transform.rotation = Camera.main.transform.rotation * transform.rotation;
            isExecute = true;
        }
    }

    private void OnDisable()
    {
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
    }
}