using _2DOF;
using LogitechG29.Sample.Input;
using System.Collections;
using TMPro;
using UnityEngine;

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    private ObjectTelemetryData telemetryDataData;
    private SendingData _sendingData;

    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private TextMeshProUGUI platformDebug;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rb;    

    private Vector3 lastLinearVelocity;
    private Vector3 lastAngularVelocity;
    private float lastLinearAccel;
    private float lastAngularAccel;

    private const float maxPlatformAngle = 15f; // Максимальный угол наклона платформы 2DOF    
    private float currentPitch = 0f;
    private float currentRoll = 0f;

    private void Awake()
    {
        _sendingData = new SendingData();
        telemetryDataData = _sendingData.ObjectTelemetryData;
    }


    public void OnEnable()
    {
        StartCoroutine(TelemetryHandler());
        _sendingData.SendingStart();
    }

    public void OnDisable()
    {
        StopCoroutine(TelemetryHandler());
        _sendingData.SendingStop();
    }

    private IEnumerator TelemetryHandler()
    {
        while (true)
        {
            if (telemetryDataData == null)
            {
                yield return new WaitForSeconds(WAIT_TIME * 10f);
                continue;
            }
            UpdatePlatformAngles();
            UpdatePlatformVelocity();


            //Debug.Log(telemetryDataData.ToString());
            platformDebug.text = "telemetryDataData: " + telemetryDataData.ToString();

            yield return new WaitForSeconds(WAIT_TIME);
        }
    }
    private float NormalizeAngle(float angle) // нормализуем угол в диапазон -180 до 180
    {
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }

    private void UpdatePlatformVelocity() // отправка данных о скорости на платформу
    {
        telemetryDataData.Velocity = new Vector3(rb.linearVelocity.x, rb.angularVelocity.y, rb.linearVelocity.z);
        //Debug.Log($" = {}");
    }

    private void UpdatePlatformAngles()
    {
        // считаем линейное ускорение для Pitch и сам Pitch
        float targetPitch = 0;        
        
        Vector3 currentLinearVelocity = rb.linearVelocity;        
        Vector3 VectorLinearAcceleration = (currentLinearVelocity - lastLinearVelocity) / Time.deltaTime; // вектор линейного ускорения
        lastLinearVelocity = currentLinearVelocity;     
        
        float linearAcceleration = Vector3.Dot(VectorLinearAcceleration, vehicleTransform.forward); // числовое значение ускорения с учетом направления вектора 
        linearAcceleration = Mathf.Lerp(lastLinearAccel, linearAcceleration, 0.01f);
        lastLinearAccel = linearAcceleration;

        if (linearAcceleration > 0.5f) // изменение угла при ускорении 
        {
            targetPitch = -Mathf.Clamp(linearAcceleration * 1.5f, 0f, maxPlatformAngle);
        }
        else if (linearAcceleration < -0.5f) // изменение угла при торможении
        {
            targetPitch = Mathf.Clamp(Mathf.Abs(linearAcceleration) * 1.5f, 0f, maxPlatformAngle);
        }

        targetPitch += NormalizeAngle(vehicleTransform.eulerAngles.x); // учет наклона поверхности
        targetPitch = Mathf.Clamp(targetPitch, -maxPlatformAngle, maxPlatformAngle);
            
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, 0.01f);

        //---------------------------------------------------------------------------
        // считаем угловое ускорение для Roll и сам Roll
        float targetRoll = 0;

        Vector3 currentAngularVelocity = rb.angularVelocity;
        Vector3 VectorAngularAcceleration = (currentAngularVelocity - lastAngularVelocity) / Time.deltaTime; // вектор линейного ускорения
        lastAngularVelocity = currentAngularVelocity;

        float angularAcceleration = Vector3.Dot(VectorAngularAcceleration, vehicleTransform.up); // числовое значение ускорения с учетом направления вектора 
        angularAcceleration = Mathf.Lerp(lastAngularAccel, angularAcceleration, 0.01f);
        lastAngularAccel = angularAcceleration;

        if (angularAcceleration > 0.05f) // изменение угла при ускорении
        {
            targetRoll = -Mathf.Clamp(angularAcceleration * 35f, 0f, maxPlatformAngle);
        }
        else if (angularAcceleration < -0.05f) // изменение угла при торможении
        {
            targetRoll = Mathf.Clamp(Mathf.Abs(angularAcceleration) * 35f, 0f, maxPlatformAngle);
        }

        targetRoll += NormalizeAngle(vehicleTransform.eulerAngles.z);
        targetRoll = Mathf.Clamp(targetRoll, -maxPlatformAngle, maxPlatformAngle);

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, 0.01f);


        Vector3 resultAngles = new Vector3(targetPitch, targetRoll, 0f); // конечный возврат углов для передачи данных в платформу
        telemetryDataData.Angles = resultAngles;
    }   
    
}