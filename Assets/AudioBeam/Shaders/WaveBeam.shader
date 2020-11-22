Shader "WaveBeam/WaveBeam"
{
    Properties
    {
        [HDR]_Color("Color", Color) = (1, 1, 1, 1)
        _LineWidth("Line Width", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };
 
            struct v2g
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
 
            struct g2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _LineWidth;
            float audioData[2048];
            int _AudioLength;
            int sampleLength;
            int pointsPerSegment;
            float4 points[100];
 
            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.color = v.color;
                return o;
            }
 
            [maxvertexcount(6)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;
                o.color = IN[0].color;
                float4 vertex;

                float4 up = IN[1].vertex - IN[0].vertex;
                float4 forward = mul(float3(0, 0, 1), UNITY_MATRIX_IT_MV);
                float3 rightVec = cross(normalize(up), forward);

                float4 right = float4(rightVec.x, rightVec.y, rightVec.z, 0);

                float4 points[6];

                points[0] = IN[0].vertex - _LineWidth * right;
                points[1] = IN[0].vertex + up + _LineWidth * right;
                points[2] = IN[0].vertex + up - _LineWidth * right;
                points[3] = points[1];
                points[4] = points[0];
                points[5] = IN[0].vertex + _LineWidth * right;

                for (int i = 0; i < 3; i++)
                {
                    o.vertex = UnityObjectToClipPos(points[i]);
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    triStream.Append(o);
                }

                triStream.RestartStrip();

                for (int i = 3; i < 6; i++)
                {
                    o.vertex = UnityObjectToClipPos(points[i]);
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    triStream.Append(o);
                }
            }
 
            fixed4 frag (g2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}