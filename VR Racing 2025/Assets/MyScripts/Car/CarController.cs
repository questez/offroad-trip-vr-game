using LogitechG29.Sample.Input;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using Bhaptics.SDK2;

public class CarController : MonoBehaviour
{
    [SerializeField] private AudioSource startEngineSound, stopEngineSound;
    [SerializeField] private AudioSource EngineIsRunningSound;
    [SerializeField] private AudioSource gearShifterSound;

    private bool InMud; // едет ли машина по грязи
    private bool InWater; // едет ли машина по лужам или находиться в воде

    private bool isStartingEngine = false; // запускается ли двигатель
    private bool isReverseGear; // включена ли задняя передача

    private bool EngineIsRunning = false; // запущен ли двигатель
    private bool AllWheelDriveMode = false; // включен ли полный привод
    public static bool CarIsBroken { get; private set; }

    private const float maxPitch = 2f;
    private const float minPitch = 1f;

    private float engineRPM; // виртуальные обороты двигателя
    private const float minRPM = 800f;
    private const float maxRPM = 7000f;
    private float rpmSmoothVelocity; // для сглаживания
    private char previous_shifter = 'N';
    private bool shifterJustChanged = false;    
    private float gearChangeCooldown = 0f;

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

    public static char current_shifter = 'N'; // текущая передача
    public static string wheel_drive_mode; // задний/передний привод    

    [SerializeField] private GameObject Body1;
    private Collider[] Body1Colliders;
    [SerializeField] private GameObject Body2;
    private Collider[] Body2Colliders;

    private DetailedTriggerChecker triggerChecker = new DetailedTriggerChecker();

    private void Start()
    {
        Body2Colliders = Body2.GetComponentsInChildren<Collider>();
        Body1Colliders = Body1.GetComponentsInChildren<Collider>();
        South_button_hold_timer = 0f;
        StartEngineScreen.SetActive(false);
        wheel_drive_mode = "Задний привод";
        StartCoroutine(OffInputDelay());
        CarIsBroken = false;
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
        if (!PauseScreenWork.isPaused && !OffInput && !CarIsBroken)
        {
            UpdateWheelState();
            UpdateHighRPMHapticIntensity();
        }                
    }

    private void Update()
    {        
        if (!PauseScreenWork.isPaused && !OffInput && !CarIsBroken && !Fuel.IsFuelEmpty)
        {
            OnEngine();            
        }
        //Debug.Log($"engineRPM: {engineRPM}");
        Fuel.UpdateFuelValue(AllWheelDriveMode, rb.linearVelocity.magnitude);
        if (Fuel.IsFuelEmpty)
        {
            OffEngine(true);
        }
    }

    private void UpdateEngineSound(float throttleInput)
    {
        if (!EngineIsRunning) return;

        // Если нейтраль — обороты растут от газа напрямую
        if (current_shifter == 'N')
        {
            float targetRPM = Mathf.Lerp(minRPM, maxRPM, Mathf.Abs(throttleInput));
            engineRPM = Mathf.SmoothDamp(engineRPM, targetRPM, ref rpmSmoothVelocity, 0.4f);
        }
        else
        {
            // На передаче — RPM зависит от скорости колес и газа
            float wheelRPM = 0f;
            int motorWheels = 0;

            foreach (var axle in axleInfos)
            {
                if (axle.isMotor)
                {
                    wheelRPM += Mathf.Abs(axle.leftWheel.rpm / 1.5f + axle.rightWheel.rpm / 1.5f) * 0.5f;
                    motorWheels++;
                }
            }

            if (motorWheels > 0) wheelRPM /= motorWheels;

            // добавляем эффект газа (Throttle повышает обороты)
            float targetRPM = Mathf.Lerp(minRPM, maxRPM, Mathf.Clamp01(Mathf.Abs(wheelRPM) / 500f)) + Mathf.Abs(throttleInput) * 1000f;
            if (shifterJustChanged)
            {
                // сбрасываем RPM до 2000 после переключения передачи
                engineRPM = Mathf.Lerp(engineRPM, 2000f, Time.deltaTime * 6f);

                gearChangeCooldown -= Time.deltaTime;
                if (gearChangeCooldown <= 0)
                {                    
                    shifterJustChanged = false; // вернуться к обычному росту оборотов
                }
            }
            else
            {
                engineRPM = Mathf.SmoothDamp(engineRPM, targetRPM, ref rpmSmoothVelocity, 0.4f);
            }
        }

        // Нормализуем pitch от RPM
        float normalizedRPM = Mathf.InverseLerp(minRPM, maxRPM, engineRPM);
        EngineIsRunningSound.pitch = Mathf.Lerp(minPitch, maxPitch, normalizedRPM);
        EngineIsRunningSound.pitch = Mathf.Clamp(EngineIsRunningSound.pitch, minPitch, maxPitch);
    }


