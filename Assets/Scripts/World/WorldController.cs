using System;
using Unity.VisualScripting;
using UnityEngine;

public class WorldController
{
    public static event Action onSave;
    public static event Action onLoad;

    public static void DoSave()
    {
        onSave?.Invoke();
    }

    public static void DoLoad()
    {
        onLoad?.Invoke();  
    }
}
