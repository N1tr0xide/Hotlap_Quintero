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

    [SerializeField] private float _torque = 1000f;
    [SerializeField] private float _brakeForce = 600f;
    [SerializeField] private float _handBrakeForce = 600f;
    [SerializeField] private int _downforce;

    private float _currentTorque = 0f;
    private float _currentBrakeForce = 0f;
    private float _steeringInput;
    private bool _handbrakeInput;

    public float Kph { get; private set; }

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

        ApplyAcceleration(_poweredWheels, _currentTorque, _poweredWheels.Length);
        ApplySteering(_wheelsThatSteer, _steeringInput);
        VisualWheelUpdate(_wheels);
        ApplyDownforce(_rb, _downforce);
    }

    void PlayerInputs()
    {
        float brakeInput = Convert.ToSingle(Input.GetKey(KeyCode.Space));
        float throttleInput = Input.GetAxis("Vertical");

        _handbrakeInput = Input.GetKey(KeyCode.H);
        _steeringInput = Input.GetAxis("Horizontal");
        _currentBrakeForce = _brakeForce * brakeInput;
        _currentTorque = _torque * throttleInput;
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
        _rearTrackWidth = CalculateDistanceBetweenWheels(rearWheels[0], rearWheels[1]);
    }

    [ExposeMethodInEditor]
    private void DetermineWheelBase()
    {
        Wheel[] leftWheels = GetFilteredWheels(_wheels, WheelFilters.IsLeftWheel);
        _wheelBase = CalculateDistanceBetweenWheels(leftWheels[0], leftWheels[1]);
    }
}


