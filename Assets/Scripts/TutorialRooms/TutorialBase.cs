using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ConversationSender))]
public abstract class TutorialBase : MonoBehaviour
{
    [SerializeField] protected UnityEvent OnRoomBegin;
    [SerializeField] private UnityEvent onRoomComplete;
    [SerializeField] private UnityEvent<string> onPromptChange;
    [SerializeField] private ConversationSender cs;

    protected PlayerController playerController;
    bool starting, finishing;

    public event System.Action<TutorialBase> roomComplete;

    private void Start()
    {
        cs = GetComponent<ConversationSender>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (starting) return;
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        playerController = player;
        starting = true;
        OnRoomStarted();
    }
    //private void OnTriggerExit(Collider other)
    //{
    //    ShowPrompt("");
    //}

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
