using System;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
	[SerializeField] private PlayerMovement pm = default;
	[SerializeField] private Player player = default;
	[SerializeField] private Animator animator = default;
	private bool movingPressed;
	private Vector3 originalPosition;
	private IChargingWeapon equippedChargingWeapon;

	public Vector3 GetGunSpawnLoc() { return originalPosition; }
	public Animator GetAnimator() { return animator; }
	public void SetMovingPressed(bool value) { movingPressed = value; }
	public void SetChargingWeaponEquipped(IChargingWeapon value) { equippedChargingWeapon = value; }

	private void Start()
	{
		originalPosition = new Vector3(0, -0.427f, 0.63f);
	}

	private void Update()
	{
		PerformAnimations();
		StopContinuous();
	}

	private void StopContinuous()
	{
		if (!Player.GetShootingPressed())
			DeactivateWeaponIfNotFiring();
	}

	private void DeactivateWeaponIfNotFiring()
	{
		if (equippedChargingWeapon != null)
		{
			if (!equippedChargingWeapon.isFiring)
				animator.SetBool("IsShooting", false);
		}
		else
			animator.SetBool("IsShooting", false);
	}

	private void PerformAnimations()
	{
		if (Player.GetShootingPressed() && !PlayerUnit.GetPlayerDead() && !Player.GetIsCasting() && !Player.GetIsSwapping())
		{
			if (player.GetCurrentWeapon())
				AnimateWeaponsOnClick();
		}
		else if (movingPressed && pm.GetIsGrounded())
			AnimateMovement();
		else if (equippedChargingWeapon == null)
			ResetToIdle();
		else if (equippedChargingWeapon != null)
			ResetChargersIfNotFiring();
		else
			AllowForShootingAnimations();
	}

	private void ResetChargersIfNotFiring()
	{
		if (!equippedChargingWeapon.isFiring)
		{
			animator.SetBool("IsShooting", false);
			animator.SetBool("IsMoving", false);
		}
	}

	private void AllowForShootingAnimations()
	{
		animator.SetBool("IsMoving", false);
	}

	private void ResetToIdle()
	{
		animator.SetBool("IsShooting", false);
		animator.SetBool("IsMoving", false);
	}

	private void AnimateMovement()
	{
		animator.SetBool("IsMoving", true);
	}

	private void AnimateWeaponsOnClick()
	{
		if (player.GetCurrentWeapon().name == "Reaver" || 
			player.GetCurrentWeapon().name == "Devourer" || 
			player.GetCurrentWeapon().name == "Void Cannon")
			AnimateChargeWeaponsOnClick();
		else if (player.GetCurrentWeapon().name == "Crucifier" ||
			player.GetCurrentWeapon().name == "Triptikoss" ||
			player.GetCurrentWeapon().name == "Sledgehammer" ||
			player.GetCurrentWeapon().name == "Goregun")	{/*do nothing*/}
		else
			AnimateBasicWeapons();
	}

	private void AnimateBasicWeapons()
	{
		animator.SetBool("IsShooting", true);
		animator.SetBool("IsMoving", false);
	}

	private void AnimateChargeWeaponsOnClick()
	{
		if (equippedChargingWeapon != null)
			if (equippedChargingWeapon.isFiring)
			{
				animator.SetBool("IsShooting", true);
				animator.SetBool("IsMoving", false);
			}
			else
			{
				animator.SetBool("IsShooting", false);
				animator.SetBool("IsMoving", false);
			}
	}

	public void AnimateOneSingleshot()
	{
		bool quickboi =
			player.GetCurrentWeapon().name == "Triptikoss" ||
			player.GetCurrentWeapon().name == "Goregun" ||
			player.GetCurrentWeapon().name == "Sledgehammer";
		animator.SetTrigger(quickboi? "QuickSingleShot" : "SingleShot");
		animator.SetBool("IsShooting", false);
		animator.SetBool("IsMoving", false);
	}

	public void SpawnWeapon()
	{
		player.EquipWeapon();
	}

	public void SwapDone()
	{
		player.SetIsSwapping(false);
		animator.SetTrigger("SwapDone");
	}

	/// <summary>
	///  Triggered by animation through SpellCast trigger in Weapon Animator
	/// </summary>
	public void StartCasting()
	{
		player.RaiseAndCastLHand();
	}
}
