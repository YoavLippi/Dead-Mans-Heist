using System;
using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    [SerializeField] private Scheduler associatedScheduler;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Gradient timerChangeGradient;

    private void OnEnable()
    {
        associatedScheduler.OnTimeChanged += UpdateTime;
    }

    private void OnDisable()
    {
        associatedScheduler.OnTimeChanged -= UpdateTime;
    }

    private void UpdateTime(int value)
    {
        int highestTime = associatedScheduler.HighestTimeValue;
        TimeSpan ts = TimeSpan.FromSeconds(highestTime-value);
        text.text = $"{ts.Minutes:D1}:{ts.Seconds:D2}";
        text.color = timerChangeGradient.Evaluate((float)value / highestTime);
    }
}
