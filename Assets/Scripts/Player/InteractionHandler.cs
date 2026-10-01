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
	[SerializeField] private float timeSinceClean;
	[SerializeField] private float refreshPeriod;
	[SerializeField] private ConditionInteraction currentInteractionType = ConditionInteraction.Interact;

	public ConditionInteraction CurrentInteractionType
	{
		get => currentInteractionType;
		set
		{
			currentInteractionType = value;
			ResetRange();
		}
	}

	private void Awake()
	{
		interactablesInRange = new List<Interactable>();
		timeSinceClean = 0;
	}

	/*public void DoInteract()
	{
		if (closestInteractable != null)
		{
			closestInteractable.TriggerConditionalEvent();
		}
	}*/

	public void DoInteract(ConditionInteraction interaction)
	{
		if (closestInteractable != null)
		{
			closestInteractable.TriggerConditionalEvent(interaction);
		}
	}

	public bool TryGetClosestInteractable(out Interactable i)
	{
		i = closestInteractable != null? closestInteractable : null;
		return closestInteractable != null;
	}

	private void ResetRange()
	{
		foreach (var col in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
		{
			OnTriggerExit(col);
			OnTriggerEnter(col);
		}
	}

	private void OnTriggerEnter(Collider other)
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
			}
			else
			{
				ClearClosest(); 
				//closestInteractable = null;
			}
		}
	}

	private void FixedUpdate()
	{
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
		}
	}

	private void SetClosest(Interactable target)
	{
		closestInteractable = target;
		closestInteractable.SetOutlineWidth(5f);

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
}
