using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponWheel : MonoBehaviour
{
	[SerializeField] private Image[] wWheelImages = default;
	[SerializeField] private TextMeshProUGUI[] wWheelLabels = default;
	[SerializeField] private Player player = default;
	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private PowerupBench powerupBench = default;
	[SerializeField] private float slowdownamount = 0.4f;
	private Vector2 currentSelection;

	void OnEnable()
    {
		wWheelImages[0].color = Color.white;
		wWheelImages[2].color = Color.white;
		SlowDownTimeAndPitch();
		LabelAndColourWeapons();
	}

	private void LabelAndColourWeapons()
	{
		for(int i = 1; i < wWheelLabels.Length; i++)
		{
			PopulateWeaponAndVariants(i);
		}

	}

	private string PopulateWeaponAndVariants(int i)
	{
		string weapon = "";
		switch(i)
		{
			case 0:
				wWheelImages[i].color = Color.white;
				break;
			case 1:
				weapon = "Gauntlets";
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
			case 2:
				weapon = CheckSnubnoseVariant();
				ModifyLabelText(weapon, i);
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
			case 3:
				weapon = player.CheckTargetVariant("Shotgun");
				ModifyLabelText(weapon, i);
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
			case 4:
				weapon = player.CheckTargetVariant("Crucifier");
				ModifyLabelText(weapon, i);
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
			case 5:
				weapon = player.CheckTargetVariant("Reaver");
				ModifyLabelText(weapon, i);
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
			case 6:
				weapon = "Devourer";
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
			case 7:
				weapon = "Void Cannon";
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
			case 8:
				weapon = "Godhand";
				ColourWedgeOnOwnership(weapon, i);
				return weapon;
		}
		return weapon;
	}

	private void ModifyLabelText(string weapon, int i)
	{
		wWheelLabels[i].text = weapon;
	}

	private void ColourWedgeOnOwnership(string weapon, int i)
	{
		if(player.CheckWeaponOwned(weapon))
			wWheelImages[i].color = Color.white;
		else
			wWheelImages[i].color = Color.black;
	}

	private string CheckSnubnoseVariant()
	{
		if (player.CheckWeaponOwned("Snubnose"))
			return "Snubnose";
		else if (player.CheckWeaponOwned("Penance"))
			return "Penance";
		else if (player.CheckWeaponOwned("Hand Cannon"))
			return "Hand Cannon";
		else
			return "BoneBag";
	}

	// Update is called once per frame
	void Update()
	{
		CheckSelection();
	}

	private void SlowDownTimeAndPitch()
	{
		if (Time.timeScale == 1 && !powerupBench.GetIsInSlowdown())
		{
			Time.timeScale = slowdownamount;
			audioMixer.SetFloat("SFXPitch", slowdownamount);
		}
	}

	private void ResetTime()
	{
		if (!powerupBench.GetIsInSlowdown())
		{
			Time.timeScale = 1;
			audioMixer.SetFloat("SFXPitch", 1); 
		}
	}

	internal void ReadSelectionInput(Vector2 value)
	{
		currentSelection = value;
	}

	public void CheckSelection()
	{
		if (wWheelImages[GetDivisionOfCircle(GetPositionOnCircle(currentSelection))])
		{
			wWheelImages[GetDivisionOfCircle(GetPositionOnCircle(currentSelection))].color = Color.green;
			for (int i = 0; i < wWheelImages.Length; i++)
			{
				if (i != GetDivisionOfCircle(GetPositionOnCircle(currentSelection)))
				{
					PopulateWeaponAndVariants(i);
				}
			}
		}
	}

	private int GetDivisionOfCircle(float v)
	{
		return (int)v / 40 == 0 ? 0 : Mathf.Abs((int)v / 40 - 9);
	}

	private float GetPositionOnCircle(Vector2 moveValues)
	{
		float x = moveValues.x;
		float y = moveValues.y;
		if (x != 0.0f || y != 0.0f)
			return Mathf.Atan2(y, x) * Mathf.Rad2Deg + 180;
		return 0;
	}

	public void ConfirmSelection(bool value)
	{
		//Debug.Log(value.ReadValueAsButton());
		if (!value) //this means button is released
		{
			var selection = GetDivisionOfCircle(GetPositionOnCircle(currentSelection));
			switch (selection)
			{
				case 0:
					ExitWheel();
					break;
				case 1:
					player.OnW1Pressed();
					ExitWheel();
					break;
				case 2:
					player.OnW2Pressed();
					ExitWheel();
					break;
				case 3:
					player.OnW3Pressed();
					ExitWheel();
					break;
				case 4:
					player.OnW4Pressed();
					ExitWheel();
					break;
				case 5:
					player.OnW5Pressed();
					ExitWheel();
					break;
				case 6:
					player.OnW6Pressed();
					ExitWheel();
					break;
				case 7:
					player.OnW7Pressed();
					ExitWheel();
					break;
				case 8:
					player.OnW8Pressed();
					ExitWheel();
					break;
			}
		}
	}

	private void ExitWheel()
	{
		ResetTime();
	}
}
