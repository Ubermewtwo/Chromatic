Shader "CustomShaders/DiscardMask"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Distance ("Distance", Range(0, 1)) = 1
        [Toggle] _DiscardFragments ("Discard Pixels", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalRenderPipeline"
        }
        LOD 100

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "ShaderSource/MaskInput.hlsl"

            #define UNITY_PI 3.14159265359f
            #define UNITY_TWO_PI 6.28318530718f
        ENDHLSL

        Pass
        {
            Name "MaskingPass"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex VSMain
            #pragma fragment PSMain

            #include "ShaderSource/MaskForwardPass.hlsl"
            ENDHLSL
        }
    }
}
