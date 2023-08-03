#include "Macros.fxh"

DECLARE_CUBEMAP(_texture, 0);

BEGIN_CONSTANTS

MATRIX_CONSTANTS

float4x4 _transform;

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float3 TexCoord: TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    output.Position = mul(input.Position, _transform);
    output.TexCoord = input.Position.xyz;

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    return SAMPLE_CUBEMAP(_texture, input.TexCoord);
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);