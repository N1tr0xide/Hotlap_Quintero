using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    private Camera _camera;
    private PlayerController _playerController;
    private GameObject _targetPos;

    private float _speed, _defaultFOV;
    [SerializeField, Range(0,3)] private float _smoothTime;
    
    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _defaultFOV = _camera.fieldOfView;
        _targetPos = _player.GetComponentInChildren<CameraTargetPos>().gameObject;
        _playerController = _player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowTarget(_targetPos.transform.position);
        ChangeFOV(_defaultFOV, _smoothTime);
    }

    private void FollowTarget(Vector3 target)
    {
        _speed = Mathf.Lerp(_speed, _playerController.Kph / 3, Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * _speed);
        transform.LookAt(_player.transform);
    }

    private void ChangeFOV(float desiredFOV, float smoothTime)
    {
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, desiredFOV, Time.deltaTime * smoothTime);
    }
}
