using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudioController : MonoBehaviour
{
    [SerializeField] private float _pitchMultiplier = 1,
        _lowPitchMin = 1,
        _lowPitchMax = 6,
        _highPitchMultiplier = 0.25f,
        _dopplerLevel = 1;
    [SerializeField] private bool _useDoppler = true;
    [SerializeField] private AudioSource _lowAccel, _lowDecel, _highAccel, _highDecel;
    [SerializeField] private CarController _carController;

    // Update is called once per frame
    void Update()
    {
        //interpolate pitch from revs
        float pitch = Mathf.Lerp(_lowPitchMin, _lowPitchMax,
            _carController.CurrentRpm / _carController.RpmRedLine);
        //clamp to min pitch
        pitch = Mathf.Min(_lowPitchMax, pitch);

        //adjust pitch based on multipliers
        _lowAccel.pitch = pitch * _pitchMultiplier;
        _lowDecel.pitch = pitch * _pitchMultiplier;
        _highAccel.pitch = pitch * _highPitchMultiplier * _pitchMultiplier;
        _highDecel.pitch = pitch * _highPitchMultiplier * _pitchMultiplier;

        //get the low fade from acceleration
        float accFade = Mathf.Abs((Input.GetAxis("Vertical") > 0) ? Input.GetAxis("Vertical") : 0);
        float decFade = 1 - accFade;

        //get the high fade from revs
        float highFade = Mathf.InverseLerp(0.2f, 0.8f, _carController.CurrentRpm / _carController.RpmRedLine);
        float lowFade = 1 - highFade;

        //adjust values for realism
        highFade = 1 - ((1 - highFade) * (1 - highFade));
        lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
        accFade = 1 - ((1 - accFade) * (1 - accFade));
        decFade = 1 - ((1 - decFade) * (1 - decFade));

        //adjust volume
        _lowAccel.volume = lowFade * accFade;
        _lowDecel.volume = lowFade * decFade;
        _highAccel.volume = highFade * accFade;
        _highDecel.volume = highFade * decFade;

        //Adjust doppler level
        _lowAccel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
        _lowDecel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
        _highAccel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
        _highDecel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
    }
}
