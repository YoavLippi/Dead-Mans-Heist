using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueManager : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI nameBox;
    [SerializeField] private TextMeshProUGUI dialogueBox;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject optionPanelContent;
    [SerializeField] private GameObject dialogueOptionPrefab;
    
    [SerializeField] private bool isDisplayingOptions;
    [Header("Data")]
    //[SerializeField] private List<DialogueConversation> conversations;
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

    /*public void PlayDialogue(DialogueConversation.ConversationName conversationName)
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
    }*/

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
        //Debug.Log("Populating dialogue");
        currentSnippet = s;

        optionPanel.SetActive(s.hasOptions);
        isDisplayingOptions = s.hasOptions;
        if (s.hasOptions)
        {
            //clear children
            foreach (var child in optionPanelContent.GetComponentsInChildren<Transform>())
            {
                if (child == optionPanelContent.transform) continue;
                Destroy(child.gameObject);
            }
            
            //populate
            foreach (var option in s.options)
            {
                GameObject temp = Instantiate(dialogueOptionPrefab, optionPanelContent.transform);
                if (temp.TryGetComponent<ConvoButtonBehaviour>(out var component))
                {
                    component.InitialiseOption(option);
                }
            }
        }
        dialogueBox.text = s.text;
        nameBox.text = s.speakerName;
    }

    public void LoadNextSnippet(DialogueConversation.Option o)
    {
        if (TryGetSnippetByID(o.nextBox, out DialogueConversation.Snippet s))
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

    public void LoadNextSnippet()
    {
        if (isDisplayingOptions) return;
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

    /*private bool TryGetConversationByName(DialogueConversation.ConversationName inputName, [CanBeNull] out DialogueConversation outConvo)
    {
        var correctConvoList = conversations.Where(p => p.thisConversationName == inputName).ToArray();
        outConvo = null;
        if (correctConvoList.Length <= 0) return false;
        outConvo = correctConvoList[0];
        return true;
    }*/

    private bool TryGetSnippetByID(string ID, out DialogueConversation.Snippet outSnippet)
    {
        var snippetList = currentConvo.conversation.Where(p => p.ID == ID).ToArray();
        //outSnippet = null;
        outSnippet = new DialogueConversation.Snippet();
        if (snippetList.Length <= 0 && !ID.Equals("Exit",StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError($"Couldn't find a message with the ID {ID}");
            return false;
        }
        
        if (ID.Equals("Exit",StringComparison.OrdinalIgnoreCase)) 
        {
            return false;
        }
        outSnippet = snippetList[0];
        return true;
    }
}
