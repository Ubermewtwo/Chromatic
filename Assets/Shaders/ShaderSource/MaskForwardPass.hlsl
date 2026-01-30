#ifndef MASK_FORWARD_PASS_INCLUDED
#define MASK_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

Varyings VSMain(in Attributes input)
{
    Varyings output;

    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = input.uv;
    return output;
}

float4 PSMain(in Varyings input) : SV_Target
{
    float2 minusOneToOneCoords = input.uv * 2 - 1;
    float4 color = _BaseColor;
    color *= tex2D(_MainTex, input.uv);

    const float sqrtTwo = sqrt(2);

    if (_DiscardFragments)
    {
        float dist = length(minusOneToOneCoords);
        if (dist > (_Distance * sqrtTwo))
            discard;
    }
    
    return color;
}

#endif