using System.Collections;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] private string requiredKey;
    [SerializeField] private bool isLocked;
    [SerializeField] private Transform doorPivot;
    [SerializeField] private Transform closedPivot;
    [SerializeField] private float openAngle = 90;
    [SerializeField] private float smoothSpeed = 2;

    [SerializeField] private bool isOpening = false;
    [SerializeField] private bool isOpen = false;
    [SerializeField]private Quaternion targetRotation;



    void Start()
    {
        if (doorPivot == null) 
        {
            doorPivot = transform;
            closedPivot.rotation = doorPivot.rotation;
        }
        targetRotation = doorPivot.rotation * Quaternion.Euler(0,openAngle,0);

    }

    void Update()
    {
        if (isOpening) 
        {
            doorPivot.rotation = Quaternion.Slerp(doorPivot.rotation, targetRotation, Time.deltaTime * smoothSpeed);
            isOpen = true;
        }
    }
    public IEnumerator waitToClose() 
    {
        yield return new WaitForSeconds(2);
        doorPivot.rotation = Quaternion.Slerp(doorPivot.rotation, closedPivot.rotation, Time.deltaTime * smoothSpeed);
        isOpening = false;
        isOpening = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory playerInventory = GetComponent<PlayerInventory>();
        if (isLocked && other.CompareTag("Player"))
        {
            if (playerInventory.HasKey(requiredKey))
            {
                isLocked = false;
                isOpening = true;
                Debug.Log("Opening door");
            }
            else
            {
                //Aryanna could you add liked a locked door noise
            }
        }
        else 
        {
            isOpening = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isOpen && other.CompareTag("Player")) 
        {
            StartCoroutine(waitToClose());
        }
    }
}
