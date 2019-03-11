Shader "Custom/ModelTransparentMask" {
	SubShader{
		Tags{ "Queue" = "Geometry-10" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		ZWrite On
		ZTest Always
		Pass
		{
			Color(0,0,0,0)
		}
	}
}
