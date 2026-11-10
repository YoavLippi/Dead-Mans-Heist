using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MoveSnapHandler : MonoBehaviour
{
    [SerializeField] private Collider thisCol;
    [SerializeField] private Transform snapTF;
    [SerializeField] private string[] snapIds;

    private void Start()
    {
        thisCol = GetComponent<Collider>();
        thisCol.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Movable m)) return;
        bool flag = false;
        foreach (var id in snapIds)
        {
            if (id == m.MovableID)
            {
                flag = true;
                break;
            }
        }

        if (flag)
        {
            m.IsPlaceable = true;
            m.isInMovingState = false;
            m.DoPlace(true);
            m.transform.position = snapTF.position;
            m.transform.rotation = snapTF.rotation;
        }
    }
}
