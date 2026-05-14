using System;
using UnityEngine;

[Serializable]
public enum EnemyMoveMode 
{
    Patrolling,
    Chasing,
    Idle
   
}

[Serializable]
public enum EnemyState
{
    None,
    Stunned,
    Recovery
}

public class Enemy : MonoBehaviour
{
    public EnemyMoveMode currentMoveMode;
    public EnemyState currentState;
  
    void Start()
    {
        
    }

    void Update()
    {
    }


}

