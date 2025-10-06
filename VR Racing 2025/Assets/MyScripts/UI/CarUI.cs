using TMPro;
using UnityEngine;
using _2DOF;


public class CarUI : MonoBehaviour
{
    private ObjectTelemetryData telemetryDataData;

    [SerializeField] private Rigidbody rb;

    [SerializeField] private TextMeshProUGUI currentShifter;
    [SerializeField] private TextMeshProUGUI currentSpeed;   
    

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
        }        
    }

}
