using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class OTSCameraController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private float zoomSpeed = 2;
    [SerializeField] private float zoomLerpSpeed = 10;
    [SerializeField] private float minDist = 3;
    [SerializeField] private float maxDist = 15;

    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private float scrollDelta;

    private float targetZoom;
    private float currentZoom;

    //lots of reference to https://www.youtube.com/watch?v=o7O28SFGWS4
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = GetComponent<CinemachineCamera>();
        orbital = GetComponent<CinemachineOrbitalFollow>();

        targetZoom = currentZoom = orbital.Radius;
    }

    public void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<float>();
    }

    private void Update()
    {
        if (scrollDelta != 0)
        {
            targetZoom = math.clamp(orbital.Radius - scrollDelta * zoomSpeed, minDist, maxDist);
            scrollDelta = 0;
        }

        currentZoom = math.lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }
}
