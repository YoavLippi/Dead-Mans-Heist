using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbeltVisualHandler : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject selectionBox;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject hotbarParent;
    [Header("Runtime")]
    [SerializeField] private int currentSelection;
    [SerializeField] private List<GameObject> hotbarSlots;

    public int CurrentSelection
    {
        get => currentSelection;
        set
        {
            currentSelection = value;
            if (selectionBox != null && hotbarSlots != null && hotbarSlots.Count > value)
                selectionBox.transform.position = hotbarSlots[value].transform.position;
        }
    }

    public void SetSlotImage(int index, Sprite sprite)
    {
        if (index >= 0 && index < hotbarSlots.Count)
        {
            hotbarSlots[index].GetComponent<Image>().sprite = sprite;
        }
    }

    public void SetupSlots(GameObject[] slots)
    {
        hotbarSlots.Clear();
        foreach (var slot in slots)
        {
            GameObject temp = Instantiate(slotPrefab, hotbarParent.transform);
            if (slot.TryGetComponent<Tool>(out Tool t))
            {
                temp.GetComponent<HotbarSlotController>().AttachedToolID = t.ToolID;
                temp.GetComponent<HotbarSlotController>().UpdateUsageIndicator(t.Charges);
            }
            hotbarSlots.Add(temp);
        }
    }

    public void SetSlotCharge(string toolID, int chargeVal)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot.TryGetComponent<HotbarSlotController>(out HotbarSlotController controller))
            {
                if (controller.AttachedToolID == toolID)
                {
                    controller.UpdateUsageIndicator(chargeVal);
                    return;
                }
            }
        }
    }
}
