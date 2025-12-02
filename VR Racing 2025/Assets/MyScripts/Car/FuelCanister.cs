using UnityEngine;
using System.Collections;

public class FuelCanister : MonoBehaviour
{
    private MeshRenderer m_Renderer;
    private Collider fuel_collider;

    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        fuel_collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (Fuel.GetFuel() < (Fuel.GetMaxFuel() - 5))
            {
                Fuel.AddFuel(10);
                StartCoroutine(VisibilityDelay());
            }            
        }
    }

    private IEnumerator VisibilityDelay()
    {
        m_Renderer.enabled = false;
        fuel_collider.enabled = false;
        yield return new WaitForSecondsRealtime(60);
        m_Renderer.enabled = true;
        fuel_collider.enabled = true;
    }
}
