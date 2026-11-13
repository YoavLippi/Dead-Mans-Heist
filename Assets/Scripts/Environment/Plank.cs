using DefaultNamespace;
using UnityEngine;

public class Plank : MonoBehaviour, iSaveable
{

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    void Start()
    {
       savedPosition = transform.position;
        savedRotation = transform.rotation;
    }
    public void DoLoad()
    {
        transform.position = savedPosition;
        transform.rotation = savedRotation;
    }

    public void DoSave()
    {
        throw new System.NotImplementedException();
    }

}
