#include "Macros.fxh"

#ifdef SKINNING

#define MAX_BONES 128

#endif

BEGIN_CONSTANTS

#ifdef CLIP_PLANE

float4 _clipPlane;

#endif

MATRIX_CONSTANTS

#ifdef SKINNING
float4x3 _bones[MAX_BONES];
#endif

float4x4 _worldViewProj;

END_CONSTANTS

struct VSInput
{
	float4 Position : POSITION0;

#ifdef SKINNING
	int4   Indices  : BLENDINDICES0;
	float4 Weights  : BLENDWEIGHT0;
#endif
};

struct VSOutput
{
	float4 Position: POSITION0;
	float4 PositionCopy: TEXCOORD0;

#ifdef CLIP_PLANE
	float4 ClipDistances: TEXCOORD1;
#endif
};

VSOutput VS(VSInput input)
{
    VSOutput output;

#ifdef SKINNING
	float4x3 skinning = 0;
	skinning += _bones[(int)input.Indices[0]] * input.Weights[0];
	skinning += _bones[(int)input.Indices[1]] * input.Weights[1];
	skinning += _bones[(int)input.Indices[2]] * input.Weights[2];
	skinning += _bones[(int)input.Indices[3]] * input.Weights[3];
	input.Position.xyz = mul(input.Position, skinning);
#endif

	output.Position = mul(input.Position, _worldViewProj);
	output.PositionCopy = output.Position;

#ifdef CLIP_PLANE
	output.ClipDistances.yzw = 0;
	output.ClipDistances.x = dot(output.Position, _clipPlane); 
#endif

	return output;
}

float4 PS(VSOutput input): COLOR
{
#ifdef CLIP_PLANE
	clip(input.ClipDistances.x); 
#endif

	float depth = input.PositionCopy.z / input.PositionCopy.w;
	return float4(depth, 0, 0, 0);
}

TECHNIQUE(Default, VS, PS);
