using System;
using UnityEngine;

public class SneakZone : MonoBehaviour
{
    public event Action PlayerSneakedIn;

    [SerializeField] private bool requireSpecificState = true;
    [SerializeField] private PlayerController.PlayerState requiredState = PlayerController.PlayerState.Sneaking;

    private void OnTriggerStay(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (!requireSpecificState || pc.CurrentState == requiredState)
            PlayerSneakedIn?.Invoke();
    }
}
