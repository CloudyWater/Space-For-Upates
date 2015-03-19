using UnityEngine;
using System.Collections;

public class AIControlledShip : Ship {

	// Ship Classes will be stored in a folder in the same directory as the
	// game executable. These are the folder locations.
	
	private const float MAX_VELOCITY = 10f;
	private const float ACCELERATION = 2f;
	private const float ROTATION = 2f;
	
	private Ship mCurrentTarget;
	
	// Use this for initialization
	void Start () 
	{
		foreach (Emplacement turret in mTurretLocations)
		{
			Turret tScript = turret.GetTurret ();
			tScript.UseTargetPosition(true);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		float mDistanceToTarget, mClosestTarget = float.MaxValue;
		bool bEnginesFiring = false;
		
		if (mCurrentTarget != null)
		{		
			foreach (Emplacement turret in mTurretLocations)
			{
				Turret turretScript = turret.GetTurret ();
				Vector3 targetPosition = GetAimingPoint (mCurrentTarget, turretScript.GetBulletSpeed ());
				turretScript.SetTargetPosition (targetPosition);
//				Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, targetPosition - turret.transform.position);
//				turret.transform.rotation = newRotation;
			}
		}
		
		Ship[] ships;
		
		if (mTeam.Equals (GameTracker.Team.TeamOne))
		{
			ships = GameTracker.mTeamTwoPlayers.ToArray ();
		}
		else
		{
			ships = GameTracker.mTeamOnePlayers.ToArray ();
		}
		
		foreach (Ship ship in ships)
		{	
			if (!ship.Equals (this))
			{
				Transform targetTransform = ship.gameObject.transform;
				mDistanceToTarget = Vector3.Distance (transform.position, targetTransform.position);
				
				if (mDistanceToTarget < mClosestTarget)
				{
					mClosestTarget = mDistanceToTarget;
					mCurrentTarget = ship;
				}
			}
		}
		
		foreach (Emplacement turret in mTurretLocations)
		{
			Turret script = turret.GetTurret ();
			script.SetTarget (mCurrentTarget.gameObject);
			if (script.IsTargetInRange ())
			{
				script.Fire (GetComponent<Rigidbody2D>().velocity);
			}
		}
		
		foreach (EngineBehavior behavior in mEngines)
		{
			behavior.SetActive (bEnginesFiring);
		}
	}
	
	
}
