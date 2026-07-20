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
    [SerializeField] private AnimationCurve suspicionFalloffCurve;

    //this is for like the turn stuffs
    [SerializeField] private float turnAngle = 60f;
    [SerializeField] private float speed = 4f;
    [SerializeField] private float waitTime = 0.3f;
    Quaternion startingPos;
    
    void OnEnable()
    {
        WorldTime.secondsChange += CheckTime;
    }

    void OnDisable()
    {
        WorldTime.secondsChange -= CheckTime;
    }

    public override void Start()
    {
        base.Start();
        target = GameObject.FindWithTag("Player");
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
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
        if (CurrentSuspicion < 45) return;
        islookingAround = true;

        nav.ResetPath();
        currentMoveMode = EnemyMoveMode.Idle;
        StartCoroutine(LookSequence(waitTime*3)); 
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
    public override void moveToCheckPoint(Transform target) 
    {
       if(target !=null && nav != null) 
        {
           
            nav.SetDestination(target.position);
        }
    }

    public void lookAround() 
    {
        StartCoroutine(LookSequence(3));
    }
    public IEnumerator LookSequence(float time) 
    {

        islookingAround = true;
        startingPos = gameObject.transform.rotation;
        Quaternion leftRotation = startingPos * Quaternion.Euler(0, -turnAngle, 0);
        Quaternion rightRotation = startingPos * Quaternion.Euler(0, turnAngle, 0);

        yield return StartCoroutine(RotateTo(leftRotation));
        yield return new WaitForSeconds(time/3);

        yield return StartCoroutine(RotateTo(rightRotation));
        yield return new WaitForSeconds(time/3);

        yield return StartCoroutine(RotateTo(startingPos));
        //FastForward();
        islookingAround = false;
        

    }
    public IEnumerator RotateTo(Quaternion targetRot) 
    {
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f) 
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, speed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;

    }

    public IEnumerator distractTime(float time, Transform pos, float suspicion) 
    {
        CurrentSuspicion = suspicion;
        do
        {
            yield return new WaitForEndOfFrame();

        }
        while (Vector3.Distance(transform.position, pos.position)> 2f);

        nav.SetDestination(transform.position);

        yield return StartCoroutine(LookSequence(time));
        isDistracted = false;
        while (CurrentSuspicion > 0) 
        {
            yield return new WaitForEndOfFrame();
        }
       
        isOnSchedule = true;

    }

    public override void GetDistracted(Transform distractionPos, DistractionHandler.DistractionSeverity sev)
    {

        //need to have something for enemies to target area rather than transform
        Debug.Log($"{name} was distracted, pos: {distractionPos.position}, severity {sev.ToString()}");
        switch (sev)
        {
            case DistractionHandler.DistractionSeverity.Severe:
                isOnSchedule = false;
                moveToCheckPoint(distractionPos);
                isDistracted = true;
                StartCoroutine(distractTime(5, distractionPos, 75));
                
                break;
            case DistractionHandler.DistractionSeverity.Moderate:
                isOnSchedule = false;
                moveToCheckPoint(distractionPos);
                isDistracted = true;
                StartCoroutine(distractTime(3, distractionPos, 50));
                //get distracted for 5 seconds
                break;
        }
    }




}
