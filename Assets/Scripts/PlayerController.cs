using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : WheelController
{
    [SerializeField] private Wheel[] _wheels = new Wheel[4];
    private Wheel[] _wheelsThatSteer;
    private Wheel[] _poweredWheels;
    private Wheel[] _handbrakeWheels;
    private Rigidbody _rb;
    private RaceManager _raceManager;

    private enum DriveType { Fwd, Rwd, Awd }
    [SerializeField] private DriveType _drive;
    
    private float _currentTorque, _startingDrag;
    [SerializeField] private float _horsePower, _rpmRedLine, _diffRatio;
    [SerializeField] private AnimationCurve _hpToRpmCurve;
    [SerializeField] private float[] _gearRatios;
    [SerializeField] private float _brakeForce = 600f, _handBrakeForce = 600f;
    [SerializeField] private int _downforce;

    private float _steeringInput;
    private bool _handbrakeInput;
    private bool _reverseInput;

    public float Kph { get; private set; }
    public float CurrentRpm { get; private set; }
    public int CurrentGear { get; private set; }
    public float ThrottleInput { get; private set; }
    public float BrakeInput { get; private set; }
    public float RpmRedLine => _rpmRedLine;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _wheelsThatSteer = GetFilteredWheels(_wheels, WheelFilters.Steer);
        _poweredWheels = SetDrive(_wheels, _drive);
        _handbrakeWheels = GetFilteredWheels(_wheels, WheelFilters.IsRearWheel);
        _raceManager = FindFirstObjectByType<RaceManager>();
        _startingDrag = _rb.drag;
    }

    void Update()
    {
        if (!_raceManager.RaceStarted)
        {
            ThrottleInput = 0;
            _handbrakeInput = true;
            return;
        }

        #region Player Inputs

            _reverseInput = Input.GetKey(KeyCode.C);
            _handbrakeInput = Input.GetKey(KeyCode.H);
            _steeringInput = Input.GetAxis("Horizontal");
            ThrottleInput = _reverseInput && _rb.velocity.z <= .1f ? -.3f : Input.GetAxis("Vertical") <= 0 ? 0 : Input.GetAxis("Vertical");
            BrakeInput = Input.GetKey(KeyCode.LeftShift) ? 0 : Input.GetAxis("Vertical") < 0 ? -Input.GetAxis("Vertical") : 0;
            
            if (Input.GetKeyDown(KeyCode.Z) && CurrentGear < _gearRatios.Length - 1)
            {
                CurrentGear++;
                CurrentRpm -= CurrentRpm / (CurrentGear + 1);
            }

            if (Input.GetKeyDown(KeyCode.X) && CurrentGear > 0) CurrentGear--;
            if (Input.GetKeyDown(KeyCode.R)) transform.position += new Vector3(0, 1, 0); 

        #endregion

        _rb.drag = _reverseInput && _rb.velocity.z <= .1f ? 1 : _startingDrag;
        Kph = _rb.velocity.magnitude * 3.6f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateEnginePower();
        ApplyBraking(_wheels, BrakeInput * _brakeForce);
        ApplyAcceleration(_poweredWheels, _currentTorque * ThrottleInput);
        ApplySteering(_wheelsThatSteer, _steeringInput);
        if (_handbrakeInput) ApplyBraking(_handbrakeWheels, _handBrakeForce);
        VisualWheelUpdate(_wheels);
        ApplyDownforce(_rb, _downforce);
    }

    private void UpdateEnginePower()
    {
        float wheelsRpm = GetWheelsTotalRpm(_wheels) * _gearRatios[CurrentGear] * _diffRatio;
        CurrentRpm = Mathf.Lerp(CurrentRpm, Mathf.Max(1000 - 100, wheelsRpm), Time.deltaTime * 2.5f);
        _currentTorque = _hpToRpmCurve.Evaluate(CurrentRpm / _rpmRedLine) * (_horsePower / CurrentRpm) * _gearRatios[CurrentGear] *
                         _diffRatio * 5252f ;
    }

    private void ApplyDownforce(Rigidbody rb, float downforceValue)
    {
        rb.AddForce(-transform.up * (downforceValue * rb.velocity.magnitude));
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