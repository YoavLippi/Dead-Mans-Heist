using UnityEngine;

public abstract class Tool : MonoBehaviour
{
    [SerializeField] protected string _toolID;
    [SerializeField] protected int _charges;
    [SerializeField] protected bool usable;

    public string ToolID => _toolID;

    public int Charges
    {
        get => _charges;
        set => _charges = value;
    }

    public virtual void Use()
    {
        if (!usable) return;
        if (Charges == 0) return;
    }

    public bool CanBeUsed()
    {
        if (!usable) return false;
        if (Charges == 0) return false;
        return true;
    }
}
