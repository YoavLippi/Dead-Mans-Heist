using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;

public enum TutActions
{
    Walk,
    Sneak,
    Hide,
    Unhide,
    SneakToZone,
    MoveToZone,
    InteractWith,
    Mantle
}

[Serializable]
public struct TutorialStep
{
    public TutActions action;
    public Sprite image;
    public SneakZone zone;
    public Interactable target;
    [TextArea] public string prompt;
    public UnityEvent onStepStart; 
    public UnityEvent onStepComplete;
    
    public AudioClip startAudio;
    public AudioClip endAudio;
}

public class TutorialRoom : TutorialBase, iSaveable
{
    [SerializeField] private List<TutorialStep> steps;
    [SerializeField] private TutorialFade promptUI;
    [SerializeField] protected PlayAudio tutaudio;

    private bool actionDone;
    [SerializeField] private TutActions currentAction;
    private Coroutine routine;
    private SneakZone activeZone;
    private UnityEvent activeEvent;
    private bool isComplete = false;



    protected override void OnRoomStarted()
    {
        playerController.onStateChange += HandleStateChanged;
        routine = StartCoroutine(RunTutorial());
    }

    protected override void OnRoomFinished()
    {
        isComplete = true;
        if (playerController != null)
        {
            playerController.onStateChange -= HandleStateChanged;
        }
        UnhookZone();
        UnhookEvent();
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

            if (step.action == TutActions.SneakToZone || step.action == TutActions.MoveToZone)
            {
                if (step.zone == null)
                {
                    Debug.LogError("TutorialRoom: SneakToZone step has no zone assigned.", this);
                    continue;
                }
                activeZone = step.zone;
                activeZone.PlayerSneakedIn += HandleZoneReached;
            }
            if (step.action == TutActions.InteractWith)
            {
                if (step.target == null ||
                    !step.target.TryGetConditionalEventOfType(ConditionInteraction.Interact, out activeEvent))
                {
                    activeEvent = null;
                    continue;
                }
                activeEvent.AddListener(HandleInteracted);
            }
            step.onStepStart?.Invoke();
            if (tutaudio != null) tutaudio.Play(step.startAudio);
            else Debug.LogError("TutorialRoom: tutaudio isn't assigned.", this);
            ShowStep(step.prompt, step.image);
            yield return new WaitUntil(() => actionDone);
            step.onStepComplete?.Invoke();
            if (tutaudio != null) tutaudio.Play(step.endAudio);
            else Debug.LogError("TutorialRoom: tutaudio isn't assigned.", this);
            UnhookZone() ;
            UnhookEvent();
        }
        ShowStep("", null);
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
            TutActions.Mantle => state == PlayerController.PlayerState.Mantling,
            _ => false
        };

        if (matched) actionDone = true;
    }
    private void HandleZoneReached() => actionDone = true;
    private void HandleInteracted() => actionDone = true;

    private void UnhookZone()
    {
        if (activeZone == null) return;
        activeZone.PlayerSneakedIn -= HandleZoneReached;
        activeZone = null;
    }
    private void UnhookEvent()
    {
        if (activeEvent == null) return;
        activeEvent.RemoveListener(HandleInteracted);
        activeEvent = null;
    }

    #region Saving

    [Header("Saving")] 
    [SerializeField] private bool savedCompleteState;
    
    private void OnEnable()
    {
        ((iSaveable)this).RegisterSaveEvents();
    }

    private void OnDisable()
    {
        ((iSaveable)this).DeregisterSaveEvents();
    }

    public void DoSave()
    {
        savedCompleteState = isComplete;
    }
    
    public void DoLoad()
    {
        isComplete = savedCompleteState;
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        routine = StartCoroutine(RunTutorial());
    }
    
    #endregion
}