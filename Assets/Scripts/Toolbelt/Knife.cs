using UnityEngine;

public class Knife : Tool
{
    public override void Use()
    {
        if (!CanBeUsed()) return;
        Charges--;
        Debug.Log("Stabby stabby arhghgg " + Charges);
    }
}
