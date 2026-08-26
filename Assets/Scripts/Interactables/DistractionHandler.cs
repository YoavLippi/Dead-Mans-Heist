using System;
using System.Collections.Generic;
using UnityEngine;

public class DistractionHandler : Interactable
{
	public enum DistractionSeverity
	{
		Severe,
		Moderate,
		Minor,
		None
	}

	[Header("Editor")]
	[SerializeField] private bool alwaysDrawArea;
	[SerializeField] private bool drawWireframeOnly;
	[SerializeField] private Color sphereColour;
	[SerializeField] private Color wireColour;
	[Header("Setup")]
	[SerializeField] private DistractionSeverity thisSeverity;
	[SerializeField] private float distractionRadius;
	[SerializeField] private bool isLowPriorityDistraction;
	[SerializeField] private AudioClip soundEffect;

	public DistractionSeverity ThisSeverity
	{
		get => thisSeverity;
		set => thisSeverity = value;
	}

	public float DistractionRadius
	{
		get => distractionRadius;
		set => distractionRadius = value;
	}

	private void OnDrawGizmos()
	{
		if (alwaysDrawArea)
		{
			DrawGizmo();
		}
		//Gizmos.DrawCube(Vector3.zero, Vector3.one);
	}

	private void OnDrawGizmosSelected()
	{
		DrawGizmo();
	}

	private void DrawGizmo()
	{
		//Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = wireColour;
		Gizmos.DrawWireSphere(transform.position, distractionRadius);
		if (drawWireframeOnly) return;
		Gizmos.color = sphereColour;
		Gizmos.DrawSphere(transform.position, distractionRadius);
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		/*Enemy a = new Enemy();

		a.GetDistracted(transform, thisSeverity);*/
	}

	private void Update()
	{
		//UseDistraction();
	}

	/*public void DoDistraction()
	{
			//List<Enemy> enemiesInRange = new List<Enemy>();
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, distractionRadius);
			foreach (var col in hitColliders)
			{
					if (!col.CompareTag("Enemy")) continue;

					if (col.GetComponent<EnemyAbs>())
					{
							//enemiesInRange.Add(col.GetComponent<Enemy>());
							col.GetComponent<EnemyAbs>().GetDistracted(transform, thisSeverity);
					}
			}
	}*/


	public override void DoInteract()
	{

		//if (AudioManager.Instance != null && soundEffect != null)
		//{
		//	AudioManager.Instance.PlaySFXAtPosition(soundEffect, transform.position, 1f, distractionRadius);
		//}
		//else if (AudioManager.Instance != null && AudioManager.Instance.crateDistraction != null)
		//{
		//	// Optional fallback: use default bell distraction clip from AudioManager
		//	AudioManager.Instance.PlaySFXAtPosition(AudioManager.Instance.crateDistraction, transform.position, 1f, distractionRadius);
		//}

		if (AudioManager.Instance != null)
		{
			AudioClip clipToPlay = soundEffect != null ? soundEffect : AudioManager.Instance.crateDistraction;
			if (clipToPlay != null)
			{
				AudioManager.Instance.PlaySFX(clipToPlay, 1f, 0.05f);
			}
			else
			{
				Debug.LogError($"[DistractionHandler] No sound clip assigned to {gameObject.name} or AudioManager!");
			}
		}

		//List<Enemy> enemiesInRange = new List<Enemy>();
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, distractionRadius);
		foreach (var col in hitColliders)
		{
			if (!col.CompareTag("Enemy")) continue;

			if (!col.GetComponent<EnemyAbs>()) continue;
			//enemiesInRange.Add(col.GetComponent<Enemy>());
			if (!isLowPriorityDistraction || col.GetComponent<EnemyAbs>().CurrentState != EnemyState.Distracted)
			{
				col.GetComponent<EnemyAbs>().GetDistracted(transform.position, thisSeverity);
			}
		}
	}
}
