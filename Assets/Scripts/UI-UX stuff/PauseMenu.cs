using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
	[SerializeField] private GameObject pauseMenuUI;
	[SerializeField] private string mainMenuSceneName = "MainMenu";

	[Header("Player Input Locking")]
	[Tooltip("Assign player movement/camera look scripts here so camera rotation freezes while paused")]
	[SerializeField] private MonoBehaviour[] scriptsToDisableOnPause;

	public static bool isPaused = false;

	private void Start()
	{
		// Ensure the game always starts unpaused when loading a level
		ResumeGame();
	}

	private void Update()
	{
		if (WasPauseKeyPressed())
		{
			if (isPaused)
				ResumeGame();
			else
				PauseGame();
		}
	}

	private bool WasPauseKeyPressed()
	{
		// Supports both the new Input System and the legacy Input Manager
#if ENABLE_INPUT_SYSTEM
		if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
			return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.P))
            return true;
#endif
		return false;
	}

	public void PauseGame()
	{
		isPaused = true;

		if (pauseMenuUI != null)
			pauseMenuUI.SetActive(true);

		// Freeze gameplay time and physics
		Time.timeScale = 0f;

		// Pause all active AudioSources (footsteps, water gushing, ambience)
		AudioListener.pause = true;

		// Unlock and show cursor so the player can click UI buttons
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// Disable camera look / input scripts that still run in Update() when timeScale is 0
		SetScriptsEnabled(false);
	}

	public void ResumeGame()
	{
		isPaused = false;

		if (pauseMenuUI != null)
			pauseMenuUI.SetActive(false);

		Time.timeScale = 1f;
		AudioListener.pause = false;

		// Re-lock and hide cursor for 3D first/third-person gameplay
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		SetScriptsEnabled(true);
	}

	public void RestartLevel()
	{
		// Always reset timeScale and audio before reloading a scene
		Time.timeScale = 1f;
		AudioListener.pause = false;
		isPaused = false;

		Scene currentScene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(currentScene.name);
	}

	public void LoadMainMenu()
	{
		Time.timeScale = 1f;
		AudioListener.pause = false;
		isPaused = false;

		// Keep cursor unlocked for the Main Menu scene
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SceneManager.LoadScene(mainMenuSceneName);
	}

	public void QuitGame()
	{
		Debug.Log("Quitting game...");
		Application.Quit();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

	private void SetScriptsEnabled(bool state)
	{
		if (scriptsToDisableOnPause == null) return;

		foreach (var script in scriptsToDisableOnPause)
		{
			if (script != null)
				script.enabled = state;
		}
	}
}
