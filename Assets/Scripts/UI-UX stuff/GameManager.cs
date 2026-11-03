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
	public GameObject startButton;
	public GameObject restartButton;


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
		}
	}

	public void ChangeState(GameState newState)
	{
		currentState = newState;

		// 1. Manage the main Unified Menu visibility
		bool showMenu = (currentState == GameState.MainMenu || currentState == GameState.Paused);
		//if (unifiedMenuPanel != null) unifiedMenuPanel.SetActive(showMenu);

		// 2. Swap Start and Restart buttons inside that menu
		if (showMenu)
		{
			startButton.SetActive(!hasGameStartedOnce);
			restartButton.SetActive(hasGameStartedOnce);
		}
		
		// Handle time pausing for mechanics/animations
		Time.timeScale = (currentState == GameState.Paused||currentState==GameState.MainMenu) ? 0f : 1f;
	}

	
	#region UI Button Hookups
	public void StartGame()
	{
		hasGameStartedOnce = true; // Flips the switch permanently for this session
		ChangeState(GameState.Playing);

		if (AudioManager.Instance != null && AudioManager.Instance.oceanAmbience != null)
		{
			AudioManager.Instance.PlayMusic(AudioManager.Instance.oceanAmbience, 0.6f);
		}
	}

	//public void RestartGame()
	//{
	//	Time.timeScale = 1f;
	//	hasGameStartedOnce = false;
	//	SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	//	ChangeState(GameState.MainMenu);
	//}

	//public void ResumeGame()
	//{
	//	ChangeState(GameState.Playing);
	//}

	public void ExitGameApplication()
	{
		Debug.Log("Exiting Game Application...");
		Application.Quit();
		// Allows it to quit play mode inside the Unity Editor
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
	#endregion
}

