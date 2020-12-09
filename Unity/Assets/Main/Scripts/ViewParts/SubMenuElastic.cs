// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// // Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections;
using Microsoft.MixedReality.Toolkit.Experimental.Physics;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Experimental.Demos
{
    /// <summary>
    ///     Demonstration script to show how ElasticSystem can be used to drive
    ///     a satisfying UI inflation/deflation effect of an example UI widget.
    /// </summary>
    public class SubMenuElastic : MonoBehaviour
    {
        // The elastic extent for our flippy panels.
        private static LinearElasticExtent flipExtent = new LinearElasticExtent
        {
            MinStretch = -180.0f,
            MaxStretch = 0.0f,
            SnapPoints = new float[] { },
            SnapToEnds = false
        };

        // The elastic properties of our springs.
        private static ElasticProperties elasticProperties = new ElasticProperties
        {
            Mass = 0.03f,
            HandK = 4.0f,
            EndK = 3.0f,
            SnapK = 1.0f,
            Drag = 0.2f
        };

        // The elastic system for the backplate (treated separately)
        private LinearElasticSystem backplateElastic;

        // The goal value for the backplate's horizontal scale.

        // The backplate, which will scale horizontally.
        public float DeflateAngle = -180.0f;

        // Internal list of of the elastic systems. Each ElasticSystem holds its
        // own state. We use a LinearElasticSystem because we are only controlling
        // a single value for each element that will be elastic-ified.
        private LinearElasticSystem flipElastic;

        // The corresponding list of "goal values" for each of the flip panels' elastic systems.
        private float flipGoals;

        // A list of the panels that will flip up. These will be
        // inflated in the order that they're listed.
        public Transform FlipPanel;

        // Allow the user to configure the enabled/disabled states of the
        // elements controlled by our elastic systems.
        public float InflateAngle = 0.0f;

        // Is the widget inflated or not?
        private bool isInflated;

        private void Start()
        {    
            flipElastic=new LinearElasticSystem(DeflateAngle, 0.0f, flipExtent, elasticProperties);
            flipGoals = DeflateAngle;
        }

        private void Update()
        {
            FlipPanel.localEulerAngles =
                    new Vector3(FlipPanel.localRotation.x,
                        Mathf.Clamp(flipElastic.ComputeIteration(flipGoals, Time.deltaTime), -360, 0), FlipPanel.localRotation.z);
        }

        public void ToggleInflate()
        {
            if (!isInflated)
            {
                isInflated = true;
                StartCoroutine(InflateCoroutine());
            }
            else
            {
                isInflated = false;
                StartCoroutine(DeflateCoroutine());
            }
        }

        public IEnumerator DeflateCoroutine()
        {
            flipGoals = DeflateAngle;
            yield return new WaitForSeconds(0.1f);
            gameObject.transform.GetChild(0).gameObject.SetActive(false);

        }

        public IEnumerator InflateCoroutine()
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            flipGoals = InflateAngle;
            yield return new WaitForSeconds(0.1f);
            
        }
    }
}