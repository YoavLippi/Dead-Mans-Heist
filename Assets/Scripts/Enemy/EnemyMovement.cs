using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class EnemyMovement : EnemyAbs
{  
    [SerializeField] private NavMeshAgent nav;
    [SerializeField] private int index;
    [SerializeField] private GameObject target;
    [SerializeField] private bool isChasing;
    [SerializeField] private bool islookingAround;
    int lastknownWorldTime;

    //this is for like the turn stuffs
    [SerializeField] private float turnAngle = 60f;
    [SerializeField] private float speed = 4f;
    [SerializeField] private float waitTime = 0.5f;
    Quaternion startingPos;
    
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
        handleLostAttention();
        if (CurrentMoveMode == EnemyMoveMode.Chasing)
        {
            nav.SetDestination(target.transform.position);
            if (!attachedLos.IsSeeingPlayer) 
            {
                attention = Mathf.Max(0, attention - Time.deltaTime);
     
            }
        }
        

    }
    public void handleLostAttention()
    {
        if (attention != 0) return;
        if (islookingAround) return;

        nav.ResetPath();
        currentMoveMode = EnemyMoveMode.Idle;
        StartCoroutine(LookSequence()); 
    }

    
    protected override void CheckTime(int currentworld) 
    {
        lastknownWorldTime = currentworld;
        if (index >= newSchedules.Count) 
        {
            return;
        } 
        if (!isOnSchedule) return;
        newEvents next = newSchedules[index];
        if (currentworld >= next.timeTrigger) 
        {
            //if (currentMoveMode == EnemyMoveMode.Chasing) return;
           
            
            next.attachedEvent.Invoke();
            index++;
        }
    }

    public void FastForward()
    {
        while (index < newSchedules.Count && lastknownWorldTime >= newSchedules[index].timeTrigger) 
        {
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


    public IEnumerator LookSequence() 
    {

        islookingAround = true;
        startingPos = gameObject.transform.rotation;
        Quaternion leftRotation = startingPos * Quaternion.Euler(0, -turnAngle, 0);
        Quaternion rightRotation = startingPos * Quaternion.Euler(0, turnAngle, 0);

        yield return StartCoroutine(RotateTo(leftRotation));
        yield return new WaitForSeconds(waitTime);

        yield return StartCoroutine(RotateTo(rightRotation));
        yield return new WaitForSeconds(waitTime);

        yield return StartCoroutine(RotateTo(startingPos));
        FastForward();
        islookingAround = false;
        isOnSchedule = true;

    }
    public IEnumerator RotateTo(Quaternion targetRot) 
    {
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f) 
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;

    }

    
}
