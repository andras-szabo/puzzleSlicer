Shader "Custom/OutlinePrePass"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1, 1, 1, 1)
		_AlphaThreshold("Threshold", float) = 0.2
		_AlphaDiv("Foo", float) = 2.0
		padding("Padding", float) = 0.02
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
				float2 vertexInObjectSpace : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Tint;
			float _AlphaThreshold;
			float _AlphaDiv;
			float padding;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertexInObjectSpace = float2(v.vertex.x, v.vertex.y);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				// step(edge, x)    => if edge >= x : 0
				//					=> if edge < x  : 1

				float isRight = step(0.5, i.vertex.x);
				float isTop = step(0.5, i.vertex.y);
				float isLeft = 1.0 - isRight;
				float isBottom = 1.0 - isTop;

				fixed4 up = tex2D(_MainTex, i.uv + float2(0, padding));
				fixed4 down = tex2D(_MainTex, i.uv + float2(0, -padding));
				fixed4 left = tex2D(_MainTex, i.uv + float2(-padding, 0));
				fixed4 right = tex2D(_MainTex, i.uv + float2(padding, 0));

				fixed4 upLeft = tex2D(_MainTex, i.uv + float2(-padding, padding));
				fixed4 upRight = tex2D(_MainTex, i.uv + float2(padding, padding));
				fixed4 downRight = tex2D(_MainTex, i.uv + float2(padding, -padding));
				fixed4 downLeft = tex2D(_MainTex, i.uv + float2(-padding, -padding));

				fixed4 all = up * isBottom +
					down * isTop +
					left * isRight +
					right * isLeft +
					upLeft * (isBottom + isRight) +
					upRight * (isBottom + isLeft) +
					downRight * (isTop + isLeft) +
					downLeft * (isTop + isRight);

				float alpha = all.w / _AlphaDiv;

				// But how to smoothen jagged edges?

				if (alpha > _AlphaThreshold)
				{
					return _Tint;
				}

				return fixed4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
}
