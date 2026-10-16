using UnityEngine;

public class FlickeringLights : MonoBehaviour
{

    [SerializeField]private Light currentLight;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;

    [SerializeField] private float flickerSpeed;

    private float timer;


    void Start()
    {
        currentLight = GetComponent<Light>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flickerSpeed) 
        {
            currentLight.intensity = Random.Range(minIntensity, maxIntensity);
            timer = 0;
        }
        
    }
}
