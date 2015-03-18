using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapUI : MonoBehaviour {

	public GameObject mFighterIndicatorPrefab;
	public GameObject mShipIndicatorPrefab;
	public GameObject mStationIndicatorPrefab;
	public Color mTeamOneColor;
	public Color mTeamTwoColor;

	public BoxCollider2D mFieldBounds;
	public GameObject mMapArea;
	public GameTracker mTracker;
	
	struct MapTracker
	{
		public GameObject mImage;
		public Ship mTrackingShip;
		
		public MapTracker (GameObject image, Ship ship)
		{
			mImage = image;
			mTrackingShip = ship;
		}
	}
	
	private ArrayList mMapTrackers;
	
	// Use this for initialization
	void Start () 
	{
		mMapTrackers = new ArrayList ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		bool bShipFound = false;
		
		//Handle addition of new ships to the game
		AddTrackers (mTeamOneColor, GameTracker.mTeamOnePlayers);
		AddTrackers (mTeamTwoColor, GameTracker.mTeamTwoPlayers);
		
		for (int i = 0; i < mMapTrackers.Count; i++)
		{
			MapTracker tracker = (MapTracker) mMapTrackers[i];
			if (tracker.mTrackingShip == null)
			{
				mMapTrackers.RemoveAt(i);
			}
		}
		
		foreach (MapTracker tracker in mMapTrackers)
		{
			Vector3 enemyWorldPos = tracker.mTrackingShip.gameObject.transform.position;	
			float mPercentageWidth = enemyWorldPos.x / mFieldBounds.size.x;	
			float mPercentageHeight = enemyWorldPos.y / mFieldBounds.size.y;	
			RectTransform mapArea = mMapArea.GetComponent <RectTransform> ();
			Vector3 finalPosition = new Vector3 (mPercentageWidth * mapArea.rect.width - mapArea.rect.width / 2, mPercentageHeight * mapArea.rect.height - mapArea.rect.height / 2);
			
			tracker.mImage.gameObject.transform.localPosition = finalPosition;
			tracker.mImage.gameObject.transform.rotation = tracker.mTrackingShip.gameObject.transform.rotation;
		}
		
	}
	
	private void AddTrackers (Color mTeamColor, List<Ship> ships)
	{
		bool shipFound = false;
		
		foreach (Ship ship in ships)
		{
			foreach (MapTracker tracker in mMapTrackers)
			{
				if (ship.Equals (tracker.mTrackingShip))
				{
					shipFound = true;
				}
			}
			if (!shipFound)
			{
				if (ship.gameObject.tag.Equals ("Ship"))
				{
					GameObject image = (GameObject) Instantiate (mShipIndicatorPrefab);
					mMapTrackers.Add (new MapTracker(image, ship));
					image.transform.parent = this.transform;
					SpriteRenderer renderer = image.GetComponent <SpriteRenderer> ();
					renderer.color = mTeamColor;
				}
				else if (ship.gameObject.tag.Equals ("Fighter"))
				{
					GameObject image = (GameObject) Instantiate (mFighterIndicatorPrefab);
					mMapTrackers.Add (new MapTracker(image, ship));
					image.transform.parent = this.transform;
					SpriteRenderer renderer = image.GetComponent <SpriteRenderer> ();
					renderer.color = mTeamColor;
				}
				else if (ship.gameObject.tag.Equals ("Station"))
				{
					GameObject image = (GameObject) Instantiate (mStationIndicatorPrefab);
					mMapTrackers.Add (new MapTracker(image, ship));
					image.transform.parent = this.transform;
					SpriteRenderer renderer = image.GetComponent <SpriteRenderer> ();
					renderer.color = mTeamColor;
				}
			}
			shipFound = false;
		}
	}
}
