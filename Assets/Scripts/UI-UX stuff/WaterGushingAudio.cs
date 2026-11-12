using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WaterGushingAudio : MonoBehaviour
{
	[SerializeField] private List<AudioSource> waterAudioSources;

	[Header("Continuous Turbulence")]
	[Range(0.8f, 1.5f)][SerializeField] private float minPitch = 0.94f;
	[Range(0.8f, 1.5f)][SerializeField] private float maxPitch = 1.05f;
	[SerializeField] private float turbulenceSpeed = 0.8f; // Slow, organic water surging

	[Header("Volume Modulation")]
	[Range(0f, 1f)][SerializeField] private float activeVolume = 0.65f;
	[SerializeField] private float fadeDuration = 1.2f;

	[SerializeField] private bool triggerOnceOnly = false;
	[SerializeField] private bool stopOnExit = true;
	[SerializeField] private string playerTag = "Player";

	private Coroutine fadeRoutine;
	private bool hasTriggered = false;

	private void Awake()
	{
		foreach (var source in waterAudioSources)
		{
			if (source == null) continue;

			source.loop = true;
			source.playOnAwake = false;
			source.volume = 0f;

			// Desynchronize pitch and start point so multiple holes don't sound like a robotic chorus
			source.pitch = Random.Range(minPitch, maxPitch);
			if (source.clip != null)
			{
				source.time = Random.Range(0f, source.clip.length);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (hasTriggered && triggerOnceOnly) return;

		if (other.CompareTag(playerTag) || other.GetComponentInParent<PlayerController>() != null)
		{
			hasTriggered = true;
			FadeAllToVolume(activeVolume);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!stopOnExit || (hasTriggered && triggerOnceOnly)) return;

		if (other.CompareTag(playerTag) || other.GetComponentInParent<PlayerController>() != null)
		{
			FadeAllToVolume(0f);
		}
	}

	private void FadeAllToVolume(float targetVol)
	{
		if (fadeRoutine != null)
			StopCoroutine(fadeRoutine);

		fadeRoutine = StartCoroutine(FadeRoutine(targetVol));
	}

	private IEnumerator FadeRoutine(float targetVol)
	{
		// Start any stopped sources before fading up
		if (targetVol > 0.001f)
		{
			foreach (var source in waterAudioSources)
			{
				if (source != null && !source.isPlaying)
				{
					source.volume = 0f;
					source.Play();
				}
			}
		}

		// Record starting volumes
		float[] startVolumes = new float[waterAudioSources.Count];
		for (int i = 0; i < waterAudioSources.Count; i++)
		{
			startVolumes[i] = waterAudioSources[i] != null ? waterAudioSources[i].volume : 0f;
		}

		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / fadeDuration;

			for (int i = 0; i < waterAudioSources.Count; i++)
			{
				if (waterAudioSources[i] != null)
				{
					waterAudioSources[i].volume = Mathf.Lerp(startVolumes[i], targetVol, t);
				}
			}
			yield return null;
		}

		// Snap to final volume and stop if muted
		for (int i = 0; i < waterAudioSources.Count; i++)
		{
			if (waterAudioSources[i] == null) continue;

			waterAudioSources[i].volume = targetVol;
			if (targetVol <= 0.001f)
			{
				waterAudioSources[i].Stop();
			}
		}

		fadeRoutine = null;
	}
}
