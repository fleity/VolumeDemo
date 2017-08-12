Shader "Custom/raymarch_simple"
{
	Properties
	{
		_steps("Steps", int) = 128
		_stepSize("Step Size", float) = 1
		_mipLevel("mip Level", int) = -1
		_volumeScale("Volume Scale", float) = 1
		_offset("offset", vector) = (1, 1, 1, 0)
		_threshold("threshold", float) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
		CULL Front		// cull front faces instead of backfaces
		ZTest Always	// always draw this geometry no matter if something is in front of it
		ZWrite Off		// do not write this geometry into the depth buffer

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
				float4 pos : SV_POSITION;	// Clip space
				float3 wPos : TEXCOORD1;	// World position
				// float4 screenPos : TEXCOORD2; // Screen position
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz; // world space position
				// o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}

			int _steps;
			int _mipLevel;
			float _stepSize;
			float _volumeScale;
			sampler3D _volume3d;
			float3 _offset;
			float _threshold;
			
			float4 frag (v2f i) : SV_Target
			{
				float3 p = i.wPos;
				float3 viewDirection = normalize(i.wPos - _WorldSpaceCameraPos);
				float4 c = float4(0, 0, 0, 1);
				for (int i = 0; i < _steps; ++i) // for loop to raymarch
				{
					p += viewDirection * _stepSize; // advance sample position
					c = tex3Dlod(_volume3d, float4((p.xzy / _volumeScale) + _offset, _mipLevel)).rgba; // sample volume texture
					if (c.r + c.g > _threshold) // break the raymarch loop as soon as we found something
						break;
				}
				return c;
			}
			ENDCG
		}
	}
}
