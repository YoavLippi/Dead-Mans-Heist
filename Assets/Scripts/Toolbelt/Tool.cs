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

    public virtual void Use() {}
    public virtual void HandleSelection(bool val){}

    public bool CanBeUsed()
    {
        if (!usable) return false;
        if (Charges == 0) return false;
        return true;
    }
}
