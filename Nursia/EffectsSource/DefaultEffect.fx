#include "Macros.fxh"

#ifdef LIGHTNING

#include "Lightning.fxh"

#endif

#ifdef SKINNING

#define MAX_BONES 128

#endif

#ifdef TEXTURE

DECLARE_TEXTURE_LINEAR_WRAP(_texture);

#endif

BEGIN_CONSTANTS

#ifdef CLIP_PLANE

float4 _clipPlane;

#endif

float4 _diffuseColor;

MATRIX_CONSTANTS

#ifdef LIGHTNING
float3x3 _worldInverseTranspose;
#endif

#ifdef SKINNING
float4x3 _bones[MAX_BONES];
#endif

float4x4 _worldViewProj;

END_CONSTANTS

struct VSInput
{
	float4 Position : SV_POSITION;

#ifdef LIGHTNING
	float3 Normal   : NORMAL;
#endif

#ifdef TEXTURE
	float2 TexCoord : TEXCOORD0;
#endif

#ifdef SKINNING
	int4   Indices  : BLENDINDICES0;
	float4 Weights  : BLENDWEIGHT0;
#endif
};

struct VSOutput
{
	float4 Position: SV_POSITION;

#ifdef TEXTURE
    float2 TexCoord: TEXCOORD0;
#endif

#ifdef LIGHTNING
	float3 WorldNormal: TEXCOORD1;
	float4 SourcePosition: TEXCOORD2;
#endif

#ifdef CLIP_PLANE
	float4 ClipDistances: TEXCOORD3;
#endif

	float2 Depth: TEXCOORD5;
};

VSOutput VS(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
#ifdef SKINNING
	float4x3 skinning = 0;
	skinning += _bones[(int)input.Indices[0]] * input.Weights[0];
	skinning += _bones[(int)input.Indices[1]] * input.Weights[1];
	skinning += _bones[(int)input.Indices[2]] * input.Weights[2];
	skinning += _bones[(int)input.Indices[3]] * input.Weights[3];
	input.Position.xyz = mul(input.Position, skinning);
	
#ifdef LIGHTNING
    input.Normal = mul(input.Normal, (float3x3)skinning);    
#endif
#endif

	output.Position = mul(input.Position, _worldViewProj);
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	
	
#ifdef LIGHTNING
	output.SourcePosition = input.Position;
#endif

#ifdef TEXTURE
	output.TexCoord = input.TexCoord;
#endif

#ifdef LIGHTNING
	output.WorldNormal = mul(input.Normal, _worldInverseTranspose);
#endif

#ifdef CLIP_PLANE
	output.ClipDistances.yzw = 0;
	output.ClipDistances.x = dot(output.Position, _clipPlane); 
#endif

	return output;
}

float4 PSColor(VSOutput input) : SV_Target0
{
#ifdef CLIP_PLANE
    clip(input.ClipDistances.x); 
#endif

#if TEXTURE
    float4 color = SAMPLE_TEXTURE(_texture, input.TexCoord) * _diffuseColor;
#else
	float4 color = _diffuseColor;
#endif

#ifdef LIGHTNING
	float3 normal = normalize(input.WorldNormal);
	float3 src = input.SourcePosition.xyz;

	float3 lightPower = CalculateLightPower(normal, src);
	color *= float4(lightPower, 1);
#endif

	clip(color.a < 0.1?-1:1);

	return color;
}

float4 PSDepth(VSOutput input) : COLOR
{
#ifdef CLIP_PLANE
	clip(input.ClipDistances.x); 
#endif

	float depth = input.Depth.x / input.Depth.y;
	return float4(depth, 0.0f, 0.0f, 1.0f);
}

TECHNIQUE(Color, VS, PSColor);
TECHNIQUE(Depth, VS, PSDepth);