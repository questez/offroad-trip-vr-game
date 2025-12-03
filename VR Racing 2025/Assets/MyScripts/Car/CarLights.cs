using LogitechG29.Sample.Input;
using UnityEngine;

public class CarLights : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private Light leftLight;
    [SerializeField] private Light rightLight;

    [SerializeField] private AudioSource lightModeToggleSound;

    private void Start()
    {
        leftLight.enabled = false;
        rightLight.enabled = false;
    }

    private void OnEnable()
    {
        inputControllerReader.OnLeftShiftCallback += SetLightsCondition;
    }

    private void OnDisable()
    {
        inputControllerReader.OnLeftShiftCallback -= SetLightsCondition;
    }

    private void SetLightsCondition(bool value)
    {
        if (Wheather.GetDayStatus() == "night" && CarController.EngineIsRunning)
        {
            if (value)
            {
                if (lightModeToggleSound != null)
                {
                    lightModeToggleSound.Play();
                }

                if (!leftLight.isActiveAndEnabled)
                {
                    leftLight.enabled = true;
                    rightLight.enabled = true;
                }
                else
                {
                    leftLight.enabled = false;
                    rightLight.enabled = false;
                }
            }
        }        
    }
}
