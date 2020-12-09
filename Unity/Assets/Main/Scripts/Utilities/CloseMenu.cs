// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

public class CloseMenu : MonoBehaviour
{
    private SolverHandler[] handlers;

    [SerializeField]
    public bool isClose;

    private Quaternion startRotation;

    // Start is called before the first frame update
    private void Start()
    {
        handlers = GetComponentsInChildren<SolverHandler>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isClose)
        {
            foreach (var solverHandler in handlers)
            {
                solverHandler.UpdateSolvers = false;
            }

            var goal = Quaternion.AngleAxis(90f, Camera.main.transform.right) * startRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, goal, 0.1f);
            if ((transform.rotation * Vector3.one - goal * Vector3.one).magnitude < 0.05)
            {
                gameObject.SetActive(false);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                foreach (var solverHandler in handlers)
                {
                    solverHandler.UpdateSolvers = true;
                }

                isClose = false;
            }
        }
        else
        {
            startRotation = transform.rotation;
        }
    }
}