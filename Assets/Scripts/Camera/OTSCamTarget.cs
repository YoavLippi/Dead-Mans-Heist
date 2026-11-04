using Unity.Cinemachine;
using UnityEngine;

public class OTSCamTarget : MonoBehaviour
{
    [SerializeField] private GameObject playerObj;
    [SerializeField] private Vector2 playerOffset;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private float moveSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 realOffset = Vector3.zero;
        //transform from cam to a 2D offset

        if (CameraController.Instance.ActiveCam is CinemachineCamera cam)
        {
            realOffset = Camera.main.transform.right * playerOffset.x + Camera.main.transform.up * playerOffset.y;
        }

        //target pos
        Vector3 targetPos = playerObj.transform.position + realOffset;
        Vector3 playerPos = playerObj.transform.position;
        //raycast to make sure it isn't phasing through anything
        if (Physics.SphereCast(playerPos, 1f, (targetPos - playerPos).normalized, out var hit, realOffset.magnitude,
                hitLayers))
        {
            //collided
            targetPos = hit.point;
        }

        //move towards it
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
}