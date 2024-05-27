using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerDEBUG : WheelController
{
    [SerializeField] private CarConfiguration _carConfig;
    private PlayerInputActions _playerInputActions;
    private Rigidbody _rb;
    
    private float _currentTorque, _standardDrag;
    private float _steeringInput;
    private bool _handbrakeInput;

    public float Kph;
    public float CurrentRpm;
    public int CurrentGear;
    public bool ReverseActive;
    public Wheel[] SteerWheels => WheelsThatSteer;
    
    // Start is called before the first frame update
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        WheelsThatSteer = GetFilteredWheels(WheelFilters.Steer);
        PoweredWheels = SetDrive(_carConfig.Drive);
        HandbrakeWheels = GetFilteredWheels(WheelFilters.IsRearWheel);
        _standardDrag = _rb.drag;

        //player input actions
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Driving.Enable();
        _playerInputActions.Driving.GearUp.performed += GearUp;
        _playerInputActions.Driving.GearDown.performed += GearDown;
        _playerInputActions.Driving.ResetVehicle.performed += _ => transform.position += new Vector3(0, 1, 0); 
    }

    void Update()
    {
        Kph = _rb.velocity.magnitude * 3.6f;
        ApplyTireSquealSound(Kph);
    }
    
    void FixedUpdate()
    {
        UpdateEnginePower(_carConfig);
        ApplySteering(WheelsThatSteer, _playerInputActions.Driving.Steering.ReadValue<float>());
        ApplyBraking(_playerInputActions.Driving.Brake.ReadValue<float>() * _carConfig.BrakeForce, _carConfig.Abs, Kph);
        ApplyHandbrake(_carConfig.HandBrakeForce * _playerInputActions.Driving.Handbrake.ReadValue<float>());

        if (ReverseActive)
        {
            ApplyAcceleration(PoweredWheels, _currentTorque * -_playerInputActions.Driving.Throttle.ReadValue<float>());
        }
        else
        {
            ApplyAcceleration(PoweredWheels, _currentTorque * _playerInputActions.Driving.Throttle.ReadValue<float>());
        }
        
        VisualWheelUpdate(Wheels);
        ApplyDownforce(_carConfig.Downforce);
    }

    #region Input Actions

    private void GearUp(InputAction.CallbackContext context)
    {
        if (CurrentGear >= _carConfig.GearRatios.Length - 1) return;
        
        if (ReverseActive)
        {
            ReverseActive = false;
            _rb.drag = _standardDrag;
        }
        
        CurrentGear++;
        CurrentRpm -= CurrentRpm / (CurrentGear + 1);
    }

    private void GearDown(InputAction.CallbackContext context)
    {
        if (CurrentGear > 0)
        {
            CurrentGear--;
            return;
        }

        if (!(Kph < 5f)) return;
        ReverseActive = true;
        _rb.drag = 1;
    }

    #endregion

    private void UpdateEnginePower(CarConfiguration cc)
    {
        float wheelsRpm = GetWheelsTotalRpm() * cc.GearRatios[CurrentGear] * cc.DifferentialRatio;
        CurrentRpm = Mathf.Lerp(CurrentRpm, Mathf.Max(1000 - 100, wheelsRpm), Time.deltaTime * 2.5f);
        _currentTorque = cc.HpToRpmCurve.Evaluate(CurrentRpm / cc.RpmRedLine) * (cc.HorsePower / CurrentRpm) * cc.GearRatios[CurrentGear] *
                         cc.DifferentialRatio * 5252f ;
    }

    private void ApplyDownforce(float downforceValue)
    {
        _rb.AddForce(-transform.up * (downforceValue * _rb.velocity.magnitude));
    }

    private Wheel[] SetDrive(DriveType driveType)
    {
        Wheel[] w = driveType switch
        {
            DriveType.Fwd => GetFilteredWheels(WheelFilters.IsFrontWheel),
            DriveType.Rwd => GetFilteredWheels(WheelFilters.IsRearWheel),
            _ => Wheels
        };

        return w;
    }

    #region In-Editor Methods

    [ExposeMethodInEditor]
    private void DetermineRearTrackWidth()
    {
        Wheel[] rearWheels = GetFilteredWheels(WheelFilters.IsRearWheel);
        RearTrackWidth = CalculateDistanceBetweenWheels(rearWheels[0], rearWheels[1]);
    }

    [ExposeMethodInEditor]
    private void DetermineWheelBase()
    {
        Wheel[] leftWheels = GetFilteredWheels(WheelFilters.IsLeftWheel);
        WheelBase = CalculateDistanceBetweenWheels(leftWheels[0], leftWheels[1]);
    }

    #endregion
}