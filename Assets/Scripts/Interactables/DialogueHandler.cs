using UnityEngine;

public class DialogueHandler : Interactable
{
    public DialogueConversation.ConversationName corrConversation;
    public override void DoInteract()
    {
        DialogueManager.Instance.PlayDialogue(corrConversation);
    }
}
