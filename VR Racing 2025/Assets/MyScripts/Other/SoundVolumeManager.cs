using LogitechG29.Sample.Input;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundVolumeManager : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private Slider SoundSlider;

    private AudioSource[] AllAudioSources;

    public static float TotalSoundVolume { get; private set; } = 0.75f;

    private bool OffInput;

    private void Start()
    {
        if (SoundSlider != null)
        {
            SoundSlider.value = TotalSoundVolume;
        }
        AllAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        SetVolume(AllAudioSources);
    }

    private void OnEnable()
    {
        if (SoundSlider != null)
        {
            SoundSlider.onValueChanged.AddListener(ChangeVolume);
        }        
    }

    private void OnDisable()
    {
        if (SoundSlider != null)
        {
            SoundSlider.onValueChanged.RemoveListener(ChangeVolume);
        }            
    }

    private void Update()
    {
        if (SoundSlider != null && SoundSlider.IsActive())
        {
            if (inputControllerReader.LeftTurn || inputControllerReader.HatSwitch.x == -1)
            {
                if (!OffInput)
                {
                    ScrollToLeft();
                    StartCoroutine(OffInputDelay());
                    SetVolume(AllAudioSources);
                }
            }
            else if (inputControllerReader.RightTurn || inputControllerReader.HatSwitch.x == 1)
            {
                if (!OffInput)
                {
                    ScrollToRight();
                    StartCoroutine(OffInputDelay());
                    SetVolume(AllAudioSources);
                }
            }            
        }        
    }

    private void ScrollToLeft()
    {
        if (SoundSlider.value != SoundSlider.minValue)
        {
            SoundSlider.value -= 0.05f;
        }
    }

    private void ScrollToRight()
    {
        if (SoundSlider.value != SoundSlider.maxValue)
        {
            SoundSlider.value += 0.05f;
        }
    }

    private void ChangeVolume(float value)
    {
        TotalSoundVolume = value;
        Debug.Log($"TotalSoundVolume: {TotalSoundVolume}");
    }

    private IEnumerator OffInputDelay()
    {
        OffInput = true;
        yield return new WaitForSecondsRealtime(0.15f);
        OffInput = false;
    }

    private void SetVolume(AudioSource[] sources)
    {
        foreach (AudioSource source in sources)
        {
            source.volume = TotalSoundVolume;
        }
    }
}
