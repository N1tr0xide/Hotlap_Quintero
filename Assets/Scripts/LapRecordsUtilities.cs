using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LapRecordsUtilities
{
    public static string FloatToStopWatchTime(float time)
    {
        TimeSpan timer = TimeSpan.FromSeconds(time);
        return $"{timer.Minutes} : {timer.Seconds} : {timer.Milliseconds}";
    }

    public static void SaveLapRecord(float time, string levelName)
    {
        if(PlayerPrefs.HasKey(levelName + "_lapRecord"))
        {
            if (PlayerPrefs.GetFloat(levelName + "_lapRecord") < time) return;
        }

        PlayerPrefs.SetFloat(levelName + "_lapRecord", time);
    }

    public static float GetLapRecordTime(string levelName) 
    {  
        if(PlayerPrefs.HasKey(levelName + "_lapRecord"))
        {
            return PlayerPrefs.GetFloat(levelName + "_lapRecord");
        }

        Debug.LogWarning("Lap record key not found!!");
        return 3599.999f;
    }

    public static void DeleteLapRecord(string levelName)
    {
        if (PlayerPrefs.HasKey(levelName + "_lapRecord"))
        {
            PlayerPrefs.DeleteKey(levelName + "_lapRecord");
        }
    }
}
