using UnityEngine;

public class ConversationSender : MonoBehaviour
{
    public DialogueConversation[] Conversations;

    public void SendConversation(int index = 0)
    {
        //Debug.Log("Sending conversation");
        DialogueManager.Instance?.PlayDialogue(Conversations[index]);
    }
}
