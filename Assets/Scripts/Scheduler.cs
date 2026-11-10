using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Scheduler : MonoBehaviour
{
    [Serializable]
    public class EventWrapper
    {
        [SerializeField, ReadableOnly] public string name;
        [SerializeField] public int time;
        [SerializeField] public UnityEvent onTimeReached;
        
        public void OnValidate()
        {
            name = $"Event at {time} seconds";
        }

        public void Invoke()
        {
            onTimeReached.Invoke();
        }
    }
    [SerializeField] private int lastKnownTime;
    [SerializeField] private bool isOnSchedule = true;
    [SerializeField] private List<EventWrapper> schedule;
    private void OnEnable() => WorldTime.secondsChange += CheckTime;

    private void OnDisable() => WorldTime.secondsChange -= CheckTime;

    public bool IsOnSchedule
    {
        get => isOnSchedule;
        set => isOnSchedule = value;
    }

    private void CheckTime(int time)
    {
        time %= GetHighestTriggerTime()+1;
        lastKnownTime = time;
        if (!isOnSchedule) return;
        var currentEvents = schedule.Where(p => p.time == lastKnownTime);
        foreach (var e in currentEvents)
        {
            Debug.Log($"Invoking event for time: {lastKnownTime}");
            e.Invoke();
        }
    }

    private int GetHighestTriggerTime()
    {
        return schedule.Max(p => p.time);
    }

    private void OnValidate()
    {
        foreach (var ew in schedule)
        {
            ew.OnValidate();
        }
    }
}
