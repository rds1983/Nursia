//-----------------------------------------------------------------------------
// BasicEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"

#ifdef TEXTURE

DECLARE_TEXTURE(_texture, 0);

#endif

BEGIN_CONSTANTS

#ifdef LIGHTNING

float3 _lightDir;
float3 _lightColor;

#endif

float4 _diffuseColor;

MATRIX_CONSTANTS

float4x4 _world;
float4x4 _worldViewProj;
float3x3 _worldInverseTranspose;

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
#ifdef TEXTURE	
    float2 TexCoord : TEXCOORD0;
#endif

#ifdef LIGHTNING
	float3 WorldPosition: TEXCOORD1;
	float3 WorldNormal: TEXCOORD2;
#endif
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;

    output.Position = mul(input.Position, _worldViewProj);
	
#ifdef TEXTURE
	output.TexCoord = input.TexCoord;
#endif

#ifdef LIGHTNING
	output.WorldPosition = mul(input.Position, _world).xyz;
	output.WorldNormal = mul(input.Normal, _worldInverseTranspose);
#endif

    return output;
}

float3 ComputeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    float3 diffuseColor = lightColor  * diffuse * attenuation;

    return diffuseColor;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
#ifdef TEXTURE
    float4 color = SAMPLE_TEXTURE(_texture, input.TexCoord);
#else
	float4 color = float4(1, 1, 1, 1);
#endif

#ifdef LIGHTNING
	float3 result = float3(0, 0, 0);
	float3 normal = normalize(input.WorldNormal);
    result += ComputeLighting(normal, -_lightDir, _lightColor, 1.0);

	return color * float4(result, 1) * _diffuseColor;
#else
	return color * _diffuseColor;
#endif
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);