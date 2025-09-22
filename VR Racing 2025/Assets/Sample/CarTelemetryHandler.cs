using System.Collections;
using _2DOF;
using UnityEngine;

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rb;

    private Vector3 lastVelocity;
    
    private const float MAX_PLATFORM_ANGLE = 10f; // Максимальный угол наклона платформы

    private ObjectTelemetryData telemetryDataData;
    private SendingData _sendingData;

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

            
            UpdatePlatformVelocity();
            
            //Debug.Log(telemetryDataData.ToString());

            yield return new WaitForSeconds(WAIT_TIME);
        }
    }

    private void FixedUpdate()
    {
        UpdatePlatformAngles();
    }

    private void UpdatePlatformVelocity() // отправка данных о скорости на платформу
    {
        telemetryDataData.Velocity = rb.linearVelocity;

        //Debug.Log($"telemetryDataData.Velocity = {telemetryDataData.Velocity.magnitude * 3.6f}");
    }

    private void UpdatePlatformAngles()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 linearAcceleration = (currentVelocity - lastVelocity) / Time.deltaTime; // считаем линейное ускорение для Pitch
        lastVelocity = currentVelocity;

        Debug.Log($"linearAcceleration = {linearAcceleration.magnitude}");
    }


    private float NormalizeAngle(float angle) // Нормализуем угол в диапазон -180 до 180
    {
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }
}