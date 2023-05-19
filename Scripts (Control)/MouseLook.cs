using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class MouseLook : MonoBehaviour
{
	#region exposed parameters

	[Header("Components")]
	[SerializeField] private Camera _camera = default;
	[SerializeField] private Transform playerBody = default;
	[SerializeField] private PlayerMovement playerMovement = default;
	[SerializeField] private PlayerInput playerInput = default;
	private Player player = default;

	[Header("Conecasting")]
	[SerializeField] private LayerMask shrinesLayer = default;
	[SerializeField] private LayerMask soulsLayer = default;
	[SerializeField] public Color highlightColor = Color.green;
	[SerializeField] public Color highlightColorGrey = Color.gray;
	[ColorUsage(true, true)]
	[SerializeField] private Color highlightColorHDR = Color.green;
	[ColorUsage(true, true)]
	[SerializeField] private Color highlightColorHDRGrey = Color.gray;
	[ColorUsage(true, true)]
	[SerializeField] private Color normalSoulHDRColor = Color.black;

	[Header("Cursor lock")]
	[SerializeField] private CursorLockMode cursorLockMode = CursorLockMode.Locked;
	[SerializeField] private bool cursorvisible = false;

	[Header("Input Parameters")]
	[Tooltip("Used to divide Input Sensitivity to an acceptable amount. Used only for Mouse Movement.")]
	[SerializeField] private float sensitivityDivisor = 50;
	[Tooltip("Used to power quickturns / backdash turning strength")]
	[SerializeField] private float turnfactor = 180.0f;

	[Header("Stick Parameters")]
	[Tooltip("Used in FixedUpdate for the t factor for Smoothdamping target stickiness")]
	[SerializeField] private float moveSmoothTime = 0.03f;
	[Tooltip("Used to translate sensitivity for Stick")]
	[SerializeField] private float stickSensitivityModifier = 1;
	[Tooltip("Used to modify horizontal aiming strength")]
	[SerializeField] private float stickHorizontalModifier = 1.5f;
	[Tooltip("Used to modify the aim assist's Time.deltaTime multiplier")]
	[SerializeField] private float aimAssistDeltaFactor = 1;
	[Tooltip("Ensures Aim Assist does not multiply if below a certain threshold.")]
	[SerializeField] private float aimAssistMinMultiplier = 0.4f;
	[Tooltip("Used as a multiplier against the weighting of a LoS target's co-ordinates when performing aim assist.")]
	[SerializeField] private float aimTrackFactor = 0.6f;
	[Tooltip("")]
	[SerializeField] private bool aimAssistEnabled = true;

	[Header("Stick Acceleration")]
	[Tooltip("")]
	[SerializeField] private float stickAccelerationRate = 2.0f;
	[Tooltip("")]
	[SerializeField] private float stickAccelerationThreshold = 0.8f;

	#endregion

	#region locals

	public static string LookTargetShrine { get; private set; } = "";
	public static Transform LookTargetTransform;
	//private int lookTargetDist = 0;

	private GameOptions gops;
	private Transform lineOfSightTarget = null;
	private Vector2 cachedVelocity = Vector2.zero;
	private Vector2 cachedInputAxes = Vector2.zero;
	private Vector2 moveValues = Vector2.zero;
	private Vector2 mouseMovement = Vector2.zero;
	private Vector2 cachedMoveAxes;
	private static Color highlightColorstatic = Color.green;
	private static Color highlightColorGreystatic = Color.grey;
	private static Color highlightColorHDRstatic = Color.green;
	private static Color normalColorHDRstatic = Color.black;
	private float xRotation = 0f;
	private float mouseSensitivity;
	private float stickAcceleration = 1.0f;
	private float aimAssistMultiplier = 1;
	private bool initialised = false;
	private bool isMoving = false;

    public float GetxRotation() { return xRotation; }
	#endregion

	#region start
	void Start()
	{
		gops = GameOptions.GetGOPS();
		Cursor.lockState = cursorLockMode;
		Cursor.visible = cursorvisible;
		_camera.GetUniversalAdditionalCameraData().renderPostProcessing = gops.GetSave().postProcessingEnabled;
		highlightColorstatic = highlightColor;
		highlightColorGreystatic = highlightColorGrey;
		highlightColorHDRstatic = highlightColorHDR;
		normalColorHDRstatic = normalSoulHDRColor;
		player = GameManagerScript.GetGMScript().GetPlayerScript();
	}

	#endregion

	#region updates

	private void Update()
	{
		if (player.transform.position.y <= player.abyssDistanceThreshold)
			return;
		OrientLook();
		HandleStickMovement();
		CheckConeCasts();
	}

	void FixedUpdate()
	{
		if (player.transform.position.y <= player.abyssDistanceThreshold)
			return;

		if (!initialised)
		{
			_camera.fieldOfView = gops.GetSave().sfov;
			initialised = true;
			mouseSensitivity = gops.GetSave().inputSensitivity;
		}

		if (lineOfSightTarget && isMoving && playerInput.currentControlScheme != "Keyboard and Mouse")
			cachedMoveAxes += Vector2.SmoothDamp(cachedMoveAxes, CheckTargetCoordinates() * aimTrackFactor, ref cachedVelocity, moveSmoothTime);
	}

    #endregion

    #region cone checks

    private void CheckConeCasts()
    {
        CheckSoulsCone();
    }

	private void CheckSoulsCone()
	{
		RaycastHit[] hits = GenerateConeHits();

		if (hits.Length == 0)
		{
			LookTargetTransform = null;
			return;
		}
		if (FindNearestHit(hits) == null)
		{
			LookTargetTransform = null;
			return;
		}

		LookTargetTransform = FindNearestHit(hits);
	}

	/// <summary>
	/// generates from two lists of hits to cover a blindspot due to how conecast works
	/// </summary>
	/// <returns></returns>
	private RaycastHit[] GenerateConeHits()
	{
		var temphits1 = Aiming.ConeCastAll(transform.position, transform.forward, 50000f, 15, 50, soulsLayer);
		var temphits2 = Aiming.RayCastAll(transform.position, transform.forward, 50000f, soulsLayer);

		List<RaycastHit> hitlist = new List<RaycastHit>();
		foreach (RaycastHit item in temphits1)
		{
			if (hitlist.Contains(item))
				continue;
			hitlist.Add(item);
		}
		foreach (RaycastHit item in temphits2)
		{
			if (hitlist.Contains(item))
				continue;
			hitlist.Add(item);
		}
		var hits = hitlist.ToArray();
		return hits;
	}

	private Transform FindNearestHit(RaycastHit[] hits)
	{
		Transform closest = null;
		float minDist = Mathf.Infinity;
		foreach (RaycastHit hit in hits)
		{
			Vector3 currentPos = transform.position;
			float dist = Vector3.Distance(hit.transform.position, currentPos);

			if ((dist < minDist) && hit.transform != null)
			{
				closest = hit.transform;
				minDist = dist;
			}
		}
		return closest;
	}

	#endregion

	#region highlighting

	public static void DoHighlighting(List<MeshRenderer> renderers)
	{
		//shrines / GlowingPBR shader
		foreach (MeshRenderer renderer in renderers)
		{

			if (!renderer.material.HasProperty("_Fresnel_Color"))
				continue;
			if (!renderer.material.HasProperty("_Fresnel_Power"))
				continue;

			bool isAvailable = CheckBenchAffordable();
			Color color = isAvailable ? highlightColorstatic : highlightColorGreystatic;

			if (renderer.material.GetColor("_Fresnel_Color") != highlightColorstatic || renderer.material.GetColor("_Fresnel_Color") != highlightColorGreystatic)
                renderer.material.SetColor("_Fresnel_Color", color);

            if (renderer.material.GetFloat("_Fresnel_Power") != 0.4f)
                renderer.material.SetFloat("_Fresnel_Power", 0.4f);
        }

        //gaseous shader
        foreach (MeshRenderer renderer in renderers)
		{

			if (!renderer.material.HasProperty("_TintColor"))
				continue;

			bool isAvailable = CheckSummonAvailable();
			Color color = isAvailable ? highlightColorstatic : highlightColorGreystatic;

			if (renderer.material.GetColor("_TintColor") != highlightColorHDRstatic)
				renderer.material.SetColor("_TintColor", highlightColorHDRstatic);
		}
	}

    public static void RemoveHighlighting(List<MeshRenderer> renderers)
	{
		//shrines / GlowingPBR shader
		foreach (MeshRenderer renderer in renderers)
		{
			if (!renderer.material.HasProperty("_Fresnel_Color"))
				continue;
			if (!renderer.material.HasProperty("_Fresnel_Power"))
				continue;

			if (renderer.material.GetColor("_Fresnel_Color") != Color.black)
				renderer.material.SetColor("_Fresnel_Color", Color.black);

			if (renderer.material.GetFloat("_Fresnel_Power") != 1)
				renderer.material.SetFloat("_Fresnel_Power", 1);
		}

		//gaseous shader
		foreach (MeshRenderer renderer in renderers)
		{

			if (!renderer.material.HasProperty("_TintColor"))
				continue;

			if (renderer.material.GetColor("_TintColor") != normalColorHDRstatic)
				renderer.material.SetColor("_TintColor", normalColorHDRstatic);
		}
	}

	private static bool CheckSummonAvailable()
	{
		Resurrector res = GameManagerScript.GetGMScript().GetResurrector();
		return res.summonavailable;
	}

	private static bool CheckBenchAffordable()
	{

		SpawnController sc = GameManagerScript.GetGMScript().GetSpawnController();
		switch (LookTargetShrine)
		{
			case "corp":
				return sc.canAffordCorp;
			case "holy":
				return sc.canAffordHoly;
			case "blood":
				return sc.canAffordBlood;
		}
		return false;
	}

	#endregion

	#region stick movement

	private void HandleStickMovement()
	{
		SmoothStickMovement();
		UpdateAcceleration();
		lineOfSightTarget = CheckAssistLineOfSight();
		isMoving = playerMovement.GetIsMoving();
		CalculateAimMultiplier();
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
			Vector2 screenMidpoint = new Vector2(Screen.width / 2, Screen.height / 2);
			targetDirection = targetPos2D - screenMidpoint;
		}
		return targetDirection.normalized;
	}

	private void UpdateAcceleration()
	{
		if ((Mathf.Abs(moveValues.x) >= stickAccelerationThreshold || Mathf.Abs(moveValues.y) >= stickAccelerationThreshold) && stickAcceleration < 3)
			stickAcceleration += stickAccelerationRate * Time.deltaTime;
		else
			stickAcceleration = 1;
	}

	private Transform CheckAssistLineOfSight()
	{
		if (!aimAssistEnabled)
			return null;
		return Aiming.CheckDirectLineOfSight(transform);
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

	#endregion

	#region mouse and orientation

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

    #endregion

    #region unused

    private void CheckShrinesCone()
	{

		RaycastHit[] hits = Aiming.ConeCastAll(transform.position, transform.forward, 50000f, 15, 50, shrinesLayer);

		if (hits.Length == 0/* || !RitualRing.isInRing*/)
		{
			LookTargetShrine = "none";
			return;
		}

		/*		disabled for now

				foreach (RaycastHit hit in hits)
				{
					switch (hit.collider.gameObject.tag)
					{
						case "CorpShrine":
							LookTargetShrine = "corp";
							break;
						case "HolyShrine":
							LookTargetShrine = "holy";
							break;
						case "BloodShrine":
							LookTargetShrine = "blood";
							break;
					}
				}
		*/
		//Debug.Log("LookTargetShrine = " + LookTargetShrine);
	}
	#endregion
}
