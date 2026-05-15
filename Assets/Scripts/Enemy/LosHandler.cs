using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class LosHandler : MonoBehaviour
{
    [SerializeField] private Transform eyePos;
    [SerializeField] private float rayDistance;
    [Range(0,360)][SerializeField] private float fieldOfView;
    [SerializeField] private int rayNumber;
    [SerializeField] private List<LayerMask> ignoreLayers;
    [SerializeField] private MeshFilter coneMeshFilter;
    [SerializeField] private bool isLooking;

    /*[SerializeField] private List<test> tester;

    [Serializable]
    public struct test
    {
        public int val;
        public UnityEvent val2;
    }*/

    //RUNTIME VARS
    private LayerMask totalMask;
    private List<Vector3> rayPoints;
    private Mesh coneMesh;
    //staggering checks so that not all raycasters update at once
    private float nextIntervalMilis;

    private void Start()
    {
        totalMask = 0;
        foreach (var mask in ignoreLayers)
        {
            totalMask |= mask;
        }
        //bitwise NOT to make it so we ignore those layers
        totalMask = ~totalMask;
        rayPoints = new List<Vector3>();

        coneMesh = new Mesh();
        coneMesh.name = "ViewCone";
        coneMesh.MarkDynamic();
        coneMeshFilter.mesh = coneMesh;
        
        
        StartLooking();
    }

    // Update is called once per frame
    void Update()
    {
        /*RaycastHit hitInfo;
        float angleChange = fieldOfView / rayNumber;
        float offsetFOV = -fieldOfView / 2f;
        List<Vector3> rayPoints = new List<Vector3>();
        rayPoints.Add(transform.InverseTransformPoint(eyePos.position));
        for (int i = 0; i < rayNumber; i++)
        {
            //we need to adjust the angle for each new ray
            float currentAngle = offsetFOV + angleChange * i;
            Vector3 lookDir = Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward;
            
            //Debug.DrawLine(eyePos.position, eyePos.position+(rayDistance*lookDir), Color.red);
            Vector3 finalPoint = eyePos.position;
            if (Physics.Raycast(eyePos.position, lookDir, out hitInfo, rayDistance, totalMask))
            {
                finalPoint = hitInfo.point;
            }
            else
            {
                finalPoint = eyePos.position + lookDir * rayDistance;
            }
            Debug.DrawLine(eyePos.position, finalPoint, Color.red);
            rayPoints.Add(transform.InverseTransformPoint(finalPoint));
        }
        //rayPoints.Add(eyePos.position);
        DrawShape(rayPoints);*/
    }

    public void StartLooking()
    {
        isLooking = true;
        StartCoroutine(DoVision());
    }

    public void StopLooking()
    {
        isLooking = false;
    }

    private IEnumerator DoVision()
    {
        while (isLooking)
        {
            rayPoints.Clear();
            float angleChange = fieldOfView / rayNumber;
            float offsetFOV = -fieldOfView / 2f;
            rayPoints.Add(transform.InverseTransformPoint(eyePos.position));
            for (int i = 0; i < rayNumber; i++)
            {
                //we need to adjust the angle for each new ray
                float currentAngle = offsetFOV + angleChange * i;
                //Vector3 lookDir = Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward;
                
                //Optimisation from online, instead of the Quaternion approach from before
                float radians = currentAngle * Mathf.Deg2Rad;
                Vector3 lookDir = transform.forward * Mathf.Cos(radians) + transform.right * Mathf.Sin(radians);
            
                //Debug.DrawLine(eyePos.position, eyePos.position+(rayDistance*lookDir), Color.red);
                Vector3 finalPoint;
                
                RaycastHit hitInfo;
                if (Physics.Raycast(eyePos.position, lookDir, out hitInfo, rayDistance, totalMask))
                {
                    finalPoint = hitInfo.point;
                }
                else
                {
                    finalPoint = eyePos.position + lookDir * rayDistance;
                }
                
                #if UNITY_EDITOR
                Debug.DrawLine(eyePos.position, finalPoint, Color.red);
                #endif
                
                rayPoints.Add(transform.InverseTransformPoint(finalPoint));
            }

            DrawShape(rayPoints);
            nextIntervalMilis = Random.Range(30, 76);
            yield return new WaitForSeconds(nextIntervalMilis / 1000);
        }
    }

    //generating new mesh
    private void DrawShape(List<Vector3> points)
    {
        //there are n-2 tris for n points, 3 values per tri
        int[] trisArr = new int[(points.Count - 2) * 3];
        int triCount = 0;
        //getting tris
        for (int i = 0; i < points.Count; i++)
        {
            if (i >= 2)
            {
                trisArr[triCount] = 0;
                triCount++;
                trisArr[triCount] = i - 1;
                triCount++;
                trisArr[triCount] = i;
                triCount++;
            }
        }
        
        //Removing this because it's very expensive to recreate each time
        //we need just the verts and tris for now
        /*Mesh cone = new Mesh()
        {
            vertices = points.ToArray(),
            triangles = tris.ToArray(),
        };*/

        coneMesh.vertices = points.ToArray();
        coneMesh.triangles = trisArr;
        coneMesh.RecalculateNormals();
    }
}
