using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;

    private void Start()
    {
        SetSkyboxMaterial(Wheather.GetDayStatus());
    }

    private void SetSkyboxMaterial(string datStatus)
    {
        if (daySkybox != null && datStatus == "day")
        {
            RenderSettings.skybox = daySkybox;
        }
        else if (nightSkybox != null && datStatus == "night")
        {
            RenderSettings.skybox = nightSkybox;
        }
    }
}
