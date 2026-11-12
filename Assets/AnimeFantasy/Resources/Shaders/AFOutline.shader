// Konturlinie per "Inverted Hull": konstante Linienbreite auf dem Bildschirm, wie bei Anime-Figuren.
// Wird als zusätzliches Material an Renderer angehängt (siehe Toon.AddOutline).
Shader "AnimeFantasy/Outline"
{
    Properties
    {
        _OutlineColor ("Konturfarbe", Color) = (0.12,0.08,0.12,1)
        _OutlineWidth ("Konturbreite", Range(0,0.02)) = 0.004
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 worldN = UnityObjectToWorldNormal(v.normal);
                float2 clipN = mul((float3x3)UNITY_MATRIX_VP, worldN).xy;
                float len = max(length(clipN), 0.0001);
                float2 offset = clipN / len * _OutlineWidth * o.pos.w;
                offset.x *= _ScreenParams.y / _ScreenParams.x;
                o.pos.xy += offset;
                return o;
            }

            float4 frag (v2f i) : SV_Target { return _OutlineColor; }
            ENDCG
        }
    }
}
