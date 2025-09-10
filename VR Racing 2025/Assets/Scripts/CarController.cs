using LogitechG29.Sample.Input;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] InputControllerReader inputControllerReader;

    private void Awake()
    {
        inputControllerReader = new InputControllerReader();
    }  


    private void OnEnable()
    {
        inputControllerReader.OnHomeCallback += HandleOnHomeCallback;
        inputControllerReader.OnOptionsCallback += HandleOnOptionsCallback;
    }

    private void OnDisable()
    {
        inputControllerReader.OnHomeCallback -= HandleOnHomeCallback;
        inputControllerReader.OnOptionsCallback -= HandleOnOptionsCallback;
    }

    private void Update()
    {
        //inputControllerReader.SetDebugMode(true);
    }

    private void HandleOnHomeCallback(bool value)
    {
        if (value)
        {
            // Выполните действие при нажатии кнопки Home
        }
        else
        {
            // Выполните действие при отпускании кнопки Home
        }
    }

    private void HandleOnOptionsCallback(bool value)
    {
        if (value)
        {
            // Выполните действие при нажатии кнопки Options
        }
        else
        {
            // Выполните действие при отпускании кнопки Options
        }
    }

    


}
