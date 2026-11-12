using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetectionUX : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	[Header("UI Components")]
	public SpriteRenderer fillImage;
	public TextMeshProUGUI statusText;
	[SerializeField] private GameObject camVisualPrefab;
	private bool isShowingIcon;

	public bool IsShowingIcon
	{
		get => isShowingIcon;
		set
		{
			isShowingIcon = value;
			camVisualInstance.SetActive(value);
		}
	}

	[Header("UX Tuning")]
	[UnityEngine.Range(0f, 100f)] public float currentDetection = 0f;
	public float drainRate = 15f;
	[Tooltip("Padding for offscreen visuals")]
	[SerializeField] private float edgePaddingX, edgePaddingY;

	[Header("Colour Palette")]
	public Color calmColor = new Color(0.2f, 0.8f, 0.2f);
	public Color suspiciousColor = new Color(1f, 0.8f, 0.2f);
	public Color alertColor = new Color(1f, 0.2f, 0.2f);

	private GameObject camVisualInstance;
	private PopupHandler visualInstanceHandler;

	private void Start()
	{
		camVisualInstance = Instantiate(camVisualPrefab, GameObject.FindWithTag("OverlayCanvas").transform);
		visualInstanceHandler = camVisualInstance.GetComponentInChildren<PopupHandler>();
		camVisualInstance.SetActive(false);
	}


	public void UpdateUXState(float stateChange)
	{
		currentDetection = stateChange;
		if (currentDetection >= 100f)
		{
			string t = "!";
			fillImage.color = alertColor;
			statusText.text = t;
			statusText.color = alertColor;
			visualInstanceHandler.UpdateText(t, alertColor);
			// FUTURE IMPLEMENTATION: TriggerSnapBack();
		}
		else if (currentDetection >= 50f)
		{
			string t = "?";
			fillImage.color = suspiciousColor;
			statusText.text = t;
			statusText.color = suspiciousColor;
			visualInstanceHandler.UpdateText(t, suspiciousColor);
		}
		else
		{
			fillImage.color = calmColor;
			statusText.text = ""; // Hidden when calm
			visualInstanceHandler.UpdateText("", calmColor);
		}
	}

	// A public function your future pirate/parrot AI scripts can call
	public void IncreaseSuspicion(float amount)
	{
		currentDetection += amount;
	}

	public void showOnCamera(Vector3 screenPos)
	{
		Vector3 workingPos = screenPos;
		Debug.Log($"Current screen pos is {screenPos}");
		bool isBehind = screenPos.z < 0;
		bool isOnScreen = !isBehind && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height;
		if (isOnScreen)
		{
			IsShowingIcon = false;
			return;
		}

		IsShowingIcon = true;
		Vector2 screenCentre = new Vector2(Screen.width / 2f, Screen.height / 2f);

		Vector2 direction = new Vector2(
			screenPos.x - screenCentre.x,
			screenPos.y - screenCentre.y
		);

		// Behind the camera
		if (isBehind)
			direction = -direction;

		// Find the closest intersection with the screen boundaries.
		float scaleX = (Screen.width / 2f - edgePaddingX) / Mathf.Abs(direction.x);
		float scaleY = (Screen.height / 2f - edgePaddingY) / Mathf.Abs(direction.y);

		float scale = Mathf.Min(scaleX, scaleY);

		Vector2 iconPosition = screenCentre + direction * scale;

		camVisualInstance.transform.position = new Vector3(iconPosition.x, iconPosition.y, 0f);
		/*IsShowingIcon = true;
		
		if (isBehind) workingPos *= -1;
		workingPos.x = Math.Clamp(workingPos.x, edgePaddingX, Screen.width-edgePaddingX);
		workingPos.y = Math.Clamp(workingPos.y, edgePaddingY, Screen.height-edgePaddingY);
		camVisualInstance.transform.position = workingPos;*/
	}

	public void StopShowingIcon()
	{
		IsShowingIcon = false;
	}
}
