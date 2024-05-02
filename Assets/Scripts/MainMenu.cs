using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Camera _mainCamera;
    private AudioSource _audioSource;
    [SerializeField] private GameObject _mainMenuCanvas, _trackSelectionCanvas, _instructionsCanvas;
    [SerializeField] private Transform _mainMenuTransform, _trackSelectionTransform;
    [SerializeField] private Text _track01LapRecordText, _track02LapRecordText, _track03LapRecordText;
    [SerializeField] private float _cameraSpeed;
    [SerializeField] private AudioClip _startEngineSound, _engineSound;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _audioSource = GetComponent<AudioSource>();
        _mainCamera.transform.position = _mainMenuTransform.position;
        _mainMenuCanvas.SetActive(true);
        _trackSelectionCanvas.SetActive(false);
        _instructionsCanvas.SetActive(false);
        StartCoroutine(CarEngineShake());
        LoadTrackRecords();

        float engineTimeDelay = _startEngineSound.length - .5f;
        _audioSource.PlayOneShot(_startEngineSound);
        _audioSource.clip = _engineSound;
        _audioSource.loop = true;
        _audioSource.PlayDelayed(engineTimeDelay);
    }

    private void LoadTrackRecords()
    {
        float track01Time = LapRecordsUtilities.GetLapRecordTime("Track_01");
        _track01LapRecordText.text = LapRecordsUtilities.FloatToStopWatchTime(track01Time);

        float track02Time = LapRecordsUtilities.GetLapRecordTime("Track_02");
        _track02LapRecordText.text = LapRecordsUtilities.FloatToStopWatchTime(track02Time);

        float track03Time = LapRecordsUtilities.GetLapRecordTime("Track_03");
        _track03LapRecordText.text = LapRecordsUtilities.FloatToStopWatchTime(track03Time);
    }

    public void PlayButton()
    {
        _mainMenuCanvas.SetActive(false);
        StartCoroutine(MoveCameraToMenuView(_trackSelectionTransform, _trackSelectionCanvas));
    }

    public void InstructionsButton() 
    {
        _mainMenuCanvas.SetActive(false);
        _instructionsCanvas.SetActive(true);
    }

    public void ReturnToMainMenuButton()
    {
        _trackSelectionCanvas.SetActive(false);
        _instructionsCanvas.SetActive(false);
        StartCoroutine(MoveCameraToMenuView(_mainMenuTransform, _mainMenuCanvas));
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
        while (_mainCamera.transform.rotation != target.transform.rotation)
        {
            _mainCamera.transform.position = Vector3.MoveTowards(_mainCamera.transform.position, target.position, Time.deltaTime * _cameraSpeed);
            _mainCamera.transform.rotation = Quaternion.RotateTowards(_mainCamera.transform.rotation, target.rotation, Time.deltaTime * _cameraSpeed * 3);
            yield return new WaitForEndOfFrame();
        }

        menuToActivate.SetActive(true);
    }
}
