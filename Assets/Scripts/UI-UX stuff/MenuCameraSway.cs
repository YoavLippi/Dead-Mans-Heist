using UnityEngine;

public class MenuCameraSway : MonoBehaviour
{
	[Header("Rotation Sway (Degrees)")]
	[Tooltip("Tilt up and down (pitch)")]
	[SerializeField] private float pitchAmount = 0.8f;
	[Tooltip("Tilt side to side (roll - the most noticeable ship motion)")]
	[SerializeField] private float rollAmount = 1.4f;
	[Tooltip("Slow twist left and right (yaw)")]
	[SerializeField] private float yawAmount = 0.5f;

	[Header("Position Sway (Units)")]
	[Tooltip("Subtle heave up/down and drift side/side")]
	[SerializeField] private float horizontalDrift = 0.03f;
	[SerializeField] private float verticalHeave = 0.02f;

	[Header("Speed & Frequency")]
	[Tooltip("How slow and heavy the swell feels (lower = calmer, larger ship)")]
	[SerializeField] private float swaySpeed = 0.45f;

	[Header("Noise Variation")]
	[Tooltip("Blends organic Perlin noise so the motion doesn't feel like a rigid, robotic loop")]
	[SerializeField] private bool useOrganicNoise = true;

	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private float noiseSeedX;
	private float noiseSeedY;

	private void Start()
	{
		initialPosition = transform.localPosition;
		initialRotation = transform.localRotation;

		// Random seeds so the sway starts smoothly anywhere in the wave cycle
		noiseSeedX = Random.Range(0f, 100f);
		noiseSeedY = Random.Range(0f, 100f);
	}

	private void LateUpdate()
	{
		float time = Time.time * swaySpeed;

		// Calculate pitch, roll, and yaw angles
		float pitch, roll, yaw;
		float posX, posY;

		if (useOrganicNoise)
		{
			// Layering low-frequency sine waves with Perlin noise creates natural sea swell
			float noiseX = Mathf.PerlinNoise(noiseSeedX, time) * 2f - 1f;
			float noiseY = Mathf.PerlinNoise(noiseSeedY, time * 0.8f) * 2f - 1f;

			pitch = Mathf.Sin(time * 0.9f) * pitchAmount + (noiseY * pitchAmount * 0.3f);
			roll = Mathf.Cos(time * 0.7f) * rollAmount + (noiseX * rollAmount * 0.3f);
			yaw = Mathf.Sin(time * 0.5f) * yawAmount;

			posX = Mathf.Cos(time * 0.7f) * horizontalDrift;
			posY = Mathf.Sin(time * 1.1f) * verticalHeave;
		}
		else
		{
			// Pure sinusoidal motion
			pitch = Mathf.Sin(time) * pitchAmount;
			roll = Mathf.Cos(time * 0.8f) * rollAmount;
			yaw = Mathf.Sin(time * 0.4f) * yawAmount;

			posX = Mathf.Cos(time * 0.8f) * horizontalDrift;
			posY = Mathf.Sin(time * 1.2f) * verticalHeave;
		}

		// Apply offsets relative to the camera's original starting transform
		transform.localPosition = initialPosition + new Vector3(posX, posY, 0f);
		transform.localRotation = initialRotation * Quaternion.Euler(pitch, yaw, roll);
	}
}
