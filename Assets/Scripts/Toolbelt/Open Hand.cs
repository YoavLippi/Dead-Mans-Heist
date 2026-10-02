using System;
using UnityEngine;

[Serializable]
public class OpenHand : Tool
{ 
    public override void Use()
    {
        if (!CanBeUsed()) return;
        //this can be the interaction handler link
        if (!interactionHandler) interactionHandler = GameObject.FindWithTag("Interaction").GetComponentInChildren<InteractionHandler>();
        
        if (interactionHandler)
        {
            interactionHandler.GetComponentInChildren<InteractionHandler>().DoInteract(_interactionType);
        }
    }
}
