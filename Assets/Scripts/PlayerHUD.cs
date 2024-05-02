using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PlayerHUD : MonoBehaviour
{
    private PlayerController _playerController;
    [SerializeField] private GameObject _hudCanvas;
    [SerializeField] private Text _speedText, _rpmText, _gearText;
    [SerializeField] private Slider _throttleSlider, _brakeSlider;

    // Start is called before the first frame update
    void Start()
    {
        _hudCanvas.SetActive(true);
        _playerController = GetComponent<PlayerController>();
        _gearText.text = (_playerController.CurrentGear + 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        _speedText.text = ((int)_playerController.Kph).ToString() + "Km/h";
        _rpmText.text = ((int)_playerController.CurrentRpm).ToString() + "rpm";
        _gearText.text = (_playerController.CurrentGear + 1).ToString();
        _throttleSlider.value = _playerController.ThrottleInput >= 0 ? _playerController.ThrottleInput : -_playerController.ThrottleInput;
        _brakeSlider.value = _playerController.BrakeInput;
    }
}
