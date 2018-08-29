Shader "Custom/FlatColour"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_AlphaThreshold("Threshold", float) = 0.2
		_BorderThreshold("BorderThreshold", float) = 0.05
	}

		SubShader
		{

		Tags{ "RenderType" = "Transparent" }
			LOD 100
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 worldPosition : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Tint;
			float _AlphaThreshold;
			float _BorderThreshold;
			// float4 _ClipRect;

			v2f vert(appdata v)
			{
				v2f o;
				o.worldPosition = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				// step(edge, x)    => if edge >= x : 0
				//					=> if edge < x  : 1

				float visible = step(_AlphaThreshold, col.w);

				// highlight if: not visible but has high red
				float isHighlight = step(_BorderThreshold, col.x) * (1 - visible);

				return _Tint * (visible + isHighlight);

				// If clipping were allowed:
				// fixed4 color = _Tint * (visible + isHighlight);
				// color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				// return color;
		}
		ENDCG
		}
		}
}
