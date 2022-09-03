using System.Collections;

public interface IChargingWeapon
{
	float cooldownTimer { get; set; }
	bool isChargeSoundPlaying { get; set; }
	bool isFiring { get; set; }
	bool isOverheatSoundPlaying { get; set; }
	float LMBtimedown { get; set; }

	void InitialiseChargingWeapon();
	void ChargeShot();
	float GetCoolDownTime();
	float GetDelayTime();
	float GetOverheatTime();
	void Overheat();
	void PlayChargeSound();
	void ResetCharge();
	void ResetState();
	void Start();
	void StopChargeSound();
	IEnumerator StopFiring(float delay);
	IEnumerator PlayOHSound();
}