#include "Macros.fxh"

DECLARE_TEXTURE(_textureRefraction, 0);
DECLARE_TEXTURE(_textureReflection, 1);
DECLARE_TEXTURE(_textureDUDV, 2);

BEGIN_CONSTANTS

const static float Tiling = 6.0;
const static float WaveStrength = 0.02;

MATRIX_CONSTANTS

float4x4 _worldViewProj;
float _moveFactor;

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float4 ClipSpace: TEXCOORD0;
    float2 TextureCoords: TEXCOORD01;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    output.Position = mul(input.Position, _worldViewProj);
    output.ClipSpace = output.Position;
    output.TextureCoords = float2(input.Position.x/2.0 + 0.5, input.Position.z/2.0 + 0.5) * Tiling;

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    float2 ndc = (input.ClipSpace.xy / input.ClipSpace.w)/2.0 + 0.5;
    
    float2 refractTexCoords = float2(ndc.x, -ndc.y);
    float2 reflectTexCoords = float2(ndc.x, ndc.y);
    
    float2 distortion1 = (SAMPLE_TEXTURE(_textureDUDV, float2(input.TextureCoords.x + _moveFactor, input.TextureCoords.y)).rg * 2.0 - 1.0) * WaveStrength;
    float2 distortion2 = (SAMPLE_TEXTURE(_textureDUDV, float2(-input.TextureCoords.x + _moveFactor, input.TextureCoords.y + _moveFactor)).rg * 2.0 - 1.0) * WaveStrength;
    float2 totalDistortion = distortion1 + distortion2;
    
    refractTexCoords += totalDistortion;
    refractTexCoords.x = clamp(refractTexCoords.x, 0.001, 0.999);
    refractTexCoords.y = clamp(refractTexCoords.y, -0.999, -0.001);    
    
    reflectTexCoords += totalDistortion;
    reflectTexCoords = clamp(reflectTexCoords, 0.001, 0.999);

    float4 colorRefraction = SAMPLE_TEXTURE(_textureRefraction, refractTexCoords);
    float4 colorReflection = SAMPLE_TEXTURE(_textureReflection, reflectTexCoords);

    float4 result = lerp(colorRefraction, colorReflection, 0.5);
    result = lerp(result, float4(0, 0.3, 0.5, 1.0), 0.2);
    
    return result;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);