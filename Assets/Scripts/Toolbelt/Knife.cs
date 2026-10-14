using UnityEngine;

public class Knife : Tool
{
    public override void Use()
    {
        if (!CanBeUsed()) return;
        if (interactionHandler.TryGetClosestInteractable(out Interactable i))
        {
            if (i.HasConditionalEventOfType(ThisInteractionType))
            {
                Charges--;
                interactionHandler.DoInteract(ThisInteractionType);
            }
        }
    }
}
