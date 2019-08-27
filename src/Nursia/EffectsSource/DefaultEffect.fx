//-----------------------------------------------------------------------------
// BasicEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"

#define MAX_BONES   72

DECLARE_TEXTURE(_texture, 0);

BEGIN_CONSTANTS

#ifdef LIGHTNING

float3 _lightDir;
float3 _lightColor;

#endif

float4 _diffuseColor;

MATRIX_CONSTANTS

float4x4 _worldViewProj;
float3x3 _worldInverseTranspose;

float4x3 _bones[MAX_BONES];

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;

#ifdef BONES
    float2 Weights0: BLENDWEIGHT0;
#if BONES==2
    float2 Weights1: BLENDWEIGHT1;
#endif
#if BONES==4
    float2 Weights2: BLENDWEIGHT2;
    float2 Weights3: BLENDWEIGHT3;
#endif
#endif
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float2 TexCoord: TEXCOORD0;

#ifdef LIGHTNING
	float3 WorldNormal: TEXCOORD2;
#endif
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
#ifdef BONES
    float4x3 skinning = 0;
    float w = 0;
    skinning += _bones[(int)input.Weights0.x] * input.Weights0.y;
    w += input.Weights0.y;
    
#if BONES==2
    skinning += _bones[(int)input.Weights1.x] * input.Weights1.y;
    w += input.Weights1.y;
#endif
#if BONES==4
    skinning += _bones[(int)input.Weights2.x] * input.Weights2.y;
    skinning += _bones[(int)input.Weights3.x] * input.Weights3.y;
    w += input.Weights2.y;
    w += input.Weights3.y;
#endif    
    input.Position.xyz = mul(input.Position, skinning);
    input.Position.w = w;
#ifdef LIGHTNING
    input.Normal = mul(input.Normal, (float3x3)skinning);    
#endif
#endif    

    output.Position = mul(input.Position, _worldViewProj);
    output.TexCoord = input.TexCoord;
    
#ifdef LIGHTNING
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
    float4 color = SAMPLE_TEXTURE(_texture, input.TexCoord);

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