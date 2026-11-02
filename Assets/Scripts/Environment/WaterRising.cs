using System.Collections;
using UnityEngine;

public class WaterRising : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 2f;

 
    public void StartRisingIndefinitely()
    {
        StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine()
    {
       
        while (true)
        {
          
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
