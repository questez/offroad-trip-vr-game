using LogitechG29.Sample.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private Transform steeringWheelTransform;    

    [SerializeField] private List<AxleInfo> axleInfos; // информация о каждой отдельной оси автомобиля

    [SerializeField] private float enginePower; // максимальный крутящий момент, который двигатель может приложить к колесу (мощность двигателя)

    [SerializeField] private float maxSteeringAngle; // максимальный угол поворота, который может иметь колесо
    [SerializeField] private float BrakeForce; // тормозная сила

    private float MaxSpeedR = 13f; // максимально допустимая скорость машины на задней передаче
    private float MaxSpeed1 = 20f; // максимально допустимая скорость машины на первой передаче
    private float MaxSpeed2 = 35f; // максимально допустимая скорость машины на второй передаче
    private float MaxSpeed3 = 50f; // максимально допустимая скорость машины на третьей передаче
    private float MaxSpeed4 = 75f; // максимально допустимая скорость машины на четвертой передаче
    private float MaxSpeed5 = 100f; // максимально допустимая скорость машины на пятой передаче      
    

    public static char current_shifter; // текущая передача

    private bool isReverseGear; // включена ли задняя передача
    
    private bool EngineIsRunning; // запущен ли двигатель

    private bool InMud; // едет ли машина по грязи
    private bool InWater; // едет ли машина по лужам

    private void Start()
    {
        current_shifter = 'N';
        EngineIsRunning = false;
    }

    private void OnEnable()
    {
        inputControllerReader.OnSouthButtonCallback += HandleOnSouthCallback;
    }

    private void OnDisable()
    {
        inputControllerReader.OnSouthButtonCallback -= HandleOnSouthCallback;
    }

    private void FixedUpdate()
    {     
        UpdateWheelState();
    }

    private void UpdateWheelState() // поведение колес и повороты рулем
    {         
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
            CheckWheelCollision(info);
            if (info.isSteering)
            {
                steeringWheelTransform.localRotation = Quaternion.Euler(24f, 0, -steering_angle * 3.5f); // поворот руля при повороте колес
                info.rightWheel.steerAngle = steering_angle;
                info.leftWheel.steerAngle = steering_angle;
            }
            if (info.isMotor)
            {                
                if ((rb.linearVelocity.magnitude <= CurrentMaxSpeed / 3.6f) && EngineIsRunning)
                {
                    if (!isReverseGear)
                    {
                        info.rightWheel.motorTorque = current_power;
                        info.leftWheel.motorTorque = current_power;
                    }
                    else
                    {
                        info.rightWheel.motorTorque = -current_power;
                        info.leftWheel.motorTorque = -current_power;
                    }
                }                
                else
                {
                    info.rightWheel.motorTorque = 0;
                    info.leftWheel.motorTorque = 0;
                }                
            }

            //info.rightWheel.brakeTorque = isBraking ? BrakeForce * inputControllerReader.Brake : 0;
            //info.leftWheel.brakeTorque = isBraking ? BrakeForce * inputControllerReader.Brake: 0;
            info.rightWheel.brakeTorque = isBraking ? BrakeForce : 0;
            info.leftWheel.brakeTorque = isBraking ? BrakeForce : 0;            
        }
    }

    private void CheckWheelCollision(AxleInfo info)
    {
        WheelHit hitLeft, hitRight;        
        bool leftGrounded = info.leftWheel.GetGroundHit(out hitLeft);
        bool rightGrounded = info.rightWheel.GetGroundHit(out hitRight);
        
        if (leftGrounded && InWater || rightGrounded && InWater)
        {
            ApplyValuesForWaterSurface(info);
        }

        if (leftGrounded && InMud || rightGrounded && InMud)
        {
            ApplyValuesForMuddySurface(info);
        }

        else
        {
            ApplyValuesForDefaultSurface(info);
        }
    }

    private void ApplyValuesForWaterSurface(AxleInfo info)
    {
        if (rb.linearVelocity.magnitude * 3.6f > 15f)
        {
            info.leftWheel.wheelDampingRate = 71f;
            info.rightWheel.wheelDampingRate = 71f;
        }
        else
        {
            info.leftWheel.wheelDampingRate = 1f;
            info.rightWheel.wheelDampingRate = 1f;
        }

        // пробуксовка для forwardFriction:
        WheelFrictionCurve leftForwardFriction = info.leftWheel.forwardFriction;
        WheelFrictionCurve rightForwardFriction = info.rightWheel.forwardFriction;

        leftForwardFriction.extremumSlip = 30f;
        rightForwardFriction.extremumSlip = 30f;

        info.leftWheel.forwardFriction = leftForwardFriction;
        info.rightWheel.forwardFriction = rightForwardFriction;

        // пробуксовка для sidewaysFriction:
        WheelFrictionCurve leftSidewaysFriction = info.leftWheel.sidewaysFriction;
        WheelFrictionCurve rightSidewaysFriction = info.rightWheel.sidewaysFriction;

        leftSidewaysFriction.extremumSlip = 0.6f;
        rightSidewaysFriction.extremumSlip = 0.6f;

        info.leftWheel.sidewaysFriction = leftSidewaysFriction;
        info.rightWheel.sidewaysFriction = rightSidewaysFriction;
        Debug.Log("Заехал в лужу!!"); ;
    }
    private void ApplyValuesForMuddySurface(AxleInfo info)
    {
        // пробуксовка для forwardFriction:
        WheelFrictionCurve leftForwardFriction = info.leftWheel.forwardFriction;
        WheelFrictionCurve rightForwardFriction = info.rightWheel.forwardFriction;

        leftForwardFriction.extremumSlip = 11f;
        rightForwardFriction.extremumSlip = 11f;

        info.leftWheel.forwardFriction = leftForwardFriction;
        info.rightWheel.forwardFriction = rightForwardFriction;

        // пробуксовка для sidewaysFriction:
        WheelFrictionCurve leftSidewaysFriction = info.leftWheel.sidewaysFriction;
        WheelFrictionCurve rightSidewaysFriction = info.rightWheel.sidewaysFriction;

        leftSidewaysFriction.extremumSlip = 0.5f;
        rightSidewaysFriction.extremumSlip = 0.5f;

        info.leftWheel.sidewaysFriction = leftSidewaysFriction;
        info.rightWheel.sidewaysFriction = rightSidewaysFriction;
        Debug.Log("Едет по грязи!!");
    }

    private void ApplyValuesForDefaultSurface(AxleInfo info)
    {
        info.leftWheel.wheelDampingRate = 1f;
        info.rightWheel.wheelDampingRate = 1f;

        // пробуксовка для forwardFriction:
        WheelFrictionCurve leftForwardFriction = info.leftWheel.forwardFriction;
        WheelFrictionCurve rightForwardFriction = info.rightWheel.forwardFriction;

        leftForwardFriction.extremumSlip = 0.4f;
        rightForwardFriction.extremumSlip = 0.4f;

        info.leftWheel.forwardFriction = leftForwardFriction;
        info.rightWheel.forwardFriction = rightForwardFriction;

        // пробуксовка для sidewaysFriction:
        WheelFrictionCurve leftSidewaysFriction = info.leftWheel.sidewaysFriction;
        WheelFrictionCurve rightSidewaysFriction = info.rightWheel.sidewaysFriction;

        leftSidewaysFriction.extremumSlip = 0.2f;
        rightSidewaysFriction.extremumSlip = 0.2f;

        info.leftWheel.sidewaysFriction = leftSidewaysFriction;
        info.rightWheel.sidewaysFriction = rightSidewaysFriction;
    }

    private float CurrentMaxSpeed // максимально допустимая скорость машины на текущей передаче
    {
        get
        { 
            if (!(inputControllerReader.Shifter6 || inputControllerReader.Shifter7)) isReverseGear = false;

            if (inputControllerReader.Shifter1)
            {
                current_shifter = '1';
                return MaxSpeed1;
            }
            else if (inputControllerReader.Shifter2)
            {
                current_shifter = '2';
                return MaxSpeed2;
            }
            else if (inputControllerReader.Shifter3)
            {
                current_shifter = '3';
                return MaxSpeed3;
            }
            else if (inputControllerReader.Shifter4)
            {
                current_shifter = '4';
                return MaxSpeed4;
            }
            else if (inputControllerReader.Shifter5)
            {
                current_shifter = '5';
                return MaxSpeed5;
            }
            else if (inputControllerReader.Shifter6 || inputControllerReader.Shifter7)
            {
                isReverseGear = true;
                current_shifter = 'R';
                return MaxSpeedR;
            }
            current_shifter = 'N';
            return 0;                           
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mud")) InMud = true;
        else if (other.CompareTag("Water")) InWater = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mud")) InMud = false;
        else if (other.CompareTag("Water")) InWater = false;
    }
}
