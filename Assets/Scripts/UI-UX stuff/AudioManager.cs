using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[Header("Mixer & Routing")]
	[SerializeField] private AudioMixerGroup musicGroup;
	[SerializeField] private AudioMixerGroup sfxGroup;
	[SerializeField] private AudioMixerGroup ghostGroup;

	[Header("Audio Sources")]
	[SerializeField] private AudioSource musicSource;
	[SerializeField] private AudioSource sfxSource;
	[SerializeField] private AudioSource ghostSource;

	[Header("Common Sound Clips")]
	public AudioClip oceanAmbience;
	public AudioClip ghostVisionEnter;
	//public AudioClip ghostVisionLoop;
	//public AudioClip itemPickup;
	//public AudioClip skullElimination;
	//public AudioClip doorUnlock;
	public AudioClip footSteps;
	public AudioClip crateDistraction;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// Ensure AudioSources exist and route to correct mixer groups
		SetupSources();
	}

	private void SetupSources()
	{
		if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
		if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
		if (ghostSource == null) ghostSource = gameObject.AddComponent<AudioSource>();

		musicSource.outputAudioMixerGroup = musicGroup;
		musicSource.loop = true;

		sfxSource.outputAudioMixerGroup = sfxGroup;

		ghostSource.outputAudioMixerGroup = ghostGroup;
		ghostSource.loop = true;
	}

	private void Start()
	{
		if (oceanAmbience != null)
		{
			PlayMusic(oceanAmbience);
		}
	}

	#region 2D Global Playback
	/// <summary>
	/// Plays a non-diegetic 2D SFX (UI, pickups, HUD triggers).
	/// </summary>
	public void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariation = 0.05f)
	{
		if (clip == null) return;

		// Slight pitch variation avoids repetitive audio fatigue (e.g. footsteps/pickups)
		sfxSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
		sfxSource.PlayOneShot(clip, volume);
	}

	public void PlayMusic(AudioClip clip, float volume = 0.6f)
	{
		if (clip == null) return;
		musicSource.clip = clip;
		musicSource.volume = volume;
		musicSource.Play();
	}
	#endregion

	#region 3D Positional Audio (Distractions & World Objects)
	/// <summary>
	/// Spawns a temporary 3D audio source at a world location so guards and player hear spatial audio.
	/// </summary>
	public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float maxDistance = 15f)
	{
		if (clip == null) return;

		GameObject tempAudioObj = new GameObject("TempAudio_" + clip.name);
		tempAudioObj.transform.position = position;

		AudioSource source = tempAudioObj.AddComponent<AudioSource>();
		source.clip = clip;
		source.outputAudioMixerGroup = sfxGroup;
		source.spatialBlend = 1f; // Full 3D sound
		source.minDistance = 1f;
		source.maxDistance = maxDistance;
		source.rolloffMode = AudioRolloffMode.Linear;
		source.volume = volume;
		source.Play();

		Destroy(tempAudioObj, clip.length + 0.1f);
	}
	#endregion

	#region Ghost Mode Audio Handling
	public void SetGhostVisionAudio(bool active)
	{
		if (active)
		{
			PlaySFX(ghostVisionEnter, 0.8f, 0f);
			//if (ghostVisionLoop != null)
			//{
			//	ghostSource.clip = ghostVisionLoop;
			//	ghostSource.Play();
			//}
		}
		else
		{
			ghostSource.Stop();
		}
	}
	#endregion
}
