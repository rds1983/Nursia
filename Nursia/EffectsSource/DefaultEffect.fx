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

DECLARE_TEXTURE_LINEAR_CLAMP(_shadowMap);

BEGIN_CONSTANTS

#ifdef CLIP_PLANE

float4 _clipPlane;

#endif

float DepthBias = 0.001f;
float4 _diffuseColor;
float3 _cameraPosition;

MATRIX_CONSTANTS

#ifdef LIGHTNING
float3x3 _worldInverseTranspose;
#endif

#ifdef SKINNING
float4x3 _bones[MAX_BONES];
#endif

float4x4 _lightViewProj;
float4x4 _worldViewProj;
float4x4 _world;

END_CONSTANTS

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
	float4 WorldPosition: TEXCOORD4;
#endif

#ifdef CLIP_PLANE
	float4 ClipDistances: TEXCOORD5;
#endif

	float2 Depth: TEXCOORD6;
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
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	
	
#ifdef LIGHTNING
	output.SourcePosition = input.Position;
	output.WorldPosition = mul(input.Position, _world);
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
	
    // Find the position of this pixel in light space
    float4 lightingPosition = mul(input.WorldPosition, _lightViewProj);
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / 
                            lightingPosition.w + float2( 0.5, 0.5 );
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

    // Get the current depth stored in the shadow map
    float shadowdepth = SAMPLE_TEXTURE(_shadowMap, ShadowTexCoord).r;
	
    // Calculate the current pixel depth
    // The bias is used to prevent folating point errors that occur when
    // the pixel of the occluder is being drawn
    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias;
    
    // Check to see if this pixel is in front or behind the value in the shadow map
    if (shadowdepth < ourdepth)
    {
        // Shadow the pixel by lowering the intensity
        color *= float4(0.5,0.5,0.5,1);
    };	
#endif

	clip(color.a < 0.1?-1:1);

	return color;
}

TECHNIQUE(Default, VS, PS);
