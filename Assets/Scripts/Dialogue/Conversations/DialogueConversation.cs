using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DialogueConversation", menuName = "Dialogue/Conversation")]
public class DialogueConversation : ScriptableObject
{
    public enum DialogueActionType
    {
        None,
        StartQuest,
        CompleteQuest,
        GiveItem,
        RemoveItem,
        SetFlag
    }
    
    [Serializable]
    public struct DialogueAction
    {
        public DialogueActionType type;
        public string parameter;
        public string value;
    }
    public delegate void DialogueEvent();
    [Serializable]
    public struct Option
    {
        [TextArea(1,6)]
        public string text;

        public DialogueAction[] actions;
        [TextArea(1,6)]
        public string nextBox;
    }
    
    [Serializable]
    public struct Snippet
    {
        public string ID;
        public string speakerName;
        [TextArea(1,6)]
        public string text;
        public bool hasOptions;
        public Option[] options;
        public DialogueAction[] snippetCompletionActions;
        public string nextBox;
    }

    public List<DialogueCondition> conditions;
    public List<Snippet> conversation = new List<Snippet>();
}
