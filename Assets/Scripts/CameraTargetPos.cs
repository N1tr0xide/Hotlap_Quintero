using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetPos : MonoBehaviour
{
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = GetComponentInParent<Transform>().localPosition + offset;
    }
}