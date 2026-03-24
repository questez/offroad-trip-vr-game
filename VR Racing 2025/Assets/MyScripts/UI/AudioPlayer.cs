using LogitechG29.Sample.Input;
using TMPro;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private TextMeshProUGUI audioPlayerInfoText;
    [SerializeField] private AudioClip[] songs;

    private void Start()
    {
        audioPlayerInfoText.text = "Плеер выключен";
    }

    private void OnEnable()
    {
        inputControllerReader.OnLeftBumperCallback += AudioPlayerToggle;
    }

    private void OnDisable()
    {
        inputControllerReader.OnLeftBumperCallback -= AudioPlayerToggle;
    }

    private void AudioPlayerToggle(bool value)
    {
        if (value && CarController.EngineIsRunning)
        {
            if (audioPlayer.isPlaying)
            {
                audioPlayerInfoText.text = "Плеер выключен";
                audioPlayer.Stop();
            }
            else
            {
                audioPlayerInfoText.text = "Играет " + songs[0].name;
                audioPlayer.resource = songs[0];
                audioPlayer.Play();
            }
        }
    }

}
