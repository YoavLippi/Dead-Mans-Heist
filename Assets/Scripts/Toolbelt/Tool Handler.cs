using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public class ToolHandler : MonoBehaviour
{
    [SerializeReference] private List<GameObject> toolsArr = new List<GameObject>();
    [SerializeField] private int selectedIndex;
    [SerializeField] private GameObject selectedTool;
    [SerializeField] private InteractionHandler interactionHandler;
    [SerializeField] private GameObject hotbarParent;
    
    [Header("Runtime Only")]
    public List<GameObject> instancedToolsArr = new List<GameObject>();

    private int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value>-1&&value<instancedToolsArr.Count) selectedTool = instancedToolsArr[value];
            selectedIndex = value;
        }
    }

    public GameObject SelectedTool => selectedTool;

    private void Start()
    {
        foreach (var tool in toolsArr)
        {
            GameObject temp = Instantiate(tool, transform);
            temp.GetComponent<SpriteRenderer>().enabled = false;
            instancedToolsArr.Add(temp);
        }
        SelectedIndex = 0;
        //hotbarParent = GameObject.FindWithTag("Hotbar");
        //InitialiseSlots();
        //SetupVisualsAfterDelay(250);
    }

    private void InitialiseSlots()
    {
        ToolbeltVisualHandler tb = hotbarParent.GetComponent<ToolbeltVisualHandler>();
        tb.SetupSlots(toolsArr.ToArray());
        SetupVisuals();
        selectedTool.GetComponent<Tool>().IsSelected = true;
    }

    private void SetupVisuals()
    {
        ToolbeltVisualHandler tb = hotbarParent.GetComponent<ToolbeltVisualHandler>();
        for (int i = 0; i < toolsArr.Count; i++)
        {
            tb.SetSlotImage(i, toolsArr[i].GetComponent<SpriteRenderer>().sprite);   
        }
        SetHotbarVisualPos();
    }

    private async void SetupVisualsAfterDelay(int milis)
    {
        await Task.Delay(milis);
        SetHotbarVisualPos();
    }
    
    private void SetHotbarVisualPos()
    {
        hotbarParent.GetComponent<ToolbeltVisualHandler>().CurrentSelection = selectedIndex;
    }

    public void AddTool(GameObject t)
    {
        if (t.TryGetComponent(out Tool tool))
        {
            toolsArr.Add(t);
            instancedToolsArr.Add(Instantiate(t));
            InitialiseSlots();
        }
    }

    public void RemoveTool(GameObject t)
    {
        toolsArr.Remove(t);
        instancedToolsArr.Remove(t);
        InitialiseSlots();
    }

    public void RemoveTool(string toolID)
    {
        foreach (var tool in toolsArr)
        {
            if (tool.GetComponent<Tool>().ToolID.Equals(toolID))
            {
                toolsArr.Remove(tool);
                return;
            }
        }
        foreach (var tool in instancedToolsArr)
        {
            if (tool.GetComponent<Tool>().ToolID.Equals(toolID))
            {
                instancedToolsArr.Remove(tool);
                return;
            }
        }
        InitialiseSlots();
    }

    public void ClearToolbelt()
    {
        toolsArr.Clear();
        instancedToolsArr.Clear();
        InitialiseSlots();
    }

    public void ScrollDown()
    {
        if (Time.timeScale == 0) return;
        instancedToolsArr[selectedIndex].GetComponent<Tool>().IsSelected = false;
        SelectedIndex++;
        SelectedIndex %= toolsArr.Count;
        UpdateInteractionHandler();
        //SetHotbarVisualPos();
        instancedToolsArr[selectedIndex].GetComponent<Tool>().IsSelected = true;
        //the open hand can always be used so this shouldn't be able to loop infinitely
        if (!instancedToolsArr[SelectedIndex].GetComponent<Tool>().CanBeUsed()) ScrollDown();
    }

    public void ScrollUp()
    {
        if (Time.timeScale == 0) return;
        instancedToolsArr[selectedIndex].GetComponent<Tool>().IsSelected = false;
        SelectedIndex--;
        if (SelectedIndex < 0) SelectedIndex = toolsArr.Count - 1;
        UpdateInteractionHandler();
        //SetHotbarVisualPos();
        instancedToolsArr[selectedIndex].GetComponent<Tool>().IsSelected = true;
        if (!instancedToolsArr[selectedIndex].GetComponent<Tool>().CanBeUsed()) ScrollUp();
    }

    public void UpdateInteractionHandler()
    {
        Tool currentTool = toolsArr[SelectedIndex].GetComponent<Tool>();
        interactionHandler.CurrentInteractionType = currentTool.ThisInteractionType;
    }

    public void UseSelected()
    {
        Tool currentTool = selectedTool.GetComponent<Tool>();
        currentTool.Use();
        //hotbarParent.GetComponent<ToolbeltVisualHandler>().SetSlotCharge(currentTool.ToolID,currentTool.Charges);
        //if (!currentTool.CanBeUsed()) ScrollDown();
    }
}
