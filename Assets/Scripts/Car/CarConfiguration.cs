using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarConfiguration : ScriptableObject
{
    [Tooltip("Set of wheels that the engine's power is delivered to.\n Fwd = front wheels. \n Rwd = Rear wheels. \n Awd = all wheels.")] 
    public DriveType Drive;

    [Tooltip("Set to true to stop the wheels from locking up on braking. \nEspecially useful with really high brake forces.")] 
    public bool Abs;

    [Tooltip("if ABS is active, and the car keeps rolling despite braking. \nUse this value to determine the minimum speed (in KPH) required for ABS to activate.")]
    public int AbsThreshold;
    
    [Tooltip("Engine's Power")] 
    public float HorsePower;
    
    [Tooltip("Maximum engine revolutions per Minute")] 
    public float RpmRedLine;
    
    [Tooltip("Use this to set the car's top speed and acceleration. \nA higher value leads to lower top speed and acceleration")] 
    public float DifferentialRatio;
    
    [Tooltip("Use this to modify the way the engine delivers power and increases the RPM")]
    public AnimationCurve HpToRpmCurve;

    [Tooltip("Use this to modify the length of each gear.\nA lower value makes a shorter gear that delivers power faster.")]
    public float[] GearRatios = new float[] {3, 2.5f, 2, 1.5f, 1};
    
    public float BrakeForce = 600f, HandBrakeForce = 2000f;
    
    [Tooltip("Force applied downward to the vehicle for better stability. \nThis force is multiplied by the car's current Kph.")]
    public int Downforce;
}

public enum DriveType { Fwd, Rwd, Awd }
