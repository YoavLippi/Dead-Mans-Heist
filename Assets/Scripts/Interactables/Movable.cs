using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
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
    [SerializeField] private bool checkGrounded;
    [SerializeField] private List<Transform> raycastPositions;
    [SerializeField] private float groundedCheckLength=0.5f;
    [SerializeField] private LayerMask validGroundLayers;
    [SerializeField] private Renderer _renderer;
    private float objectHeight;
    public UnityEvent snapEvent;

    public string MovableID => movableID;
    public float ObjectHeight => objectHeight;

    public bool IsPlaceable
    {
        get => isPlaceable;
        set
        {
            isPlaceable = value;
            _renderer.material.SetFloat("_IsPlaceable", value ? 1 : 0);
        }
    }

    private void Start()
    {
        if (TryGetComponent(out Renderer r)) 
        {
            _renderer = r;
        }
        GetComponent<Rigidbody>().isKinematic = true;
        playerInteractionHandler = GameObject.FindWithTag("Player").transform.root.gameObject.GetComponentInChildren<InteractionHandler>();
        baseMat = _renderer.material;
        objectHeight = GetComponent<Collider>().bounds.size.y;
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
        _renderer.material = baseMat;
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
        _renderer.material = placementMat;
        IsPlaceable = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        GetComponent<Collider>().isTrigger = true;
        playerInteractionHandler.StartPlacementRoutine(this);
    }

    private void Update()
    {
        if (!isInMovingState) return;
        IsPlaceable = overlappingCols.Count == 0 && IsFullyGrounded();
    }

    private bool IsFullyGrounded()
    {
        if (!checkGrounded) return true;
        foreach (Transform pos in raycastPositions)
        {
            if (!Physics.Raycast(pos.position, Vector3.down, groundedCheckLength, validGroundLayers))
            {
                return false;
            }
        }
        
        return true;
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
