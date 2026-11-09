using UnityEngine;

public class Anim : MonoBehaviour
{
    [SerializeField] private Animator animator;

    
    public void SetTrue(string animName) 
    {
        animator.SetBool(animName, true);
    }
    public void SetFalse(string animName)
    {
        animator.SetBool(animName, false);
    }
}
