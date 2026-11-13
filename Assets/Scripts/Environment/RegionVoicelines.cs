using DefaultNamespace;
using UnityEngine;

public class RegionVoicelines : MonoBehaviour, iSaveable
{
    [SerializeField] private PlayAudio playAudio;
    [SerializeField] private AudioClip clip;
    [SerializeField] private bool playOnce;
    private bool hasPlayed;      
    private bool savedHasPlayed;

     private void OnEnable()
    {
        ((iSaveable)this).RegisterSaveEvents();
    }

    private void OnDisable()
    {
        ((iSaveable)this).DeregisterSaveEvents();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayed)
        {
            playAudio.Play(clip);
            hasPlayed = true;
        }
    }

    public void PlayAudio()
    {
        playAudio.Play(clip);
    }

    public void DoSave()
    {
        savedHasPlayed = hasPlayed;
    }

    public void DoLoad()
    {
        hasPlayed = savedHasPlayed;
    }
}
