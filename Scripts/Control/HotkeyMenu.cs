using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HotkeyMenu : MonoBehaviour
{
	[SerializeField] private GameObject gamePersistent = default;
	[SerializeField] private AudioSource clickeraso = default;
	[SerializeField] private PlayerInput playerInput = default;
	[SerializeField] private EventSystem eventSystem = default;
	[SerializeField] private GameObject esFirstSelected = default;
	private bool isAwaitingKey = false;
	private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
	private GameOptions gops;

	private void OnEnable()
	{
		GameObject gp;
		if (!FindObjectOfType<GameOptions>())
		{
			gp = Instantiate(gamePersistent);
			gp.name = gamePersistent.name;
		}
		gops = FindObjectOfType<GameOptions>();
		eventSystem.SetSelectedGameObject(esFirstSelected);
		Load();
		PopulateKeys();
	}

	private void Load()
	{
		string rebinds = gops.GetSave().inputOverrideJson;
		if (!string.IsNullOrEmpty(rebinds))
			playerInput.actions.LoadBindingOverridesFromJson(rebinds);
	}

	public void PopulateKeys()
	{
		if (!isAwaitingKey)
		{
			playerInput.SwitchCurrentActionMap("PlayerControls");
			var actions = playerInput.currentActionMap.actions;
			InputBinding bindingmask = InputBinding.MaskByGroup(playerInput.currentControlScheme);

			foreach (TextMeshProUGUI gui in FindObjectsOfType<TextMeshProUGUI>())
			{
				foreach (InputAction action in actions)
				{
					if (gui.gameObject.name == action.name)
						gui.text = action.GetBindingDisplayString(bindingmask);
				}
			}
			playerInput.SwitchCurrentActionMap("MenuControls");
		}
	}

	public void ResetToDefaults()
	{
		gops.GetSave().inputOverrideJson = "";
		foreach(InputAction action in playerInput.actions)
		{
			action.RemoveAllBindingOverrides();
		}
		PopulateKeys();
	}


	public void AwaitKeyPress(GameObject button)
	{
		playerInput.SwitchCurrentActionMap("PlayerControls");
		var actions = playerInput.currentActionMap.actions;
		foreach (InputAction action in actions)
		{
			if (!isAwaitingKey && button.name == action.name+"Button")
			{
				isAwaitingKey = true;
				button.GetComponentInChildren<TextMeshProUGUI>().text = "...";
				StartRebinding(action, button);
			}
		}
	}

	public void StartRebinding(InputAction action, GameObject button)
	{
		InputBinding bindingmask = InputBinding.MaskByGroup(playerInput.currentControlScheme);
		playerInput.SwitchCurrentActionMap("MenuControls");
		InputActionReference actionReference = InputActionReference.Create(action);

		rebindingOperation = actionReference.action.PerformInteractiveRebinding()
			.WithControlsExcluding("Mouse")
			.WithCancelingThrough("<Keyboard>/escape")
			.WithBindingMask(bindingmask)
			.OnMatchWaitForAnother(0.1f)
			.OnComplete(operation =>
			{
				//can see definitions of cleanup and PerformInteractiveRebind at 18.15 of the video

				if (CheckDuplicateBindings(action))
				{
					operation.Cancel();
					operation.Dispose();
					AwaitKeyPress(button);
				}
				RebindComplete();
			})
			.Start();
	}

	private bool CheckDuplicateBindings(InputAction action)
	{
		for (int i = 0; i < action.bindings.Count; i++)
		{
			InputBinding newBinding = action.bindings[i];
			foreach (InputBinding binding in action.actionMap.bindings)
			{
				if (binding.action == newBinding.action)
					continue;
				if (binding.effectivePath == newBinding.effectivePath)
				{
					action.RemoveBindingOverride(i);
					GameLog.Log("Duplicate binding. Binding rejected.");
					return true;
				}
			} 
		}
		return false;
	}

	private void RebindComplete()
	{
		if (isAwaitingKey)
		{
			isAwaitingKey = false;
			rebindingOperation.Dispose();
			PopulateKeys();
			Save();
		}
	}

	public void PlayClickSound()
	{
		clickeraso.Play();
	}

	public void Save()
	{
		string rebinds = playerInput.actions.SaveBindingOverridesAsJson();
		gops.GetSave().inputOverrideJson = rebinds;
	}

}
