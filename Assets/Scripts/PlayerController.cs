using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : WheelController
{
    [SerializeField] private Wheel[] _wheels = new Wheel[4];
    private Wheel[] _wheelsThatSteer;
    private Wheel[] _poweredWheels;
    private Wheel[] _handbrakeWheels;
    private Rigidbody _rb;

    private enum DriveType { Fwd, Rwd, Awd }
    [SerializeField] private DriveType _drive;
    
    [SerializeField] private float _horsePower, _rpmRedLine, _currentTorque, _currentRpm, _diffRatio;
    [SerializeField] private AnimationCurve _hpToRpmCurve;
    [SerializeField] private float[] _gearRatios;
    [SerializeField] private int _currentGear;
    [SerializeField] private float _brakeForce = 600f, _handBrakeForce = 600f;
    [SerializeField] private int _downforce;
    
    private float _currentBrakeForce;
    private float _throttleInput;
    private float _steeringInput;
    private bool _handbrakeInput;

    public float Kph { get; private set; }
    public float CurrentRpm => _currentRpm;
    public float RpmRedLine => _rpmRedLine;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _wheelsThatSteer = GetFilteredWheels(_wheels, WheelFilters.Steer);
        _poweredWheels = SetDrive(_wheels, _drive);
        _handbrakeWheels = GetFilteredWheels(_wheels, WheelFilters.IsRearWheel);
    }

    void Update()
    {
        PlayerInputs();
        Debug.Log(Kph);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Kph = _rb.velocity.magnitude * 3.6f;
        ApplyBraking(_wheels, _currentBrakeForce);
        if (_handbrakeInput) ApplyBraking(_handbrakeWheels, _handBrakeForce);

        UpdateEnginePower();
        ApplyAcceleration(_poweredWheels, _currentTorque);
        ApplySteering(_wheelsThatSteer, _steeringInput);
        VisualWheelUpdate(_wheels);
        ApplyDownforce(_rb, _downforce);
    }

    void PlayerInputs()
    {
        _throttleInput = Input.GetAxis("Vertical");
        _handbrakeInput = Input.GetKey(KeyCode.H);
        _steeringInput = Input.GetAxis("Horizontal");
        _currentBrakeForce = _brakeForce * Convert.ToSingle(Input.GetKey(KeyCode.Space));

        if (Input.GetKeyDown(KeyCode.Z) && _currentGear < _gearRatios.Length - 1) _currentGear++;
        if (Input.GetKeyDown(KeyCode.X) && _currentGear > 0) _currentGear--;
        if (Input.GetKeyDown(KeyCode.R)) transform.position += new Vector3(0, 2, -3);
    }

    private void UpdateEnginePower()
    {
        float wheelsRpm = GetWheelsTotalRpm(_wheels) * _gearRatios[_currentGear] * _diffRatio;
        _currentRpm = Mathf.Lerp(_currentRpm, Mathf.Max(1000 - 100, wheelsRpm), Time.deltaTime * 3);
        _currentTorque = _hpToRpmCurve.Evaluate(_currentRpm / _rpmRedLine) * (_horsePower / _currentRpm) * _gearRatios[_currentGear] *
                         _diffRatio * 5252f * _throttleInput;
    }

    void ApplyDownforce(Rigidbody rb, float downforceValue)
    {
        rb.AddForce(-transform.up * (downforceValue * rb.velocity.magnitude));
    }

    Wheel[] SetDrive(Wheel[] wheels, DriveType driveType)
    {
        Wheel[] w = driveType switch
        {
            DriveType.Fwd => GetFilteredWheels(wheels, WheelFilters.IsFrontWheel),
            DriveType.Rwd => GetFilteredWheels(wheels, WheelFilters.IsRearWheel),
            _ => wheels
        };

        return w;
    }

    [ExposeMethodInEditor]
    private void ReconfigureDrive()
    {
        _poweredWheels = SetDrive(_wheels, _drive);
    }

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
}


