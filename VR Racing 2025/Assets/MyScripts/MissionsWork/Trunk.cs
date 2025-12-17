using UnityEngine;

public class Trunk : MonoBehaviour
{
    public static int CounterOfObjectsInTrunk { get; private set; }

    private void Start()
    {
        CleanCounter();
    }

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

    public static void CleanCounter()
    {
        CounterOfObjectsInTrunk = 0;
    }
}
