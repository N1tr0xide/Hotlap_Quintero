using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private GameObject mainMenuCanvas, trackSelectionCanvas, instructionsCanvas;
    [SerializeField] private Transform mainMenuTransform, trackSelectionTransform;
    [SerializeField] private Text track01_lapRecordText, track02_lapRecordText, track03_lapRecordText;
    [SerializeField] private float cameraSpeed;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.transform.position = mainMenuTransform.position;
        mainMenuCanvas.SetActive(true);
        trackSelectionCanvas.SetActive(false);
        instructionsCanvas.SetActive(false);
        StartCoroutine(CarEngineShake());
        LoadTrackRecords();
    }

    private void LoadTrackRecords()
    {
        float track01Time = LapRecordsUtilities.GetLapRecordTime("Track_01");
        track01_lapRecordText.text = LapRecordsUtilities.FloatToStopWatchTime(track01Time);

        float track02Time = LapRecordsUtilities.GetLapRecordTime("Track_02");
        track02_lapRecordText.text = LapRecordsUtilities.FloatToStopWatchTime(track02Time);

        float track03Time = LapRecordsUtilities.GetLapRecordTime("Track_03");
        track03_lapRecordText.text = LapRecordsUtilities.FloatToStopWatchTime(track03Time);
    }

    public void PlayButton()
    {
        mainMenuCanvas.SetActive(false);
        StartCoroutine(MoveCameraToMenuView(trackSelectionTransform, trackSelectionCanvas));
    }

    public void InstructionsButton() 
    {
        mainMenuCanvas.SetActive(false);
        instructionsCanvas.SetActive(true);
    }

    public void ReturnToMainMenuButton()
    {
        trackSelectionCanvas.SetActive(false);
        instructionsCanvas.SetActive(false);
        StartCoroutine(MoveCameraToMenuView(mainMenuTransform, mainMenuCanvas));
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void StartLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void ResetLapRecordsButton()
    {
        LapRecordsUtilities.DeleteLapRecord("Track_01");
        LapRecordsUtilities.DeleteLapRecord("Track_02");
        LapRecordsUtilities.DeleteLapRecord("Track_03");
        LoadTrackRecords();
    }

    private IEnumerator CarEngineShake()
    {
        while (true)
        {
            gameObject.transform.position += new Vector3(0, 0.005f, 0);
            yield return new WaitForFixedUpdate();
            gameObject.transform.position -= new Vector3(0, 0.005f, 0);
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator MoveCameraToMenuView(Transform target, GameObject menuToActivate)
    {
        while (mainCamera.transform.rotation != target.transform.rotation)
        {
            mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, target.position, Time.deltaTime * cameraSpeed);
            mainCamera.transform.rotation = Quaternion.RotateTowards(mainCamera.transform.rotation, target.rotation, Time.deltaTime * cameraSpeed * 3);
            yield return new WaitForEndOfFrame();
        }

        menuToActivate.SetActive(true);
    }
}
