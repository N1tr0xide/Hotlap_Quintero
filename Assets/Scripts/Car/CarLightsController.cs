using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLightsController : MonoBehaviour
{
    [SerializeField] private GameObject _headLightLeft,
        _headLightRight,
        _brakeLightLeft,
        _brakeLightRight,
        _reverseLightLeft,
        _reverseLightRight;
    
    public bool HeadLightsEnabled { get; private set; }
    
    private Light _brakeLightL, _brakeLightR, _brakeSpotLightL, _brakeSpotLightR;
    private float _brakeLightIntensity;
    private float _brakeSpotLightIntensity;
    
    // Start is called before the first frame update
    void Start()
    {
        _brakeLightL = _brakeLightLeft.GetComponent<Light>();
        _brakeSpotLightL = _brakeLightLeft.transform.GetChild(0).GetComponent<Light>();
        
        _brakeLightR = _brakeLightRight.GetComponent<Light>();
        _brakeSpotLightR = _brakeLightRight.transform.GetChild(0).GetComponent<Light>();
        
        _brakeLightIntensity = _brakeLightL.intensity;
        _brakeSpotLightIntensity = _brakeSpotLightL.intensity;

        SetHeadLightsActive(true);
        SetBrakeLightsActive(false);
        SetReverseLightsActive(false);
    }
    
    public void SetHeadLightsActive(bool state)
    {
        _headLightLeft.SetActive(state);
        _headLightRight.SetActive(state);
        _brakeLightLeft.SetActive(state);
        _brakeLightRight.SetActive(state);
        HeadLightsEnabled = state;
        
        if (!state)
        {
            _brakeLightL.intensity = _brakeLightIntensity;
            _brakeLightR.intensity = _brakeLightIntensity;
            _brakeSpotLightL.intensity = _brakeSpotLightIntensity;
            _brakeSpotLightR.intensity = _brakeSpotLightIntensity;
        }
        else
        {
            _brakeLightL.intensity = _brakeLightIntensity / 2;
            _brakeLightR.intensity = _brakeLightIntensity / 2;
            _brakeSpotLightL.intensity = _brakeSpotLightIntensity / 2;
            _brakeSpotLightR.intensity = _brakeSpotLightIntensity / 2;
        }
    }

    public void SetBrakeLightsActive(bool state)
    {
        if (HeadLightsEnabled)
        {
            if (state)
            {
                _brakeLightL.intensity = _brakeLightIntensity;
                _brakeLightR.intensity = _brakeLightIntensity;
                _brakeSpotLightL.intensity = _brakeSpotLightIntensity;
                _brakeSpotLightR.intensity = _brakeSpotLightIntensity;
            }
            else
            {
                _brakeLightL.intensity = _brakeLightIntensity / 2;
                _brakeLightR.intensity = _brakeLightIntensity / 2;
                _brakeSpotLightL.intensity = _brakeSpotLightIntensity / 2;
                _brakeSpotLightR.intensity = _brakeSpotLightIntensity / 2;
            }
            
            return;
        }
        
        _brakeLightLeft.SetActive(state);
        _brakeLightRight.SetActive(state);
    }
    
    public void SetReverseLightsActive(bool state)
    {
        _reverseLightLeft.SetActive(state);
        _reverseLightRight.SetActive(state);
    }
}
