using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class ConversationSender : MonoBehaviour
{
    [FormerlySerializedAs("Conversations")] public DialogueConversation[] conversations;

    private DialogueConversation[] GetOrderedConvos()
    {
        return conversations.OrderByDescending(p => p.conditions.Count).ToArray();
    }

    public void SendConversation(int index = 0)
    {
        //Debug.Log("Sending conversation");
        DialogueManager.Instance.PlayDialogue(conversations[index]);
    }

    public void SelectAndSendConversation()
    {
        DialogueManager.Instance?.PlayDialogue(GetOrderedConvos()[SelectConversation()]);
    }
    
    private int SelectConversation()
    {
        var tempConversations = GetOrderedConvos();
        for (var i = 0; i < tempConversations.Length; i++)
        {
            if (IsValid(tempConversations[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsValid(DialogueConversation convo)
    {
        foreach (var condition in convo.conditions)
        {
            if (!condition.Evaluate())
            {
                return false;
            }
        }

        return true;
    }
}
