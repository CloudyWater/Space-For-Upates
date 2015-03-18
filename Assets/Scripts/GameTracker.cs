using UnityEngine;
using System.Collections.Generic;

public class GameTracker : MonoBehaviour {

	public const int STARTING_POINTS = 500;

	public enum Team
	{
		TeamOne, TeamTwo, Neutral
	}
	
	public static List<Ship> mTeamOnePlayers;
	public static List<Ship> mTeamTwoPlayers;
	
	public static List<CapturePoint> mCapturePoints;
	
	public int mTeamOnePoints;
	public int mTeamTwoPoints;

	// Use this for initialization
	void Start () 
	{
		mTeamOnePlayers = new List<Ship> ();
		mTeamTwoPlayers = new List<Ship> ();
		mCapturePoints = new List<CapturePoint> ();
		
		mTeamOnePoints = mTeamTwoPoints =  STARTING_POINTS;		
	}
	
	// Update is called once per frame
	void Update () 
	{
		foreach (Ship ship in FindObjectsOfType <Ship> ())
		{
			if (ship.mTeam == Team.TeamOne)
			{
				if (!mTeamOnePlayers.Contains(ship))
				{
					mTeamOnePlayers.Add (ship);
				}
			}
			else if (ship.mTeam == Team.TeamTwo)
			{
				if (!mTeamTwoPlayers.Contains (ship))
				{
					mTeamTwoPlayers.Add (ship);
				}
			}
		}
		
		foreach (CapturePoint point in FindObjectsOfType <CapturePoint> ())
		{
			if (!mCapturePoints.Contains (point))
			{
				mCapturePoints.Add (point);
			}
		}
	}
}
