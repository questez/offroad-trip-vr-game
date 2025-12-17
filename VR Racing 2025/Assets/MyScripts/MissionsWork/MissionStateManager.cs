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
    [SerializeField] private Slider loadingCarSlider;
    [SerializeField] private GameObject DeliveryCarScreen;
    [SerializeField] private Slider deliveryCarSlider;

    [SerializeField] private GameObject Barrel;
    [SerializeField] private GameObject Plank;
    [SerializeField] private GameObject Box;    
    [SerializeField] private GameObject Chest;

    private const int barrelCost = 400;
    private const int plankCost = 200;
    private const int boxCost = 800;

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
        DestroyCargos();
    }

    private void GiveAward(string curr_mission, int counterOfObjects)
    {
        if (curr_mission == "Compote")
        {
            PlayerData.PlayerBalance += (barrelCost * counterOfObjects);
        }
        else if (curr_mission == "Planks")
        {
            PlayerData.PlayerBalance += (plankCost * counterOfObjects);
        }
        else if (curr_mission == "Tools")
        {
            PlayerData.PlayerBalance += (boxCost * counterOfObjects);
        }        

        if (counterOfObjects > 0)
        {
            PlayerData.FinishedMissionsCounter++;
        }        
    }

    private void OnLoadingCarScreen()
    {
        LoadingCarScreen.SetActive(true);
        North_button_hold_timer += Time.deltaTime;
        loadingCarSlider.value = Mathf.Lerp(0, North_button_hold_timer, 0.25f);
    }

    private void OffLoadingCarScreen()
    {
        North_button_hold_timer = 0;
        loadingCarSlider.value = 0;
        LoadingCarScreen.SetActive(false);
    }

    private void OnDeliveryCarScreen()
    {
        DeliveryCarScreen.SetActive(true);
        North_button_hold_timer += Time.deltaTime;
        deliveryCarSlider.value = Mathf.Lerp(0, North_button_hold_timer, 0.25f);
    }

    private void OffDeliveryCarScreen()
    {
        North_button_hold_timer = 0;
        deliveryCarSlider.value = 0;
        DeliveryCarScreen.SetActive(false);
    }

    private void CheckLoading(Collider other)
    {
        if (PlayerData.CurrentMission == "None")
        {
            if (other.gameObject.CompareTag("LoadingPlace"))
            {
                if (inputControllerReader.NorthButton && triggerChecker.IsObjectsCompletelyInsideTrigger(Body1Colliders, other) && triggerChecker.IsObjectsCompletelyInsideTrigger(Body2Colliders, other))
                {
                    OnLoadingCarScreen();
                    if (loadingCarSlider.value == loadingCarSlider.maxValue)
                    {                        
                        if (other.gameObject.name.Contains("Compote"))
                        {
                            // —мещени€ относительно spawnPoint (кузова)
                            Vector3[] localOffsets =
                            {
                            new Vector3(0f, 0f, 0f),
                            new Vector3(0f, 0f, 0.7f),
                            new Vector3(0f, 0f, -0.7f)
                        };

                            foreach (var offset in localOffsets)
                            {
                                // ѕереводим локальные смещени€ в мировые координаты без вращени€ родител€
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(90f, spawnPoint.eulerAngles.y, 0f);
                                GameObject barrel = Instantiate(Barrel, worldPos, worldRot);
                                spawnedCargos.Add(barrel);
                                boxHitSound.Play();
                            }
                            BhapticsLibrary.Play(BhapticsEvent.BARRELSLOADING);
                            PlayerData.CurrentMission = "Compote";
                        }
                        else if (other.gameObject.name.Contains("Planks"))
                        {
                            // —мещени€ относительно spawnPoint (кузова)
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
                                // ѕереводим локальные смещени€ в мировые координаты без вращени€ родител€
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
                                GameObject plank = Instantiate(Plank, worldPos, worldRot);
                                spawnedCargos.Add(plank);
                                plankHitSound.Play();
                            }
                            BhapticsLibrary.Play(BhapticsEvent.PLANKSLOADING);
                            PlayerData.CurrentMission = "Planks";
                        }
                        else
                        {
                            // —мещени€ относительно spawnPoint (кузова)
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
                                // ѕереводим локальные смещени€ в мировые координаты без вращени€ родител€
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
                                GameObject box = Instantiate(Box, worldPos, worldRot);
                                spawnedCargos.Add(box);
                                boxHitSound.Play();
                            }
                            foreach (var offset in localOffsetsForChest)
                            {
                                // ѕереводим локальные смещени€ в мировые координаты без вращени€ родител€
                                Vector3 worldPos = spawnPoint.TransformPoint(offset);
                                Quaternion worldRot = Quaternion.Euler(0f, spawnPoint.eulerAngles.y - 90f, 0f);
                                GameObject chest = Instantiate(Chest, worldPos, worldRot);
                                spawnedCargos.Add(chest);
                            }
                            BhapticsLibrary.Play(BhapticsEvent.BOXESLOADING);
                            PlayerData.CurrentMission = "Tools";
                        }
                        spawnedCargosCount = spawnedCargos.Count;
                        OffLoadingCarScreen();
                        Debug.Log("Ќачата мисси€ " + PlayerData.CurrentMission);
                    }                              

                    
                }
            }
        }
    }

    private void CheckDelivery(Collider other)
    {
        if (PlayerData.CurrentMission != "None")
        {
            if (other.gameObject.CompareTag("DeliveryPlace"))
            {
                if (inputControllerReader.NorthButton && triggerChecker.IsObjectsCompletelyInsideTrigger(Body1Colliders, other) && triggerChecker.IsObjectsCompletelyInsideTrigger(Body2Colliders, other))
                {
                    OnDeliveryCarScreen();
                    if (deliveryCarSlider.value == deliveryCarSlider.maxValue)
                    {                        
                        if (PlayerData.CurrentMission == "Compote" && other.gameObject.name.Contains("Compote"))
                        {
                            Debug.Log("«авершена мисси€ " + PlayerData.CurrentMission);
                            DestroyCargos();
                            GiveAward(PlayerData.CurrentMission, Trunk.CounterOfObjectsInTrunk);
                            PlayerData.CurrentMission = "None";
                        }
                        else if (PlayerData.CurrentMission == "Planks" && other.gameObject.name.Contains("Planks"))
                        {
                            Debug.Log("«авершена мисси€ " + PlayerData.CurrentMission);
                            DestroyCargos();
                            GiveAward(PlayerData.CurrentMission, Trunk.CounterOfObjectsInTrunk);
                            PlayerData.CurrentMission = "None";
                        }
                        else if (PlayerData.CurrentMission == "Tools" && other.gameObject.name.Contains("Tools"))
                        {
                            Debug.Log("«авершена мисси€ " + PlayerData.CurrentMission);
                            DestroyCargos();
                            GiveAward(PlayerData.CurrentMission, Trunk.CounterOfObjectsInTrunk);
                            PlayerData.CurrentMission = "None";
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
            if (PlayerData.CurrentMission == "None")
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
