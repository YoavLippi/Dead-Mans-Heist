using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 2f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    public bool IsOpen { get; private set; }

    void Start()
    {
        if (doorPivot == null) doorPivot = transform;

        openRotation = doorPivot.localRotation;
        closedRotation = openRotation * Quaternion.Euler(0f, -openAngle, 0f);
        targetRotation = openRotation;
        IsOpen = true;
    }

    void Update()
    {
        doorPivot.localRotation = Quaternion.Slerp(
            doorPivot.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed);
    }

    public void Open()
    {
        IsOpen = true;
        targetRotation = openRotation;
    }

    public void Close()
    {
        Debug.Log("close");
        IsOpen = false;
        targetRotation = closedRotation;
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }
}