using System.Collections;
using _2DOF;
using UnityEngine;

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    private ObjectTelemetryData telemetryDataData;
    private SendingData _sendingData;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rb;

    private Vector3 lastLinearVelocity;
    private Vector3 lastAngularVelocity;

    private float lastLinearAccel;
    private float lastAngularAccel;

    private const float maxPlatformAngle = 10f; // Максимальный угол наклона платформы 2DOF
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

            yield return new WaitForSeconds(WAIT_TIME);
        }
    }



    private void UpdatePlatformVelocity() // отправка данных о скорости на платформу
    {
        telemetryDataData.Velocity = rb.linearVelocity;

        //Debug.Log($"telemetryDataData.Velocity = {telemetryDataData.Velocity.magnitude * 3.6f}");
    }

    private void UpdatePlatformAngles()
    {
        // считаем линейное ускорение для Pitch
        float targetPitch = 0;        
        
        Vector3 currentLinearVelocity = rb.linearVelocity;
        Vector3 VectorLinearAcceleration = (currentLinearVelocity - lastLinearVelocity) / Time.deltaTime; 
        lastLinearVelocity = currentLinearVelocity;     
        
        float linearAcceleration = Vector3.Dot(VectorLinearAcceleration, vehicleTransform.forward); // учет направления вектора ускорения 
        linearAcceleration = Mathf.Lerp(lastLinearAccel, linearAcceleration, 0.01f);
        lastLinearAccel = linearAcceleration;


        if (linearAcceleration > 0.5f) // изменение угла при ускорении 
        {
            targetPitch = -Mathf.Clamp(linearAcceleration, 0f, maxPlatformAngle);
        }
        else if (linearAcceleration < -0.8f) // изменение угла при торможении
        {
            targetPitch = Mathf.Clamp(Mathf.Abs(linearAcceleration), 0f, maxPlatformAngle);
        }

        targetPitch = Mathf.Clamp(targetPitch, -maxPlatformAngle, maxPlatformAngle);
            
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, 0.01f);


        //---------------------------------------------------------------------------
        // считаем угловое ускорение для Roll [работает неправильно] (возможно нужно просто передавать угол наклона по Z)
        float targetRoll = 0;

        Vector3 currentAngularVelocity = rb.angularVelocity;
        Vector3 VectorAngularAcceleration = (currentAngularVelocity - lastAngularVelocity) / Time.deltaTime;
        lastAngularVelocity = currentAngularVelocity;

        float angularAcceleration = Vector3.Dot(VectorAngularAcceleration, vehicleTransform.right); // учет направления вектора ускорения 
        Debug.Log($"angularAcceleration = {angularAcceleration}");
        angularAcceleration = Mathf.Lerp(lastAngularAccel, angularAcceleration, 0.01f);
        lastAngularAccel = angularAcceleration;

        

        if (angularAcceleration > 0.5f) // изменение угла при ускорении 
        {
            targetRoll = -Mathf.Clamp(angularAcceleration, 0f, maxPlatformAngle);
        }
        else if (angularAcceleration < -0.5f) // изменение угла при торможении
        {
            targetRoll = Mathf.Clamp(Mathf.Abs(angularAcceleration), 0f, maxPlatformAngle);
        }

        targetRoll = Mathf.Clamp(targetRoll, -maxPlatformAngle, maxPlatformAngle);

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, 0.01f);

        Vector3 resultAngles = new Vector3(targetPitch, 0f, 0f); // конечный возврат углов для передачи данных в платформу

        telemetryDataData.Angles = resultAngles;
    }


    private float NormalizeAngle(float angle) // Нормализуем угол в диапазон -180 до 180
    {
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }
}