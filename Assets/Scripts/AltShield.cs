using UnityEngine;
using System.Collections;
using System.Collections.Generic;

struct shieldHitInfo
{
	public Vector3 m_hitPos;
	public float m_lifeTime;
	public float m_radialFactor;
}

struct bufferData
{
	public Vector3 m_hitPos;
	public float m_radialFactor;
}

public class AltShield : MonoBehaviour
{
	public Color m_shieldColor = Color.magenta;
	public int m_shieldHitLimit = 10;
	public float m_hitDuration = 0.5f;
	public float m_hitPower = 0.2f;
	public Ship mConnectedShip;
	private float m_time = 0.0f;
	private List <shieldHitInfo> m_shieldHits = new List <shieldHitInfo> ();
	//public Material m_material;
	ComputeBuffer buffer;
	
	// Use this for initialization
	void Start ()
	{
		buffer = new ComputeBuffer (m_shieldHitLimit, sizeof(float) * 4, ComputeBufferType.Default);
		renderer.material.SetColor("_Color",m_shieldColor);
		renderer.sortingLayerName = "Shields";
	}
	
	void OnTriggerEnter2D(Collider2D collision)
	{
		//AddShieldHit(transform.InverseTransformPoint(collision.transform));
		if (collision.tag.Equals ("Bullet"))
		{
			Projectile bullet = collision.gameObject.GetComponent <Projectile> ();
			if (bullet.mTeam != mConnectedShip.mTeam)
			{
				if(m_shieldHits.Count < m_shieldHitLimit)
				{
					Vector3 hitpos = transform.InverseTransformPoint(collision.transform.position);
					
					bool found = false;
//					for(int i = 0; i < m_shieldHits.Count; i++)
//					{
//						if((m_shieldHits[i].m_hitPos - hitpos).sqrMagnitude < 0.1f) 
//						{ 
//							//found an effect close by... refresh it.. 
//							shieldHitInfo tmp = m_shieldHits[i];
//							tmp.m_lifeTime = m_hitDuration/2;
//							m_shieldHits[i] = tmp;
//							found = true;
//							break; 
//						}
//					}
					
					if(!found) 
					{ 
						shieldHitInfo newHit = new shieldHitInfo();
						newHit.m_hitPos = hitpos;
						m_shieldHits.Add(newHit);
						newHit.m_lifeTime = m_hitDuration;
						newHit.m_radialFactor = m_hitPower;
						m_shieldHits.Add(newHit);
					} 
				} 
				Destroy (collision.gameObject);
			}
		}
		
	} 
	
	void OnDestroy() 
	{ 
		buffer.Release();
	 } 
	 
	 void Update () 
	 { 
	 	if(Time.deltaTime > 0)
		{
			List<int> remove = new List<int> ();
			for(int i = 0; i < m_shieldHits.Count; i++)
			{
				shieldHitInfo tmp = m_shieldHits[i];
				
				float time = tmp.m_lifeTime;
				time -= Time.deltaTime;
				if(time <= 0) 
				{ 
					remove.Add(i); continue; 
				}
				
				tmp.m_lifeTime = time;
				float radial;

				radial = (time/m_hitDuration) * m_hitPower;

				radial = Mathf.Clamp(radial,0.0f,.2f);
				
				tmp.m_radialFactor = radial;
				m_shieldHits[i] = tmp;
			}
			
			for(int i = remove.Count; i > 0; i--)
			{
				m_shieldHits.RemoveAt(remove[i-1]);
			}
			
			bufferData[] data = new bufferData[m_shieldHits.Count];
			
			for(int i = 0; i < m_shieldHits.Count; i++) 
			{ 
				data[i].m_hitPos = m_shieldHits[i].m_hitPos;
				data[i].m_radialFactor = m_shieldHits[i].m_radialFactor;
			} 
			
			buffer.SetData(data);
			renderer.material.SetBuffer("buffer",buffer);
			m_time += (Time.deltaTime); 
			
			if(m_time > 1.0f)
			{
				m_time = 0;
			}
			
			renderer.material.SetFloat("_Offset", m_time);
			renderer.material.SetFloat("_RadialFactor",m_time);
			renderer.material.SetInt("_hitCount",m_shieldHits.Count);
		}
		
		transform.localPosition = new Vector3 (0,0,0);
	}
}