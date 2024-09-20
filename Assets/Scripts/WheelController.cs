using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] protected Wheel[] Wheels = new Wheel[4];
    protected Wheel[] WheelsThatSteer;
    protected Wheel[] PoweredWheels;
    protected Wheel[] HandbrakeWheels;
    [SerializeField] protected float RearTrackWidth;
    [SerializeField] protected float WheelBase;
    [SerializeField] protected float WheelRadius;

    protected void ApplyTireSquealSound(float carKph)
    {
        foreach (var wheel in Wheels)
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
    
    protected void ApplyBraking(float brakeForce, bool useAbs, float absThreshold ,float carKph)
    {
        foreach (var wheel in Wheels)
        {
            float wheelRadPerSec = wheel.Collider.rotationSpeed * 0.017453f;
            float wheelKph = Mathf.Abs(3.6f * WheelRadius * wheelRadPerSec);

            wheel.Collider.brakeTorque = useAbs switch
            {
                true when wheelKph < carKph - 10 && carKph > absThreshold => 0,
                true when wheelKph > carKph + 10 && carKph > absThreshold => brakeForce * brakeForce,
                _ => brakeForce
            };
        }
    }

    protected void ApplyHandbrake(float handBrakeForce)
    {
        foreach (var wheel in HandbrakeWheels)
        {
            wheel.Collider.brakeTorque = handBrakeForce;
        }
    }

    protected void ApplyAcceleration(Wheel[] wheels, float torque)
    {
        foreach (var wheel in wheels)
        {
            wheel.Collider.motorTorque = torque / wheels.Length;
        }
    }

    protected void VisualWheelUpdate(Wheel[] wheels)
    {
        foreach (var wheel in wheels)
        {
            wheel.Collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.WheelMesh.position = pos;
            wheel.WheelMesh.rotation = rot;
        }
    }

    protected void ApplySteering(Wheel[] wheels, float steeringInput)
    {
        foreach (var wheel in wheels)
        {
            wheel.Collider.steerAngle = steeringInput switch
            {
                > 0 => wheel.SideWheelIsOn == Wheel.Side.Left ?
                    Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(false, steeringInput), Time.deltaTime * (WheelRadius /2)) 
                    : Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(true, steeringInput), Time.deltaTime * (WheelRadius /2)),  //AckermanSteeringCalc(true, steeringInput),
                      < 0 => wheel.SideWheelIsOn == Wheel.Side.Right ?
                    Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(false, steeringInput), Time.deltaTime * (WheelRadius /2)) 
                    : Mathf.Lerp(wheel.Collider.steerAngle, AckermanSteeringCalc(true, steeringInput), Time.deltaTime * (WheelRadius /2)), 
                    /*AckermanSteeringCalc(false, steeringInput)
                    : AckermanSteeringCalc(true, steeringInput),*/
                _ => Mathf.Lerp(wheel.Collider.steerAngle, 0, Time.deltaTime * WheelRadius) //0
            };
        }
    }

    protected float CalculateDistanceBetweenWheels(Wheel wheelOne, Wheel wheelTwo)
    {
        return Vector3.Distance(wheelOne.Collider.transform.position, wheelTwo.Collider.transform.position);
    }

    protected float GetWheelsAvgRpm()
    {
        float rpmSum = 0;

        for (int i = 0; i < Wheels.Length; i++)
        {
            rpmSum += Wheels[i].Collider.rpm;
        }

        return Wheels.Length != 0 ? Mathf.Abs(rpmSum / Wheels.Length) : 0;
    }
    
    protected Wheel[] GetFilteredWheels(WheelFilters filter)
    {
        List<Wheel> filteredWheels = new List<Wheel>();

        foreach (var wheel in Wheels)
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
                default:
                    break;
            }
        }

        return filteredWheels.ToArray();
    }

    /// <summary>
    /// Used for calculating the steering angle of the tire. mark true if the angle being calculated is the one from the tire on the inside of the turn.
    /// Use radius to determine max steering angle. NO LOWER THAN 1.
    /// </summary>
    /// <param name="isInsideWheel"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    private float AckermanSteeringCalc(bool isInsideWheel, float input)
    {
        return isInsideWheel switch
        {
            true => Mathf.Rad2Deg * Mathf.Atan(WheelBase / (WheelRadius - (RearTrackWidth / 2))) * input,
            false => Mathf.Rad2Deg * Mathf.Atan(WheelBase / (WheelRadius + (RearTrackWidth / 2))) * input
        };
    }
}

[Serializable]
public struct Wheel
{
    public WheelCollider Collider;
    public Transform WheelMesh;
    public bool Steer;
    public Axle WheelAxle;
    public Side SideWheelIsOn;
    public AudioSource AudioSource;

    public enum Side { Left, Right }
    public enum Axle { Front, Rear }
}

public enum WheelFilters { Steer, IsFrontWheel, IsRearWheel, IsLeftWheel, IsRightWheel, }
