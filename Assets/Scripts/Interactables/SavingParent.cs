using UnityEngine;

public class SavingParent : MonoBehaviour
{
    [SerializeField] protected bool hasSaved = false;
    [SerializeField] protected bool saveOneshot;

    public void CallSave()
    {
        if (saveOneshot && hasSaved) return;
        WorldController.DoSave();
    }

    public void CallLoad()
    {
        WorldController.DoLoad();
    }
}
