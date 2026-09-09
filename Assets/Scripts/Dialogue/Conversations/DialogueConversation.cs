using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueConversation", menuName = "Scriptable Objects/Dialogue Conversation")]
public class DialogueConversation : ScriptableObject
{
    [Serializable]
    public struct Option
    {
        [TextArea(1,6)]
        public string text;
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
        public string nextBox;
    }

    public List<Snippet> conversation = new List<Snippet>();
}
