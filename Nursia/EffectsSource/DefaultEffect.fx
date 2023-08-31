#include "Macros.fxh"

#define MAX_BONES   96

#ifdef TEXTURE
DECLARE_TEXTURE(_texture, 0);
#endif

BEGIN_CONSTANTS

#ifdef LIGHTNING

float3 _lightDir;
float3 _lightColor;

#endif

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
#endif

#ifdef CLIP_PLANE
    float4 ClipDistances: TEXCOORD2;
#endif
};

VSOutput VertexShaderFunction(VSInput input)
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

#ifdef LIGHTNING
float3 ComputeLighting(float3 normalVector, float3 lightDirection, float3 lightColor, float attenuation)
{
    float diffuse = max(dot(normalVector, lightDirection), 0.0);
    float3 diffuseColor = lightColor  * diffuse * attenuation;

    return diffuseColor;
}
#endif

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
#ifdef CLIP_PLANE
    clip(input.ClipDistances); 
#endif

#if TEXTURE
    float4 color = SAMPLE_TEXTURE(_texture, input.TexCoord);
#else
    float4 color = float4(1.0, 1.0, 1.0, 1.0);
#endif

#ifdef LIGHTNING
	float3 result = float3(0, 0, 0);
	float3 normal = normalize(input.WorldNormal);
    result += ComputeLighting(normal, -_lightDir, _lightColor, 1.0);

	float4 c = color * float4(result, 1) * _diffuseColor;
#else
	float4 c = color * _diffuseColor;
#endif
    clip(c.a < 0.1?-1:1);

    return c;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);