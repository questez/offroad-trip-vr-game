using LogitechG29.Sample.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PauseScreenWork : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private AudioSource clickSound;

    public static bool isPaused;

    [SerializeField] private GameObject PauseScreen;
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button RestartButton;
    [SerializeField] private Button QuitButton;

    [SerializeField] private GameObject RestartScreenConfirm;
    [SerializeField] private Button YesRestart;
    [SerializeField] private Button NoStay1;

    [SerializeField] private GameObject QuitScreenConfirm;
    [SerializeField] private Button YesQuit;
    [SerializeField] private Button NoStay2;

    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private Slider loading_bar;

    private string active_scene_name;
    private string next_scene_name;

    [SerializeField] private TextMeshProUGUI playerBalanceText; 
    [SerializeField] private TextMeshProUGUI finishedMissionsCounterText;
    [SerializeField] private TextMeshProUGUI CurrentMissionText;

    private void Start()
    {
        active_scene_name = SceneManager.GetActiveScene().name;
        next_scene_name = "MainMenu";
        isPaused = false;
        PauseScreen.SetActive(false);
        QuitScreenConfirm.SetActive(false);
        RestartScreenConfirm.SetActive(false);
        LoadingScreen.SetActive(false);
        PlayerData.PlayerBalance = 0;
        PlayerData.CurrentMission = "None";
        PlayerData.FinishedMissionsCounter = 0;
    }      

    private void SetTextInfo()
    {
        playerBalanceText.text = "Баланс: " + PlayerData.PlayerBalance + "руб";
        finishedMissionsCounterText.text = "Пройдено миссий: " + PlayerData.FinishedMissionsCounter;

        if (PlayerData.CurrentMission != "None")
        {
            CurrentMissionText.text = "Текущая миссия: " + PlayerData.CurrentMission;
        }
        else
        {
            CurrentMissionText.text = "Текущая миссия: -";
        }
    }

    private void TogglePause(bool value) // вкл/выкл режим паузы
    {
        if (value)
        {
            if (!isPaused)
            {
                Time.timeScale = 0f;
                isPaused = true;
                ResumeButton.Select();
            }
            else
            {
                Time.timeScale = 1f;
                isPaused = false;
                StartCoroutine(CarController.OffInputDelay());
            }
            PauseScreen.SetActive(isPaused);
            SetTextInfo();
        }
    }

    private void Resume() // продолжить игру
    {
        clickSound.Play();
        PauseScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        StartCoroutine(CarController.OffInputDelay());
    }

    private void Restart() // переход на окно подтверждения рестарта игры
    {
        clickSound.Play();
        PauseScreen.SetActive(false);
        RestartScreenConfirm.SetActive(true);
        StartCoroutine(DelayBetweenScreens(YesRestart));
        StartCoroutine(CarController.OffInputDelay());
    }

    private void ConfirmRestart() // рестарт игры
    {
        clickSound.Play();
        isPaused = false;
        Time.timeScale = 1f;
        Trunk.CleanCounter();
        PauseScreen.SetActive(false);
        QuitScreenConfirm.SetActive(false);
        RestartScreenConfirm.SetActive(false);
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadAsync(active_scene_name));
    }
    private void CancelRestart() // отмена рестарта
    {
        clickSound.Play();
        PauseScreen.SetActive(true);
        RestartScreenConfirm.SetActive(false);
        StartCoroutine(DelayBetweenScreens(ResumeButton));
    }

    private void Quit() // переход на окно подтверждения выхода в меню
    {
        clickSound.Play();
        PauseScreen.SetActive(false);
        QuitScreenConfirm.SetActive(true);
        StartCoroutine(DelayBetweenScreens(YesQuit));
    }

    private void ConfirmQuit() // выход в меню
    {
        clickSound.Play();
        StartCoroutine(CarController.OffInputDelay());
        PauseScreen.SetActive(false);
        QuitScreenConfirm.SetActive(false);
        RestartScreenConfirm.SetActive(false);
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadAsync(next_scene_name));
    }
    private void CancelQuit() // отмена выхода в меню
    {
        clickSound.Play();
        PauseScreen.SetActive(true);
        QuitScreenConfirm.SetActive(false);        
        StartCoroutine(DelayBetweenScreens(ResumeButton));
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

    private IEnumerator LoadAsync(string new_scene) // асинхронная загрузка сцены
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(new_scene);

        while (!asyncLoad.isDone)
        {
            loading_bar.value = asyncLoad.progress;
            yield return null;
        }
    }

    private void SwitchInteractableState(bool value)
    {
        ResumeButton.interactable = value;
        RestartButton.interactable = value;
        QuitButton.interactable = value;
        YesRestart.interactable = value;
        NoStay1.interactable = value;
        YesQuit.interactable = value;
        NoStay2.interactable = value;
    }

    private void OnEnable()
    {
        ResumeButton.onClick.AddListener(Resume);
        RestartButton.onClick.AddListener(Restart);
        YesRestart.onClick.AddListener(ConfirmRestart);
        NoStay1.onClick.AddListener(CancelRestart);
        QuitButton.onClick.AddListener(Quit);
        YesQuit.onClick.AddListener(ConfirmQuit);
        NoStay2.onClick.AddListener(CancelQuit);

        inputControllerReader.OnHomeCallback += TogglePause;
    }

    private void OnDisable()
    {
        ResumeButton.onClick.RemoveListener(Resume);
        RestartButton.onClick.RemoveListener(Restart);
        YesRestart.onClick.RemoveListener(ConfirmRestart);
        NoStay1.onClick.RemoveListener(CancelRestart);
        QuitButton.onClick.RemoveListener(Quit);
        YesQuit.onClick.RemoveListener(ConfirmQuit);
        NoStay2.onClick.RemoveListener(CancelQuit);

        inputControllerReader.OnHomeCallback -= TogglePause;
    }
}
