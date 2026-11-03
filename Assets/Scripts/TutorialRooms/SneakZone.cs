using System;
using UnityEngine;

public class SneakZone : MonoBehaviour
{
    public event Action PlayerSneakedIn;

    private void OnTriggerStay(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (pc.CurrentState == PlayerController.PlayerState.Sneaking)
            PlayerSneakedIn?.Invoke();
    }
}
