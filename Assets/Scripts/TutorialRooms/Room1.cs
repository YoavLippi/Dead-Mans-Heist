using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TutActions
{ 
    Walk, 
    Sneak,
    Sprint
}

[Serializable]
public struct TutorialStep
{
    public TutActions action;
    [TextArea] public string prompt;
}



public class Room1 : TutorialBase
{
    [SerializeField] private List<TutorialStep> steps;
    private bool actionDone;
    private TutActions currentAction;
    private Coroutine routine;



    protected override void OnRoomStarted()
    {
        playerController.onStateChange += HandleStateChanged;
        routine = StartCoroutine(RunTutorial());
    }

    protected override void OnRoomFinished()
    {
        if (playerController != null)
            playerController.onStateChange -= HandleStateChanged;
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
    private IEnumerator RunTutorial()
    {
        foreach (var step in steps)
        {
            currentAction = step.action;
            actionDone = false;
            ShowPrompt(step.prompt);
            yield return new WaitUntil(() => actionDone);
        }
        CompleteRoom();
    }
    private void HandleStateChanged(PlayerController.PlayerState state)
    {
        if (Matches(currentAction, state))
            actionDone = true;
    }

    private bool Matches(TutActions action, PlayerController.PlayerState state)
    {
        return action switch
        {
            TutActions.Walk => state == PlayerController.PlayerState.Walking,
            TutActions.Sneak => state == PlayerController.PlayerState.Sneaking,
            TutActions.Sprint => state == PlayerController.PlayerState.Running,
            _ => false
        };
    }
}
