using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
	[Header("Dependencies")]
	[SerializeField] private PlayerController playerController;
	[SerializeField] private AudioSource audioSource;

	[Header("Ground Audio Clips")]
	[SerializeField] private AudioClip[] groundStepClips;

	[Header("Water Audio Clips")]
	[SerializeField] private AudioClip[] waterStepClips;

	[Header("Volume Levels")]
	[Range(0f, 1f)][SerializeField] private float sneakVolume = 0.2f;
	[Range(0f, 1f)][SerializeField] private float walkVolume = 0.55f;
	[Range(0f, 1f)][SerializeField] private float runVolume = 0.9f;

	[Header("Pitch Variation")]
	[Range(0.8f, 1.2f)][SerializeField] private float minPitch = 0.93f;
	[Range(0.8f, 1.2f)][SerializeField] private float maxPitch = 1.07f;

	[Header("Water Detection")]
	[SerializeField] private string waterTag = "Water";
	[SerializeField] private LayerMask waterLayer;

	private bool isInWater = false;

	private void Awake()
	{
		if (playerController == null)
			playerController = GetComponent<PlayerController>();

		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();
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

	private void HandleFootstep()
	{
		// Ignore ghost mode or hidden states
		if (playerController.CurrentState == PlayerController.PlayerState.Hiding || playerController.CurrentState == PlayerController.PlayerState.Idle)
			return;

		AudioClip[] targetClips = isInWater ? waterStepClips : groundStepClips;

		if (targetClips == null || targetClips.Length == 0)
			return;

		// Select a random clip from the active pool
		AudioClip selectedClip = targetClips[Random.Range(0, targetClips.Length)];

		// Determine volume by player state
		float volume = walkVolume;
		switch (playerController.CurrentState)
		{
			case PlayerController.PlayerState.Sneaking:
				volume = sneakVolume;
				break;
			case PlayerController.PlayerState.Running:
				volume = runVolume;
				break;
			case PlayerController.PlayerState.Walking:
				volume = walkVolume;
				break;
		}

		// Water wading sounds tend to be naturally noisier
		if (isInWater)
		{
			volume = Mathf.Clamp01(volume * 1.15f);
		}

		// Apply audio via AudioManager or local 3D AudioSource
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX(selectedClip, volume, 0.1f);
		}
		else if (audioSource != null)
		{
			audioSource.pitch = Random.Range(minPitch, maxPitch);
			audioSource.PlayOneShot(selectedClip, volume);
		}
	}

	// Trigger detection for water volumes (puddles, shorelines, bilge water)
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
