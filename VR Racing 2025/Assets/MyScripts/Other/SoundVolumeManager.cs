using LogitechG29.Sample.Input;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundVolumeManager : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private Slider SoundSliderInMainMenu;
    [SerializeField] private Slider AudioPlayerSlider;

    private AudioSource[] AllAudioSources;
    private AudioSource audioPlayer;

    public static float CommonSoundVolume { get; private set; } = 0.75f;
    public static float AudioPlayerVolume { get; private set; } = 0.3f;

    private bool OffInput;

    private void Start()
    {
        if (SoundSliderInMainMenu != null)
        {
            SoundSliderInMainMenu.value = CommonSoundVolume;
            AudioPlayerVolume = CommonSoundVolume;
        }

        if (AudioPlayerSlider != null)
        {
            AudioPlayerSlider.value = AudioPlayerVolume;
        }

        AllAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        SetVolumeForAll(AllAudioSources);

        foreach (AudioSource source in AllAudioSources)
        {
            if (source.tag == "AudioPlayer")
            {
                audioPlayer = source;
            }
        }
    }

    private void OnEnable()
    {
        if (SoundSliderInMainMenu != null)
        {
            SoundSliderInMainMenu.onValueChanged.AddListener(ChangeCommonSoundVolume);
        }
        if (AudioPlayerSlider != null)
        {
            AudioPlayerSlider.onValueChanged.AddListener(ChangeAudioPlayerVolume);
        }
    }

    private void OnDisable()
    {
        if (SoundSliderInMainMenu != null)
        {
            SoundSliderInMainMenu.onValueChanged.RemoveListener(ChangeCommonSoundVolume);
        }
        if (AudioPlayerSlider != null)
        {
            AudioPlayerSlider.onValueChanged.RemoveListener(ChangeAudioPlayerVolume);
        }
    }

    private void Update()
    {
        if (SoundSliderInMainMenu != null && SoundSliderInMainMenu.IsActive())
        {
            if (inputControllerReader.LeftTurn || inputControllerReader.HatSwitch.x == -1)
            {
                ScrollToLeftInMainMenu();
            }
            else if (inputControllerReader.RightTurn || inputControllerReader.HatSwitch.x == 1)
            {
                ScrollToRightInMainMenu();
            }            
        }
        if (AudioPlayerSlider != null && AudioPlayerSlider.IsActive())
        {
            if (inputControllerReader.LeftTurn || inputControllerReader.HatSwitch.x == -1)
            {
                ScrollToLeftAudioPlayer();
            }
            else if (inputControllerReader.RightTurn || inputControllerReader.HatSwitch.x == 1)
            {
                ScrollToRightAudioPlayer();
            }
        }
    }

    private void ScrollToLeftInMainMenu()
    {
        if (!OffInput)
        {
            if (SoundSliderInMainMenu.value != SoundSliderInMainMenu.minValue)
            {
                SoundSliderInMainMenu.value -= 0.05f;
            }
            StartCoroutine(OffInputDelay());
            SetVolumeForAll(AllAudioSources);
        }        
    }

    private void ScrollToRightInMainMenu()
    {
        if (!OffInput)
        {
            if (SoundSliderInMainMenu.value != SoundSliderInMainMenu.maxValue)
            {
                SoundSliderInMainMenu.value += 0.05f;
            }
            StartCoroutine(OffInputDelay());
            SetVolumeForAll(AllAudioSources);
        }        
    }

    private void ScrollToLeftAudioPlayer()
    {
        if (!OffInput)
        {
            if (AudioPlayerSlider.value != AudioPlayerSlider.minValue)
            {
                AudioPlayerSlider.value -= 0.05f;
            }
            StartCoroutine(OffInputDelay());
            SetVolumeForAudioPlayer(audioPlayer);
        }
    }

    private void ScrollToRightAudioPlayer()
    {
        if (!OffInput)
        {
            if (AudioPlayerSlider.value != AudioPlayerSlider.maxValue)
            {
                AudioPlayerSlider.value += 0.05f;
            }
            StartCoroutine(OffInputDelay());
            SetVolumeForAudioPlayer(audioPlayer);
        }
    }

    private void ChangeCommonSoundVolume(float value)
    {
        CommonSoundVolume = value;
        Debug.Log($"CommonSoundVolume: {CommonSoundVolume}");
    }

    private void ChangeAudioPlayerVolume(float value)
    {
        AudioPlayerVolume = value;
        Debug.Log($"AudioPlayerVolume: {AudioPlayerVolume}");
    }

    private IEnumerator OffInputDelay()
    {
        OffInput = true;
        yield return new WaitForSecondsRealtime(0.15f);
        OffInput = false;
    }

    private void SetVolumeForAll(AudioSource[] sources)
    {
        foreach (AudioSource source in sources)
        {
            source.volume = CommonSoundVolume;
        }
    }

    private void SetVolumeForAudioPlayer(AudioSource source)
    {
        source.volume = AudioPlayerVolume;
    }
}
