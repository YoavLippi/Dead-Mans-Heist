using UnityEngine;
using static Unity.VisualScripting.Member;

public class PlayAudio : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;

    public void play(AudioClip clip) 
    {
        Debug.Log($"[TutorialAudio] Play called. clip={(clip ? clip.name : "NULL")}, source={(audioSource ? "ok" : "NULL")}");
        if (clip == null && audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

}
