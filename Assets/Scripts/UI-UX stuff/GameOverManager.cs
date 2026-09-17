using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
	public static GameOverManager Instance { get; private set; }

	[Header("UI Reference")]
	[Tooltip("The root GameObject of your Game Over / Lose Panel.")]
	[SerializeField] private GameObject losePanel;
	[SerializeField] private GameObject winPanel;
	[SerializeField] private AudioClip winSFX;

	private bool isGameOver = false;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	private void Start()
	{
		if (losePanel != null)
		{
			losePanel.SetActive(false);
		}
		if (winPanel != null)
		{
			winPanel.SetActive(false);
		}
		Time.timeScale = 1f;
	}

	public void TriggerGameOver()
	{
		if (isGameOver) return;
		isGameOver = true;

		if (losePanel != null)
		{
			losePanel.SetActive(true);
		}

		// Freeze the game when panel is displayed
		Time.timeScale = 0f;

		// 3. Unlock and reveal the cursor for UI interaction
		//Cursor.lockState = CursorLockMode.None;
		//Cursor.visible = true;
	}

	public void TriggerWin()
	{
		if (isGameOver) return;
		isGameOver = true;

		if (winPanel != null) winPanel.SetActive(true);

		if (AudioManager.Instance != null && winSFX != null)
		{
			AudioManager.Instance.PlaySFX(winSFX);
		}
		Time.timeScale = 0f;
	}
}
