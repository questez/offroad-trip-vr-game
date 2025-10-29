using UnityEngine;

public class Trunk : MonoBehaviour
{
    public static int CounterOfObjectsInTrunk { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Cargo"))
        {
            CounterOfObjectsInTrunk++;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Cargo"))
        {
            CounterOfObjectsInTrunk--;
        }
    }
}
