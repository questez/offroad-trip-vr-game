using LogitechG29.Sample.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseScreenWork : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    public static bool isPaused;
    [SerializeField] GameObject PauseScreen;
    [SerializeField] Button ResumeButton;
    [SerializeField] Button RestartButton;
    [SerializeField] Button QuitButton;

    [SerializeField] GameObject RestartScreenConfirm;
    [SerializeField] Button YesRestart;
    [SerializeField] Button NoStay1;

    [SerializeField] GameObject QuitScreenConfirm;
    [SerializeField] Button YesQuit;
    [SerializeField] Button NoStay2;

    private string active_scene_name;

    private void Awake()
    {
        active_scene_name = SceneManager.GetActiveScene().name;
        isPaused = false;
        PauseScreen.SetActive(false);
        QuitScreenConfirm.SetActive(false);
        RestartScreenConfirm.SetActive(false);
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
            }
            PauseScreen.SetActive(isPaused);
        }
    }

    private void Resume() // продолжить игру
    {
        PauseScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Restart() // переход на окно подтверждени€ рестарта игры
    {
        PauseScreen.SetActive(false);
        RestartScreenConfirm.SetActive(true);
        StartCoroutine(DelayBetweenScreens(YesRestart));
    }

    private void ConfirmRestart() // рестарт игры
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(active_scene_name);
    }
    private void CancelRestart() // отмена рестарта
    {
        PauseScreen.SetActive(true);
        RestartScreenConfirm.SetActive(false);
        StartCoroutine(DelayBetweenScreens(ResumeButton));
    }

    private void Quit() // переход на окно подтверждени€ выхода в меню
    {
        PauseScreen.SetActive(false);
        QuitScreenConfirm.SetActive(true);
        StartCoroutine(DelayBetweenScreens(YesQuit));
    }

    private void ConfirmQuit() // выход в меню
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("¬ышел в главное меню!");
    }
    private void CancelQuit() // отмена выхода в меню
    {
        PauseScreen.SetActive(true);
        QuitScreenConfirm.SetActive(false);        
        StartCoroutine(DelayBetweenScreens(ResumeButton));
    }

    private IEnumerator DelayBetweenScreens(Button buttonToSelect) // задержка во врем€ переключени€ между экранами паузы
    {
        SwitchInteractableState(false);

        yield return new WaitForSecondsRealtime(0.3f);

        if (buttonToSelect != null)
        {
            buttonToSelect.Select();
        }
        SwitchInteractableState(true);
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
}
