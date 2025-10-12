using LogitechG29.Sample.Input;
using System.Collections.Generic;
using UnityEngine;

public class MissionStateManager : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private GameObject Barrel;
    [SerializeField] private GameObject Box;
    [SerializeField] private GameObject Plank;
    [SerializeField] private GameObject Chest;

    [SerializeField] private Transform spawnPoint;

    private List<GameObject> spawnedCargos = new List<GameObject>();

    public static char CurrentMission;

    private void Start()
    {
        CurrentMission = '0';
        DestroyCargos();
    }



    private void CheckLoading(Collider other)
    {
        if (CurrentMission == '0')
        {
            if (other.gameObject.CompareTag("LoadingPlace"))
            {
                if (inputControllerReader.NorthButton)
                {
                    if (other.gameObject.name == "LoadingPlatform1")
                    {
                        // Смещения относительно spawnPoint (кузова)
                        Vector3[] localOffsets =
                        {
                            new Vector3(0f, 0f, 0f),
                            new Vector3(0f, 0f, 0.7f),
                            new Vector3(0f, 0f, -0.7f)
                        };

                        // Мировая позиция кузова
                        Vector3 basePos = spawnPoint.position;

                        foreach (var offset in localOffsets)
                        {
                            // Переводим локальные смещения в мировые координаты без вращения родителя
                            Vector3 worldPos = spawnPoint.TransformPoint(offset);
                            Quaternion worldRot = Quaternion.Euler(90f, spawnPoint.rotation.eulerAngles.y, 0f);

                            GameObject barrel = Instantiate(Barrel, worldPos, worldRot);
                            spawnedCargos.Add(barrel);
                        }

                        CurrentMission = '1';
                    }
                    else if (other.gameObject.name == "LoadingPlatform2")
                    {                                                   
                        CurrentMission = '2';
                    }
                    else
                    {                        
                        CurrentMission = '3';                        
                    }                   

                    Debug.Log("Начата миссия " + CurrentMission);
                }
            }
        }
    }

    private void CheckDelivery(Collider other)
    {
        if (CurrentMission != '0')
        {
            if (other.gameObject.CompareTag("DeliveryPlace"))
            {
                if (inputControllerReader.NorthButton)
                {
                    if (CurrentMission == '1' && other.gameObject.name == "DeliveryPlatform1")
                    {
                        DestroyCargos();
                        CurrentMission = '0';
                    }
                    Debug.Log("Завершена миссия " + CurrentMission);
                }
            }                
        }
    }

    private void DestroyCargos()
    {
        if (spawnedCargos != null)
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
        CheckLoading(other);
        CheckDelivery(other);
    }
}
