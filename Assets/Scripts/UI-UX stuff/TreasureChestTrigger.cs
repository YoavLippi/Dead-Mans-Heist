using UnityEngine;

public class ChestCollide : MonoBehaviour
{
	private bool treasureCollected = false;

	private void OnTriggerEnter(Collider other)
	{
		if (treasureCollected) return;

		if (other.CompareTag("Player"))
		{
			treasureCollected = true;
			if (GameOverManager.Instance != null)
			{
				GameOverManager.Instance.TriggerWin();
			}
		}
	}
}
