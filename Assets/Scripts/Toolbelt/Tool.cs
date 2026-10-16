using System;
using UnityEngine;

public abstract class Tool : MonoBehaviour
{
    [SerializeField] protected string _toolID;
    [SerializeField] protected int _charges;
    [SerializeField] protected bool usable;
    public InteractionHandler interactionHandler;
    [SerializeField] protected bool _interactsWithEntities;
    [SerializeField] protected ConditionInteraction _interactionType;
    [SerializeField] protected bool isSelected;
    public static event Action<Tool> OnToolUsed;
    

    public string ToolID => _toolID;
    public ConditionInteraction ThisInteractionType => _interactionType;
    public bool InteractsWithEntities => _interactsWithEntities;

    public bool IsSelected
    {
        get=>isSelected;
        set
        {
            isSelected=value;
            HandleSelection(value);
        }
    }
    
    private void Start()
    {
        if (!interactionHandler) interactionHandler = GameObject.FindWithTag("Interaction").GetComponentInChildren<InteractionHandler>();
    }

    public int Charges
    {
        get => _charges;
        set => _charges = value;
    }

    /*
    Sample code for listening to OnToolUsed:
     
     The event is a public static delegate, so you need to reference the tool class
    private void OnEnable()
    {
        Tool.OnToolUsed += PrintTestMessage;
    }

    private void OnDisable()
    {
        Tool.OnToolUsed -= PrintTestMessage;
    }

    To disambiguate, the event sends a Tool object with the event call, so use that to figure out what it was
    private void PrintTestMessage(Tool t)
    {
        Debug.Log(t.ToolID+" just got used");
        //for a switch it could be
        switch (t.ToolID) {
            case "Coin":
                //stuff
                break;
            case "Hand":
                //stuff
                break;
            etc...
        }
    }
     */
    
    public virtual void Use()
    {
        OnToolUsed?.Invoke(this);
    }
    public virtual void HandleSelection(bool val){}

    public bool CanBeUsed()
    {
        if (!usable) return false;
        if (Charges == 0) return false;
        if (Time.timeScale == 0) return false;
        return true;
    }
}
