using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class MouseLook : MonoBehaviour
{
	[Header("Components")]
	[SerializeField] private Camera _camera = default;
	[SerializeField] private Transform playerBody = default;
	[SerializeField] private PlayerMovement playerMovement = default;
	[SerializeField] private PlayerInput playerInput = default;

	[Header("Input Parameters")]
	[SerializeField] private float sensitivityDivisor = 50;

	[Header("Stick Parameters")]
	[SerializeField] private float moveSmoothTime = 0.03f;
	[SerializeField] private float turnfactor = 180.0f;
	[SerializeField] private float stickSensitivityModifier = 1;
	[SerializeField] private float stickHorizontalModifier = 1.5f;
	[SerializeField] private float stickAccelerationRate = 2.0f;
	[SerializeField] private float stickAccelerationThreshold = 0.8f;
	[SerializeField] private float aimAssistDeltaFactor = 1;
	[SerializeField] private float aimAssistMinMultiplier = 0.4f;
	[SerializeField] private float aimTrackFactor = 0.6f;
	[SerializeField] private bool aimAssistEnabled = true;

	private GameOptions gops;
	private Transform lineOfSightTarget = null;
	private Vector2 cachedVelocity = Vector2.zero;
	private Vector2 cachedInputAxes = Vector2.zero;
	private Vector2 moveValues = Vector2.zero;
	private Vector2 mouseMovement = Vector2.zero;
	private Vector2 cachedMoveAxes;
	private float xRotation = 0f;
	private float mouseSensitivity;
	private float stickAcceleration = 1.0f;
	private float aimAssistMultiplier = 1;
	private bool initialised = false;
	private bool isMoving = false;

	public float GetxRotation() { return xRotation; }

	void Start()
	{
		gops = GameOptions.GetGOPS();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		_camera.GetUniversalAdditionalCameraData().renderPostProcessing = gops.GetSave().postProcessingEnabled;
	}

	private void Update()
	{
		OrientLook();
		HandleStickMovement();
	}

	void FixedUpdate()
	{
		if (!initialised)
		{
			_camera.fieldOfView = gops.GetSave().sfov;
			initialised = true;
			mouseSensitivity = gops.GetSave().inputSensitivity;
		}

		if (lineOfSightTarget && isMoving && playerInput.currentControlScheme != "Keyboard and Mouse")
			cachedMoveAxes += Vector2.SmoothDamp(cachedMoveAxes, CheckTargetCoordinates() * aimTrackFactor, ref cachedVelocity, moveSmoothTime);
	}

	private void HandleStickMovement()
	{
		SmoothStickMovement();
		UpdateAcceleration();
		lineOfSightTarget = CheckAssistLineOfSight();
		isMoving = playerMovement.GetIsMoving();
		CalculateAimMultiplier();
	}

	private Transform CheckAssistLineOfSight()
	{
		if (aimAssistEnabled)
		{
			int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("TriggerGround") |
		1 << LayerMask.NameToLayer("HighGround");
			layerMask = ~layerMask;
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, 50000f, layerMask))
				if (hit.collider.CompareTag("Enemies") || hit.transform.CompareTag("Enemies"))
					return hit.transform; 
			return null;
		}
		else
			return null;
	}

	private void SmoothStickMovement()
	{
		cachedInputAxes = Vector2.SmoothDamp(cachedInputAxes, moveValues, ref cachedVelocity, moveSmoothTime);
		if (lineOfSightTarget)
			cachedInputAxes *= aimAssistMultiplier;
		//Debug.Log("aimAssistMultiplier = " + aimAssistMultiplier);
		cachedMoveAxes = cachedInputAxes * mouseSensitivity / 3 * stickSensitivityModifier * stickAcceleration * stickHorizontalModifier;
		float dx = cachedMoveAxes.x;
		float dy = cachedMoveAxes.y;
		xRotation -= dy;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);
		transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		TurnBody(dx);
	}

	private Vector2 CheckTargetCoordinates()
	{
		Vector2 targetDirection = new Vector2();
		if (lineOfSightTarget)
		{
			Vector3 targetScreenPos = _camera.WorldToScreenPoint(lineOfSightTarget.position);
			Vector2 targetPos2D = new Vector2(targetScreenPos.x, targetScreenPos.y);
			Vector2 screenMidpoint = new Vector2(Screen.width/2, Screen.height / 2);
			targetDirection = targetPos2D - screenMidpoint;
		}
		return targetDirection.normalized;
	}

	public void OnMouseLook(InputAction.CallbackContext value)
	{
		//this is currently only triggering when a delta is observed through the input controller.
		//this needs to be checked in a player update loop and checked each frame.
		Vector2 inputMovement = value.ReadValue<Vector2>();
		mouseMovement.x = inputMovement.x * mouseSensitivity / sensitivityDivisor;
		mouseMovement.y = inputMovement.y * mouseSensitivity / sensitivityDivisor;
	}

	private void OrientLook()
	{
		xRotation -= mouseMovement.y;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		TurnBody(mouseMovement.x);
	}

	public void OnStickLook(InputAction.CallbackContext value)
	{
		moveValues = value.ReadValue<Vector2>();
	}

	public void DoQuickTurn()
	{
		StartCoroutine(DoTurn());
	}

	private IEnumerator DoTurn()
	{
		float rotationAmount = turnfactor * Time.deltaTime;
		float turnTotal = 180.0f;
		float turnAmount = 0;
		while (turnAmount <= turnTotal)
		{
			TurnBody(rotationAmount);
			turnAmount += turnfactor * Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}
	}

	private void TurnBody(float dx)
	{
		playerBody.Rotate(Vector3.up * dx);
	}

	private void UpdateAcceleration()
	{
		if ((Mathf.Abs(moveValues.x) >= stickAccelerationThreshold || Mathf.Abs(moveValues.y) >= stickAccelerationThreshold) && stickAcceleration < 3)
			stickAcceleration += stickAccelerationRate * Time.deltaTime;
		else
			stickAcceleration = 1;
	}

	private void CalculateAimMultiplier()
	{
		if (lineOfSightTarget && aimAssistMultiplier >= aimAssistMinMultiplier)
			aimAssistMultiplier -= aimAssistDeltaFactor * Time.deltaTime;
		else if (aimAssistMultiplier <= 1)
			aimAssistMultiplier += aimAssistDeltaFactor * Time.deltaTime;
		else
			aimAssistMultiplier = 1;
	}
}
