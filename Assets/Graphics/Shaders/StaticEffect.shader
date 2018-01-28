Shader "Static/StaticEffect"
{
	Properties
	{
	    _ColorA ("Color A", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (0,0,0,0)

		_ResX ("X Resolution", Float) = 100
        _ResY ("Y Resolution", Float) = 200

		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			//This produces random values between 0 and 1
            float rand(float2 co)
             {
                     return frac((sin( dot(co.xy , float2(12.345 * _Time.w, 67.890 * _Time.w) )) * 12345.67890+_Time.w));
             }

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 _ColorA;
            fixed4 _ColorB;
			float _ResX;
            float _ResY;
			sampler2D _MainTex;

			fixed4 frag (v2f i,float4 screenPos : SV_POSITION) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				if(col.r < 0.025f && col.b < 0.025f && col.g < 0.025f){
					fixed4 sc = fixed4((screenPos.xy), 0.0, 1.0);
 
				    //round the screen coordinates to give it a blocky appearance
					sc.x = round(sc.x*_ResX) / _ResX;
					sc.y = round(sc.y*_ResY) / _ResY;

					float noise = rand(sc.xy);
					float4 stat = lerp(_ColorA, _ColorB, noise.x);
					return fixed4(stat.xyz, 1.0);
				}

				return col;
			}
			ENDCG
		}
	}
}
