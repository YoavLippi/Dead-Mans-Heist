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
    Unhide,
    UseTool
}

[Serializable]
public struct TutorialStep
{
    public TutActions action;
    public string toolID;
    [TextArea] public string prompt;
}

public class TutorialRoom : TutorialBase
{
    [SerializeField] private List<TutorialStep> steps;

    private bool actionDone;
    private TutActions currentAction;
    private Coroutine routine;

    private string currentToolID;


    protected override void OnRoomStarted()
    {
        playerController.onStateChange += HandleStateChanged;
        playerController.onGhostChange += HandleGhostChanged;
        playerController.onGhostReturned += HandleGhostReturned;
        Tool.OnToolUsed += HandleToolUsed;
        routine = StartCoroutine(RunTutorial());
    }

    protected override void OnRoomFinished()
    {
        if (playerController != null)
        {
            playerController.onStateChange -= HandleStateChanged;
            playerController.onGhostChange -= HandleGhostChanged;
            playerController.onGhostReturned -= HandleGhostReturned;
            Tool.OnToolUsed -= HandleToolUsed;
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
            currentToolID = step.toolID;
            actionDone = false;
            ShowPrompt(step.prompt);
            yield return new WaitUntil(() => actionDone);
        }
        ShowPrompt("You may leave the room and talk to Davi Jones to continue");
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
    private void HandleToolUsed(Tool tool)
    {
        if (currentAction == TutActions.UseTool && currentToolID == tool.ToolID) 
        {
            actionDone = true;
        }
    }
    }