using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : EnemyAbs
{
    [SerializeField] private NavMeshAgent nav;
    [SerializeField] private int index;
    [SerializeField] private GameObject target;
    [SerializeField] private bool isLookingAround;
    private int lastKnownWorldTime;
    [SerializeField] private AnimationCurve suspicionFalloffCurve;

    [Header("Look-around turn")]
    [SerializeField] private float turnAngle = 60f;
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float lookAroundWaitTime = 0.3f;

    [Header("Attention loss")]
    [SerializeField] private float suspicionThresholdToLookAround = 45f;

    [Header("Distraction tuning")]
    [SerializeField] private float severeDistractionDuration = 5f;
    [SerializeField] private float severeDistractionSuspicion = 75f;
    [SerializeField] private float moderateDistractionDuration = 3f;
    [SerializeField] private float moderateDistractionSuspicion = 50f;
    [SerializeField] private float arrivalDistanceThreshold = 2f;
    [SerializeField] private float maxTravelToDistractionSeconds = 10f;

    private Quaternion startingRotation;
    private Coroutine distractionRoutine;
    private Coroutine lookRoutine;

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
        HandleLostAttention();

        if (CurrentMoveMode == EnemyMoveMode.Chasing)
        {
            nav.SetDestination(target.transform.position);
            if (!attachedLos.IsSeeingPlayer)
            {
                attention = Mathf.Max(0, attention - Time.deltaTime);
            }
        }
    }

    private void HandleLostAttention()
    {
        if (attention != 0f) return;
        if (isLookingAround) return;
        if (CurrentSuspicion < suspicionThresholdToLookAround) return;

        StopNav();
        CurrentMoveMode = EnemyMoveMode.Idle;
        lookRoutine = StartCoroutine(LookSequence(lookAroundWaitTime * 3f));
    }

    protected override void CheckTime(int currentworld)
    {
        lastKnownWorldTime = currentworld;
        if (index >= newSchedules.Count) return;
        if (!isOnSchedule) return;

        newEvents next = newSchedules[index];
        if (currentworld >= next.timeTrigger)
        {
            next.attachedEvent.Invoke();
            index++;
        }
    }

    public override void moveToCheckPoint(Transform target)
    {
        if (target != null && nav != null)
        {
            nav.SetDestination(target.position);
        }
    }

    public void lookAround() => lookRoutine = StartCoroutine(LookSequence(3f));

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

    private IEnumerator RotateTo(Quaternion targetRot)
    {
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;
    }

    private void StopNav()
    {
        // Single, consistent way to halt the agent instead of mixing
        // ResetPath() in one place and SetDestination(here) in another.
        nav.ResetPath();
    }

    private IEnumerator DistractionSequence(float lookTime, Transform pos, float suspicion)
    {
        CurrentState = EnemyState.Distracted;
        CurrentSuspicion = suspicion;

        // Walk toward the distraction point, but don't wait forever if
        // it's unreachable (blocked, off-mesh, rounding error, etc).
        float elapsed = 0f;
        while (Vector3.Distance(transform.position, pos.position) > arrivalDistanceThreshold
               && elapsed < maxTravelToDistractionSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopNav();

        yield return StartCoroutine(LookSequence(lookTime));

        while (CurrentSuspicion > 0f)
        {
            yield return null;
        }

        CurrentState = EnemyState.None;
        isOnSchedule = true;
        distractionRoutine = null;
    }

    public override void GetDistracted(Transform distractionPos, DistractionHandler.DistractionSeverity sev)
    {
        Debug.Log($"{name} was distracted, pos: {distractionPos.position}, severity {sev}");

        // Prevent overlapping distraction coroutines from racing each
        // other's suspicion/state writes.
        if (distractionRoutine != null)
        {
            StopCoroutine(distractionRoutine);
        }

        isOnSchedule = false;
        moveToCheckPoint(distractionPos);

        switch (sev)
        {
            case DistractionHandler.DistractionSeverity.Severe:
                distractionRoutine = StartCoroutine(
                    DistractionSequence(severeDistractionDuration, distractionPos, severeDistractionSuspicion));
                break;

            case DistractionHandler.DistractionSeverity.Moderate:
                distractionRoutine = StartCoroutine(
                    DistractionSequence(moderateDistractionDuration, distractionPos, moderateDistractionSuspicion));
                break;
        }
    }
}