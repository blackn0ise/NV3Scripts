using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour
{
	private List<Boid> boidlist;
	public void AddToBoidList(Boid boid) { boidlist.Add(boid); }
	public List<Boid> GetBoidList() { return boidlist; }

	void Start()
	{
		boidlist = new List<Boid>();
	}

	// Update is called once per frame
	void Update()
    {
        
    }


}
