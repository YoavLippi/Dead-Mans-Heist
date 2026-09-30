using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ToolHandler : MonoBehaviour
{
    [SerializeReference] private List<GameObject> toolsArr = new List<GameObject>();
    private List<GameObject> instancedToolsArr = new List<GameObject>();
    [SerializeField] private int selectedIndex;
    [SerializeField] private GameObject selectedTool;

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
            instancedToolsArr.Add(Instantiate(tool));
        }
        SelectedIndex = 0;
    }

    public void AddTool(GameObject t)
    {
        if (t.TryGetComponent(out Tool tool))
        {
            toolsArr.Add(t);
            instancedToolsArr.Add(Instantiate(t));
        }
    }

    public void RemoveTool(GameObject t)
    {
        toolsArr.Remove(t);
        instancedToolsArr.Remove(t);
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
    }

    public void ClearToolbelt()
    {
        toolsArr.Clear();
        instancedToolsArr.Clear();
    }

    public void ScrollDown()
    {
        SelectedIndex++;
        SelectedIndex %= toolsArr.Count;
        //the open hand can always be used so this shouldn't be able to loop infinitely
        if (!instancedToolsArr[selectedIndex].GetComponent<Tool>().CanBeUsed()) ScrollDown();
    }

    public void ScrollUp()
    {
        SelectedIndex--;
        if (SelectedIndex < 0) SelectedIndex = toolsArr.Count - 1;
        if (!instancedToolsArr[selectedIndex].GetComponent<Tool>().CanBeUsed()) ScrollUp();
    }

    public void UseSelected()
    {
        instancedToolsArr[selectedIndex].GetComponent<Tool>().Use();
    }
}
