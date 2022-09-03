using System.Collections;
using UnityEngine;

public interface IBasicWeapon
{
	void InitialisePlayerWeapon();
	void HandleShooting();
	void BeginFireProjectile();
	void CreateMuzzleFlash();
	void aimProjectile(GameObject projectile);
	void SetUIAmmoString(string value);
	void SetWeaponAnimatorEquip(IChargingWeapon value);

}
