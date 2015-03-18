using UnityEngine;
using System.Collections;

public class BulletHit : MonoBehaviour {

	public ParticleSystem mParticles;

	// Use this for initialization
	void Start () 
	{
		mParticles.Emit (3);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (mParticles.particleCount == 0)
		{
			Destroy (this.gameObject);
		}
	}
}
