Shader "WaveBeam/ProceduralGen"
{
	Properties
    {
		[HDR]_MainColor ("MainColor", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4 _MainColor;
            sampler2D _MainTex;
            float _Amplitude;
            float _Frequency;
            float _Periods;
            float _XYPhase;

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
                float weight = (1 - abs(uv - 0.5) * 2);
                return function * weight;
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                // float4 offset = (float4(1, 0, 0, 0) * EvaluateFunction(v.uv.x, 0) + float4(0, 0, 1, 0) * EvaluateFunction(v.uv.x, _XYPhase)) * v.color.r * _Amplitude;
                // float4 vertex = v.vertex + offset;
                float4 vertex = v.vertex;

                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = v.uv;
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            #define CUSTOM_GEOM
            #define SUBDIVISION 16

            #if defined(CUSTOM_GEOM)
            [maxvertexcount(SUBDIVISION * 6)]
            void geom(uint primitiveID : SV_PrimitiveID, triangle v2f input[3], inout TriangleStream<v2f> triStream)
            {
                _Subdivision = SUBDIVISION;
                v2f oBot;
                v2f oTop;

                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    oBot.fogCoord = input[0].fogCoord;
                    oTop.fogCoord = input[0].fogCoord;
                #endif

                float4 bottomCenter = lerp(input[0].vertex, input[1].vertex, 0.5);
                float4 up = input[2].vertex - input[0].vertex;
                float4 right = input[1].vertex - bottomCenter;
                float fraction = float(1) / _Subdivision;
                
                for (int i = 0; i < _Subdivision; i++)
                {
                    float4 currentPoint = bottomCenter + up * fraction * i;
                    float4 nextPoint = bottomCenter + up * fraction * (i + 1);
                    float uvX = fraction * i;
                    float nextUVx = fraction * (i + 1);
                    // float4 offset = (float4(1, 0, 0, 0) * EvaluateFunction(uvX, 0) + float4(0, 0, 1, 0) * EvaluateFunction(uvX, _XYPhase)) * _Amplitude;
                    float4 offset = (normalize(right) * EvaluateFunction(uvX, 0)) * _Amplitude;
                    // offset = 0;
                    // float4 nextOffset = (float4(1, 0, 0, 0) * EvaluateFunction(nextUVx, 0) + float4(0, 0, 1, 0) * EvaluateFunction(nextUVx, _XYPhase)) * _Amplitude;
                    float4 nextOffset = (normalize(right) * EvaluateFunction(nextUVx, 0)) * _Amplitude;
                    // nextOffset = 0;

                    currentPoint += offset;
                    nextPoint += offset;
                    oBot.color = lerp(input[0].color, input[2].color, uvX);
                    oBot.uv = float2(uvX, 0);
                    oTop.uv = float2(nextUVx, 0);

                    oBot.vertex = currentPoint - right;
                    triStream.Append(oBot);

                    oBot.vertex = currentPoint + right;
                    oBot.uv.y = 1;
                    triStream.Append(oBot);
                }
            }

            #else

            [maxvertexcount(3)]
            void geom(uint primitiveID : SV_PrimitiveID, triangle v2f input[3], inout TriangleStream<v2f> triStream)
            {
                for (int i = 0; i < 3; i++)
                    {
                        v2f o;

                        o.uv = input[i].uv;
                        o.vertex = input[i].vertex;
                        o.color = input[i].color * 0.5 * i;

                        #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                        o.fogCoord = input[i].fogCoord;
                        #endif
                        
                        triStream.Append(o);
                    }
                
            }

            #endif

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _MainColor;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
