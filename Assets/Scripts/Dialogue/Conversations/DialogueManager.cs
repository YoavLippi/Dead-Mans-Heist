using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueBox;
    [SerializeField] private List<DialogueConversation> conversations;
    [SerializeField] private DialogueConversation currentConvo;

    public static DialogueManager Instance;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayDialogue(DialogueConversation.ConversationName conversationName)
    {
        if (TryGetConversationByName(conversationName, out DialogueConversation convo))
        {
            currentConvo = convo;
        }
    }

    private bool TryGetConversationByName(DialogueConversation.ConversationName inputName, out DialogueConversation convo)
    {
        var correctConvoList = conversations.Where(p => p.thisConversationName == inputName).ToArray();
        convo = null;
        if (correctConvoList.Length <= 0) return false;
        convo = correctConvoList[0];
        return true;
    }
}
