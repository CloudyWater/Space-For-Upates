using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//TODO: Set up rotation speed! IMPORTANT!

public class Turret : MonoBehaviour {

	public enum TurretType
	{
		Light, Heavy, WorldKiller
	}

	public float mFireDelay;
	public float mMaxHeat;
	public float mHeatPerShot;
	public float mCoolingRate;
	public float mTurningSpeed;
	public TurretType mType;
	
	public GameTracker.Team mTeam;

	public GameObject[] mFiringPosition;
	public GameObject mBulletPrefab;
	public Animator mAnimator;
	public Image mHeatingUI;
	public Color mCoolColor, mHotColor;
	
	public GameObject mCurrentTarget;
	public Vector3 mCurrentTargetPosition;
	public bool mbUseTargetPosition;
	
	public AudioClip mFiringSound;
	
	private float mFiringTimer;
	private float mCurrentHeat;
	
	private bool mbIsOverheating;
	
	private Emplacement mEmplacement;
	
	// Use this for initialization
	void Start () 
	{
		mCurrentHeat = 0f;
		mbIsOverheating = false;
		mCurrentTarget = null;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		if (!mbUseTargetPosition)
		{
			if (mCurrentTarget != null)
			{
				mCurrentTargetPosition = mCurrentTarget.transform.position;
			}
		}
		
		//only rotate if we can rotate!
		if (!mEmplacement.mbIsStationary)
		{
			Quaternion newRotation = Quaternion.FromToRotation (Vector3.up, mCurrentTargetPosition - transform.position);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, newRotation, 200 * Time.deltaTime);
		}
		
		if (transform.rotation.eulerAngles.z < mEmplacement.GetMaxRotation () && transform.rotation.eulerAngles.z > mEmplacement.GetMinRotation ())
		{
			//This is the acceptable area
		}
		else if (transform.rotation.eulerAngles.z > mEmplacement.GetMaxRotation ())
		{
			//Need to constrain rotation to Max Rotation
		}
		else if (transform.rotation.eulerAngles.z < mEmplacement.GetMinRotation ())
		{
			//Need to constrain rotation to Min Rotation
		}			
								
		if (mbIsOverheating)
		{
			mCurrentHeat -= mCoolingRate * 2 * Time.deltaTime;
			
			if (mCurrentHeat <= 0)
			{
				mCurrentHeat = 0;
				mbIsOverheating = false;
			}
		}
		else if (mFiringTimer > 0)
		{
			mFiringTimer -= Time.deltaTime;
			if (mFiringTimer < 0)
			{
				mFiringTimer = 0;
			}
		}
		
		if (mCurrentHeat > 0 && !mbIsOverheating)
		{
			mCurrentHeat -= mCoolingRate * Time.deltaTime;
			if (mCurrentHeat < 0)
			{
				mCurrentHeat = 0;
			}
		}
		
		//Handle the Overheat UI
		mHeatingUI.fillAmount = mCurrentHeat / mMaxHeat;
		mHeatingUI.color = Color.Lerp (mCoolColor, mHotColor, mHeatingUI.fillAmount);	
		
	}
	
	public void SetEmplacement (Emplacement emplacement)
	{
		mEmplacement = emplacement;
		transform.eulerAngles = new Vector3 (0,0,mEmplacement.mPrimaryRotation);
	}
	
	public void Fire (Vector2 shipVelocity)
	{
		if (mFiringTimer == 0)
		{
			mFiringTimer = mFireDelay;
			
			foreach (GameObject firingPosition in mFiringPosition)
			{
				GameObject bullet = (GameObject) Instantiate (mBulletPrefab);
				Projectile bulletScript = bullet.GetComponent <Projectile> ();
				bullet.transform.position = firingPosition.transform.position;
				bulletScript.Fire (transform.eulerAngles.z, firingPosition.transform.position, shipVelocity, mTeam);
			}
			
			mAnimator.SetTrigger ("Fire");
			
			mCurrentHeat += mHeatPerShot;
			
			if (mCurrentHeat >= mMaxHeat)
			{
				mbIsOverheating = true;
			}
			
			GetComponent<AudioSource>().PlayOneShot (mFiringSound);
		}
	}
	
	public bool IsTargetInRange ()
	{
		bool bReturn = true;
		Projectile bullet = mBulletPrefab.GetComponent <Projectile> ();
		float range = bullet.mRange;
		
		if (mCurrentTarget != null)
		{
			if (Vector3.Distance (mCurrentTarget.transform.position, transform.position) > range)
			{
				bReturn = false;
			}
		}
		else
		{
			bReturn = false;
		}
		
		return bReturn;
	}
	
	public void UseTargetPosition (bool bUse)
	{
		mbUseTargetPosition = bUse;
	}
	
	public void SetTargetPosition (Vector3 position)
	{
		mCurrentTargetPosition = position;
	}
	
	public GameObject GetTarget ()
	{
		return mCurrentTarget;
	}
	
	public void SetTarget (GameObject target)
	{
		mCurrentTarget = target;
	}
	
	public float GetBulletSpeed ()
	{
		Projectile bullet = mBulletPrefab.GetComponent <Projectile> ();
		return bullet.mProjectileSpeed;
	}
	
	public float GetBulletRange ()
	{
		Projectile bullet = mBulletPrefab.GetComponent <Projectile> ();
		return bullet.mRange;
	}
}
