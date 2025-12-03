using UnityEngine;

public class FogManager : MonoBehaviour
{
    private void Start()
    {
        SetFog(Wheather.GetDayStatus());
    }

    private void SetFog(string dayStatus)
    {
        if (dayStatus == "night")
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.01f;
        }
        else
        {
            RenderSettings.fog = false;
        }
    }
}
