Shader "Custom/PlaneFadeToVoid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _VoidColor ("Void Color", Color) = (0,0,0,1)
        _FadeDistance ("Fade Distance", Range(0.1, 10)) = 2.0
        _FadeEdgeSmoothing ("Edge Smoothing", Range(0.01, 2.0)) = 0.5
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _VoidColor;
            float _FadeDistance;
            float _FadeEdgeSmoothing;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Calculate distance from center
                float2 center = float2(unity_ObjectToWorld[0][3], unity_ObjectToWorld[2][3]);
                float2 currentPos = float2(i.worldPos.x, i.worldPos.z);
                float distFromCenter = length(currentPos - center);
                
                // Create smooth fade to void
                float fadeStart = _FadeDistance - _FadeEdgeSmoothing;
                float fadeEnd = _FadeDistance + _FadeEdgeSmoothing;
                float fadeFactor = smoothstep(fadeStart, fadeEnd, distFromCenter);
                
                // Blend between normal color and void color
                return lerp(col, _VoidColor, fadeFactor);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}