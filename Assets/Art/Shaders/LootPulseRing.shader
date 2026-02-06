Shader "VSL/LootPulseRing"
{
	Properties
	{
		_Color("Color", Color) = (0.2, 0.85, 1.0, 1.0)
		_Progress("Progress", Range(0,1)) = 0
		_Thickness("Thickness", Range(0,0.5)) = 0.08
		_Softness("Softness", Range(0,0.5)) = 0.05
		_Intensity("Intensity", Range(0,5)) = 1.2
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		Blend One One
		Cull Off
		ZWrite Off

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
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			fixed4 _Color;
			float _Progress;
			float _Thickness;
			float _Softness;
			float _Intensity;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 p = i.uv * 2.0 - 1.0;
				float dist = length(p);
				float radius = lerp(0.0, 1.0, _Progress);
				float d = abs(dist - radius);
				float ring = saturate(1.0 - smoothstep(_Thickness, _Thickness + _Softness, d));
				float fade = saturate(1.0 - _Progress);
				float a = ring * fade;
				return _Color * (a * _Intensity);
			}
			ENDCG
		}
	}
}

