using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinTrigger : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private string playerTag = "Player";
	[SerializeField] private GameObject winPanel;
	[SerializeField] private CanvasGroup winContentGroup;

	[Header("Timing & Scene")]
	[SerializeField] private float displayDuration = 4f;
	[SerializeField] private float fadeDuration = 1f;
	[SerializeField] private string mainMenuSceneName = "0Menu";

	private Image panelBackground;
	private bool hasTriggered;

	private void Awake()
	{
		GetComponent<Collider>().isTrigger = true;

		if (winPanel != null)
		{
			panelBackground = winPanel.GetComponent<Image>();
			winPanel.SetActive(false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (hasTriggered) return;

		if (other.CompareTag(playerTag) || other.GetComponentInParent<PlayerController>() != null)
		{
			hasTriggered = true;
			StartCoroutine(WinRoutine());
		}
	}

	private IEnumerator WinRoutine()
	{
		if (winPanel != null)
			winPanel.SetActive(true);

		if (winContentGroup != null)
			winContentGroup.alpha = 1f;

		Time.timeScale = 0f;

		// 2. Display for 4 seconds
		yield return new WaitForSecondsRealtime(displayDuration);

		// 3. Fade out the text while darkening the panel's black background to fully opaque (alpha = 1)
		Color startBgColor = panelBackground != null ? panelBackground.color : Color.black;
		Color targetBgColor = new Color(startBgColor.r, startBgColor.g, startBgColor.b, 1f);

		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / Mathf.Max(fadeDuration, 0.001f));

			if (winContentGroup != null)
				winContentGroup.alpha = 1f - t;

			if (panelBackground != null)
				panelBackground.color = Color.Lerp(startBgColor, targetBgColor, t);

			yield return null;
		}

		if (winContentGroup != null) winContentGroup.alpha = 0f;
		if (panelBackground != null) panelBackground.color = targetBgColor;

		// 4. Restore time & cursor, then load Main Menu
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SceneManager.LoadSceneAsync(mainMenuSceneName);
	}
}
