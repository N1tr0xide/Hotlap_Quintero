using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarEffects : MonoBehaviour
{
    private CarController _carController;
    private PlayerInputController _inputController;
    
    // Start is called before the first frame update
    void Start()
    {
        _carController = GetComponent<CarController>();
        _inputController = GetComponent<PlayerInputController>();
    }

    // Update is called once per frame
    void Update()
    {
        ApplyTireSquealSound(_carController.Kph);
        ApplySkidMark();
    }
    
    private void ApplyTireSquealSound(float carKph) 
    {
        foreach (var wheel in _carController.Wheels)
        {
            wheel.Collider.GetGroundHit(out WheelHit hit);
            float forwardSlipValue = Mathf.Abs(hit.forwardSlip);
            float sidewaysSlipValue = Mathf.Abs(hit.sidewaysSlip);

            if(hit.collider && carKph > 10 && (forwardSlipValue >= .7f || sidewaysSlipValue >= .4f))
            {
                wheel.AudioSource.volume = Mathf.MoveTowards(wheel.AudioSource.volume, 1, Time.deltaTime * 2);
                return;
            }

            wheel.AudioSource.volume = Mathf.MoveTowards(wheel.AudioSource.volume, 0, Time.deltaTime * 3);
        }
    }

    private void ApplySkidMark()
    {
        foreach (var wheel in _carController.Wheels)
        {
            wheel.Collider.GetGroundHit(out WheelHit hit);
            float forwardSlipValue = Mathf.Abs(hit.forwardSlip);
            float sidewaysSlipValue = Mathf.Abs(hit.sidewaysSlip);

            if(hit.collider && (forwardSlipValue >= .8f || sidewaysSlipValue >= .5f))
            {
                wheel.SkidMarkTrail.emitting = true;
                return;
            }

            if (!hit.collider || _inputController.BrakeInput == 0)
            {
                wheel.SkidMarkTrail.emitting = false;
            }
        }
    }
}
