using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] protected float RearTrackWidth;
    [SerializeField] protected float WheelBase;
    [SerializeField] protected float Radius;

    protected void ApplyBraking(Wheel[] wheels, float brakeForce)
    {
        foreach (var wheel in wheels)
        {
            wheel.Collider.brakeTorque = brakeForce;
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
                    AckermanSteeringCalc(false, steeringInput)
                    : AckermanSteeringCalc(true, steeringInput),
                < 0 => wheel.SideWheelIsOn == Wheel.Side.Right ?
                    AckermanSteeringCalc(false, steeringInput)
                    : AckermanSteeringCalc(true, steeringInput),
                _ => 0
            };
        }
    }

    protected float CalculateDistanceBetweenWheels(Wheel wheelOne, Wheel wheelTwo)
    {
        return Vector3.Distance(wheelOne.Collider.transform.position, wheelTwo.Collider.transform.position);
    }

    protected float GetWheelsTotalRpm(Wheel[] wheels)
    {
        float rpmSum = 0;

        for (int i = 0; i < wheels.Length; i++)
        {
            rpmSum += wheels[i].Collider.rpm;
        }

        return wheels.Length != 0 ? rpmSum / wheels.Length : 0;
    }
    
    protected Wheel[] GetFilteredWheels(Wheel[] wheels, WheelFilters filter)
    {
        List<Wheel> filteredWheels = new List<Wheel>();

        foreach (var wheel in wheels)
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
            true => Mathf.Rad2Deg * Mathf.Atan(WheelBase / (Radius - (RearTrackWidth / 2))) * input,
            false => Mathf.Rad2Deg * Mathf.Atan(WheelBase / (Radius + (RearTrackWidth / 2))) * input
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

    public enum Side { Left, Right }
    public enum Axle { Front, Rear }
}

public enum WheelFilters { Steer, IsFrontWheel, IsRearWheel, IsLeftWheel, IsRightWheel, }
