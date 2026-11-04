using UnityEngine;

public class DestroyTutEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("TutorialEnemy")) 
        { 
            var enemy = other.GetComponentInParent<EnemyMovement>();
            Destroy(enemy.gameObject);
        }
       

      
    }
}
