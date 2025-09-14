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
    [SerializeField] float maxBrakeForce; // максимальна€ тормозна€ сила
    
    [SerializeField] float MaxSpeed; // максимально допустима€ скорость машины

    bool EngineIsRunning; // запущен ли двигатель


    private void Awake()
    {
        inputControllerReader = new InputControllerReader();
    }

    private void Start()
    {
        EngineIsRunning = false;
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
        if (EngineIsRunning)
        {
            UpdateWheelState();
        }
        //Debug.Log($"SPEED: {Input.GetAxis("Vertical")}");
    }

    private void UpdateWheelState() // поведение колес и ввод с рул€ (поворот)
    {
        float speed = 0f;

        if (inputControllerReader.Throttle != 0)
        {
            speed = inputControllerReader.Throttle;
        }       

        //float current_power = MathF.Min(speed , MaxSpeed) * maxEnginePower;
        float current_power = MathF.Min(Input.GetAxis("Vertical"), MaxSpeed) * maxEnginePower;
        //float steering_angle = maxSteeringAngle * inputControllerReader.Steering;
        float steering_angle = maxSteeringAngle * Input.GetAxis("Horizontal");

        bool isBraking = Input.GetKey(KeyCode.Z);
        //bool isBraking = inputControllerReader.Brake != 0;

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
            
            info.rightWheel.brakeTorque = isBraking ? maxBrakeForce : 0;
            info.leftWheel.brakeTorque = isBraking ? maxBrakeForce : 0;
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
        if (value)
        {
            EngineIsRunning = !EngineIsRunning; // ѕереключаем состо€ние двигател€
            Debug.Log(EngineIsRunning ? "ƒвигатель запущен!" : "ƒвигатель заглушен!");
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
