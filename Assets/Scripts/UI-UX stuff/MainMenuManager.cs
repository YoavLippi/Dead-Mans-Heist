using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	[SerializeField] private string firstLevelSceneName = "Level 1";

	void Start()
	{
		// Ensure cursor is visible and free to click buttons
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 1f;
	}

	public void StartGame()
	{
		//if (AudioManager.Instance != null && AudioManager.Instance.oceanAmbience != null)
		//{
		//	AudioManager.Instance.PlayMusic(AudioManager.Instance.oceanAmbience, 0.6f);
		//}

		Debug.Log($"[MAIN MENU] Loading scene: {firstLevelSceneName}");
		SceneManager.LoadScene(firstLevelSceneName);
	}

	public void ExitGame()
	{
		Debug.Log("[MAIN MENU] Exiting game application...");
		Application.Quit();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
}
