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

[CreateAssetMenu(menuName = "Dialogue/Conditions/Quest Complete")]
public class QuestCompleteCondition : DialogueCondition
{
    [SerializeField] private string questID;
    public override bool Evaluate()
    {
        return DialogueManager.Instance.IsQuestComplete(questID);
    }
}

[CreateAssetMenu(menuName = "Dialogue/Conditions/Random Chance")]
public class RandomChance : DialogueCondition
{
    [SerializeField] private int percentChance;
    public override bool Evaluate()
    {
        //random number btwn 1 & 100
        int chanceEval = Random.Range(1, 101);
        return chanceEval <= percentChance;
    }
}
