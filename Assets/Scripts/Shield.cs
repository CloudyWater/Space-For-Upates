using UnityEngine;

public class Shield : MonoBehaviour
{
	
	public float EffectTime;
	public Ship mConnectedShip;
	
	public void Start ()
	{
		renderer.sortingLayerName = "Shields";
	}
	
	public void Update ()
	{
		if(EffectTime>0){
			if(EffectTime < 450 && EffectTime > 400)
			{
				renderer.sharedMaterial.SetVector("_ShieldColor", new Vector4(0.7f, 1f, 1f, 0f) );
			}        
			
			EffectTime-=Time.deltaTime * 1000;
			
			renderer.material.SetFloat("_EffectTime", EffectTime);
		}
		transform.localPosition = new Vector3 (0,0,0);
	}
	
	public void OnTriggerEnter2D (Collider2D collider) {
		
		if (collider.tag.Equals ("Bullet"))
		{
			Projectile bullet = collider.gameObject.GetComponent <Projectile> ();
			if (bullet.mTeam != mConnectedShip.mTeam)
			{
				renderer.material.SetVector("_ShieldColor", new Vector4(0.7f, 1f, 1f, 0.05f));

				renderer.material.SetVector("_Position", transform.InverseTransformPoint (collider.gameObject.transform.position));
				
				EffectTime=500;
				
				Destroy (collider.gameObject);
			}
		}
	}
}