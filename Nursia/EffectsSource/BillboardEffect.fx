#include "Macros.fxh"

BEGIN_CONSTANTS

float4 _color;

MATRIX_CONSTANTS

float4x4 _transform;

END_CONSTANTS

#ifdef TEXTURE

DECLARE_TEXTURE_LINEAR_WRAP(_texture);

#endif

struct VSInput
{
	float3 Position: POSITION0;
	float2 TexCoord: TEXCOORD0;
};

struct VSOutput
{
	float4 Position: POSITION0;
	float2 TexCoord: TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
	VSOutput output = (VSOutput)0;

	output.Position = mul(float4(input.Position, 1), _transform);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 PixelShaderFunction(VSOutput input): COLOR0
{
#if TEXTURE
    float4 color = SAMPLE_TEXTURE(_texture, input.TexCoord) * _color;
#else
	float4 color = _color;
#endif

	return color;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);