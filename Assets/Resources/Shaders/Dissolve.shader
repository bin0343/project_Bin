Shader "Custom/BuiltIn/EnemyDissolve"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}

        _NoiseTex ("Dissolve Noise", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1.2)) = 0
        _EdgeWidth ("Edge Width", Range(0.01, 0.2)) = 0.05
        _EdgeColor ("Edge Color", Color) = (1, 0.55, 0.15, 1)
        _EdgeEmission ("Edge Emission", Range(0, 5)) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
        }

        LOD 200
        Cull Back

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NoiseTex;

        fixed4 _Color;
        fixed4 _EdgeColor;

        half _DissolveAmount;
        half _EdgeWidth;
        half _EdgeEmission;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NoiseTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            half noise = tex2D(_NoiseTex, IN.uv_NoiseTex).r;

            clip(noise - _DissolveAmount);

            half edge = smoothstep(_DissolveAmount, _DissolveAmount + _EdgeWidth, noise);

            o.Albedo = lerp(_EdgeColor.rgb, baseColor.rgb, edge);
            o.Emission = _EdgeColor.rgb * (1 - edge) * _EdgeEmission;

            o.Metallic = 0;
            o.Smoothness = 0.25;
            o.Alpha = baseColor.a;
        }
        ENDCG
    }

    FallBack "Transparent/Cutout/VertexLit"
}