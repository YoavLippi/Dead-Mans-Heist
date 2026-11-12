using UnityEngine;

public class RegionVoicelines : MonoBehaviour
{
    [SerializeField] private PlayAudio playAudio;
    [SerializeField] private AudioClip clip;
    [SerializeField] private bool playonce;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playonce) 
        {
            playAudio.Play(clip);
            playonce = true;
        }
    }
}
