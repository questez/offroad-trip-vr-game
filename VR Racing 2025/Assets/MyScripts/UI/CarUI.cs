using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LogitechG29.Sample.Input;

public class CarUI : MonoBehaviour
{
    private bool isPaused;
    [SerializeField] GameObject PauseScreen;


    [SerializeField] private InputControllerReader inputControllerReader;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private TextMeshProUGUI currentSpeed;
    [SerializeField] private TextMeshProUGUI currentShifter;
    [SerializeField] private TextMeshProUGUI currentWheelDriveMode;

    private void Awake()
    {
        isPaused = false;
        PauseScreen.SetActive(false);
    }

    private void OnEnable()
    {
        inputControllerReader.OnHomeCallback += TogglePause;
    }

    private void OnDisable()
    {
        inputControllerReader.OnHomeCallback -= TogglePause;
    }

    private void Update()
    {
        UpdateBoardInfo();               
    }

    private void UpdateBoardInfo()
    {
        if (rb != null && currentShifter != null && currentSpeed != null)
        {
            int speed = Mathf.RoundToInt(rb.linearVelocity.magnitude * 3.6f);

            currentShifter.text = CarController.current_shifter.ToString();
            currentSpeed.text = speed.ToString() + " κμ/χ";
            currentWheelDriveMode.text = CarController.wheel_drive_mode;
        }        
    }

    private void TogglePause(bool value)
    {
        if (value)
        {            
            if (!isPaused)
            {
                Time.timeScale = 0f;
                isPaused = true;
            }
            else
            {                
                Time.timeScale = 1f;
                isPaused = false;
            }
            PauseScreen.SetActive(isPaused);
        }
    }


}
