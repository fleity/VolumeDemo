Shader "Custom/3dTileableVoronoi"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_res("Texture Size", int) = 512
		_seed("Seed", float) = 0
		_freq("Frequency", vector) = (1,1,1,1)
		_scale("Scale", vector) = (1,1,1,1)
		_range("Range", float) = 1
		_points("Points per Cell", int) = 50
		_positionOffset("Position Offset", vector) = (0,0,0,0)
		_contrast("Contrast: Stretch, Power, Offset", vector) = (1,1,0,0)
		[Toggle]_manhattan("Manhattan", int) = 0
		[Toggle]_colored("Colored", int) = 0
		[Toggle]_inverted("Inverted", int) = 0
		[Space]
		[Toggle]_fbmBool("FBM", int) = 0
		[IntRange]_octaves("Octaves", Range(1, 10)) = 2
		_lacunarity("Lacunarity", float) = 2
		_persistence("Persistance", float) = 1
		
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float3 wpos : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 wpos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _res;
			float3 _freq;
			float3 _scale;
			float _range;
			float _seed;
			float _points;
			float4 _contrast;
			float3 _positionOffset;
			bool _manhattan;
			bool _colored;
			bool _inverted;
			bool _fbmBool;
			uint _octaves;
			float _lacunarity;
			float _persistence;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}

			float voronoi(float3 pos, float3 scale, inout fixed4 color)
			{
				// pos = fmod(pos * 0.5, scale);
				float m_dist = 10000;
				float dist;
				float3 p1, p2, pp, offset;
				p1 = (pos + _positionOffset);
				// cheap hash to offset loop
				uint seed = floor(sin(_seed)*43758.5453);
				for (uint i = seed; i < uint(_points)+seed; ++i)
				{
					float2 uv = float2(i%_res, i / _res);
					float2x2 rotationMatrix = float2x2(cos(i), -sin(i), sin(i), cos(i)) * 0.5;
					p2 = tex2D(_MainTex, mul(uv, rotationMatrix) / _res);
					// sample 3x3x3 grid
					for (int x = -1; x <= 1; ++x)
						for (int y = -1; y <= 1; ++y)
							for (int z = -1; z <= 1; ++z)
							{
								// calc distance, keep lowest (euclidean) / add differences (manhattan)
								offset = floor(p1) + float3(x, y, z);
								pp = (p1 - offset - p2) * _scale;
								if (_manhattan)
								{
									pp = abs(pp);
									dist = (pp.x + pp.y + pp.z);
								}
								else
								{
									dist = pow(pp, pp);
								}
								dist *= _range;
								if (m_dist > dist)
								{
									m_dist = dist;
									color.rgb = p2;
								}
							}
				}
				return m_dist;
			}

			
			float voronoiFBM(float3 pos, float3 scale, inout fixed4 color, uint octaves, float lacunarity, float persistence)
			{
				float sum = 0;
				float amplitude = _contrast.x;
				float3 frequency = _freq * 0.1;
				for (uint i = 0; i < octaves; i++)
				{
					sum += voronoi(pos * frequency, scale, color) * amplitude;
					saturate(sum);
					scale *= 2;
					frequency *= lacunarity;
					amplitude *= 0.35;
					amplitude *= persistence;
					pos = pos.zyx;
					if (i % 2 == 0)
					{
						pos = pos.yxz;
					}
				}
				return sum;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 color = fixed4(0,0,0,1);
				float3 scale = _scale * 2.0;
				float vn = 0;
				if (_fbmBool)
				{
					vn = voronoiFBM(i.wpos, scale, color, _octaves, _lacunarity, _persistence * 0.1);
				}
				else
				{
					vn = voronoi(i.wpos * _freq * 0.1, scale, color);
				}

				if (_colored)
				{
					if (_fbmBool)
					{
						// color = lerp(color, tex2D(_MainTex, float2(vn, vn*vn)), 0.5);
						color = tex2D(_MainTex, float2(vn, vn*vn));
					}
					if (_inverted)
					{
						color = 1 - color;
					}
					return color;
				}
				else
				{
					vn = vn * _contrast.x + 1.0;
					vn = pow(vn, _contrast.y);
					vn = (vn - 1.0) / _contrast.x;
					vn += _contrast.z;
					color = fixed4(vn, vn, vn, 1);
					if (_inverted)
					{
						color = 1 - color;
					}	
					return color;
				}
			}
			ENDCG
		}
	}
}
