using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	public enum GameState { MainMenu, Playing, Paused }
	[Header("Current State")]
	public GameState currentState = GameState.MainMenu;

	[Header("Unified Menu UI Panels")]
	[Tooltip("The shared canvas panel used for both Starting and Pausing the game.")]
	public GameObject unifiedMenuPanel;
	public GameObject startButton;
	public GameObject restartButton;

	[Header("Ghost Vision Elements")]
	[Tooltip("The 2D World Space object with the sprite tint.")]
	public GameObject ghostVisionWorldObject;
	[Tooltip("The UI Canvas overlay that displays text/images when Ghost Vision is active.")]
	public GameObject ghostVisionCanvasUI;

	[Header("Ghost Mode Settings")]
	public bool isGhostModeActive = false;
	private bool hasGameStartedOnce = false;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		//DontDestroyOnLoad(gameObject);
	}

	void Start()
	{
		hasGameStartedOnce = false;
		ChangeState(GameState.MainMenu);
	}

	void Update()
	{
		if (Keyboard.current != null)
		{
			// Toggle Pause Menu (Escape key) if we are currently playing or paused
			if (Keyboard.current.escapeKey.wasPressedThisFrame)
			{
				if (currentState == GameState.Playing) ChangeState(GameState.Paused);
				else if (currentState == GameState.Paused) ChangeState(GameState.Playing);
			}

			// Toggle Ghost Mode (G key) ONLY while actively playing the game
			if (Keyboard.current.gKey.wasPressedThisFrame && currentState == GameState.Playing)
			{
				ToggleGhostVision(!isGhostModeActive);
			}
		}
	}

	public void ChangeState(GameState newState)
	{
		currentState = newState;

		// 1. Manage the main Unified Menu visibility
		bool showMenu = (currentState == GameState.MainMenu || currentState == GameState.Paused);
		if (unifiedMenuPanel != null) unifiedMenuPanel.SetActive(showMenu);

		// 2. Swap Start and Restart buttons inside that menu
		if (showMenu)
		{
			startButton.SetActive(!hasGameStartedOnce);
			restartButton.SetActive(hasGameStartedOnce);
		}

		// 3. If we pause or quit, cleanly force Ghost Vision off
		if (currentState != GameState.Playing)
		{
			ToggleGhostVision(false);
		}

		// Handle time pausing for mechanics/animations
		Time.timeScale = (currentState == GameState.Paused||currentState==GameState.MainMenu) ? 0f : 1f;
	}

	public void ToggleGhostVision(bool active)
	{
		isGhostModeActive = active;

		if (ghostVisionWorldObject != null)
		{
			ghostVisionWorldObject.SetActive(isGhostModeActive);
		}

		if (ghostVisionCanvasUI != null)
		{
			ghostVisionCanvasUI.SetActive(isGhostModeActive);
		}
	}

	#region UI Button Hookups
	public void StartGame()
	{
		hasGameStartedOnce = true; // Flips the switch permanently for this session
		ChangeState(GameState.Playing);
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		hasGameStartedOnce = false;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		ChangeState(GameState.MainMenu);
	}

	public void ResumeGame()
	{
		ChangeState(GameState.Playing);
	}

	public void ExitGameApplication()
	{
		Debug.Log("Exiting Game Application...");
		Application.Quit();
	}
	#endregion
}

