using DefaultNamespace;
using UnityEngine;

public class Plank : MonoBehaviour, iSaveable
{
    private Rigidbody rb;
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        savedPosition = transform.position;
        savedRotation = transform.rotation;
    }

    private void OnEnable()
    {
        ((iSaveable)this).RegisterSaveEvents();
    }

    private void OnDisable()
    {
        ((iSaveable)this).DeregisterSaveEvents();
    }

    public void DoSave()
    {
        savedPosition = transform.position;
        savedRotation = transform.rotation;
    }

    public void DoLoad()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
            rb.position = savedPosition;
            rb.rotation = savedRotation;
        }
        transform.SetPositionAndRotation(savedPosition, savedRotation);
    }
}
