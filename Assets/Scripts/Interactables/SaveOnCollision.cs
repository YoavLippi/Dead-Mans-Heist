using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SaveOnCollision : SavingParent
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WorldController.DoSave();
        }
    }
}
