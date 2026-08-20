using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

    [Header("Speeds")] 
    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float sneakSpeed;
    [SerializeField] private float ghostSpeed;
    [Header("Setup")]
    [SerializeField] private CharacterController charController;
    [SerializeField] private Animator spriteAnimator;
    [SerializeField] private InteractionHandler interactionHandler;
    
    [Header("Ghost")]
    [SerializeField] private GameObject ghostedPlayerPrefab;
    [SerializeField] private float maxLeashDistance;
    [SerializeField] private AnimationCurve leashResistanceCurve;
    [SerializeField] private float ghostReturnTime;
    
    [Header("Steps")]
    [SerializeField] private float stepDistance;
    [SerializeField] private GameObject stepDistractionPrefab;
    [SerializeField] private float minorStepRadius, moderateStepRadius, severeStepRadius;
    [SerializeField] private Color minorStepColor, moderateStepColor, severeStepColor;
    [SerializeField] private bool showStepRadii;
    
    [Header("Runtime")]
    [SerializeField] private PlayerState currentState;
    [SerializeField] private Vector2 currentMoveDir;
    [SerializeField] private bool isRunHeld, isSneakHeld, isAcceptingInputs=true;
    [SerializeField] private bool isGhost;
    [SerializeField] private GameObject ghostedPlayerInstance;
    [SerializeField] private Vector3 lastStepPos;

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
            if (value == PlayerState.Hiding)
            {
                if (currentMoveDir.magnitude != 0) return;
            }
            currentState = value;
            //we can add listeners here for animation triggers etc
            SetAnimationFlag(value);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showStepRadii)
        {
            Gizmos.color = minorStepColor;
            Gizmos.DrawWireSphere(transform.position, minorStepRadius);
            Gizmos.color = moderateStepColor;
            Gizmos.DrawWireSphere(transform.position, moderateStepRadius);
            Gizmos.color = severeStepColor;
            Gizmos.DrawWireSphere(transform.position, severeStepRadius);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        charController = GetComponentInChildren<CharacterController>();
        spriteAnimator = GetComponentInChildren<Animator>();
        interactionHandler = GetComponentInChildren<InteractionHandler>();
        lastStepPos = transform.position;
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
        string runFlag = "IsRunning", moveFlag = "IsWalking", crouchFlag = "IsCrouching", hideFlag="IsHiding";
        spriteAnimator.SetBool(runFlag, newState==PlayerState.Running);
        spriteAnimator.SetBool(crouchFlag, newState==PlayerState.Sneaking);
        spriteAnimator.SetBool(moveFlag, newState==PlayerState.Walking);
        spriteAnimator.SetBool(hideFlag, newState == PlayerState.Hiding);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float moveSpeed = 0f;
        if (isGhost && ghostedPlayerInstance)
        {
            moveSpeed = ghostSpeed;
            Vector3 currentPos = transform.position, shellPos = ghostedPlayerInstance.transform.position;
            
            Vector3 toPlayerShell = shellPos - currentPos;
            float dist = Vector3.Distance(currentPos, shellPos);
            float resistance = leashResistanceCurve.Evaluate(dist / maxLeashDistance);
            
            charController.Move(toPlayerShell.normalized*resistance);
        }
        
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
            //float moveSpeed = 0f;
            if (!isGhost)
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

            if (!isGhost && isAcceptingInputs && Vector3.Distance(lastStepPos, transform.position) > stepDistance)
            {
                //do a step sound
                DoStep();
                lastStepPos = transform.position;
            }
        }
        else
        {
            lastStepPos = transform.position;
        }
    }

    private void DoStep()
    {
        GameObject distraction = Instantiate(stepDistractionPrefab, transform.position, Quaternion.identity);
        DistractionHandler dh = distraction.GetComponentInChildren<DistractionHandler>();
        //we can dynamically assign severity and radius based on move state
        switch (currentState)
        {
            case PlayerState.Running:
                dh.ThisSeverity = DistractionHandler.DistractionSeverity.Severe;
                dh.DistractionRadius = severeStepRadius;
                break;
            case PlayerState.Walking:
                dh.ThisSeverity = DistractionHandler.DistractionSeverity.Moderate;
                dh.DistractionRadius = moderateStepRadius;
                break;
            case PlayerState.Sneaking:
                dh.ThisSeverity = DistractionHandler.DistractionSeverity.Minor;
                dh.DistractionRadius = minorStepRadius;
                break;
        }
        dh.DoInteract();
        dh.DoSelfDistruct(0.3f);
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
