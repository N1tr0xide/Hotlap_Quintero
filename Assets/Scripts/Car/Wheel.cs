using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Wheel
{
    public WheelCollider Collider;
    public Transform WheelMesh;
    public bool Steer;
    public Axle WheelAxle;
    public Side SideWheelIsOn;
    public AudioSource AudioSource;
    public TrailRenderer SkidMarkTrail;

    public enum Side { Left, Right }
    public enum Axle { Front, Rear }
}

public enum WheelFilters { Steer, IsFrontWheel, IsRearWheel, IsLeftWheel, IsRightWheel, }
