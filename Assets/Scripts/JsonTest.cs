using System;
using UnityEngine;

public class JsonTest : MonoBehaviour
{
    [Serializable]
    public class VsWhereResult
    {
        public VsWhereEntry[] entries;
    }

    [Serializable]
    public class VsWhereEntry
    {
        public string displayName;
        public string productPath;
    }

    void Start()
    {
        string json = @"[ { ""displayName"": ""Test"", ""productPath"": ""C:\Users\fabio"" } ]";
        try
        {
            var result = JsonUtility.FromJson<VsWhereResult>("{ \"entries\": " + json + " }");
            Debug.Log("Parsed successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse: " + e.Message);
        }
    }
}
