using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
	public static HUDManager Instance { get; private set; }

	[Header("Interaction & Prompt Displays")]
	public TMP_Text interactionPromptText;
	public GameObject interactionSlot;

	[Header("Skull elimination tracker")]
	public GameObject[] skullIcons = new GameObject[3];
	public TMP_Text crewEliminatedText;
	private int currentEliminations = 0;

	[Header("Inventory/Ability Display")]
	public TMP_Text keysCollectedText;
	public Image inventoryBackground;
	public Image[] inventorySlotIcons = new Image[5];
	private Sprite[] currentInventory = new Sprite[5];

	[Header("Ghost Ability & Cooldown UI")]
	[Tooltip("The main icon representing Ghost Vision ability.")]
	public Image ghostAbilityIcon;
	[Tooltip("UI Image with Image Type set to 'Filled' (Radial 360) that darkens the icon during cooldown.")]
	public Image ghostCooldownFillImage;
	[Tooltip("Text overlay displaying remaining cooldown seconds (e.g., '2.5s').")]
	public TMP_Text ghostCooldownText;
	[Tooltip("Color to tint the ability icon when Ghost Vision is actively running.")]
	public Color activeGhostModeColor = Color.cyan;
	public Color normalGhostModeColor = Color.white;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		ClearInteractionPrompt();
		InitializeSlots();
		SetGhostCooldown(0f, 0f);
		SetGhostModeVisualActive(false);
	}


	private void InitializeSlots()
	{
		for (int i = 0; i < inventorySlotIcons.Length; i++)
		{
			if (inventorySlotIcons[i] != null)
			{
				inventorySlotIcons[i].enabled = false;
			}
		}
	}

	#region Skull Elimination Logic
	/// <summary>
	/// Updates the skulls UI. 'eliminatedCount' turns that number of skulls active (or inactive depending on preference).
	/// </summary>
	/// <param name="eliminatedCount">Number of enemies killed so far.</param>
	public void UpdateSkullEliminations(int eliminatedCount)
	{
		currentEliminations = Mathf.Clamp(eliminatedCount, 0, skullIcons.Length);

		for (int i = 0; i < skullIcons.Length; i++)
		{
			if (skullIcons[i] != null)
			{
				skullIcons[i].SetActive(i < currentEliminations);
			}
		}
		if (crewEliminatedText != null)
		{
			crewEliminatedText.text = $"Crew Eliminated: {currentEliminations}/3";
		}
	}
	#endregion


	#region 5-Slot Inventory Logic
	/// <summary>
	/// Adds an item icon to the first available slot (up to 5).
	/// </summary>
	/// <param name="itemSprite">The icon sprite to display.</param>
	/// <returns>Returns true if item was successfully added, false if inventory was full.</returns>
	public bool AddItemToSlot(Sprite itemSprite)
	{
		for (int i = 0; i < currentInventory.Length; i++)
		{
			if (currentInventory[i] == null)
			{
				currentInventory[i] = itemSprite;
				inventorySlotIcons[i].sprite = itemSprite;
				keysCollectedText.enabled = true;
				inventoryBackground.enabled = true;
				inventorySlotIcons[i].enabled = true; // Show the icon inside the slot frame
				return true;
			}
		}

		Debug.LogWarning("Inventory full! Cannot hold more than 5 items.");
		return false;
	}

	/// <summary>
	/// Clears a specific slot (0, 1, or 2) when an item/ability is used or dropped.
	/// </summary>
	public void RemoveItemFromSlot(int slotIndex)
	{
		if (slotIndex >= 0 && slotIndex < currentInventory.Length)
		{
			currentInventory[slotIndex] = null;
			inventorySlotIcons[slotIndex].sprite = null;
			inventorySlotIcons[slotIndex].enabled = false; // Hide icon image
		}
	}
	#endregion

	#region Ghost Mode and Cooldown Display
	/// <summary>
	/// Updates the radial cooldown fill and timer text.
	/// </summary>
	/// <param name="currentCooldown">Seconds remaining.</param>
	/// <param name="maxCooldown">Total cooldown duration in seconds.</param>

	public void SetGhostCooldown(float currentCooldown, float maxCooldown)
	{
		if (currentCooldown > 0f && maxCooldown > 0f)
		{
			if (ghostCooldownFillImage != null)
			{
				ghostCooldownFillImage.gameObject.SetActive(true);
				ghostCooldownFillImage.fillAmount = currentCooldown / maxCooldown;
			}

			if (ghostCooldownText != null)
			{
				ghostCooldownText.gameObject.SetActive(true);
				ghostCooldownText.text = currentCooldown.ToString("F1") + "s";
			}
		}
		else
		{
			if (ghostCooldownFillImage != null)
			{
				ghostCooldownFillImage.fillAmount = 0f;
				ghostCooldownFillImage.gameObject.SetActive(false);
			}

			if (ghostCooldownText != null)
			{
				ghostCooldownText.gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Toggles icon highlight color when Ghost Mode is active vs inactive.
	/// </summary>
	public void SetGhostModeVisualActive(bool isActive)
	{
		if (ghostAbilityIcon != null)
		{
			ghostAbilityIcon.color = isActive ? activeGhostModeColor : normalGhostModeColor;
		}
	}
	#endregion

	#region Hover Prompt Logic
	public void SetInteractionPrompt(string objectName, string prompt)
	{
		if (interactionPromptText != null)
		{
			interactionPromptText.text = $"[{objectName}]: {prompt}";
		}

		if (interactionSlot != null)
		{
			interactionSlot.SetActive(true);
		}
	}

	public void ClearInteractionPrompt()
	{
		if (interactionPromptText != null)
		{
			interactionPromptText.text = "";
		}

		if (interactionSlot != null)
		{
			interactionSlot.SetActive(false);
		}
	}
	#endregion
}
