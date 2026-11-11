using UnityEngine;

public class WaterGushingAudio : MonoBehaviour
{
	[SerializeField] private AudioSource audioSource;

	[Header("Continuous Turbulence")]
	[Range(0.8f, 1.2f)][SerializeField] private float minPitch = 0.94f;
	[Range(0.8f, 1.2f)][SerializeField] private float maxPitch = 1.05f;
	[SerializeField] private float turbulenceSpeed = 0.8f; // Slow, organic water surging

	[Header("Volume Modulation")]
	[Range(0f, 1f)][SerializeField] private float baseVolume = 0.75f;
	[Range(0f, 0.2f)][SerializeField] private float volumeFluctuation = 0.05f;

	private float noiseSeed;

	private void Awake()
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();

		// Ensure looping is active
		audioSource.loop = true;
		audioSource.playOnAwake = true;
		audioSource.spatialBlend = 1f; // 3D

		noiseSeed = Random.Range(0f, 100f);
	}

	private void Start()
	{
		if (!audioSource.isPlaying)
			audioSource.Play();
	}

	private void Update()
	{
		float sampleTime = Time.time * turbulenceSpeed;

		// Subtle organic pitch wobble mimicking surges in water pressure
		float noise = Mathf.PerlinNoise(noiseSeed, sampleTime);
		audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, noise);

		// Subtle volume swell
		float volNoise = Mathf.PerlinNoise(noiseSeed + 10f, sampleTime * 1.2f);
		audioSource.volume = baseVolume + Mathf.Lerp(-volumeFluctuation, volumeFluctuation, volNoise);
	}
}
