using LogitechG29.Sample.Input;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{
    [SerializeField] private AudioSource startEngineSound;
    private bool isStartingEngine = false;
    [SerializeField] private AudioSource stopEngineSound;
    [SerializeField] private AudioSource EngineIsRunningSound;
    private const float maxPitch = 2.5f;
    private const float minPitch = 1f;

    [SerializeField] private GameObject StartEngineScreen;
    [SerializeField] private Slider slider3;
    private float South_button_hold_timer;

    [SerializeField] private Rigidbody rb;

    public static bool OffInput { get; private set; }
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private Transform steeringWheelTransform;

    [SerializeField] private List<AxleInfo> axleInfos; // информация о каждой отдельной оси автомобиля

    [SerializeField] private float enginePower; // максимальный крутящий момент, который двигатель может приложить к колесу (мощность двигателя)
    [SerializeField] private float maxSteeringAngle; // максимальный угол поворота, который может иметь колесо
    [SerializeField] private float BrakeForce; // тормозная сила
    [SerializeField] private float steeringAngleMultiplier;

    private float MaxSpeedR = 13f; // максимально допустимая скорость машины на задней передаче
    private float MaxSpeed1 = 20f; // максимально допустимая скорость машины на первой передаче
    private float MaxSpeed2 = 35f; // максимально допустимая скорость машины на второй передаче
    private float MaxSpeed3 = 50f; // максимально допустимая скорость машины на третьей передаче
    private float MaxSpeed4 = 75f; // максимально допустимая скорость машины на четвертой передаче
    private float MaxSpeed5 = 100f; // максимально допустимая скорость машины на пятой передаче    

    public static char current_shifter; // текущая передача
    public static string wheel_drive_mode; // задний/передний привод

    private bool isReverseGear; // включена ли задняя передача
    
    private bool EngineIsRunning; // запущен ли двигатель
    private bool AllWheelDriveMode; // включен ли полный привод

    private bool InMud; // едет ли машина по грязи
    private bool InWater; // едет ли машина по лужам

    private void Start()
    {
        current_shifter = 'N';
        EngineIsRunning = false;
        AllWheelDriveMode = false;
        South_button_hold_timer = 0f;
        StartEngineScreen.SetActive(false);
        wheel_drive_mode = "Задний привод";
        StartCoroutine(OffInputDelay());
    }

    private void OnEnable()
    {
        inputControllerReader.OnWestButtonCallback += OnAllWheelDriveMode;
        inputControllerReader.OnEastButtonCallback += OffEngine;
        
    }

    private void OnDisable()
    {        
        inputControllerReader.OnWestButtonCallback -= OnAllWheelDriveMode;
        inputControllerReader.OnEastButtonCallback -= OffEngine;
    }

    private void FixedUpdate()
    {
        if (!PauseScreenWork.isPaused && !OffInput)
        {
            UpdateWheelState();
        }                
    }

    private void Update()
    {        
        if (!PauseScreenWork.isPaused && !OffInput)
        {
            OnEngine();
        }
    }

    private void ChangePitchSound(float value, char shifter)
    {
        if (shifter == 'N')
        {
            if (value > 0)
            {
                if (EngineIsRunningSound.pitch < maxPitch)
                {
                    EngineIsRunningSound.pitch += value * 0.01f;
                }
                else
                {
                    EngineIsRunningSound.pitch = maxPitch;
                }
            }
            else
            {
                EngineIsRunningSound.pitch = Mathf.Lerp(EngineIsRunningSound.pitch, minPitch, 0.03f);
            }
        }
        else
        {
            if (value > 0)
            {
                if (EngineIsRunningSound.pitch < maxPitch)
                {
                    EngineIsRunningSound.pitch += value * 0.01f;
                }
                else
                {
                    EngineIsRunningSound.pitch = maxPitch;
                }
            }
            else
            {
                EngineIsRunningSound.pitch = Mathf.Lerp(EngineIsRunningSound.pitch, minPitch, 0.03f);
            }
        }
    }

    private void UpdateWheelState() // поведение колес и повороты рулем
    {         
        float speed = 0f;

        if (inputControllerReader.Throttle != 0)
        {
            speed = inputControllerReader.Throttle;
        }               

        float current_power = speed * enginePower; // передача крутящего момента колесам (педали)        
        //float current_power = Input.GetAxis("Vertical") * enginePower; // передача крутящего момента колесам (клавиатура)

        float steering_angle = maxSteeringAngle * inputControllerReader.Steering; // поворот (руль)
        //float steering_angle = maxSteeringAngle * Input.GetAxis("Horizontal"); // поворот (клавиатура)
        
        bool isBraking = inputControllerReader.Brake != 0; // тормоз (педали)
        //bool isBraking = Input.GetKey(KeyCode.Z); // тормоз (клавиатура)


        foreach (var info in axleInfos)
        {            
            if (info.isSteering)
            {
                steeringWheelTransform.localRotation = Quaternion.Euler(24f, 0, -(steering_angle * steeringAngleMultiplier)); // поворот руля при повороте колес
                info.rightWheel.steerAngle = steering_angle;
                info.leftWheel.steerAngle = steering_angle;
                if (AllWheelDriveMode)
                {
                    info.isMotor = true;
                }
                else
                {
                    info.rightWheel.motorTorque = 0;
                    info.leftWheel.motorTorque = 0;
                    info.isMotor = false;
                }
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
                    if (current_shifter == 'N')
                    {
                        ChangePitchSound(speed, current_shifter);
                    }
                    else
                    {
                        ChangePitchSound(Mathf.Max(info.rightWheel.motorTorque, info.leftWheel.motorTorque) * 0.0001f, current_shifter);
                    }
                }                
                else
                {
                    info.rightWheel.motorTorque = 0;
                    info.leftWheel.motorTorque = 0;
                }
                                   
            }
            info.rightWheel.brakeTorque = isBraking ? BrakeForce * inputControllerReader.Brake : 0;
            info.leftWheel.brakeTorque = isBraking ? BrakeForce * inputControllerReader.Brake: 0;
            //info.rightWheel.brakeTorque = isBraking ? BrakeForce : 0;
            //info.leftWheel.brakeTorque = isBraking ? BrakeForce : 0;

            CheckWheelCollision(info);
        }
    }

    private void CheckWheelCollision(AxleInfo info)
    {      
        bool leftGrounded = info.leftWheel.GetGroundHit(out WheelHit hitLeft);
        bool rightGrounded = info.rightWheel.GetGroundHit(out WheelHit hitRight);
        
        if (leftGrounded && InWater || rightGrounded && InWater)
        {
            ApplyValuesForWaterSurface(info);
        }

        else if (leftGrounded && InMud || rightGrounded && InMud)
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
        if (rb.linearVelocity.magnitude * 3.6f > 30f)
        {
            info.leftWheel.wheelDampingRate = 1500f;
            info.rightWheel.wheelDampingRate = 1500f;
        }
        else
        {
            info.leftWheel.wheelDampingRate = 20f;
            info.rightWheel.wheelDampingRate = 20f;
        }        

        // пробуксовка для forwardFriction:
        WheelFrictionCurve leftForwardFriction = info.leftWheel.forwardFriction;
        WheelFrictionCurve rightForwardFriction = info.rightWheel.forwardFriction;

        leftForwardFriction.extremumSlip = 600f;
        rightForwardFriction.extremumSlip = 600f;

        info.leftWheel.forwardFriction = leftForwardFriction;
        info.rightWheel.forwardFriction = rightForwardFriction;

        // пробуксовка для sidewaysFriction:
        WheelFrictionCurve leftSidewaysFriction = info.leftWheel.sidewaysFriction;
        WheelFrictionCurve rightSidewaysFriction = info.rightWheel.sidewaysFriction;

        leftSidewaysFriction.extremumSlip = 2.2f;
        rightSidewaysFriction.extremumSlip = 2.2f;

        info.leftWheel.sidewaysFriction = leftSidewaysFriction;
        info.rightWheel.sidewaysFriction = rightSidewaysFriction;
        Debug.Log("Заехал в лужу!!"); ;
    }
    private void ApplyValuesForMuddySurface(AxleInfo info)
    {
        info.leftWheel.wheelDampingRate = 18f;
        info.rightWheel.wheelDampingRate = 18f;

        // пробуксовка для forwardFriction:
        WheelFrictionCurve leftForwardFriction = info.leftWheel.forwardFriction;
        WheelFrictionCurve rightForwardFriction = info.rightWheel.forwardFriction;

        leftForwardFriction.extremumSlip = 22f;
        rightForwardFriction.extremumSlip = 22f;

        info.leftWheel.forwardFriction = leftForwardFriction;
        info.rightWheel.forwardFriction = rightForwardFriction;

        // пробуксовка для sidewaysFriction:
        WheelFrictionCurve leftSidewaysFriction = info.leftWheel.sidewaysFriction;
        WheelFrictionCurve rightSidewaysFriction = info.rightWheel.sidewaysFriction;

        leftSidewaysFriction.extremumSlip = 0.85f;
        rightSidewaysFriction.extremumSlip = 0.85f;

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

        leftSidewaysFriction.extremumSlip = 0.55f;
        rightSidewaysFriction.extremumSlip = 0.55f;

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

    private void OnEngine()
    {        
        if (inputControllerReader.SouthButton && !EngineIsRunning)
        {
            if (!isStartingEngine)
            {
                isStartingEngine = true;
                startEngineSound.Play();
            }
            StartEngineScreen.SetActive(true);
            South_button_hold_timer += Time.deltaTime;
            slider3.value = South_button_hold_timer;
            if (slider3.value == slider3.maxValue)
            {
                StartEngineScreen.SetActive(false);
                South_button_hold_timer = 0;
                EngineIsRunning = true;
                EngineIsRunningSound.Play();
                Debug.Log("Двигатель запущен!");
                return;
            }
        }
        else
        {
            isStartingEngine = false;
            StartEngineScreen.SetActive(false);
            South_button_hold_timer = 0;
            startEngineSound.Stop();
        }            
    }

    private void OffEngine(bool value)
    {
        if (value && !PauseScreenWork.isPaused)
        {
            EngineIsRunningSound.Stop();
            EngineIsRunning = false;
            stopEngineSound.Play();
            Debug.Log("Двигатель заглушен!");
            StartCoroutine(OffInputDelay());
        }
    }

    private void OnAllWheelDriveMode(bool value)
    {
        if (value)
        {
            AllWheelDriveMode = !AllWheelDriveMode; // Переключаем полный привод
            wheel_drive_mode = AllWheelDriveMode ? "Полный привод" : "Задний привод";            
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

    public static IEnumerator OffInputDelay() // сигнал для отключения ввода с руля и педалей, когда это нужно
    {
        OffInput = true;
        yield return new WaitForSecondsRealtime(3f);
        OffInput = false;
    }
}
