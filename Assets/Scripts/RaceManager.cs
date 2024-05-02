using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    [SerializeField] private Text _stopWatchText;
    [SerializeField] private GameObject _raceOverPanel, _timePenaltyPanel;
    private Text _timePenaltyText;
    private float _timer;
    public bool RaceStarted { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartRaceOnInput());
        _raceOverPanel.SetActive(false);
        _timePenaltyPanel.SetActive(false);
        _timePenaltyText = _timePenaltyPanel.GetComponentInChildren<Text>();
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

    private IEnumerator DeactivateTimePenaltyPanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        _timePenaltyPanel.SetActive(false);
    }

    public void TimePenalty(float bySeconds)
    {
        _timer += bySeconds;
        _timePenaltyText.text = $"+ {bySeconds} Secs";
        _timePenaltyPanel.SetActive(true);
        StartCoroutine(DeactivateTimePenaltyPanel(3));
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



