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
                        // неправильный поворот при спавне
                        Quaternion spawnrotation = Quaternion.Euler(0, 0, 90f) * spawnPoint.rotation;
                        GameObject barrel1 = Instantiate(Barrel, spawnPoint.position, spawnrotation);
                        GameObject barrel2 = Instantiate(Barrel, new Vector3(spawnPoint.position.x + 0.7f, spawnPoint.position.y, spawnPoint.position.z), spawnrotation);
                        GameObject barrel3 = Instantiate(Barrel, new Vector3(spawnPoint.position.x - 0.7f, spawnPoint.position.y, spawnPoint.position.z), spawnrotation);

                        spawnedCargos.Add(barrel1);
                        spawnedCargos.Add(barrel2);
                        spawnedCargos.Add(barrel3);

                        CurrentMission = '1';
                    }
                    else if (other.gameObject.name == "LoadingPlatfыorm2")
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
