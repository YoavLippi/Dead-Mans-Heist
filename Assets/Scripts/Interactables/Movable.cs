using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Movable : Interactable
{
    public static float rotationSensitivity = 5f;
    public bool isInMovingState = false;
    [SerializeField] private InteractionHandler playerInteractionHandler;
    [SerializeField] private Material baseMat;
    [SerializeField] private Material placementMat;
    [SerializeField] private bool isPlaceable;
    [SerializeField] private List<Collider> overlappingCols;
    [SerializeField] private string movableID;
    public UnityEvent snapEvent;

    public string MovableID => movableID;

    public bool IsPlaceable
    {
        get => isPlaceable;
        set
        {
            isPlaceable = value;
            GetComponent<Renderer>().material.SetFloat("_IsPlaceable", value ? 1 : 0);
        }
    }

    private void Start()
    {
        playerInteractionHandler = GameObject.FindWithTag("Player").transform.root.gameObject.GetComponentInChildren<InteractionHandler>();
        baseMat = GetComponent<Renderer>().material;
        if (placementMat)
        {
            placementMat.mainTexture = baseMat.mainTexture;
        }
    }

    public override void DoInteract()
    {
        //pickup the thing
        //go into a placing state - give gameobject info to interactionHandler?
        if (isInMovingState)
        {
            if (!IsPlaceable) return;
            isInMovingState = !isInMovingState;
            DoPlace();
        }
        else
        {
            isInMovingState = !isInMovingState;
            DoMove();
        }
    }

    public void DoPlace(bool disable = false)
    {
        IsPlaceable = true;
        GetComponent<Renderer>().material = baseMat;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        GetComponent<Collider>().isTrigger = false;
        //this is a snap
        if (disable)
        {
            snapEvent.Invoke();
            Destroy(this);
        }
    }

    private void DoMove()
    {
        GetComponent<Renderer>().material = placementMat;
        IsPlaceable = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        GetComponent<Collider>().isTrigger = true;
        playerInteractionHandler.StartPlacementRoutine(this);
    }

    private void Update()
    {
        if (!isInMovingState) return;
        IsPlaceable = overlappingCols.Count == 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isInMovingState)
        {
            if (!overlappingCols.Contains(other))
            {
                overlappingCols.Add(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isInMovingState)
        {
            //IsPlaceable = true;
            if (overlappingCols.Contains(other))
            {
                overlappingCols.Remove(other);
            }
        }
    }
}
