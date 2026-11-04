using UnityEngine;

public class DoorFall : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string parameterName;

    public void SetTrue() => animator.SetBool(parameterName, true);
}