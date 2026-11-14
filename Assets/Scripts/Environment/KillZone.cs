using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private float waitBeforeFade = 0.3f;  
   

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;

        triggered = true;
        CameraController.Instance.FadeToBlackAndReload(waitBeforeFade);
        Invoke(nameof(ResetTrigger), waitBeforeFade + 3f);
    }

    private void ResetTrigger() => triggered = false;
}
