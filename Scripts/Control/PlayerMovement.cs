using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{

	[Header("Components")]
	[SerializeField] private WeaponAnimator weaponanimator = default;
	[SerializeField] private MouseLook mouseLook = default;
	[SerializeField] private Player player = default;
	[SerializeField] private PlayerUnit playerUnit = default;
	[SerializeField] private Transform groundCheck = default;
	[SerializeField] private AudioClip landingboom = default;
	[SerializeField] private AudioClip softland = default;
	[SerializeField] private AudioClip dashsound = default;
	[SerializeField] private AudioSource feetASO = default;
	[SerializeField] private Image dashborder = default;
	[SerializeField] private GameObject teleportPainterPrefab = default;
	[SerializeField] private GameObject teleportParticles = default;

	[Header("Movement")]
	[SerializeField] private float walkSpeed = 45;
	[SerializeField] private float moveSmoothTime = 0.03f;
	[SerializeField] private float groundDistance = 2;
	[SerializeField] private float airTimerThreshold = 0.4f;

	[Header("Jumping")]
	[SerializeField] private float jumpHeight = 10;
	[SerializeField] private float jumpextensionfactor = 1;
	[SerializeField] private float jumpboostfactor = 5;
	[SerializeField] private float jumpmomentum = 5;
	[SerializeField] private float walkresetrate = 5;
	[SerializeField] private float longJumpCDDuration = 2;
	[SerializeField] private float singlejumpCDduration = 0.35f;
	[SerializeField] private float smoothFallFactor = -12;
	[SerializeField] private float maxmomentum = 85;
	[SerializeField] private float gravityFactor = 5;

	[Header("Dashing")]
	[SerializeField] private float dashduration = 0.3f;
	[SerializeField] private float dashcooldown = 3;
	[SerializeField] private float dashmultiplier = 3;

	[Header("Teleporting")]
	[SerializeField] private float teleportThreshold = 0.3f;
	[SerializeField] private float telePainterGrowRate = 5.3f;
	[SerializeField] private float teleportCooldownDuration = 45.0f;

	[Header("Sound")]
	[SerializeField] private float boomdistancethreshold = 150.0f;
	[SerializeField] private float boomTimerThreshold = 2.5f;
	[SerializeField] private float soundStepSpeed = 0.33f;

	private GameObject teleportPainter;
	private CharacterController cc;
	private SoundLibrary sl;
	private AudioClip walk;
	private AudioClip wingflap;
	private GameOptions gops;
	private Vector2 moveInputValues;
	private Vector2 cachedInputAxes;
	private Vector2 cachedVelocity = Vector2.zero;
	private Vector3 cachedPainterPosition = Vector3.zero;
	private Vector3 velocity = Vector3.zero;
	private Vector3 lastPosition;
	private LayerMask groundMask;
	private bool isWalkPlaying = false;
	private bool isWingFlapPlaying = false;
	private bool isGrounded;
	private bool launched;
	private bool movementDisabled = false;
	private bool boomcheckreached;
	private bool boomTimeron = false;
	private bool boomdistancereached;
	private bool boomTimerThresholdReached;
	private bool wingsEnabled = false;
	private bool hasSpareJump = true;
	private bool leftfootsound = false;
	private bool dashing;
	private bool jumpPressed;
	private bool isMoving;
	private bool isTeleporting = false;
	private bool teleportQueued = false;
	private bool softLandPlayed = true;
	private bool teleportReady = false;
	private bool teleportEnabled = false;
	private float originalWalkSpeed;
	private float boomTimer = 0.0f;
	private float deltaPositionY;
	private float boomdistance = 0;
	private float predashwspeed;
	private float dashtimer = 0;
	private float dashCDtimer = 0;
	private float longJumpCDTimer = 0;
	private float singlejumpCDTimer = 0;
	private float teleportChargeTime = 0;
	private float teleportCooldownTimer = 0;
	private float cdebugVelocityFactor = 1;
	private float airTimer = 0;
	private bool teleportDone = false;

	public bool GetTeleportReady() { return teleportReady; }
	public bool GetWingsEnabled() { return wingsEnabled; }
	public float GetWalkSpeed() { return walkSpeed; }
	public float GetCdebugVelocityFactor() { return cdebugVelocityFactor; }
	public float GetJumpExtensionFactor() { return jumpextensionfactor; }
	public bool GetIsGrounded() { return isGrounded; }
	public float GetGravityFactor() { return gravityFactor; }
	public float GetDashCooldown() { return dashcooldown; }
	public float GetDashCDTimer() { return dashCDtimer; }
	public bool GetIsMoving() { return isMoving; }
	public float GetTeleCooldown() { return teleportCooldownDuration; }
	public float GetTeleCDTimer() { return teleportCooldownTimer; }
	public bool GetTeleportDone() { return teleportDone; }
	internal bool GetTeleportEnabled() { return teleportEnabled; }
	public void SetWingsEnabled(bool value) { wingsEnabled = value; }
	public void SetCdebugVelocityFactor(float value) { cdebugVelocityFactor = value; }
	public void SetTeleportEnabled(bool value) { teleportEnabled = value; }
	public void SetWalkSpeed(float value) { walkSpeed = value; }
	public void SetJumpExtensionFactor(float value) { jumpextensionfactor = value; }
	public void SetMDisabled(bool value) { movementDisabled = value; }
	public void SetVelocity(Vector3 value) { velocity = value; }
	internal void SetLaunched(bool value) { launched = value; }
	public void DecreaseYVelocity(float value) { velocity.y -= value; }
	public void IncreaseWalkSpeed(float value) { walkSpeed += value; }
	public void DecreaseWalkSpeed(float value) { if (walkSpeed - value > 0) walkSpeed -= value; else walkSpeed = 0; }

	void Start()
	{
		InitialiseVariables();
	}

	private void InitialiseVariables()
    {
		gops = GameOptions.GetGOPS();
		HandleCheats();
        sl = GameManagerScript.GetGMScript().GetSoundLibrary();
        walk = sl.GetPlayerStepLeft();
        wingflap = sl.GetWingFlap();
        gravityFactor *= -9.81f;
        cc = GetComponent<CharacterController>();
        originalWalkSpeed = walkSpeed;
        lastPosition = transform.position;
        groundMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("HighGround");

    }

    private void HandleCheats()
    {
        if (gops.GetCustomWalkSpeedEnabled())
            walkSpeed = gops.GetCustomWalkSpeed();
        if (gops.GetCustomTPEnabled())
            teleportCooldownDuration = gops.GetCustomTPCooldown();
    }

    void FixedUpdate()
	{
		AnimateDashUI();
		CheckGrounding();
		SmoothFallingIfGrounded();
		ResetWalkingSpeed();
		UpdateJumpCDTimers();
		if (!playerUnit.IsDead() && cc)
		{
			SoundWalking();
			CheckAndSetDashing();
			UpdateDash();
			UpdateDashCooldown();
		}
		HandleBoomThreshold();
		deltaPositionY = transform.position.y - lastPosition.y;
		lastPosition = transform.position;
		isMoving = Mathf.Abs(cachedInputAxes.x) > 0.1f || Mathf.Abs(cachedInputAxes.y) > 0.1f;
	}

	private void Update()
	{
		if (!playerUnit.IsDead() && cc)
		{
			MoveByAxis();
			ControlJumping();
			ControlTeleporting();
		}
	}

	public void SetTeleportingControl(InputAction.CallbackContext value)
	{
		isTeleporting = value.ReadValueAsButton();
	}

	private void ControlTeleporting()
	{
		teleportReady = isTeleporting && teleportCooldownTimer <= 0 && teleportEnabled;
		if (teleportReady)
		{
			UpdateTeleportTimings();
		}
		else if (teleportQueued)
			DoTeleport();
		else
			teleportCooldownTimer -= Time.deltaTime;
	}

	private void UpdateTeleportTimings()
	{
		teleportChargeTime += Time.deltaTime;
		if (teleportChargeTime >= teleportThreshold)
		{
			AimTeleport();
			teleportQueued = true;
		}
	}

	private void AimTeleport()
	{
		if (!teleportPainter)
		{
			teleportPainter = Instantiate(teleportPainterPrefab, transform.position, transform.rotation, transform);
			teleportPainter.transform.forward = transform.forward;
		}
		else
		{
			teleportPainter.transform.position += teleportPainter.transform.forward * telePainterGrowRate * Time.deltaTime;
			cachedPainterPosition = teleportPainter.transform.position;
		}
	}

	private void DoTeleport()
	{
		if(teleportPainter)
		{
			cc.Move(cachedPainterPosition - transform.position);
			var particles = Instantiate(teleportParticles, transform.position, transform.rotation, transform);
			Destroy(teleportPainter);
			feetASO.PlayOneShot(sl.GetTeleported());
			teleportChargeTime = 0;
			teleportQueued = false;
			teleportCooldownTimer = gops.GetIsTutorialMode()? 5.0f : teleportCooldownDuration;
			cachedPainterPosition = Vector3.zero;
			teleportDone = true;
		}
	}

	private void AnimateDashUI()
	{
		if (dashtimer > 0)
		{
			Color color = dashborder.color;
			color.a = 1;
			color.a *= dashtimer / dashduration;
			dashborder.color = color;
		}
		else if (dashtimer <= 0 && dashborder.color.a > 0)
		{
			Color color = dashborder.color;
			color.a = 0;
			dashborder.color = color;
		}
	}

	private void CheckGrounding()
	{
		var tempIsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
		if (!tempIsGrounded)
			airTimer += Time.deltaTime;
		else
			airTimer = 0;
		isGrounded = airTimer < airTimerThreshold;
		if (isGrounded)
		{
			launched = false;
		}
	}

	private void SmoothFallingIfGrounded()
	{
		if (isGrounded && velocity.y < 0)
		{
			ResetJump();
			//if boomcheckreached flag on then when next grounded play sound
			if (boomcheckreached)
			{
				feetASO.PlayOneShot(landingboom);
				boomcheckreached = false;
			}
			else if (!softLandPlayed)
			{
				feetASO.PlayOneShot(softland);
				softLandPlayed = true;
			}
		}
	}

	private void ResetWalkingSpeed()
	{
		if (walkSpeed >= originalWalkSpeed && isGrounded)
			walkSpeed -= walkresetrate * Time.deltaTime;
		else if (walkSpeed < originalWalkSpeed)
			walkSpeed = originalWalkSpeed;
	}

	private void UpdateJumpCDTimers()
	{
		if (longJumpCDTimer >= 0)
			longJumpCDTimer -= Time.deltaTime;
		else if (!hasSpareJump)
			hasSpareJump = true;

		if (singlejumpCDTimer >= 0)
			singlejumpCDTimer -= Time.deltaTime;
	}

	public void OnMovement(InputAction.CallbackContext value)
	{
		moveInputValues = value.ReadValue<Vector2>();
		bool anyvalue = Mathf.Abs(moveInputValues.x) > 0 || Mathf.Abs(moveInputValues.y) > 0;
		weaponanimator.SetMovingPressed(anyvalue);
		player.SetLeftStickValues(moveInputValues);
	}

	public void OnJumpPressed(InputAction.CallbackContext value)
	{
		jumpPressed = value.ReadValueAsButton();
		if (isGrounded && !movementDisabled && singlejumpCDTimer <= 0)
		{
			if (longJumpCDTimer <= 0)
			{
				DoSingleJump();
				longJumpCDTimer = longJumpCDDuration;
			}
			else if (hasSpareJump)
			{
				DoSingleJump();
				hasSpareJump = false;
			}
		}
	}

	private void MoveByAxis()
	{
		if (!movementDisabled)
		{
			cachedInputAxes = Vector2.SmoothDamp(cachedInputAxes, moveInputValues, ref cachedVelocity, moveSmoothTime);
			Vector3 movement = transform.right * cachedInputAxes.x + transform.forward * cachedInputAxes.y;
			cc.Move(movement * walkSpeed * Time.deltaTime); 
		}
	}

	private void ControlJumping()
	{
		if (!gops.GetCameraDebugMode())
			velocity.y += gravityFactor * Time.deltaTime;
		ExtendJumping();
		cc.Move(velocity * Time.deltaTime);
	}

	private void DoSingleJump()
	{
		softLandPlayed = false;
		feetASO.PlayOneShot(sl.GetPlayerJump());
		if (walkSpeed < maxmomentum)
			walkSpeed += jumpboostfactor;
		velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityFactor);
		singlejumpCDTimer = singlejumpCDduration;
	}


	private void SoundWalking()
	{
		if (Mathf.Abs(cachedInputAxes.x) > 0.1f && isGrounded && !isWalkPlaying)
			StartCoroutine(playWalk());
		else if (Mathf.Abs(cachedInputAxes.y) > 0.1f && isGrounded && !isWalkPlaying)
			StartCoroutine(playWalk());
	}

	private void CheckAndSetDashing()
	{
		if (dashtimer > 0)
		{
			walkSpeed = predashwspeed * dashmultiplier;
			dashing = true;
		}
	}

	private void UpdateDash()
	{
		if (dashtimer > 0)
			dashtimer -= Time.deltaTime;
		else if (dashing)
			FinishDash();
	}

	private void FinishDash()
	{
		dashing = false;
		walkSpeed = predashwspeed;
	}

	private void UpdateDashCooldown()
	{
		if (dashCDtimer > 0)
			dashCDtimer -= Time.deltaTime;
	}

	private void HandleBoomThreshold()
	{
		//DebugBoomVars();
		SetBoomTimer();
		DoBoomBasicChecks();
		DoBoomFinalChecks();
	}

	private void DebugBoomVars()
	{
		Debug.Log("boomTimeron = " + boomTimeron);
		Debug.Log("boomTimer = " + boomTimer);
		Debug.Log("boomdistance = " + boomdistance);
		Debug.Log("boomdistancereached = " + boomdistancereached);
		Debug.Log("boomTimerThreshold = " + boomTimerThreshold);
		Debug.Log("boomTimerThresholdReached = " + boomTimerThresholdReached);
		Debug.Log("boomthreshold = " + boomdistancethreshold);
		Debug.Log("boomcheckreached = " + boomcheckreached);
	}

	internal void AttemptDash()
	{
		if(dashtimer <= 0 && dashCDtimer <= 0)
		{
			DoDash();
		}
	}

	private void DoDash()
	{
		InitialiseDashVars();
		feetASO.PlayOneShot(dashsound);
	}

	public void StartBackdash()
	{
		if (!gops.GetCameraDebugMode())
		{
			if (player.gameObject.activeInHierarchy)
				if (dashtimer <= 0 && dashCDtimer <= 0)
					StartCoroutine(DoBackDash());
		}
		else
		{
			velocity.y = 0;
		}
	}

	private void StartTurnlessBackdash()
	{
		StartCoroutine(DoTurnlessBackDash());
	}

	public IEnumerator DoBackDash()
	{
		float bdashtimer = 0;
		Vector3 cachedForward = transform.forward;
		DoDash();
		mouseLook.DoQuickTurn();
		while (bdashtimer <= dashduration)
		{
			movementDisabled = true;
			Vector3 movement = transform.right * cachedInputAxes.x - cachedForward * 2;
			cc.Move(movement * walkSpeed * Time.deltaTime);
			bdashtimer += Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		movementDisabled = false;
	}

	public IEnumerator DoTurnlessBackDash()
	{
		float bdashtimer = 0;
		Vector3 cachedForward = transform.forward;
		DoDash();
		while (bdashtimer <= dashduration)
		{
			movementDisabled = true;
			Vector3 movement = transform.right * cachedInputAxes.x - cachedForward * 2;
			cc.Move(movement * walkSpeed * Time.deltaTime);
			bdashtimer += Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		movementDisabled = false;
	}

	private void InitialiseDashVars()
	{
		velocity = Vector3.zero;
		predashwspeed = walkSpeed;
		dashtimer = dashduration;
		dashCDtimer = dashcooldown;
		Color color = dashborder.color;
		color.a = 1;
		dashborder.color = color;
	}

	private void DoBoomFinalChecks()
	{
		if (!isGrounded)
		{
			if (boomdistancereached && boomTimerThresholdReached)
			{
				FlagBoomAndReset();
			}
		}
	}

	private void DoBoomBasicChecks()
	{
		if (boomTimeron)
		{
			boomTimer += Time.deltaTime;
			boomdistance = boomdistance + Mathf.Abs(deltaPositionY);
			if (boomdistance > boomdistancethreshold)
				boomdistancereached = true;
			if (boomTimer > boomTimerThreshold)
				boomTimerThresholdReached = true;

		}
	}

	private void FlagBoomAndReset()
	{
		boomcheckreached = true;
	}

	private void SetBoomTimer()
	{
		if (!isGrounded)
			boomTimeron = true;
		else
			boomTimeron = false;
	}

	private void ResetJump()
	{
		velocity.y = smoothFallFactor;
		velocity.x = 0;
		velocity.z = 0;
		ResetBoomVars();
	}

	private void ResetBoomVars()
	{
		boomTimer = 0;
		boomdistance = 0;
		boomdistancereached = false;
		boomTimerThresholdReached = false;
	}

	private void ExtendJumping()
	{
		if (gops.GetCameraDebugMode() && jumpPressed)
			velocity.y += cdebugVelocityFactor;
		else if (jumpPressed && !isGrounded && !movementDisabled)
		{
			velocity.y += jumpextensionfactor * Time.deltaTime;
			if (!wingsEnabled)
				walkSpeed += jumpmomentum * Time.deltaTime;
			if (!isWingFlapPlaying && wingsEnabled)
				StartCoroutine(playWingFlap());
		}

	}

	IEnumerator playWalk()
	{
		DecideWalkClip();
		feetASO.PlayOneShot(walk);
		yield return new WaitForSeconds(walk.length*soundStepSpeed);
		isWalkPlaying = false;
	}

	IEnumerator playWingFlap()
	{
		isWingFlapPlaying = true;
		feetASO.PlayOneShot(wingflap);
		yield return new WaitForSeconds(wingflap.length);
		isWingFlapPlaying = false;
	}

	private void DecideWalkClip()
	{
		isWalkPlaying = true;
		AudioClip selectedclip;
		if (leftfootsound)
			selectedclip = sl.GetPlayerStepLeft();
		else
			selectedclip = sl.GetPlayerStepRight();
		leftfootsound = !leftfootsound;
		AudioClip clip = selectedclip;
		walk = clip;
	}
}
