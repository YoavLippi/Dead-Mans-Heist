using UnityEngine;

public class GateAnim : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void ResetGate()
    {
        animator.SetBool("GateUp", false);
    }
}