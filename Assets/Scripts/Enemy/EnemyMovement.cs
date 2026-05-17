using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
public class EnemyMovement : EnemyAbs
{  
     public NavMeshAgent nav;
    public int index;
    void OnEnable()
    {
        WorldTime.secondsChange += CheckTime;
    }
  void OnDisable()
    {
        WorldTime.secondsChange -= CheckTime;
    }

    private void Update()
    {
        if (CurrentMoveMode == EnemyMoveMode.Chasing)
        {
            nav.SetDestination(this.transform.position);
        }
    }

    
    protected override void CheckTime(int currentworld) 
    {
        if (index >= newSchedules.Count) 
        {
            return;
        }
        newEvents next = newSchedules[index];
        if (currentworld >= next.timeTrigger) 
        {
            //if (currentMoveMode == EnemyMoveMode.Chasing) return;
            if (!isOnSchedule) return;
            
            next.attachedEvent.Invoke();
            index++;
        }
    }

    public override void moveToCheckPoint(Transform target) 
    {
       if(target !=null && nav != null) 
        {
           
            nav.SetDestination(target.position);
        }
    }

    
}
