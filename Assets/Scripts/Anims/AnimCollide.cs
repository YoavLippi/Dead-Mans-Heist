using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;

public class AnimCollide : MonoBehaviour, iSaveable
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string param;
    [SerializeField] private bool condition;
    [SerializeField] private UnityEvent events;

    private bool savedValue;

    private void Awake()
    {
        savedValue = _animator.GetBool(param);
    }

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
        if (other.CompareTag("Player"))
        {
            events.Invoke();
            _animator.SetBool(param, condition);
        }
    }

    public void DoSave()
    {
        savedValue = _animator.GetBool(param);
    }

    public void DoLoad()
    {
        _animator.SetBool(param, savedValue);
    }
}
