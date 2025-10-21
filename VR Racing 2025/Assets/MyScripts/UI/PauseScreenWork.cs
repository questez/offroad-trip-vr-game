using LogitechG29.Sample.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseScreenWork : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    private bool isPaused;
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

    private void TogglePause(bool value)
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

    private void Resume()
    {
        PauseScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Restart()
    {
        PauseScreen.SetActive(false);
        RestartScreenConfirm.SetActive(true);
        YesRestart.Select();
    }

    private void ConfirmRestart()
    {
        SceneManager.LoadScene(active_scene_name);
    }
    private void CancelRestart()
    {
        PauseScreen.SetActive(true);
        RestartScreenConfirm.SetActive(false);
        ResumeButton.Select();
    }

    private void Quit()
    {
        PauseScreen.SetActive(false);
        QuitScreenConfirm.SetActive(true);
        YesQuit.Select();
    }

    private void ConfirmQuit()
    {
        //SceneManager.LoadScene("MainMenu");
        Debug.Log("Вышел в главное меню!");
    }
    private void CancelQuit()
    {
        PauseScreen.SetActive(true);
        QuitScreenConfirm.SetActive(false);
        ResumeButton.Select();
    }
}
