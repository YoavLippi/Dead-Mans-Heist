using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InvokeOnCollision : MonoBehaviour
{
    public UnityEvent collisionEvent;
    public bool invokeOneshot;
    private bool hasInvoked = false;
    private void OnTriggerEnter(Collider other)
    {
        if (invokeOneshot && hasInvoked) return;
        if (other.CompareTag("Player"))
        {
            collisionEvent.Invoke();
            hasInvoked = true;
        }
    }
}
