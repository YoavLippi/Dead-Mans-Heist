using System;
using UnityEngine;

public class Movable : Interactable
{
    public bool isInMovingState = false;
    [SerializeField] private InteractionHandler playerInteractionHandler;
    [SerializeField] private Material baseMat;
    [SerializeField] private Material placementMat;
    [SerializeField] private bool isPlaceable;

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

    private void DoPlace()
    {
        IsPlaceable = true;
        GetComponent<Renderer>().material = baseMat;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        GetComponent<Collider>().isTrigger = false;
    }

    private void DoMove()
    {
        GetComponent<Renderer>().material = placementMat;
        IsPlaceable = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        GetComponent<Collider>().isTrigger = true;
        playerInteractionHandler.StartPlacementRoutine(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isInMovingState)
        {
            IsPlaceable = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isInMovingState)
        {
            IsPlaceable = true;
        }
    }
}
