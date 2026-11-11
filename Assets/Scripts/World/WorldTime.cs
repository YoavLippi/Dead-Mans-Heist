using System;
using System.Collections;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class WorldTime : MonoBehaviour, iSaveable
{
    public static event Action<int> secondsChange;

    [SerializeField] private float seconds = 1f;
    [SerializeField] private int currentSecond = 0;
    private float timer = 0f;

    private void Start()
    {
        StartCoroutine(DoLateStart());
    }

    private IEnumerator DoLateStart()
    {
        yield return new WaitForEndOfFrame();
        WorldController.DoSave();
    }

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

    #region Saving

    [Header("Saving")] 
    [SerializeField] private int savedTime;
    
    private void OnEnable()
    {
        ((iSaveable)this).OnEnable();
    }

    private void OnDisable()
    {
        ((iSaveable)this).OnDisable();
    }

    public void DoSave()
    {
        savedTime = currentSecond;
    }
    
    public void DoLoad()
    {
        currentSecond = savedTime;
        secondsChange?.Invoke(savedTime);
    }
    
    #endregion
}
