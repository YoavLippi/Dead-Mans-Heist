using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractionHandler : MonoBehaviour
{
    [SerializeField] private List<Interactable> interactablesInRange;
    //this will tell the list when it needs to check again for a new closest interactible
    [SerializeField] private bool isListDirty = false;
    [SerializeField] private Vector3 lastPos;
    [SerializeField] private float movementMax;
    [SerializeField] private Interactable closestInteractable;
    [SerializeField] private float closestDist;

    private void Awake()
    {
        interactablesInRange = new List<Interactable>();
    }

    public void DoInteract()
    {
        if (closestInteractable != null)
        {
            closestInteractable.OnInteract.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Interactable temp = other.GetComponentInChildren<Interactable>();
        if (temp != null)
        {
            if (interactablesInRange.Contains(temp)) return;
            
            interactablesInRange.Add(temp);
            if (interactablesInRange.Count == 1)
            {
                closestInteractable = temp;
                temp.SetOutlineWidth(5f);
            }
            else
            {
                isListDirty = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Interactable temp = other.GetComponentInChildren<Interactable>();
        if (temp != null)
        {
            if (!interactablesInRange.Contains(temp)) return;
            
            interactablesInRange.Remove(temp);
            temp.SetOutlineWidth(0);
            if (interactablesInRange.Count >= 1)
            {
                isListDirty = true;
            } else
            {
                closestInteractable = null;
            }
        }
    }

    private void FixedUpdate()
    {
        if ((transform.position - lastPos).magnitude > movementMax)
        {
            lastPos = transform.position;
            isListDirty = true;
        }

        if (isListDirty && interactablesInRange.Count>0)
        {
            closestDist = Single.MaxValue;
            //we want to recalculate if the closest pos is still the one we have
            foreach (var interactable in interactablesInRange)
            {
                interactable.SetOutlineWidth(0);
                float tempDist = (transform.position - interactable.transform.position).magnitude;
                if (tempDist < closestDist)
                {
                    closestDist = tempDist;
                    closestInteractable = interactable;
                }
            }
            
            closestInteractable.SetOutlineWidth(5f);
            isListDirty = false;
        }
    }
}
