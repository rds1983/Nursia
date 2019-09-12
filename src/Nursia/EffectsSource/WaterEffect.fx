#include "Macros.fxh"

DECLARE_TEXTURE(_textureRefraction, 0);
DECLARE_TEXTURE(_textureReflection, 1);

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
    float4 ClipSpace: TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    output.Position = mul(input.Position, _worldViewProj);
    output.ClipSpace = output.Position;

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    float2 ndc = (input.ClipSpace.xy / input.ClipSpace.w)/2.0 + 0.5;
    
    float2 refractTexCoords = float2(ndc.x, -ndc.y);
    float2 reflectTexCoords = float2(ndc.x, ndc.y);

    float4 colorRefraction = SAMPLE_TEXTURE(_textureRefraction, refractTexCoords);
    float4 colorReflection = SAMPLE_TEXTURE(_textureReflection, reflectTexCoords);

    float4 result = lerp(colorRefraction, colorReflection, 0.5);
    
    return result;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);