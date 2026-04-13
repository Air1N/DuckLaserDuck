using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Steamworks;
using System.Collections;

public class SaveGameManager : MonoBehaviour
{
    public bool savedRecently = false;

    public static bool WriteToFile(string a_FileName, string a_FileContents)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

        try
        {
            File.WriteAllText(fullPath, a_FileContents);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to {fullPath} with exception {e}");
            return false;
        }
    }

    public static bool LoadFromFile(string a_FileName, out string result)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

        try
        {
            result = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read from {fullPath} with exception {e}");
            result = "";
            return false;
        }
    }

    public SaveData saveData = new();
    public void SaveJsonData()
    {
        if (savedRecently)
        {
            StartCoroutine(RateLimitedSave());
            return;
        }

        if (WriteToFile("SaveData01.dat", saveData.ToJson()))
        {
            savedRecently = true;
            Debug.Log("Save successful");
        }
    }

    private IEnumerator RateLimitedSave()
    {
        yield return new WaitForSecondsRealtime(15f);
        savedRecently = false;
    }

    public void LoadJsonData()
    {
        if (LoadFromFile("SaveData01.dat", out var json))
        {
            saveData.LoadFromJson(json);

            Debug.Log("Load complete");
        }
    }

    private float startTime = 0;
    private void Awake()
    {
        startTime = Time.time;
        try
        {
            LoadJsonData();
        }
        catch (System.Exception)
        {
            return;
        }
    }

    private void OnApplicationQuit()
    {
        SaveJsonData();

        SteamUserStats.StoreStats();
    }
}