using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Conditions/Flag")]
public class FlagCondition : DialogueCondition
{
    [SerializeField] private string flagName;
    
    public override bool Evaluate()
    {
        return DialogueManager.Instance.IsFlagSet(flagName);
    }
}
