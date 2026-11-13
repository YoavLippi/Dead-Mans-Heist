using DefaultNamespace;
using UnityEngine;

public class RegionVoicelines : MonoBehaviour, iSaveable
{
    [SerializeField] private PlayAudio playAudio;
    [SerializeField] private AudioClip clip;
    [SerializeField] private bool playOnce;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playOnce) 
        {
            playAudio.Play(clip);
            playOnce = true;
        }
    }

    public void PlayAudio() 
    {
        playAudio.Play(clip);
    }

    public void OnEnable()
    {
           ((iSaveable)this).OnEnable();    
    }

    public void OnDisable()
    {
        ((iSaveable)this).OnDisable();
    }

    public void DoLoad() 
    {
        playOnce = false;
    }

    public void DoSave()
    {
        throw new System.NotImplementedException();
    }
}
