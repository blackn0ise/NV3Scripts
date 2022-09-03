using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeWall : MonoBehaviour
{
	public void reloadLevel()
	{
		GameManagerScript.GetGMScript().DoRestartGame();
	}
}
