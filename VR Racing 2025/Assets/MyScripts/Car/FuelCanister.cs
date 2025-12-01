using UnityEngine;

public class FuelCanister : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Car"))
        {
            if (Fuel.GetFuel() < (Fuel.GetMaxFuel() - 5))
            {
                Fuel.AddFuel(10);
                Destroy(gameObject);
            }            
        }
    }
}
