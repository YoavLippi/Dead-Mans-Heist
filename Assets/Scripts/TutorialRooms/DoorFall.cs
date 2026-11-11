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

		if (soundDelay > 0f)
		{
			StartCoroutine(PlaySoundDelayed(soundDelay));
		}
		else
		{
			PlayImpactSound();
		}
	}

	private IEnumerator PlaySoundDelayed(float delay)
	{
		yield return new WaitForSeconds(delay);
		PlayImpactSound();
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