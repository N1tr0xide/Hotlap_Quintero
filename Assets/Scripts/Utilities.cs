using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static string FloatToStopWatchTime(float time)
    {
        TimeSpan timer = TimeSpan.FromSeconds(time);
        return $"{timer.Minutes} : {timer.Seconds} : {timer.Milliseconds}";
    }
}
