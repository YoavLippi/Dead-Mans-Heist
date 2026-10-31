using UnityEngine;

public class GameEnd : MonoBehaviour
{
    int lastKnownWorldTime;


    void OnEnable() => WorldTime.secondsChange += CheckTime;
    void OnDisable() => WorldTime.secondsChange -= CheckTime;
    protected void CheckTime(int currentworld)
    {
        lastKnownWorldTime = currentworld;

        if (lastKnownWorldTime > 10) 
        {
            Debug.Log("Death");
        }
    }
    }
