 Shader "Custom/MultiShieldEffect" {
     Properties {
     	 _MainTex ("Texture", 2D) = "white" {}
         _Position ("Collision", Vector) = (-1, -1, -1, -1)        
         _MaxDistance ("Effect Size", float) = .005        
         _ShieldColor ("Color (RGBA)", Color) = (0.7, 1, 1, 0)
         _EmissionColor ("Emission color (RGBA)", Color) = (0.7, 1, 1, 0.01)        
         _EffectTime ("Effect Time (ms)", float) = 0
     }
     
     SubShader {
         Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
         LOD 2000
         Cull Off
       
         CGPROGRAM
           #pragma surface surf Lambert vertex:vert alpha
           #pragma target 3.0
       
          struct Input 
          {
              float2 uv_MainTex;
           };
       
           float4 _Position[10];
           float4x4 _CustomDistances;          
           float _MaxDistance;          
           float4 _ShieldColor;
           float4 _EmissionColor;          
           float4x4 _EffectTime;
           uint _CurrentHits;
           
           float _Amount;
           
           sampler2D _MainTex;
       
           void vert (inout appdata_full v, out Input o) 
           {
           	   UNITY_INITIALIZE_OUTPUT(Input, o);
           	   for (uint i = 0; i < _CurrentHits; i++)
               {
               		_CustomDistances = distance(_Position[i].xyz, v.vertex.xyz);
           	   }
           }
       
           void surf (Input IN, inout SurfaceOutput o) {
            
             o.Albedo = tex2D (_MainTex, IN.uv_MainTex);
             o.Emission = _EmissionColor;
             
             for (uint i = 0; i < _CurrentHits; i++)
             {
             
	             if(_EffectTime > 0)
	             {
	                 if(_CustomDistances < _MaxDistance)
	                 {
	                 	o.Alpha += _EffectTime/500 - (_CustomDistances / _MaxDistance) + _ShieldColor.a;
	                  	
	                  	if(o.Alpha < _ShieldColor.a)
	                  	{
	                           o.Alpha = _ShieldColor.a;
	                	}
	                }
	               	else 
	               	{
	                 	o.Alpha = _ShieldColor.a;
	               	}
	               }
	               else
	               {
	                   o.Alpha = o.Alpha = _ShieldColor.a;
	               }
               }
           }
       
           ENDCG
     } 
     Fallback "Transparent/Diffuse"
 }