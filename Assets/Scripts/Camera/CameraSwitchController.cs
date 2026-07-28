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
        Matrix4x4 defaultMatrix = Gizmos.matrix;

        Gizmos.matrix = thisCol.transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;
        //Vector3.Scale(thisCol.size, transform.localScale)
        //transform .TransformPoint(thisCol.center)
        Gizmos.DrawWireCube(thisCol.center, thisCol.size);
        Gizmos.matrix = defaultMatrix;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraController.Instance.SetActiveCamera(thisCam);
        }
    }
}
