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
    float dist = length(minusOneToOneCoords);

    if (_IsRing)
    {
        bool threshold = dist > (_Distance * sqrtTwo) || dist < ((_Distance - _Border) * sqrtTwo);
        if (threshold) discard;
    }
    else
    {
        bool threshold = dist > (_Distance * sqrtTwo);
        if (threshold) discard;
    }
    
    if (_IsRing)
    {
        switch (_ColorMask)
        {
            case 0: // Only red
                color.gb = float2(0, 0);
                break;
            case 1: // Only green
                color.rb = float2(0, 0);
                break;
            case 2: // Only blue
                color.rg = float2(0, 0);
                break;
            default:
                break;
        }
    }
    
    return color;
}

#endif