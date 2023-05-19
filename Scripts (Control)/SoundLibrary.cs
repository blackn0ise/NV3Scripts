using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
	#region exposed parameters and locals

	[Header("Timed ASOs")]
	[SerializeField] private GameObject timedASO = default;
	[SerializeField] private Transform timedASOCollection = default;

	[Header("Music")]
	public static int trackCount;
	[SerializeField] private List<AudioClip> tracklist = default;
	[SerializeField] private AudioClip tutorial = default;

	[Header("Banshee sounds")]
	[SerializeField] private List<AudioClip> bansheeChargeList = default;
	[SerializeField] private List<AudioClip> bansheeFireList = default;

	[Header("Player")]
	[SerializeField] private AudioClip playerPB = default;
	[SerializeField] private AudioClip playerDead = default;
	[SerializeField] private AudioClip teleported = default;
	[SerializeField] private AudioClip playerJump = default;
	[SerializeField] private AudioClip playerStepLeft = default;
	[SerializeField] private AudioClip playerStepRight = default;
	[SerializeField] private AudioClip wingflap = default;
	[SerializeField] private AudioClip detriHeal = default;
	[SerializeField] private AudioClip detriRes = default;
	[SerializeField] private AudioClip detriBank = default;
	[SerializeField] internal AudioClip cdReadyPing = default;
	[SerializeField] internal AudioClip dashReadyPing = default;
	[SerializeField] internal AudioClip pipGainedPing = default;
	[SerializeField] internal AudioClip UIClick = default;

	[Header("Player hurt sounds")]
	[SerializeField] private List<AudioClip> playerHurtList = default;

	[Header("Hit confirms")]
	[SerializeField] internal AudioClip goodHit = default;
	[SerializeField] internal AudioClip armorHit = default;
	[SerializeField] internal AudioClip terrainHit = default;

	[Header("Enemies")]
	[SerializeField] private AudioClip revenantDead = default;
	[SerializeField] private AudioClip revenantHurt = default;
	[SerializeField] private AudioClip dreadnoughtDead = default;
	[SerializeField] private AudioClip dreadnoughtHurt = default;
	[SerializeField] private AudioClip dreadnoughtWPDead = default;
	[SerializeField] private AudioClip juggernautDead = default;
	[SerializeField] private AudioClip juggernautHurt = default;
	[SerializeField] private AudioClip terrorDead = default;
	[SerializeField] private AudioClip terrorHurt = default;
	[SerializeField] private AudioClip necroDead = default;
	[SerializeField] private AudioClip necroHurt = default;
	[SerializeField] private AudioClip lichDead = default;
	[SerializeField] private AudioClip lichHurt = default;
	[SerializeField] private List<AudioClip> lichspawnList = default;
	[SerializeField] private AudioClip lacerDead = default;
	[SerializeField] private AudioClip lacerHurt = default;
	[SerializeField] private AudioClip vizierDead = default;
	[SerializeField] private AudioClip vizierHurt = default;

	[Header("Pickups")]
	[SerializeField] private AudioClip pickupSpawned = default;
	[SerializeField] private AudioClip quadbullet = default;
	[SerializeField] private AudioClip pickupGet = default;
	[SerializeField] private AudioClip highpickupGet = default;
	[SerializeField] private AudioClip annihilation = default;
	[SerializeField] private AudioClip summonUpgrade = default;
	[SerializeField] private AudioClip newsummon = default;
	[SerializeField] private AudioClip wingsready = default;
	[SerializeField] private AudioClip healthUpgrade = default;
	[SerializeField] private AudioClip godhand = default;

    #endregion

    #region getters and setters

    public AudioClip GetTutorialTrack() { return tutorial; }
	//Player getters
	public AudioClip GetPlayerDead() { return playerDead; }
	public AudioClip GetPlayerPB() { return playerPB; }
	public AudioClip GetTeleported() { return teleported; }
	public AudioClip GetPlayerJump() { return playerJump; }
	public void SetPlayerJump(AudioClip value) { playerJump = value; }
	public AudioClip GetPlayerStepLeft() { return playerStepLeft; }
	public AudioClip GetPlayerStepRight() { return playerStepRight; }
	public AudioClip GetWingFlap() { return wingflap; }
	public AudioClip GetDetriHeal() { return detriHeal; }
	public AudioClip GetDetriRes() { return detriRes; }
	public AudioClip GetDetriBank() { return detriBank; }
	public List<AudioClip> GetBansheeChargeList() { return bansheeChargeList; }
	public List<AudioClip> GetBansheeFireList() { return bansheeFireList; }
	//Enemy getters
	public AudioClip GetRevenantDead() { return revenantDead; }
	public AudioClip GetRevenantHurt() { return revenantHurt; }
	public AudioClip GetDreadnoughtDead() { return dreadnoughtDead; }
	public AudioClip GetDreadnoughtHurt() { return dreadnoughtHurt; }
	public AudioClip GetDreadnoughtWPDead() { return dreadnoughtWPDead; }
	public AudioClip GetJuggernautDead() { return juggernautDead; }
    public AudioClip GetJuggernautHurt() { return juggernautHurt; }
	public AudioClip GetTerrorDead() { return terrorDead; }
	public AudioClip GetTerrorHurt() { return terrorHurt; }
	public AudioClip GetNecroDead() { return necroDead; }
	public AudioClip GetNecroHurt() { return necroHurt; }
	public AudioClip GetLichDead() { return lichDead; }
	public AudioClip GetLichHurt() { return lichHurt; }
	public AudioClip GetLacerDead() { return lacerDead; }
	public AudioClip GetLacerHurt() { return lacerHurt; }
	public AudioClip GetVizierDead() { return vizierDead; }
	public AudioClip GetVizierHurt() { return vizierHurt; }
	//Pickup getters
	public AudioClip GetPickupSpawned() { return pickupSpawned; }
	public AudioClip GetQuadBullet() { return quadbullet; }
	public AudioClip GetPickupGet() { return pickupGet; }
	public AudioClip GetHighPickupGet() { return highpickupGet; }
	public AudioClip GetAnnihilation() { return annihilation; }
	public AudioClip GetSummonUpgrade() { return summonUpgrade; }
	public AudioClip GetNewSummon() { return newsummon; }
	public AudioClip GetWingsReady() { return wingsready; }
	public AudioClip GetHealthUpgrade() { return healthUpgrade; }
	public AudioClip GetGodhand() { return godhand; }

    #endregion

    private void Start()
	{
		trackCount = tracklist.Count;
	}

	public static void VarySoundPitch(AudioSource aso, float pitchvariance)
	{
		aso.pitch += Random.Range(-1 * pitchvariance, pitchvariance);
	}

	public static void ResetPitch(AudioSource aso)
	{
		aso.pitch = 1;
	}

	public AudioClip ChoosePlayerHurt()
	{
		int random = Random.Range(0, playerHurtList.Count);
		return playerHurtList[random];
	}

	public AudioClip ChooseLichSpawn()
	{
		int random = Random.Range(0, lichspawnList.Count);
		return lichspawnList[random];
	}

	public AudioClip GetTrack(MusicPlayer mp)
	{
		if (mp.dontCycle)
			return tracklist[0];
		int index = 0;
		for (int i = 0; i < tracklist.Count; i++)
		{
			if (tracklist[mp.GetLastTrack()] == tracklist[i])
			{
				int container = i;
				if (i == tracklist.Count - 1)
					container = 0;
				else
					container = i + 1;
				index = container;
			}
		}
		mp.SetLastTrack(index);
		return tracklist[index];
	}

	public static AudioClip ChooseRandomFromList(List<AudioClip> list)
	{
		int random = Random.Range(0, list.Count);
		return list[random];
	}

	public static void PlayFromTimedASO(AudioClip clip, Vector3 position, float volume = 1, float pitchVariance = 0)
    {
		var sl = GameManagerScript.GetGMScript().GetSoundLibrary();
		var instance = Instantiate(sl.timedASO, position, Quaternion.identity, sl.timedASOCollection);
		var timedAso = instance.GetComponent<TimedASO>();
		timedAso.SetClip(clip);
		timedAso.SetVolume(volume);
		timedAso.aso.pitch += Random.Range(-1 * pitchVariance, pitchVariance);

	}
}
