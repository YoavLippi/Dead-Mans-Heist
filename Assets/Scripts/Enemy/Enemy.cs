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
    Distracted,
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
    [SerializeField] protected float attention = 5f;
    [SerializeField] protected float defaultAttention = 5f;

    // Was a bare bool before; now derived from state so it can't drift
    // out of sync with CurrentState.
    public bool isDistracted => currentState == EnemyState.Distracted;

    public float CurrentSuspicion
    {
        get => currentSuspicion;
        set
        {
            currentSuspicion = Mathf.Clamp(value, 0f, maxSuspicion);
            detectUX.UpdateUXState(currentSuspicion);
            attachedLos.SetSightColour(gradient.Evaluate(currentSuspicion / maxSuspicion));
        }
    }

    public EnemyMoveMode CurrentMoveMode
    {
        get => currentMoveMode;
        set
        {
            if (currentMoveMode == value) return;
            EnemyMoveMode previous = currentMoveMode;
            currentMoveMode = value;
            HandleStateChange(previous, value);
        }
    }

    public EnemyState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }

    public virtual void Start()
    {
        attachedLos.OnSeePlayer += OnSeePlayer;
        CurrentSuspicion = 0;
        attention = defaultAttention;
    }

    public virtual void FixedUpdate()
    {
        if (currentState != EnemyState.Distracted)
        {
            if (currentMoveMode != EnemyMoveMode.Chasing && currentSuspicion > 0f)
            {
                CurrentSuspicion = currentSuspicion - suspicionDecreaseSpeed;
            }
        }

        if (currentSuspicion == 0f && attention == 0f && currentMoveMode != EnemyMoveMode.Patrolling)
        {
            isOnSchedule = true;
            CurrentMoveMode = EnemyMoveMode.Patrolling;
            attention = defaultAttention;
        }
    }

 
    protected virtual void HandleStateChange(EnemyMoveMode previous, EnemyMoveMode next)
    {
      
    }

    protected virtual void CheckTime(int currentworld) { }

    public virtual void moveToCheckPoint(Vector3 target) { }
    public virtual void moveToCheckPoint(Transform target) { }

    public virtual void GetDistracted(Vector3 distractionPos, DistractionHandler.DistractionSeverity sev) { }

    private void OnSeePlayer()
    {
        CurrentSuspicion = currentSuspicion + detectionSpeed;
        if (currentSuspicion >= maxSuspicion && currentMoveMode != EnemyMoveMode.Chasing)
        {
            CurrentMoveMode = EnemyMoveMode.Chasing;
            isOnSchedule = false;
            attention = defaultAttention;
        }
    }
}