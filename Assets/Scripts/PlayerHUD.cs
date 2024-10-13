using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CarController)), RequireComponent(typeof(PlayerInputController))]
public class PlayerHUD : MonoBehaviour
{
    private CarController _playerController;
    private PlayerInputController _playerInput;
    [SerializeField] private GameObject _hudCanvas;
    [SerializeField] private Text _speedText, _rpmText, _gearText;
    [SerializeField] private Slider _throttleSlider, _brakeSlider;

    // Start is called before the first frame update
    void Start()
    {
        _hudCanvas.SetActive(true);
        _playerController = GetComponent<CarController>();
        _playerInput = GetComponent<PlayerInputController>();
        _gearText.text = (_playerController.CurrentGear + 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        _speedText.text = ((int)_playerController.Kph) + " Km/h";
        _rpmText.text = ((int)_playerController.CurrentRpm) + " rpm";
        _gearText.text = (_playerController.CurrentGear + 1).ToString();
        _throttleSlider.value = _playerInput.ThrottleInput >= 0 ? _playerInput.ThrottleInput : -_playerInput.ThrottleInput;
        _brakeSlider.value = _playerInput.BrakeInput;
    }
}
