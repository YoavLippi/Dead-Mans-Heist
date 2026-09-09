using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueConversation", menuName = "Scriptable Objects/Dialogue Conversation")]
public class DialogueConversation : ScriptableObject
{
    public enum ConversationName
    {
        DavyIntro,
        DavyBasic
    }
    public ConversationName thisConversationName;
    [Serializable]
    public struct Snippet
    {
        public string ID;
        public string speakerName;
        [TextArea(1,6)]
        public string text;
        public bool hasOptions;
        public string[] options;
        public string nextBox;
    }

    public List<Snippet> conversation = new List<Snippet>();
}
