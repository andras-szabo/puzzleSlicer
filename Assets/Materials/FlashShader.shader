Shader "Custom/FlashShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_Weight ("TintWeight", float) = 0
		_AlphaThreshold ("Threshold", float) = 0.2
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Tint;
			float _Weight;
			float _AlphaThreshold;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				if (col.w > _AlphaThreshold)
				{
					return lerp(col, _Tint, _Weight);
				}

				return fixed4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
}
