using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    [SerializeField] private CinemachineBrain camBrain;
    [SerializeField] private ICinemachineCamera activeCam;
    [SerializeField] private GameObject ghostVisionPanel;

    public CinemachineBrain CamBrain => camBrain;

    public ICinemachineCamera ActiveCam => activeCam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        camBrain = GetComponent<CinemachineBrain>();
        //activeCam = camBrain.ActiveVirtualCamera;
        if (camBrain.ActiveVirtualCamera is CinemachineCamera cam)
        {
            SetActiveCamera(cam);
        }
    }

    public void SetActiveCamera(CinemachineCamera newActive)
    {
        if (activeCam != null && activeCam is CinemachineCamera cam)
        {
            cam.Priority = 10;
        }
        newActive.Priority = 20;
        ghostVisionPanel.transform.position = (newActive.transform.position + newActive.transform.forward);
        ghostVisionPanel.transform.rotation = newActive.transform.rotation;
        activeCam = newActive;
    }
}
