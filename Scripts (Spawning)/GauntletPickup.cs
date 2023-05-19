using UnityEngine;

public class GauntletPickup : Pickup
{
	private Player player;
	private SoundLibrary sl;
	private GameOptions gops;
	private CorporealShrine weaponBench;

	public override void Start()
    {
        Initialise();
        InitialiseGauntlets();
        gops = GameOptions.GetGOPS();
        Invoke("CheckStartWith", 0.2f);
    }

    private void CheckStartWith()
    {
        if (gops)
            if (gops.GetSave().startWithGauntlets)
            {
                GameManagerScript.GetGMScript().GetUIController().ActivateGem(0);
                Destroy(gameObject);
            }
    }

    private void InitialiseGauntlets()
    {
        gops = GameOptions.GetGOPS();
        player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		sl = GameManagerScript.GetGMScript().GetSoundLibrary();
		weaponBench = FindObjectOfType<CorporealShrine>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (gops)
			if (gops.GetSave().startWithGauntlets)
				Destroy(gameObject);

		Pickup pickup = GetComponentInChildren<Pickup>();

		if (player && other.gameObject.GetComponent<Player>() && pickup)
		{
			gops.GetSave().hasFoundGauntlets = true;
			gops.GetSave().startWithGauntlets = true;
			SaveSystem.SaveGame(gops, gops.audioMixer);
			player.GetPlayerOwnedWeapons().Add(sc.provideGameObject(pickup.GetPickupName()));
			player.AnimateSwapWeapon(player.GetCurrentWeapon(), pickup.GetPickupName());
			SoundLibrary.PlayFromTimedASO(sl.GetPickupGet(), transform.position);
			GameManagerScript.GetGMScript().GetUIController().ActivateGem(0);
			foreach (GauntletPickup gp in FindObjectsOfType<GauntletPickup>())
			{
				Destroy(gp.gameObject, 0.2f);
			}

		}
	}
}
