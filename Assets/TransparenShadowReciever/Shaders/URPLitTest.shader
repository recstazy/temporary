Shader "Universal Render Pipeline/LitTest"
{
    Properties
    {
        // Specular vs Metallic workflow
        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0

        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        _ReceiveShadows("Receive Shadows", Float) = 1.0
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections("EnvironmentReflections", Float) = 0.0
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 300

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard SRP library
            // All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _OCCLUSIONMAP

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            
            #ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
            #define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
	            float4 positionOS   : POSITION;
	            float3 normalOS     : NORMAL;
	            float4 tangentOS    : TANGENT;
	            float2 texcoord     : TEXCOORD0;
	            float2 lightmapUV   : TEXCOORD1;
	            UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
	            float2 uv                       : TEXCOORD0;
	            DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
	            float3 positionWS               : TEXCOORD2;
            #endif

            #ifdef _NORMALMAP
	            float4 normalWS                 : TEXCOORD3;    // xyz: normal, w: viewDir.x
	            float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: viewDir.y
	            float4 bitangentWS              : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
            #else
	            float3 normalWS                 : TEXCOORD3;
	            float3 viewDirWS                : TEXCOORD4;
            #endif

	            half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	            float4 shadowCoord              : TEXCOORD7;
            #endif

	            float4 positionCS               : SV_POSITION;
	            UNITY_VERTEX_INPUT_INSTANCE_ID
	            UNITY_VERTEX_OUTPUT_STEREO
            };

            void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
            {
	            inputData = (InputData)0;

            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
	            inputData.positionWS = input.positionWS;
            #endif

            #ifdef _NORMALMAP
	            half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
	            inputData.normalWS = TransformTangentToWorld(normalTS,
		            half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
            #else
	            half3 viewDirWS = input.viewDirWS;
	            inputData.normalWS = input.normalWS;
            #endif

	            inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
	            viewDirWS = SafeNormalize(viewDirWS);
	            inputData.viewDirectionWS = viewDirWS;

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	            inputData.shadowCoord = input.shadowCoord;
            #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	            inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
            #else
	            inputData.shadowCoord = float4(0, 0, 0, 0);
            #endif

	            inputData.fogCoord = input.fogFactorAndVertexLight.x;
	            inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
	            inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
            }

            ///////////////////////////////////////////////////////////////////////////////
            //                  Vertex and Fragment functions                            //
            ///////////////////////////////////////////////////////////////////////////////

            // Used in Standard (Physically Based) shader
            Varyings LitPassVertex(Attributes input)
            {
	            Varyings output = (Varyings)0;

	            UNITY_SETUP_INSTANCE_ID(input);
	            UNITY_TRANSFER_INSTANCE_ID(input, output);
	            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
	            VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
	            half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
	            half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
	            half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

	            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

            #ifdef _NORMALMAP
	            output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
	            output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
	            output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
            #else
	            output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
	            output.viewDirWS = viewDirWS;
            #endif

	            OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
	            OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

	            output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
	            output.positionWS = vertexInput.positionWS;
            #endif

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	            output.shadowCoord = GetShadowCoord(vertexInput);
            #endif

	            output.positionCS = vertexInput.positionCS;

	            return output;
            }

            // Used in Standard (Physically Based) shader
            half4 LitPassFragment(Varyings input) : SV_Target
            {
	            UNITY_SETUP_INSTANCE_ID(input);
	            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	            SurfaceData surfaceData;
	            InitializeStandardLitSurfaceData(input.uv, surfaceData);

	            InputData inputData;
	            InitializeInputData(input, surfaceData.normalTS, inputData);
                Light mainLight = GetMainLight(inputData.shadowCoord);

                half3 shadow = mainLight.shadowAttenuation;

                half4 color;
                color.rgb = _SubtractiveShadowColor.xyz;
                color.a = 1 - shadow.x;

	            color.rgb = MixFog(color.rgb, inputData.fogCoord);
	            return color;
            }

            #endif


            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
}
