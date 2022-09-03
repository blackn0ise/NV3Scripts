using System.Collections;

public interface IAmmoUser
{
	int GetAmmoCount();
	IEnumerator PlayOOAClip();
	void TriggerAmmoUI();
}