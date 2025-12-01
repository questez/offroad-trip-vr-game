using LogitechG29.Sample.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bhaptics.SDK2;

public class MissionStateManager : MonoBehaviour
{
    [SerializeField] private AudioSource boxHitSound, plankHitSound;    

    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private GameObject LoadingCarScreen;
    [SerializeField] private Slider slider1;
    [SerializeField] private GameObject DeliveryCarScreen;
    [SerializeField] private Slider slider2;

    [SerializeField] private GameObject Barrel;
    [SerializeField] private GameObject Plank;
    [SerializeField] private GameObject Box;    
    [SerializeField] private GameObject Chest;

    [SerializeField] private Transform spawnPoint;

    private List<GameObject> spawnedCargos = new List<GameObject>();

    public static int spawnedCargosCount { get; private set; }

    private DetailedTriggerChecker triggerChecker = new DetailedTriggerChecker();

    [SerializeField] private GameObject Body1;
    private Collider[] Body1Colliders;
    [SerializeField] private GameObject Body2;
    private Collider[] Body2Colliders;

    private float North_button_hold_timer;

    private void Start()
    {
        Body2Colliders = Body2.GetComponentsInChildren<Collider>();
        Body1Colliders = Body1.GetComponentsInChildren<Collider>();
        North_button_hold_timer = 0f;
        if (DeliveryCarScreen != null && LoadingCarScreen != null)
        { 
            DeliveryCarScreen.SetActive(false);
            LoadingCarScreen.SetActive(false);
        }        
        PlayerBehaviour.CurrentMission = '0';
        DestroyCargos();
    }

    private void GiveAward(char curr_mission, int counterOfObjects)
    {
        if (curr_mission == '1')
        {
            PlayerBehaviour.PlayerBalance += (500 * counterOfObjects);
        }
        else if (curr_mission == '2')
        {
            PlayerBehaviour.PlayerBalance += (200 * counterOfObjects);
        }
        else if (curr_mission == '3')
        {
            PlayerBehaviour.PlayerBalance += (600 * counterOfObjects);
        }        

        if (counterOfObjects > 0)
        {
            PlayerBehaviour.FinishedMissionsCounter++;
        }        
    }

    private void OnLoadingCarScreen()
    {
        LoadingCarScreen.SetActive(true);
        North_button_hold_timer += Time.deltaTime;
        slider1.value = Mathf.Lerp(0, North_button_hold_timer, 0.25f);
        //Debug.Log("Метод OnLoadingCarScreen выполняется!");
    }

    private void OffLoadingCarScreen()
    {
        North_button_hold_timer = 0;
        slider1.value = 0;
        LoadingCarScreen.SetActive(false);
       //Debug.Log("Метод OffLoadingCarScreen выполняется!");
    }

    private void OnDeliveryCarScreen()
    {
        DeliveryCarScreen.SetActive(true);
        North_button_hold_timer += Time.deltaTime;
        slider2.value = Mathf.Lerp(0, North_button_hold_timer, 0.25f);
        //Debug.Log("Метод OnDeliveryCarScreen выполняется!");
    }

    private void OffDeliveryCarScreen()
    {
        North_button_hold_timer = 0;
        slider2.value = 0;
        DeliveryCarScreen.SetActive(false);
        //Debug.Log("Метод OffDeliveryCarScreen выполняется!");
    }

