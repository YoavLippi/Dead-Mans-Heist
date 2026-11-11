using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    [SerializeField] private CinemachineBrain camBrain;
    [SerializeField] private ICinemachineCamera activeCam;
    [FormerlySerializedAs("ghostVisionPanel")] [SerializeField] private GameObject visionPanel;
    public bool isGhostPanelActive;

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
        visionPanel.transform.position = (newActive.transform.position + newActive.transform.forward);
        visionPanel.transform.rotation = newActive.transform.rotation;
        visionPanel.transform.parent = newActive.transform;
        activeCam = newActive;
    }
    public void ToggleVisionPanel(bool value)
    {
        isGhostPanelActive = value;
        if (visionPanel != null)
        {
            visionPanel.SetActive(value);
        }
    }

    public void FadeToBlackAndReload(float waitTime)
    {
        StartCoroutine(DoFadeWithWaitTime(waitTime, 0.5f));
    }

    private IEnumerator DoFadeWithWaitTime(float waitTime, float fadeTime)
    {
        Time.timeScale = 0;
        ToggleVisionPanel(true);
        SpriteRenderer r = visionPanel.GetComponent<SpriteRenderer>();
        yield return new WaitForSecondsRealtime(waitTime);
        Color initialColor = r.color, finalColor = Color.black;
        float currentTime = 0;
        do
        {
            r.color = Color.Lerp(initialColor, finalColor, currentTime / fadeTime);
            yield return new WaitForEndOfFrame();
            currentTime += Time.unscaledDeltaTime;
        } while (currentTime<fadeTime);

        yield return new WaitForSecondsRealtime(0.3f);
        WorldController.DoLoad();
        r.color = initialColor;
        ToggleVisionPanel(false);
        Time.timeScale = 1;
    }
}
