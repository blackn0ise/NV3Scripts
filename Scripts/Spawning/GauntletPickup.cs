using UnityEngine;

public class GauntletPickup : Pickup
{
	private Player player;
	private AudioSource playeraso;
	private SpawnController sc;
	private SoundLibrary sl;
	private GameOptions gops;
	private WeaponBench weaponBench;
	private GameObject ownershipGem;

	public override void Start()
	{
		Initialise();
		InitialiseGauntlets();
		gops = GameOptions.GetGOPS();
		if(gops)
			if (gops.GetHasGauntlets())
			{
				if (weaponBench && ownershipGem)
				{
					weaponBench.UpdateMaterial(ownershipGem);
					weaponBench.ActivatePip(0);
				}
				Destroy(gameObject);
			}
	}

	private void InitialiseGauntlets()
	{
		gops = GameOptions.GetGOPS();
		player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		playeraso = player.gameObject.GetComponent<AudioSource>();
		sc = GameManagerScript.GetGMScript().GetSpawnController();
		sl = GameManagerScript.GetGMScript().GetSoundLibrary();
		weaponBench = FindObjectOfType<WeaponBench>();
		ownershipGem = weaponBench.GetGem(0);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (gops)
			if (gops.GetHasGauntlets())
				Destroy(gameObject);

		Pickup pickup = GetComponentInChildren<Pickup>();

		if (player && other.gameObject.GetComponent<Player>() && pickup)
		{
			gops.SetHasGauntlets(true);
			player.GetPlayerOwnedWeapons().Add(sc.provideGameObject(pickup.GetPickupName()));
			player.AnimateSwapWeapon(player.GetCurrentWeapon(), pickup.GetPickupName());
			playeraso.PlayOneShot(sl.GetPickupGet());
			if (weaponBench && ownershipGem)
			{
				weaponBench.UpdateMaterial(ownershipGem);
				weaponBench.ActivatePip(0);
			}
			foreach (GauntletPickup gp in FindObjectsOfType<GauntletPickup>())
			{
				Destroy(gp.gameObject, 0.2f);
			}

		}
	}
}
