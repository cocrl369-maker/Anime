// Verlaufshimmel mit Sonnen-/Mondschein für die Kampfbühne.
Shader "AnimeFantasy/Sky"
{
    Properties
    {
        _Top ("Oben", Color) = (0.25,0.45,0.85,1)
        _Horizon ("Horizont", Color) = (0.95,0.75,0.65,1)
        _Bottom ("Unten", Color) = (0.15,0.15,0.2,1)
        _SunDir ("Sonnenrichtung", Vector) = (0.3,0.4,1,0)
        _SunColor ("Sonnenfarbe", Color) = (1,0.9,0.7,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Front
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Top, _Horizon, _Bottom, _SunDir, _SunColor;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float3 col = d.y > 0 ? lerp(_Horizon.rgb, _Top.rgb, pow(saturate(d.y), 0.55))
                                     : lerp(_Horizon.rgb, _Bottom.rgb, pow(saturate(-d.y * 3), 0.5));
                float s = saturate(dot(d, normalize(_SunDir.xyz)));
                col += _SunColor.rgb * (pow(s, 600) * 2.0 + pow(s, 12) * 0.35);
                return float4(col, 1);
            }
            ENDCG
        }
    }
}
