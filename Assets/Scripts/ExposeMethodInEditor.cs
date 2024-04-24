using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Restrict to methods only
[AttributeUsage(AttributeTargets.Method)]
public class ExposeMethodInEditor : Attribute
{
}
