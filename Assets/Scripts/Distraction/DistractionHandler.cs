using System;
using System.Collections.Generic;
using UnityEngine;

public class DistractionHandler : Interactable
{
    public enum DistractionSeverity
    {
        Severe,
        Moderate,
        Minor
    }

    [Header("Editor")] 
    [SerializeField] private bool alwaysDrawArea;
    [SerializeField] private bool drawWireframeOnly;
    [SerializeField] private Color sphereColour;
    [SerializeField] private Color wireColour;
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
        if (alwaysDrawArea)
        {
            DrawGizmo();
        }
        //Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = wireColour;
        Gizmos.DrawWireSphere(Vector3.zero, distractionRadius);
        if (drawWireframeOnly) return;
        Gizmos.color = sphereColour;
        Gizmos.DrawSphere(Vector3.zero, distractionRadius);
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

    /*public void DoDistraction()
    {
        //List<Enemy> enemiesInRange = new List<Enemy>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distractionRadius);
        foreach (var col in hitColliders)
        {
            if (!col.CompareTag("Enemy")) continue;
            
            if (col.GetComponent<EnemyAbs>())
            {
                //enemiesInRange.Add(col.GetComponent<Enemy>());
                col.GetComponent<EnemyAbs>().GetDistracted(transform, thisSeverity);
            }
        }
    }*/


    public override void DoInteract()
    {
        //List<Enemy> enemiesInRange = new List<Enemy>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distractionRadius);
        foreach (var col in hitColliders)
        {
            if (!col.CompareTag("Enemy")) continue;
            
            if (col.GetComponent<EnemyAbs>())
            {
                //enemiesInRange.Add(col.GetComponent<Enemy>());
                col.GetComponent<EnemyAbs>().GetDistracted(transform, thisSeverity);
            }
        }
    }
}
