using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : EnemyAbs
{
    [SerializeField] private int index;
    [SerializeField] private GameObject target;
    [SerializeField] private bool isLookingAround;
    private int lastKnownWorldTime;
    [SerializeField] private AnimationCurve suspicionFalloffCurve;
    [SerializeField] private int timeOffset = 0;

    [Header("Look-around turn")]
    [SerializeField] private float turnAngle = 60f;
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float lookAroundWaitTime = 0.3f;

    [Header("Attention loss")]
    [SerializeField] private DistractionHandler.DistractionSeverity currentDistractionSeverity;

    [Header("Distraction tuning")]
    [SerializeField] private float severeDistractionDuration = 5f;
    [SerializeField] private float severeDistractionSuspicion = 75f;
    [SerializeField] private float moderateDistractionDuration = 3f;
    [SerializeField] private float moderateDistractionSuspicion = 50f;
    [SerializeField] private float minorDistractionDuration = 3f;
    [SerializeField] private float minorDistractionSuspicion = 25f;


    [Header("Travelling")]
    [SerializeField] private float arrivalDistanceThreshold = 2f;
    [SerializeField] private float maxTravelToDistractionSeconds = 10f;

    [Header("Animation")]
    [SerializeField] private float facingX;
    [SerializeField] private float facingY;
    [SerializeField] private Animator anim;

    private Quaternion startingRotation;
    private Coroutine distractionRoutine;
    private Coroutine lookRoutine;
    private Coroutine waitRoutine;

    private float lastSeenTime;
    [SerializeField] private float alertTurnSpeed = 300f;

    void OnEnable() => WorldTime.secondsChange += CheckTime;
    void OnDisable() => WorldTime.secondsChange -= CheckTime;

    public override void Start()
    {
        base.Start();
        target = GameObject.FindWithTag("Player");
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        anim.SetFloat("Speed", nv.velocity.magnitude, 0.1f, Time.deltaTime);
        if (isAlerted)
        {
            Vector3 dir = target.transform.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, look, alertTurnSpeed * Time.deltaTime);
            }

            if (Time.time - lastSeenTime >= 1f)
            {
                isAlerted = false;
                nv.isStopped = false;
            }
        }
    }

    protected override void CheckTime(int currentworld)
    {
        lastKnownWorldTime = currentworld;


        if (!isOnSchedule) return;

        int localTime = currentworld - timeOffset;
        if (index >= newSchedules.Count)
        {
            timeOffset += newSchedules[newSchedules.Count - 1].timeTrigger;
            index = 0;
            return;
        }

        newEvents next = newSchedules[index];
        if (localTime >= next.timeTrigger)
        {
            next.attachedEvent.Invoke();
            index++;
        }
    }

    public override void moveToCheckPoint(Vector3 target)
    {
        if (target != null && nv != null)
        {
            nv.SetDestination(target);
        }
    }

    public override void moveToCheckPoint(Transform target)
    {
        if (target != null && nv != null)
        {
            nv.SetDestination(target.position);
        }
    }

    public void lookAround() => lookRoutine = StartCoroutine(LookSequence(3f));
    public void waitAround() => waitRoutine = StartCoroutine(WaitSequence());

    private IEnumerator LookSequence(float time)
    {
        isLookingAround = true;
        startingRotation = transform.rotation;
        Quaternion leftRotation = startingRotation * Quaternion.Euler(0, -turnAngle, 0);
        Quaternion rightRotation = startingRotation * Quaternion.Euler(0, turnAngle, 0);

        yield return StartCoroutine(RotateTo(leftRotation));
        yield return new WaitForSeconds(time / 3f);

        yield return StartCoroutine(RotateTo(rightRotation));
        yield return new WaitForSeconds(time / 3f);

        yield return StartCoroutine(RotateTo(startingRotation));

        isLookingAround = false;
        lookRoutine = null;
    }
    private IEnumerator WaitSequence()
    {
        yield return new WaitForSeconds(3f);
    }

    private IEnumerator RotateTo(Quaternion targetRot)
    {
        float maxTurnTime = 3;
        float currentTime = 0;
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f && currentTime <= maxTurnTime)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
            currentTime += Time.deltaTime;

        }

        if (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            Quaternion fallbackStart = transform.rotation;
            float fallbackDuration = 0.25f;
            float t = 0f;
            while (t < fallbackDuration)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(fallbackStart, targetRot, t / fallbackDuration);
                yield return null;
            }
        }

        transform.rotation = targetRot;
    }

    private void StopNav()
    {
        nv.ResetPath();
    }

    private IEnumerator DistractionSequence(float lookTime, Vector3 pos, float suspicion)
    {
        CurrentState = EnemyState.Distracted;
        CurrentSuspicion = suspicion;
        float elapsed = 0f;
        while (Vector3.Distance(transform.position, pos) > arrivalDistanceThreshold
               && elapsed < maxTravelToDistractionSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        StopNav();
        nv.SetDestination(transform.position);

        yield return StartCoroutine(LookSequence(lookTime));

        CurrentState = EnemyState.None;

        while (CurrentSuspicion > 0f)
        {
            yield return null;
        }

        isOnSchedule = true;
        distractionRoutine = null;
        currentDistractionSeverity = DistractionHandler.DistractionSeverity.None;
    }

    public override void GetDistracted(Vector3 distractionPos, DistractionHandler.DistractionSeverity sev)
    {
        isOnSchedule = false;
        if (sev == DistractionHandler.DistractionSeverity.None) return;
        Debug.Log($"{name} was distracted, pos: {distractionPos}, severity {sev}");
        if (distractionRoutine != null)
        {
            StopCoroutine(distractionRoutine);
        }

        switch (sev)
        {
            case DistractionHandler.DistractionSeverity.Severe:
                distractionRoutine = StartCoroutine(
                    DistractionSequence(severeDistractionDuration, distractionPos, severeDistractionSuspicion));
                moveToCheckPoint(distractionPos);

                break;

            case DistractionHandler.DistractionSeverity.Moderate:

                if (currentDistractionSeverity == DistractionHandler.DistractionSeverity.Severe)
                {
                    break;
                }
                distractionRoutine = StartCoroutine(
                    DistractionSequence(moderateDistractionDuration, distractionPos, moderateDistractionSuspicion));
                moveToCheckPoint(distractionPos);

                break;

            case DistractionHandler.DistractionSeverity.Minor:
                if (currentDistractionSeverity == DistractionHandler.DistractionSeverity.Severe || currentDistractionSeverity == DistractionHandler.DistractionSeverity.Moderate)
                {
                    break;
                }
                distractionRoutine = StartCoroutine(
                    DistractionSequence(minorDistractionDuration, distractionPos, minorDistractionSuspicion));
                moveToCheckPoint(distractionPos);
                break;
        }

        currentDistractionSeverity = sev;

    }
    protected override void OnSeePlayer()
    {
        base.OnSeePlayer();
        lastSeenTime = Time.time;
        if (!isAlerted) EnterAlert();
    }
    protected void EnterAlert() 
    {
        isAlerted = true;
        if (lookRoutine != null) 
        {
            StopCoroutine(lookRoutine);
            lookRoutine = null;
            nv.isStopped = true;
            isLookingAround = false;
        }
    }
}