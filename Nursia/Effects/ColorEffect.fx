#include "Macros.fxh"

BEGIN_CONSTANTS

float4 _color;

MATRIX_CONSTANTS

float4x4 _worldViewProj;

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    output.Position = mul(input.Position, _worldViewProj);

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    return _color;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);