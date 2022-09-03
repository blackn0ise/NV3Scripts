using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Text;

public class Leaderboard : MonoBehaviour
{
	[SerializeField] private TMPro.TextMeshProUGUI LBList = default;
	[SerializeField] private TMPro.TextMeshProUGUI tmpselfname = default;
	[SerializeField] private TMPro.TextMeshProUGUI tmpselfrank = default;
	[SerializeField] private int numberPerPage = 15;
	List<SteamScript.LeaderboardData> lbdataset;
	private int pageoffset = 1;
	private SteamScript steamo;
	private bool waitingForUpdate = false;
	private ELeaderboardDataRequest currenttype = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;

	void Start()
	{
		steamo = FindObjectOfType<SteamScript>();
		GetAndPopulate();

	}

	private void Update()
	{
		if (!waitingForUpdate)
			StartCoroutine(DoRenderUpdate());
	}

	IEnumerator DoRenderUpdate()
	{
		waitingForUpdate = true;
		if (steamo && SteamManager.Initialized && steamo.GetLeaderboardDataset() != null)
		{
			lbdataset = steamo.GetLeaderboardDataset();
			StartCoroutine(LoopPopulateLeaderboardList(lbdataset, pageoffset));
		}
		yield return new WaitForSeconds(0.3f);
		waitingForUpdate = false;
	}

	public void ChooseButtonAction(string action)
	{
		switch(action)
		{
			case "Global":
				pageoffset = 1;
				currenttype = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
				GetAndPopulate(currenttype);
				break;
			case "Friends":
				pageoffset = 1;
				currenttype = ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;
				GetAndPopulate(currenttype);
				break;
			case "Search":
				break;
			case "Previous":
				pageoffset -= numberPerPage;
				GetAndPopulate(currenttype);
				Debug.Log("pageoffset = " + pageoffset);
				break;
			case "Next":
				pageoffset += numberPerPage;
				GetAndPopulate(currenttype);
				Debug.Log("pageoffset = " + pageoffset);
				break;
			case "First":
				pageoffset = 1;
				GetAndPopulate(currenttype);
				Debug.Log("pageoffset = " + pageoffset);
				break;
			case "Last":
				pageoffset = (lbdataset.Count - numberPerPage) + 1;
				GetAndPopulate(currenttype);
				Debug.Log("pageoffset = " + pageoffset);
				break;
			default:
				break;
		}
	}

	public void GetAndPopulate(ELeaderboardDataRequest type = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, int rangestart = 1)
	{
		ClearFields();
		if (!steamo)
			Debug.LogError("SteamScript not found by Leaderboard!");
		if (steamo && SteamManager.Initialized)
		{
			steamo.GetLeaderBoardData(type);
			var lbdataset = steamo.GetLeaderboardDataset();
			string typeasstring = type == ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal ? "Global" : "Friend";
			if (lbdataset != null && lbdataset.Count > 0)
				GameLog.Log($"Leaderboard data retrieved. Type = {typeasstring}");
		}
		else
		{
			GameLog.Log("Steammanager not initialized!");
		}
	}

	private void ClearFields()
	{
		LBList.text = "";
		tmpselfname.text = "";
		tmpselfrank.text = "";
	}

	private IEnumerator LoopPopulateLeaderboardList(List<SteamScript.LeaderboardData> lbdataset, int startoffset = 1, float delay = 0.1f)
	{
		yield return new WaitForSeconds(delay);
		string temptext = "";

		if (lbdataset != null && lbdataset.Count > 0)
		{
			for (int i = startoffset - 1; i < startoffset - 1 + numberPerPage; i++)
			{
				if (i < lbdataset.Count && i >= 0)
				{
					if (lbdataset[i].username != null)
					{
						var finalstring = new StringBuilder();
						finalstring.Append(String.Format("{0,2:D} : {1,9:###0.000}s     {2,-18}\n", lbdataset[i].rank, (float)lbdataset[i].score / 1000, lbdataset[i].username));

						//string finalstring = $"{lbdataset[i].rank}{ChooseOrdinal(lbdataset[i].rank)} : {(float)lbdataset[i].score / 1000}s - {lbdataset[i].username}\n";
						temptext += finalstring;
					}
				}
			}
			LBList.text = temptext;
			if (steamo.GetFoundOwnResult())
			{
				tmpselfname.text = steamo.GetOwnName();
				tmpselfrank.text = steamo.GetOwnRank().ToString() + ChooseOrdinal(steamo.GetOwnRank()) + " : " + ((float)steamo.GetOwnScore() / 1000).ToString();
			}
			Debug.Log("Leaderboard populated successfully.");
		}
		else
		{
			GameLog.Log("No Leaderboard data!");
			if (SteamManager.Initialized)
				GetAndPopulate(currenttype);
		}
	}

	private string ChooseOrdinal(int rank)
	{
		switch (rank)
		{
			case 11:
				return "th";
			case 12:
				return "th";
			case 13:
				return "th";
		}
		int simplified = rank % 10;
		switch (simplified)
		{
			case 1:
				return "st";
			case 2:
				return "nd";
			case 3:
				return "rd";
			default:
				return "th";
		}
	}
}