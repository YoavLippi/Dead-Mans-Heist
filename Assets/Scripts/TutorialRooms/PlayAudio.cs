using UnityEngine;
using static Unity.VisualScripting.Member;

public class PlayAudio : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;

    public void Play(AudioClip clip) 
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

}
