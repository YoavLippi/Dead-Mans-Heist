using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InteractionHandler : MonoBehaviour
{
	/*[SerializeField] private List<Interactable> interactablesInRange;
	//this will tell the list when it needs to check again for a new closest interactible
	[SerializeField] private bool isListDirty = false;
	[SerializeField] private Vector3 lastPos;
	[SerializeField] private float movementMax;
	[SerializeField] private float closestDist;
	[SerializeField] private float timeSinceClean;
	[SerializeField] private float refreshPeriod;*/
	[SerializeField] private Interactable closestInteractable;
	[SerializeField] private ConditionInteraction currentInteractionType = ConditionInteraction.Interact;

	[SerializeField] private float reachDistance = 13f;
	[SerializeField] private Interactable highlightedInteractable;
	[SerializeField] private LayerMask hitLayers;
	[SerializeField] private Vector3 cursorHitPoint;

	[Header("Holding")] 
	[SerializeField] private Interactable currentlyHeldObj;
	public ConditionInteraction CurrentInteractionType
	{
		get => currentInteractionType;
		set
		{
			currentInteractionType = value;
			//ResetRange();
		}
	}

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		//interactablesInRange = new List<Interactable>();
		//timeSinceClean = 0;
	}

	/*public void DoInteract()
	{
		if (closestInteractable != null)
		{
			closestInteractable.TriggerConditionalEvent();
		}
	}*/

	public void DoInteract(ConditionInteraction interaction, string eID = "")
	{
		if (closestInteractable != null)
		{
			closestInteractable.TriggerConditionalEvent(interaction, eID);
		} else if (currentlyHeldObj != null)
		{
			currentlyHeldObj.TriggerConditionalEvent(interaction, eID);
		}
	}

	public void DoInteract()
	{
		if (closestInteractable != null)
		{
			closestInteractable.TriggerConditionalEvent(CurrentInteractionType);
		} else if (currentlyHeldObj != null)
		{
			currentlyHeldObj.TriggerConditionalEvent(currentInteractionType);
		}
	}

	public bool TryGetClosestInteractable(out Interactable i)
	{
		i = closestInteractable != null? closestInteractable : null;
		return closestInteractable != null;
	}

	private void ResetRange()
	{
		/*foreach (var col in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
		{
			OnTriggerExit(col);
			OnTriggerEnter(col);
		}*/
	}

	/*private void OnTriggerEnter(Collider other)
	{
		Interactable temp = other.GetComponentInChildren<Interactable>();
		//if it doesn't have a specific interaction, we assume it's the default interact
		if (temp == null) return;

		if (temp.HasConditionalEventOfType(currentInteractionType))
		{
			if (interactablesInRange.Contains(temp)) return;

			interactablesInRange.Add(temp);
			if (interactablesInRange.Count == 1)
			{
				SetClosest(temp);
				//closestInteractable = temp;
				//temp.SetOutlineWidth(5f);
			}
			else
			{
				isListDirty = true;
			}
		}
	}*/

	/*private void OnTriggerExit(Collider other)
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
			}
			else
			{
				ClearClosest(); 
				//closestInteractable = null;
			}
		}
	}*/

	private void FixedUpdate()
	{
		/*
		//current implementation assumes nothing is moving, so I'll add a periodic check as well which should catch other things
		timeSinceClean += Time.deltaTime;
		if (timeSinceClean >= refreshPeriod)
		{
			isListDirty = true;
			timeSinceClean = 0;
		}
		if ((transform.position - lastPos).magnitude > movementMax)
		{
			lastPos = transform.position;
			isListDirty = true;
		}

		if (isListDirty && interactablesInRange.Count > 0)
		{
			closestDist = Single.MaxValue;
			//we want to recalculate if the closest pos is still the one we have
			Interactable newClosest = null;
			foreach (var interactable in interactablesInRange)
			{
				if (interactable == null) continue;

				interactable.SetOutlineWidth(0);
				float tempDist = (transform.position - interactable.transform.position).magnitude;
				if (tempDist < closestDist)
				{
					closestDist = tempDist;
					newClosest = interactable;
					//closestInteractable = interactable;
				}
			}
			if (newClosest != null)
			{
				SetClosest(newClosest);
			}
			else
			{
				ClearClosest();
			}

			//closestInteractable.SetOutlineWidth(5f);
			isListDirty = false;
		}*/
		//do new raycast
		//we need to know where the camera is and where it's pointed
		Vector3 camPos = Vector3.zero;
		Vector3 camDir = Vector3.zero;
		if (CameraController.Instance.ActiveCam is CinemachineCamera cam)
		{
			camPos = cam.State.GetFinalPosition();
			camDir = cam.transform.forward;
		}

        Interactable found = null;
        if (Physics.Raycast(camPos, camDir, out var hit, reachDistance, hitLayers))
        {
            Interactable candidate = hit.transform.GetComponentInParent<Interactable>();

            if (candidate != null && candidate.HasConditionalEventOfType(currentInteractionType))
            {
                found = candidate;
            }
            cursorHitPoint = hit.point;
        }
        else
        {
	        cursorHitPoint = camPos + camDir * reachDistance;
        }

        if (found != closestInteractable)
        {
            if (closestInteractable != null) ClearClosest();
            if (found != null) SetClosest(found);
        }

#if UNITY_EDITOR
        Debug.DrawLine(camPos, camPos + camDir * reachDistance, Color.green);
		#endif
	}

	public void StartPlacementRoutine(Movable objToPlace)
	{
		StartCoroutine(PlacementRoutine(objToPlace));
		currentlyHeldObj = objToPlace;
	}

	private IEnumerator PlacementRoutine(Movable objToPlace)
	{
		while (objToPlace.isInMovingState)
		{
			//push it into the ground
			LayerMask ignorePushLayers = LayerMask.GetMask("Triggers", "Ignore Raycast");
			if (Physics.Raycast(cursorHitPoint, Vector3.down, out var hit, 100f, ~ignorePushLayers))
			{
				objToPlace.transform.position = hit.point;
			}
			yield return new WaitForEndOfFrame();
		}
		currentlyHeldObj = null;
	}

	private void SetClosest(Interactable target)
	{
		closestInteractable = target;
		closestInteractable.SetOutlineWidth(0.03f);

		if (HUDManager.Instance != null)
		{
			HUDManager.Instance.SetInteractionPrompt(closestInteractable.DisplayName, closestInteractable.PromptMessage);
		}
	}

	private void ClearClosest()
	{
		if (closestInteractable != null)
		{
			closestInteractable.SetOutlineWidth(0);
			closestInteractable = null;
		}

		if (HUDManager.Instance != null)
		{
			HUDManager.Instance.ClearInteractionPrompt();
		}
	}

	public void HandleScroll(InputAction.CallbackContext context)
	{
		if (currentlyHeldObj && currentlyHeldObj is Movable c)
		{
			currentlyHeldObj.transform.Rotate(Vector3.forward, context.ReadValue<float>()*c.rotationSensitivity);
		}
	}
}
