// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ShowDetailInfoButton : MonoBehaviour
{
    public SensorDetailView DetailView;

    public string Uuid { set; get; }

    // Start is called before the first frame update
    private void Start()
    {
        var helper = GetComponent<ButtonConfigHelper>();
        helper.OnClick.AddListener(SetDetailData);
    }

    public void SetDetailData()
    {
        DetailView.gameObject.SetActive(true);
        DetailView.Uuid = Uuid;
    }
}