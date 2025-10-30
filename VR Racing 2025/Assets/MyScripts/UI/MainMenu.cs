using System.Collections;
using LogitechG29.Sample.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] GameObject MainMenuScreen;
    [SerializeField] Button PlayButton;
    [SerializeField] Button SettingsButton;
    [SerializeField] Button QuitButton;

    [SerializeField] GameObject SettingsScreen;
    [SerializeField] Button CloseSettingsButton;

    [SerializeField] GameObject QuitScreenConfirm;
    [SerializeField] Button YesQuit;
    [SerializeField] Button NoStay;

    private void Awake()
    {
        MainMenuScreen.SetActive(true);
        SettingsScreen.SetActive(false);
        QuitScreenConfirm.SetActive(false);
        StartCoroutine(DelayBetweenScreens(PlayButton));
    }

    private void OnEnable()
    {
        PlayButton.onClick.AddListener(StartGame);
        QuitButton.onClick.AddListener(Quit);
        SettingsButton.onClick.AddListener(OpenSettings);
        CloseSettingsButton.onClick.AddListener(CloseSettings);
        YesQuit.onClick.AddListener(QuitConfirm);
        NoStay.onClick.AddListener(CancelQuit);
    }

    private void OnDisable()
    {
        PlayButton.onClick.RemoveListener(StartGame);
        QuitButton.onClick.RemoveListener(Quit);
        SettingsButton.onClick.RemoveListener(OpenSettings);
        CloseSettingsButton.onClick.RemoveListener(CloseSettings);
        YesQuit.onClick.RemoveListener(QuitConfirm);
        NoStay.onClick.RemoveListener(CancelQuit);
    }

    private void StartGame()
    {
        if (!CarController.OffInput)

            PauseScreenWork.isPaused = false;
            Time.timeScale = 1f;
            Trunk.CleanCounter();
            SceneManager.LoadScene("MainScene");
            StartCoroutine(CarController.OffInputDelay());  
    }

    private void OpenSettings()
    {
        MainMenuScreen.SetActive(false);
        SettingsScreen.SetActive(true);
        StartCoroutine(DelayBetweenScreens(CloseSettingsButton));
    }

    private void CloseSettings()
    {
        MainMenuScreen.SetActive(true);
        SettingsScreen.SetActive(false);
        StartCoroutine(DelayBetweenScreens(PlayButton));
    }

    private void Quit()
    {
        MainMenuScreen.SetActive(false);
        QuitScreenConfirm.SetActive(true);
        StartCoroutine(DelayBetweenScreens(YesQuit));
    }

    private void QuitConfirm()
    {
        //Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    private void CancelQuit()
    {
        MainMenuScreen.SetActive(true);
        QuitScreenConfirm.SetActive(false);
        StartCoroutine(DelayBetweenScreens(PlayButton));
    }

    private IEnumerator DelayBetweenScreens(Button buttonToSelect) // задержка во время переключения между экранами паузы
    {
        SwitchInteractableState(false);

        yield return new WaitForSecondsRealtime(0.5f);

        if (buttonToSelect != null)
        {
            buttonToSelect.Select();
        }        
        SwitchInteractableState(true);
    }

    private void SwitchInteractableState(bool value)
    {
        PlayButton.interactable = value;
        SettingsButton.interactable = value;
        QuitButton.interactable = value;
        CloseSettingsButton.interactable = value;
        YesQuit.interactable = value;
        NoStay.interactable = value;
    }
}
