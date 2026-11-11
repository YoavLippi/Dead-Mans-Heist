using UnityEngine;

public class RegionVoicelines : MonoBehaviour
{
    [SerializeField] private PlayAudio playAudio;
    [SerializeField] private AudioClip clip;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            playAudio.Play(clip);
        }
    }
}
