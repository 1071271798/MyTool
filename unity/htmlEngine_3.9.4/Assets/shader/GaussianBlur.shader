// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GaussianBlur" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}

	}

		CGINCLUDE

#include "UnityCG.cginc"  

		sampler2D _MainTex;

	uniform half4 _MainTex_TexelSize;
	uniform float _blurSize;

	// weight curves  

	static const half curve[4] = { 0.0205, 0.0855, 0.232, 0.324 };
	static const half4 coordOffs = half4(1.0h, 1.0h, -1.0h, -1.0h);

	struct v2f_withBlurCoordsSGX
	{
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		half4 offs[3] : TEXCOORD1;
	};


	v2f_withBlurCoordsSGX vertBlurHorizontalSGX(appdata_img v)
	{
		v2f_withBlurCoordsSGX o;
		o.pos = UnityObjectToClipPos(v.vertex);

		o.uv = v.texcoord.xy;
		half2 netFilterWidth = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _blurSize;
		half4 coords = -netFilterWidth.xyxy * 3.0;

		o.offs[0] = v.texcoord.xyxy + coords * coordOffs;
		coords += netFilterWidth.xyxy;
		o.offs[1] = v.texcoord.xyxy + coords * coordOffs;
		coords += netFilterWidth.xyxy;
		o.offs[2] = v.texcoord.xyxy + coords * coordOffs;

		return o;
	}

	v2f_withBlurCoordsSGX vertBlurVerticalSGX(appdata_img v)
	{
		v2f_withBlurCoordsSGX o;
		o.pos = UnityObjectToClipPos(v.vertex);

		o.uv = v.texcoord.xy;
		half2 netFilterWidth = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _blurSize;
		half4 coords = -netFilterWidth.xyxy * 3.0;

		o.offs[0] = v.texcoord.xyxy + coords * coordOffs;
		coords += netFilterWidth.xyxy;
		o.offs[1] = v.texcoord.xyxy + coords * coordOffs;
		coords += netFilterWidth.xyxy;
		o.offs[2] = v.texcoord.xyxy + coords * coordOffs;

		return o;
	}

	half4 fragBlurSGX(v2f_withBlurCoordsSGX i) : SV_Target
	{
		half2 uv = i.uv;

		half4 color = tex2D(_MainTex, i.uv) * curve[3];

		for (int l = 0; l < 3; l++)
		{
			half4 tapA = tex2D(_MainTex, i.offs[l].xy);
			half4 tapB = tex2D(_MainTex, i.offs[l].zw);
			color += (tapA + tapB) * curve[l];
		}

		return color;

	}

		ENDCG

		SubShader {
		ZTest Off  ZWrite Off Blend Off



			Pass{
			ZTest Always


			CGPROGRAM

#pragma vertex vertBlurVerticalSGX  
#pragma fragment fragBlurSGX  

			ENDCG
		}


			Pass{
			ZTest Always


			CGPROGRAM

#pragma vertex vertBlurHorizontalSGX  
#pragma fragment fragBlurSGX  

			ENDCG
		}
	}

	FallBack Off
}
