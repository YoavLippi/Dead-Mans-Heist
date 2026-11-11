using System.Collections;
using UnityEngine;

public class EndSequence : MonoBehaviour
{
    [SerializeField] private AudioSource cannon;
    [SerializeField] private AudioClip sizzle;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private GameObject wall;


    public void playSequence() 
    {
        StartCoroutine(startEnd());
    }
    public IEnumerator startEnd()
    {
        cannon.PlayOneShot(sizzle);
        yield return new WaitForSeconds(3f);
        cannon.PlayOneShot(explosion);
        wall.SetActive(false);

    }
}
