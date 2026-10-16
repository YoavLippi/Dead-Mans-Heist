using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Conditions/Inverse Flag")]
public class InverseFlagCondition : DialogueCondition
{
    [SerializeField] private FlagCondition baseCondition;
    public override bool Evaluate()
    {
        return !baseCondition.Evaluate();
    }
}
