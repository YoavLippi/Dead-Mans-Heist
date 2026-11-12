using TMPro;
using UnityEngine;

public class PopupHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void UpdateText(string newText, Color color)
    {
        text.text = newText;
        text.color = color;
    }
}
