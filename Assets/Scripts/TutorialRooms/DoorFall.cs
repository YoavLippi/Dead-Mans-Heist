using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DoorFall : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private string parameterName;

	[Header("Player Control Locking")]
	[Tooltip("Drag your PlayerMovement, MouseLook, and Footstep scripts here")]
	[SerializeField] private MonoBehaviour[] playerScriptsToDisable;

	[Header("Audio")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip impactSound;
	[Range(0f, 1f)][SerializeField] private float volume = 1f;
	[SerializeField] private float soundDelay = 0.45f;

	[SerializeField] private AudioSource voiceAudioSource;
	[SerializeField] private AudioClip voiceLineClip;
	[Range(0f, 1f)][SerializeField] private float voiceVolume = 1f;
	[SerializeField] private float delayBeforeVoice = 0.6f;
	[SerializeField] private float postVoiceBuffer = 0.25f;

	[Header("Sequence Events")]
	[Tooltip("Fires ONLY after the door crash, voice line, and player unlock have all completed")]
	public UnityEvent onSequenceComplete;

	private bool hasFallen = false;
	private Coroutine sequenceRoutine;
	public bool IsSequenceActive { get; private set; } = false;

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
			animator.SetBool(parameterName, true);

		sequenceRoutine = StartCoroutine(DoorFallSequence());
	}

	private IEnumerator DoorFallSequence()
	{
		IsSequenceActive = true;
		SetPlayerControl(false);

		if (soundDelay > 0f)
			yield return new WaitForSeconds(soundDelay);

		if (impactSound != null)
		{
			if (audioSource != null) audioSource.PlayOneShot(impactSound, volume);
			else AudioSource.PlayClipAtPoint(impactSound, transform.position, volume);
		}

		if (voiceLineClip != null)
		{
			if (delayBeforeVoice > 0f)
				yield return new WaitForSeconds(delayBeforeVoice);

			if (voiceAudioSource != null)
			{
				voiceAudioSource.spatialBlend = 0f;
				voiceAudioSource.PlayOneShot(voiceLineClip, voiceVolume);
			}
			else if (audioSource != null)
			{
				audioSource.PlayOneShot(voiceLineClip, voiceVolume);
			}

			yield return new WaitForSeconds(voiceLineClip.length + postVoiceBuffer);
		}

		SetPlayerControl(true);
		IsSequenceActive = false;
		sequenceRoutine = null;
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

	private void SetPlayerControl(bool isEnabled)
	{
		if (playerScriptsToDisable == null) return;

		foreach (var script in playerScriptsToDisable)
		{
			if (script != null)
				script.enabled = isEnabled;
		}
	}
}