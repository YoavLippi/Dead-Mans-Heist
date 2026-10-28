using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

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
    [SerializeField] protected EnemyState currentState;
    [SerializeField] protected DetectionUX detectUX;
    [SerializeField] protected Gradient gradient;
    [SerializeField] protected Collider catchCollider;
    [SerializeField] protected float chaseSpeed = 6;
    [SerializeField] protected float patrolSpeed = 3.5f;
    protected NavMeshAgent nv;
    [SerializeField]protected bool isAlerted;
    private bool gameOverTriggered;
    public static event Action OnPlayerCaught;

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

    public EnemyState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
        }

    }

    public virtual void Start()
    {
        attachedLos.OnSeePlayer += OnSeePlayer;
        CurrentSuspicion = 0;
        nv = GetComponent<NavMeshAgent>();
    }

    public virtual void FixedUpdate()
    {
        if (currentState != EnemyState.Distracted && !isAlerted && currentSuspicion > 0f)
        {
            CurrentSuspicion = currentSuspicion - suspicionDecreaseSpeed;
        }
    }

    protected virtual void CheckTime(int currentworld) { }

    public virtual void moveToCheckPoint(Vector3 target) { }
    public virtual void moveToCheckPoint(Transform target) { }

    public virtual void GetDistracted(Vector3 distractionPos, DistractionHandler.DistractionSeverity sev) { }

    protected virtual void OnSeePlayer()
    {
        if (gameOverTriggered) return;
        CurrentSuspicion = currentSuspicion + detectionSpeed;
        if (currentSuspicion >= maxSuspicion)
        {
            gameOverTriggered = true;
            OnPlayerCaught?.Invoke();
        }
    }
}