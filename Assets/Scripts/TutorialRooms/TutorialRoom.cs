using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutActions
{
    Walk,
    Sneak,
    Sprint,
    GhostOn,
    GhostReturned,
    Hide,
    Unhide
}

[Serializable]
public struct TutorialStep
{
    public TutActions action;
    [TextArea] public string prompt;
}

public class TutorialRoom : TutorialBase
{
    [SerializeField] private List<TutorialStep> steps;

    private bool actionDone;
    private TutActions currentAction;
    private Coroutine routine;

    protected override void OnRoomStarted()
    {
        playerController.onStateChange += HandleStateChanged;
        playerController.onGhostChange += HandleGhostChanged;
        playerController.onGhostReturned += HandleGhostReturned;
        routine = StartCoroutine(RunTutorial());
    }

    protected override void OnRoomFinished()
    {
        if (playerController != null)
        {
            playerController.onStateChange -= HandleStateChanged;
            playerController.onGhostChange -= HandleGhostChanged;
            playerController.onGhostReturned -= HandleGhostReturned;
        }
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
        bool matched = currentAction switch
        {
            TutActions.Walk => state == PlayerController.PlayerState.Walking,
            TutActions.Sneak => state == PlayerController.PlayerState.Sneaking,
            TutActions.Sprint => state == PlayerController.PlayerState.Running,
            TutActions.Hide => state == PlayerController.PlayerState.Hiding,
            TutActions.Unhide => state != PlayerController.PlayerState.Hiding,
            _ => false
        };

        if (matched) actionDone = true;
    }

    private void HandleGhostChanged(bool ghosted)
    {
        if (currentAction == TutActions.GhostOn && ghosted)
            actionDone = true;
    }

    private void HandleGhostReturned()
    {
        if (currentAction == TutActions.GhostReturned)
            actionDone = true;
    }
}