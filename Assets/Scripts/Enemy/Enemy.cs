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
public class EnemyAbs : MonoBehaviour
{
    [System.Serializable]
    public struct newEvents
    {
        public int timeTrigger;
        public UnityEvent attachedEvent;
    }

    public List<newEvents> newSchedules;
    [SerializeField] protected bool isOnSchedule;
    [SerializeField] protected LosHandler attachedLos;

    [SerializeField] protected float detectionSpeed = 0.6f;
    [SerializeField] protected float suspicionDecreaseSpeed = 0.3f;
    [SerializeField] protected float currentSuspicion;
    [SerializeField] protected float maxSuspicion = 100f;
    [SerializeField] protected EnemyMoveMode currentMoveMode;
    [SerializeField] protected EnemyState currentState;
    [SerializeField] protected DetectionUX detectUX;
    [SerializeField] protected Gradient gradient;
    [SerializeField] protected float attention = 5;
    [SerializeField] protected bool isDistracted = false;

    public float CurrentSuspicion
    {
        get => currentSuspicion;
        set
        {   
            currentSuspicion = value;
            detectUX.UpdateUXState(value);
            attachedLos.SetSightColour(gradient.Evaluate(value/maxSuspicion));
            
        }
    }

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

    public EnemyState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }

    void Start()
    {
        attachedLos.OnSeePlayer += OnSeePlayer;
        CurrentSuspicion = 0;
    }

    public virtual void FixedUpdate()
    {
        if (!isDistracted)
        {
            if (currentMoveMode != EnemyMoveMode.Chasing && currentSuspicion > 0f)
            {
                CurrentSuspicion = Mathf.Max(0, currentSuspicion - suspicionDecreaseSpeed);

            }
        }
        if (currentSuspicion == 0 && attention == 0) 
        {
            isOnSchedule = true;
            currentMoveMode = EnemyMoveMode.Patrolling;
            attention = 5;
        }
       
    }

    protected void HandleStateChange(EnemyMoveMode val){}
    protected virtual void CheckTime(int currentworld) { }
    public virtual void moveToCheckPoint(Transform target) { }

    public virtual void GetDistracted(Transform distractionPos, DistractionHandler.DistractionSeverity sev)
    {
        
        
    }

    private void OnSeePlayer()
    {
        CurrentSuspicion = Mathf.Min(currentSuspicion + detectionSpeed, maxSuspicion);
        if (currentSuspicion >= maxSuspicion && currentMoveMode != EnemyMoveMode.Chasing)
        {
            currentMoveMode = EnemyMoveMode.Chasing;
            isOnSchedule = false;
            attention = 5;

        }
    }
}

