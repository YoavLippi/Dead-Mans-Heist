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
    
    //NB: these should be in the game manager, just here for now for testing
    [SerializeField] private List<Flag> flags;
    [SerializeField] private Quest[] quests;
    
    [Serializable]
    public struct Flag
    {
        public Flag(string name, bool isSet)
        {
            this.name = name;
            this.isSet = isSet;
        }
        public string name;
        public bool isSet;
    }

    [Serializable]
    public struct Quest
    {
        public string questID;
        public bool isComplete;
    }

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
        HandleActions(o.actions);
        //o.optionEvent.Invoke();
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

        //currentSnippet.snippetCompletionEvent.Invoke();
        //now assuming it is a snippet
        HandleActions(currentSnippet.snippetCompletionActions);
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

    public void HandleActions(DialogueConversation.DialogueAction[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            switch (actions[i].type)
            {
                case DialogueConversation.DialogueActionType.None:
                    break;
                case DialogueConversation.DialogueActionType.StartQuest:
                    break;
                case DialogueConversation.DialogueActionType.CompleteQuest:
                    break;
                case DialogueConversation.DialogueActionType.GiveItem:
                    break;
                case DialogueConversation.DialogueActionType.RemoveItem:
                    break;
                case DialogueConversation.DialogueActionType.SetFlag:
                    //set gameController state control stuff here
                    //"I learned LINQ, can you tell?" ahh condition
                    var matchingFlagArr = flags.Where(p => p.name == actions[i].parameter).ToArray();
                    if (matchingFlagArr.Length!=0)
                    {
                        matchingFlagArr[0].isSet = Boolean.Parse(actions[i].value);
                    }
                    else
                    {
                        Flag newFlag = new Flag(actions[i].parameter, Boolean.Parse(actions[i].value));
                        flags.Add(newFlag);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }   
    }

    public bool IsFlagSet(string flag)
    {
        var a = flags.Where(p => p.name.Equals(flag,StringComparison.OrdinalIgnoreCase)).ToArray();
        if (a.Length != 0)
        {
            return a[0].isSet;
        }

        return false;
    }

    public bool IsQuestComplete(string qID)
    {
        var a = quests.Where(p => p.questID == qID).ToArray();
        if (a.Length !=0)
        {
            return a[0].isComplete;
        }

        return false;
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
