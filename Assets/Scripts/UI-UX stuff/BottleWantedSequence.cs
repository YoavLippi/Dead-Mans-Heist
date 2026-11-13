using UnityEngine;
using System.Collections;

public class BottleWantedSequence : MonoBehaviour
{
	[SerializeField] private string playerTag = "Player";

	[Header("Player Control Locking")]	
	[SerializeField] private MonoBehaviour[] playerScriptsToDisable;
	[SerializeField] private Transform playerCamera;

	[Header("Camera Look Settings")]
	[SerializeField] private float cameraLookDuration = 0.8f;
	[SerializeField] private float delayBeforePoster = 0.4f;

	[Header("UI - Wanted Poster")]
	[SerializeField] private GameObject wantedPosterUI;

	[Header("Audio Sources & Clips")]
	[SerializeField] private AudioSource sfxAudioSource;
	[SerializeField] private AudioClip bottleClinkClip;
	[Range(0f, 1f)][SerializeField] private float bottleClinkVolume = 0.8f;

	[SerializeField] private AudioClip paperCrinkleClip;
	[Range(0f, 1f)][SerializeField] private float paperCrinkleVolume = 0.75f;
	[SerializeField] private float crinkleLeadTime = 0.25f;

	//[Space(5)]
	[SerializeField] private AudioSource voiceAudioSource;
	[SerializeField] private AudioClip voiceLineClip;
	[Range(0f, 1f)][SerializeField] private float voiceLineVolume = 1.0f;
	[SerializeField] private float postVoiceBuffer = 0.3f;

	private bool hasTriggered = false;

	private void Start()
	{
		if (wantedPosterUI != null)
			wantedPosterUI.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (hasTriggered) return;

		if (other.CompareTag(playerTag) || other.GetComponentInParent<CharacterController>() != null)
		{
			hasTriggered = true;
			StartCoroutine(PlayDiscoverySequence(other.transform));
		}
	}

	// Also supports non-trigger physical collisions if preferred
	private void OnCollisionEnter(Collision collision)
	{
		if (hasTriggered) return;

		if (collision.gameObject.CompareTag(playerTag) || collision.gameObject.GetComponentInParent<CharacterController>() != null)
		{
			hasTriggered = true;
			StartCoroutine(PlayDiscoverySequence(collision.transform));
		}
	}

	private IEnumerator PlayDiscoverySequence(Transform playerTransform)
	{
		SetPlayerControl(false);
				
		if (sfxAudioSource != null && bottleClinkClip != null)
		{
			sfxAudioSource.PlayOneShot(bottleClinkClip, bottleClinkVolume);
		}		

		// Smoothly rotate the camera down to look at the bottle
		Quaternion originalCamRotation = Quaternion.identity;
		if (playerCamera != null)
		{
			originalCamRotation = playerCamera.rotation;
			yield return StartCoroutine(SmoothLookAtTarget(playerCamera, transform, cameraLookDuration));
		}

		yield return new WaitForSeconds(delayBeforePoster);
				

		if (sfxAudioSource != null && paperCrinkleClip != null)
		{
			sfxAudioSource.PlayOneShot(paperCrinkleClip, paperCrinkleVolume);
		}

		if (crinkleLeadTime > 0f)
		{
			yield return new WaitForSeconds(crinkleLeadTime);
		}

		if (wantedPosterUI != null)
		{
			wantedPosterUI.SetActive(true);
		}

		// Slight pause so the crinkle registers before speaking
		yield return new WaitForSeconds(0.35f);


		if (voiceAudioSource != null && voiceLineClip != null)
		{
			voiceAudioSource.clip = voiceLineClip; 
			voiceAudioSource.volume = voiceLineVolume;
			voiceAudioSource.Play();

			yield return new WaitForSeconds(voiceLineClip.length + postVoiceBuffer);
		}
		else
		{
			// Fallback wait if no voice clip is assigned yet
			yield return new WaitForSeconds(2.5f);
		}

		// Hide the Wanted Poster, restore camera angle, and return control
		if (wantedPosterUI != null)
		{
			wantedPosterUI.SetActive(false);
		}

		if (playerCamera != null)
		{
			yield return StartCoroutine(SmoothRestoreRotation(playerCamera, originalCamRotation, 0.5f));
		}

		SetPlayerControl(true);

		Destroy(gameObject);
	}

	private IEnumerator SmoothLookAtTarget(Transform cam, Transform target, float duration)
	{
		Quaternion startRot = cam.rotation;
		Vector3 direction = (target.position - cam.position).normalized;
		Quaternion targetRot = Quaternion.LookRotation(direction);

		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
			yield return null;
		}

		cam.rotation = targetRot;
	}

	private IEnumerator SmoothRestoreRotation(Transform cam, Quaternion targetRot, float duration)
	{
		Quaternion startRot = cam.rotation;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
			yield return null;
		}

		cam.rotation = targetRot;
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
