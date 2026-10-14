using TMPro;
using UnityEngine;

public class HotbarSlotController : MonoBehaviour
{
    [SerializeField] private string attachedToolID;
    [SerializeField] private TextMeshProUGUI usageIndicator;

    public string AttachedToolID
    {
        get => attachedToolID;
        set => attachedToolID = value;
    }

    public void UpdateUsageIndicator(int value)
    {
        if (value<0)
        {
            usageIndicator.gameObject.SetActive(false);
            return;
        }

        usageIndicator.text = ""+value;
    }
}
