#include "Macros.fxh"

BEGIN_CONSTANTS

float _width;
float _height;
float4 _color;

MATRIX_CONSTANTS

float4x4 _worldViewProj;
float4x4 _view;

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

	float3 right = _view._m00_m10_m20;
	float3 up = _view._m01_m11_m21;

	float3 position = 0;
	position += right * (input.TexCoord.x - 0.5) * _width;
	position += up * (0.5 - input.TexCoord.y) * _height;

	output.Position = mul(float4(position, 1), _worldViewProj);
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