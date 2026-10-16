using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class Coin : Tool
{
    [SerializeField] private Material sphereMat;
    [SerializeField] private GameObject distractionSphere;
    [SerializeField] private float distractionRadius;
    [SerializeField] private DistractionHandler.DistractionSeverity distractionSeverity;
    [SerializeField] private LayerMask mouseIgnoreLayers;
    [SerializeField] private LayerMask rayIgnoreLayers;
    [SerializeField] private float maxCoinDistance=20;
    
    private void FixedUpdate()
    {
        if (!IsSelected) return;
        //~ignoreLayers
        if (!distractionSphere) return;
        Vector2 mousePos = Mouse.current.position.value;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out var hit, 1000, ~mouseIgnoreLayers))
        {
            Vector3 hitPoint = hit.point;
            //second raycast from where the player is to where the mouse is. should let us block throwing
            //coins through walls etc.
            Vector3 startPos = transform.position;
            ray = new Ray(startPos, hitPoint - startPos);
            Debug.DrawRay(startPos, hitPoint - startPos);
            if (Physics.Raycast(ray, out var secHit, maxCoinDistance, ~rayIgnoreLayers))
            {
                distractionSphere.transform.position = secHit.point;
            }
            else
            {
                //hit max distance
                Vector3 maxDistancePoint = startPos + ((hitPoint - startPos).normalized * maxCoinDistance);
                distractionSphere.transform.position = maxDistancePoint;
            }
        }
    }

    public override void HandleSelection(bool val)
    {
        if (val)
        {
            distractionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            distractionSphere.GetComponent<MeshRenderer>().material = sphereMat;
            Rigidbody rb = distractionSphere.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            Collider c = distractionSphere.GetComponent<SphereCollider>();
            c.isTrigger = true;
            distractionSphere.layer = LayerMask.NameToLayer("Triggers");
            distractionSphere.transform.localScale =
                new Vector3(distractionRadius*2, distractionRadius*2, distractionRadius*2);
        }
        else
        {
            if (distractionSphere)
            {
                Destroy(distractionSphere.gameObject);
            }
        }
    }

    public override void Use()
    {
        if (!CanBeUsed()) return;
        base.Use();
        Charges--;
        DistractionHandler.DoDistract(distractionSphere.transform.position, distractionRadius, distractionSeverity, false);
    }
}
