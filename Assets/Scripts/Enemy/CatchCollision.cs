using UnityEngine;

public class CatchCollision : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (GameOverManager.Instance != null)
			{
				GameOverManager.Instance.TriggerGameOver();
			}
			Debug.Log("Dead");
		}
	}
}
