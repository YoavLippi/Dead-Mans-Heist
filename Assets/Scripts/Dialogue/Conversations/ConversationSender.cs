using UnityEngine;
using UnityEngine.Serialization;

public class ConversationSender : MonoBehaviour
{
    [FormerlySerializedAs("Conversations")] public DialogueConversation[] conversations;

    public void SendConversation(int index = 0)
    {
        //Debug.Log("Sending conversation");
        DialogueManager.Instance?.PlayDialogue(conversations[index]);
    }

    public void SelectAndSendConversation()
    {
        DialogueManager.Instance?.PlayDialogue(conversations[SelectConversation()]);
    }
    
    private int SelectConversation()
    {
        for (var i = 0; i < conversations.Length; i++)
        {
            if (IsValid(conversations[i]))
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
