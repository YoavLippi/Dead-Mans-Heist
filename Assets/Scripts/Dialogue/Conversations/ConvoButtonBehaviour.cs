using TMPro;
using UnityEngine;

public class ConvoButtonBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI optionText;
    public DialogueConversation.Option thisOption;

    public void InitialiseOption(DialogueConversation.Option newOption)
    {
        thisOption = newOption;
        optionText.text = newOption.text;
    }

    public void SendChoice()
    {
        DialogueManager.Instance?.LoadNextSnippet(thisOption);
    }
}
