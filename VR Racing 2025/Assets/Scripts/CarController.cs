using LogitechG29.Sample.Input;
using System;
using System.Collections.Generic;
using UnityEngine;



public class CarController : MonoBehaviour
{
    [SerializeField] InputControllerReader inputControllerReader;

    [SerializeField] List<AxleInfo> axleInfos; // информаци€ о каждой отдельной оси автомобил€

    [SerializeField] float maxEnginePower; // максимальный крут€щий момент, который двигатель может приложить к колесу (max мощность двигател€)

    [SerializeField] float maxSteeringAngle; // максимальный угол поворота, который может иметь колесо

    float speed; // скорость машины

    bool EngineIsRunning; // запущен ли двигатель


    private void Awake()
    {
        inputControllerReader = new InputControllerReader();
    }

    private void Start()
    {
        EngineIsRunning = false;
        speed = 0f;
    }

    private void OnEnable()
    {
        inputControllerReader.OnHomeCallback += HandleOnHomeCallback;
        inputControllerReader.OnSouthButtonCallback += HandleOnSouthCallback;
    }

    private void OnDisable()
    {
        inputControllerReader.OnHomeCallback -= HandleOnHomeCallback;
        inputControllerReader.OnSouthButtonCallback -= HandleOnSouthCallback;
    }

    private void FixedUpdate()
    {
        InputMovement();
        if (EngineIsRunning)
        {
            UpdateWheelState();
        }
    }

    private void InputMovement() // ввод с педалей
    {       
        if (inputControllerReader.Throttle != 0 && EngineIsRunning)
        {
            speed = inputControllerReader.Throttle;
        }
        else if (inputControllerReader.Brake != 0 && EngineIsRunning)
        {
            speed = -inputControllerReader.Brake;
        }
    }

    private void UpdateWheelState() // поведение колес и ввод с рул€ (поворот)
    {
        //float current_power = speed * maxEnginePower;
        float current_power = Input.GetAxis("Vertical") * maxEnginePower;
        //float steering_angle = maxSteeringAngle * inputControllerReader.Steering;
        float steering_angle = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach (var info in axleInfos)
        {
            if (info.isSteering)
            {
                info.rightWheel.steerAngle = steering_angle;
                info.leftWheel.steerAngle = steering_angle;
            }
            if (info.isMotor)
            {
                info.rightWheel.motorTorque = current_power;
                info.leftWheel.motorTorque = current_power;
            }
        }
    }


    private void HandleOnHomeCallback(bool value)
    {
        if (value)
        {
            // ¬ыполните действие при нажатии кнопки Home
        }
        else
        {
            // ¬ыполните действие при отпускании кнопки Home
        }
    }

    private void HandleOnSouthCallback(bool value)
    {
        if (value && !EngineIsRunning)
        {
            EngineIsRunning = true;
            Debug.Log("ƒвигатель запущен!");
        }

        else if (value && EngineIsRunning)
        {
            EngineIsRunning = false;
            Debug.Log("ƒвигатель заглушен!");
        }
    }


    [Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool isMotor; // это колесо прикреплено к мотору?
        public bool isSteering; // может ли это колесо поворачивать?
    }
}
