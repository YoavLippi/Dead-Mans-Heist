using UnityEngine;

public class PuzzleDistraction : MonoBehaviour
{
    [SerializeField] private Transform source;
    [SerializeField] private float radius = 10f;
    [SerializeField] private DistractionHandler.DistractionSeverity severity = DistractionHandler.DistractionSeverity.Moderate;

    public void Fire()
    {
        Vector3 pos = source != null ? source.position : transform.position;
        DistractionHandler.DoDistract(pos, radius, severity, false);
    }
}
