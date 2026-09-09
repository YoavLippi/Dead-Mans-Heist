using UnityEngine;

public class ConversationSender : MonoBehaviour
{
    public DialogueConversation[] Conversations;

    public void SendConversation(int index = 0)
    {
        DialogueManager.Instance?.PlayDialogue(Conversations[index]);
    }
}
