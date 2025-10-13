using LogitechG29.Sample.Input;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionStateManager : MonoBehaviour
{
    [SerializeField] private InputControllerReader inputControllerReader;

    [SerializeField] private GameObject Barrel;
    [SerializeField] private GameObject Plank;
    [SerializeField] private GameObject Box;    
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
                    if (other.gameObject.name.Contains('1'))
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
                        }

                        CurrentMission = '1';
                    }
                    else if (other.gameObject.name.Contains('2'))
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
                        }
                        CurrentMission = '2';
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
                        }
                        foreach (var offset in localOffsetsForChest)
                        {
                            // ѕереводим локальные смещени€ в мировые координаты без вращени€ родител€
                            Vector3 worldPos = spawnPoint.TransformPoint(offset);
                            Quaternion worldRot = Quaternion.Euler(0f, spawnPoint.eulerAngles.y - 90f, 0f);
                            GameObject chest = Instantiate(Chest, worldPos, worldRot);
                            spawnedCargos.Add(chest);
                        }
                        CurrentMission = '3';                        
                    }                   

                    Debug.Log("Ќачата мисси€ " + CurrentMission);
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
                    if (CurrentMission == '1' && other.gameObject.name.Contains('1'))
                    {
                        Debug.Log("«авершена мисси€ " + CurrentMission);
                        DestroyCargos();
                        CurrentMission = '0';
                    }
                    else if (CurrentMission == '2' && other.gameObject.name.Contains('2'))
                    {
                        Debug.Log("«авершена мисси€ " + CurrentMission);
                        DestroyCargos();
                        CurrentMission = '0';
                    }
                    else if (CurrentMission == '3' && other.gameObject.name.Contains('3'))
                    {
                        Debug.Log("«авершена мисси€ " + CurrentMission);
                        DestroyCargos();
                        CurrentMission = '0';
                    }

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
