#include "Macros.fxh"

DECLARE_TEXTURE(_texture, 0);

BEGIN_CONSTANTS

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
    float2 TexCoord: TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    output.Position = mul(input.Position, _worldViewProj);
    output.TexCoord = float2(input.Position.x/2.0 + 0.5, input.Position.y/2.0 + 0.5);

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    return float4(0, 0, 1, 1);
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);