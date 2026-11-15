using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SneakGateCollider : MonoBehaviour
{
	[Header("Gate & Player References")]
	[SerializeField] private PlayerController playerController;
	[Tooltip("The physical blocking collider (Is Trigger = false) that stops non-sneaking players")]
	[SerializeField] private Collider blockingCollider;
	[SerializeField] private PlayerController.PlayerState requiredState = PlayerController.PlayerState.Sneaking;
	[SerializeField] private bool activeOnStart = false;
	[SerializeField] private bool disableBlockerOnceTriggered = true;
	[SerializeField] private float delayAfterSneakBeforePan = 2.0f;

	[Header("Player Control Locking")]
	[SerializeField] private Camera mainCamera;
	[Tooltip("Drag your player movement, mouse-look, and footstep scripts here")]
	[SerializeField] private MonoBehaviour[] scriptsToDisable;

	[Header("Target 1: Approaching Enemy")]
	[SerializeField] private Transform enemyTarget;
	[SerializeField] private float enemyZoomedFOV = 35f;
	[SerializeField] private float lookAtEnemyDuration = 0.8f;
	[Tooltip("How long to hold on the enemy before panning to the hiding spot (sync this with your voice line)")]
	[SerializeField] private float holdOnEnemyDuration = 2.0f;
	[SerializeField] private Renderer[] enemyRenderersToHighlight;

	[Header("Target 2: Hiding Spot")]
	[SerializeField] private Transform hidingSpotTarget;
	[SerializeField] private float hidingSpotZoomedFOV = 38f;
	[SerializeField] private float panToHidingSpotDuration = 0.9f;
	[SerializeField] private float returnToPlayerDuration = 0.8f;
	[SerializeField] private Renderer[] hidingSpotRenderersToHighlight;

	[Header("Single Continuous Voice Line")]
	[SerializeField] private AudioSource voiceAudioSource;
	[SerializeField] private AudioClip singleVoiceLineClip;
	[Range(0f, 3f)][SerializeField] private float voiceLineVolume = 1f;
	[SerializeField] private float delayBeforeVoice = 0.2f;
	[SerializeField] private float postVoiceHoldTime = 0.4f;

	[Header("Hint UI (Optional)")]
	[SerializeField] private CanvasGroup hintCanvasGroup;
	[SerializeField] private float uiFadeDuration = 0.35f;

	[Header("Occlusion Hiding (Blocking Props/Walls)")]
	[SerializeField] private LayerMask occluderLayers = ~0;
	[SerializeField] private float occlusionCheckRadius = 0.35f;
	[SerializeField] private GameObject[] extraObjectsToHide;
	[SerializeField] private Transform[] ignoreTransforms;

	[Header("Highlight Shader Settings (Optional)")]
	[SerializeField] private Material highlightOverlayMaterial;
	[SerializeField] private float targetOutlineSize = 0.015f;
	[ColorUsage(true, true)]
	[SerializeField] private Color highlightColor = new Color(1f, 0.82f, 0f, 1f);

	private bool gateEnabled;
	private bool isSneaking;
	private bool hasTriggeredSequence;
	private Behaviour cinemachineBrain;

	private Material runtimeHighlightInstance;
	private readonly Dictionary<Renderer, Material[]> originalMaterialsMap = new Dictionary<Renderer, Material[]>();
	private readonly List<Renderer> hiddenOccluders = new List<Renderer>();

	private static readonly int UseOutlineID = Shader.PropertyToID("_UseOutline");
	private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize");

	public bool IsSequenceRunning { get; private set; }

	private void Awake()
	{
		if (blockingCollider == null)
			blockingCollider = GetComponent<Collider>();

		if (playerController == null)
			playerController = FindFirstObjectByType<PlayerController>();

		if (mainCamera == null)
			mainCamera = Camera.main;

		if (mainCamera != null)
			cinemachineBrain = mainCamera.GetComponent("CinemachineBrain") as Behaviour;

		if (hintCanvasGroup != null)
		{
			hintCanvasGroup.alpha = 0f;
			hintCanvasGroup.gameObject.SetActive(false);
		}

		gateEnabled = activeOnStart;
	}

	private void OnEnable()
	{
		if (playerController != null)
			playerController.onStateChange += HandlePlayerStateChanged;

		RefreshColliderState();
	}

	private void OnDisable()
	{
		if (playerController != null)
			playerController.onStateChange -= HandlePlayerStateChanged;

		RestoreHiddenOccluders();
		RemoveHighlightOverlay();
	}

	private void HandlePlayerStateChanged(PlayerController.PlayerState newState)
	{
		isSneaking = (newState == requiredState);
		RefreshColliderState();

		if (gateEnabled && isSneaking && !hasTriggeredSequence)
		{
			hasTriggeredSequence = true;
			StartCoroutine(PlayDoubleTargetVoiceSequence());
		}
	}

	private IEnumerator PlayDoubleTargetVoiceSequence()
	{
		IsSequenceRunning = true;

		if (disableBlockerOnceTriggered)
		{
			gateEnabled = false;
			RefreshColliderState();
		}

		if (delayAfterSneakBeforePan > 0f)
		{
			float sneakElapsed = 0f;
			while (sneakElapsed < delayAfterSneakBeforePan)
			{
				if (!isSneaking)
				{
					// Player stood back up early — reset so it triggers once they sneak for a full 2s
					hasTriggeredSequence = false;
					IsSequenceRunning = false;
					yield break;
				}

				sneakElapsed += Time.deltaTime;
				yield return null;
			}
		}

		if (mainCamera == null)
			mainCamera = Camera.main;

		Time.timeScale = 0f;
		SetControlsEnabled(false);

		Transform camTransform = mainCamera.transform;
		Quaternion originalRotation = camTransform.rotation;
		float originalFOV = mainCamera.fieldOfView;

		// Start the single continuous voice line
		float totalVoiceWait = 3.5f;
		if (voiceAudioSource != null && singleVoiceLineClip != null)
		{
			StartCoroutine(PlaySingleVoiceDelayed());
			totalVoiceWait = delayBeforeVoice + singleVoiceLineClip.length + postVoiceHoldTime;
		}

		float sequenceStartTime = Time.unscaledTime;

		if (hintCanvasGroup != null)
			StartCoroutine(FadeHintText(1f));

		// --- LEG 1: Look & Zoom onto Enemy ---
		if (enemyTarget != null)
		{
			AttachHighlightOverlay(enemyRenderersToHighlight);
			Vector3 enemyCenter = GetTargetCenter(enemyTarget);
			HideOccludingObjects(camTransform.position, enemyCenter);

			yield return StartCoroutine(SmoothPanAndZoom(
					camTransform,
					enemyCenter,
					originalFOV,
					enemyZoomedFOV,
					lookAtEnemyDuration,
					fadeHighlightIn: true
			));

			if (holdOnEnemyDuration > 0f)
				yield return new WaitForSecondsRealtime(holdOnEnemyDuration);

			RestoreHiddenOccluders();
			RemoveHighlightOverlay();
		}

		// --- LEG 2: Pan Directly from Enemy to Hiding Spot ---
		if (hidingSpotTarget != null)
		{
			AttachHighlightOverlay(hidingSpotRenderersToHighlight);
			Vector3 hideCenter = GetTargetCenter(hidingSpotTarget);
			HideOccludingObjects(camTransform.position, hideCenter);

			yield return StartCoroutine(SmoothPanAndZoom(
					camTransform,
					hideCenter,
					mainCamera.fieldOfView,
					hidingSpotZoomedFOV,
					panToHidingSpotDuration,
					fadeHighlightIn: true
			));
		}

		// Wait for the remainder of the single voice line so the camera doesn't return before the line finishes
		float elapsedSoFar = Time.unscaledTime - sequenceStartTime;
		float remainingVoiceTime = totalVoiceWait - elapsedSoFar;
		if (remainingVoiceTime > 0f)
		{
			yield return new WaitForSecondsRealtime(remainingVoiceTime);
		}

		// --- RESTORE: Return to Player View ---
		if (hintCanvasGroup != null)
			StartCoroutine(FadeHintText(0f));

		yield return StartCoroutine(SmoothRestoreView(camTransform, originalRotation, originalFOV, returnToPlayerDuration));

		RestoreHiddenOccluders();
		RemoveHighlightOverlay();

		SetControlsEnabled(true);
		Time.timeScale = 1f;
		IsSequenceRunning = false;
	}

	private IEnumerator PlaySingleVoiceDelayed()
	{
		if (delayBeforeVoice > 0f)
			yield return new WaitForSecondsRealtime(delayBeforeVoice);

		voiceAudioSource.ignoreListenerPause = true;
		voiceAudioSource.clip = singleVoiceLineClip;
		voiceAudioSource.volume = voiceLineVolume;
		voiceAudioSource.Play();
	}

	private IEnumerator SmoothPanAndZoom(Transform cam, Vector3 targetPoint, float startFOV, float endFOV, float duration, bool fadeHighlightIn)
	{
		Quaternion startRot = cam.rotation;
		Vector3 dir = (targetPoint - cam.position).normalized;
		if (dir.sqrMagnitude < 0.0001f) yield break;

		Quaternion targetRot = Quaternion.LookRotation(dir);
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.001f)));

			cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
			mainCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);
			if (fadeHighlightIn) SetHighlightFade(t);

			HideOccludingObjects(cam.position, targetPoint);
			yield return null;
		}

		cam.rotation = targetRot;
		mainCamera.fieldOfView = endFOV;
		if (fadeHighlightIn) SetHighlightFade(1f);
	}

	private IEnumerator SmoothRestoreView(Transform cam, Quaternion targetRot, float targetFOV, float duration)
	{
		Quaternion startRot = cam.rotation;
		float startFOV = mainCamera.fieldOfView;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.001f)));

			cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
			mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
			SetHighlightFade(1f - t);

			yield return null;
		}

		cam.rotation = targetRot;
		mainCamera.fieldOfView = targetFOV;
		SetHighlightFade(0f);
	}

	private Vector3 GetTargetCenter(Transform target)
	{
		Renderer[] rends = target.GetComponentsInChildren<Renderer>();
		if (rends.Length == 0) return target.position;

		Bounds bounds = rends[0].bounds;
		for (int i = 1; i < rends.Length; i++)
			bounds.Encapsulate(rends[i].bounds);

		return bounds.center;
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
		float dist = toTarget.magnitude;
		if (dist <= 0.1f) return;

		RaycastHit[] hits = Physics.SphereCastAll(camPos, occlusionCheckRadius, toTarget.normalized, dist, occluderLayers, QueryTriggerInteraction.Ignore);
		foreach (var hit in hits)
		{
			Transform hitT = hit.transform;
			if (IsTargetOrIgnored(hitT)) continue;

			foreach (var rend in hitT.GetComponentsInChildren<Renderer>())
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
		if (enemyTarget != null && (candidate == enemyTarget || candidate.IsChildOf(enemyTarget)))
			return true;

		if (hidingSpotTarget != null && (candidate == hidingSpotTarget || candidate.IsChildOf(hidingSpotTarget)))
			return true;

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

	private void AttachHighlightOverlay(Renderer[] renderers)
	{
		RemoveHighlightOverlay();
		if (highlightOverlayMaterial == null || renderers == null || renderers.Length == 0)
			return;

		runtimeHighlightInstance = new Material(highlightOverlayMaterial);
		if (runtimeHighlightInstance.HasProperty(UseOutlineID))
			runtimeHighlightInstance.SetFloat(UseOutlineID, 1f);

		foreach (Renderer rend in renderers)
		{
			if (rend == null) continue;
			Material[] currentMats = rend.sharedMaterials;
			originalMaterialsMap[rend] = currentMats;

			Material[] expandedMats = new Material[currentMats.Length + 1];
			for (int i = 0; i < currentMats.Length; i++)
				expandedMats[i] = currentMats[i];

			expandedMats[currentMats.Length] = runtimeHighlightInstance;
			rend.materials = expandedMats;
		}
	}

	private void SetHighlightFade(float progress)
	{
		if (runtimeHighlightInstance == null) return;

		if (runtimeHighlightInstance.HasProperty(OutlineSizeID))
			runtimeHighlightInstance.SetFloat(OutlineSizeID, Mathf.Lerp(0f, targetOutlineSize, progress));

		Color fadedColor = highlightColor;
		fadedColor.a = (highlightColor.a > 0.001f ? highlightColor.a : 1f) * progress;

		Shader shader = runtimeHighlightInstance.shader;
		int propCount = shader.GetPropertyCount();
		for (int i = 0; i < propCount; i++)
		{
			if (shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
				runtimeHighlightInstance.SetColor(shader.GetPropertyName(i), fadedColor);
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
		if (cinemachineBrain != null)
			cinemachineBrain.enabled = state;

		if (scriptsToDisable == null) return;
		foreach (var script in scriptsToDisable)
		{
			if (script != null)
				script.enabled = state;
		}
	}

	private IEnumerator FadeHintText(float targetAlpha)
	{
		if (hintCanvasGroup == null) yield break;

		if (targetAlpha > 0f)
			hintCanvasGroup.gameObject.SetActive(true);

		float startAlpha = hintCanvasGroup.alpha;
		float elapsed = 0f;

		while (elapsed < uiFadeDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			hintCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / uiFadeDuration);
			yield return null;
		}

		hintCanvasGroup.alpha = targetAlpha;
		if (Mathf.Approximately(targetAlpha, 0f))
			hintCanvasGroup.gameObject.SetActive(false);
	}

	private void RefreshColliderState()
	{
		if (blockingCollider == null) return;
		blockingCollider.enabled = gateEnabled && !isSneaking;
	}

	public void ActivateGate()
	{
		gateEnabled = true;
		RefreshColliderState();
	}

	public void DeactivateGate()
	{
		gateEnabled = false;
		if (blockingCollider != null)
			blockingCollider.enabled = false;
	}
}
