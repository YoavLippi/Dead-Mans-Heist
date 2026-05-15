using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
public class EnemyMovement : Enemy
{  
     public NavMeshAgent nav;
   

    /*[System.Serializable]
    public struct worldEvents 
    {
        public int timeTrigger;
        public Transform checkPointLocation;
    }*/
    public int index;
    //public List<worldEvents> schedules;

    void OnEnable()
    {
        WorldTime.secondsChange += checkTime;
    }
  void OnDisable()
    {
        WorldTime.secondsChange -= checkTime;
    }

    private void Update()
    {
        if (CurrentMoveMode == EnemyMoveMode.Chasing)
        {
            nav.SetDestination(this.transform.position);
        }
    }

    private void checkTime(int currentworld) 
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

    public void moveToCheckPoint(Transform target) 
    {
       if(target !=null && nav != null) 
        {
           
            nav.SetDestination(target.position);
        }
    }
}
