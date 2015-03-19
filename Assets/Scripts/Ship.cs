using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Ship : MonoBehaviour {

	// Ship Classes will be stored in a folder in the same directory as the
	// game executable. These are the folder locations.
	
	protected const string FIGHTER_FILE = "/Ships/Fighters/";
	protected const string FREIGHTER_FILE = "/Ships/Freighters/";
	protected const string FRIGATE_FILE = "/Ships/Frigates";
	protected const string CORVETTE_FILE = "/Ships/Corvettes/";
	protected const string CRUISER_FILE = "/Ships/Cruisers/";
	protected const string BATTLECRUISER_FILE = "/Ships/BattleCruisers/";
	protected const string BATTLESHIP_FILE = "/Ships/BattleShips/";

	public Emplacement[] mTurretLocations;
	public EngineBehavior[] mEngines;
	
	public GameTracker.Team mTeam;
	
	protected int mCurrentHull, mCurrentShields;
	
	public enum ShipClass
	{
		Fighter = 1, Freighter = 2, Frigate = 3, Corvette = 4, Cruiser = 5, BattleCruiser = 6, BattleShip = 7
	}
	
	//ShipStats class holds all the information about a ship.
	public struct ShipStats
	{
		//Local Variables
		public ShipClass mShipClass;
		public string mShipMake;
		public int mMaxHull, mMaxShields;
		public float mShieldChargeTime;
		public float mMaxSpeed, mMaxTurnSpeed, mAcceleration;
		public List<string> mTurrets;
	}
	
	protected enum StrafeDirection
	{
		Left, Right, None
	}
	
	public ShipStats mShipStats;
	
	public string SaveShip ()
	{
		JSONObject ship = new JSONObject(JSONObject.Type.OBJECT);
		ship.AddField ("mShipClass", (int)mShipStats.mShipClass);
		ship.AddField ("mShipMake", mShipStats.mShipMake);
		ship.AddField ("mMaxHull", mShipStats.mMaxHull);
		ship.AddField ("mMaxShields", mShipStats.mMaxShields);
		ship.AddField ("mShieldChargeTime", mShipStats.mShieldChargeTime);
		ship.AddField ("mMaxSpeed", mShipStats.mMaxSpeed);
		ship.AddField ("mMaxTurnSpeed", mShipStats.mMaxTurnSpeed);
		ship.AddField ("mAcceleration", mShipStats.mAcceleration);
		
		for (int i = 0; i < mShipStats.mTurrets.Count; i++)
		{
			ship.AddField ("turret"+i.ToString (), mShipStats.mTurrets[i]);
		}
		
		return ship.Print ();
	}
	
	private const float MAX_VELOCITY = 6f;
	private const float ACCELERATION = 8f;
	private const float ROTATION = 25f;
	private const float MAX_ROT_SPEED = 2000f;
	private const float ANGULAR_DRAG = 10f;
	
	protected StrafeDirection mStrafingDirection;
	protected bool mbStrafing;
	
	// Use this for initialization
	void Start () 
	{		
		GetComponent<Rigidbody2D>().angularDrag = ANGULAR_DRAG;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Quaternion rotation;
		bool bEnginesFiring = false;
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = transform.position.z - Camera.main.transform.position.z;
		mousePosition = Camera.main.ScreenToWorldPoint (mousePosition);
		
		foreach (Emplacement turret in mTurretLocations)
		{
//			Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, mousePosition - turret.transform.position);
//			turret.transform.rotation = newRotation;
			Turret turretScript = turret.GetTurret ();
			turretScript.SetTargetPosition (mousePosition);
			turretScript.UseTargetPosition (true);
		}
		
		if (Input.GetKey (KeyCode.LeftShift))
		{
			mbStrafing = true;
		}
		else
		{
			mbStrafing = false;
		}
		
		if (mbStrafing)
		{
			rotation = Quaternion.FromToRotation (Vector3.up, mousePosition - transform.position);
			
			if (Mathf.Abs (transform.rotation.eulerAngles.z - rotation.eulerAngles.z) < 2)
			{
				GetComponent<Rigidbody2D>().angularVelocity = 0;
			}
			else if (transform.rotation.eulerAngles.z < rotation.eulerAngles.z)
			{
				if (Mathf.Abs (transform.rotation.eulerAngles.z - rotation.eulerAngles.z) < 180)
				{
					GetComponent<Rigidbody2D>().angularVelocity += ROTATION;
					if (GetComponent<Rigidbody2D>().angularVelocity > MAX_ROT_SPEED)
					{
						GetComponent<Rigidbody2D>().angularVelocity = MAX_ROT_SPEED;
					}
				}
				else
				{
					GetComponent<Rigidbody2D>().angularVelocity -= ROTATION;
					if (GetComponent<Rigidbody2D>().angularVelocity < -MAX_ROT_SPEED)
					{
						GetComponent<Rigidbody2D>().angularVelocity = -MAX_ROT_SPEED;
					}
				}					
			}
			else
			{	
				if (Mathf.Abs (transform.rotation.eulerAngles.z - rotation.eulerAngles.z) < 180)
				{
					GetComponent<Rigidbody2D>().angularVelocity -= ROTATION;
					if (GetComponent<Rigidbody2D>().angularVelocity < -MAX_ROT_SPEED)
					{
						GetComponent<Rigidbody2D>().angularVelocity = -MAX_ROT_SPEED;
					}
				}
				else
				{
					GetComponent<Rigidbody2D>().angularVelocity += ROTATION;
					if (GetComponent<Rigidbody2D>().angularVelocity > MAX_ROT_SPEED)
					{
						GetComponent<Rigidbody2D>().angularVelocity = MAX_ROT_SPEED;
					}
				}
			}
			
			if (transform.eulerAngles.z > 360)
			{
				transform.eulerAngles = new Vector3 (0,0,transform.eulerAngles.z - 360);
			}
			else if (transform.eulerAngles.z < 0)
			{
				transform.eulerAngles = new Vector3 (0,0,transform.eulerAngles.z + 360);
			}
		}
		
		if (Input.GetKey (KeyCode.W))
		{
			GetComponent<Rigidbody2D>().AddForce (transform.up * ACCELERATION);
			if (GetComponent<Rigidbody2D>().velocity.magnitude > MAX_VELOCITY)
			{
				GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude (GetComponent<Rigidbody2D>().velocity, MAX_VELOCITY);
			}
			bEnginesFiring = true;
		}
		else if (Input.GetKey (KeyCode.S))
		{			
			GetComponent<Rigidbody2D>().AddForce (transform.up * -ACCELERATION);
			if (GetComponent<Rigidbody2D>().velocity.magnitude > MAX_VELOCITY)
			{
				GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude (GetComponent<Rigidbody2D>().velocity, MAX_VELOCITY);
			}
		}
		
		if (Input.GetKey(KeyCode.A))
		{
			if (!mbStrafing)
			{
				GetComponent<Rigidbody2D>().angularVelocity += ROTATION;
				if (GetComponent<Rigidbody2D>().angularVelocity > MAX_ROT_SPEED)
				{
					GetComponent<Rigidbody2D>().angularVelocity = MAX_ROT_SPEED;
				}
			}
			else
			{
				if (transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270 && mStrafingDirection == StrafeDirection.None)
				{
					mStrafingDirection = StrafeDirection.Right;
				}
				else if (mStrafingDirection == StrafeDirection.None)
				{
					mStrafingDirection = StrafeDirection.Left;
				}				
			}
		}
		else if (Input.GetKey (KeyCode.D))
		{
			if (!mbStrafing)
			{
				GetComponent<Rigidbody2D>().angularVelocity -= ROTATION;
				if (GetComponent<Rigidbody2D>().angularVelocity < -MAX_ROT_SPEED)
				{
					GetComponent<Rigidbody2D>().angularVelocity = -MAX_ROT_SPEED;
				}
				mbStrafing = false;
			}
			else
			{			
				if (transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270 && mStrafingDirection == StrafeDirection.None)
				{
					mStrafingDirection = StrafeDirection.Left;
				}
				else if (mStrafingDirection == StrafeDirection.None)
				{
					mStrafingDirection = StrafeDirection.Right;
				}
			}
		}
		else
		{
			mStrafingDirection = StrafeDirection.None;
		}
		
		if (mbStrafing)
		{
			if (mStrafingDirection == StrafeDirection.Left)
			{
				rotation = Quaternion.Euler (0,0,90);
				Vector3 strafeVector = rotation * transform.up;
				GetComponent<Rigidbody2D>().AddForce (strafeVector * ACCELERATION / 2);
				if (GetComponent<Rigidbody2D>().velocity.magnitude > MAX_VELOCITY)
				{
					GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude (GetComponent<Rigidbody2D>().velocity, MAX_VELOCITY);
				}
			}
			else if (mStrafingDirection == StrafeDirection.Right)
			{
				rotation = Quaternion.Euler (0,0,-90);
				Vector3 strafeVector = rotation * transform.up;
				GetComponent<Rigidbody2D>().AddForce (strafeVector * ACCELERATION / 2);
				if (GetComponent<Rigidbody2D>().velocity.magnitude > MAX_VELOCITY)
				{
					GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude (GetComponent<Rigidbody2D>().velocity, MAX_VELOCITY);
				}
			}
		}
		
		if (Input.GetKey (KeyCode.C))
		{	
			GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude (GetComponent<Rigidbody2D>().velocity, GetComponent<Rigidbody2D>().velocity.magnitude - (MAX_VELOCITY * Time.deltaTime));
			if (GetComponent<Rigidbody2D>().velocity.magnitude / MAX_VELOCITY < .05)
			{
				GetComponent<Rigidbody2D>().velocity = Vector3.zero;
			}
		}
		
		if (Input.GetButton ("Fire1"))
		{
			foreach (Emplacement turret in mTurretLocations)
			{
				Turret script = turret.GetTurret ();
				if (mbStrafing)
				{
					script.Fire (Vector2.zero);
				}
				else
				{
					script.Fire (GetComponent<Rigidbody2D>().velocity);
				}
			}
		}
		
		foreach (EngineBehavior behavior in mEngines)
		{
			behavior.SetActive (bEnginesFiring);
		}
	}
	
	public void LoadShip (ShipClass shipclass, string make)
	{
		
	}
	
	public Vector3 GetPosition()
	{	
		return transform.position;
	}
	
	protected Vector3 GetAimingPoint (Ship target, float bulletSpeed)
	{
		Vector3 aimPoint = Vector3.zero;
		Vector3 quadSolution = Vector3.zero;
		
		float tx, ty, tvx, tvy, a, b, c, t1, t2, t;
		tx = target.gameObject.transform.position.x - transform.position.x;
		ty = target.gameObject.transform.position.y - transform.position.y;
		tvx = target.gameObject.GetComponent<Rigidbody2D>().velocity.x - gameObject.GetComponent<Rigidbody2D>().velocity.x;
		tvy = target.gameObject.GetComponent<Rigidbody2D>().velocity.y - gameObject.GetComponent<Rigidbody2D>().velocity.y;
		
		// get quadratic equation components:
		a = tvx * tvx + tvy * tvy - (bulletSpeed + GetComponent<Rigidbody2D>().velocity.x) * (bulletSpeed + GetComponent<Rigidbody2D>().velocity.y);
		b = 2 * (tvx * tx + tvy * ty);
		c = tx * tx + ty * ty;
		
		quadSolution = Quad (a, b, c);
		
		t1 = quadSolution.x;
		t2 = quadSolution.y;
			
		t = Mathf.Min (t1, t2);
		if (t < 0)
		{
			t = Mathf.Max (t1, t2);
		}
		if (t > 0)
		{
			aimPoint = new Vector3 (target.gameObject.transform.position.x + tvx * t, target.gameObject.transform.position.y + tvy * t, 0);
		}		
		
		return aimPoint;
	}
	
	protected Vector3 Quad (float a, float b, float c)
	{
		Vector3 solution;
		
		float rootSolution = Mathf.Sqrt (b * b - 4 * a * c);
		float root1 = (-b + rootSolution) / (2 * a);
		float root2 = (-b - rootSolution) / (2 * a);
		
		solution = new Vector3 (root1, root2, 0);
		
		return solution;
	}
	
	void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag.Equals ("Bullet"))
		{
			Projectile projectile = collision.GetComponent <Projectile> ();
			if (projectile.mTeam != mTeam)
			{
				Destroy (collision.gameObject);
			}
		}
	}
	
}
