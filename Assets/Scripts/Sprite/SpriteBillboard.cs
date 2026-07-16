using System;
using Unity.Cinemachine;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        //you actually can't be serious wtf why is this the thing that works
        //lookat makes the sprites twist, so we're just going to copy the camera's base rotation
        Quaternion cameraRot = Quaternion.identity;
        if (CameraController.Instance.ActiveCam is CinemachineCamera cam)
        {
            cameraRot = cam.transform.rotation;
        }

        transform.rotation = cameraRot;
        //transform.Rotate(0,180f,0);
    }
}