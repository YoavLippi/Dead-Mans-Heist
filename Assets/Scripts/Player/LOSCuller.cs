using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LOSCuller : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemiesInRange;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other.gameObject);
            other.gameObject.GetComponentInChildren<LosHandler>().StartLooking();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.gameObject);
            other.gameObject.GetComponentInChildren<LosHandler>().StopLooking();
        }
    }
}
