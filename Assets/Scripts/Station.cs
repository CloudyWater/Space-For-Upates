using UnityEngine;
using System.Collections;

public class Station : AIControlledShip {

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
		Ship target;
		Turret turretScript;
		float distance, minDistance;
		
		
		foreach (Emplacement turret in mTurretLocations)
		{
			target = null;
			turretScript = turret.GetTurret ();
			Projectile turretShot = turretScript.mBulletPrefab.GetComponent <Projectile> ();
			minDistance = turretShot.mRange;
			
			//If we already have a target in range, no need to bother.
			if (turretScript.IsTargetInRange ())
			{
				target = turretScript.GetTarget ().GetComponent <Ship> ();
//				Vector3 targetPosition = GetAimingPoint (target, turretScript.GetBulletSpeed ());
//				Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, targetPosition - turret.transform.position);
//				turret.transform.rotation = newRotation;
				turretScript.Fire (GetComponent<Rigidbody2D>().velocity);	
			}
			else
			{	
				//Select target ship for individual turret:
				if (mTeam == GameTracker.Team.TeamOne)
				{
					foreach (Ship ship in GameTracker.mTeamTwoPlayers)
					{
						distance = Vector3.Distance (ship.GetPosition (), turret.transform.position);
						if (distance < minDistance)
						{
							target = ship;
							minDistance = distance;
						}
					}
				}
				else
				{
					foreach (Ship ship in GameTracker.mTeamOnePlayers)
					{
						distance = Vector3.Distance (ship.GetPosition (), turret.transform.position);
						if (distance < minDistance)
						{
							target = ship;
							minDistance = distance;
						}
					}
				}	
				
				//If target is null here no ship was in range.
				if (target != null)
				{
					turretScript.SetTarget (target.gameObject);
//					Vector3 targetPosition = GetAimingPoint (target, turretScript.GetBulletSpeed ());
//					Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, turretScript.GetTarget ().transform.position - turret.transform.position);
//					turret.transform.rotation = newRotation;
					turretScript.Fire (GetComponent<Rigidbody2D>().velocity);
				}
			}
		}
	}
}
