using System;
using UnityEditor;
using UnityEngine;

public class DrawAxisGizmo : MonoBehaviour
{
    [SerializeField] private float arrowSize;

    [SerializeField] private bool alwaysDrawGizmo;

    private void OnDrawGizmosSelected()
    {
        if (!alwaysDrawGizmo)
        {
            DrawGizmo();
        }
    }

    private void OnDrawGizmos()
    {
        if (alwaysDrawGizmo)
        {
            DrawGizmo();
        }
    }

    private void DrawGizmo()
    {
        Color prevColor = Handles.color;
        //forward
        Handles.color = Color.purple;
        Handles.ArrowHandleCap(0,transform.position, transform.rotation*Quaternion.LookRotation(Vector3.forward),arrowSize, EventType.Repaint);

        Handles.color = prevColor;
    }
}
