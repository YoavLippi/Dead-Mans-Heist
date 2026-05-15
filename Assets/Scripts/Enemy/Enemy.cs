using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public enum EnemyMoveMode 
{
    Patrolling,
    Chasing,
    Idle
   
}

[Serializable]
public enum EnemyState
{
    None,
    Stunned,
    Recovery
}

public class Enemy : MonoBehaviour
{
    [System.Serializable]
    public struct newEvents 
    {
        public int timeTrigger;
        public UnityEvent attachedEvent;
    }
    
    public List<newEvents> newSchedules;
    [SerializeField] protected bool isOnSchedule;
    
    protected EnemyMoveMode currentMoveMode;
    protected EnemyState currentState;

    public EnemyMoveMode CurrentMoveMode
    {
        get => currentMoveMode;
        //attaching listener that will fire when the property is set
        set
        {
            currentMoveMode = value;
            HandleStateChange(value);
        }
    }

    //listener handling
    private void HandleStateChange(EnemyMoveMode val)
    {
        Debug.Log($"Current move mode is {val}");
    }

    public EnemyState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }

    void Start()
    {
        
    }

    void Update()
    {
    }


}

