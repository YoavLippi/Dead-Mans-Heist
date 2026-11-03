using System;
using TMPro;
using UnityEngine;

public class WorldTime : MonoBehaviour
{
    public static event Action<int> secondsChange;

    [SerializeField] private float seconds = 1f;
   [SerializeField] private int currentSecond = 0;
    private float timer = 0f;

    void FixedUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= seconds) 
        {
            timer -= seconds;
            currentSecond++;
            secondsChange?.Invoke(currentSecond);
        }
    }
}
