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

	[Header("Audio (Optional)")]
	[SerializeField] private AudioSource winAudioSource;
	[SerializeField] private AudioClip winClip;
	[Range(0f, 1f)][SerializeField] private float winVolume = 1f;

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

		if (winClip != null)
		{
			if (winAudioSource != null)
			{
				winAudioSource.ignoreListenerPause = true;
				winAudioSource.spatialBlend = 0f;
				winAudioSource.PlayOneShot(winClip, winVolume);
			}
			else
			{
				AudioSource.PlayClipAtPoint(winClip, transform.position, winVolume);
			}
		}

		yield return new WaitForSecondsRealtime(displayDuration);

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

		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SceneManager.LoadSceneAsync(mainMenuSceneName);
	}
}
