using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class LosHandler : MonoBehaviour
{
    [SerializeField] private Transform eyePos;
    [SerializeField] private float rayDistance;
    [Range(0,360)][SerializeField] private float fieldOfView;
    [SerializeField] private int rayNumber;
    [SerializeField] private List<LayerMask> ignoreLayers;
    [SerializeField] private GameObject coneMeshHolder;

    // Update is called once per frame
    void Update()
    {
        LayerMask totalMask = 0;
        foreach (var mask in ignoreLayers)
        {
            totalMask |= mask;
        }
        //bitwise NOT to make it so we ignore those layers
        totalMask = ~totalMask;
        
        RaycastHit hitInfo;
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
        DrawShape(rayPoints);
    }

    //generating new mesh
    private void DrawShape(List<Vector3> points)
    {
        List<int> tris = new List<int>();
        //getting tris
        for (int i = 0; i < points.Count; i++)
        {
            if (i >= 2)
            {
                tris.Add(0);
                tris.Add(i-1);
                tris.Add(i);
            }
        }
        //we need just the verts and tris for now
        Mesh cone = new Mesh()
        {
            vertices = points.ToArray(),
            triangles = tris.ToArray(),
        };
        cone.RecalculateNormals();
        cone.name = "viewCone";
        
        coneMeshHolder.GetComponent<MeshFilter>().mesh = cone;
    }
}
