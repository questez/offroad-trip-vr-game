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
    private const float MAX_PLATFORM_ANGLE = 10f; // Максимальный угол наклона платформы

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

    private float NormalizeAngle(float angle)
    {
        // Нормализуем угол в диапазон -180 до 180
        angle = angle > 180 ? angle - 360 : angle;
        return Mathf.Clamp(angle, -45f, 45f); // Ограничиваем максимальный угол
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
        Vector3 localEuler = vehicleTransform.localEulerAngles;
        float carPitch = NormalizeAngle(localEuler.x);
        float carRoll = NormalizeAngle(localEuler.z);


        // Ускорение/торможение -> тангаж (pitch)
        float accelerationPitch = 0f;

        if (longitudinalAccel > 0.5f) // Сильное ускорение
        {
            accelerationPitch = -Mathf.Clamp(longitudinalAccel * 3f, 0f, MAX_PLATFORM_ANGLE);
        }
        else if (longitudinalAccel < -0.8f) // Сильное торможение
        {
            accelerationPitch = Mathf.Clamp(Mathf.Abs(longitudinalAccel) * 3f, 0f, MAX_PLATFORM_ANGLE);
        }

        // Повороты -> крен (roll)
        float accelerationRoll = 0f;
        if (Mathf.Abs(lateralAccel) > 0.3f)
        {
            accelerationRoll = -Mathf.Clamp(lateralAccel * 5f, -MAX_PLATFORM_ANGLE, MAX_PLATFORM_ANGLE);
        }
        float targetPitch = accelerationPitch + carPitch;
        float targetRoll = accelerationRoll + carRoll;

        // Ограничиваем общий угол
        targetPitch = Mathf.Clamp(targetPitch, -MAX_PLATFORM_ANGLE, MAX_PLATFORM_ANGLE);
        targetRoll = Mathf.Clamp(targetRoll, -MAX_PLATFORM_ANGLE, MAX_PLATFORM_ANGLE);

        // Плавное изменение углов
        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, 0.3f);
        _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, 0.4f);

        Debug.Log($"🎯 Платформа: Pitch={_currentPitch:F1}°, Roll={_currentRoll:F1}° | " + $"Ускорение: {longitudinalAccel:F2} m/s², Боковое: {lateralAccel:F2} m/s² | " + $"Авто: Pitch={carPitch:F1}°, Roll={carRoll:F1}°");

        _telemetryDataData.Angles = new Vector3(_currentPitch, 0, _currentRoll); // отправка данных на платформу 2DOF
    }
}