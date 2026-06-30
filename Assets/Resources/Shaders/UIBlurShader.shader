Shader "Custom/UI/GrabPassBlur"
{
    Properties
    {
        _Size ("Blur Size", Range(0, 20)) = 5
        _Color ("Tint Color", Color) = (1, 1, 1, 0.5) // 살짝 어둡거나 투명한 느낌을 주는 틴트
    }
    SubShader
    {
        // UI 환경에 맞게 투명(Transparent) 큐와 타입을 설정합니다.
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        // 중요: 이 명령어가 현재 이 UI가 그려지기 직전의 화면을 캡처하여 _GrabTexture에 저장합니다.
        GrabPass { "_GrabTexture" }

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
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize; // 텍셀(픽셀 크기) 정보 자동 할당
            float _Size;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 화면 공간(Screen Space)을 기준으로 캡처된 텍스처의 좌표를 계산합니다.
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 uv = i.grabPos;
                
                // _Size값에 따라 주변 픽셀을 샘플링할 오프셋 크기를 결정합니다.
                float2 texelSize = _GrabTexture_TexelSize.xy * _Size;

                // 9개의 방향(중앙 + 대각선 4방향 + 상하좌우 4방향)을 샘플링하여 뭉개는 가벼운 블러 연산
                fixed4 col = tex2Dproj(_GrabTexture, uv);
                col += tex2Dproj(_GrabTexture, uv + float4(-texelSize.x, -texelSize.y, 0, 0));
                col += tex2Dproj(_GrabTexture, uv + float4(texelSize.x, -texelSize.y, 0, 0));
                col += tex2Dproj(_GrabTexture, uv + float4(-texelSize.x, texelSize.y, 0, 0));
                col += tex2Dproj(_GrabTexture, uv + float4(texelSize.x, texelSize.y, 0, 0));
                
                col += tex2Dproj(_GrabTexture, uv + float4(-texelSize.x, 0, 0, 0));
                col += tex2Dproj(_GrabTexture, uv + float4(texelSize.x, 0, 0, 0));
                col += tex2Dproj(_GrabTexture, uv + float4(0, -texelSize.y, 0, 0));
                col += tex2Dproj(_GrabTexture, uv + float4(0, texelSize.y, 0, 0));

                // 9개 샘플의 평균값 계산
                col /= 9.0;
                
                // 설정한 틴트 컬러와 결합 (예: 뒤를 흐리게 하면서 약간 검은색 반투명 레이어를 얹는 효과)
                return col * _Color;
            }
            ENDCG
        }
    }
}