using TMPro;
using UnityEngine;



public class CarUI : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] private TextMeshProUGUI currentSpeed;
    [SerializeField] private TextMeshProUGUI currentShifter;
    [SerializeField] private TextMeshProUGUI currentWheelDriveMode;
    


    private void Update()
    {
        UpdateBoardInfo();               
    }

    private void UpdateBoardInfo()
    {
        if (rb != null && currentShifter != null && currentSpeed != null)
        {
            int speed = Mathf.RoundToInt(rb.linearVelocity.magnitude * 3.6f);

            currentShifter.text = CarController.current_shifter.ToString();
            currentSpeed.text = speed.ToString() + " κμ/χ";
            currentWheelDriveMode.text = CarController.wheel_drive_mode.ToString();
        }        
    }
}
