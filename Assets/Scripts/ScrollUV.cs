using UnityEngine;
using System.Collections;

public class ScrollUV : MonoBehaviour {
	
	public GameObject mPlayerShip;
	
	private const int SCROLL_SPEED_MOD = 10;
	
	// Update is called once per frame
	void Update () 
	{
		MeshRenderer mr = GetComponent <MeshRenderer> ();
		
		Material mat = mr.material;
		
		Vector2 offset = mat.mainTextureOffset;
		
		offset.x = transform.position.x / transform.localScale.x / SCROLL_SPEED_MOD;
		offset.y = transform.position.y / transform.localScale.y / SCROLL_SPEED_MOD;
		
		mat.mainTextureOffset = offset;
	}
}
