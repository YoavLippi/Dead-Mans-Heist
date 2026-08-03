using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
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
    [SerializeField] private float ghostSpeed;
    [SerializeField] private CharacterController charController;
    [SerializeField] private Animator spriteAnimator;
    [SerializeField] private InteractionHandler interactionHandler;
    [SerializeField] private GameObject ghostedPlayerPrefab;
    [SerializeField] private float leashDistance;
    [SerializeField] private AnimationCurve leashResistanceCurve;
    [SerializeField] private float ghostReturnTime;
    
    [Header("Runtime")]
    [SerializeField] private PlayerState currentState;
    [SerializeField] private Vector2 currentMoveDir;
    [SerializeField] private bool isRunHeld, isSneakHeld, isAcceptingInputs=true;
    [SerializeField] private bool isGhost;
    [SerializeField] private GameObject ghostedPlayerInstance;

    public bool IsGhost
    {
        get => isGhost;
        set
        {
            isGhost = value;
            spriteAnimator.SetBool("IsGhost", value);
            SetGhost(value);
        }
    }

    public PlayerState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            //we can add listeners here for animation triggers etc
            SetAnimationFlag(value);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        charController = GetComponentInChildren<CharacterController>();
        spriteAnimator = GetComponentInChildren<Animator>();
        interactionHandler = GetComponentInChildren<InteractionHandler>();
    }

    private void SetGhost(bool setGhost)
    {
        if (setGhost)
        {
            ghostedPlayerInstance = Instantiate(ghostedPlayerPrefab, transform.position, Quaternion.identity);
            ghostedPlayerInstance.GetComponent<LeashRenderer>().endPoint = gameObject;
        }
        else
        {
            if (ghostedPlayerInstance)
            {
                StartCoroutine(ResetGhostPosInTime(ghostReturnTime));
                //Destroy(ghostedPlayerInstance);
            }
        }
    }

    private IEnumerator ResetGhostPosInTime(float time)
    {
        isAcceptingInputs = false;
        currentMoveDir = Vector2.zero;
        float currentTime = 0f;
        Vector3 startPos = transform.position;
        do
        {
            //charController.transform.position
            charController.transform.position =
                Vector3.Lerp(startPos, ghostedPlayerInstance.transform.position, currentTime / time);
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;
        } while (currentTime < time);
        
        isAcceptingInputs = true;
        Destroy(ghostedPlayerInstance);
    }

    private void SetAnimationFlag(PlayerState newState)
    {
        string runFlag = "IsRunning", moveFlag = "IsWalking", crouchFlag = "IsCrouching";
        spriteAnimator.SetBool(runFlag, newState==PlayerState.Running);
        spriteAnimator.SetBool(crouchFlag, newState==PlayerState.Sneaking);
        spriteAnimator.SetBool(moveFlag, newState==PlayerState.Walking);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentMoveDir.magnitude != 0)
        {
            Vector3 move;
            //Adjusting move to work based on direction of camera
            if (CameraController.Instance.ActiveCam is CinemachineCamera cam)
            {
                Vector3 camForward = cam.transform.forward;
                Vector3 camRight = cam.transform.right;

                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                move = (currentMoveDir.x * camRight) + (currentMoveDir.y * camForward);
            }
            else
            {
                move = new Vector3(currentMoveDir.x, 0, currentMoveDir.y);
            }
            
            //moving only in the xz-plane
            //Vector3 move = new Vector3(currentMoveDir.x, 0, currentMoveDir.y);
            
            //for now, just modifying move speed based on current state, can iterate more later
            float moveSpeed = 0f;
            if (isGhost && ghostedPlayerInstance)
            {
                moveSpeed = ghostSpeed;
                float dist = Vector3.Distance(transform.position, ghostedPlayerInstance.transform.position);
                //moveSpeed = ghostSpeed;
                //moveSpeed = Mathf.Lerp(0f, ghostSpeed, (leashDistance - dist)/leashDistance);
                if (Vector3.Dot(move.normalized,
                        (ghostedPlayerInstance.transform.position - transform.position).normalized) < 0)
                {
                    moveSpeed -= leashResistanceCurve.Evaluate(dist / leashDistance) * ghostSpeed;
                }
            }
            else
            {
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
            }
            
            charController.Move(move * (moveSpeed * Time.deltaTime));
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!isAcceptingInputs) return;
        //if (!context.performed) return;
        currentMoveDir = context.action.ReadValue<Vector2>();
        spriteAnimator.SetFloat("MoveX", currentMoveDir.x);
        spriteAnimator.SetFloat("MoveY", currentMoveDir.y);
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
            spriteAnimator.SetBool("IsMoving", true);
            
            //This should store only the last movement so we know which way it was facing when it stopped
            spriteAnimator.SetFloat("FacingX", currentMoveDir.x);
            spriteAnimator.SetFloat("FacingY", currentMoveDir.y);
        }
        else
        {
            //we've stopped moving
            if (currentState != PlayerState.Sneaking)
            {
                CurrentState = PlayerState.Idle;
            }
            spriteAnimator.SetBool("IsMoving", false);
        }
    }

    public void OnSneak(InputAction.CallbackContext context)
    {
        if (!isAcceptingInputs) return;
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
        if (!isAcceptingInputs) return;
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

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!isAcceptingInputs) return;
        if (!context.performed) return;
        interactionHandler.DoInteract();
    }

    public void OnGhost(InputAction.CallbackContext context)
    {
        if (!isAcceptingInputs) return;
        if (!context.performed) return;
        IsGhost = !IsGhost;
    }
}
