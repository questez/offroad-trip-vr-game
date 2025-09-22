using LogitechG29.Sample.Input;
using System;
using System.Collections.Generic;
using UnityEngine;



public class CarController : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private Transform steeringWheelTransform;

    [SerializeField] private List<AxleInfo> axleInfos; // информация о каждой отдельной оси автомобиля

    [SerializeField] private float enginePower; // максимальный крутящий момент, который двигатель может приложить к колесу (мощность двигателя)

    [SerializeField] private float maxSteeringAngle; // максимальный угол поворота, который может иметь колесо
    [SerializeField] private float BrakeForce; // тормозная сила

    private Vector3 _lastPosition;

    private float MaxSpeed1 = 15f; // максимально допустимая скорость машины на первой передаче
    private float MaxSpeed2 = 35f; // максимально допустимая скорость машины на второй передаче
    private float MaxSpeed3 = 50f; // максимально допустимая скорость машины на третьей передаче
    private float MaxSpeed4 = 75f; // максимально допустимая скорость машины на четвертой передаче
    
    bool EngineIsRunning; // запущен ли двигатель

    private void Start()
    {
        _lastPosition = transform.position;
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
    }

    private void UpdateWheelState() // поведение колес и повороты рулем
    {
        Vector3 currentPos = transform.position;
        Vector3 calculatedVelocity = (currentPos - _lastPosition) / Time.deltaTime;
        _lastPosition = currentPos;

        //Debug.Log($"telemetryDataData.Velocity = {calculatedVelocity.magnitude * 3.6f}");
    

    float speed = 0f;

        if (inputControllerReader.Throttle != 0)
        {
            speed = inputControllerReader.Throttle;
        }

        //float current_power = speed * enginePower; // передача крутящего момента колесам (педали)        
        float current_power = Input.GetAxis("Vertical") * enginePower; // передача крутящего момента колесам (клавиатура)

        //float steering_angle = maxSteeringAngle * inputControllerReader.Steering; // поворот (руль)
        float steering_angle = maxSteeringAngle * Input.GetAxis("Horizontal"); // поворот (клавиатура)

        //bool isBraking = inputControllerReader.Brake != 0; // тормоз (педали)
        bool isBraking = Input.GetKey(KeyCode.Z); // тормоз (клавиатура)


        foreach (var info in axleInfos)
        {
            if (info.isSteering)
            {
                steeringWheelTransform.localRotation = Quaternion.Euler(24f, 0, -steering_angle * 3.5f); // поворот руля при повороте колес
                info.rightWheel.steerAngle = steering_angle;
                info.leftWheel.steerAngle = steering_angle;
            }
            if (info.isMotor)
            {                
                if (calculatedVelocity.magnitude * 3.6f <= CurrentMaxSpeed)
                {
                    info.rightWheel.motorTorque = current_power;
                    info.leftWheel.motorTorque = current_power;
                }
                else
                {
                    info.rightWheel.motorTorque = 0;
                    info.leftWheel.motorTorque = 0;
                }                
            }
            
            info.rightWheel.brakeTorque = isBraking ? BrakeForce : 0;
            info.leftWheel.brakeTorque = isBraking ? BrakeForce : 0;
        }
    }

    private float CurrentMaxSpeed // максимально допустимая скорость машины на текущей передаче
    {
        get
        { 
            if (inputControllerReader.Shifter1)
            {
                //Debug.Log("Первая передача!");
                return MaxSpeed1;
            }
            else if (inputControllerReader.Shifter2)
            {
                //Debug.Log("Вторая передача!");
                return MaxSpeed2;
            }
            else if (inputControllerReader.Shifter3)
            {
                //Debug.Log("Третья передача!");
                return MaxSpeed3;
            }
            else if (inputControllerReader.Shifter4)
            {
                //Debug.Log("Четвертая передача!");
                return MaxSpeed4;
            }
            //Debug.Log("Нейтральная передача!");
            return 0;                           
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

    private void HandleOnSouthCallback(bool value)
    {
        if (value)
        {
            EngineIsRunning = !EngineIsRunning; // Переключаем состояние двигателя
            Debug.Log(EngineIsRunning ? "Двигатель запущен!" : "Двигатель заглушен!");
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
