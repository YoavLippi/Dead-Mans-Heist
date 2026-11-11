using UnityEngine;

public class RollingBottle : MonoBehaviour
{
	public enum RollAxis { X, Y, Z }

	[Header("Waypoints")]
	[Tooltip("Leave empty to use the bottle's current position as Point A")]
	[SerializeField] private Transform pointA;
	[SerializeField] private Transform pointB;

	[Header("Rolling Motion")]
	[SerializeField] private float rollDuration = 3.5f; // Seconds to roll one way
	[SerializeField] private float bottleRadius = 0.06f; // Adjust to roughly match bottle thickness
	[SerializeField] private RollAxis rollAxis = RollAxis.Z; // The axis running through the cap and base
	[SerializeField] private bool reverseRotation = false;

	[Header("Motion Feel")]
	[SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	//[Header("Audio")]
	//[SerializeField] private AudioSource audioSource;
	//[SerializeField] private AudioClip rollingClip;
	//[Range(0f, 1f)][SerializeField] private float maxRollVolume = 0.45f;
	//[SerializeField] private float minPitch = 0.85f;
	//[SerializeField] private float maxPitch = 1.15f;

	private Vector3 posA;
	private Vector3 posB;
	private Quaternion initialRot;
	private float timer = 0f;
	private bool movingToB = true;
	//private Vector3 lastPosition;

	private void Start()
	{
		// Fall back to local space if points aren't assigned
		posA = pointA != null ? pointA.position : transform.position;
		posB = pointB != null ? pointB.position : transform.position + transform.right * 0.35f;

		initialRot = transform.localRotation;
		//lastPosition = transform.position;

		//SetupAudio();
	}

	//private void SetupAudio()
	//{
	//	if (audioSource == null)
	//		audioSource = GetComponent<AudioSource>();

	//	if (audioSource != null && rollingClip != null)
	//	{
	//		audioSource.clip = rollingClip;
	//		audioSource.loop = true;
	//		audioSource.playOnAwake = false;
	//		audioSource.spatialBlend = 0f; // 2D menu audio (or 0.5f if you want subtle table panning)
	//		audioSource.volume = 0f;
	//		audioSource.Play();
	//	}
	//}

	private void Update()
	{
		if (rollDuration <= 0f) return;

		// Progress timer 0 -> 1 back and forth
		timer += Time.deltaTime / rollDuration;
		float progress = Mathf.Clamp01(timer);
		float curvedProgress = motionCurve.Evaluate(progress);

		// Calculate current position between Point A and Point B
		Vector3 currentPos = movingToB
				? Vector3.Lerp(posA, posB, curvedProgress)
				: Vector3.Lerp(posB, posA, curvedProgress);

		transform.position = currentPos;

		// Calculate physical rotation based on distance travelled: Angle (deg) = (Distance / Radius) * Rad2Deg
		float totalDist = Vector3.Distance(posA, posB);
		float currentDist = movingToB ? (curvedProgress * totalDist) : ((1f - curvedProgress) * totalDist);

		float rollAngle = (currentDist / Mathf.Max(bottleRadius, 0.001f)) * Mathf.Rad2Deg;
		if (reverseRotation) rollAngle = -rollAngle;

		// Apply rotation around the cylindrical axis
		Vector3 axisVector = rollAxis switch
		{
			RollAxis.X => Vector3.right,
			RollAxis.Y => Vector3.up,
			_ => Vector3.forward
		};

		transform.localRotation = initialRot * Quaternion.AngleAxis(rollAngle, axisVector);

		//UpdateRollingAudio();

		// Reverse direction once the end of the roll is reached
		if (progress >= 1f)
		{
			timer = 0f;
			movingToB = !movingToB;
		}
	}

	//private void UpdateRollingAudio()
	//{
	//	if (audioSource == null || rollingClip == null) return;

	//	// Calculate instantaneous speed
	//	float speed = (transform.position - lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
	//	lastPosition = transform.position;

	//	// Theoretical average speed = totalDist / rollDuration
	//	float totalDist = Vector3.Distance(posA, posB);
	//	float peakExpectedSpeed = (totalDist / Mathf.Max(rollDuration, 0.001f)) * 1.5f;

	//	// Normalized speed ratio (0 = stationary at the turnaround, 1 = peak roll speed)
	//	float speedFactor = Mathf.Clamp01(speed / Mathf.Max(peakExpectedSpeed, 0.001f));

	//	// Scale volume and pitch dynamically
	//	audioSource.volume = Mathf.Lerp(0f, maxRollVolume, speedFactor);
	//	audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedFactor);
	//}

	//private void OnDisable()
	//{
	//	if (audioSource != null && audioSource.isPlaying)
	//	{
	//		audioSource.Stop();
	//	}
	//}

	private void OnDrawGizmosSelected()
	{
		Vector3 a = pointA != null ? pointA.position : transform.position;
		Vector3 b = pointB != null ? pointB.position : transform.position + transform.right * 0.35f;

		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(a, b);
		Gizmos.DrawWireSphere(a, 0.02f);
		Gizmos.DrawWireSphere(b, 0.02f);
	}
}
