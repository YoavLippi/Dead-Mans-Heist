using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Conditions/Random Chance")]
public class RandomChanceCondition : DialogueCondition
{
    [SerializeField] private int percentChance;
    public override bool Evaluate()
    {
        //random number btwn 1 & 100
        int chanceEval = Random.Range(1, 101);
        return chanceEval <= percentChance;
    }
}