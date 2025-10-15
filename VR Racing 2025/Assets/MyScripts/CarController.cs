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
    

    [NonSerialized] public static char current_shifter;

    private bool isReverseGear; // включена ли задняя передача
    
    private bool EngineIsRunning; // запущен ли двигатель

    private bool InMud; // едет ли машина по грязи

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
        
        if (leftGrounded && hitLeft.collider.gameObject.layer == LayerMask.NameToLayer("Water") ||
                rightGrounded && hitRight.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            if (rb.linearVelocity.magnitude * 3.6f > 15f)
            {
                info.leftWheel.wheelDampingRate = 55f;
                info.rightWheel.wheelDampingRate = 55f;
            }
            else
            {
                info.leftWheel.wheelDampingRate = 1f;
                info.rightWheel.wheelDampingRate = 1f;
            }
            var leftFriction = info.leftWheel.forwardFriction;
            var rightFriction = info.rightWheel.forwardFriction;

            leftFriction.extremumSlip = 25f;
            rightFriction.extremumSlip = 25f;

            info.leftWheel.forwardFriction = leftFriction;
            info.rightWheel.forwardFriction = rightFriction;
            Debug.Log("Заехал в лужу!!");
        }

        if (leftGrounded && InMud || rightGrounded && InMud)
        {
            var leftFriction = info.leftWheel.forwardFriction;
            var rightFriction = info.rightWheel.forwardFriction;

            leftFriction.extremumSlip = 7f;
            rightFriction.extremumSlip = 7f;

            info.leftWheel.forwardFriction = leftFriction;
            info.rightWheel.forwardFriction = rightFriction;
            Debug.Log("Едет по грязи!!");
        }

        else
        {
            info.leftWheel.wheelDampingRate = 1f;
            info.rightWheel.wheelDampingRate = 1f;

            var leftFriction = info.leftWheel.forwardFriction;
            var rightFriction = info.rightWheel.forwardFriction;

            leftFriction.extremumSlip = 0.4f;
            rightFriction.extremumSlip = 0.4f;

            info.leftWheel.forwardFriction = leftFriction;
            info.rightWheel.forwardFriction = rightFriction;
        }
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mud")) InMud = false;
    }
}
