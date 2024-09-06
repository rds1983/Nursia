#include "Macros.fxh"

DECLARE_TEXTURE_LINEAR_WRAP(_texture);

BEGIN_CONSTANTS

float4 _color;

MATRIX_CONSTANTS

float4x4 _worldViewProj;
float4x4 _view;

END_CONSTANTS

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

	float3 direction = _view._m02_m12_m22;
	float3 up = float3(0, 1, 0);
	float3 right = normalize(cross(direction, up));

	float3 position = input.Position;
	position += right * (input.TexCoord.x - 0.5);
	position += up * (input.TexCoord.y - 0.5);

	output.Position = mul(float4(position, 1), _worldViewProj);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 PixelShaderFunction(VSOutput input): COLOR0
{
	return _color;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);