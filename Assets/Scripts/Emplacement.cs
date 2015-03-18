using UnityEngine;
using System.Collections;

public class Emplacement : MonoBehaviour {

	public Ship mConnectedShip;
	public Turret.TurretType mEmplacementType;
	public GameObject mEmplacementTurretPrefab;
	public float mPrimaryRotation;
	public float mRotationRadius;
	public bool mbIsStationary;
	
	private GameObject mEmplacementTurret;
	private Turret mEmplacementTurretScript;
	private float mMaxRotation, mMinRotation;
	
	// Use this for initialization
	void Start () 
	{
		mEmplacementTurret = (GameObject) Instantiate (mEmplacementTurretPrefab);
		mEmplacementTurret.gameObject.transform.parent = this.transform;
		mEmplacementTurret.transform.localPosition = new Vector3 (0, 0, 0);
		mEmplacementTurretScript = mEmplacementTurret.GetComponent <Turret> ();
		mEmplacementTurretScript.mTeam = mConnectedShip.mTeam;
		mEmplacementTurretScript.SetEmplacement (this);
		
		mMaxRotation = mPrimaryRotation + mRotationRadius / 2;
		mMinRotation = mPrimaryRotation - mRotationRadius / 2;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	
	public Turret GetTurret ()
	{
		return mEmplacementTurretScript;
	}
	
	public float GetMaxRotation ()
	{
		return mMaxRotation;
	}
	
	public float GetMinRotation ()
	{
		return mMinRotation;
	}
	
	public bool mbIsTurretCompatible (Turret turret)
	{
		bool bReturn = false;
		
		if (turret.mType == Turret.TurretType.Light)
		{
			bReturn = true;	
		}
		else if (turret.mType == Turret.TurretType.Heavy )
		{
			if (mEmplacementType == Turret.TurretType.Heavy || mEmplacementType == Turret.TurretType.WorldKiller)
			{
				bReturn = true;
			}
		}
		else if (turret.mType == Turret.TurretType.WorldKiller)
		{
			if (mEmplacementType == Turret.TurretType.WorldKiller)
			{
				bReturn = true;
			}
		}
		
		return bReturn;
	}
}
