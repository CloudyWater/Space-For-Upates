using UnityEngine;
using System.Collections;

public class Minion : AIControlledShip 
{

	private Ship mTargetedShip;
	private CapturePoint mTargetedPoint;
	private const float AGGRO_RANGE = 10.0f;
	private const float MAX_VELOCITY = 8f;
	private const float ACCELERATION = 12f;

	// Use this for initialization
	void Start () 
	{
		mTargetedShip = null;
		mTargetedPoint = null;
		mStrafingDirection = StrafeDirection.None;
	}
	
	// Update is called once per frame
	void Update ()
	{
		float currentDist, minDist = float.MaxValue;
		Vector3 mTargetLocation = Vector3.zero;
		bool bEnginesFiring = false;
		
		if (mTargetedShip == null)
		{
			if (mTeam == GameTracker.Team.TeamOne)
			{
				foreach (Ship ship in GameTracker.mTeamTwoPlayers)
				{
					currentDist = Vector3.Distance (ship.gameObject.transform.position, transform.position);
					if (currentDist < minDist)
					{
						minDist = currentDist;
						mTargetedShip = ship;
					}
				}
				if (mTargetedShip != null && Vector3.Distance (mTargetedShip.gameObject.transform.position, transform.position) > AGGRO_RANGE)
				{
					mTargetedShip = null;
				}
			}
			else
			{
				foreach (Ship ship in GameTracker.mTeamOnePlayers)
				{
					currentDist = Vector3.Distance (ship.gameObject.transform.position, transform.position);
					if (currentDist < minDist)
					{
						minDist = currentDist;
						mTargetedShip = ship;
					}
				}
				if (mTargetedShip != null && Vector3.Distance (mTargetedShip.gameObject.transform.position, transform.position) > AGGRO_RANGE)
				{
					mTargetedShip = null;
				}
			}
		}
		
		if (mTargetedShip != null)
		{
			mTargetLocation = GetAimingPoint (mTargetedShip, mTurretLocations[0].GetTurret ().GetBulletSpeed ());
			Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, mTargetLocation - transform.position);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, newRotation, 250 * Time.deltaTime);
		}
		else if (mTargetedPoint == null)
		{
			minDist = float.MaxValue;
			foreach (CapturePoint point in GameTracker.mCapturePoints)
			{
				if (point.GetOwningTeam () != mTeam)
				{
					currentDist = Vector3.Distance (transform.position, point.transform.position);
					if (currentDist < minDist)
					{
						minDist = currentDist;
						mTargetedPoint = point;
					}
				}
			}
			
			//if our team owns all points
			if (mTargetedPoint == null)
			{
				int capturePoint = Random.Range (0, GameTracker.mCapturePoints.Count - 1);
				mTargetedPoint = GameTracker.mCapturePoints[capturePoint];
				mTargetLocation = mTargetedPoint.transform.position;
				Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, mTargetLocation - transform.position);
				transform.rotation = Quaternion.RotateTowards (transform.rotation, newRotation, 250 * Time.deltaTime);
			}
		}
		else
		{
			mTargetLocation = mTargetedPoint.transform.position;
			Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, mTargetLocation - transform.position);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, newRotation, 250 * Time.deltaTime);
		}
		
		if (!mTargetLocation.Equals (Vector3.zero) && Vector3.Distance (mTargetLocation, transform.position) > mTurretLocations[0].GetTurret ().GetBulletRange ())
		{
			GetComponent<Rigidbody2D>().AddForce (transform.up * ACCELERATION);
			if (GetComponent<Rigidbody2D>().velocity.magnitude > MAX_VELOCITY)
			{
				GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude (GetComponent<Rigidbody2D>().velocity, MAX_VELOCITY);
			}
			bEnginesFiring = true;
			mbStrafing = false;
		}
		else
		{
//			rigidbody2D.velocity = Vector3.ClampMagnitude (rigidbody2D.velocity, rigidbody2D.velocity.magnitude - (MAX_VELOCITY * Time.deltaTime));
//			if (rigidbody2D.velocity.magnitude / MAX_VELOCITY < .05)
//			{
//				rigidbody2D.velocity = Vector3.zero;
//			}
			mbStrafing = true;
			if (mStrafingDirection.Equals (StrafeDirection.None))
			{
				mStrafingDirection = (StrafeDirection) Random.Range (0,1);
			}
			Quaternion rotation;
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
		
		foreach (Emplacement turret in mTurretLocations)
		{
			Turret tScript = turret.GetTurret ();
			if (!mTargetLocation.Equals (Vector3.zero) && Vector3.Distance (mTargetLocation, transform.position) < tScript.GetBulletRange () && mTargetedShip != null)
			{
				tScript.Fire (GetComponent<Rigidbody2D>().velocity);
			}
		}
		
		foreach (EngineBehavior behavior in mEngines)
		{
			behavior.SetActive (bEnginesFiring);
		}
	}
}
