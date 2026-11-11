using UnityEngine;

public class AnimCollide : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string param;
    [SerializeField] private bool condition;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            _animator.SetBool(param, condition);
        }
    }
}
