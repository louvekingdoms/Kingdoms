
Shader "Unlit/SH_InnerStroke"
{
    Properties
    {
		_MainTex("Color (RGB) Alpha (A)", 2D) = "white"
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull front
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert alpha
			#pragma fragment frag alpha
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
		
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _OutlineWidth = 0.1;

            v2f vert (appdata v)
            {
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col = tex2D(_MainTex, i.uv.xy); // multiply by _Color
			/*
				col =
					tex2D(_MainTex, i.uv.xy + float2(0, _OutlineWidth))*
					tex2D(_MainTex, i.uv.xy - float2(0, _OutlineWidth))*
					tex2D(_MainTex, i.uv.xy + float2(_OutlineWidth, 0))*
					tex2D(_MainTex, i.uv.xy + float2(_OutlineWidth, 0));
			*/
				return col;
            }
            ENDCG
        }
    }
}
