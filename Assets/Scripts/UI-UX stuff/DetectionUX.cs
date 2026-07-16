using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetectionUX : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	[Header("UI Components")]
	public SpriteRenderer fillImage;
	public TextMeshProUGUI statusText;

	[Header("UX Tuning")]
	[Range(0f, 100f)] public float currentDetection = 0f;
	public float drainRate = 15f;

	[Header("Colour Palette")]
	public Color calmColor = new Color(0.2f, 0.8f, 0.2f);
	public Color suspiciousColor = new Color(1f, 0.8f, 0.2f);
	public Color alertColor = new Color(1f, 0.2f, 0.2f);


	public void UpdateUXState(float stateChange)
	{
		currentDetection = stateChange;
		if (currentDetection >= 100f)
		{
			fillImage.color = alertColor;
			statusText.text = "!";
			statusText.color = alertColor;
			// FUTURE IMPLEMENTATION: TriggerSnapBack();
		}
		else if (currentDetection >= 50f)
		{
			fillImage.color = suspiciousColor;
			statusText.text = "?";
			statusText.color = suspiciousColor;


		}
		else
		{
			fillImage.color = calmColor;
			statusText.text = ""; // Hidden when calm
		}
	}

	// A public function your future pirate/parrot AI scripts can call
	public void IncreaseSuspicion(float amount)
	{
		currentDetection += amount;
	}
}
