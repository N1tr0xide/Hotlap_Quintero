using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : WheelController
{
    [SerializeField] private CarConfiguration _carConfig;
    [SerializeField] private Wheel[] _wheels = new Wheel[4];
    private Wheel[] _wheelsThatSteer;
    private Wheel[] _poweredWheels;
    private Wheel[] _handbrakeWheels;
    private Rigidbody _rb;
    private RaceManager _raceManager;
    private PlayerInputActions _playerInputActions;
    
    private float _currentTorque, _standardDrag;
    private float _steeringInput;
    private bool _handbrakeInput;
    private bool _reverseActive;

    public float Kph { get; private set; }
    public float CurrentRpm { get; private set; }
    public int CurrentGear { get; private set; }
    public float ThrottleInput => _playerInputActions.Driving.Throttle.ReadValue<float>();
    public float BrakeInput => _playerInputActions.Driving.Brake.ReadValue<float>();
    public float RpmRedLine => _carConfig.RpmRedLine;

    // Start is called before the first frame update
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _wheelsThatSteer = GetFilteredWheels(_wheels, WheelFilters.Steer);
        _poweredWheels = SetDrive(_wheels, _carConfig.Drive);
        _handbrakeWheels = GetFilteredWheels(_wheels, WheelFilters.IsRearWheel);
        _raceManager = FindFirstObjectByType<RaceManager>();
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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateEnginePower(_carConfig);
        ApplySteering(_wheelsThatSteer, _playerInputActions.Driving.Steering.ReadValue<float>());
        ApplyBraking(_wheels, _playerInputActions.Driving.Brake.ReadValue<float>() * _carConfig.BrakeForce);
        if(_playerInputActions.Driving.Handbrake.ReadValue<float>() != 0) ApplyBraking(_handbrakeWheels, _carConfig.HandBrakeForce);

        if (_reverseActive)
        {
            ApplyAcceleration(_poweredWheels, _currentTorque * -_playerInputActions.Driving.Throttle.ReadValue<float>());
        }
        else
        {
            ApplyAcceleration(_poweredWheels, _currentTorque * _playerInputActions.Driving.Throttle.ReadValue<float>());
        }
        
        VisualWheelUpdate(_wheels);
        ApplyDownforce(_carConfig.Downforce);
    }
    
    #region Input Actions

    private void GearUp(InputAction.CallbackContext context)
    {
        if (CurrentGear >= _carConfig.GearRatios.Length - 1) return;
        
        if (_reverseActive)
        {
            _reverseActive = false;
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
        _reverseActive = true;
        _rb.drag = 1;
    }

    #endregion

    private void UpdateEnginePower(CarConfiguration cc) 
    {
        float wheelsRpm = GetWheelsTotalRpm(_wheels) * cc.GearRatios[CurrentGear] * cc.DifferentialRatio;
        CurrentRpm = Mathf.Lerp(CurrentRpm, Mathf.Max(1000 - 100, wheelsRpm), Time.deltaTime * 2.5f);
        _currentTorque = cc.HpToRpmCurve.Evaluate(CurrentRpm / cc.RpmRedLine) * (cc.HorsePower / CurrentRpm) * cc.GearRatios[CurrentGear] *
                         cc.DifferentialRatio * 5252f ;
    }

    private void ApplyDownforce(float downforceValue)
    {
        _rb.AddForce(-transform.up * (downforceValue * _rb.velocity.magnitude));
    }

    private Wheel[] SetDrive(Wheel[] wheels, DriveType driveType)
    {
        Wheel[] w = driveType switch
        {
            DriveType.Fwd => GetFilteredWheels(wheels, WheelFilters.IsFrontWheel),
            DriveType.Rwd => GetFilteredWheels(wheels, WheelFilters.IsRearWheel),
            _ => wheels
        };

        return w;
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

    #region In-Editor Methods

    [ExposeMethodInEditor]
    private void DetermineRearTrackWidth()
    {
        Wheel[] rearWheels = GetFilteredWheels(_wheels, WheelFilters.IsRearWheel);
        RearTrackWidth = CalculateDistanceBetweenWheels(rearWheels[0], rearWheels[1]);
    }

    [ExposeMethodInEditor]
    private void DetermineWheelBase()
    {
        Wheel[] leftWheels = GetFilteredWheels(_wheels, WheelFilters.IsLeftWheel);
        WheelBase = CalculateDistanceBetweenWheels(leftWheels[0], leftWheels[1]);
    }

    #endregion
}