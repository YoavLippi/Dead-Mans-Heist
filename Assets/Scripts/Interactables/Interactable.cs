using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum InteractionType
{
	Hide,               // Barrels, crates
	Distract,           // For now: Barrels and crates but can later be: hanging bells, thrown bottles, whistling pipes
	CollectItem,        // Keys, artifacts, tools
	ToggleMechanism,    // Levers, doors, pressure plates
	Dialogue
}

//very basic helper script for now, but it should let us categorise later
public abstract class Interactable : MonoBehaviour
{
	//[Header("HUD Display Settings")]
	//[SerializeField] public string objectName = "Object";
	//[SerializeField] public string promptMessage = "Interact";

	[Header("Identity & HUD Info")]
	[Tooltip("The display name shown on the HUD (e.g. 'Cargo Crate', 'Ship's Bell').")]
	[SerializeField] private string customObjectName = "";

	[Tooltip("Defines what category of interaction this object performs.")]
	[SerializeField] private InteractionType interactionType = InteractionType.Hide;

	public UnityEvent OnInteract;
	[SerializeField] private SpriteRenderer sr;
	private MaterialPropertyBlock materialProperties;

	private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize");
	private static readonly int UseOutLineBoolID = Shader.PropertyToID("_UseOutline");

	// Dynamic Display Name: Uses custom name if set, otherwise falls back to GameObject name
	public string DisplayName => string.IsNullOrWhiteSpace(customObjectName) ? gameObject.name : customObjectName;

	public virtual string PromptMessage
	{
		get
		{
			switch (interactionType)
			{
				case InteractionType.Hide:
					return "Press E to Hide";
				case InteractionType.Distract:
					return "Press E to Cause Distraction";
				//case InteractionType.CollectItem:
				//	return "Press E to Pick Up";
				//case InteractionType.ToggleMechanism:
				//	return "Press E to Activate";
				default:
					return "Press E to Interact";
			}
		}
	}

	public InteractionType Type => interactionType;
	protected virtual void Awake()
	{
		materialProperties = new MaterialPropertyBlock();
		if (sr == null)
		{
			sr = GetComponentInChildren<SpriteRenderer>();
		}
	}

	public virtual void DoInteract()
	{
		//OnInteract?.Invoke();
		throw new NotImplementedException("Please use a child of the interactable parent, not the parent itself");
	}

	public virtual void SetOutlineWidth(float w)
	{
		sr.GetPropertyBlock(materialProperties);
		materialProperties.SetFloat(OutlineSizeID, w);
		materialProperties.SetFloat(UseOutLineBoolID, (w == 0) ? 0f : 1f);
		//sr.material.SetFloat(OutlineSizeID, w);
		sr.SetPropertyBlock(materialProperties);
	}

	public virtual void DoSelfDistruct(float time)
	{
		StartCoroutine(DestroyAfterTime(time));
	}

	private IEnumerator DestroyAfterTime(float time)
	{
		yield return new WaitForSeconds(time);
		Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
		//Destroy(transform.parent.gameObject);
	}
}
