using UnityEngine;
using System.Collections;

public class EngineBehavior : MonoBehaviour {

	public ParticleSystem mFlare;
	public ParticleSystem mSmoke;
	
	public GameObject mShip;

	private bool mbEngaged;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	public void SetActive (bool bEngaged)
	{
		mbEngaged = bEngaged;
	
		mFlare.enableEmission = mbEngaged;
		mSmoke.enableEmission = mbEngaged;
	}
}
