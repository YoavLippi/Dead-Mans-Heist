using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unity.Cinemachine;

[RequireComponent(typeof(PlayableDirector))]
public class CutscenePlayer : MonoBehaviour
{
    public bool playOnStart = false;
    public UnityEvent onStarted;   // for the disabling player input
    public UnityEvent onFinished;  // enable input or to load next scene

    PlayableDirector director;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.playOnAwake = false;
        director.stopped += _ => onFinished?.Invoke();
    }

    void Start() { if (playOnStart) Play(); }

    [ContextMenu("Play Cutscene")]
    public void Play()
    {
        var brain = FindFirstObjectByType<CinemachineBrain>();
        var timeline = director.playableAsset as TimelineAsset;
        foreach (var track in timeline.GetOutputTracks())
            if (track is CinemachineTrack)
            {
                director.SetGenericBinding(track, brain);
            }
                
        onStarted?.Invoke();
        director.Play();
    }

    public void Skip()
    {
        director.time = director.duration;
        director.Evaluate();
        director.Stop();
    }
}