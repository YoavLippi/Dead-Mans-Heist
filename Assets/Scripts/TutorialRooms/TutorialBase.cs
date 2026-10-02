using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class TutorialBase : MonoBehaviour
{

    [SerializeField] private UnityEvent onRoomComplete;
    [SerializeField] private UnityEvent<string> onPromptChange;

    protected PlayerController playerController;
    bool starting, finishing;

    public event System.Action<TutorialBase> roomComplete;

    private void OnTriggerEnter(Collider other)
    {
        if (starting) return;
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        playerController = player;
        starting = true;
        OnRoomStarted();
    }
    private void OnTriggerExit(Collider other)
    {
        ShowPrompt("");
    }

    protected void ShowPrompt(string text) => onPromptChange?.Invoke(text);

    protected void CompleteRoom()
    {
        if (finishing) return;
        finishing = true;
        OnRoomFinished();
        onRoomComplete?.Invoke();
        roomComplete?.Invoke(this);
    }

    protected abstract void OnRoomStarted();
    protected virtual void OnRoomFinished() { }

    protected virtual void OnDisable() 
    {
        OnRoomFinished(); 
    }
}
