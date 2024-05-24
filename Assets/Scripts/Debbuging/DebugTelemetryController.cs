using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTelemetryController : MonoBehaviour
{
    [SerializeField] private PlayerControllerDEBUG _playerController;
    [SerializeField] private DebugMode _debugMode;

    private Wheel _frontLeft, _frontRight;
    private enum DebugMode
    {
        TireAngle
    }

    [Header("UI Elements")] [SerializeField]
    
    private GameObject _tireAngleCanvas;
    [SerializeField] private Text _leftTireAngle, _rightTireAngle;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _playerController.SteerWheels.Length; i++)
        {
            if (_playerController.SteerWheels[i].SideWheelIsOn == Wheel.Side.Left)
            {
                _frontLeft = _playerController.SteerWheels[i];
            }
            else
            {
                _frontRight = _playerController.SteerWheels[i];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_debugMode == DebugMode.TireAngle)
        {
            _tireAngleCanvas.SetActive(true);
            _leftTireAngle.text = $"{(int)_frontLeft.Collider.steerAngle}";
            _rightTireAngle.text = $"{(int)_frontRight.Collider.steerAngle}";
        }
    }
}
