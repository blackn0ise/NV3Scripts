using System.Dynamic;

public interface IBasicBulletShooter
{
	bool readytofire { get; set; }
	bool GetReadyToFire();
	void SetReadyToFire(bool value);
	void ConfirmReadyToFire();
}
