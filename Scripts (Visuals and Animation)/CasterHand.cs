using UnityEngine;

public class CasterHand : MonoBehaviour
{
	private Player player;
	private Resurrector resurrector;
	[SerializeField] private GameObject hand = default;

	private void Start()
	{
		player = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Player>();
		resurrector = GameManagerScript.GetGMScript().GetPlayer().GetComponent<Resurrector>();
		foreach (MeshRenderer mr in hand.GetComponentsInChildren<MeshRenderer>())
		{
			mr.enabled = true;
		}
		foreach (SkinnedMeshRenderer mr in hand.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			mr.enabled = true;
		}
	}

	/// <summary>
	/// Called from Casterhand's raise animation
	/// </summary>
	public void DoCast()
	{
		resurrector.SelectAndApplyCast();
	}

	public void NoLongerCasting()
	{
		player.SetIsCasting(false);
		resurrector.SetSwitchingSummons(false);
	}

}
