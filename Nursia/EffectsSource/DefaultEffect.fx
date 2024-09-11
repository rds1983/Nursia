#include "Macros.fxh"

#ifdef LIGHTNING

#include "Lightning.fxh"

#endif

#ifdef SHADOWS

#include "Shadows.fxh"

#endif

#ifdef SKINNING

#define MAX_BONES 128

#endif

#ifdef TEXTURE

DECLARE_TEXTURE_LINEAR_WRAP(_texture);

#endif

#ifdef CLIP_PLANE

float4 _clipPlane;

#endif

float4 _diffuseColor;
float3 _cameraPosition;

#ifdef LIGHTNING
float3x3 _worldInverseTranspose;
#endif

#ifdef SKINNING
float4x3 _bones[MAX_BONES];
#endif

float4x4 _worldViewProj;
float4x4 _world;

struct VSInput
{
	float4 Position : POSITION0;

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
	float4 Position: POSITION0;
	float3 ToCamera: TEXCOORD0;

#ifdef TEXTURE
	float2 TexCoord: TEXCOORD1;
#endif

#ifdef LIGHTNING
	float3 WorldNormal: TEXCOORD2;
	float4 SourcePosition: TEXCOORD3;
#endif

#ifdef SHADOWS
	float4 LightPosition: TEXCOORD4;
#endif

#ifdef CLIP_PLANE
	float4 ClipDistances: TEXCOORD5;
#endif
};

VSOutput VS(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
	float3 worldPosition = mul(input.Position, _world).xyz;
	output.ToCamera = _cameraPosition - worldPosition;

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

#ifdef LIGHTNING
	output.SourcePosition = input.Position;
#endif

#ifdef SHADOWS
	output.LightPosition = mul(float4(worldPosition, 1), _lightViewProj);
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

float4 PS(VSOutput input): COLOR
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

	LightPower lightPower = CalculateLightPower(normal, src, input.ToCamera);
	color *= float4(lightPower.Diffuse, 1);
	color += float4(lightPower.Specular, 0);
#endif

#ifdef SHADOWS
	// Finish the projection
	float3 lightingPosition = input.LightPosition.xyz / input.LightPosition.w;

	float4 shadowFactor = CalculateShadowFactor(lightingPosition);
	color *= shadowFactor;
#endif

	return color;
}

TECHNIQUE(Default, VS, PS);
