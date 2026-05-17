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
public abstract class EnemyAbs : MonoBehaviour
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
    protected void HandleStateChange(EnemyMoveMode val){}
    protected virtual void CheckTime(int currentworld) { }
    public virtual void moveToCheckPoint(Transform target) { }


}

