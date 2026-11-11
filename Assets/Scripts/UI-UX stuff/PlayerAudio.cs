using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
	[Header("Dependencies")]
	[SerializeField] private PlayerController playerController;
	[SerializeField] private AudioSource audioSource;

	[Header("Dry Surface Clips (Wood / Deck)")]
	[SerializeField] private AudioClip[] walkClips;
	[SerializeField] private AudioClip[] sneakClips;

	[Header("Water Surface Clips (Shallow / Bilge)")]
	[SerializeField] private AudioClip[] waterWalkClips;
	[SerializeField] private AudioClip[] waterSneakClips;

	[Header("Volume by State")]
	[Range(0f, 1f)][SerializeField] private float walkVolume = 0.6f;
	[Range(0f, 1f)][SerializeField] private float sneakVolume = 0.25f;
	[Range(0f, 1f)][SerializeField] private float waterWalkVolume = 0.75f;
	[Range(0f, 1f)][SerializeField] private float waterSneakVolume = 0.35f;

	[Header("Pitch Variation")]
	[Range(0.8f, 1.2f)][SerializeField] private float minPitch = 0.94f;
	[Range(0.8f, 1.2f)][SerializeField] private float maxPitch = 1.06f;

	[Header("Water Detection")]
	[SerializeField] private string waterTag = "Water";
	[SerializeField] private LayerMask waterLayer;

	private bool isInWater = false;
	
	//singleton stuff so we can play audio from here any time
	public static PlayerAudio Instance;

	private void Awake()
	{
		if (playerController == null)
			playerController = GetComponent<PlayerController>();

		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();

		Instance = this;
	}

	private void OnEnable()
	{
		if (playerController != null)
			playerController.onStepTaken += HandleFootstep;
	}

	private void OnDisable()
	{
		if (playerController != null)
			playerController.onStepTaken -= HandleFootstep;
	}

	public void PlayAudioOneShot(AudioClip clip, float volume, bool playSolo = false)
	{
		if (playSolo)
		{
			audioSource.Stop();
		}
		audioSource.PlayOneShot(clip,volume);
	}

	private void HandleFootstep()
	{
		// Ignore hiding, or idle
		if (playerController.CurrentState == PlayerController.PlayerState.Hiding ||	playerController.CurrentState == PlayerController.PlayerState.Idle)
		{
			return;
		}

		bool isSneaking = playerController.CurrentState == PlayerController.PlayerState.Sneaking;

		AudioClip[] targetClips;
		float volume;

		// Route to the appropriate clip pool and volume level
		if (isInWater)
		{
			if (isSneaking)
			{
				targetClips = waterSneakClips;
				volume = waterSneakVolume;
			}
			else
			{
				targetClips = waterWalkClips;
				volume = waterWalkVolume;
			}
		}
		else
		{
			if (isSneaking)
			{
				targetClips = sneakClips;
				volume = sneakVolume;
			}
			else
			{
				targetClips = walkClips;
				volume = walkVolume;
			}
		}

		// Safety fallback: if specific sneak clips aren't assigned, fall back to walk clips
		if ((targetClips == null || targetClips.Length == 0) && isSneaking)
		{
			targetClips = isInWater ? waterWalkClips : walkClips;
		}

		if (targetClips == null || targetClips.Length == 0) return;

		AudioClip selectedClip = targetClips[Random.Range(0, targetClips.Length)];

		if (audioSource != null)
		{
			audioSource.pitch = Random.Range(minPitch, maxPitch);
			audioSource.PlayOneShot(selectedClip, volume);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(waterTag) || ((1 << other.gameObject.layer) & waterLayer) != 0)
		{
			isInWater = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag(waterTag) || ((1 << other.gameObject.layer) & waterLayer) != 0)
		{
			isInWater = false;
		}
	}
}
