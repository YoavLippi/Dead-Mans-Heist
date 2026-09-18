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
	private bool isTreasure = false;
	private MaterialPropertyBlock materialProperties;
	public GameObject ghostModePanel;

	private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
	private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize");
	private static readonly int UseOutLineBoolID = Shader.PropertyToID("_UseOutline");

	void Awake()
	{
		isTreasure = CompareTag("Treasure");
		// Universally grab all sprite renderers on this object or its children
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		materialProperties = new MaterialPropertyBlock();

		if (spriteRenderers.Length > 0)
		{
			//Color initialColor = isTreasure ? treasureOutlineColor : spectralOutlineColor;
			materialProperties.SetColor(OutlineColorID, spectralOutlineColor);
			spriteRenderers[0].SetPropertyBlock(materialProperties);
			Debug.Log($"[DIAGNOSTIC] {gameObject.name} found {spriteRenderers.Length} SpriteRenderer(s) in its hierarchy.");
		}
	}

	void Start()
	{
		//SetOutlineActive(false);

		thisSpriteMask = GetComponent<SpriteMask>();
		if (thisSpriteMask != null && spriteRenderers.Length > 0)
		{
			thisSpriteMask.sprite = spriteRenderers[0].sprite;
		}
		if (isTreasure)
		{
			SetOutlineActive(true, spectralOutlineColor);
			if (thisSpriteMask != null) thisSpriteMask.enabled = true;
		}
		else
		{
			SetOutlineActive(false, spectralOutlineColor);
			if (thisSpriteMask != null) thisSpriteMask.enabled = false;
		}
		if (ghostModePanel != null) ghostModePanel.SetActive(false);
	}

	void Update()
	{
		if (isTreasure)
		{
			ApplyPulseEffect(spectralOutlineColor);
			return;
		}

		if (GameManager.Instance == null) return;

		bool currentGlobalGhostState = GameManager.Instance.isGhostModeActive;

		// 1. Handles when Ghost Mode is turned ON or OFF
		if (currentGlobalGhostState != isGhostModeActive)
		{
			isGhostModeActive = currentGlobalGhostState;
			SetOutlineActive(isGhostModeActive, spectralOutlineColor);

			if (thisSpriteMask != null)
			{
				thisSpriteMask.enabled = isGhostModeActive;
			}
		}

		// If ghost mode is active, the sprite outline pulse dynamically
		if (isGhostModeActive)
		{
			ApplyPulseEffect(spectralOutlineColor);
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

	private void SetOutlineActive(bool state, Color targetColor)
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

	private void ApplyPulseEffect(Color targetColor)
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
		if (HUDManager.Instance != null)
		{
			HUDManager.Instance.SetInteractionPrompt(objectName, promptMessage);
		}
	}

	public void OnGhostHoverExit()
	{
		pulseSpeed = 3f; // Return to slow idle pulse
		if (HUDManager.Instance != null)
		{
			HUDManager.Instance.ClearInteractionPrompt();
		}
	}
}
