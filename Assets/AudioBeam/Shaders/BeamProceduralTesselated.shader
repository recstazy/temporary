Shader "WaveBeam/ProceduralTesselated"
{
	Properties
    {
		[HDR]_MainColor ("MainColor", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
        _Tess("Tesselation", float) = 1

        _Amplitude("Amplitude", float) = 1
        _Frequency("Frequency", float) = 1
        _Periods("Periods", float) = 1
        _XYPhase("XY Phase", Range(0, 1)) = 0.25

        _Modulations("Modulations count", int) = 3
        _ModulationStep("Modulation Freq Step", float) = 300
        _AmplitudeDelta("Amplitude Per Step", float) = 0.25

        [Enum(UnityEngine.Rendering.CullMode)]
		_CullMode("Cull Mode", Int) = 2
    }
        SubShader
        {
            Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 300
            Cull [_CullMode]

            CGPROGRAM
            #pragma surface surf BlinnPhong vertex:vert tessellate:tessFixed nolightmap alpha
            #pragma target 4.6

            struct appdata 
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Input 
            {
                float2 uv_MainTex;
            };

            float _Tess;

            float4 _MainColor;
            sampler2D _MainTex;
            float _Amplitude;
            float _Frequency;
            float _Periods;
            float _XYPhase;

            int _Modulations;
            float _ModulationStep;
            float _AmplitudeDelta;

            float4 tessFixed()
            {
                return _Tess;
            }

            float Function(float t)
            {
                float result;

                for (int i = 0; i < _Modulations; i++)
                {
                    result += sin(t + t * i * (_ModulationStep)) * pow(_AmplitudeDelta, i);
                }

                return result;
            }

            float EvaluateFunction(float uv, float phase)
            {
                float function = Function(uv * _Periods + (_Time + phase) * _Frequency);
                float weight = (1 - abs(uv - 0.5) * 2);
                return function * weight;
            }

            float4 DispaceVertex(appdata v)
            {
                float4 offset = (float4(1, 0, 0, 0) * EvaluateFunction(v.texcoord.x, 0) + float4(0, 0, 1, 0) * EvaluateFunction(v.texcoord.x, _XYPhase)) * v.color.r * _Amplitude;
                float4 vertex = v.vertex + offset;
                return vertex;
            }

            void vert (inout appdata v)
            {
                v.vertex = DispaceVertex(v);
            }

            void surf (Input IN, inout SurfaceOutput o) 
            {
                half4 c = tex2D(_MainTex, IN.uv_MainTex) * _MainColor;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }
            ENDCG
        }
        FallBack "Diffuse"
}
