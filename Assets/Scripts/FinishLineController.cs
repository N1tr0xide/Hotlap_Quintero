using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineController : MonoBehaviour
{
    private RaceManager raceManager;

    // Start is called before the first frame update
    void Start()
    {
        raceManager = FindFirstObjectByType<RaceManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            raceManager.RaceOver();
        }
    }
}
