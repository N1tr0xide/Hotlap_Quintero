using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    [SerializeField] private Text stopWatchText;
    private float timer;
    private bool raceStarted;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartRaceOnInput());
        string currentScene = SceneManager.GetActiveScene().name;
        float recordTime = LapRecordsUtilities.GetLapRecordTime(currentScene);
        Debug.Log(LapRecordsUtilities.FloatToStopWatchTime(recordTime));
    }

    // Update is called once per frame
    void Update()
    {
        if (raceStarted) timer += Time.deltaTime;
        stopWatchText.text = LapRecordsUtilities.FloatToStopWatchTime(timer);
    }

    private IEnumerator StartRaceOnInput()
    {
        yield return new WaitUntil(() => Input.GetAxis("Vertical") != 0);
        raceStarted = true;
    }

    public void RaceOver()
    {
        raceStarted = false;
        string currentScene = SceneManager.GetActiveScene().name;
        LapRecordsUtilities.SaveLapRecord(timer, currentScene);
    }
}



