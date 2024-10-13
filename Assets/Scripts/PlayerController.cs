using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private RaceManager _raceManager;

    // Start is called before the first frame update
    void Awake()
    {
        _raceManager = FindFirstObjectByType<RaceManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("FinishLine")) return;
        _raceManager.RaceOver();
        GetComponentInChildren<Canvas>().gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            _raceManager.TimePenalty(2);
        }
    }
}