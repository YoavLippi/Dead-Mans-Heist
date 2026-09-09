using UnityEngine;

[ExecuteAlways]
public class ShipWakeUpdate : MonoBehaviour
{
	[SerializeField] private Material waterMaterial;

	private static readonly int ShipPosID = Shader.PropertyToID("_ShipPosition");
	private static readonly int ShipFwdID = Shader.PropertyToID("_ShipForward"); 
	private static readonly int ShipRightID = Shader.PropertyToID("_ShipRight");

	private Vector3 lastPosition;
	private Quaternion lastRotation;

	void Start() => PushTransformToShader();

	void Update()
	{
		if (waterMaterial == null) return;

		if (transform.position != lastPosition || transform.rotation != lastRotation)
		{
			PushTransformToShader();
			lastPosition = transform.position;
			lastRotation = transform.rotation;
		}
	}

	private void PushTransformToShader()
	{
		waterMaterial.SetVector(ShipPosID, transform.position);

		// Red arrow points toward the back, so -transform.right points toward the bow (Forward)
		waterMaterial.SetVector(ShipFwdID, transform.right);

		// Green arrow points left (port), so -transform.up points to the right (Starboard)
		waterMaterial.SetVector(ShipRightID, -transform.up);
	}
}
