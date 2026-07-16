using UnityEngine;
using UnityEngine.AI;
public class EnemyMovement : EnemyAbs
{  
    [SerializeField] private NavMeshAgent nav;
    [SerializeField] private int index;
    [SerializeField] private GameObject target;
    [SerializeField] private bool isChasing;
    
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
        if (currentSuspicion == maxSuspicion) 
        {
            isChasing = true;
            //gonna use this for searching around
        }
        else 
        {
            isChasing = false;
        }
        if (attention == 0)
        {
            nav.ResetPath();
            currentMoveMode = EnemyMoveMode.Idle;

            //then maybe we stop suspicion at 50% for like another timer?
            //method to search around?
           
        }

        if (CurrentMoveMode == EnemyMoveMode.Chasing)
        {
            nav.SetDestination(target.transform.position);
            if (!attachedLos.IsSeeingPlayer) 
            {
                attention = Mathf.Max(0, attention - Time.deltaTime);
     
            }
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
