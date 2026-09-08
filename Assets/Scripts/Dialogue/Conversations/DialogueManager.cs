using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI nameBox;
    [SerializeField] private TextMeshProUGUI dialogueBox;
    [SerializeField] private GameObject dialoguePanel;
    [Header("Data")]
    [SerializeField] private List<DialogueConversation> conversations;
    [SerializeField] private DialogueConversation currentConvo;
    [SerializeField] private int currentConvoIndex;
    [SerializeField] private DialogueConversation.Snippet currentSnippet;

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
        if (dialoguePanel.activeSelf) return;
        dialoguePanel.SetActive(true);
        if (TryGetConversationByName(conversationName, out DialogueConversation convo))
        {
            currentConvo = convo;
        }

        if (currentConvo == null) return;
        if (TryGetSnippetByID("Start", out DialogueConversation.Snippet s))
        {
            SetDialogue(s);
        }
    }

    public void PlayDialogue(DialogueConversation d)
    {
        if (dialoguePanel.activeSelf) return;
        dialoguePanel.SetActive(true);
        currentConvo = d;
        if (TryGetSnippetByID("Start", out DialogueConversation.Snippet s))
        {
            SetDialogue(s);
        }
    }

    private void SetDialogue(DialogueConversation.Snippet s)
    {
        currentSnippet = s;
        dialogueBox.text = s.text;
        nameBox.text = s.speakerName;
    }

    public void LoadNextSnippet()
    {
        //pageToDisplay is 1-counting
        if (dialogueBox.pageToDisplay < dialogueBox.textInfo.pageCount)
        {
            dialogueBox.pageToDisplay++;
            return;
        }
        if (TryGetSnippetByID(currentSnippet.nextBox, out DialogueConversation.Snippet s))
        {
            SetDialogue(s);
        }
        else
        {
            //if we can't find the id it points to, we need to exit anyway
            //if (currentSnippet.nextBox.Equals("Next", StringComparison.OrdinalIgnoreCase))
            //{
                //exit the dialogue
                Debug.Log("That was the end");
                dialoguePanel.SetActive(false);
            //}
        }
    }

    private bool TryGetConversationByName(DialogueConversation.ConversationName inputName, [CanBeNull] out DialogueConversation outConvo)
    {
        var correctConvoList = conversations.Where(p => p.thisConversationName == inputName).ToArray();
        outConvo = null;
        if (correctConvoList.Length <= 0) return false;
        outConvo = correctConvoList[0];
        return true;
    }

    private bool TryGetSnippetByID(string ID, out DialogueConversation.Snippet outSnippet)
    {
        var snippetList = currentConvo.conversation.Where(p => p.ID == ID).ToArray();
        //outSnippet = null;
        outSnippet = new DialogueConversation.Snippet();
        if (snippetList.Length <= 0) return false;
        outSnippet = snippetList[0];
        return true;
    }
}
