using UnityEngine;

public class LeashRenderer : MonoBehaviour
{
    public GameObject endPoint;

    [SerializeField] private GameObject startPoint;

    [SerializeField] private LineRenderer lr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPoint = gameObject;
        lr.positionCount = 3;
        lr.SetPosition(0, startPoint.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (endPoint)
        {
            lr.SetPosition(1, Vector3.Lerp(startPoint.transform.position, endPoint.transform.position, 0.5f));
            lr.SetPosition(2,endPoint.transform.position);
        }
    }
}
