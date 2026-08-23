using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class HUDManager : MonoBehaviour
{
	public static HUDManager Instance { get; private set; }

	[Header("Interaction & Prompt Displays")]
	public TMP_Text interactionPromptText;
	public GameObject interactionSlot;

	[Header("Skull elimination tracker")]
	public List<GameObject> skullIcons = new List<GameObject>();

	[Header("Inventory/Ability Display")]
	public Image[] inventorySlotIcons = new Image[5];

	private Sprite[] currentInventory = new Sprite[5];

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
		for (int i = 0; i < skullIcons.Count; i++)
		{
			if (skullIcons[i] != null)
			{
				skullIcons[i].SetActive(i < eliminatedCount);
			}
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
