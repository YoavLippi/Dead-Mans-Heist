using System;
using System.Collections.Generic;
using UnityEngine;

public class DistractionHandler : MonoBehaviour
{
    public enum DistractionSeverity
    {
        Severe,
        Moderate,
        Minor
    }

    [Header("Setup")]
    [SerializeField] private DistractionSeverity thisSeverity;
    [SerializeField] private float distractionRadius;

    public DistractionSeverity ThisSeverity
    {
        get => thisSeverity;
        set => thisSeverity = value;
    }

    public float DistractionRadius
    {
        get => distractionRadius;
        set => distractionRadius = value;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.4f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(Vector3.zero, distractionRadius);
        Gizmos.color = new Color(1f, 0.6f, 0.2f);
        Gizmos.DrawWireSphere(Vector3.zero, distractionRadius);
        //Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*Enemy a = new Enemy();
        a.GetDistracted(transform, thisSeverity);*/
    }

    private void Update()
    {
        //UseDistraction();
    }

    public void DoDistraction()
    {
        //List<Enemy> enemiesInRange = new List<Enemy>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distractionRadius);
        foreach (var col in hitColliders)
        {
            if (!col.CompareTag("Enemy")) continue;
            
            if (col.GetComponent<Enemy>())
            {
                //enemiesInRange.Add(col.GetComponent<Enemy>());
                col.GetComponent<Enemy>().GetDistracted(transform, thisSeverity);
            }
        }
    }
}
