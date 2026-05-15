using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyMovement : MonoBehaviour
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
 
    public void checkTime(int currentworld) 
    {
        if (index > schedules.Count) return;
        worldEvents next = schedules[index];
        if (currentworld >= next.timeTrigger) 
        {
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
