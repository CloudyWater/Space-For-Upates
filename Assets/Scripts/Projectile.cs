using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public float mRange;
	public float mProjectileSpeed;
	public GameTracker.Team mTeam;
	public GameObject mBulletHit;
	private float mFiringAngle;
	private Vector3 mFiringLocation;

	// Use this for initialization
	void Start () 
	{
		mFiringLocation = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Vector3.Distance (mFiringLocation, transform.position) > mRange)
		{
			Destroy (this.gameObject);
		}
	}
	
	void OnTriggerEnter2D (Collider2D collision)
	{
		if (collision.tag.Equals ("Ship") || collision.tag.Equals ("Fighter") || collision.tag.Equals ("Station"))
		{
			Ship connectedShip = collision.GetComponent <Ship> ();
			if (connectedShip.mTeam != mTeam)
			{
				GameObject bulletHit = (GameObject) Instantiate (mBulletHit);
				bulletHit.transform.position = transform.position;
				bulletHit.transform.rotation = Quaternion.Euler (-transform.rotation.eulerAngles.z + 90, 90, bulletHit.transform.rotation.eulerAngles.z);
			}	
		}
		else if (collision.tag.Equals ("Shield"))
		{
			AltShield shield = collision.GetComponent <AltShield> ();
			if (shield.mConnectedShip.mTeam != mTeam)
			{
				GameObject bulletHit = (GameObject) Instantiate (mBulletHit);
				bulletHit.transform.position = transform.position;
				bulletHit.transform.rotation = Quaternion.Euler (-transform.rotation.eulerAngles.z + 90, 90, bulletHit.transform.rotation.eulerAngles.z);
			}	
		}
	}
	
	public void Fire (float angle, Vector3 position, Vector2 shipVelocity, GameTracker.Team team)
	{
		mFiringAngle = angle;
		transform.position = position;
		
		transform.Rotate (Vector3.forward, mFiringAngle);
		GetComponent<Rigidbody2D>().velocity =  (Vector2)(transform.up * mProjectileSpeed) + shipVelocity;
		
		mTeam = team;
	}
}
