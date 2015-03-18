using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CapturePoint : MonoBehaviour {

	private const float CAPTURE_POINTS = 100;

	private class ShipConnection
	{
		public Ship mShip;
		public LineRenderer mRenderer;
		
		public ShipConnection (Ship ship)
		{
			mShip = ship;
		}
		
		public void Update (Vector3 stationPosition)
		{
			
		}
	}

	private List<ShipConnection> mNearShips;
	
	private GameTracker.Team mOwningTeam;
	private GameTracker.Team mCapturingTeam;
	
	private float mCaptureProgress;
	
	
	// Use this for initialization
	void Start () 
	{
		mNearShips = new List<ShipConnection> ();
		mOwningTeam = GameTracker.Team.Neutral;
	}
	
	// Update is called once per frame
	void Update () 
	{
		int teamOneShips = 0, teamTwoShips = 0; 
		foreach (ShipConnection connection in mNearShips)
		{
			if (connection.mShip.mTeam.Equals (GameTracker.Team.TeamOne))
			{
				teamOneShips += (int)connection.mShip.mShipStats.mShipClass;
			}
			else if (connection.mShip.mTeam.Equals (GameTracker.Team.TeamTwo))
			{
				teamTwoShips += (int)connection.mShip.mShipStats.mShipClass;
			}
		}
		
		if (teamOneShips > teamTwoShips)
		{
			mCapturingTeam = GameTracker.Team.TeamOne;
		}
		else if (teamTwoShips > teamOneShips)
		{
			mCapturingTeam = GameTracker.Team.TeamTwo;
		}
		else
		{
			mCapturingTeam = GameTracker.Team.Neutral;
		}
		
		if (mOwningTeam.Equals (GameTracker.Team.Neutral))
		{
			if (mCapturingTeam.Equals (GameTracker.Team.TeamOne))
			{
				mCaptureProgress += (teamOneShips - teamTwoShips) * Time.deltaTime;
				
				if (mCaptureProgress >= CAPTURE_POINTS)
				{
					mOwningTeam = GameTracker.Team.TeamOne;
					mCaptureProgress = CAPTURE_POINTS;
				}
			}
			else if (mCapturingTeam.Equals (GameTracker.Team.TeamTwo))
			{
				mCaptureProgress += (teamTwoShips - teamOneShips) * Time.deltaTime;
				
				if (mCaptureProgress >= CAPTURE_POINTS)
				{
					mOwningTeam = GameTracker.Team.TeamTwo;
					mCaptureProgress = CAPTURE_POINTS;
				}
			}
		}
		else if (mOwningTeam.Equals (GameTracker.Team.TeamOne))
		{
			if (mCapturingTeam.Equals (GameTracker.Team.TeamOne))
			{
				mCaptureProgress += (teamOneShips - teamTwoShips) * Time.deltaTime;
				
				if (mCaptureProgress >= CAPTURE_POINTS)
				{
					mCaptureProgress = CAPTURE_POINTS;
				}
			}
			else if (mCapturingTeam.Equals (GameTracker.Team.TeamTwo))
			{
				mCaptureProgress -= (teamTwoShips - teamOneShips) * Time.deltaTime;
				
				if (mCaptureProgress <= 0)
				{
					mCaptureProgress = 0;
					mOwningTeam = GameTracker.Team.Neutral;
				}
			}
		}
		else if (mOwningTeam.Equals (GameTracker.Team.TeamTwo))
		{
			if (mCapturingTeam.Equals (GameTracker.Team.TeamOne))
			{
				mCaptureProgress -= (teamOneShips - teamTwoShips) * Time.deltaTime;
				
				if (mCaptureProgress <= 0)
				{
					mCaptureProgress = 0;
					mOwningTeam = GameTracker.Team.Neutral;
				}
			}
			else if (mCapturingTeam.Equals (GameTracker.Team.TeamTwo))
			{
				mCaptureProgress += (teamTwoShips - teamOneShips) * Time.deltaTime;
				
				if (mCaptureProgress >= CAPTURE_POINTS)
				{
					mCaptureProgress = CAPTURE_POINTS;
				}
			}
		}
	}	
	
	void OnTriggerEnter2D (Collider2D collider)
	{
		Ship enteringShip = collider.GetComponent <Ship> ();
		
		if (enteringShip != null)
		{
			ShipConnection connection = new ShipConnection (enteringShip);
			mNearShips.Add (connection);
		}
	}
	
	void OnTriggerExit2D (Collider2D collider)
	{
		Ship exitingShip = collider.GetComponent <Ship> ();
		
		if (exitingShip != null)
		{
			foreach (ShipConnection connection in mNearShips)
			{
				if (connection.mShip.Equals (exitingShip))
				{
					mNearShips.Remove (connection);
					break;
				}
			}
		}
	}
	
	public float GetPercentageCaptured ()
	{
		return mCaptureProgress / CAPTURE_POINTS;
	}
	
	public GameTracker.Team GetDisplayTeam ()
	{
		GameTracker.Team team;
		if (mOwningTeam.Equals (GameTracker.Team.Neutral))
		{
			team = mCapturingTeam;
		}
		else
		{
			team = mOwningTeam;
		}
		
		return team;
	}
	
	public GameTracker.Team GetOwningTeam ()
	{
		return mOwningTeam;
	}
}
