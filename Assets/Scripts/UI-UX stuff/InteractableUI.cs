using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableUI : MonoBehaviour
{
	[Header("UX Visual Settings")]
	[ColorUsage(true, true)]
	public Color spectralOutlineColor = new Color(0.7f, 0.065f, 0.8f, 1f);
	[Range(0f, 10f)] public float maxOutlineThickness = 3f;
	public float pulseSpeed = 3f;
	public SpriteMask thisSpriteMask;

	[Header("Interaction Settings")]
	public string objectName = "Object";
	public string promptMessage = "Cause Chaos";

	private SpriteRenderer[] spriteRenderers;
	private bool isGhostModeActive = false;
	private MaterialPropertyBlock materialProperties;
	public GameObject ghostModePanel;

	private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
	private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize");
	private static readonly int UseOutLineBoolID = Shader.PropertyToID("_UseOutline");

	void Awake()
	{
		// Universally grab all sprite renderers on this object or its children
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		materialProperties = new MaterialPropertyBlock();
		materialProperties.SetColor(OutlineColorID, spectralOutlineColor);
		spriteRenderers[0].SetPropertyBlock(materialProperties);
		Debug.Log($"[DIAGNOSTIC] {gameObject.name} found {spriteRenderers.Length} SpriteRenderer(s) in its hierarchy.");
	}

	void Start()
	{
		SetOutlineActive(false);
		if (ghostModePanel != null) ghostModePanel.SetActive(false);
		thisSpriteMask = GetComponent<SpriteMask>();
		thisSpriteMask.sprite = spriteRenderers[0].sprite;
	}

	void Update()
	{
		if (GameManager.Instance == null) return;

		bool currentGlobalGhostState = GameManager.Instance.isGhostModeActive;

		// 1. Handles when Ghost Mode is turned ON or OFF
		if (currentGlobalGhostState != isGhostModeActive)
		{
			isGhostModeActive = currentGlobalGhostState;
			SetOutlineActive(isGhostModeActive);

			if (thisSpriteMask != null)
			{
				thisSpriteMask.enabled = isGhostModeActive;
			}
		}

		// If ghost mode is active, the sprite outline pulse dynamically
		if (isGhostModeActive)
		{
			ApplyPulseEffect();
		}
	}

	//public void ToggleGhostVision(bool active)
	//{
	//	isGhostModeActive = active;
	//	SetOutlineActive(active);
	//	thisSpriteMask.enabled = active;

	//	if (ghostModePanel != null)
	//	{
	//		ghostModePanel.SetActive(active);
	//	}
	//}

	private void SetOutlineActive(bool state)
	{
		foreach (SpriteRenderer spriteRen in spriteRenderers)
		{
			spriteRen.GetPropertyBlock(materialProperties);

			if (state)
			{
				materialProperties.SetColor(OutlineColorID, spectralOutlineColor);
				materialProperties.SetFloat(OutlineSizeID, maxOutlineThickness);
				materialProperties.SetFloat(UseOutLineBoolID, 1f);
				Debug.Log($"[DIAGNOSTIC] Sending values to {spriteRen.gameObject.name}: Color = {spectralOutlineColor}, Size = {maxOutlineThickness}");
			}
			else
			{
				// Turn off outline by setting its thickness to 0
				materialProperties.SetFloat(OutlineSizeID, 0f);
				materialProperties.SetFloat(UseOutLineBoolID, 0f);
			}

			spriteRen.SetPropertyBlock(materialProperties);
		}
	}

	private void ApplyPulseEffect()
	{
		float currentThickness = Mathf.PingPong(Time.time * pulseSpeed, maxOutlineThickness);

		foreach (SpriteRenderer spriteRen in spriteRenderers)
		{
			spriteRen.GetPropertyBlock(materialProperties);
			materialProperties.SetFloat(OutlineSizeID, currentThickness);
			materialProperties.SetFloat(UseOutLineBoolID, 1f);
			spriteRen.SetPropertyBlock(materialProperties);
		}
	}

	// Call these functions when the Ghost's detection cursor/radius enters or exits this prop
	public void OnGhostHoverEnter()
	{
		pulseSpeed = 6f; // Pulse faster when targeted
		Debug.Log($"Targeting: {objectName}. Prompt: [Press E to {promptMessage}]");
	}

	public void OnGhostHoverExit()
	{
		pulseSpeed = 3f; // Return to slow idle pulse
	}
}
