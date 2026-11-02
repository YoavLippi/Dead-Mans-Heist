using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    [SerializeField] private int lastKnownWorldTime;
    [SerializeField] private bool boatSinking = false;
    [SerializeField] private bool phase1 = false;
    [SerializeField] private bool phase2 = false;

    [SerializeField] private List<GameObject> phase1Holes = new List<GameObject>();
    [SerializeField] private List<GameObject> phase2Holes = new List<GameObject>();

    [SerializeField] private WaterRising waterObject;

    void OnEnable() => WorldTime.secondsChange += CheckTime;
    void OnDisable() => WorldTime.secondsChange -= CheckTime;

    private void Start()
    {
        if (waterObject == null)
        {
            waterObject = Object.FindFirstObjectByType<WaterRising>();
        }
    }

    protected void CheckTime(int currentworld)
    {
        lastKnownWorldTime = currentworld;

        if (lastKnownWorldTime >= 45 && !phase1)
        {
            phase1 = true;
            StartCoroutine(ActivateHolesWithDelay(phase1Holes)); 
        }

        if (lastKnownWorldTime >= 65 && !boatSinking)
        {
            boatSinking = true; 
            waterObject.StartRisingIndefinitely(); 
        }

        if (lastKnownWorldTime >= 100 && !phase2)
        {
            phase2 = true;
            StartCoroutine(ActivateHolesWithDelay(phase2Holes)); 
        }

        if (lastKnownWorldTime > 180)
        {
            Debug.Log("Death");
        }
    }
    private IEnumerator ActivateHolesWithDelay(List<GameObject> holesList)
    {
        foreach (GameObject hole in holesList)
        {
            if (hole != null)
            {
                hole.SetActive(true);
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
