using UnityEngine;

public class DoorFall : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private string parameterName;

	[Header("Audio")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip impactSound;
	[Range(0f, 1f)][SerializeField] private float volume = 1f;

	private void Awake()
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();
	}
	public void SetTrue() => animator.SetBool(parameterName, true);

	public void PlayImpactSound()
	{
		if (audioSource != null && impactSound != null)
		{
			audioSource.PlayOneShot(impactSound, volume);
		}
	}
}