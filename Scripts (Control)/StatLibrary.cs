using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class StatLibrary : MonoBehaviour
{
	private int souls = 0;
	public void SetSouls(int gains) { souls = gains; }
	public int GetSouls() { return souls; }

	private int weaponBank;
	public void SetWeaponBank(int gains) { weaponBank = gains; }
	public int GetWeaponBank() { return weaponBank; }

	private int spiritBank;
	public void SetSpiritBank(int gains) { spiritBank = gains; }
	public int GetSpiritBank() { return spiritBank; }

	private int bloodBank = 0;
	public void SetBloodBank(int gains) { bloodBank = gains; }
	public int GetBloodBank() { return bloodBank; }

	private bool powerupactive = false;
	public void SetPowerupActive(bool value) { powerupactive = value; }
	public bool GetPowerupActive() { return powerupactive; }

    private void Start()
    {
		if (GameOptions.GetGOPS() != null)
			if (GameOptions.GetGOPS().GetCustomSoulsEnabled())
				souls = GameOptions.GetGOPS().GetStartingSouls();

	}

}
