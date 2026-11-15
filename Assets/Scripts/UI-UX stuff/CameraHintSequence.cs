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
	[Tooltip("Single object or empty Transform point the camera should turn to look at")]
	[SerializeField] private Transform focusTarget;
	[Tooltip("Optional: Multiple objects to frame together (overrides Focus Target if populated)")]
	[SerializeField] private Transform[] focusTargets;
	[Tooltip("How long the camera takes to smoothly turn toward the target")]
	[SerializeField] private float lookAtDuration = 1.0f;
	[Tooltip("How long the camera takes to return to the player's original view")]
	[SerializeField] private float returnDuration = 0.8f;

	[Header("Occlusion Hiding (Blocking Objects)")]
	[Tooltip("Layers that can block the camera view (e.g., Default, Environment, Props)")]
	[SerializeField] private LayerMask occluderLayers = ~0;
	[Tooltip("Radius of the line-of-sight check between the camera and target")]
	[SerializeField] private float occlusionCheckRadius = 0.35f;
	[Tooltip("Optional: Specific objects (such as walls/beams without colliders) to always hide during the zoom")]
	[SerializeField] private GameObject[] extraObjectsToHide;
	[Tooltip("Objects that should NEVER be hidden even if hit by the check (e.g., Player, Floor)")]
	[SerializeField] private Transform[] ignoreTransforms;

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
	[Range(0f, 3f)][SerializeField] private float voiceLineVolume = 1.5f;
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

	[Header("Hint UI")]
	[Tooltip("Attach a CanvasGroup to your hint text (or parent banner) and drag it here")]
	[SerializeField] private CanvasGroup hintCanvasGroup;
	[SerializeField] private float fadeDuration = 0.4f;

	private bool hasTriggered = false;
	public bool isSequenceRunning = false;
	private Behaviour cinemachineBrain;
	public bool IsPlaying { get; private set; } = false;

	private Material runtimeHighlightInstance;
	private readonly Dictionary<Renderer, Material[]> originalMaterialsMap = new Dictionary<Renderer, Material[]>();
	private readonly List<Renderer> hiddenOccluders = new List<Renderer>();
	private Coroutine fadeRoutine;

	private static readonly int UseOutlineID = Shader.PropertyToID("_UseOutline");
	private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize");

	private void Awake()
	{
		if (mainCamera == null)
			mainCamera = Camera.main;

		// Detect CinemachineBrain on the Main Camera without needing a Cinemachine using-directive
		if (mainCamera != null)
			cinemachineBrain = mainCamera.GetComponent("CinemachineBrain") as Behaviour;

		if (hintCanvasGroup != null)
		{
			hintCanvasGroup.alpha = 0f;
			hintCanvasGroup.gameObject.SetActive(false);
		}
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
		IsPlaying = true;
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

		bool hasTarget = HasValidTarget();
		Vector3 targetPoint = hasTarget ? GetFocusCenter() : Vector3.zero;

		if (hasTarget)
			HideOccludingObjects(camTransform.position, targetPoint);

		Coroutine lookRoutine = null;
		if (hasTarget)
		{
			lookRoutine = StartCoroutine(SmoothLookAndZoom(camTransform, targetPoint, originalFOV, zoomedFOV, lookAtDuration));
			TriggerFade(1f);
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

		TriggerFade(0f);
		yield return StartCoroutine(SmoothRestoreView(camTransform, originalRotation, originalFOV, returnDuration));

		RestoreHiddenOccluders();
		RemoveHighlightOverlay();

		SetControlsEnabled(true);
		Time.timeScale = 1f;
		isSequenceRunning = false;
		IsPlaying = false;
	}

	private IEnumerator SmoothLookAndZoom(Transform cam, Vector3 targetPoint, float startFOV, float endFOV, float duration)
	{
		Quaternion startRot = cam.rotation;
		Vector3 direction = (targetPoint - cam.position).normalized;
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

			HideOccludingObjects(cam.position, targetPoint);

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

	private void HideOccludingObjects(Vector3 camPos, Vector3 targetPoint)
	{
		if (extraObjectsToHide != null)
		{
			foreach (var obj in extraObjectsToHide)
			{
				if (obj == null) continue;
				foreach (var rend in obj.GetComponentsInChildren<Renderer>())
					HideRenderer(rend);
			}
		}

		Vector3 toTarget = targetPoint - camPos;
		float distance = toTarget.magnitude;
		if (distance <= 0.1f) return;

		RaycastHit[] hits = Physics.SphereCastAll(
			camPos,
			occlusionCheckRadius,
			toTarget.normalized,
			distance,
			occluderLayers,
			QueryTriggerInteraction.Ignore
		);

		foreach (var hit in hits)
		{
			Transform hitTransform = hit.transform;
			if (IsTargetOrIgnored(hitTransform)) continue;

			foreach (var rend in hitTransform.GetComponentsInChildren<Renderer>())
				HideRenderer(rend);
		}
	}

	private void HideRenderer(Renderer rend)
	{
		if (rend != null && rend.enabled)
		{
			rend.enabled = false;
			if (!hiddenOccluders.Contains(rend))
				hiddenOccluders.Add(rend);
		}
	}

	private void RestoreHiddenOccluders()
	{
		foreach (var rend in hiddenOccluders)
		{
			if (rend != null)
				rend.enabled = true;
		}
		hiddenOccluders.Clear();
	}

	private bool IsTargetOrIgnored(Transform candidate)
	{
		if (focusTarget != null && (candidate == focusTarget || candidate.IsChildOf(focusTarget)))
			return true;

		if (focusTargets != null)
		{
			foreach (var t in focusTargets)
			{
				if (t != null && (candidate == t || candidate.IsChildOf(t)))
					return true;
			}
		}

		if (ignoreTransforms != null)
		{
			foreach (var ignore in ignoreTransforms)
			{
				if (ignore != null && (candidate == ignore || candidate.IsChildOf(ignore)))
					return true;
			}
		}

		return false;
	}

	private bool HasValidTarget()
	{
		if (focusTarget != null) return true;
		if (focusTargets != null)
		{
			foreach (var t in focusTargets)
				if (t != null) return true;
		}
		return false;
	}

	private Vector3 GetFocusCenter()
	{
		Bounds combinedBounds = new Bounds();
		bool hasBounds = false;

		void EncapsulateTransform(Transform t)
		{
			if (t == null) return;
			Renderer[] renderers = t.GetComponentsInChildren<Renderer>();
			if (renderers.Length > 0)
			{
				foreach (var r in renderers)
				{
					if (!hasBounds)
					{
						combinedBounds = r.bounds;
						hasBounds = true;
					}
					else
					{
						combinedBounds.Encapsulate(r.bounds);
					}
				}
			}
			else
			{
				if (!hasBounds)
				{
					combinedBounds = new Bounds(t.position, Vector3.zero);
					hasBounds = true;
				}
				else
				{
					combinedBounds.Encapsulate(t.position);
				}
			}
		}

		if (focusTargets != null && focusTargets.Length > 0)
		{
			foreach (var t in focusTargets)
				EncapsulateTransform(t);
		}

		if (!hasBounds && focusTarget != null)
			EncapsulateTransform(focusTarget);

		return hasBounds ? combinedBounds.center : Vector3.zero;
	}

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

	private void TriggerFade(float targetAlpha)
	{
		if (fadeRoutine != null) StopCoroutine(fadeRoutine);
		fadeRoutine = StartCoroutine(FadeHintText(targetAlpha));
	}

	private IEnumerator FadeHintText(float targetAlpha)
	{
		if (hintCanvasGroup == null) yield break;

		if (targetAlpha > 0f)
			hintCanvasGroup.gameObject.SetActive(true);

		float startAlpha = hintCanvasGroup.alpha;
		float elapsed = 0f;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.unscaledDeltaTime; // Works even if Time.timeScale == 0f
			hintCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
			yield return null;
		}

		hintCanvasGroup.alpha = targetAlpha;

		if (Mathf.Approximately(targetAlpha, 0f))
			hintCanvasGroup.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		RestoreHiddenOccluders();
	}
}
