﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RadiusTexture" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_RADIUSBUCE("_RADIUSBUCE",Range(0,0.5)) = 0.2
		_TexWidth("_TexWidth",float) = 1
		_TexHeight("_TexHeight",float) = 1
	}
	SubShader
	{
		pass
		{
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#include "unitycg.cginc"
			float _RADIUSBUCE;
			float _TexWidth;
			float _TexHeight;
			sampler2D _MainTex;


			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 ModeUV: TEXCOORD0;
				float2 RadiusBuceVU : TEXCOORD1;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex); //v.vertex;
				o.ModeUV = v.texcoord ;
				o.RadiusBuceVU = v.texcoord - float2(0.5,0.5);       //将模型UV坐标原点置为中心原点,为了方便计算
				return o;
			}




			fixed4 frag(v2f i) :COLOR
			{
				fixed4 col;
				col = (0, 1, 1,0);
				if (abs(i.RadiusBuceVU.x)<0.5 - _RADIUSBUCE || abs(i.RadiusBuceVU.y) < (0.5 - _RADIUSBUCE) / _TexHeight)    //即上面说的|x|<(0.5-r)或|y|<(0.5-r)
				{
					col = tex2D(_MainTex,i.ModeUV);
				}
				else
				{
					if (length(abs(i.RadiusBuceVU) - float2(0.5 - _RADIUSBUCE,(0.5 - _RADIUSBUCE))) <_RADIUSBUCE)
					{
						col = tex2D(_MainTex,i.ModeUV);
					}
					else
					{
						discard;
					}
				}
				return col;
			}
			ENDCG

		}
	}
}
