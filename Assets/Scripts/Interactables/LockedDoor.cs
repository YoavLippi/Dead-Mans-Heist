using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class LockedDoor : MonoBehaviour
{
	[SerializeField] private string requiredKey;
	[SerializeField] private bool isLocked;
	[SerializeField] private Transform doorPivot;
	//[SerializeField] private Transform closedPivot;
	[SerializeField] private float openAngle = 90;
	[SerializeField] private float smoothSpeed = 2;
	[SerializeField] private bool isOpening = false;
	//[SerializeField] private bool isClosing = false;
	//[SerializeField] private bool isOpen = false;
	[SerializeField] private Quaternion targetRotation;

	[Header("Door Audio")]
	[SerializeField] private AudioClip doorOpenSFX;
	[SerializeField] private AudioClip doorLockedSFX;
	[SerializeField] private float openDelay = 0.35f;

	private bool hasPlayedOpenAudio = false;
	private float nextAllowedLockedSoundTime = 0f;
	private const float LockedSoundCooldown = 0.8f;

	void Start()
	{
		if (doorPivot == null)
		{
			doorPivot = transform;
			//closedPivot.rotation = doorPivot.rotation;
		}
		targetRotation = doorPivot.rotation * Quaternion.Euler(0, openAngle, 0);
		StopAllCoroutines();
	}

	void Update()
	{

		if (isOpening)
		{
			doorPivot.rotation = Quaternion.Slerp(doorPivot.rotation, targetRotation, Time.deltaTime * smoothSpeed);
			//isOpen = true;
		}
	}
	//public IEnumerator waitToClose() 
	//{   
	//    isOpening = false;
	//    isClosing = true;
	//    yield return new WaitForSeconds(2);
	//    while (Quaternion.Angle(doorPivot.rotation, closedPivot.rotation) > 0.5f) 
	//    {
	//        doorPivot.rotation = Quaternion.Slerp(doorPivot.rotation, closedPivot.rotation, Time.deltaTime * smoothSpeed);
	//        yield return null;
	//    }

	//    isClosing = false;

	//}

	public void isOpeningMethod()
	{
		//StartOpening();
		TriggerOpenSequence();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		if (isLocked)
		{

			if (HUDManager.Instance != null && HUDManager.Instance.HasKey(requiredKey))
			{
				isLocked = false;
				//isOpening = true;
				TriggerOpenSequence();
				Debug.Log("Opening door");
			}
			else
			{
				//Aryanna could you add liked a locked door noise
				PlayLockedAudio();
				Debug.Log("[DOOR] Locked! Need key: " + requiredKey);
			}
		}
		else
		{
			TriggerOpenSequence();
		}
	}
	//if we want the doors to close
	//private void OnTriggerExit(Collider other)
	//{
	//    if (isOpen && other.CompareTag("Player")) 
	//    {
	//       isClosing = true;
	//        StartCoroutine(waitToClose());
	//    }
	//}

	//private void StartOpening()
	//{
	//	isOpening = true;

	//	if (!hasPlayedOpenAudio)
	//	{
	//		if (doorOpenSFX != null)
	//		{
	//			hasPlayedOpenAudio = true;
	//			PlayClip(doorOpenSFX); // Ensure this is active, not commented out!
	//		}
	//		else
	//		{
	//			Debug.LogWarning($"[DOOR] 'doorOpenSFX' is not assigned on {gameObject.name}!");
	//		}
	//	}
	//}

	private void TriggerOpenSequence()
	{
		if (!hasPlayedOpenAudio)
		{
			hasPlayedOpenAudio = true;
			StartCoroutine(PlaySoundThenOpenRoutine());
		}
	}

	private IEnumerator PlaySoundThenOpenRoutine()
	{
		// 1. Play the audio immediately
		if (doorOpenSFX != null)
		{
			PlayClip(doorOpenSFX);
		}
		else
		{
			Debug.LogWarning($"[DOOR] 'doorOpenSFX' is missing on {gameObject.name}!");
		}

		// 2. Wait for the audio lead-in (latch/handle movement)
		if (openDelay > 0f)
		{
			yield return new WaitForSeconds(openDelay);
		}

		// 3. Begin physical door rotation
		isOpening = true;
	}

	private void PlayLockedAudio()
	{
		if (doorLockedSFX == null) return;

		// Rate-limit the rattle/thump so it doesn't machine-gun if triggers bounce
		if (Time.time >= nextAllowedLockedSoundTime)
		{
			nextAllowedLockedSoundTime = Time.time + LockedSoundCooldown;
			PlayClip(doorLockedSFX);
		}
	}
	private void PlayClip(AudioClip clip)
	{
		if (clip == null) return;

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySFX(clip);
		}
		else
		{
			AudioSource.PlayClipAtPoint(clip, transform.position);
		}
	}
}
