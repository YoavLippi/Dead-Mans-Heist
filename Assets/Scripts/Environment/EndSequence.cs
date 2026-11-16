using System.Collections;
using UnityEngine;

public class EndSequence : MonoBehaviour
{
    [SerializeField] private AudioSource cannon;
    [SerializeField] private AudioClip sizzle;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private GameObject wall;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private ParticleSystem explosionvfx;
    [SerializeField] private Anim cannonanim;


    public void playSequence() 
    {
        StartCoroutine(startEnd());
    }
    public IEnumerator startEnd()
    {
        cannon.PlayOneShot(sizzle);
        yield return new WaitForSeconds(3f);
        cannonanim.SetTrue("CannonShot");
        cannon.PlayOneShot(explosion);
        wall.SetActive(false);
        smoke.Play();
        explosionvfx.Play();

    }
}
