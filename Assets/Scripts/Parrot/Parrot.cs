using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Parrot : MonoBehaviour
{

    [SerializeField] protected LosHandler attachedLos;
    [SerializeField] protected MeshRenderer coneRenderer;
    [SerializeField] protected float flightSpeed = 2f;
    [SerializeField] private float flyUpHeight;
    [SerializeField] private float perchHeight;



    [SerializeField] protected float detectionSpeed = 0.6f;
    [SerializeField] protected float suspicionDecreaseSpeed = 0.3f;
    [SerializeField] protected float currentSuspicion;
    [SerializeField] protected float maxSuspicion = 100f;
    [SerializeField] protected DetectionUX detectUX;
    [SerializeField] protected Gradient gradient;
    [SerializeField] protected bool isSeeingPlayer;
    [SerializeField] protected GameObject noiseMaker;

    [Header("Look-around turn")]
    [SerializeField] private float turnAngle = 60f;
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float lookAroundWaitTime = 0.3f;

    [SerializeField] private Quaternion startingRotation;
    private Coroutine lookRoutine;
    private Coroutine flightRoutine;

 
    [Header("Look schedule")]
    [SerializeField]private int lookIndex;
    [SerializeField]private int timeOffset;
    private int lastKnownWorldTime;
    [SerializeField] private List<newEvents> newSchedules;
    [SerializeField] private bool isLookingAround;
    [System.Serializable]
    public struct newEvents
    {
        public int timeTrigger;
        public UnityEvent attachedEvent;
    }
    [Header("Distraction")]
    [SerializeField] private GameObject distractionPrefab;
    [SerializeField] private int distractionInterval = 2;
    private Coroutine distractionLoop;
    private bool isplayDistraction;
    [SerializeField] private Transform[] perches;
    private int perchIndex;


    public float CurrentSuspicion
    {
        get => currentSuspicion;
        set
        {
            currentSuspicion = Mathf.Clamp(value, 0f, maxSuspicion);
            detectUX.UpdateUXState(currentSuspicion);
            attachedLos.SetSightColour(gradient.Evaluate(currentSuspicion / maxSuspicion));
        }
    }

    void OnEnable()
    {
        attachedLos.OnSeePlayer += OnSeePlayer;
        WorldTime.secondsChange += CheckTime;
    }

    void OnDisable()
    {
        attachedLos.OnSeePlayer -= OnSeePlayer;
        WorldTime.secondsChange -= CheckTime;
        if (distractionLoop != null) StopCoroutine(distractionLoop);
    }
    public void Start()
    {
        isLookingAround = false;
        CurrentSuspicion = 0;
    }
    public virtual void FixedUpdate()
    {
        if (currentSuspicion > 0f && !attachedLos.IsSeeingPlayer)
        {
                CurrentSuspicion = currentSuspicion - suspicionDecreaseSpeed;
                
        }
        if (attachedLos.IsSeeingPlayer) 
        {
            isSeeingPlayer = true;
        }
        if (currentSuspicion == 0) 
        {
            isSeeingPlayer = false;
        }

    }
    private void OnSeePlayer()
    {
        CurrentSuspicion = currentSuspicion + detectionSpeed;
        if (currentSuspicion >= maxSuspicion)
        {
            if (distractionLoop == null && !isplayDistraction)
            {
                StartCoroutine(DistractionSoundLoop());
            }
            Debug.Log("start making noise");
           
        }
    }

    public IEnumerator DistractionSoundLoop() 
    {
        isplayDistraction = true;
        while (isSeeingPlayer) 
        {
            //Instantiate(distractionPrefab, noiseMaker.transform.position, noiseMaker.transform.rotation);
            DistractionHandler.DoDistract(noiseMaker.transform.position, 10, DistractionHandler.DistractionSeverity.Severe, false);
            yield return new WaitForSeconds(distractionInterval);
        }
        isplayDistraction = false;
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
        lookRoutine = null;
        isLookingAround = false;
    }

    private IEnumerator RotateTo(Quaternion targetRot)
    {
        //float maxTurnTime = 10;
        float currentTime = 0;
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            while (isSeeingPlayer) 
            {
                yield return null;
                continue;
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
            currentTime += Time.deltaTime;

        }

        //if (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        //{
        //    Quaternion fallbackStart = transform.rotation;
        //    float fallbackDuration = 0.5f;
        //    float t = 0f;
        //    while (t < fallbackDuration)
        //    {
        //        t += Time.deltaTime;
        //        transform.rotation = Quaternion.Slerp(fallbackStart, targetRot, t / fallbackDuration);
        //        yield return null;
        //    }
        //}

        transform.rotation = targetRot;
    }

    protected void FlyUp(float targetY) 
    {

        Vector3 target = new Vector3(transform.position.x, targetY, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, flightSpeed * Time.deltaTime);
    }
    protected void FlyDown(float targetY)
    {
        Vector3 target = new Vector3(transform.position.x, targetY, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, flightSpeed * Time.deltaTime);
    }

    protected IEnumerator MoveToNewLocation(Transform newLocation) 
    {
        coneRenderer.enabled = false;
        while (Mathf.Abs(transform.position.y - flyUpHeight) > 0.05f) 
        {
            FlyUp(flyUpHeight);
            yield return null;
            continue;
           
        }
        Vector3 pos = transform.position;
        transform.position = new Vector3(newLocation.position.x, pos.y, newLocation.position.z);
        yield return new WaitForSeconds(0.5f);
        while (Mathf.Abs(transform.position.y - perchHeight) > 0.05f)
        {
            FlyDown(perchHeight);
            yield return null;
            continue;

        }
        coneRenderer.enabled = true;
        

    }

    public void flight()
    {
        Transform nextPerch = perches[perchIndex];
        perchIndex = (perchIndex + 1) % perches.Length;
        flightRoutine = StartCoroutine(MoveToNewLocation(nextPerch));
    }
    private void CheckTime(int currentworld)
    {
        lastKnownWorldTime = currentworld;
        if (isLookingAround) return;

        int localTime = currentworld - timeOffset;
        if (lookIndex >= newSchedules.Count)
        {
            timeOffset += newSchedules[newSchedules.Count - 1].timeTrigger;
            lookIndex = 0;
            return;
        }

        newEvents next = newSchedules[lookIndex];
        if (localTime >= next.timeTrigger)
        {
            next.attachedEvent.Invoke();
            lookIndex++;
        }
    }

}
