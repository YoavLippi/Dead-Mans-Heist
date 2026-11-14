using UnityEngine;

public class CannonController : MonoBehaviour
{
	[SerializeField] private ParticleSystem cannonSmokeVFX;
	[SerializeField] private AudioSource cannonAudioSource;
	[SerializeField] private AudioClip fireClip;

	public void FireCannon()
	{
		if (cannonSmokeVFX != null)
		{
			cannonSmokeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			cannonSmokeVFX.Play(true); 
		}

		if (cannonAudioSource != null && fireClip != null)
		{
			cannonAudioSource.PlayOneShot(fireClip);
		}
	}
}
