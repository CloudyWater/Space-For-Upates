using UnityEngine;
using System.Collections;

public class CameraTracking : MonoBehaviour {

	public GameObject mTrackingTarget;
	public Camera mCamera;
	
	public float mMouseX;
	public float mMouseY;
	public float mCenterDist;
	
	private float mCameraSize;
	private float mDampening = 0.15f;
	private float mMaxCameraDist;
	
	private const float CAMERA_AREA = .4f;
	private const float MAX_CAMERA_SIZE = 12f;
	private const float MIN_CAMERA_SIZE = 5f;
	
	// Use this for initialization
	void Start () 
	{
		mCameraSize = mCamera.orthographicSize;
		mMaxCameraDist = mCameraSize * CAMERA_AREA;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		Vector3 playerPosition = mTrackingTarget.transform.position;
		
		Vector3 center = Vector3.MoveTowards (playerPosition, mousePosition, mMaxCameraDist);
		
		center.z = transform.position.z;
		
		transform.position = center; 

		mMouseX = mousePosition.x;
		mMouseY = mousePosition.y;
		mCenterDist = Vector3.Distance (center, playerPosition);

		//transform.position = mTrackingTarget.transform.position;
		
		float scroll = Input.GetAxis ("Mouse ScrollWheel");
		mCameraSize += scroll * 5;
		
		if (mCameraSize > MAX_CAMERA_SIZE)
		{
			mCameraSize = MAX_CAMERA_SIZE;
		}
		else if (mCameraSize < MIN_CAMERA_SIZE)
		{
			mCameraSize = MIN_CAMERA_SIZE;
		}
		mMaxCameraDist = mCameraSize * CAMERA_AREA;
		mCamera.orthographicSize = mCameraSize;
	}
}
