using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    [SerializeField] private Text _stopWatchText;
    [SerializeField] private GameObject _raceOverPanel;
    private float _timer;
    public bool RaceStarted { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartRaceOnInput());
        _raceOverPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (RaceStarted) _timer += Time.deltaTime;
        _stopWatchText.text = LapRecordsUtilities.FloatToStopWatchTime(_timer);
    }

    private IEnumerator StartRaceOnInput()
    {
        yield return new WaitUntil(() => Input.GetAxis("Vertical") != 0);
        RaceStarted = true;
    }

    public void RaceOver()
    {
        RaceStarted = false;
        _raceOverPanel.SetActive(true);
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (_timer < LapRecordsUtilities.GetLapRecordTime(currentScene))
        {
            LapRecordsUtilities.SaveLapRecord(_timer, currentScene);
            return;
        }
        
        _raceOverPanel.GetComponentInChildren<Text>().text = "Try Again.";
    }

    public void RestartRace()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}



