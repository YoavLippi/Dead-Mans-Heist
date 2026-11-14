using DefaultNamespace;
using UnityEngine;
using System.Collections;

public class RegionVoicelines : MonoBehaviour, iSaveable
{
	[SerializeField] private PlayAudio playAudio;
	[SerializeField] private AudioClip clip;
	public float voiceLineDelay;
	[SerializeField] private bool playOnce;
	private bool hasPlayed;
	private bool savedHasPlayed;
	private Coroutine delayRoutine;

	private void OnEnable()
	{
		((iSaveable)this).RegisterSaveEvents();
	}

	private void OnDisable()
	{
		((iSaveable)this).DeregisterSaveEvents();
	}

	private void OnTriggerEnter(Collider other)
	{
		//if (other.CompareTag("Player") && !hasPlayed)
		//{
		//	playAudio.Play(clip);
		//	hasPlayed = true;
		//}

		if (other.CompareTag("Player"))
		{
			if (playOnce && hasPlayed) return;

			if (playOnce)
				hasPlayed = true;

			PlayAudio();
		}
	}

	public void PlayAudio()
	{
		//playAudio.Play(clip);
		if (delayRoutine != null)
		{
			StopCoroutine(delayRoutine);
		}

		if (voiceLineDelay > 0f)
		{
			delayRoutine = StartCoroutine(PlayDelayedRoutine());
		}
		else
		{
			if (playAudio != null && clip != null)
				playAudio.Play(clip);
		}
	}

	private IEnumerator PlayDelayedRoutine()
	{
		yield return new WaitForSeconds(voiceLineDelay);

		if (playAudio != null && clip != null)
		{
			playAudio.Play(clip);
		}

		delayRoutine = null;
	}

	public void DoSave()
	{
		savedHasPlayed = hasPlayed;
	}

	public void DoLoad()
	{
		hasPlayed = savedHasPlayed;

		if (delayRoutine != null)
		{
			StopCoroutine(delayRoutine);
			delayRoutine = null;
		}
	}
}
