#ifndef MASK_INPUT_INCLUDED
#define MASK_INPUT_INCLUDED

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    sampler2D _MainTex;
    float4 _MainTex_ST;
    float _Distance;
    bool _DiscardFragments;
CBUFFER_END

struct Attributes
{
	float4 positionOS : POSITION;
	float2 uv : TEXCOORD0;
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;

    float4 positionCS : SV_POSITION;
};

#endif