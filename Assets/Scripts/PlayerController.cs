using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : WheelController
{
    [SerializeField] private CarConfiguration _carConfig;
    [SerializeField] private CarLightsController _lightsController;
    private Rigidbody _rb;
    private RaceManager _raceManager;
    private PlayerInputActions _playerInputActions;
    
    private float _currentTorque, _standardDrag;
    private float _power;
    private float _steeringInput;
    private float _brakeInput;
    private bool _handbrakeInput;
    private bool _reverseActive;
    private bool _clutchActive;
    private bool _shiftingUp, _shiftingDown;

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
        _lightsController = GetComponentInChildren<CarLightsController>();
        WheelsThatSteer = GetFilteredWheels(WheelFilters.Steer);
        PoweredWheels = SetDrive(_carConfig.Drive);
        HandbrakeWheels = GetFilteredWheels(WheelFilters.IsRearWheel);
        _raceManager = FindFirstObjectByType<RaceManager>();
        _standardDrag = _rb.drag;
        _power = _carConfig.HorsePower * 3.6f; // adapt horsepower to match real life results
        
        //player input actions
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Driving.Enable();
        _playerInputActions.Driving.GearUp.performed += GearUp;
        _playerInputActions.Driving.GearDown.performed += GearDown;
        _playerInputActions.Driving.ResetVehicle.performed += _ => transform.position += new Vector3(0, 1, 0); 
        _playerInputActions.Driving.Brake.performed += _ => _lightsController.SetBrakeLightsActive(true);
        _playerInputActions.Driving.Brake.canceled += _ => _lightsController.SetBrakeLightsActive(false);
    }

    void Update()
    {
        Kph = _rb.velocity.magnitude * 3.6f;
        _brakeInput = _playerInputActions.Driving.Brake.ReadValue<float>();
        ApplyTireSquealSound(Kph);
        if(Input.GetKeyDown(KeyCode.H)) _lightsController.SetHeadLightsActive(!_lightsController.HeadLightsEnabled);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateEnginePower(_carConfig);
        ApplySteering(WheelsThatSteer, _playerInputActions.Driving.Steering.ReadValue<float>());
        ApplyBraking(_brakeInput * _carConfig.BrakeForce, _carConfig.Abs, _carConfig.AbsThreshold, Kph);
        ApplyHandbrake(_carConfig.HandBrakeForce * _playerInputActions.Driving.Handbrake.ReadValue<float>());

        if (_reverseActive)
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
        
        if (_reverseActive)
        {
            _reverseActive = false;
            _lightsController.SetReverseLightsActive(false);
            return;
        }
        
        CurrentGear++;
        StartCoroutine(ApplyShiftUpClutch());
    }

    private void GearDown(InputAction.CallbackContext context)
    {
        if (CurrentGear > 0)
        {
            CurrentGear--;
            StartCoroutine(ApplyShiftDownClutch());
            StartCoroutine(ApplyDragForSeconds(.5f));
            return;
        }

        if (!(Kph < 5f)) return;
        _reverseActive = true;
        _lightsController.SetReverseLightsActive(true);
    }

    #endregion

    private void UpdateEnginePower(CarConfiguration cc)
    {
        if (!_clutchActive)
        {
            float wheelsRpm = GetWheelsAvgRpm() * cc.GearRatios[CurrentGear] * cc.DifferentialRatio;
            CurrentRpm = Mathf.Lerp(CurrentRpm, Mathf.Max(1000, wheelsRpm), Time.deltaTime * cc.GearRatios[CurrentGear]);
            CurrentRpm = Mathf.Clamp(CurrentRpm, 1000, RpmRedLine);
            _currentTorque = cc.HpToRpmCurve.Evaluate(CurrentRpm / cc.RpmRedLine) * (_power / CurrentRpm) * cc.GearRatios[CurrentGear] *
                             cc.DifferentialRatio * 5252f;
            return;
        }

        if (_shiftingUp)
        {
            CurrentRpm = Mathf.Lerp(CurrentRpm, 1000, Time.deltaTime * cc.GearRatios[CurrentGear]);
            _currentTorque = 0;
        }
        else if(_shiftingDown)
        {
            float wheelsRpm = GetWheelsAvgRpm() * cc.GearRatios[CurrentGear] * cc.DifferentialRatio;
            CurrentRpm = Mathf.Lerp(CurrentRpm, Mathf.Max(1000, wheelsRpm), Time.deltaTime * cc.GearRatios[CurrentGear]);
            CurrentRpm = Mathf.Clamp(CurrentRpm, 1000, RpmRedLine);
            _currentTorque = cc.HpToRpmCurve.Evaluate(CurrentRpm / cc.RpmRedLine) * (_power / CurrentRpm) * cc.GearRatios[CurrentGear] *
                             cc.DifferentialRatio * 5252f;
        }
    }

    private IEnumerator ApplyShiftUpClutch()
    {
        _clutchActive = true;
        _shiftingUp = true;
        yield return new WaitForSeconds(.5f);
        _clutchActive = false;
        _shiftingUp = false;
    }
    
    private IEnumerator ApplyShiftDownClutch()
    {
        _clutchActive = true;
        _shiftingDown = true;
        yield return new WaitForSeconds(.5f);
        _clutchActive = false;
        _shiftingDown = false;
    }

    private IEnumerator ApplyDragForSeconds(float seconds)
    {
        _rb.drag = 0.75f;
        yield return new WaitWhile(()=> GetWheelsAvgRpm() > _currentTorque);
        _rb.drag = _standardDrag;
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