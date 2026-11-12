// Anime-Cel-Shading im Stil moderner Anime-RPGs:
// harte Licht/Schatten-Kante, farbiger (nicht grauer) Schatten, Randlicht, stilisierter Glanzpunkt.
// Funktioniert in der Built-in Render Pipeline und in URP (keine pipelinespezifischen Includes).
Shader "AnimeFantasy/Toon"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Farbe", Color) = (1,1,1,1)
        _ShadeColor ("Schattenfarbe (Multiplikator)", Color) = (0.78,0.70,0.86,1)
        _ShadeThreshold ("Schattengrenze", Range(-1,1)) = 0.05
        _ShadeSoftness ("Schattenweichheit", Range(0.001,0.5)) = 0.03
        _RimColor ("Randlicht (A = Stärke)", Color) = (1,1,1,0.35)
        _RimPower ("Randlicht Breite", Range(1,12)) = 4
        _SpecColor2 ("Glanz (A = Stärke)", Color) = (1,1,1,0)
        _SpecSize ("Glanz Größe", Range(0.8,1)) = 0.965
        _EmissionColor ("Leuchten", Color) = (0,0,0,0)
        _FlashAmount ("Treffer-Blitz", Range(0,1)) = 0
        _FlashColor ("Treffer-Farbe", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;
            float4 _Color, _ShadeColor, _RimColor, _SpecColor2, _EmissionColor, _FlashColor;
            float _ShadeThreshold, _ShadeSoftness, _RimPower, _SpecSize, _FlashAmount;
            // Globale Werte, gesetzt von AnimeFantasy.Toon.SetLighting
            float4 _AF_LightDir, _AF_LightColor, _AF_Ambient;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float3 n : TEXCOORD1; float3 wpos : TEXCOORD2; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.n = UnityObjectToWorldNormal(v.normal);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.n);
                float3 l = normalize(_AF_LightDir.xyz + float3(0, 0.0001, 0));
                float3 v = normalize(_WorldSpaceCameraPos - i.wpos);
                float4 albedo = tex2D(_MainTex, i.uv) * _Color;

                float lit = smoothstep(_ShadeThreshold - _ShadeSoftness, _ShadeThreshold + _ShadeSoftness, dot(n, l));
                float3 lightCol = max(_AF_LightColor.rgb, 0.05);
                float3 col = lerp(albedo.rgb * _ShadeColor.rgb, albedo.rgb, lit) * lightCol + albedo.rgb * _AF_Ambient.rgb;

                float3 h = normalize(l + v);
                float spec = smoothstep(_SpecSize - 0.005, _SpecSize + 0.005, dot(n, h)) * lit;
                col += _SpecColor2.rgb * _SpecColor2.a * spec;

                float rim = pow(1.0 - saturate(dot(n, v)), _RimPower) * (0.35 + 0.65 * lit);
                col += _RimColor.rgb * _RimColor.a * smoothstep(0.3, 0.6, rim);

                col += _EmissionColor.rgb;
                col = lerp(col, _FlashColor.rgb, _FlashAmount);
                return float4(col, 1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Color"
}
