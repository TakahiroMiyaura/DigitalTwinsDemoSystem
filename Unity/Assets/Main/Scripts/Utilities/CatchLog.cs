// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Text;
using TMPro;
using UnityEngine;

public class CatchLog : MonoBehaviour
{
    private bool autoScroll = true;
    private StringBuilder builder = new StringBuilder();

    [SerializeField]
    private bool coloredByLogType = true;

    [SerializeField]
    private string[] ignore = {""};

    private TextMeshPro text;

    [SerializeField]
    private bool useTimeStamp = true;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
        if (text == null)
        {
            enabled = false;
            throw new NullReferenceException("No text component found.");
        }

        if (autoScroll)
            text.overflowMode = TextOverflowModes.Truncate;

        if (coloredByLogType)
            text.richText = true;

        text.text = string.Empty;
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        builder = new StringBuilder();
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        builder = null;
    }

    private void HandleLog(string logText, string stackTrace, LogType logType)
    {
        builder.Clear();

        if (0 < ignore.Length)
        {
            for (var i = 0; i < ignore.Length; i++)
            {
                if (ignore[i] != string.Empty && logText.Contains(ignore[i]))
                    return;
            }
        }

        if (useTimeStamp)
            builder.Append(string.Format("[{0}:{1:D3}] ", DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond));

        if (coloredByLogType)
        {
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Warning:
                    logText = GetColoredString(logText, "yellow");
                    break;
                case LogType.Error:
                case LogType.Exception:
                    logText = GetColoredString(logText, "red");
                    break;
            }
        }

        builder.Append(logText);
        builder.Append(Environment.NewLine);

        text.text += builder.ToString();

        if (text.text.Split('\n').Length > 29)
        {
            text.text = text.text.Remove(0, text.text.IndexOf('\n') + 1);
        }
    }

    private string GetColoredString(string src, string color)
    {
        return string.Format("<color={0}>{1}</color>", color, src);
    }
}