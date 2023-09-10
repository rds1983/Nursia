#include "Macros.fxh"

#define PI 3.1415926535897932384626433832795

#ifdef LIGHTNING
#include "Lightning.fxh"
#endif

DECLARE_TEXTURE(_textureBase, 0);

#if TEXTURES > 0
DECLARE_TEXTURE(_textureBlend, 2);
DECLARE_TEXTURE(_texture1, 1);
#endif

#if TEXTURES > 1
DECLARE_TEXTURE(_texture2, 3);
#endif

#if TEXTURES > 2
DECLARE_TEXTURE(_texture3, 4);
#endif

#if TEXTURES > 3
DECLARE_TEXTURE(_texture4, 5);
#endif

BEGIN_CONSTANTS

float4 _diffuseColor;

#ifdef CLIP_PLANE

float4 _clipPlane;

#endif

#ifdef MARKER

float3 _markerPosition;
float _markerRadius;
#define COLOR_BRUSH float4(0.4,0.4,0.4, 0.4);

#endif

MATRIX_CONSTANTS

float4x4 _worldViewProjection;
float3x3 _worldInverseTranspose;

#if LIGHTNING || MARKER
float4x4 _world;
#endif

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;

#ifdef LIGHTNING
    float3 Normal   : NORMAL;
#endif

    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position: SV_POSITION;

    float2 TexCoord: TEXCOORD0;

#ifdef LIGHTNING
	float3 WorldNormal: TEXCOORD1;
#endif

#ifdef CLIP_PLANE
    float4 ClipDistances: TEXCOORD2;
#endif

#if LIGHTNING || MARKER
	float4 SourcePosition: POSITION1;
#endif
};

VSOutput Vertex(VSInput input)
{
    // Zero out our output.
	VSOutput output = (VSOutput)0;

	output.TexCoord = input.TexCoord;
	output.Position = mul(input.Position, _worldViewProjection);

#ifdef LIGHTNING
	output.WorldNormal = mul(input.Normal, _worldInverseTranspose);
#endif

#if LIGHTNING || MARKER
	output.SourcePosition = mul(input.Position, _world);
#endif

#ifdef CLIP_PLANE
    output.ClipDistances.yzw = 0;
    output.ClipDistances.x = dot(output.Position, _clipPlane); 
#endif
	 
	// Done--return the output.
	return output;
}

float4 Pixel(VSOutput input) : COLOR
{
#ifdef CLIP_PLANE
    clip(input.ClipDistances); 
#endif

	float3 color = SAMPLE_TEXTURE(_textureBase, input.TexCoord).rgb;

#if TEXTURES > 0
	float4 b = SAMPLE_TEXTURE(_textureBlend, input.TexCoord).rgba;
	float3 c1 = SAMPLE_TEXTURE(_texture1, input.TexCoord).rgb;
	color = lerp(color, c1, b.r);
#endif

#if TEXTURES > 1
	float3 c2 = SAMPLE_TEXTURE(_texture2, input.TexCoord).rgb;
	color = lerp(color, c2, b.g);
#endif

#if TEXTURES > 2
	float3 c3 = SAMPLE_TEXTURE(_texture3, input.TexCoord).rgb;
	color = lerp(color, c3, b.b);
#endif

#if TEXTURES > 3
	float3 c4 = SAMPLE_TEXTURE(_texture4, input.TexCoord).rgb;
	color = lerp(color, c4, b.a);
#endif

#ifdef LIGHTNING
	float3 normal = normalize(input.WorldNormal);
	float3 lightPower = CalculateLightPower(normal, input.SourcePosition.xyz);

	float4 c = float4(color, 1) * float4(lightPower, 1) * _diffuseColor;
#else
	float4 c = float4(color, 1) * _diffuseColor;
#endif
	
#ifdef MARKER
	float dist = distance(_markerPosition, input.SourcePosition.xyz);
	if(dist <= _markerRadius)
	{
		float gradient = (_markerRadius - dist + 0.01) / _markerRadius;
		gradient = 1.0 - clamp(cos(gradient * PI), 0.0, 1.0);
		c += float4(0.4 * gradient, 0.4 * gradient, 0.4 * gradient, 0.4 * gradient);
	}
#endif

    clip(c.a < 0.1?-1:1);

	return c;
}

TECHNIQUE(Default, Vertex, Pixel);