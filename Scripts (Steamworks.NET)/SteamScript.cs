using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using System;

public class SteamScript : MonoBehaviour
{
	public struct LeaderboardData
	{
		public string username;
		public int rank;
		public int score;
	}

	protected Callback<GameOverlayActivated_t> gameOverlayActivated;
	private CallResult<NumberOfCurrentPlayers_t> numberCurrentPlayers;
	private CallResult<LeaderboardFindResult_t> LBFindResult = new CallResult<LeaderboardFindResult_t>();
	private CallResult<LeaderboardScoreUploaded_t> LBUploadResult = new CallResult<LeaderboardScoreUploaded_t>();
	private CallResult<LeaderboardScoresDownloaded_t> LBDownloadResult = new CallResult<LeaderboardScoresDownloaded_t>();

	[SerializeField] private AudioMixer audioMixer = default;
	[SerializeField] private bool offlineTesting = true;
	private bool foundOwnResult = false;
	private bool LBInitialized = false;
	private bool pbreset = false;
	private int ownScore = 0;
	private int ownRank = 0;
	private string ownName = "";
	private int retryCount = 0;
	private int retryTimes = 5;
	private SteamLeaderboard_t currentLeaderboard;
	List<LeaderboardData> LeaderboardDataset;
	private GameOptions gops;
	public static SteamScript instance;

	public List<LeaderboardData> GetLeaderboardDataset() { return LeaderboardDataset; }
	public bool GetOfflineTesting() { return offlineTesting; }
	public bool GetFoundOwnResult() { return foundOwnResult; }
	public int GetOwnScore() { return ownScore; }
	public int GetOwnRank() { return ownRank; }
	public string GetOwnName() { return ownName; }

	//OnEnable is before Start but after Awake, and can be called again after OnDestroy.

	private void OnEnable()
	{
		instance = this;
		retryCount = retryTimes;
		if (SteamManager.Initialized) //this needs to be checked before calling any SW functions
		{
			TestSteamworks();
			gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(CheckGameOverlayActivated);
			numberCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(CheckNumCurrentPlayers);
			TryFindLeaderboard();
			StartCoroutine(DelayedGetLeaderboardData(1.0f));
		}
	}

	private void TestSteamworks()
	{
		string name = SteamFriends.GetPersonaName();
		GameLog.Log(name + " is now using SteamWorks.NET");
	}

	private void TryFindLeaderboard()
	{
		string targetLeaderboard = offlineTesting ? "OfflinePBTest" : "PersonalBest";
		SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(targetLeaderboard);
		LBFindResult.Set(hSteamAPICall, CheckLeaderboardFindResult);
		GameLog.Log(targetLeaderboard + " leaderboard Find Result set.");
	}

	private void CheckLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
	{
		Debug.Log($"Steam Leaderboard Find Results: Failed? {failure}, Found: {pCallback.m_bLeaderboardFound}, leaderboardID: {pCallback.m_hSteamLeaderboard.m_SteamLeaderboard}");
		if (!failure)
		{
			currentLeaderboard = pCallback.m_hSteamLeaderboard;
			LBInitialized = true;
			GameLog.Log("Leaderboard find result succeeded.");
		}
		else
		{
			GameLog.Log("Leaderboard find result failed.");
		}
	}

	private IEnumerator DelayedGetLeaderboardData(float delay = 0.5f, ELeaderboardDataRequest type = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal)
	{
		yield return new WaitForSeconds(delay);
		while (!foundOwnResult && retryCount > 0)
		{
			if (retryCount != retryTimes)
				GameLog.Log("Unable to find own result on leaderboard. Trying again...");
			retryCount--;
			GetLeaderBoardData(type);
			yield return new WaitForSeconds(1.0f);
		}
		retryCount = retryTimes;
		if (!foundOwnResult)
			GameLog.AddLog("CAUTION : Unable to find own result. Score uploads will overwrite any of your existing scores!");
	}

