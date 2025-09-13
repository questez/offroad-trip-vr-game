using LogitechG29.Sample.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CarControllerSample;


public class CarController : MonoBehaviour
{
    [SerializeField] InputControllerReader inputControllerReader;

    [SerializeField] List<AxleInfo> axleInfos; // информация о каждой отдельной оси автомобиля

    [SerializeField] float maxEnginePower; // максимальный крутящий момент, который двигатель может приложить к колесу (max мощность двигателя)

    [SerializeField] float maxSteeringAngle; // максимальный угол поворота, который может иметь колесо

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

    private void FixedUpdate()
    {
        float speed = 0f;

        if (inputControllerReader.Throttle != 0)
        {
            speed = inputControllerReader.Throttle;
        }
        else if (inputControllerReader.Brake != 0)
        {
            speed = -inputControllerReader.Brake;
        }
        
        else if (Input.GetKey(KeyCode.Space))
        {
            speed = 0;
        }

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


    [Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool isMotor; // это колесо прикреплено к мотору?
        public bool isSteering; // может ли это колесо поворачивать?
    }
}
