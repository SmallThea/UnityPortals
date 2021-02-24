Shader "Unlit/PortalShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SecondTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Lighting Off
		Cull Off
		ZWrite On
		ZTest Less
		
		Fog{ Mode Off }

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
				float4 screenPos : TEXCOORD1;
				float3 distFromCam : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.distFromCam = length(ObjSpaceViewDir(v.vertex));
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _SecondTex;

			fixed4 frag (v2f i) : SV_Target
			{
				i.screenPos /= i.screenPos.w;
				fixed4 col = tex2D(_MainTex, float2(i.screenPos.x, i.screenPos.y));
				float xPos = fmod(i.uv.x*0.1 + _Time*0.4, _ScreenParams.x);
				float yPos = fmod(i.uv.y*0.1 + _Time*0.8, _ScreenParams.y);
				fixed4 noise = tex2D(_SecondTex, float2(xPos, yPos));
				col.rgb -= noise * clamp( (i.distFromCam-2)/15 , 0 , 0.6 );
				return col;
			}
			ENDCG
		}
	}
}
