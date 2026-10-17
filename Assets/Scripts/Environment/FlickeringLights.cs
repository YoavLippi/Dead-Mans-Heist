using UnityEngine;

public class FlickeringLights : MonoBehaviour
{

    [SerializeField]private Light currentLight;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;

    [SerializeField] private float flickerSpeed;
    [SerializeField] private float targetIntensity;
    [SerializeField] private float smoothing;

    private float timer;


    void Start()
    {
        currentLight = GetComponent<Light>();
        targetIntensity = currentLight.intensity;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flickerSpeed) 
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
            timer = 0;
        }
        currentLight.intensity = Mathf.Lerp(currentLight.intensity, targetIntensity, Time.deltaTime * smoothing);
        
    }
}
