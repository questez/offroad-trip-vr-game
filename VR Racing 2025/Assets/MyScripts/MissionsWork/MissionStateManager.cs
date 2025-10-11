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

    private List<GameObject> Buffer;

    public static char CurrentMission;    

    private void Start()
    {
        CurrentMission = '0';                
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
                        Instantiate(Barrel, spawnPoint.position, Quaternion.Euler(0f, 0f, 90f));
                        Instantiate(Barrel, new Vector3(spawnPoint.position.x + 0.7f, spawnPoint.position.y, spawnPoint.position.z), Quaternion.Euler(0f, 0f, 90f));
                        Instantiate(Barrel, new Vector3(spawnPoint.position.x - 0.7f, spawnPoint.position.y, spawnPoint.position.z), Quaternion.Euler(0f, 0f, 90f));

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
                        ;
                    }
                    Debug.Log("Завершена миссия " + CurrentMission);
                }
            }                
        }
    }



    private void OnTriggerStay(Collider other)
    {
        CheckLoading(other);
        CheckDelivery(other);
    }

}
