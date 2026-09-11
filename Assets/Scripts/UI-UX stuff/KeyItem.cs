using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum KeyType
{
	SilverKey,  // First key
	GoldKey,    // Second key
	CaptainsKey // Main key
}
public class KeyItem : Interactable
{
	[Header("Key Data")]
	[SerializeField] private KeyType keyType;
	[SerializeField] private Sprite keyIcon;
	[SerializeField] private AudioClip pickupSFX;

	public KeyType KeyItemType => keyType;
	public Sprite Icon => keyIcon;

	protected override void Awake()
	{
		base.Awake();
		OnInteract.AddListener(DoInteract);
	}

	public override void DoInteract()
	{
		if (HUDManager.Instance != null && keyIcon != null)
		{
			bool added = HUDManager.Instance.AddItemToSlot(keyIcon);
			if (!added)
			{
				Debug.LogWarning("Cannot pick up key: Inventory is full!");
				return;
			}
		}

		// 2. Play pickup SFX
		if (AudioManager.Instance != null && pickupSFX != null)
		{
			AudioManager.Instance.PlaySFX(pickupSFX, 0.8f);
		}

		StartCoroutine(PickupAndCleanupRoutine());
	}

	private IEnumerator PickupAndCleanupRoutine()
	{
		if (HUDManager.Instance != null)
		{
			HUDManager.Instance.ClearInteractionPrompt();
		}

		Collider col = GetComponentInChildren<Collider>();
		if (col != null)
		{
			col.enabled = false;
		}

		SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
		if (sr != null)
		{
			sr.enabled = false;
		}
				
		yield return new WaitForFixedUpdate();
		Destroy(gameObject);
	}
}
