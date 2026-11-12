// Weicher, halbtransparenter Kontaktschatten unter den Figuren.
Shader "AnimeFantasy/Shadow"
{
    Properties
    {
        _MainTex ("Textur", 2D) = "white" {}
        _Color ("Farbe", Color) = (0,0,0.05,0.45)
    }
    SubShader
    {
        Tags { "Queue"="Transparent-10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _Color;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            float4 frag (v2f i) : SV_Target { float4 c = _Color; c.a *= tex2D(_MainTex, i.uv).a; return c; }
            ENDCG
        }
    }
}
