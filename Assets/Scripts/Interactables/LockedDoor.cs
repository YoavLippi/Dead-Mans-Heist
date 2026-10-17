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
		StartOpening();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		if (isLocked)
		{

			if (HUDManager.Instance != null && HUDManager.Instance.HasKey(requiredKey))
			{
				isLocked = false;
				isOpening = true;
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
			StartOpening();
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

	private void StartOpening()
	{
		isOpening = true;

		if (!hasPlayedOpenAudio)
		{
			if (doorOpenSFX != null)
			{
				hasPlayedOpenAudio = true;
				//PlayClip(doorOpenSFX);
			}
			else
			{
				Debug.LogWarning($"[DOOR] Cannot play open sound: 'doorOpenSFX' is not assigned in the Inspector on {gameObject.name}!");
			}
		}
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
