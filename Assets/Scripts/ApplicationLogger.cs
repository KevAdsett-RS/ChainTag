using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ApplicationLogger : MonoBehaviour
{

    private GUID _logId;
        
    private void Awake()
    {
        _logId = GUID.Generate();
        Debug.Log($"_logId: {_logId}");

        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
        File.Delete($"C:/Users/kevad/Documents/Programming/Git repos/ChainTag/Logs/{_logId}_log.txt");
    }

    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        File.AppendAllText($"C:/Users/kevad/Documents/Programming/Git repos/ChainTag/Logs/{_logId}_log.txt", $"{DateTime.UtcNow} - {type}: {logString}\n");
    }
}