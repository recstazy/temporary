Shader "WaveBeam/ProceduralGen"
{
	Properties
    {
		[HDR]_MainColor ("MainColor", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}

        _Width("Width", float) = 1
        _Amplitude("Amplitude", float) = 1
        _Frequency("Frequency", float) = 1
        _Periods("Periods", float) = 1
        _XYPhase("XY Phase", Range(0, 1)) = 0.25
        _DepthAmplitude("Depth Amplitude", Range(0, 1)) = 1

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
        LOD 100

        Pass
        {
            Cull [_CullMode]

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag alpha 

            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2g
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(2)
            };

            float4 _MainColor;
            sampler2D _MainTex;
            float _Width;
            float _Amplitude;
            float _Frequency;
            float _Periods;
            float _XYPhase;
            float _DepthAmplitude;

            int  _Modulations;
            float _ModulationStep;
            float _AmplitudeDelta;
            int _Subdivision;

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
                float weight = 1 - pow(abs(uv - 0.5) * 2, 3);
                return function * weight;
            }

            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                o.uv1 = v.uv1;
                
                return o;
            }
            
            #define SUBDIVISION 64

            g2f TransferOutput(g2f o)
            {
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            [maxvertexcount(SUBDIVISION * 2)]
            void geom(uint primitiveID : SV_PrimitiveID, triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                _Subdivision = SUBDIVISION;
                g2f output;

                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    output.fogCoord = input[0].fogCoord;
                #endif

                float4 bottomCenter = lerp(input[0].vertex, input[1].vertex, 0.5);
                float4 up = input[2].vertex - bottomCenter;
                float4 right = mul(float4(1, 0, 0, 0), UNITY_MATRIX_V);
                float4 fwd = float4(cross(normalize(up), right), 0);

                float4 triangleRight = normalize(input[1].vertex - bottomCenter);
                float4 triangleFwd = float4(cross(normalize(up), triangleRight), 1);
                float fraction = float(1) / (_Subdivision - 1);
                
                for (int i = 0; i <= _Subdivision; i++)
                {
                    float uv1X = lerp(input[0].uv1.x, input[2].uv1.x, fraction * i);
                    float4 currentPoint = bottomCenter + up * fraction * i;
                    float4 offset = (triangleRight * EvaluateFunction(uv1X, 0) + triangleFwd * EvaluateFunction(uv1X, _XYPhase) * _DepthAmplitude) * _Amplitude;

                    currentPoint += offset;

                    output.vertex = currentPoint - right * _Width;
                    float uvX = lerp(input[0].uv.x, input[2].uv.x, fraction * i);
                    output.uv = float2(uvX, 0);
                    output = TransferOutput(output);
                    triStream.Append(output);

                    output.vertex = currentPoint + right * _Width;
                    output.uv.y = 1;
                    output = TransferOutput(output);
                    triStream.Append(output);
                }
            }

            fixed4 frag (g2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _MainColor;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
