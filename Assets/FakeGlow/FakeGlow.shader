Shader "Unlit/FakeGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color", Color) = (1, 1, 1, 1)
        _Bias("Bias", float) = 0.5
        _Scale("Scale", float) = 1
        _Power("Power", float) = 0.85
        _AlphaTreshold("AlphaTreshold", float) = 1.7
        _Fade("Fade", float) = 0.7
        _HDRFade("HDRFade", float) = 15
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag alpha
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

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
                float2 fresnel : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Bias;
            float _Scale;
            float _Power;
            float _AlphaTreshold;
            float _Fade;
            float _HDRFade;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float3 posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz);

                float3 I = normalize(posWorld - _WorldSpaceCameraPos.xyz);
                o.fresnel = float2(_Bias + _Scale * pow(1.0 + dot(I, normWorld), _Power), 0);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed inverseFresnel = _AlphaTreshold - i.fresnel.x;
                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 rawColor = texColor * _Color;
                fixed fade = pow(inverseFresnel, _Fade);
                fixed4 col = lerp(normalize(rawColor), rawColor, pow(fade, _HDRFade));
                col.a = saturate(texColor.a * fade * normalize(rawColor).a);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
