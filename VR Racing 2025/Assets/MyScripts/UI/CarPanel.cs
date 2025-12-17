using TMPro;
using UnityEngine;

public class CarPanel : MonoBehaviour
{    
    [SerializeField] private Rigidbody rb;

    [SerializeField] private TextMeshProUGUI currentSpeed;
    [SerializeField] private TextMeshProUGUI currentShifter;
    [SerializeField] private TextMeshProUGUI currentWheelDriveMode;
    [SerializeField] private TextMeshProUGUI currentCounterOfObjectsInTrunk;

    private void Update()
    {
        UpdateBoardInfo();               
    }

    private void UpdateBoardInfo()
    {
        if (CarController.CarIsBroken)
        {
            currentSpeed.text = "ГИДРОУДАР";
            currentSpeed.color = Color.red;
            currentShifter.text = "";
            currentWheelDriveMode.text = "";
            currentCounterOfObjectsInTrunk.text = "";            
            return;
        }

        int speed = Mathf.RoundToInt(rb.linearVelocity.magnitude * 3.6f);

        currentShifter.text = CarController.current_shifter.ToString();
        currentSpeed.text = speed.ToString() + " км/ч";
        currentWheelDriveMode.text = CarController.wheel_drive_mode;

        if (PlayerData.CurrentMission != "None")
        {
            currentCounterOfObjectsInTrunk.text = $"Грузов в багажнике {Trunk.CounterOfObjectsInTrunk}/{MissionStateManager.spawnedCargosCount}";
        }
        else
        {
            currentCounterOfObjectsInTrunk.text = "";
        }              
    }
}
