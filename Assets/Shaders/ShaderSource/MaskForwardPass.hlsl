#ifndef MASK_FORWARD_PASS_INCLUDED
#define MASK_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

Varyings CommonVertex(in Attributes input)
{
	Varyings output;

	output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
	output.uv = TRANSFORM_TEX(input.uv, _MainTex);
	output.unscaledUV = input.uv;
	return output;
}

float4 OuterRingFragment(in Varyings input) : SV_Target
{
	float2 minusOneToOneCoords = input.uv * 2 - 1;
	float4 color = float4(1, 1, 1, 1);

	const float sqrtTwo = sqrt(2);
	float dist = length(minusOneToOneCoords);

	bool threshold = dist > (_Distance * sqrtTwo) || dist < ((_Distance - _Border) * sqrtTwo);
	if (threshold) discard;

	switch (_ColorMasking)
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

	return color;
}

float4 CircleMaskFragment(in Varyings input) : SV_Target
{
	float2 minusOneToOneCoords = input.uv * 2 - 1;
	float2 screenUVs = input.positionCS.xy / _ScaledScreenParams.xy;
	float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, screenUVs);

	const float sqrtTwo = sqrt(2);
	float dist = length(minusOneToOneCoords);

	bool threshold = dist > (_Distance * sqrtTwo);
	if (threshold) discard;

	return color;
}

#endif