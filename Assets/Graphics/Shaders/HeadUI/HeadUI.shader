Shader "Shader Graphs/Head"
{
    Properties
    {

        
        _Stencil ("Stencil ID", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15




        _SkinColor("SkinColor", Color) = (0.8037735, 0.598343, 0.3442577, 1)
        _HairColor("HairColor", Color) = (0.4566037, 0.260739, 0.05427551, 1)
        _Direction("Direction", Int) = 0
        _EyesIndex("EyesIndex", Float) = 2
        _MouthIndex("MouthIndex", Float) = 0
        _BeardIndex("BeardIndex", Float) = 0
        _HairIndex("HairIndex", Int) = 3
        _PaintingsIndex("PaintingsIndex", Int) = 3
        [NoScaleOffset]_Mask("Mask", 2D) = "white" {}
        _MaskColor("MaskColor", Color) = (0.8784314, 0.1994039, 0.1994039, 1)
        [NoScaleOffset]_Hat("Hat", 2D) = "white" {}
        _HatColor("HatColor", Color) = (0.4339623, 0.04830898, 0.2136369, 1)
        [ToggleUI]_HairUnderHat("HairUnderHat", Float) = 1
        [NoScaleOffset]_Hairs("Hairs", 2D) = "white" {}
        [NoScaleOffset]_Eyes("Eyes", 2D) = "white" {}
        [NoScaleOffset]_Mouth("Mouth", 2D) = "white" {}
        [NoScaleOffset]_Beard("Beard", 2D) = "white" {}
        [NoScaleOffset]_Paintings("Paintings", 2D) = "white" {}
        [NoScaleOffset]_MainTex("Heads", 2D) = "white" {}
        [NoScaleOffset]_CutMask("CutMask", 2D) = "white" {}
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        [HideInInspector]_BUILTIN_Surface("Float", Float) = 1
        [HideInInspector]_BUILTIN_Blend("Float", Float) = 0
        [HideInInspector]_BUILTIN_AlphaClip("Float", Float) = 0
        [HideInInspector]_BUILTIN_SrcBlend("Float", Float) = 1
        [HideInInspector]_BUILTIN_DstBlend("Float", Float) = 0
        [HideInInspector]_BUILTIN_ZWrite("Float", Float) = 0
        [HideInInspector]_BUILTIN_ZWriteControl("Float", Float) = 0
        [HideInInspector]_BUILTIN_ZTest("Float", Float) = 4
        [HideInInspector]_BUILTIN_CullMode("Float", Float) = 0
        [HideInInspector]_BUILTIN_QueueOffset("Float", Float) = 0
        [HideInInspector]_BUILTIN_QueueControl("Float", Float) = -1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalSpriteLitSubTarget"
        }
        Pass
        {
            Name "Sprite Lit"
            Tags
            {
                "LightMode" = "Universal2D"
            }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_SCREENPOSITION
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITELIT
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
             float4 screenPosition;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float4 screenPosition : INTERP2;
             float3 positionWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.screenPosition.xyzw = input.screenPosition;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.screenPosition = input.screenPosition.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
        Out = A * B;
        }
        
        void Unity_ReplaceColor_float(float3 In, float3 From, float3 To, float Range, out float3 Out, float Fuzziness)
        {
            float Distance = distance(From, In);
            Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 1e-5f)));
        }
        
        void Unity_ColorspaceConversion_RGB_HSV_float(float3 In, out float3 Out)
        {
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float  E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            Out = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        }
        
        void Unity_Modulo_float(float A, float B, out float Out)
        {
            Out = fmod(A, B);
        }
        
        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
        Out = A * B;
        }
        
        struct Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float
        {
        };
        
        void SG_Color_273f886ee6f0c3844bf36127a8973ed7_float(float3 _Vector3, float4 _Color, float _Dark, Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float IN, out float4 New_0)
        {
        float3 _Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3 = _Vector3;
        float4 _Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4 = _Color;
        float _Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float = _Dark;
        float4 _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxxx), _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4);
        float3 _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3;
        Unity_ReplaceColor_float(_Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3, IsGammaSpace() ? float3(0.7843138, 0.7843138, 0.7843138) : SRGBToLinear(float3(0.7843138, 0.7843138, 0.7843138)), (_Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4.xyz), float(0.01), _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, float(0));
        float4 _Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4 = _Color;
        float3 _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3;
        Unity_ColorspaceConversion_RGB_HSV_float((_Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4.xyz), _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3);
        float _Split_92969d9d4ec1471882addef057112aae_R_1_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[0];
        float _Split_92969d9d4ec1471882addef057112aae_G_2_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[1];
        float _Split_92969d9d4ec1471882addef057112aae_B_3_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[2];
        float _Split_92969d9d4ec1471882addef057112aae_A_4_Float = 0;
        float _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float = float(0.85);
        float _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float;
        Unity_Add_float(_Split_92969d9d4ec1471882addef057112aae_R_1_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float);
        float _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float;
        Unity_Modulo_float(_Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float, float(1), _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float);
        float _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean);
        float3 _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3 = float3(float(0.46), float(0.65), float(0.74));
        float3 _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3 = float3(float(0.78), float(0.77), float(0.72));
        float3 _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3;
        Unity_Branch_float3(_Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean, _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3, _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3, _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3);
        float3 _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4.xyz), _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3, _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3);
        float3 _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxx), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3);
        float3 _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3;
        Unity_ReplaceColor_float(_ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, IsGammaSpace() ? float3(0.7137255, 0.7137255, 0.7137255) : SRGBToLinear(float3(0.7137255, 0.7137255, 0.7137255)), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3, float(0.01), _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, float(0));
        New_0 = (float4(_ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, 1.0));
        }
        
        void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = lerp(Base, Blend, Opacity);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float4 SpriteMask;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mask);
            float _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e;
            _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e.uv0 = IN.uv0;
            float4 _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4;
            float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D, _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float, float(0), 0, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5;
            float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float4 _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_SkinColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_853ed1e076184a5c9b1cb51ad54e1df3;
            float4 _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.xyz), _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4, float(1), _Color_853ed1e076184a5c9b1cb51ad54e1df3, _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4);
            UnityTexture2D _Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Paintings);
            float _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float = _PaintingsIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_542310d721c844a1b5a30a99e3acf826;
            _GetTexture2D_542310d721c844a1b5a30a99e3acf826.uv0 = IN.uv0;
            float4 _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4;
            float _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D, _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float, _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float, 0, _GetTexture2D_542310d721c844a1b5a30a99e3acf826, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            float4 _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            UnityTexture2D _Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Eyes);
            float _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float = _EyesIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_f518cf5937a5450e83ccd899942e92da;
            _GetTexture2D_f518cf5937a5450e83ccd899942e92da.uv0 = IN.uv0;
            float4 _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4;
            float _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D, _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float, _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float, 0, _GetTexture2D_f518cf5937a5450e83ccd899942e92da, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            float4 _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HairColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_31c5a05f11274ae9bd3632cacc009d46;
            float4 _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(2), _Color_31c5a05f11274ae9bd3632cacc009d46, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4);
            float4 _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4, _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_7b678612ec264b188ef510d576ffe94f;
            float4 _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_7b678612ec264b188ef510d576ffe94f, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4);
            float4 _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4, _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            UnityTexture2D _Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mouth);
            float _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float = _MouthIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6;
            _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6.uv0 = IN.uv0;
            float4 _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4;
            float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D, _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float, _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float, 0, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            float4 _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_804628686c844668827fde54bc493cdd;
            float4 _Color_804628686c844668827fde54bc493cdd_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_804628686c844668827fde54bc493cdd, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_MaskColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_339c4f9d98f74ff4ba66717b984f0f61;
            float4 _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4.xyz), _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4, float(1), _Color_339c4f9d98f74ff4ba66717b984f0f61, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4);
            float4 _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            float4 _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4);
            float4 _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HatColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_1c3463bac1454eeb84873f9ec01099a4;
            float4 _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4.xyz), _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4, float(1), _Color_1c3463bac1454eeb84873f9ec01099a4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4);
            float4 _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            float4 _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4);
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.BaseColor = (_Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4.xyz);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.SpriteMask = IsGammaSpace() ? float4(1, 1, 1, 1) : float4 (SRGBToLinear(float3(1, 1, 1)), 1);
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteLitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Sprite Normal"
            Tags
            {
                "LightMode" = "NormalsRendering"
            }
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITENORMAL
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float3 normalWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
        Out = A * B;
        }
        
        void Unity_ReplaceColor_float(float3 In, float3 From, float3 To, float Range, out float3 Out, float Fuzziness)
        {
            float Distance = distance(From, In);
            Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 1e-5f)));
        }
        
        void Unity_ColorspaceConversion_RGB_HSV_float(float3 In, out float3 Out)
        {
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float  E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            Out = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        }
        
        void Unity_Modulo_float(float A, float B, out float Out)
        {
            Out = fmod(A, B);
        }
        
        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
        Out = A * B;
        }
        
        struct Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float
        {
        };
        
        void SG_Color_273f886ee6f0c3844bf36127a8973ed7_float(float3 _Vector3, float4 _Color, float _Dark, Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float IN, out float4 New_0)
        {
        float3 _Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3 = _Vector3;
        float4 _Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4 = _Color;
        float _Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float = _Dark;
        float4 _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxxx), _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4);
        float3 _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3;
        Unity_ReplaceColor_float(_Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3, IsGammaSpace() ? float3(0.7843138, 0.7843138, 0.7843138) : SRGBToLinear(float3(0.7843138, 0.7843138, 0.7843138)), (_Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4.xyz), float(0.01), _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, float(0));
        float4 _Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4 = _Color;
        float3 _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3;
        Unity_ColorspaceConversion_RGB_HSV_float((_Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4.xyz), _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3);
        float _Split_92969d9d4ec1471882addef057112aae_R_1_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[0];
        float _Split_92969d9d4ec1471882addef057112aae_G_2_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[1];
        float _Split_92969d9d4ec1471882addef057112aae_B_3_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[2];
        float _Split_92969d9d4ec1471882addef057112aae_A_4_Float = 0;
        float _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float = float(0.85);
        float _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float;
        Unity_Add_float(_Split_92969d9d4ec1471882addef057112aae_R_1_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float);
        float _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float;
        Unity_Modulo_float(_Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float, float(1), _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float);
        float _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean);
        float3 _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3 = float3(float(0.46), float(0.65), float(0.74));
        float3 _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3 = float3(float(0.78), float(0.77), float(0.72));
        float3 _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3;
        Unity_Branch_float3(_Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean, _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3, _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3, _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3);
        float3 _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4.xyz), _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3, _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3);
        float3 _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxx), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3);
        float3 _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3;
        Unity_ReplaceColor_float(_ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, IsGammaSpace() ? float3(0.7137255, 0.7137255, 0.7137255) : SRGBToLinear(float3(0.7137255, 0.7137255, 0.7137255)), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3, float(0.01), _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, float(0));
        New_0 = (float4(_ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, 1.0));
        }
        
        void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = lerp(Base, Blend, Opacity);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mask);
            float _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e;
            _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e.uv0 = IN.uv0;
            float4 _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4;
            float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D, _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float, float(0), 0, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5;
            float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float4 _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_SkinColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_853ed1e076184a5c9b1cb51ad54e1df3;
            float4 _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.xyz), _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4, float(1), _Color_853ed1e076184a5c9b1cb51ad54e1df3, _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4);
            UnityTexture2D _Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Paintings);
            float _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float = _PaintingsIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_542310d721c844a1b5a30a99e3acf826;
            _GetTexture2D_542310d721c844a1b5a30a99e3acf826.uv0 = IN.uv0;
            float4 _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4;
            float _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D, _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float, _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float, 0, _GetTexture2D_542310d721c844a1b5a30a99e3acf826, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            float4 _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            UnityTexture2D _Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Eyes);
            float _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float = _EyesIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_f518cf5937a5450e83ccd899942e92da;
            _GetTexture2D_f518cf5937a5450e83ccd899942e92da.uv0 = IN.uv0;
            float4 _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4;
            float _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D, _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float, _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float, 0, _GetTexture2D_f518cf5937a5450e83ccd899942e92da, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            float4 _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HairColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_31c5a05f11274ae9bd3632cacc009d46;
            float4 _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(2), _Color_31c5a05f11274ae9bd3632cacc009d46, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4);
            float4 _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4, _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_7b678612ec264b188ef510d576ffe94f;
            float4 _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_7b678612ec264b188ef510d576ffe94f, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4);
            float4 _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4, _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            UnityTexture2D _Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mouth);
            float _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float = _MouthIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6;
            _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6.uv0 = IN.uv0;
            float4 _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4;
            float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D, _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float, _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float, 0, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            float4 _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_804628686c844668827fde54bc493cdd;
            float4 _Color_804628686c844668827fde54bc493cdd_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_804628686c844668827fde54bc493cdd, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_MaskColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_339c4f9d98f74ff4ba66717b984f0f61;
            float4 _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4.xyz), _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4, float(1), _Color_339c4f9d98f74ff4ba66717b984f0f61, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4);
            float4 _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            float4 _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4);
            float4 _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HatColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_1c3463bac1454eeb84873f9ec01099a4;
            float4 _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4.xyz), _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4, float(1), _Color_1c3463bac1454eeb84873f9ec01099a4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4);
            float4 _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            float4 _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4);
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.BaseColor = (_Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4.xyz);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteNormalPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
           
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        
        #define _ALPHATEST_ON 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Sprite Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
            
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITEFORWARD
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
        Out = A * B;
        }
        
        void Unity_ReplaceColor_float(float3 In, float3 From, float3 To, float Range, out float3 Out, float Fuzziness)
        {
            float Distance = distance(From, In);
            Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 1e-5f)));
        }
        
        void Unity_ColorspaceConversion_RGB_HSV_float(float3 In, out float3 Out)
        {
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float  E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            Out = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        }
        
        void Unity_Modulo_float(float A, float B, out float Out)
        {
            Out = fmod(A, B);
        }
        
        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
        Out = A * B;
        }
        
        struct Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float
        {
        };
        
        void SG_Color_273f886ee6f0c3844bf36127a8973ed7_float(float3 _Vector3, float4 _Color, float _Dark, Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float IN, out float4 New_0)
        {
        float3 _Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3 = _Vector3;
        float4 _Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4 = _Color;
        float _Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float = _Dark;
        float4 _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxxx), _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4);
        float3 _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3;
        Unity_ReplaceColor_float(_Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3, IsGammaSpace() ? float3(0.7843138, 0.7843138, 0.7843138) : SRGBToLinear(float3(0.7843138, 0.7843138, 0.7843138)), (_Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4.xyz), float(0.01), _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, float(0));
        float4 _Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4 = _Color;
        float3 _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3;
        Unity_ColorspaceConversion_RGB_HSV_float((_Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4.xyz), _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3);
        float _Split_92969d9d4ec1471882addef057112aae_R_1_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[0];
        float _Split_92969d9d4ec1471882addef057112aae_G_2_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[1];
        float _Split_92969d9d4ec1471882addef057112aae_B_3_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[2];
        float _Split_92969d9d4ec1471882addef057112aae_A_4_Float = 0;
        float _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float = float(0.85);
        float _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float;
        Unity_Add_float(_Split_92969d9d4ec1471882addef057112aae_R_1_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float);
        float _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float;
        Unity_Modulo_float(_Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float, float(1), _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float);
        float _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean);
        float3 _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3 = float3(float(0.46), float(0.65), float(0.74));
        float3 _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3 = float3(float(0.78), float(0.77), float(0.72));
        float3 _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3;
        Unity_Branch_float3(_Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean, _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3, _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3, _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3);
        float3 _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4.xyz), _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3, _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3);
        float3 _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxx), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3);
        float3 _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3;
        Unity_ReplaceColor_float(_ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, IsGammaSpace() ? float3(0.7137255, 0.7137255, 0.7137255) : SRGBToLinear(float3(0.7137255, 0.7137255, 0.7137255)), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3, float(0.01), _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, float(0));
        New_0 = (float4(_ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, 1.0));
        }
        
        void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = lerp(Base, Blend, Opacity);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mask);
            float _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e;
            _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e.uv0 = IN.uv0;
            float4 _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4;
            float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D, _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float, float(0), 0, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5;
            float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float4 _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_SkinColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_853ed1e076184a5c9b1cb51ad54e1df3;
            float4 _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.xyz), _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4, float(1), _Color_853ed1e076184a5c9b1cb51ad54e1df3, _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4);
            UnityTexture2D _Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Paintings);
            float _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float = _PaintingsIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_542310d721c844a1b5a30a99e3acf826;
            _GetTexture2D_542310d721c844a1b5a30a99e3acf826.uv0 = IN.uv0;
            float4 _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4;
            float _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D, _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float, _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float, 0, _GetTexture2D_542310d721c844a1b5a30a99e3acf826, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            float4 _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            UnityTexture2D _Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Eyes);
            float _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float = _EyesIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_f518cf5937a5450e83ccd899942e92da;
            _GetTexture2D_f518cf5937a5450e83ccd899942e92da.uv0 = IN.uv0;
            float4 _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4;
            float _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D, _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float, _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float, 0, _GetTexture2D_f518cf5937a5450e83ccd899942e92da, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            float4 _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HairColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_31c5a05f11274ae9bd3632cacc009d46;
            float4 _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(2), _Color_31c5a05f11274ae9bd3632cacc009d46, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4);
            float4 _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4, _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_7b678612ec264b188ef510d576ffe94f;
            float4 _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_7b678612ec264b188ef510d576ffe94f, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4);
            float4 _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4, _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            UnityTexture2D _Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mouth);
            float _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float = _MouthIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6;
            _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6.uv0 = IN.uv0;
            float4 _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4;
            float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D, _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float, _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float, 0, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            float4 _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_804628686c844668827fde54bc493cdd;
            float4 _Color_804628686c844668827fde54bc493cdd_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_804628686c844668827fde54bc493cdd, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_MaskColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_339c4f9d98f74ff4ba66717b984f0f61;
            float4 _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4.xyz), _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4, float(1), _Color_339c4f9d98f74ff4ba66717b984f0f61, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4);
            float4 _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            float4 _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4);
            float4 _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HatColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_1c3463bac1454eeb84873f9ec01099a4;
            float4 _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4.xyz), _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4, float(1), _Color_1c3463bac1454eeb84873f9ec01099a4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4);
            float4 _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            float4 _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4);
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.BaseColor = (_Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4.xyz);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            // RenderPipeline: <None>
            "RenderType"="Transparent"
            "BuiltInMaterialType" = "Unlit"
            "Queue"="Transparent"
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="BuiltInUnlitSubTarget"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                "LightMode" = "ForwardBase"
            }
        
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        // Render State
        Cull [_BUILTIN_CullMode]
        Blend [_BUILTIN_SrcBlend] [_BUILTIN_DstBlend]
        ZTest [_BUILTIN_ZTest]
        ZWrite [_BUILTIN_ZWrite]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile_fwdbase
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHAPREMULTIPLY_ON
        #pragma shader_feature_local_fragment _ _BUILTIN_AlphaClip
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define BUILTIN_TARGET_API 1
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
        Out = A * B;
        }
        
        void Unity_ReplaceColor_float(float3 In, float3 From, float3 To, float Range, out float3 Out, float Fuzziness)
        {
            float Distance = distance(From, In);
            Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 1e-5f)));
        }
        
        void Unity_ColorspaceConversion_RGB_HSV_float(float3 In, out float3 Out)
        {
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
            float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
            float D = Q.x - min(Q.w, Q.y);
            float  E = 1e-10;
            float V = (D == 0) ? Q.x : (Q.x + E);
            Out = float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), V);
        }
        
        void Unity_Modulo_float(float A, float B, out float Out)
        {
            Out = fmod(A, B);
        }
        
        void Unity_Branch_float3(float Predicate, float3 True, float3 False, out float3 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
        Out = A * B;
        }
        
        struct Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float
        {
        };
        
        void SG_Color_273f886ee6f0c3844bf36127a8973ed7_float(float3 _Vector3, float4 _Color, float _Dark, Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float IN, out float4 New_0)
        {
        float3 _Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3 = _Vector3;
        float4 _Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4 = _Color;
        float _Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float = _Dark;
        float4 _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4;
        Unity_Multiply_float4_float4(_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxxx), _Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4);
        float3 _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3;
        Unity_ReplaceColor_float(_Property_a154734073c0448f8ab8aa175c17e84f_Out_0_Vector3, IsGammaSpace() ? float3(0.7843138, 0.7843138, 0.7843138) : SRGBToLinear(float3(0.7843138, 0.7843138, 0.7843138)), (_Multiply_790e626bee4447708bfb1a9aea1db164_Out_2_Vector4.xyz), float(0.01), _ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, float(0));
        float4 _Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4 = _Color;
        float3 _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3;
        Unity_ColorspaceConversion_RGB_HSV_float((_Property_4b7de95309f047b1b7514f2aab134f0d_Out_0_Vector4.xyz), _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3);
        float _Split_92969d9d4ec1471882addef057112aae_R_1_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[0];
        float _Split_92969d9d4ec1471882addef057112aae_G_2_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[1];
        float _Split_92969d9d4ec1471882addef057112aae_B_3_Float = _ColorspaceConversion_5b5b9f0d08c341aab845c7d432675a4c_Out_1_Vector3[2];
        float _Split_92969d9d4ec1471882addef057112aae_A_4_Float = 0;
        float _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float = float(0.85);
        float _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float;
        Unity_Add_float(_Split_92969d9d4ec1471882addef057112aae_R_1_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float);
        float _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float;
        Unity_Modulo_float(_Add_ea8e8c906e7f47ab934e9e0cdb1f8e81_Out_2_Float, float(1), _Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float);
        float _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Modulo_d2c9a5f70e96429eadc4a70d061b5584_Out_2_Float, _Float_621c6c6329ef4aa5afb6f829f65e561f_Out_0_Float, _Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean);
        float3 _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3 = float3(float(0.46), float(0.65), float(0.74));
        float3 _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3 = float3(float(0.78), float(0.77), float(0.72));
        float3 _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3;
        Unity_Branch_float3(_Comparison_4afa203839a24a11aedb5d7a8993ff8c_Out_2_Boolean, _Vector3_b777aa2fc12a47bead1a7a94e5c3f67c_Out_0_Vector3, _Vector3_d4074e6f711e4a9a99e518e1bdbf78d5_Out_0_Vector3, _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3);
        float3 _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3;
        Unity_Multiply_float3_float3((_Property_f312dab958654d4dbba95701d015244b_Out_0_Vector4.xyz), _Branch_33dbb76f4faa455c8c8dd893b4fca534_Out_3_Vector3, _Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3);
        float3 _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3;
        Unity_Multiply_float3_float3(_Multiply_8165d0cd6eec49808fbc3f0d95077982_Out_2_Vector3, (_Property_464bc67d13cf439a80f51f9ddc51fddf_Out_0_Float.xxx), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3);
        float3 _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3;
        Unity_ReplaceColor_float(_ReplaceColor_0bfd24c458974f86a578b7aa14d835cb_Out_4_Vector3, IsGammaSpace() ? float3(0.7137255, 0.7137255, 0.7137255) : SRGBToLinear(float3(0.7137255, 0.7137255, 0.7137255)), _Multiply_a444600ddb404b348ccc98b68a37f98e_Out_2_Vector3, float(0.01), _ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, float(0));
        New_0 = (float4(_ReplaceColor_1b22b271c9574a2a9e81bc0562efe7ee_Out_4_Vector3, 1.0));
        }
        
        void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
        {
            Out = lerp(Base, Blend, Opacity);
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mask);
            float _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e;
            _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e.uv0 = IN.uv0;
            float4 _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4;
            float _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_253af47afad04a669547b092440f56c7_Out_0_Texture2D, _Property_50a5be2788004860b86f3b8bee6f21aa_Out_0_Float, float(0), 0, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5;
            float _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5, _IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float4 _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_SkinColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_853ed1e076184a5c9b1cb51ad54e1df3;
            float4 _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.xyz), _Property_b48334dbc08342b2b3700e1f2e235672_Out_0_Vector4, float(1), _Color_853ed1e076184a5c9b1cb51ad54e1df3, _Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4);
            UnityTexture2D _Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Paintings);
            float _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float = _PaintingsIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_542310d721c844a1b5a30a99e3acf826;
            _GetTexture2D_542310d721c844a1b5a30a99e3acf826.uv0 = IN.uv0;
            float4 _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4;
            float _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_90fab63cf3cb4850a23db994f7f67780_Out_0_Texture2D, _Property_4f825cffceb74f0d85539ba2f4a497dc_Out_0_Float, _Property_cfd639bc57ac4de6bc4a0e1d600bd218_Out_0_Float, 0, _GetTexture2D_542310d721c844a1b5a30a99e3acf826, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            float4 _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Color_853ed1e076184a5c9b1cb51ad54e1df3_New_0_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_RGBA_1_Vector4, _Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _GetTexture2D_542310d721c844a1b5a30a99e3acf826_A_2_Float);
            UnityTexture2D _Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Eyes);
            float _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float = _EyesIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_f518cf5937a5450e83ccd899942e92da;
            _GetTexture2D_f518cf5937a5450e83ccd899942e92da.uv0 = IN.uv0;
            float4 _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4;
            float _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_5a09a58a0ede4f979fd915dd9e0f86ee_Out_0_Texture2D, _Property_61da8722389448ea8d2830c4dace098d_Out_0_Float, _Property_63957ee6c38d4a62ac2b9ee3efdfb48b_Out_0_Float, 0, _GetTexture2D_f518cf5937a5450e83ccd899942e92da, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            float4 _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HairColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_31c5a05f11274ae9bd3632cacc009d46;
            float4 _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_f518cf5937a5450e83ccd899942e92da_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(2), _Color_31c5a05f11274ae9bd3632cacc009d46, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4);
            float4 _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_d42b469339824a9ebf2707bcea4f0d1a_Out_2_Vector4, _Color_31c5a05f11274ae9bd3632cacc009d46_New_0_Vector4, _Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _GetTexture2D_f518cf5937a5450e83ccd899942e92da_A_2_Float);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_7b678612ec264b188ef510d576ffe94f;
            float4 _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_7b678612ec264b188ef510d576ffe94f, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4);
            float4 _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_3a57c67926b946feb86df4aff28f981c_Out_2_Vector4, _Color_7b678612ec264b188ef510d576ffe94f_New_0_Vector4, _Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            UnityTexture2D _Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Mouth);
            float _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float = _MouthIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6;
            _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6.uv0 = IN.uv0;
            float4 _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4;
            float _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_b6122328c84340eaadc93dfa4ce1343b_Out_0_Texture2D, _Property_44ad822d5cb04bb8a7b948ac68c9c6cf_Out_0_Float, _Property_a756fa916e8f443fa5ecded9a1dabf5d_Out_0_Float, 0, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            float4 _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_a603160ec3d34aaf9c7b62e4ef31c1f1_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_RGBA_1_Vector4, _Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _GetTexture2D_0f88998cc3b14c29aee6171c4dff36f6_A_2_Float);
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_804628686c844668827fde54bc493cdd;
            float4 _Color_804628686c844668827fde54bc493cdd_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4.xyz), _Property_73dcef327853415f942f83cf9b9bbf8a_Out_0_Vector4, float(1), _Color_804628686c844668827fde54bc493cdd, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_e332ae269db84bc6b5fb7b730cf2e15a_Out_2_Vector4, _Color_804628686c844668827fde54bc493cdd_New_0_Vector4, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            float4 _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_MaskColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_339c4f9d98f74ff4ba66717b984f0f61;
            float4 _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_RGBA_1_Vector4.xyz), _Property_c0819032af5b4493a6cbb81bb3ba25d3_Out_0_Vector4, float(1), _Color_339c4f9d98f74ff4ba66717b984f0f61, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4);
            float4 _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Color_339c4f9d98f74ff4ba66717b984f0f61_New_0_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _GetTexture2D_ec72fd101df44f9abbafc0ddbcf3994e_A_2_Float);
            float4 _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_6d62631fe25c483ab8deb3fd696bfdd5_IsNULL_1_Boolean, _Blend_c3a82828c234491a8f79a97447808596_Out_2_Vector4, _Blend_57a478150da9467ea9f594aef1b8ee92_Out_2_Vector4, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4);
            float4 _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4 = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_HatColor, float4);
            Bindings_Color_273f886ee6f0c3844bf36127a8973ed7_float _Color_1c3463bac1454eeb84873f9ec01099a4;
            float4 _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4;
            SG_Color_273f886ee6f0c3844bf36127a8973ed7_float((_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4.xyz), _Property_7eb807f6046c44219b52d49398b381ee_Out_0_Vector4, float(1), _Color_1c3463bac1454eeb84873f9ec01099a4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4);
            float4 _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4;
            Unity_Blend_Overwrite_float4(_Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Color_1c3463bac1454eeb84873f9ec01099a4_New_0_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            float4 _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4;
            Unity_Branch_float4(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Branch_2335d392a95245f4878f72bc17270c1f_Out_3_Vector4, _Blend_a0afedc58a3140e4903363bb355ee2c8_Out_2_Vector4, _Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4);
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.BaseColor = (_Branch_24e853ea35ca4d379a25377b3a888686_Out_3_Vector4.xyz);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        
        // Render State
        Cull [_BUILTIN_CullMode]
        Blend [_BUILTIN_SrcBlend] [_BUILTIN_DstBlend]
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _BUILTIN_AlphaClip
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define BUILTIN_TARGET_API 1
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
            
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]

        // Render State
        Cull [_BUILTIN_CullMode]
        Blend [_BUILTIN_SrcBlend] [_BUILTIN_DstBlend]
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_shadowcaster
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        #pragma shader_feature_local_fragment _ _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #pragma shader_feature_local_fragment _ _BUILTIN_AlphaClip
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        #define BUILTIN_TARGET_API 1
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #pragma shader_feature_local_fragment _ _BUILTIN_AlphaClip
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SceneSelectionPass
        #define BUILTIN_TARGET_API 1
        #define SCENESELECTIONPASS 1
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            ColorMask [_ColorMask]
        // Render State
        Cull [_BUILTIN_CullMode]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #pragma shader_feature_local_fragment _ _BUILTIN_AlphaClip
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS ScenePickingPass
        #define BUILTIN_TARGET_API 1
        #define SCENEPICKINGPASS 1
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _HairIndex;
        float4 _SkinColor;
        float4 _HairColor;
        float _Direction;
        float _EyesIndex;
        float _BeardIndex;
        float _MouthIndex;
        float _PaintingsIndex;
        float4 _Mask_TexelSize;
        float4 _HatColor;
        float4 _MaskColor;
        float4 _Hat_TexelSize;
        float _HairUnderHat;
        float4 _Hairs_TexelSize;
        float4 _Mouth_TexelSize;
        float4 _Beard_TexelSize;
        float4 _Paintings_TexelSize;
        float4 _Eyes_TexelSize;
        float4 _MainTex_TexelSize;
        float4 _CutMask_TexelSize;
        CBUFFER_END
        
        #if defined(DOTS_INSTANCING_ON)
        // DOTS instancing definitions
        UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _SkinColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HairColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Direction)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _HatColor)
            UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float4, _MaskColor)
        UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        // DOTS instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
        #elif defined(UNITY_INSTANCING_ENABLED)
        // Unity instancing definitions
        UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SkinColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HairColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _Direction)
            UNITY_DEFINE_INSTANCED_PROP(float4, _HatColor)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MaskColor)
        UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
        // Unity instancing usage macros
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
        #else
        #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
        #endif
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Mask);
        SAMPLER(sampler_Mask);
        TEXTURE2D(_Hat);
        SAMPLER(sampler_Hat);
        TEXTURE2D(_Hairs);
        SAMPLER(sampler_Hairs);
        TEXTURE2D(_Mouth);
        SAMPLER(sampler_Mouth);
        TEXTURE2D(_Beard);
        SAMPLER(sampler_Beard);
        TEXTURE2D(_Paintings);
        SAMPLER(sampler_Paintings);
        TEXTURE2D(_Eyes);
        SAMPLER(sampler_Eyes);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_CutMask);
        SAMPLER(sampler_CutMask);
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        void Unity_Divide_float(float A, float B, out float Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
        Out = A * B;
        }
        
        struct Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float
        {
        };
        
        void SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(float _Direction, float _Index, UnityTexture2D _Texture2D, float _Width, float _Height, float _Float, Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float IN, out float2 Tiling_2, out float2 Offset_1)
        {
        float _Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float = _Float;
        UnityTexture2D _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D = _Texture2D;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.z;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.w;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelWidth_3_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.x;
        float _TextureSize_5977670a53204a0f9b9af39e09f33ec2_TexelHeight_4_Float = _Property_4596129f248f43788f8fa506f30583c9_Out_0_Texture2D.texelSize.y;
        float _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Height_2_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float);
        float _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float = _Height;
        float _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float;
        Unity_Multiply_float_float(_Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Property_f66f738f6cef48f09efa9fb1d0208d44_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float2 _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2 = float2(_Property_f0bb64b357394b1c94534db8a0889086_Out_0_Float, _Multiply_227eee30f6ca4e84bb4a3c96741830f8_Out_2_Float);
        float _Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float = _Width;
        float _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float;
        Unity_Divide_float(float(1), _TextureSize_5977670a53204a0f9b9af39e09f33ec2_Width_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float);
        float _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float;
        Unity_Multiply_float_float(_Property_c05ee89a5fd547159726d7f60a72f59c_Out_0_Float, _Divide_1d7d7c86172147c394f89bf0d86d2185_Out_2_Float, _Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float);
        float _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float = _Direction;
        float _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_0bc5c3f8ce6c4e3686dd6438d343551c_Out_2_Float, _Property_40807ba49a534e9dbfc26b096e7ce3aa_Out_0_Float, _Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float);
        float _Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float = _Height;
        float _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float;
        Unity_Multiply_float_float(_Property_5ad0adc8b183487c9a89cf82c9fb7fc2_Out_0_Float, _Divide_9806f7ab86e24badbff29a653750053f_Out_2_Float, _Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float);
        float _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float = _Index;
        float _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float;
        Unity_Multiply_float_float(_Multiply_340be539f7f74d0ba39dbc2546b6f16c_Out_2_Float, _Property_1d073ca6e54d47fbb78983723f92c427_Out_0_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        float2 _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2 = float2(_Multiply_ad8237cee9f5429eaf24e3d85d7559a4_Out_2_Float, _Multiply_9e571d194d714acaaa7834c1b2b514a5_Out_2_Float);
        Tiling_2 = _Vector2_fcdf47d3ae7f4c3993b55f755bee0bc6_Out_0_Vector2;
        Offset_1 = _Vector2_ee42452e2a8b4da0bd83357ad29116fd_Out_0_Vector2;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        struct Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float
        {
        half4 uv0;
        };
        
        void SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(UnityTexture2D _Texture2D, float _Direction, float _Index, float _Body, Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float IN, out float4 RGBA_1, out float A_2)
        {
        UnityTexture2D _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D = _Texture2D;
        float _Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float = _Direction;
        float _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float = _Index;
        float _Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean = _Body;
        float _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float;
        Unity_Branch_float(_Property_87021fdd34b14223a50df62ebd92882c_Out_0_Boolean, float(1), float(0.5), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float);
        Bindings_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float _GetOffset_a342e12568da4aaaaf0d71ba7f05f371;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2;
        half2 _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2;
        SG_GetOffset_e67a1ceaa9c9b6c48a0b5c0bb23f03b6_float(_Property_f238ad6ae92d4ce6b989f93af1613bfa_Out_0_Float, _Property_6cb07211ba6a4d209eb9d0dcdf172ac8_Out_0_Float, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D, half(21), half(21), _Branch_badcac7aa8c74671ac2f74b60193a05e_Out_3_Float, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2);
        float2 _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2;
        Unity_TilingAndOffset_float(IN.uv0.xy, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Tiling_2_Vector2, _GetOffset_a342e12568da4aaaaf0d71ba7f05f371_Offset_1_Vector2, _TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2);
        float4 _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.tex, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.samplerstate, _Property_0b3f8b76721040b98374b6265f1b9502_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_855010585dce433ea4fe81fe1e2f2a6e_Out_3_Vector2) );
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_R_4_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.r;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_G_5_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.g;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_B_6_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.b;
        float _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4.a;
        RGBA_1 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_RGBA_0_Vector4;
        A_2 = _SampleTexture2D_f621d448bbcc4ecf973038410c6af18e_A_7_Float;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Comparison_Equal_float(float A, float B, out float Out)
        {
            Out = A == B ? 1 : 0;
        }
        
        struct Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float
        {
        };
        
        void SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(float4 _Vector4, Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float IN, out float IsNULL_1)
        {
        float4 _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4 = _Vector4;
        float _Split_58193fcd1803443f9c782106bb121a14_R_1_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[0];
        float _Split_58193fcd1803443f9c782106bb121a14_G_2_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[1];
        float _Split_58193fcd1803443f9c782106bb121a14_B_3_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[2];
        float _Split_58193fcd1803443f9c782106bb121a14_A_4_Float = _Property_fc637e33e1f94d89b53b96c94bf13060_Out_0_Vector4[3];
        float _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_R_1_Float, _Split_58193fcd1803443f9c782106bb121a14_G_2_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float);
        float _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float;
        Unity_Add_float(_Split_58193fcd1803443f9c782106bb121a14_B_3_Float, _Add_db77f456ef0446f7a4c4e4d015027060_Out_2_Float, _Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float);
        float _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float;
        Unity_Divide_float(_Add_1f5f64ac415b4be2935c3622f28015d2_Out_2_Float, float(3), _Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float);
        float _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        Unity_Comparison_Equal_float(_Divide_1766cbb27c1f47e0ac1d862efb3fc3c4_Out_2_Float, float(1), _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean);
        IsNULL_1 = _Comparison_4dffd6bb2e35439aad7a043be18d63d9_Out_2_Boolean;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Clamp_float(float In, float Min, float Max, out float Out)
        {
            Out = clamp(In, Min, Max);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hat);
            float _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac;
            _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac.uv0 = IN.uv0;
            float4 _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4;
            float _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_f13dbb5a2736472ca6c52d7d005c0b83_Out_0_Texture2D, _Property_863dd02536384f31b3070ac8dd0dc484_Out_0_Float, float(0), 0, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float);
            Bindings_IsNull_2b310c125772c9a4f8c522c7718584ab_float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4;
            float _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean;
            SG_IsNull_2b310c125772c9a4f8c522c7718584ab_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_RGBA_1_Vector4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4, _IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean);
            UnityTexture2D _Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Beard);
            float _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float = _BeardIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc;
            _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc.uv0 = IN.uv0;
            float4 _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4;
            float _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_79f31b87875b4d1998d80da43366fbe0_Out_0_Texture2D, _Property_ef6e02de6ea549e48a837b277035a780_Out_0_Float, _Property_81e231b202c248cf8aa63058ba555fea_Out_0_Float, 0, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_RGBA_1_Vector4, _GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float);
            float _Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean = _HairUnderHat;
            UnityTexture2D _Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_Hairs);
            float _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float = _HairIndex;
            Bindings_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float _GetTexture2D_8ddd215fa65346779ebf0072e57238de;
            _GetTexture2D_8ddd215fa65346779ebf0072e57238de.uv0 = IN.uv0;
            float4 _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4;
            float _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float;
            SG_GetTexture2D_7e26b80b1ce9d994080f9be2a342ab41_float(_Property_94aec511d8c24091a3cf7b0b184d6f66_Out_0_Texture2D, _Property_e19841bed85644cd81e25a08657908d9_Out_0_Float, _Property_f459ba80be224def85570ee3b8f006e0_Out_0_Float, 0, _GetTexture2D_8ddd215fa65346779ebf0072e57238de, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_RGBA_1_Vector4, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float);
            UnityTexture2D _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_CutMask);
            float4 _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.tex, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.samplerstate, _Property_9d84127d0b1a4ed3bf5e2766d63e489d_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_R_4_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.r;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_G_5_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.g;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_B_6_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.b;
            float _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float = _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_RGBA_0_Vector4.a;
            float _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float;
            Unity_Subtract_float(_GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _SampleTexture2D_2d3b75146b134d128baacd0c46a6ebff_A_7_Float, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float);
            float _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float;
            Unity_Branch_float(_Property_876dd0519f3b467fa8694a0ca2659d1c_Out_0_Boolean, _Subtract_63d12b53053f47239f5d2651f2928985_Out_2_Float, _GetTexture2D_8ddd215fa65346779ebf0072e57238de_A_2_Float, _Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float);
            UnityTexture2D _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float _Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float = UNITY_ACCESS_HYBRID_INSTANCED_PROP(_Direction, float);
            float _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Property_7af187b4f17346cbbbb3c227c488ecea_Out_0_Float, float(1), _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean);
            float _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float;
            Unity_Multiply_float_float(((float) _Comparison_889b1bfa060c40f385045ed9a8f94476_Out_2_Boolean), 0.5, _Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float);
            float2 _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2 = float2(_Multiply_e04b179ffcfc4f68b2f5e88089521c88_Out_2_Float, float(0));
            float2 _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_b54f82335189476eb8c00afde2af0b5c_Out_0_Vector2, _TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2);
            float4 _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.tex, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.samplerstate, _Property_dac46e44748d496aac5b29d76954b1dd_Out_0_Texture2D.GetTransformedUV(_TilingAndOffset_8b9de4f1279f47e6b4445cf02c1d2b6c_Out_3_Vector2) );
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_R_4_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.r;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_G_5_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.g;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_B_6_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.b;
            float _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float = _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_RGBA_0_Vector4.a;
            float _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float;
            Unity_Add_float(_Branch_200d1fdbb04945ddaa2bc5e8f4b80b09_Out_3_Float, _SampleTexture2D_4f696ab984d345bd95d25b9c7bba0c9d_A_7_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float);
            float _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float;
            Unity_Add_float(_GetTexture2D_34ba96f1cc4345838b51cd37e6c51bfc_A_2_Float, _Add_f4b76a9e96bb43419a3921cd280e71a6_Out_2_Float, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float);
            float _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float;
            Unity_Preview_float(_GetTexture2D_20e8973e093f4ef7bdc4d831247bd1ac_A_2_Float, _Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float);
            float _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float;
            Unity_Clamp_float(_Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, float(0), float(1), _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float);
            float _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float;
            Unity_Add_float(_Preview_53985dd346a645a6bbb259bb495ff3a9_Out_1_Float, _Clamp_e9591e3023804d70b362b6a3254f07ee_Out_3_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float);
            float _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float;
            Unity_Branch_float(_IsNull_b7c9833fb53f40e3a56ffb437ad5b5d4_IsNULL_1_Boolean, _Add_c5c7b53f4d5d4ec8a5b54c350ba2a9db_Out_2_Float, _Add_aba1d0502f5e44529f0a2e2e037758f2_Out_2_Float, _Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float);
            float _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            Unity_Clamp_float(_Branch_45347ee959764a2da611c5bfee88c1e3_Out_3_Float, float(0), float(1), _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float);
            surface.Alpha = _Clamp_e8d88c9eac314216b3ccf7f91cfca954_Out_3_Float;
            surface.AlphaClipThreshold = float(0.5);
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInUnlitGUI" ""
    FallBack "Hidden/Shader Graph/FallbackError"
}