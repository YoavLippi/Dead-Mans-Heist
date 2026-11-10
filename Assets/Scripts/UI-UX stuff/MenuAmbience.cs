using UnityEngine;
using System.Collections;

public class MenuAmbience : MonoBehaviour
{
	[Header("Ocean Waves (Continuous Bed)")]
	[SerializeField] private AudioSource oceanAudioSource;
	[SerializeField] private AudioClip oceanLoopClip;
	[Range(0f, 1f)][SerializeField] private float oceanVolume = 0.8f;

	[Header("Wood Creaks (Randomized One-Shots)")]
	[SerializeField] private AudioSource creakAudioSource;
	[SerializeField] private AudioClip[] woodCreakClips;
	[Range(0f, 1f)][SerializeField] private float minCreakVolume = 0.2f;
	[Range(0f, 1f)][SerializeField] private float maxCreakVolume = 0.5f;

	[Header("Creak Timing Intervals")]
	[SerializeField] private float minSecondsBetweenCreaks = 4f;
	[SerializeField] private float maxSecondsBetweenCreaks = 10f;

	[Header("Pitch Variation")]
	[Range(0.8f, 1.2f)][SerializeField] private float minPitch = 0.92f;
	[Range(0.8f, 1.2f)][SerializeField] private float maxPitch = 1.08f;

	private Coroutine creakRoutine;

	private void Start()
	{
		SetupOceanLoop();

		if (woodCreakClips != null && woodCreakClips.Length > 0)
		{
			creakRoutine = StartCoroutine(CreakScheduler());
		}
	}

	private void SetupOceanLoop()
	{
		if (oceanAudioSource == null || oceanLoopClip == null) return;

		oceanAudioSource.clip = oceanLoopClip;
		oceanAudioSource.loop = true;
		oceanAudioSource.volume = oceanVolume;
		oceanAudioSource.spatialBlend = 0f; // 2D flat stereo ambience for a menu
		oceanAudioSource.playOnAwake = false;
		oceanAudioSource.Play();
	}

	private IEnumerator CreakScheduler()
	{
		while (true)
		{
			// Wait for a random interval before the next creak
			float waitTime = Random.Range(minSecondsBetweenCreaks, maxSecondsBetweenCreaks);
			yield return new WaitForSeconds(waitTime);

			PlayRandomCreak();
		}
	}

	private void PlayRandomCreak()
	{
		if (creakAudioSource == null || woodCreakClips.Length == 0) return;

		AudioClip clip = woodCreakClips[Random.Range(0, woodCreakClips.Length)];
		float volume = Random.Range(minCreakVolume, maxCreakVolume);

		creakAudioSource.pitch = Random.Range(minPitch, maxPitch);
		creakAudioSource.PlayOneShot(clip, volume);
	}

	private void OnDisable()
	{
		if (creakRoutine != null)
		{
			StopCoroutine(creakRoutine);
		}
	}
}
