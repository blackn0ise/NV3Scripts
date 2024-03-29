﻿using EZCameraShake;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    #region exposed parameters

    [Header("Components")]
	[SerializeField] private UIController uic = default;
	[SerializeField] private WeaponAnimator weaponanimator = default;
	[SerializeField] private MouseLook mouseLook = default;
	[SerializeField] private Player player = default;
	[SerializeField] private PlayerUnit playerUnit = default;
	[SerializeField] private Transform groundCheck = default;
	[SerializeField] private AudioClip landingboom = default;
	[SerializeField] private AudioClip softland = default;
	[SerializeField] private AudioClip dashsound = default;
	[SerializeField] private AudioClip assimilatorFireClip = default;
	[SerializeField] private AudioClip viperFireClip = default;
	[SerializeField] private AudioClip launchClip = default;
	[SerializeField] private AudioSource feetASO = default;
	[SerializeField] private Image dashborder = default;
	[SerializeField] private GameObject teleportPainterPrefab = default;
	[SerializeField] private GameObject teleportParticles = default;
	[SerializeField] private GameObject assimilatorProjPrefab = default;
	[SerializeField] private GameObject viperProjPrefab = default;
	[SerializeField] private GameObject viperChainPrefab = default;
	[SerializeField] private GameObject teleportExplosion = default;
	[SerializeField] private Transform augmentFirePos = default;
	[SerializeField] private GameObject EDPip1 = default;
	[SerializeField] private GameObject EDPip2 = default;
	[SerializeField] private Transform viperSource = default;
	[SerializeField] private GameObject viperWhip = default;
	[SerializeField] private GameObject viperExplosion = default;

	[Header("Movement")]
	[SerializeField] private LayerMask groundMask = default;
	[SerializeField] private float walkSpeed = 45;
	[SerializeField] private float moveSmoothTime = 0.03f;
	[SerializeField] private float groundDistance = 2;
	[SerializeField] private float airTimerThreshold = 0.4f;

	[Header("Jumping")]
	[SerializeField] private float jumpHeight = 10;
	[SerializeField] private float jumpextensionfactor = 1;
	[SerializeField] private float jumpboostfactor = 5;
	[SerializeField] private float jumpmomentum = 5;
	[SerializeField] private float jumpLandDivisor = 6;
	[SerializeField] private float walkresetrate = 5;
	[SerializeField] private float longJumpCDDuration = 2;
	[SerializeField] private float singlejumpCDduration = 0.35f;
	[SerializeField] private float smoothFallFactor = -12;
	[SerializeField] private float maxWalkSpeed = 85;
	[SerializeField] private float gravityFactor = 5;

	[Header("Dashing")]
	[SerializeField] private float dashduration = 0.3f;
	[SerializeField] private float dashcooldown = 3;
	[SerializeField] private float originalDashMultiplier = 3;
	[SerializeField] private float dashMultiplierIncrease = 2;
	[SerializeField] private float extraDashCooldown = 0.33f;
	[SerializeField] private float viperDashStrength = 5.0f;
	[SerializeField] private float viperDashDuration = 2.0f;
	[SerializeField] private float chainDestroyTime = 2.5f;

	[Header("Augments")]
	[SerializeField] private float teleportThreshold = 0.3f;
	[SerializeField] private float telePainterGrowRate = 5.3f;
	[SerializeField] private float teleportCooldownDuration = 25;
	[SerializeField] private float assimilatorShootRate = 15.0f;
	[SerializeField] private float viperShootRate = 15.0f;
	[SerializeField] private float viperPregameShootRate = 2.0f;

	[SerializeField] private float viperLaunchPower = 15.0f;
	[SerializeField] private float viperUpwardsPower = 15.0f;
	[SerializeField] private float hookLengthCDFactor = 1.0f;
	[SerializeField] private float meleeViperDistance = 30.0f;

	[SerializeField] private bool teleportEnabled = false;
	[SerializeField] internal bool viperEnabled = false;
	[SerializeField] internal bool assimilationEnabled = false;
	[Header("Sound")]
	[SerializeField] private float boomdistancethreshold = 150.0f;
	[SerializeField] private float boomTimerThreshold = 2.5f;
	[SerializeField] private float soundStepSpeed = 0.33f;

	#endregion

	#region locals

	private GameObject teleportPainter;
	private GameObject viperChainInstance;
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
	private Vector3 viperLaunchTarget;
    private Vector3 viperDashDirection;
	private Vector3 viperDashOrigin;
	private int extraDashes = 0;
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
	private bool isDashing;
	private bool jumpPressed;
	private bool isMoving;
	private bool isTeleporting = false;
	private bool teleportQueued = false;
	private bool softLandPlayed = true;
	private bool teleportReady = false;
	private bool viperreadytofire = false;
	private bool assreadytofire = false;
	private bool teleportDone = false;
	internal bool countingHookLength;
	internal bool inViperLaunch = false;
	internal bool viperDashStarted = false;
    private float originalWalkSpeed;
	private float boomTimer = 0.0f;
	private float deltaPositionY;
	private float boomdistance = 0;
	private float predashwspeed;
	private float dashActivetimer = 0;
	private float extradashActivetimer = 0;
	private float dashCDtimer = 0;
    private float extradashCDtimer = 0;
	private float longJumpCDTimer = 0;
	private float singlejumpCDTimer = 0;
	private float teleportChargeTime = 0;
	private float teleportCooldownTimer = 0;
	private float cdebugVelocityFactor = 1;
    private float airTimer = 0;
    private float dashMultiplier;
	private float viperDasht = 0;
	internal float hookLengthCounter = 0;
	internal float lastShotTimeStamp = 0;
    internal float shootRateTimeStamp = 0;

    #endregion

    #region getters and setters

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

	#endregion

	#region startup

	void Start()
	{
		InitialiseVariables();
	}

	private void InitialiseVariables()
    {
		dashMultiplier = originalDashMultiplier;
		gops = GameOptions.GetGOPS();
		HandleCheats();
        sl = GameManagerScript.GetGMScript().GetSoundLibrary();
        walk = sl.GetPlayerStepLeft();
        wingflap = sl.GetWingFlap();
        gravityFactor *= -9.81f;
        cc = GetComponent<CharacterController>();
        originalWalkSpeed = walkSpeed;
        lastPosition = transform.position;
		if (assimilationEnabled)
			dashMultiplier = originalDashMultiplier + dashMultiplierIncrease;

    }

    private void HandleCheats()
    {
        if (gops.GetCustomWalkSpeedEnabled())
            walkSpeed = gops.GetCustomWalkSpeed();
        if (gops.GetCustomTPEnabled())
            teleportCooldownDuration = gops.GetCustomTPCooldown();
    }

	#endregion

	#region updates

	void FixedUpdate()
	{
		AnimateDashUI();
		CheckGrounding();
		SmoothFallingIfGrounded();
		ResetWalkingSpeed();
		UpdateJumpCDTimers();
		if (!playerUnit.GetIsDead() && cc)
		{
			SoundWalking();
			CheckAndSetDashing();
			UpdateDash();
			UpdateDashCooldowns();
		}
		HandleBoomThreshold();
		deltaPositionY = transform.position.y - lastPosition.y;
		lastPosition = transform.position;
		isMoving = Mathf.Abs(cachedInputAxes.x) > 0.1f || Mathf.Abs(cachedInputAxes.y) > 0.1f;
	}

	private void Update()
	{
		if (!playerUnit.GetIsDead() && cc)
		{
			ControlViperDashMovement();
			MoveByAxis();
			ControlJumping();
			ControlAugments();
		}
		DisplayExtraDashes();
	}

    private void DisplayExtraDashes()
    {
		EDPip1.SetActive(extraDashes > 0);
		EDPip2.SetActive(extraDashes > 1);
	}

	private void AnimateDashUI()
	{
		if (dashActivetimer > 0)
		{
			Color color = dashborder.color;
			color.a = 1;
			color.a *= dashActivetimer / dashduration;
			dashborder.color = color;
		}
		else if (dashActivetimer <= 0 && dashborder.color.a > 0)
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
			inViperLaunch = false;
            viperDashStarted = false;
            viperDasht = 0;
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

	private void SoundWalking()
	{
		if (Mathf.Abs(cachedInputAxes.x) > 0.1f && isGrounded && !isWalkPlaying)
			StartCoroutine(playWalk());
		else if (Mathf.Abs(cachedInputAxes.y) > 0.1f && isGrounded && !isWalkPlaying)
			StartCoroutine(playWalk());
	}

	private void CheckAndSetDashing()
    {
        //DebugDashVars();
        if (dashActivetimer > 0)
        {
            walkSpeed = predashwspeed * dashMultiplier;
            isDashing = true;
        }
    }

    private void DebugDashVars()
    {
#if UNITY_EDITOR
		GameLog.ClearEditorLog();
#endif
		Debug.Log("walkSpeed = " + walkSpeed);
        Debug.Log("predashwspeed = " + predashwspeed);
        Debug.Log("dashmultiplier = " + dashMultiplier);
		Debug.Log("dashActivetimer = " + dashActivetimer);
		Debug.Log("extradashActivetimer = " + extradashActivetimer);
		Debug.Log("dashCDtimer = " + dashCDtimer);
		Debug.Log("extradashCDtimer = " + extradashCDtimer);
	}

    private void UpdateDash()
	{
		if (dashActivetimer > 0)
			dashActivetimer -= Time.deltaTime;
		else if (isDashing)
			FinishDash();
		//if (extradashActivetimer > 0)
		//	extradashActivetimer -= Time.deltaTime;
	}

	private void FinishDash()
	{
		isDashing = false;
		walkSpeed = predashwspeed;
	}

	private void UpdateDashCooldowns()
	{
		if (dashCDtimer > 0)
			dashCDtimer -= Time.deltaTime;
		if (extradashCDtimer > 0)
			extradashCDtimer -= Time.deltaTime;
	}

	private void HandleBoomThreshold()
	{
		//DebugBoomVars();
		SetBoomTimer();
		DoBoomBasicChecks();
		DoBoomFinalChecks();
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

	private void ControlAugments()
    {
		ControlTeleporting();
		ControlAssimilation();
		ControlViper();
		CountHookLength();
	}

	#endregion

	#region augments

	private void CountHookLength()
	{
		if (countingHookLength)
			hookLengthCounter += Time.deltaTime;
	}

	internal void LaunchTowards(Vector3 position)
	{
		countingHookLength = false;
		inViperLaunch = true;
		viperLaunchTarget = position;
		Vector3 velocity = Vector3.zero;
		Vector3 direction = (position - transform.position);
		velocity = direction * viperLaunchPower * Time.deltaTime;
        velocity.y += viperUpwardsPower;
        SetVelocity(velocity);
        feetASO.PlayOneShot(launchClip);
        if (SpawnController.GetIsGameActive())
            shootRateTimeStamp += hookLengthCounter * hookLengthCDFactor;
		if (viperChainInstance)
			Destroy(viperChainInstance, chainDestroyTime);
		viperWhip.transform.LookAt(position);
		viperWhip.SetActive(true);
		StartCoroutine(DisableViperWhip());
	}

    private IEnumerator DisableViperWhip()
    {
		yield return new WaitForSeconds(1);
		viperWhip.SetActive(false);
	}

	private void StartViperDash()
    {
        viperDashStarted = true;
        viperDashOrigin = transform.position;
		viperDasht = 0;
		velocity = Vector3.zero;

		Vector3 direction = /*transform.right * cachedInputAxes.x + transform.forward * cachedInputAxes.y;*/transform.forward;

		if (direction == Vector3.zero)
			direction = transform.forward;

		direction = direction.normalized;

		viperDashDirection = direction;

	}

	private void ControlViperDashMovement()
	{

		//https://youtu.be/jvPPXbo87ds 3:40 in

		//direction needs to be based on keyboard input direction, not look direction

		//need to only allow viper dashing for a while, then the dash input does a stop-dash

		//need to lerp between 3 points:

		//• the origin point of the transform at the point the dash was called,
		//• the direction with a arc power factor multiplier to get the outlier position,
		//• and the target hook point

		if (!viperDashStarted)
			return;

		if (viperDasht <= 1)
		{
			var origin = viperDashOrigin;
			var target = viperLaunchTarget;
			var direction = viperDashDirection;
			var t = viperDasht;

			var point1 = Vector3.Lerp(origin, direction, t);
			var point2 = Vector3.Lerp(direction, target, t);
			var point3 = Vector3.Lerp(point1, point2, t);
			var moveDirection = point3 - transform.position;
			moveDirection = moveDirection.normalized;
			moveDirection *= viperDashStrength;

			velocity += moveDirection;
			viperDasht += Time.deltaTime / viperDashDuration;
		}
		else
		{
			viperDasht = 0;
			viperDashStarted = false;
		}

	}

	public void SetIsTeleporting(InputAction.CallbackContext value)
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

	private void ControlViper()
	{
		if (!viperEnabled)
			return;
		viperreadytofire = Time.time > shootRateTimeStamp && isTeleporting && viperEnabled;
		if (!PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
			HandleShootingAugment();
	}

	private void ControlAssimilation()
	{
		if (!assimilationEnabled)
			return;
		assreadytofire = Time.time > shootRateTimeStamp && isTeleporting && assimilationEnabled;
		if (!PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
            HandleShootingAugment();
    }

    public virtual void HandleShootingAugment()
    {
        if (assreadytofire)
            BeginLaunchAugment(assimilatorProjPrefab, assimilatorShootRate, assimilatorFireClip);


        if (viperreadytofire)
            BeginLaunchAugment(viperProjPrefab, !Altar.GetIsGameActive() ? viperPregameShootRate : viperShootRate, viperFireClip);
    }

    internal void SetAssimilationEnabled(bool v)
    {
        assimilationEnabled = v;
		dashMultiplier = originalDashMultiplier + dashMultiplierIncrease;
	}

	internal void SetViperEnabled(bool v)
	{
		viperEnabled = v;
	}

    private void BeginLaunchAugment(GameObject projPrefab, float shootRate, AudioClip fireclip)
    {
		var camera = GameManagerScript.GetGMScript().GetCamera();
		RaycastHit[] hits = Physics.RaycastAll(augmentFirePos.position, camera.transform.forward, meleeViperDistance);
		if (hits.Length == 0)
			InstantiateProjectile(projPrefab);
		else
        {
			var position = Aiming.DetermineClosestHit(gameObject, hits).point;
			LaunchTowards(position);
			CreateViperDissipation(position);
		}

		//CreateViperChain(projectileInstance);

		shootRateTimeStamp = Time.time + shootRate;
        lastShotTimeStamp = Time.time;
        countingHookLength = true;
        hookLengthCounter = 0;
		//CreateMuzzleFlash();
		//CreateMuzzleParticles();
		SoundLibrary.PlayFromTimedASO(fireclip, transform.position);
        CameraShaker.Instance.ShakeOnce(5, 5, 0.2f, 0.5f);
        uic.readyToPlayTPPing = true;
    }

    private void CreateViperDissipation(Vector3 position)
    {
		Instantiate(viperExplosion, position, Quaternion.identity, GameManagerScript.GetGMScript().GetPJTSF());
    }

    private void CreateViperChain(GameObject projectileInstance)
    {
        viperChainInstance = Instantiate(viperChainPrefab, viperSource.position, Quaternion.identity, viperSource);
        viperChainInstance.GetComponent<SimpleLine>().SetTargets(viperSource, projectileInstance.transform);
    }

    public GameObject InstantiateProjectile(GameObject projPrefab)
	{
		var camera = GameManagerScript.GetGMScript().GetCamera();

		GameObject projectileInstance = Instantiate(projPrefab, augmentFirePos.position, Quaternion.LookRotation(camera.transform.forward, camera.transform.up), GameManagerScript.GetGMScript().GetPJTSF());
		projectileInstance.name = projectileInstance.name;
		projectileInstance.GetComponent<IBullet>().SetParent(GameManagerScript.GetGMScript().GetPlayer().gameObject);
		return projectileInstance;
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
		if (teleportPainter)
		{
			cc.Move(cachedPainterPosition - transform.position);
			var particles = Instantiate(teleportParticles, transform.position, transform.rotation, transform);
			var explosion = Instantiate(teleportExplosion, transform.position, transform.rotation, transform);
			Destroy(teleportPainter);
			feetASO.PlayOneShot(sl.GetTeleported());
			teleportChargeTime = 0;
			teleportQueued = false;
			teleportCooldownTimer = gops.GetIsTutorialMode() ? 5.0f : teleportCooldownDuration;
			cachedPainterPosition = Vector3.zero;
			teleportDone = true;
			uic.readyToPlayTPPing = true;
		}
	}

	internal void ChargeDashes()
	{
		extraDashes = 2;
		SoundLibrary.PlayFromTimedASO(sl.pipGainedPing, transform.position);
	}

	#endregion

	#region basic movement

	public void OnMovement(InputAction.CallbackContext value)
	{
		moveInputValues = value.ReadValue<Vector2>();
		bool anyvalue = Mathf.Abs(moveInputValues.x) > 0 || Mathf.Abs(moveInputValues.y) > 0;
		weaponanimator.SetMovingPressed(anyvalue);
		player.SetLeftStickValues(moveInputValues);
	}

	public void OnJumpPressed(InputAction.CallbackContext value)
	{
		if (playerUnit.GetIsDead())
			return;

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

	private void DoSingleJump()
	{
		softLandPlayed = false;
		feetASO.PlayOneShot(sl.GetPlayerJump());
		if (walkSpeed < maxWalkSpeed)
			walkSpeed += jumpboostfactor;
		velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityFactor);
		singlejumpCDTimer = singlejumpCDduration;
	}

	private void ResetJump()
	{
		velocity.y = smoothFallFactor;
		velocity.x /= jumpLandDivisor;
		velocity.z /= jumpLandDivisor;
		ResetBoomVars();
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

	#endregion

	#region dashing

	internal void AttemptDash()
	{
		/*if(inViperLaunch && !viperDashStarted)
        {
			StartViperDash();
			return;
        }*/
		if(dashActivetimer <= 0 && dashCDtimer <= 0)
		{
			DoDash();
        }
        else if (extraDashes > 0 && extradashCDtimer <= 0 && dashCDtimer >= 0 && dashActivetimer <= 0)
        {
            ResetDash();
            DoDash(true);
        }
    }

	private void ResetDash()
    {
		isDashing = false;
		if (walkSpeed > maxWalkSpeed)
			walkSpeed = maxWalkSpeed;
    }

    private void DoDash(bool extra = false)
	{
		HandleDashVars(extra);
		feetASO.PlayOneShot(dashsound);
	}

	public void StartBackdash()
	{
		if (!gops.GetCameraDebugMode())
		{
			if (player.gameObject.activeInHierarchy)
				if (dashActivetimer <= 0 && dashCDtimer <= 0)
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

    private void HandleDashVars(bool extra, bool slowVelocity = true)
    {
		if (slowVelocity && moveInputValues.magnitude != 0)
			velocity = Vector3.zero;
        dashActivetimer = dashduration;
        predashwspeed = walkSpeed;
        dashCDtimer = viperDashStarted ? 0.5f : dashcooldown;
        if (!extra)
        {
            predashwspeed = walkSpeed;
            dashCDtimer = dashcooldown;
        }
        else
        {
            extradashCDtimer = extraDashCooldown;
            extradashActivetimer = extraDashCooldown;
            extraDashes--;
        }
        Color color = dashborder.color;
        color.a = 1;
        dashborder.color = color;
		uic.readyToPlayDashPing = true;
	}

	#endregion

	#region sound

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

	private void ResetBoomVars()
	{
		boomTimer = 0;
		boomdistance = 0;
		boomdistancereached = false;
		boomTimerThresholdReached = false;
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

	#endregion
}
