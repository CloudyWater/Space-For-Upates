using UnityEngine;
using System.Collections;

public class WarpGate : Station {

	private const float SPAWN_INTERVAL = 45f;
	private const float MINION_INTERVAL = 1f;
	private const float FIRST_SPAWN_TIMER = 5f;

	public int mMinionsPerWave;
	public GameObject mMinion;
	
	private float mSpawnTimer;
	private float mMinionTimer;
	private bool mbSpawning;
	private bool mbFirstSpawn;
	private int mMinionsSpawned;
	

	// Use this for initialization
	void Start () 
	{
		mbSpawning = false;
		mbFirstSpawn = true;
		mSpawnTimer = 0;
		mMinionTimer = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		GameObject spawnedMinion;
		Ship minionShip;

		mSpawnTimer += Time.deltaTime;
		
		if (!mbSpawning)
		{
			if (mbFirstSpawn)
			{
				if (mSpawnTimer >= FIRST_SPAWN_TIMER)
				{
					mbSpawning = true;
					mSpawnTimer = 0;
					mMinionsSpawned = 0;
					mbFirstSpawn = false;
				}
			}
			else if (mSpawnTimer >= SPAWN_INTERVAL)
			{
				{
					mbSpawning = true;
					mSpawnTimer = 0;
					mMinionsSpawned = 0;
				}
			}
		}
		else
		{
			mMinionTimer += Time.deltaTime;
			if (mMinionTimer >= MINION_INTERVAL)
			{
				spawnedMinion = (GameObject) Instantiate (mMinion);
				minionShip = spawnedMinion.GetComponent <Ship> ();
				minionShip.mTeam = mTeam;
				spawnedMinion.transform.position = transform.position;
				
				mMinionTimer = 0;
				mMinionsSpawned += 1;
			}
			
			if (mMinionsSpawned >= mMinionsPerWave)
			{
				mbSpawning = false;
			}
		}
	}
}
