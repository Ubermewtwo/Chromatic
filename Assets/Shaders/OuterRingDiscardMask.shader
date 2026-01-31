Shader "CustomShaders/OuterRingDiscardMask"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Distance ("Distance", Range(0, 1)) = 1
        [Toggle] _IsRing ("Is Ring", Float) = 1
        _Border ("Border", Range(0, 1)) = 0.1
        [Enum(R, 0, G, 1, B, 2, None, 3)] _ColorMasking ("Color Masking", Int) = 3
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
        }
        LOD 100

        Blend DstColor Zero
        ZWrite Off

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
            #pragma vertex CommonVertex
            #pragma fragment OuterRingFragment

            #include "ShaderSource/MaskForwardPass.hlsl"
            ENDHLSL
        }
    }
}
