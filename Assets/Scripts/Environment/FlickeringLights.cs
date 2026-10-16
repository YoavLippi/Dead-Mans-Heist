using UnityEngine;

public class FlickeringLights : MonoBehaviour
{

    [SerializeField]private Light light;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;

    [SerializeField] private float flickerSpeed;

    private float timer;


    void Start()
    {
        light = GetComponent<Light>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flickerSpeed) 
        {
            light.intensity = Random.Range(minIntensity, maxIntensity);
            timer = 0;
        }
        
    }
}
