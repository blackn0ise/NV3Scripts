using UnityEngine;

public interface IUnit
{
	AudioClip GetDeadClip();
	int GetHealth();
	AudioClip GetHurtClip();
	int GetMaxHealth();
	void HandleDamageSoundsAndFlicker();
	bool IsDead();
	void PlayDeathSound();
	void SetDeadClip(AudioClip value);
	void SetHealth(int value);
	void SetHurtClip(AudioClip value);
	void SetMaxHealth(int value);
	void Start();
}