using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineController : MonoBehaviour
{
    [SerializeField] private RaceManager _raceManager;

    // Start is called before the first frame update
    void Start()
    {
        _raceManager = FindFirstObjectByType<RaceManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _raceManager.RaceOver();
        }
    }
}
