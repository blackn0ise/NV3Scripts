using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonGhost : MonoBehaviour
{
	private IEnumerable<MeshRenderer> renderers;
	private IEnumerable<SkinnedMeshRenderer> smrenderers;
	[SerializeField] private int animaterate = 1;

	void Start()
	{
		renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
		smrenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		StartCoroutine(Animate());
    }

	IEnumerator Animate()
	{
		float alphavalue = 0;
		while (alphavalue < 1)
		{
			alphavalue += Time.deltaTime * animaterate;
			SetRendererAlphaClip(alphavalue);
			SetSMRendererAlphaClip(alphavalue);
			yield return new WaitForSeconds(Time.deltaTime);
		}
		Destroy(gameObject);
	}



	private void SetRendererAlphaClip(float alphavalue)
	{
		foreach (MeshRenderer rend in renderers)
		{
			rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}

	private void SetSMRendererAlphaClip(float alphavalue)
	{
		foreach (SkinnedMeshRenderer rend in smrenderers)
		{
			rend.material.SetFloat("_Alpha_Clip_Level", alphavalue);
		}
	}
}
