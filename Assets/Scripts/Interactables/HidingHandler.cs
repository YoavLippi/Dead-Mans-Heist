using UnityEngine;
using UnityEngine.Serialization;

public class HidingHandler : Interactable
{
    [Header("Editor")] 
    [SerializeField] private bool alwaysDrawArea;
    [SerializeField] private bool drawWireframeOnly;
    [SerializeField] private Color sphereColour;
    [SerializeField] private Color wireColour;
    [SerializeField] private float hideRadius;
    
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
        //Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = wireColour;
        Gizmos.DrawWireSphere(transform.position, hideRadius);
        if (drawWireframeOnly) return;
        Gizmos.color = sphereColour;
        Gizmos.DrawSphere(transform.position, hideRadius);
    }
    
    public override void DoInteract()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, hideRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            //seems expensive, but it should only ever run the number of times equal to the number of colliders on the player
            if (hitCollider.CompareTag("Player") && hitCollider.TryGetComponent<PlayerController>(out PlayerController controller))
            {
                if (controller.CurrentState != PlayerController.PlayerState.Hiding)
                {
                    controller.CurrentState = PlayerController.PlayerState.Hiding;
                }
            }
        }
    }
}
