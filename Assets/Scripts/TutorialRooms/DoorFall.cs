using UnityEngine;
using System.Collections;

public class DoorFall : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private string parameterName;

	[Header("Audio")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip impactSound;
	[Range(0f, 1f)][SerializeField] private float volume = 1f;
	[SerializeField] private float soundDelay = 0.45f;

	[SerializeField] private AudioSource voiceAudioSource;
	[SerializeField] private AudioClip voiceLineClip;
	[Range(0f, 1f)][SerializeField] private float voiceVolume = 1f;
	[SerializeField] private float delayBeforeVoice = 0.6f;

	private bool hasFallen = false;

	private void Awake()
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();

		if (animator == null)
			animator = GetComponent<Animator>();
	}
	public void SetTrue() 
	{
		if (hasFallen) return;
		hasFallen = true;

		if (animator != null && !string.IsNullOrEmpty(parameterName))
		{
			animator.SetBool(parameterName, true);
		}

		StartCoroutine(DoorFallSequence());
	}

	private IEnumerator DoorFallSequence()
	{
		// 1. Wait for the door to hit the floor, then play the crash
		if (soundDelay > 0f)
		{
			yield return new WaitForSeconds(soundDelay);
		}
		PlayImpactSound();

		// 2. Wait a beat so the impact registers, then play the voice line
		if (voiceLineClip != null)
		{
			if (delayBeforeVoice > 0f)
			{
				yield return new WaitForSeconds(delayBeforeVoice);
			}
			PlayVoiceLine();
		}
	}

	private void PlayVoiceLine()
	{
		if (voiceAudioSource != null)
		{
			voiceAudioSource.PlayOneShot(voiceLineClip, voiceVolume);
		}
		else if (audioSource != null)
		{
			// Fallback to the door's AudioSource if no separate voice source is assigned
			audioSource.PlayOneShot(voiceLineClip, voiceVolume);
		}
	}


	public void PlayImpactSound()
	{
		if (impactSound == null) return;

		if (audioSource != null && impactSound != null)
		{
			audioSource.PlayOneShot(impactSound, volume);
		}
		else
		{
			AudioSource.PlayClipAtPoint(impactSound, transform.position, volume);
		}
	}
}