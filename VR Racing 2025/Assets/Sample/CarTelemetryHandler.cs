using System.Collections;
using _2DOF;
using UnityEngine;

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rb;

    private Vector3 _lastPosition;
    private Vector3 _lastVelocity;

    // Для 2DOF платформы
    private float _currentPitch; // Тангаж (ускорение/торможение)
    private float _currentRoll;  // Крен (повороты)
    private const float MAX_PLATFORM_ANGLE = 15f; // Максимальный угол наклона платформы

    private ObjectTelemetryData _telemetryDataData;
    private SendingData _sendingData;

    private void Awake()
    {
        _sendingData = new SendingData();
        _telemetryDataData = _sendingData.ObjectTelemetryData;
    }

    private void Start()
    {
        _lastPosition = vehicleTransform.position;
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
            if (_telemetryDataData == null)
            {
                yield return new WaitForSeconds(WAIT_TIME * 10f);
                continue;
            }
            
            //UpdateAngles();
            UpdateVelocity();
            CalculatePlatformForces();
            //Debug.Log(_telemetryDataData.ToString());

            yield return new WaitForSeconds(WAIT_TIME);
        }
    }

    private void UpdateVelocity()
    {
        Vector3 currentPos = vehicleTransform.position;
        Vector3 calculatedVelocity = (currentPos - _lastPosition) / Time.deltaTime;
        _lastPosition = currentPos;

        // Используем либо физическую, либо расчетную скорость
        _telemetryDataData.Velocity = (rb.linearVelocity != Vector3.zero) ? rb.linearVelocity : calculatedVelocity;

        //Debug.Log($"_telemetryDataData.Velocity в км/ч: {_telemetryDataData.Velocity.magnitude * 3.6f}");
        //Debug.Log($"calculatedVelocity: {calculatedVelocity}");
    }

    //private void UpdateVelocity()
    //{
    //    _telemetryDataData.Velocity = rb.linearVelocity;
    //    Debug.Log(_telemetryDataData.Velocity);
    //}

    private void UpdateAngles()
    {
        var euler = vehicleTransform.eulerAngles;

        euler.x = Mathf.Approximately(euler.x, 180) ? 0 : euler.x;
        euler.z = Mathf.Approximately(euler.z, 180) ? 0 : euler.z;
        euler.y = Mathf.Approximately(euler.y, 180) ? 0 : euler.y;

        euler.x = euler.x > 180 ? euler.x - 360 : euler.x;
        euler.z = euler.z > 180 ? euler.z - 360 : euler.z;
        euler.y = euler.y > 180 ? euler.y - 360 : euler.y;

        _telemetryDataData.Angles = euler;
    }
    private void CalculatePlatformForces()
    {
        Vector3 currentVelocity = _telemetryDataData.Velocity;
        Vector3 acceleration = (currentVelocity - _lastVelocity) / Time.deltaTime;
        _lastVelocity = currentVelocity;

        Vector3 forwardDirection = vehicleTransform.forward;
        float longitudinalAcceleration = Vector3.Dot(acceleration, forwardDirection);

        Vector3 rightDirection = vehicleTransform.right;
        float lateralAcceleration = Vector3.Dot(acceleration, rightDirection);

        // Расчет углов для 2DOF платформы
        CalculatePlatformAngles(longitudinalAcceleration, lateralAcceleration);        
    }

    private void CalculatePlatformAngles(float longitudinalAccel, float lateralAccel)
    {
        // Ускорение/торможение -> тангаж (pitch)
        float targetPitch = 0f;

        if (longitudinalAccel > 0.5f) // Сильное ускорение
        {
            targetPitch = -Mathf.Clamp(longitudinalAccel * 5f, 0f, MAX_PLATFORM_ANGLE);
        }
        else if (longitudinalAccel < -0.8f) // Сильное торможение
        {
            targetPitch = Mathf.Clamp(Mathf.Abs(longitudinalAccel) * 4f, 0f, MAX_PLATFORM_ANGLE);
        }

        // Повороты -> крен (roll)
        float targetRoll = 0f;
        if (Mathf.Abs(lateralAccel) > 0.3f)
        {
            targetRoll = -Mathf.Clamp(lateralAccel * 8f, -MAX_PLATFORM_ANGLE, MAX_PLATFORM_ANGLE);
        }

        // Плавное изменение углов
        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, 0.3f);
        _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, 0.4f);

        //Debug.Log($"🎯 Платформа: Pitch={_currentPitch:F1}°, Roll={_currentRoll:F1}° | " + $"Ускорение: {longitudinalAccel:F2} m/s², Боковое: {lateralAccel:F2} m/s²");


        _telemetryDataData.Angles = new Vector3(_currentPitch, 0, _currentRoll); // отправка данных на платформу 2DOF

    }
}