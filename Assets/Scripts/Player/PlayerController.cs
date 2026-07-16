using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState 
    {
        Walking,
        Running,
        Idle,
        Sneaking,
        Hiding
    }

    [Header("Setup")] 
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float sneakSpeed;
    [SerializeField] private CharacterController charController;
    
    [Header("Runtime")]
    [SerializeField] private PlayerState currentState;
    [SerializeField] private Vector2 currentMoveDir;
    [SerializeField] private bool isRunHeld, isSneakHeld;

    public PlayerState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            //we can add listeners here for animation triggers etc
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        charController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentMoveDir.magnitude != 0)
        {
            //moving only in the xz-plane
            Vector3 move = new Vector3(currentMoveDir.x, 0, currentMoveDir.y);
            //for now, just modifying move speed based on current state, can iterate more later
            float moveSpeed = 0f;
            switch (currentState)
            {
                case PlayerState.Walking:
                    moveSpeed = baseMoveSpeed;
                    break;
                case PlayerState.Running:
                    moveSpeed = runSpeed;
                    break;
                case PlayerState.Sneaking:
                    moveSpeed = sneakSpeed;
                    break;
            }
            charController.Move(move * (moveSpeed * Time.deltaTime));
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        //if (!context.performed) return;
        currentMoveDir = context.action.ReadValue<Vector2>();
        if (currentMoveDir.magnitude != 0)
        {
            //running takes priority
            if (isRunHeld)
            {
                CurrentState = PlayerState.Running;
            } else if (isSneakHeld)
            {
                CurrentState = PlayerState.Sneaking;
            }
            else
            {
                CurrentState = PlayerState.Walking;
            }
            //CurrentState = isRunHeld ? PlayerState.Running : PlayerState.Walking;
        }
        else
        {
            //we've stopped moving
            if (currentState != PlayerState.Sneaking)
            {
                CurrentState = PlayerState.Idle;
            }
        }
    }

    public void OnSneak(InputAction.CallbackContext context)
    {
        //isSneakHeld = context.performed;
        if (!context.performed) return;
        isSneakHeld = !isSneakHeld;
        if (isSneakHeld)
        {
            if (!isRunHeld) CurrentState = PlayerState.Sneaking;
        }
        else
        {
            CurrentState = currentMoveDir.magnitude>0 ? PlayerState.Walking : PlayerState.Idle;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isRunHeld = context.performed;
        //if (isRunHeld) CurrentState = PlayerState.Running;
        if (isRunHeld)
        {
            isSneakHeld = false;
            if (currentMoveDir.magnitude > 0) CurrentState = PlayerState.Running;
        }
        else
        {
            if (currentMoveDir.magnitude > 0) CurrentState = isSneakHeld ? PlayerState.Sneaking : PlayerState.Walking;
        }
    }
}
