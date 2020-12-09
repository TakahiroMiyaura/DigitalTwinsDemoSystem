// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using TMPro;
using UnityEngine;

public class StatusView : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro idText = null;

    [SerializeField]
    private TextMeshPro messageText = null;

    public string Id
    {
        set
        {
            idText.text = value;
        }
        get
        {
            return idText.text;
        }
    }

    public string Message
    {
        set
        {
            messageText.text = value;
        }
        get
        {
            return messageText.text;
        }
    }

    public string Uuid { set; get; }

}