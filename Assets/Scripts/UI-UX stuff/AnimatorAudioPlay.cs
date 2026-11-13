using UnityEngine;

public class AnimatorAudioPlay : MonoBehaviour
{
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip[] soundClips;
	[Range(0f, 1f)][SerializeField] private float audioVolume = 0.8f;

	private void Awake()
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();
	}

	// Call this from the Animation Event and pass the index (0, 1, 2...)
	public void PlaySoundIndex(int index)
	{
		if (audioSource != null && index >= 0 && index < soundClips.Length && soundClips[index] != null)
		{
			audioSource.PlayOneShot(soundClips[index], audioVolume = 0.8f);
		}
	}
}
