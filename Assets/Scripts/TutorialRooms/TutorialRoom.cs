using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutActions
{
    Walk,
    Sneak,
    Hide,
    Unhide,
}

[Serializable]
public struct TutorialStep
{
    public TutActions action;
    public Sprite image;
    [TextArea] public string prompt;
}

public class TutorialRoom : TutorialBase
{
    [SerializeField] private List<TutorialStep> steps;
    [SerializeField] private TutorialFade promptUI;

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
        {
            playerController.onStateChange -= HandleStateChanged;
        }
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
    private void ShowStep(string text, Sprite sprite)
    {
        if (promptUI != null) promptUI.Show(text, sprite);
        else Debug.LogError("TutorialRoom: promptUI isn't assigned.", this);
    }

    private IEnumerator RunTutorial()
    {
        OnRoomBegin.Invoke();
        foreach (var step in steps)
        {
            currentAction = step.action;
            actionDone = false;
            ShowStep(step.prompt, step.image);
            yield return new WaitUntil(() => actionDone);
        }
        ShowStep("tester complete", null);
        CompleteRoom();
    }

    private void HandleStateChanged(PlayerController.PlayerState state)
    {
        bool matched = currentAction switch
        {
            TutActions.Walk => state == PlayerController.PlayerState.Walking,
            TutActions.Sneak => state == PlayerController.PlayerState.Sneaking,
            TutActions.Hide => state == PlayerController.PlayerState.Hiding,
            TutActions.Unhide => state != PlayerController.PlayerState.Hiding,
            _ => false
        };

        if (matched) actionDone = true;
    }
    }