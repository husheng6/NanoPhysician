Shader "NanoPhysician/BackgroundBlur"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.88, 0.88, 0.92, 0.95)
        _BlurSize ("Blur Size", Range(0, 4)) = 1.6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _BlurSize;

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            fixed4 SampleBlur(float2 uv)
            {
                float2 offset = _MainTex_TexelSize.xy * _BlurSize;
                fixed4 col = tex2D(_MainTex, uv) * 0.28;
                col += tex2D(_MainTex, uv + float2(offset.x, 0)) * 0.16;
                col += tex2D(_MainTex, uv - float2(offset.x, 0)) * 0.16;
                col += tex2D(_MainTex, uv + float2(0, offset.y)) * 0.16;
                col += tex2D(_MainTex, uv - float2(0, offset.y)) * 0.16;
                col += tex2D(_MainTex, uv + offset) * 0.04;
                col += tex2D(_MainTex, uv - offset) * 0.04;
                return col;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 col = SampleBlur(input.uv) * input.color;
                return col;
            }
            ENDCG
        }
    }
}
