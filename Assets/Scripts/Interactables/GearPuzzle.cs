using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GearPuzzle : Interactable
{

    [SerializeField] private List<Gear> gears;
    [SerializeField] private UnityEvent onSolved;
    [SerializeField] private UnityEvent onFailed;
    [SerializeField] private Animator animator;

    [SerializeField] private bool correct = false;

    public override void DoInteract() => OnSubmit();
    public void OnSubmit() 
    {
        Debug.Log("Submit");
        if (correct) return;
        animator.SetBool("LeverInput", true);
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
