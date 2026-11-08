using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GearPuzzle : MonoBehaviour
{

    [SerializeField] private List<Gear> gears;
    [SerializeField] private UnityEvent onSolved;
    [SerializeField] private UnityEvent onFailed;

    [SerializeField] private bool correct = false;

    public void OnSubmit() 
    {
        if (correct) return;
        foreach (Gear gear in gears) 
        {
            if (gear.IsCorrect == false) 
            {
                onFailed.Invoke();
                return;
            }
        }
        correct = true;
        onSolved.Invoke();
        foreach (Gear gear in gears) 
        {
            gear.Locked();
        }

    }
}
