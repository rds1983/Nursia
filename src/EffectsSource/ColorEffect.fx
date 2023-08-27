#include "Macros.fxh"

BEGIN_CONSTANTS

MATRIX_CONSTANTS

float4x4 _transform;
float4 _color;

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
    
    output.Position = mul(input.Position, _transform);

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    return _color;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);