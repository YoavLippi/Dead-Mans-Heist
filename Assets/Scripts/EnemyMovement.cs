using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
public class EnemyMovement : Enemy
{  
     public NavMeshAgent nav;
   

    [System.Serializable]
    public struct worldEvents 
    {
        public int timeTrigger;
        public Transform checkPointLocation;
    }
    public int index;
    public List<worldEvents> schedules;

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
        if (currentMoveMode == EnemyMoveMode.Chasing)
        {
            nav.SetDestination(this.transform.position);
        }
        //else if()
        //{
        //    worldEvents next = schedules[index];
        //    moveToCheckPoint(next.checkPointLocation);
        //}
    }

    public void checkTime(int currentworld) 
    {
        if (index >= schedules.Count) 
        {
           
            return;
        }
        worldEvents next = schedules[index];
        if (currentworld >= next.timeTrigger) 
        {
            if (currentMoveMode == EnemyMoveMode.Chasing) return;
                
            moveToCheckPoint(next.checkPointLocation);
            index++;
        }

            
         
    }

    private void moveToCheckPoint(Transform target) 
    {
       if(target !=null && nav != null) 
        {
           
            nav.SetDestination(target.position);
        }
    }
}
