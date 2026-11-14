using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraHintSequence : MonoBehaviour
{
	[Header("Trigger Detection")]
	[SerializeField] private string playerTag = "Player";
	[SerializeField] private bool triggerOnceOnly = true;

	[Header("Player Control Locking")]
	[Tooltip("Drag your player movement, mouse-look, and footstep scripts here")]
	[SerializeField] private MonoBehaviour[] scriptsToDisable;

	[Header("Camera Focus Target")]
	[Tooltip("The object or empty Transform point the camera should turn to look at")]
	[SerializeField] private Transform focusTarget;
	[Tooltip("How long the camera takes to smoothly turn toward the target")]
	[SerializeField] private float lookAtDuration = 1.0f;
	[Tooltip("How long the camera takes to return to the player's original view")]
	[SerializeField] private float returnDuration = 0.8f;

	[Header("Camera Zoom Settings")]
	[SerializeField] private Camera mainCamera;
	[Tooltip("Target Field of View when zoomed in (lower = closer zoom, e.g. 35)")]
	[SerializeField] private float zoomedFOV = 35f;

	[Header("Trigger SFX (Optional)")]
	[SerializeField] private AudioSource sfxAudioSource;
	[SerializeField] private AudioClip triggerClip;
	[Range(0f, 1f)][SerializeField] private float triggerVolume = 0.8f;

	[Header("Voice Line Audio")]
	[SerializeField] private AudioSource voiceAudioSource;
	[SerializeField] private AudioClip voiceLineClip;
	[Range(0f, 1f)][SerializeField] private float voiceLineVolume = 1.0f;
	[Tooltip("Delay after crossing the collider before the voice line starts (e.g., wait until camera finishes turning)")]
	[SerializeField] private float delayBeforeVoice = 0.6f;
	[Tooltip("Extra pause after the voice line finishes before returning the camera")]
	[SerializeField] private float postVoiceHoldTime = 0.4f;

	[Header("Target Highlight Overlay")]
	[Tooltip("Renderers of the target object(s) that should light up during the zoom")]
	[SerializeField] private Renderer[] objectsToHighlight;
	[Tooltip("Your Shader Graphs/Outline Material")]
	[SerializeField] private Material highlightOverlayMaterial;
	[Tooltip("Target outline thickness when fully zoomed in (e.g. 0.015)")]
	[SerializeField] private float targetOutlineSize = 0.015f;
	[ColorUsage(true, true)]
	[SerializeField] private Color highlightColor = new Color(1f, 0.82f, 0f, 1f);

	private bool hasTriggered = false;
	private bool isSequenceRunning = false; 
	private Behaviour cinemachineBrain;

	private Material runtimeHighlightInstance;
	private readonly Dictionary<Renderer, Material[]> originalMaterialsMap = new Dictionary<Renderer, Material[]>();

	private static readonly int UseOutlineID = Shader.PropertyToID("_UseOutline");
	private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize");

	private void Awake()
	{
		if (mainCamera == null)
			mainCamera = Camera.main;

		// Detect CinemachineBrain on the Main Camera without needing a Cinemachine using-directive
		if (mainCamera != null)
			cinemachineBrain = mainCamera.GetComponent("CinemachineBrain") as Behaviour;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (hasTriggered && triggerOnceOnly) return;
		if (isSequenceRunning) return;

		if (other.CompareTag(playerTag) || other.GetComponentInParent<CharacterController>() != null)
		{
			hasTriggered = true;
			StartCoroutine(PlayCameraHintRoutine());
		}
	}

	private IEnumerator PlayCameraHintRoutine()
	{
		isSequenceRunning = true;
		
		if (mainCamera == null)
			mainCamera = Camera.main;

		AttachHighlightOverlay();
		SetHighlightFade(0f);

		Time.timeScale = 0f;
		SetControlsEnabled(false);

		if (sfxAudioSource != null && triggerClip != null)
		{
			sfxAudioSource.ignoreListenerPause = true;
			sfxAudioSource.PlayOneShot(triggerClip, triggerVolume);
		}

		Transform camTransform = mainCamera.transform;
		Quaternion originalRotation = camTransform.rotation;
		float originalFOV = mainCamera.fieldOfView;

		Coroutine lookRoutine = null;
		if (focusTarget != null)
		{
			lookRoutine = StartCoroutine(SmoothLookAndZoom(camTransform, focusTarget, originalFOV, zoomedFOV, lookAtDuration));
		}

		if (delayBeforeVoice > 0f)
			yield return new WaitForSecondsRealtime(delayBeforeVoice);

		if (voiceAudioSource != null && voiceLineClip != null)
		{
			voiceAudioSource.ignoreListenerPause = true;
			voiceAudioSource.clip = voiceLineClip;
			voiceAudioSource.volume = voiceLineVolume;
			voiceAudioSource.Play();

			yield return new WaitForSecondsRealtime(voiceLineClip.length + postVoiceHoldTime);
		}
		else
		{
			yield return new WaitForSecondsRealtime(2.5f);
		}

		if (lookRoutine != null)
			yield return lookRoutine;
				
			yield return StartCoroutine(SmoothRestoreView(camTransform, originalRotation, originalFOV, returnDuration));

		RemoveHighlightOverlay();

		SetControlsEnabled(true);
		Time.timeScale = 1f;
		isSequenceRunning = false;
	}

	private IEnumerator SmoothLookAndZoom(Transform cam, Transform target, float startFOV, float endFOV, float duration)
	{
		Quaternion startRot = cam.rotation;
		Vector3 direction = (target.position - cam.position).normalized;
		if (direction.sqrMagnitude < 0.0001f) yield break;

		Quaternion targetRot = Quaternion.LookRotation(direction);

		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(duration, 0.001f));

			cam.rotation = Quaternion.Slerp(startRot, targetRot, t); 
			mainCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);
			SetHighlightFade(t);

			yield return null;
		}

		cam.rotation = targetRot;
		mainCamera.fieldOfView = endFOV;
		SetHighlightFade(1f);
	}

	private IEnumerator SmoothRestoreView(Transform cam, Quaternion targetRot, float targetFOV, float duration)
	{
		Quaternion startRot = cam.rotation;
		float startFOV = mainCamera.fieldOfView;

		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(duration, 0.001f));

			cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
			mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
			SetHighlightFade(1f - t);

			yield return null;
		}

		cam.rotation = targetRot; 
		mainCamera.fieldOfView = targetFOV;
		SetHighlightFade(0f);
	}

	//private void SetPlayerControl(bool isEnabled)
	//{
	//	if (playerScriptsToDisable == null) return;

	//	foreach (var script in playerScriptsToDisable)
	//	{
	//		if (script != null)
	//			script.enabled = isEnabled;
	//	}
	//}

	private void AttachHighlightOverlay()
	{
		if (highlightOverlayMaterial == null || objectsToHighlight == null || objectsToHighlight.Length == 0)
			return;

		originalMaterialsMap.Clear();
		runtimeHighlightInstance = new Material(highlightOverlayMaterial);

		if (runtimeHighlightInstance.HasProperty(UseOutlineID))
			runtimeHighlightInstance.SetFloat(UseOutlineID, 1f);

		foreach (Renderer rend in objectsToHighlight)
		{
			if (rend == null) continue;

			Material[] currentMats = rend.sharedMaterials;
			originalMaterialsMap[rend] = currentMats;

			Material[] expandedMats = new Material[currentMats.Length + 1];
			for (int i = 0; i < currentMats.Length; i++)
			{
				expandedMats[i] = currentMats[i];
			}
			expandedMats[currentMats.Length] = runtimeHighlightInstance;
			rend.materials = expandedMats;
		}
	}

	private void SetHighlightFade(float progress)
	{
		if (runtimeHighlightInstance == null) return;

        // 1. Smoothly grow/shrink the outline thickness from 0 to targetOutlineSize
        if (runtimeHighlightInstance.HasProperty(OutlineSizeID))
            runtimeHighlightInstance.SetFloat(OutlineSizeID, Mathf.Lerp(0f, targetOutlineSize, progress));

        // 2. Smoothly fade the OutlineColour alpha as well (works if Surface Type is Transparent)
        Color fadedColor = highlightColor;
        fadedColor.a = (highlightColor.a > 0.001f ? highlightColor.a : 1f) * progress;

        Shader shader = runtimeHighlightInstance.shader;
        int propCount = shader.GetPropertyCount();
        for (int i = 0; i < propCount; i++)
        {
            if (shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
            {
                runtimeHighlightInstance.SetColor(shader.GetPropertyName(i), fadedColor);
            }
        }
	}

	private void RemoveHighlightOverlay()
	{
		foreach (var kvp in originalMaterialsMap)
		{
			if (kvp.Key != null)
				kvp.Key.materials = kvp.Value;
		}
		originalMaterialsMap.Clear();

		if (runtimeHighlightInstance != null)
		{
			Destroy(runtimeHighlightInstance);
			runtimeHighlightInstance = null;
		}
	}

	private void SetControlsEnabled(bool state)
	{
		// Temporarily release Cinemachine's lock on the Main Camera if present
		if (cinemachineBrain != null)
			cinemachineBrain.enabled = state;

		if (scriptsToDisable == null) return;
		foreach (var script in scriptsToDisable)
		{
			if (script != null)
				script.enabled = state;
		}
	}
}