    private void UpdateWheelState() // поведение колес и повороты рулем
    {         
        float speed = 0f;
        float brake = 0f;

        if (inputControllerReader.Throttle != 0)
        {
            speed = inputControllerReader.Throttle;
        }               
        if (inputControllerReader.Brake != 0)
        {
            brake = inputControllerReader.Brake;
        }               

        float current_throttle_power = speed * enginePower; // передача крутящего момента колесам        

        float steering_angle = maxSteeringAngle * inputControllerReader.Steering; // поворот

        float current_brake_power = brake * BrakeForce; // передача тормоза колесам

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
                //Debug.Log($"info.leftWheel.rpm {info.leftWheel.rpm}");
                //Debug.Log($"info.rightWheel.rpm {info.rightWheel.rpm}");

                if ((rb.linearVelocity.magnitude <= CurrentMaxSpeed / 3.6f) && EngineIsRunning)
                {
                    if (!isReverseGear)
                    {
                        info.rightWheel.motorTorque = current_throttle_power;
                        info.leftWheel.motorTorque = current_throttle_power;
                    }
                    else
                    {
                        info.rightWheel.motorTorque = -current_throttle_power;
                        info.leftWheel.motorTorque = -current_throttle_power;
                    }                    
                }                
                else
                {
                    info.rightWheel.motorTorque = 0;
                    info.leftWheel.motorTorque = 0;
                }                                   
            }            
            if (!CarIsBroken)
            {
                info.rightWheel.brakeTorque = current_brake_power;
                info.leftWheel.brakeTorque = current_brake_power;
            }
            else
            {
                info.rightWheel.brakeTorque = BrakeForce;
                info.leftWheel.brakeTorque = BrakeForce;
            }

            CheckWheelCollision(info);
        }
        UpdateEngineSound(speed);
    }

    private float CurrentMaxSpeed
    {
        get
        {
            char new_shifter = 'N';

            if (!(inputControllerReader.Shifter6 || inputControllerReader.Shifter7)) isReverseGear = false;

            if (inputControllerReader.Shifter1) new_shifter = '1';
            else if (inputControllerReader.Shifter2) new_shifter = '2';
            else if (inputControllerReader.Shifter3) new_shifter = '3';
            else if (inputControllerReader.Shifter4) new_shifter = '4';
            else if (inputControllerReader.Shifter5) new_shifter = '5';
            else if (inputControllerReader.Shifter6 || inputControllerReader.Shifter7)
            {
                isReverseGear = true;
                new_shifter = 'R';
            }

            // если передача изменилась
            if (new_shifter != previous_shifter)
            {
                gearShifterSound.Play();
                BhapticsLibrary.Play(eventId:BhapticsEvent.GEARSHIFT);
                shifterJustChanged = true;
                gearChangeCooldown = 0.5f; // полсекунды "просадки" оборотов
                previous_shifter = new_shifter;
            }

            current_shifter = new_shifter;

            switch (current_shifter)
            {
                case '1': return MaxSpeed1;
                case '2': return MaxSpeed2;
                case '3': return MaxSpeed3;
                case '4': return MaxSpeed4;
                case '5': return MaxSpeed5;
                case 'R': return MaxSpeedR;
                default: return 0;
            }
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
        //Debug.Log("Заехал в лужу!!"); ;
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
        //Debug.Log("Едет по грязи!!");
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

    private void OnEngine()
    {        
        if (inputControllerReader.SouthButton && !EngineIsRunning)
        {
            if (!isStartingEngine)
            {
                isStartingEngine = true;
                startEngineSound.Play();
                BhapticsLibrary.Play(eventId: BhapticsEvent.STARTENGINE, startMillis: 0, intensity: 1, duration: 1, angleX: 0, offsetY: 0);
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
            BhapticsLibrary.StopByEventId(eventId: BhapticsEvent.STARTENGINE);
        }            
    }

    private void OffEngine(bool value)
    {
        if (value && !PauseScreenWork.isPaused && EngineIsRunning)
        {
            EngineIsRunningSound.Stop();
            BhapticsLibrary.Play(eventId: BhapticsEvent.STOPENGINE);
            EngineIsRunning = false;
            stopEngineSound.Play();
            engineRPM = 0;
            Debug.Log("Двигатель заглушен!");
            StartCoroutine(OffInputDelay());
        }
    }
    private void IsCarCompletelyUnderWater(Collider other) // если машина полностью затонула в пруду, то больше ее завести не получиться
    {        
        if (InWater && triggerChecker.IsObjectsCompletelyInsideTrigger(Body1Colliders, other) && triggerChecker.IsObjectsCompletelyInsideTrigger(Body2Colliders, other) && !CarIsBroken)
        {
            if (EngineIsRunning)
            {
                OffEngine(true);
            }
            CarIsBroken = true;
            Debug.Log("Машина затонула!!!!!");
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
        if (other.CompareTag("Mud"))
        {
            InMud = true;
        }
        else if (other.CompareTag("Water"))
        {
            InWater = true;            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        IsCarCompletelyUnderWater(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mud"))
        {
            InMud = false;
        }
        else if (other.CompareTag("Water"))
        {
            InWater = false;
        }        
    }

    public static IEnumerator OffInputDelay() // сигнал для отключения ввода с руля и педалей, когда это нужно
    {
        OffInput = true;
        yield return new WaitForSecondsRealtime(3f);
        OffInput = false;
    }

    private void UpdateHighRPMHapticIntensity()
    {
        float targetIntensity = 0;

        if (engineRPM > 5000)
        {            
            targetIntensity = Mathf.InverseLerp(6000, maxRPM, engineRPM);
            targetIntensity = Mathf.Clamp01(targetIntensity);
            BhapticsLibrary.Play(eventId: BhapticsEvent.HIGHRPM, startMillis: 0, intensity: targetIntensity, duration: 1, angleX: 0, offsetY: 0);
        }
        else
        {
            BhapticsLibrary.StopByEventId(eventId: BhapticsEvent.HIGHRPM);
        }

        Debug.Log($"targetIntensity: {targetIntensity}\nengineRPM: {engineRPM}");        
    }
}
