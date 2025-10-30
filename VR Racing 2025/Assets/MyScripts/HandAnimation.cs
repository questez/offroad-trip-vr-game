using UnityEngine;
public class HandAnimation: MonoBehaviour
{    
    [SerializeField] private Animator HandAnimator;

    private void Start()
    {
        HandAnimator.SetFloat("Trigger", 0.4f);
        HandAnimator.SetFloat("Grip", 0.4f);
    }
}