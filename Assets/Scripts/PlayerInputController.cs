using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class PlayerInputController : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;
    private CarController _carController;
    private CarLightsController _lightsController;

    internal float ThrottleInput => _playerInputActions.Driving.Throttle.ReadValue<float>();
    internal float BrakeInput => _playerInputActions.Driving.Brake.ReadValue<float>();
    private float HandbrakeInput => _playerInputActions.Driving.Handbrake.ReadValue<float>();
    private float SteeringInput => _playerInputActions.Driving.Steering.ReadValue<float>();
    
    // Start is called before the first frame update
    void Awake()
    {
        _carController = GetComponent<CarController>();
        _lightsController = GetComponentInChildren<CarLightsController>();
        
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Driving.Enable();
        _playerInputActions.Driving.GearUp.performed += _carController.GearUp;
        _playerInputActions.Driving.GearDown.performed += _carController.GearDown;
        _playerInputActions.Driving.ResetVehicle.performed += _ => transform.position += new Vector3(0, 1, 0); 
        _playerInputActions.Driving.Brake.performed += _ => _lightsController.SetBrakeLightsActive(true);
        _playerInputActions.Driving.Brake.canceled += _ => _lightsController.SetBrakeLightsActive(false);
    }

    private void OnDisable()
    {
        _playerInputActions.Driving.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H)) _lightsController.SetHeadLightsActive(!_lightsController.HeadLightsEnabled);
    }

    private void FixedUpdate()
    {
        _carController.ApplySteering(SteeringInput);
        _carController.ApplyBraking(BrakeInput);
        _carController.ApplyHandbrake(HandbrakeInput);

        if (_carController.ReverseActive)
        {
            _carController.ApplyAcceleration(-ThrottleInput);
        }
        else
        {
            _carController.ApplyAcceleration(ThrottleInput);
        }
    }
}