	public void GetLeaderBoardData(ELeaderboardDataRequest _type = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, int entries = 9999999, int rangestart = 1)
	{
		SteamAPICall_t hSteamAPICall;
		switch (_type)
		{
			case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal:
				hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, _type, rangestart, entries);
				LBDownloadResult.Set(hSteamAPICall, CheckLeaderboardDownloadResult);
				break;
			case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser:
				hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, _type, -(entries / 2), (entries / 2));
				LBDownloadResult.Set(hSteamAPICall, CheckLeaderboardDownloadResult);
				break;
			case ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends:
				hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, _type, rangestart, entries);
				LBDownloadResult.Set(hSteamAPICall, CheckLeaderboardDownloadResult);
				break;
		}
		//Note that the LeaderboardDataset will not be updated immediatly (see callback below)
	}

	private void CheckLeaderboardDownloadResult(LeaderboardScoresDownloaded_t pCallback, bool failure)
	{
		Debug.Log($"Steam Leaderboard Download: Did it fail? {failure}, Result - {pCallback.m_hSteamLeaderboardEntries}");

		LeaderboardDataset = new List<LeaderboardData>();

		//Iterates through each entry gathered in leaderboard
		for (int i = 0; i < pCallback.m_cEntryCount; i++)
		{
			LeaderboardEntry_t leaderboardEntry;
			SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out leaderboardEntry, null, 0);
			//Example of how leaderboardEntry might be held/used

			LeaderboardData lD = InitLeaderboardEntry(leaderboardEntry);

			LeaderboardDataset.Add(lD);
			//Debug.Log($"User: {lD.username} - Score: {lD.score} - Rank: {lD.rank}");
			foundOwnResult = LookForOwnResult(foundOwnResult, lD);
		}
		CheckPBReset();
	}

	private static LeaderboardData InitLeaderboardEntry(LeaderboardEntry_t leaderboardEntry)
	{
		LeaderboardData lD;
		lD.username = SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser);
		lD.rank = leaderboardEntry.m_nGlobalRank;
		lD.score = leaderboardEntry.m_nScore;
		return lD;
	}

	private void Start()
	{
		gops = GameOptions.GetGOPS();
		if (SteamManager.Initialized) 
		{
			GameLog.Log("SteamManager initialized successfully");
		}
		else
		{
			GameLog.Log("Unable to initialise Steam Manager. Steam needs to be active.");
		}
	}

	private void Update()
	{
		if (SteamManager.Initialized)
		{
			if (!LBInitialized)
				TryFindLeaderboard();

			if (offlineTesting)
			{
				//if (Input.GetKeyDown(KeyCode.M))
				//{
				//	SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
				//	numberCurrentPlayers.Set(handle);
				//	Debug.Log("Called GetNumberOfCurrentPlayers()");
				//}
				//if (Input.GetKeyDown(KeyCode.N))
				//{
				//	UpdateScore(1, true);
				//	GameLog.Log("Force updated score to 1");
				//}
			}
		}
	}

	internal void ClearLeaderboardDataset()
	{
		LeaderboardDataset = null;
	}

	public void UpdateScore(int score, bool force)
	{
		//if not force, will keep best.
		if (!LBInitialized)
		{
			GameLog.Log("Leaderboard not initialized");
		}
		else
		{
			SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(currentLeaderboard, 
				force? ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate : ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
			LBUploadResult.Set(hSteamAPICall, CheckLeaderboardUploadResult);
			GameLog.Log("Leaderboard result uploaded");
		}
	}

	private void CheckLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
	{
		GameLog.Log($"Steam Leaderboard Upload: Failed? {failure}, Score: {pCallback.m_nScore}, HasChanged: {pCallback.m_bScoreChanged}");
	}

	private void CheckGameOverlayActivated(GameOverlayActivated_t pCallback) {
		if(pCallback.m_bActive != 0) {
			Debug.Log("Steam Overlay has been activated");
			//should pause at this point
		}
		else {
			Debug.Log("Steam Overlay has been closed");
			//should resume at this point
		}
	}

	private void CheckNumCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_bSuccess != 1 || bIOFailure)
		{
			Debug.Log("There was an error retrieving the NumberOfCurrentPlayers.");
		}
		else
		{
			Debug.Log("The number of players playing your game: " + pCallback.m_cPlayers);
		}
	}

	private bool LookForOwnResult(bool foundOwnResult, LeaderboardData lD)
	{
		if (lD.username == SteamFriends.GetPersonaName())
		{
			foundOwnResult = true;
			retryCount = retryTimes;
			ownScore = lD.score;
			ownName = lD.username;
			ownRank = lD.rank;
			SaveHighScoreLocal(ownScore);
			GameLog.Log("Found result for " + ownName);
			GameLog.Log($"Score = {lD.score}, Rank: {ownRank}");
		}
		return foundOwnResult;
	}

	private void CheckPBReset()
	{
		if (!foundOwnResult && !pbreset)
		{
			ResetPB();
			pbreset = true;
		}
	}

	private void ResetPB()
	{
		GameLog.Log("Could not find result for " + SteamFriends.GetPersonaName());
		GameLog.Log("Resetting local result to 0...");

		SaveHighScoreLocal(0);
	}

	private void SaveHighScoreLocal(int score)
	{
		//save local
		gops.GetSave().highScore = score;
		SaveSystem.SaveGame(gops,audioMixer);
		GameLog.Log("Settings saved.");
		gops.LoadSaveData();
	}
}