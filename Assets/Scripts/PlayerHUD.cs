using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PlayerHUD : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private Text speedText, rpmText, gearText;
    [SerializeField] private Slider throttleSlider, brakeSlider;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        gearText.text = (playerController.CurrentGear + 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        speedText.text = ((int)playerController.Kph).ToString() + "Km/h";
        rpmText.text = ((int)playerController.CurrentRpm).ToString() + "rpm";
        gearText.text = (playerController.CurrentGear + 1).ToString();
        throttleSlider.value = playerController.ThrottleInput >= 0 ? playerController.ThrottleInput : -playerController.ThrottleInput;
        brakeSlider.value = playerController.BrakeInput;
    }
}
