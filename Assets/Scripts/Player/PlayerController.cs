using System;
using System.Collections;
using DefaultNamespace;
using NUnit.Framework;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour, iSaveable
{
	public enum PlayerState
	{
		Walking,
		Running,
		Idle,
		Sneaking,
		Hiding,
		Mantling
	}

	[Header("Speeds")]
	[SerializeField] private float gravityScale;
	[SerializeField] private float baseMoveSpeed;
	[SerializeField] private float runSpeed;
	[SerializeField] private float sneakSpeed;
	[SerializeField] private float ghostSpeed;

	[Header("Setup")]
	[SerializeField] private CharacterController charController;
	[SerializeField] private Animator spriteAnimator;
	[SerializeField] private InteractionHandler interactionHandler;
	[SerializeField] private ToolHandler toolHandler;
	[SerializeField] private OTSCameraController otsCam;
	[SerializeField] private float turnSpeed = 720f;
	[SerializeField] private Transform modelPivot;
	[SerializeField] private GameObject playerMesh;

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

	[Header("Hide Timings")]
	[SerializeField] private float startHideAnimationTime;
	[SerializeField] private float enterHideAnimationTime;
	[SerializeField] private float exitHideAnimationTime;
	private Vector3 exitPos;
	private Vector3 currentHidePos;

	[Header("Runtime")]
	[SerializeField] private PlayerState currentState;
	[SerializeField] private Vector2 currentMoveDir;
	[SerializeField] private bool isRunHeld, isSneakHeld, isAcceptingInputs = true;

	[Header("Crouch")]
	[SerializeField] private float crouchHeight = 1f;
	[SerializeField] private float heightChangeSpeed = 8f;
	[SerializeField] private CapsuleCollider capsule;
	[SerializeField] private CapsuleCollider catchCollider;
	[SerializeField] private float standHeight;
	[SerializeField] private LayerMask standIgnoreLayers;
	private Vector3 standCenter;
	private float capStandHeight;
	private Vector3 capStandCenter;
	private float targetHeight;

	[Header("Mantling")] 
	[SerializeField] private float mantleUpTime;
	[SerializeField] private float mantleForwardTime;
	[SerializeField] private Transform mantleCheckPos;
	[SerializeField] private Transform footPos;
	[SerializeField] private float mantleDownRayLength;
	[SerializeField] private float mantleUpRayLength;
	[SerializeField] private LayerMask mantleIgnoreLayers;

	public event Action onStepTaken;


	private bool IsAcceptingInputs
	{
		get => isAcceptingInputs;
		set
		{
			isAcceptingInputs = value;
			if (value)
			{
				//creating dummy context to update the move action
				var fakeContext = new InputAction.CallbackContext();
				OnMove(fakeContext);
			}
			else
			{
				//clear all inputs
				currentMoveDir = Vector2.zero;
				CurrentState = isSneakHeld? PlayerState.Sneaking: PlayerState.Idle;
				spriteAnimator.SetBool("IsMoving", false);
			}
		}
	}

	[Header("Ghosting")]
	[SerializeField] private bool isGhost;
	[SerializeField] private GameObject ghostedPlayerInstance;

	public event Action<PlayerState> onStateChange;
	public event Action<bool> onGhostChange;
	public event Action onGhostReturned;

	//--------------------- STUFF ARYANNA ADDED IN --------------- I was trying to sort out the audio stuff, but everything is still here, just in comments.
	[Header("Footstep Cadence")]
	[SerializeField] private Vector3 lastStepPos;
	[SerializeField] private float walkStepInterval = 0.45f;
	[SerializeField] private float runStepInterval = 0.28f;
	[SerializeField] private float sneakStepInterval = 0.65f;
	private float stepTimer = 0f;



	public bool IsGhost
	{
		get => isGhost;
		set
		{
			isGhost = value;
			spriteAnimator.SetBool("IsGhost", value);
			SetGhost(value);
			CameraController.Instance?.ToggleVisionPanel(value);
			onGhostChange?.Invoke(value);
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
				isSneakHeld = false;
				//spriteAnimator.gameObject.SetActive(false);
			}

			if (currentState == PlayerState.Hiding && value != PlayerState.Hiding)
			{
				//spriteAnimator.gameObject.SetActive(true);
				DoUnhide();
			}
			currentState = value;
			targetHeight = (value == PlayerState.Sneaking || value == PlayerState.Mantling) ? crouchHeight : standHeight;

			//we can add listeners here for animation triggers etc
			SetAnimationFlag(value);
			onStateChange?.Invoke(value);
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
		toolHandler.UpdateInteractionHandler();
		otsCam = GameObject.FindGameObjectWithTag("OTSCam").GetComponent<OTSCameraController>();
		standHeight = charController.height;
		standCenter = charController.center;
		targetHeight = standHeight;
		if (capsule)
		{
			capStandHeight = capsule.height;
			capStandCenter = capsule.center;
		}
	}
	private void ApplyHeight(float h)
	{
		float shrink = standHeight - h;

		charController.height = h;
		charController.center = new Vector3(
				standCenter.x,
				standCenter.y - shrink / 2f,
				standCenter.z);

		if (capsule)
		{
			float capH = capStandHeight - shrink;
			capsule.height = capH;
			catchCollider.height = capH;
			capsule.center = new Vector3(
					capStandCenter.x,
					capStandCenter.y - shrink / 2f,
					capStandCenter.z);
			catchCollider.center = new Vector3(
				 capStandCenter.x,
				 capStandCenter.y - shrink / 2f,
				 capStandCenter.z);
		}
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
				toolHandler.UpdateInteractionHandler();
				//Destroy(ghostedPlayerInstance);
			}
		}
	}

	private IEnumerator ResetGhostPosInTime(float time)
	{
		IsAcceptingInputs = false;
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

		IsAcceptingInputs = true;
		Destroy(ghostedPlayerInstance);
		onGhostReturned?.Invoke();
	}

	private void SetAnimationFlag(PlayerState newState)
	{
		string runFlag = "IsRunning", moveFlag = "IsWalking", crouchFlag = "IsCrouching", hideFlag = "IsHiding";
		string mantleFlag = "IsMantling";
		spriteAnimator.SetBool(runFlag, newState == PlayerState.Running);
		spriteAnimator.SetBool(crouchFlag, newState == PlayerState.Sneaking);
		spriteAnimator.SetBool(moveFlag, newState == PlayerState.Walking);
		spriteAnimator.SetBool(hideFlag, newState == PlayerState.Hiding);
		spriteAnimator.SetBool(mantleFlag, newState == PlayerState.Mantling);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		playerMesh.transform.position = modelPivot.position;
		if (!charController.isGrounded && IsAcceptingInputs)
		{
			charController.Move(Vector3.down * (Time.deltaTime * gravityScale));
		}
		float moveSpeed = 0f;
		if (isGhost && ghostedPlayerInstance)
		{
			moveSpeed = ghostSpeed;
			Vector3 currentPos = transform.position, shellPos = ghostedPlayerInstance.transform.position;

			Vector3 toPlayerShell = shellPos - currentPos;
			float dist = Vector3.Distance(currentPos, shellPos);
			float resistance = leashResistanceCurve.Evaluate(dist / maxLeashDistance);

			charController.Move(toPlayerShell.normalized * resistance);
		}
		if (!Mathf.Approximately(charController.height, targetHeight))
		{
			float newHeight = Mathf.MoveTowards(charController.height, targetHeight,
																					heightChangeSpeed * Time.deltaTime);
			ApplyHeight(newHeight);
		}

		if (currentMoveDir.magnitude > 0.01f)
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
			move.y = 0;
			if (move.sqrMagnitude > 0.0001f)
			{
				Quaternion look = Quaternion.LookRotation(move);
				modelPivot.rotation = Quaternion.RotateTowards(modelPivot.rotation, look, turnSpeed * Time.deltaTime);
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

			//if (!isGhost && IsAcceptingInputs && Vector3.Distance(lastStepPos, transform.position) > stepDistance)
			//{
			//	//do a step sound
			//	DoStep();
			//	lastStepPos = transform.position;
			//}
			if (!isGhost && IsAcceptingInputs)
			{
				float currentInterval = walkStepInterval;
				if (currentState == PlayerState.Running) currentInterval = runStepInterval;
				else if (currentState == PlayerState.Sneaking) currentInterval = sneakStepInterval;

				stepTimer += Time.fixedDeltaTime;
				if (stepTimer >= currentInterval)
				{
					DoStep();
					stepTimer = 0f;
				}
			}
		}
		else
		{
			//lastStepPos = transform.position;
			stepTimer = walkStepInterval;
		}
	}

	private void DoStep()
	{
		//GameObject distraction = Instantiate(stepDistractionPrefab, transform.position, Quaternion.identity);
		//DistractionHandler dh = distraction.GetComponentInChildren<DistractionHandler>();
		////we can dynamically assign severity and radius based on move state
		//switch (currentState)
		//{
		//	case PlayerState.Running:
		//		dh.ThisSeverity = DistractionHandler.DistractionSeverity.Severe;
		//		dh.DistractionRadius = severeStepRadius;
		//		break;
		//	case PlayerState.Walking:
		//		dh.ThisSeverity = DistractionHandler.DistractionSeverity.Moderate;
		//		dh.DistractionRadius = moderateStepRadius;
		//		break;
		//	case PlayerState.Sneaking:
		//		dh.ThisSeverity = DistractionHandler.DistractionSeverity.Minor;
		//		dh.DistractionRadius = minorStepRadius;
		//		break;
		//}
		//dh.DoInteract();
		//dh.DoSelfDistruct(0.3f);


		// 2. Alert guards using the footstep noise trigger
		/*GameObject distraction = Instantiate(stepDistractionPrefab, transform.position, Quaternion.identity);
		FootstepNoise fn = distraction.GetComponentInChildren<FootstepNoise>();

		if (fn != null)
		{
			DistractionHandler.DistractionSeverity severity = DistractionHandler.DistractionSeverity.Minor;
			float radius = minorStepRadius;

			switch (currentState)
			{
				case PlayerState.Running:
					severity = DistractionHandler.DistractionSeverity.Severe;
					radius = severeStepRadius;
					break;
				case PlayerState.Walking:
					severity = DistractionHandler.DistractionSeverity.Moderate;
					radius = moderateStepRadius;
					break;
				case PlayerState.Sneaking:
					severity = DistractionHandler.DistractionSeverity.Minor;
					radius = minorStepRadius;
					break;
			}

			fn.TriggerFootstepAlert(radius, severity, true);
		}

		Destroy(distraction, 0.3f);*/
		onStepTaken?.Invoke();

		DistractionHandler.DistractionSeverity severity = DistractionHandler.DistractionSeverity.Minor;
		float radius = minorStepRadius;

		switch (currentState)
		{
			case PlayerState.Running:
				severity = DistractionHandler.DistractionSeverity.Severe;
				radius = severeStepRadius;
				break;
			case PlayerState.Walking:
				severity = DistractionHandler.DistractionSeverity.Moderate;
				radius = moderateStepRadius;
				break;
			case PlayerState.Sneaking:
				severity = DistractionHandler.DistractionSeverity.Minor;
				radius = minorStepRadius;
				break;
		}

		DistractionHandler.DoDistract(transform.position, radius, severity, true);
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		if (!IsAcceptingInputs) return;
		//if (!context.performed) return;
		if (CurrentState == PlayerState.Hiding)
		{
			return;
		}

		//Catching empty inputs from the dummy call when IsAcceptingInputs gets enabled
		if (context.action == null)
		{
			currentMoveDir = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
		}
		else
		{
			currentMoveDir = context.action.ReadValue<Vector2>();
		}
		if (currentMoveDir.magnitude != 0)
		{
			//spriteAnimator.SetFloat("FacingX", currentMoveDir.x);
			//spriteAnimator.SetFloat("FacingY", currentMoveDir.y);
			//running takes priority
			if (isRunHeld)
			{
				CurrentState = PlayerState.Running;
			}
			else if (isSneakHeld)
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
			//spriteAnimator.SetFloat("FacingX", currentMoveDir.x);
			//spriteAnimator.SetFloat("FacingY", currentMoveDir.y);
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
		if (!IsAcceptingInputs) return;
		//isSneakHeld = context.performed;
		if (!context.performed) return;
		if (CurrentState == PlayerState.Hiding) return;
		
		float height = standHeight * transform.lossyScale.y;
		//Debug.Log(transform.lossyScale.y);
		if (Physics.Raycast(footPos.position, Vector3.up, height, ~standIgnoreLayers)) return;
		
		isSneakHeld = !isSneakHeld;
		if (isSneakHeld)
		{
			if (!isRunHeld) CurrentState = PlayerState.Sneaking;
		}
		else
		{
			CurrentState = currentMoveDir.magnitude > 0 ? PlayerState.Walking : PlayerState.Idle;
		}
	}

	public void OnSprint(InputAction.CallbackContext context)
	{
		if (!IsAcceptingInputs) return;
		if (CurrentState == PlayerState.Sneaking)
		{
			float height = standHeight * transform.lossyScale.y;
			//Debug.Log(transform.lossyScale.y);
			if (Physics.Raycast(footPos.position, Vector3.up, height, ~standIgnoreLayers)) return;
		}
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

	public void OnScroll(InputAction.CallbackContext context)
	{
		if (!IsAcceptingInputs) return;
		if (!context.performed) return;
		interactionHandler?.HandleScroll(context);
		//otsCam.HandleMouseScroll(context);
		//if (IsGhost) return;
		/*if (context.ReadValue<float>() > 0)
		{
			toolHandler.ScrollUp();
		}
		else
		{
			toolHandler.ScrollDown();
		}*/
	}

	public void OnInteract(InputAction.CallbackContext context)
	{
		if (!IsAcceptingInputs) return;
		if (!context.performed) return;
		//interactionHandler.DoInteract();
		if (!IsGhost)
		{
			toolHandler.UseSelected();
		}
		else
		{
			interactionHandler.DoInteract(ConditionInteraction.Distract);
			IsGhost = false;
		}
	}

	public void OnMantle(InputAction.CallbackContext context)
	{
		if (!IsAcceptingInputs) return;
		if (!context.performed) return;
		//we can do two raycasts: one down to see if there's a surface, and one up to see if it's blocked
#if UNITY_EDITOR
		Debug.DrawLine(mantleCheckPos.position,mantleCheckPos.position+ Vector3.down*mantleDownRayLength,Color.magenta,2f);
#endif
		
		
		//raycast down
		if (Physics.Raycast(mantleCheckPos.position, Vector3.down, out var hit, mantleDownRayLength,
			    ~mantleIgnoreLayers))
		{
			//raycast up first as return condition
			float height = crouchHeight * transform.lossyScale.y;
			//Debug.Log(transform.lossyScale.y);
#if UNITY_EDITOR
			Debug.DrawLine(hit.point,hit.point+ Vector3.up*height,Color.blue,2f);
#endif
			if (Physics.Raycast(hit.point, Vector3.up, height, ~mantleIgnoreLayers)) return;
			
			//Debug.Log($"Can mantle at {hit.point}");
			IsAcceptingInputs = false;
			StartCoroutine(DoMantle(hit.point));
		}
	}

	private IEnumerator DoMantle(Vector3 targetPoint)
	{
		CurrentState = PlayerState.Mantling;
		Vector3 displacement = targetPoint - footPos.position;
		//Debug.Log($"Displacement: {displacement}");
		Vector3 actualTarget = transform.position + displacement;
		Vector3 initialPos = transform.position;
		Vector3 verticalTarget = new Vector3(transform.position.x, actualTarget.y, transform.position.z);
		float currentTime = 0;
		do
		{
			transform.position = Vector3.Lerp(initialPos, verticalTarget, currentTime / mantleUpTime);
			Physics.SyncTransforms();
			yield return new WaitForEndOfFrame();
			currentTime += Time.deltaTime;
		} while (currentTime < mantleUpTime);

		initialPos = transform.position;
		currentTime = 0;
		do
		{
			transform.position = Vector3.Lerp(initialPos, actualTarget, currentTime / mantleForwardTime);
			Physics.SyncTransforms();
			yield return new WaitForEndOfFrame();
			currentTime += Time.deltaTime;
		} while (currentTime < mantleForwardTime);

		isSneakHeld = true;
		CurrentState = PlayerState.Sneaking;
		IsAcceptingInputs = true;
	}

	public void OnGhost(InputAction.CallbackContext context)
	{
		if (!IsAcceptingInputs) return;
		if (!context.performed) return;
		IsGhost = !IsGhost;
		if (IsGhost)
		{
			interactionHandler.CurrentInteractionType = ConditionInteraction.Distract;
		}
		else
		{
			toolHandler.UpdateInteractionHandler();
		}
	}

	public void DoHide(Vector3 enterPos, Vector3 inExitPos, Vector3 hidePos)
	{
		IsAcceptingInputs = false;
		isSneakHeld = false;
		StartCoroutine(HideRoutine(enterPos, hidePos));
		exitPos = inExitPos;
		currentHidePos = hidePos;
	}

	private IEnumerator HideRoutine(Vector3 enterPos, Vector3 hidePos)
	{
		CurrentState = PlayerState.Hiding;
		Vector3 startPos = enterPos, endPos = hidePos;
		Vector3 initialPos = transform.position;
		float currentTime = 0;

		do
		{
			//beginning of animation
			transform.position = Vector3.Lerp(initialPos, startPos, currentTime / startHideAnimationTime);
			Physics.SyncTransforms();
			yield return new WaitForEndOfFrame();
			currentTime += Time.deltaTime;
		} while (currentTime < startHideAnimationTime);

		currentTime = 0;
		do
		{
			//enter animation
			//lerp the position
			transform.position = Vector3.Lerp(startPos, endPos, currentTime / enterHideAnimationTime);
			Physics.SyncTransforms();
			yield return new WaitForEndOfFrame();
			currentTime += Time.deltaTime;
		} while (currentTime < enterHideAnimationTime);

		IsAcceptingInputs = true;
	}

	private IEnumerator ExitHideRoutine()
	{
		float currentTime = 0;
		do
		{
			transform.position = Vector3.Lerp(currentHidePos, exitPos, currentTime / enterHideAnimationTime);
			Physics.SyncTransforms();
			yield return new WaitForEndOfFrame();
			currentTime += Time.deltaTime;
		} while (currentTime < exitHideAnimationTime);

		transform.position = exitPos;
		IsAcceptingInputs = true;
	}

	public void DoUnhide()
	{
		if (!IsAcceptingInputs) return;
		IsAcceptingInputs = false;
		StartCoroutine(ExitHideRoutine());
	}
	
	#region Saving

	[Header("Saving")] 
	[SerializeField] private Vector3 savedPos;
	[SerializeField] private Quaternion savedRot;
    
	private void OnEnable()
	{
		((iSaveable)this).OnEnable();
	}

	private void OnDisable()
	{
		((iSaveable)this).OnDisable();
	}

	public void DoSave()
	{
		savedPos = transform.position;
		savedRot = modelPivot.rotation;
	}
    
	public void DoLoad()
	{
		transform.position = savedPos;
		modelPivot.rotation = savedRot;
		Physics.SyncTransforms();
	}
    
	#endregion
}
