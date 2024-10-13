using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetPos : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _mouseRotationSpeed = 2.0f;
    private GameObject _parent;
    private PlayerInputActions _playerInputActions;
    private float _timer;
    
    // Start is called before the first frame update
    void Start()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Driving.Enable();
        _playerInputActions.Driving.MoveCameraJoystick.canceled += ResetPosition;
        
        _parent = transform.parent.gameObject;
        transform.localPosition = _offset;
        _timer = 3.6f;
    }
    
    private void OnDisable()
    {
        _playerInputActions.Driving.Disable();
    }
    
    void Update()
    {
        _timer = _playerInputActions.Driving.MoveCameraMouse.ReadValue<float>() == 0 &&
                 _playerInputActions.Driving.MoveCameraJoystick.ReadValue<Vector2>() == Vector2.zero
            ? _timer + Time.deltaTime
            : 0;

        if(_timer <= 3)
        {
            if(_playerInputActions.Driving.MoveCameraJoystick.ReadValue<Vector2>() != Vector2.zero)
            {
                Vector3 targetPosition = new Vector3(_offset.z * -_playerInputActions.Driving.MoveCameraJoystick.ReadValue<Vector2>().x, _offset.y, _offset.z * _playerInputActions.Driving.MoveCameraJoystick.ReadValue<Vector2>().y);
                transform.localPosition = targetPosition;
                return;
            }

            if (_playerInputActions.Driving.MoveCameraMouse.ReadValue<float>() == 0) return;
            float y = _playerInputActions.Driving.MoveCameraMouse.ReadValue<float>() * _mouseRotationSpeed;
            transform.RotateAround(_parent.transform.position, Vector3.up, y);
        }
        else
        {
            transform.localPosition = _offset;
        }
    }

    private void ResetPosition(InputAction.CallbackContext context)
    {
        transform.localPosition = _offset;
    }
}