    private void CheckLoading(Collider other)
    {
        if (PlayerBehaviour.CurrentMission == '0')
        {
            if (other.gameObject.CompareTag("LoadingPlace"))
            {
                if (inputControllerReader.NorthButton && triggerChecker.IsObjectsCompletelyInsideTrigger(Body1Colliders, other) && triggerChecker.IsObjectsCompletelyInsideTrigger(Body2Colliders, other))
                {
                    OnLoadingCarScreen();
                    if (slider1.value == slider1.maxValue)
                    {                        
                        if (other.gameObject.name.Contains('1'))
                        {
                            // Смещения относительно spawnPoint (кузова)
                            Vector3[] localOffsets =
                            {
                            new Vector3(0f, 0f, 0f),
                            new Vector3(0f, 0f, 0.7f),
                            new Vector3(0f, 0f, -0.7f)
                        };

                            foreach (var offset in localOffsets)
                            {
                                // Переводим локальные смещения в мировые координаты без вращения родителя
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(90f, spawnPoint.eulerAngles.y, 0f);
                                GameObject barrel = Instantiate(Barrel, worldPos, worldRot);
                                spawnedCargos.Add(barrel);
                                boxHitSound.Play();
                            }
                            BhapticsLibrary.Play(BhapticsEvent.BARRELSLOADING);
                            PlayerBehaviour.CurrentMission = '1';
                        }
                        else if (other.gameObject.name.Contains('2'))
                        {
                            // Смещения относительно spawnPoint (кузова)
                            Vector3[] localOffsets =
                            {
                            new Vector3(0f, 0.3f, 0f),
                            new Vector3(0.2f, 0.3f, 0f),
                            new Vector3(-0.2f, 0.3f, 0f),
                            new Vector3(-0.1f, 0.4f, 0f),
                            new Vector3(0.1f, 0.4f, 0f),
                            new Vector3(-0.2f, 0.5f, 0f),
                            new Vector3(0.2f, 0.5f, 0f),
                            new Vector3(0f, 0.5f, 0f)
                        };

                            foreach (var offset in localOffsets)
                            {
                                // Переводим локальные смещения в мировые координаты без вращения родителя
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
                                GameObject plank = Instantiate(Plank, worldPos, worldRot);
                                spawnedCargos.Add(plank);
                                plankHitSound.Play();
                            }
                            BhapticsLibrary.Play(BhapticsEvent.PLANKSLOADING);
                            PlayerBehaviour.CurrentMission = '2';
                        }
                        else
                        {
                            // Смещения относительно spawnPoint (кузова)
                            Vector3[] localOffsetsForBox =
                            {
                            new Vector3(0.4f, 0f, 1f),
                            new Vector3(-0.4f, 0f, 1f),
                            new Vector3(0f, 0f, 0.4f)
                            };
                            Vector3[] localOffsetsForChest =
                            {
                            new Vector3(0.28f, 0f, -0.5f),
                            new Vector3(-0.28f, 0f, -0.5f)
                            };

                            foreach (var offset in localOffsetsForBox)
                            {
                                // Переводим локальные смещения в мировые координаты без вращения родителя
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
                                GameObject box = Instantiate(Box, worldPos, worldRot);
                                spawnedCargos.Add(box);
                                boxHitSound.Play();
                            }
                            foreach (var offset in localOffsetsForChest)
                            {
                                // Переводим локальные смещения в мировые координаты без вращения родителя
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(0f, spawnPoint.eulerAngles.y - 90f, 0f);
                                GameObject chest = Instantiate(Chest, worldPos, worldRot);
                                spawnedCargos.Add(chest);
                            }
                            BhapticsLibrary.Play(BhapticsEvent.BOXESLOADING);
                            PlayerBehaviour.CurrentMission = '3';
                        }
                        spawnedCargosCount = spawnedCargos.Count;
                        OffLoadingCarScreen();
                        Debug.Log("Начата миссия " + PlayerBehaviour.CurrentMission);
                    }                              

                    
                }
            }
        }
    }

    private void CheckDelivery(Collider other)
    {
        if (PlayerBehaviour.CurrentMission != '0')
        {
            if (other.gameObject.CompareTag("DeliveryPlace"))
            {
                if (inputControllerReader.NorthButton && triggerChecker.IsObjectsCompletelyInsideTrigger(Body1Colliders, other) && triggerChecker.IsObjectsCompletelyInsideTrigger(Body2Colliders, other))
                {
                    OnDeliveryCarScreen();
                    if (slider2.value == slider2.maxValue)
                    {                        
                        if (PlayerBehaviour.CurrentMission == '1' && other.gameObject.name.Contains('1'))
                        {
                            Debug.Log("Завершена миссия " + PlayerBehaviour.CurrentMission);
                            DestroyCargos();
                            GiveAward(PlayerBehaviour.CurrentMission, Trunk.CounterOfObjectsInTrunk);
                            PlayerBehaviour.CurrentMission = '0';
                        }
                        else if (PlayerBehaviour.CurrentMission == '2' && other.gameObject.name.Contains('2'))
                        {
                            Debug.Log("Завершена миссия " + PlayerBehaviour.CurrentMission);
                            DestroyCargos();
                            GiveAward(PlayerBehaviour.CurrentMission, Trunk.CounterOfObjectsInTrunk);
                            PlayerBehaviour.CurrentMission = '0';
                        }
                        else if (PlayerBehaviour.CurrentMission == '3' && other.gameObject.name.Contains('3'))
                        {
                            Debug.Log("Завершена миссия " + PlayerBehaviour.CurrentMission);
                            DestroyCargos();
                            GiveAward(PlayerBehaviour.CurrentMission, Trunk.CounterOfObjectsInTrunk);
                            PlayerBehaviour.CurrentMission = '0';
                        }
                        Trunk.CleanCounter();
                        OffDeliveryCarScreen();
                    }
                }
            }                
        }
    }
    

    private void DestroyCargos()
    {
        if (spawnedCargos != null && !CarController.OffInput)
        {
            foreach (var c in spawnedCargos)
            {
                if (c != null)
                {
                    Destroy(c);
                }
            }            
        }
        spawnedCargos.Clear();
    }   


    private void OnTriggerStay(Collider other)
    {
        if (!CarController.OffInput)
        {
            CheckLoading(other);
            CheckDelivery(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!CarController.OffInput)
        {
            if (PlayerBehaviour.CurrentMission == '0')
            {
                if (other.gameObject.CompareTag("LoadingPlace"))
                {
                    OffLoadingCarScreen();
                }
            }
            else
            {
                if (other.gameObject.CompareTag("DeliveryPlace"))
                {
                    OffDeliveryCarScreen();
                }
            }
        }        
    }
}
