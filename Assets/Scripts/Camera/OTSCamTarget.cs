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
        realOffset = Camera.main.transform.right * playerOffset.x + Camera.main.transform.up * playerOffset.y;

        //target pos
        Vector3 targetPos = playerObj.transform.position + realOffset;
        Vector3 playerPos = playerObj.transform.position;
        //raycast to make sure it isn't phasing through anything
        Vector3 dir = (targetPos - playerPos).normalized;
        if (Physics.SphereCast(playerPos, 1f, dir, out var hit, realOffset.magnitude,
                hitLayers))
        {
            //collided
            targetPos = playerPos + dir * hit.distance;
        }

        //move towards it
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
}