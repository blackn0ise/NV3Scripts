using EZCameraShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singularity : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject body = default;
    [SerializeField] private AudioClip spawnClip = default;

    [Header("Parameters")]
    [SerializeField] private float speedFactor = 3000;

    public static bool isSingularityAwake = false;
    private GameObject player;

    private void Start()
    {
        AwakenSingularity();
    }

    private void Update()
    {
        Pull();
    }

    private void AwakenSingularity()
    {
        isSingularityAwake = true;
        player = GameManagerScript.GetGMScript().GetPlayer();
        player.GetComponent<AudioSource>().PlayOneShot(spawnClip);
        CameraShaker.Instance.ShakeOnce(5, 5, 1.2f, 2.0f);
        transform.LookAt(player.transform);
    }

    private void Pull()
	{
		if (SoulCollector.soulCollectorActive)
		{
            Transform soulcollector = FindObjectOfType<SoulCollector>().transform;
            Transform[] tempchildren = soulcollector.GetComponentsInChildren<Transform>();
            List<GameObject> scchildren = new List<GameObject>();
			foreach (Transform child in tempchildren)
			{
				scchildren.Add(child.gameObject);
			}
			foreach (GameObject go in Unit.GetAllEnemiesExcluding(player, scchildren))
            {
                if (go.GetComponent<IUnit>() != null && !go.GetComponent<Weakpoint>())
                {
                    float massivefactor = go.layer == LayerMask.NameToLayer("Massive") ? 0.1f : 1;
                    float distanceFactor = (speedFactor / Vector3.Distance(go.transform.position, body.transform.position));
                    go.transform.position = Vector3.MoveTowards(go.transform.position, body.transform.position, distanceFactor * Time.deltaTime * massivefactor);
                }
            }

        }
        else
        {
            foreach (GameObject go in Unit.GetAllEnemies(player))
            {
                if (go.GetComponent<IUnit>() != null && !go.GetComponent<Weakpoint>())
                {
                    float massivefactor = go.layer == LayerMask.NameToLayer("Massive") ? 0.3f : 1;
                    float distanceFactor = (speedFactor / Vector3.Distance(go.transform.position, body.transform.position));
                    go.transform.position = Vector3.MoveTowards(go.transform.position, body.transform.position, distanceFactor * Time.deltaTime * massivefactor);

                }
            }
        }
    }

    public void DestroySelf()
    {
        isSingularityAwake = false;
        Destroy(gameObject);
    }
}
