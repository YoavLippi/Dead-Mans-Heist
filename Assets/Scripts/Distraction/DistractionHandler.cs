using UnityEngine;

public class DistractionHandler : MonoBehaviour
{
    public enum DistractionSeverity
    {
        Severe,
        Moderate,
        Minor
    }

    [Header("Setup")]
    [SerializeField] private DistractionSeverity thisSeverity;
    [SerializeField] private int distractionRadius;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*Enemy a = new Enemy();
        a.GetDistracted(transform, thisSeverity);*/
    }

    public void UseDistraction()
    {
        
    }
}
