//using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GearPuzzle : Interactable
{

	[SerializeField] private List<Gear> gears;
	[SerializeField] private UnityEvent onSolved;
	[SerializeField] private UnityEvent onFailed;
	[SerializeField] private Animator animator;

	[Header("Audio")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip solvedClip;
	[Range(0f, 1f)][SerializeField] private float solvedVolume = 1f;
	[SerializeField] private AudioClip failedClip;
	[Range(0f, 1f)][SerializeField] private float failedVolume = 1f;
	[SerializeField] private float audioDelay = 0f;

	[SerializeField] private bool correct = false;

	private void Start()
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();
	}

	public override void DoInteract() => OnSubmit();
	public void OnSubmit()
	{
		Debug.Log("Submit");
		if (correct) return;
		animator.SetBool("LeverInput", true);
		foreach (Gear gear in gears)
		{
			if (gear.IsCorrect == false)
			{
				PlayFeedbackAudio(false);
				onFailed.Invoke();
				return;
			}
		}
		correct = true;
		PlayFeedbackAudio(true);
		onSolved.Invoke();
		foreach (Gear gear in gears)
		{
			gear.Locked();
		}

	}

	public void Reset()
	{
		foreach (Gear gear in gears)
		{
			gear.Reset();
			gear.Unlocked();
		}
		correct = false;
	}

	private void PlayFeedbackAudio(bool isSolved)
	{
		AudioClip clip = isSolved ? solvedClip : failedClip;
		float volume = isSolved ? solvedVolume : failedVolume;

		if (clip == null) return;

		if (audioDelay > 0f)
		{
			StartCoroutine(PlayDelayed(clip, volume, audioDelay));
		}
		else
		{
			TriggerClip(clip, volume);
		}
	}

	private IEnumerator PlayDelayed(AudioClip clip, float volume, float delay)
	{
		yield return new WaitForSeconds(delay);
		TriggerClip(clip, volume);
	}

	private void TriggerClip(AudioClip clip, float volume)
	{
		if (audioSource != null)
		{
			audioSource.PlayOneShot(clip, volume);
		}
		else
		{
			AudioSource.PlayClipAtPoint(clip, transform.position, volume);
		}
	}
}
