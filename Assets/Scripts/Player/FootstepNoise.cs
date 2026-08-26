using UnityEngine;

public class FootstepNoise : MonoBehaviour
{
	public void TriggerFootstepAlert(float radius, DistractionHandler.DistractionSeverity severity, bool isLowPriority)
	{
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
		foreach (var col in hitColliders)
		{
			if (!col.CompareTag("Enemy")) continue;

			EnemyAbs enemy = col.GetComponent<EnemyAbs>();
			if (enemy == null) continue;

			if (!isLowPriority || enemy.CurrentState != EnemyState.Distracted)
			{
				enemy.GetDistracted(transform.position, severity);
			}
		}
	}
}
