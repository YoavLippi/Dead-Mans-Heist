using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider))]
public class CameraSwitchController : MonoBehaviour
{
    [Header("Editor")]
    [SerializeField] private bool alwaysDrawGizmo;
    [SerializeField] private Color gizmoColor = Color.blue;
    [SerializeField] private CinemachineCamera thisCam;
    [SerializeField] private BoxCollider thisCol;

    private void Start()
    {
        //thisCam = GetComponent<CinemachineCamera>();
        thisCol = GetComponent<BoxCollider>();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo();
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
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform .TransformPoint(thisCol.center), Vector3.Scale(thisCol.size, transform.localScale));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraController.Instance.SetActiveCamera(thisCam);
        }
    }
}
