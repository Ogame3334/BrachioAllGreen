Shader "LuYiSe/PassthroughEffectOverlay"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [KeywordEnum(Off, On)] _ZWrite("ZWrite", Float) = 1
        _greenWeightOffset("Green Weight Offset", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        ZTest [_ZTest]
        ZWrite [_ZWrite]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 mPos : TEXCOORD1;
                float3 wPos : TEXCOORD2;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _greenWeightOffset;

            #define PI 3.14159265358979

            //ランダム関数
            float rand(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 78.233)))*43758.5453);
            }
            //パーリンノイズ
            float pnoise(float p)
            {
                float2 ip = floor(p);
                float2 u = p - ip;
                u = u*u*(3.0-2.0*u);

                float2 res = lerp(
                    lerp(rand(ip), rand(ip + float2(1, 0)), u.x),
                    lerp(rand(ip + float2(0, 1)), rand(ip + float2(1, 1)), u.x),
                    u.y);
                return res;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.mPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.uv = (i.uv - .5)*2;
                // float2 br = float2(50,30);
                // i.uv = floor(i.uv * br) / br;
                float c = length(i.uv) + .6 - _greenWeightOffset * sqrt(2);
                return float4(0, 0, 0, c);
                // return float4(0, 40./255, 0, c);
            }
            ENDCG
        }
    }
}
