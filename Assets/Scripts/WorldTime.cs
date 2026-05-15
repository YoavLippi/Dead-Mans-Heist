using System;
using TMPro;
using UnityEngine;

public class WorldTime : MonoBehaviour
{
    public static event Action<int> secondsChange;

    [SerializeField] private float seconds = 1f;
   [SerializeField] private int currentSecond = 0;
    private float timer = 0f;
    [SerializeField]private TextMeshProUGUI text;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= seconds) 
        {
            timer -= seconds;
            convertSeconds();
            currentSecond++;
            secondsChange?.Invoke(currentSecond);
        }
    }

    private void convertSeconds() 
    {
        if (text != null) {
        TimeSpan time = TimeSpan.FromSeconds(currentSecond);
        text.text = time.ToString("mm':'ss");


    } }
}
