Shader "FX/Directional Shield" {
    Properties {
        _Color ("Tint (RGBA)", Color) = (1,1,1,1)
        _MainTex ("Texture (RGB)", 2D) = "white" {}
    }
    SubShader {
        ZWrite Off
        Tags { "Queue" = "Transparent" }
        Blend One One
        Cull Off
 
        Pass {
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_fog_exp2
 
            struct shieldHitInfo
			{
				float3 m_hitPos;
				float m_radialFactor;
			};
 
            uniform StructuredBuffer<shieldHitInfo> buffer;
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float color:COLOR0;
            };
 
            uniform float _Offset;
            uniform float _RadialFactor;
            uniform float4 _Position;
            uniform float4 _Color;
            uniform sampler2D _MainTex : register(s1);         
 
          	uniform uint _hitCount;
 
            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                v.texcoord.x = v.texcoord.x;
                v.texcoord.y = v.texcoord.y + _Offset;
                o.uv = TRANSFORM_UV (1);
                o.normal = v.normal;
 
                float brightness = 0.0;
                for(uint i = 0; i < _hitCount; i++)
                {
             		brightness += buffer[i].m_radialFactor/distance (v.vertex,buffer[i].m_hitPos);
              	}
               o.color = brightness + .4,0.0,1.0;
               if (o.color < 0.05)
                       o.color = 0.0;                       
 
                return o;
            }
 
            float4 frag (v2f f) : COLOR
            {
                half4 tex = tex2D (_MainTex, f.uv)*f.color*_Color*_Color.a ;
                return half4 (tex.r, tex.g, tex.b, tex.a);
            }
            ENDCG
 
        }
    }
    Fallback "Transparent/VertexLit"
}