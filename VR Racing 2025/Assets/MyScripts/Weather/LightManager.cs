using UnityEngine;

public class LightManager : MonoBehaviour
{
    [SerializeField] private Light directional_Light;

    private const float dayLightIntensity = 1.5f;
    private const float nightLightIntensity = 0f;

    private void Start()
    {
        SetLightIntensity(Wheather.GetDayStatus());
    }

    private void SetLightIntensity(string dayStatus)
    {
        if (directional_Light != null)
        {
            if (dayStatus == "night")
            {
                directional_Light.intensity = nightLightIntensity;
            }
            else
            {
                directional_Light.intensity = dayLightIntensity;
            }
        }
    }
}
