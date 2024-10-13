using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [SerializeField] private CarConfiguration _carConfig;
    [SerializeField] protected float WheelRadius;
    private CarLightsController _lightsController;
    [SerializeField] private Wheel[] _wheels = new Wheel[4];
    private Wheel[] _wheelsThatSteer;
    private Wheel[] _poweredWheels;
    private Wheel[] _handbrakeWheels;
    private float _rearTrackWidth;
    private float _wheelBase;
    private Rigidbody _rb;
    
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
    public float RpmRedLine => _carConfig.RpmRedLine;
    public bool ReverseActive => _reverseActive;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _lightsController = GetComponentInChildren<CarLightsController>();
        
        _wheelsThatSteer = GetFilteredWheels(WheelFilters.Steer);
        _poweredWheels = SetDrive(_carConfig.Drive);
        _handbrakeWheels = GetFilteredWheels(WheelFilters.IsRearWheel);
        _standardDrag = _rb.drag;
        _power = _carConfig.HorsePower * 3.6f; // adapt horsepower to match real life results
        
        Wheel[] rearWheels = GetFilteredWheels(WheelFilters.IsRearWheel);
        _rearTrackWidth = Vector3.Distance(rearWheels[0].Collider.transform.position, rearWheels[1].Collider.transform.position);
        Wheel[] leftWheels = GetFilteredWheels(WheelFilters.IsLeftWheel);
        _wheelBase = Vector3.Distance(leftWheels[0].Collider.transform.position, leftWheels[1].Collider.transform.position);
    }

    void Update()
    {
        Kph = _rb.velocity.magnitude * 3.6f;
        ApplyTireSquealSound(Kph);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateEnginePower(_carConfig);
        VisualWheelUpdate(_wheels);
        ApplyDownforce(_carConfig.Downforce);
    }
    
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

    private IEnumerator ApplyDragForSeconds()
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
            _ => _wheels
        };

        return w;
    }

    private void ApplyTireSquealSound(float carKph) 
    {
        foreach (var wheel in _wheels)
        {
            wheel.Collider.GetGroundHit(out WheelHit hit);
            float forwardSlipValue = Mathf.Abs(hit.forwardSlip);
            float sidewaysSlipValue = Mathf.Abs(hit.sidewaysSlip);

            if(hit.collider && carKph > 10 && (forwardSlipValue >= .7f || sidewaysSlipValue >= .4f))
            {
                wheel.AudioSource.volume = Mathf.MoveTowards(wheel.AudioSource.volume, 1, Time.deltaTime);
                return;
            }

            wheel.AudioSource.volume = Mathf.MoveTowards(wheel.AudioSource.volume, 0, Time.deltaTime * 2);
        }
    }

    private void VisualWheelUpdate(Wheel[] wheels) 
    {
        foreach (var wheel in wheels)
        {
            wheel.Collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.WheelMesh.position = pos;
            wheel.WheelMesh.rotation = rot;
        }
    }

    private float GetWheelsAvgRpm() 
    {
        float rpmSum = 0;

        for (int i = 0; i < _wheels.Length; i++)
        {
            rpmSum += _wheels[i].Collider.rpm;
        }

        return _wheels.Length != 0 ? Mathf.Abs(rpmSum / _wheels.Length) : 0;
    }

    private Wheel[] GetFilteredWheels(WheelFilters filter) 
    {
        List<Wheel> filteredWheels = new List<Wheel>();

        foreach (var wheel in _wheels)
        {
            switch (filter)
            {
                case WheelFilters.Steer:
                    if (wheel.Steer) filteredWheels.Add(wheel);
                    break;
                case WheelFilters.IsFrontWheel:
                    if (wheel.WheelAxle == Wheel.Axle.Front) filteredWheels.Add(wheel);
                    break;
                case WheelFilters.IsRearWheel:
                    if (wheel.WheelAxle == Wheel.Axle.Rear) filteredWheels.Add(wheel);
                    break;
                case WheelFilters.IsLeftWheel:
                    if (wheel.SideWheelIsOn == Wheel.Side.Left) filteredWheels.Add(wheel);
                    break;
                case WheelFilters.IsRightWheel:
                    if (wheel.SideWheelIsOn == Wheel.Side.Right) filteredWheels.Add(wheel);
                    break;
            }
        }

        return filteredWheels.ToArray();
    }

    /// <summary>
    /// /// Used for calculating the steering angle of the tire. mark true if the angle being calculated is the one from the tire on the inside of the turn.
    /// Use radius to determine max steering angle. NO LOWER THAN 1.
    /// </summary>
    /// <param name="isInsideWheel"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    private float AckermanSteeringCalc(bool isInsideWheel, float input) 
    {
        return isInsideWheel switch
        {
            true => Mathf.Rad2Deg * Mathf.Atan(_wheelBase / (WheelRadius - (_rearTrackWidth / 2))) * input,
            false => Mathf.Rad2Deg * Mathf.Atan(_wheelBase / (WheelRadius + (_rearTrackWidth / 2))) * input
        };
    }
    
    #region Input Related

        public void ApplyAcceleration(float throttleInput) 
        {
            foreach (var wheel in _poweredWheels)
            {
                wheel.Collider.motorTorque = (_currentTorque * throttleInput) / _poweredWheels.Length;
            }
        }
        
        public void ApplyBraking(float brakeInput) 
        {
            foreach (var wheel in _wheels)
            {
                float wheelRadPerSec = wheel.Collider.rotationSpeed * 0.017453f;
                float wheelKph = Mathf.Abs(3.6f * WheelRadius * wheelRadPerSec);

                wheel.Collider.brakeTorque = _carConfig.Abs switch
                {
                    true when wheelKph < Kph - 10 && Kph > _carConfig.AbsThreshold => 0,
                    true when wheelKph > Kph + 10 && Kph > _carConfig.AbsThreshold => _carConfig.BrakeForce * _carConfig.BrakeForce * brakeInput,
                    _ => _carConfig.BrakeForce * brakeInput
                };
            }
        }
        
        public void ApplySteering(float steeringInput) 
        {
            foreach (var wheel in _wheelsThatSteer)
            {
                wheel.Collider.steerAngle = steeringInput switch
                {
                    > 0 => wheel.SideWheelIsOn == Wheel.Side.Left ?
                        Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(false, steeringInput), Time.deltaTime * (WheelRadius /2)) 
                        : Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(true, steeringInput), Time.deltaTime * (WheelRadius /2)),  //AckermanSteeringCalc(true, steeringInput),
                    < 0 => wheel.SideWheelIsOn == Wheel.Side.Right ?
                        Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(false, steeringInput), Time.deltaTime * (WheelRadius /2)) 
                        : Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(true, steeringInput), Time.deltaTime * (WheelRadius /2)),
                    _ => Mathf.Lerp(wheel.Collider.steerAngle, 0, Time.deltaTime * WheelRadius) //0
                };
            }
        }
        
        public void ApplyHandbrake(float handBrakeInput) 
        {
            foreach (var wheel in _handbrakeWheels)
            {
                wheel.Collider.brakeTorque = _carConfig.HandBrakeForce * handBrakeInput;
            }
        }
        
        public void GearUp(InputAction.CallbackContext context)
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

        public void GearDown(InputAction.CallbackContext context)
        {
            if (CurrentGear > 0)
            {
                CurrentGear--;
                StartCoroutine(ApplyShiftDownClutch());
                StartCoroutine(ApplyDragForSeconds());
                return;
            }

            if (!(Kph < 5f)) return;
            _reverseActive = true;
            _lightsController.SetReverseLightsActive(true);
        }

    #endregion
}