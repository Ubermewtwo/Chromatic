#ifndef MASK_INPUT_INCLUDED
#define MASK_INPUT_INCLUDED

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float4 _MainTex_ST;
    float _Distance;
    float _Border;
    int _ColorMasking;
CBUFFER_END

struct Attributes
{
	float4 positionOS : POSITION;
	float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 unscaledUV : TEXCOORD1;
};

#endif