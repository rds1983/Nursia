#include "Macros.fxh"

DECLARE_TEXTURE(_textureRefraction, 0);
DECLARE_TEXTURE(_textureReflection, 1);
DECLARE_TEXTURE(_textureDUDV, 2);
DECLARE_TEXTURE(_textureNormals, 3);

BEGIN_CONSTANTS

const static float Tiling = 4.0;
const static float WaveStrength = 0.02;
const static int BlurSize = 2;
const static float BlurSampleCount = (BlurSize * 2.0) + 1.0;
const static float TexelSize = 1.0 / 360;
const static float shineDamper = 20.0;
const static float reflectivity = 0.5;

MATRIX_CONSTANTS

float4x4 _world, _viewProjection;
float _moveFactor;
float3 _cameraPosition;
float3 _lightPosition;
float3 _lightColor;

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float4 ClipSpace: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
    float3 ToCamera: TEXCOORD2;
    float3 FromLight: TEXCOORD3;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    float4 worldPosition = mul(input.Position, _world);
   
    output.Position = mul(worldPosition, _viewProjection);
    output.ClipSpace = output.Position;
    output.TextureCoords = float2(input.Position.x/2.0 + 0.5, input.Position.z/2.0 + 0.5) * Tiling;
    output.ToCamera = _cameraPosition - worldPosition.xyz;
    output.FromLight = worldPosition.xyz - _lightPosition; 

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
    // Calculate refract/reflect coords
    float2 ndc = (input.ClipSpace.xy / input.ClipSpace.w)/2.0 + 0.5;

    float2 refractTexCoords = float2(ndc.x, -ndc.y);
    float2 reflectTexCoords = float2(ndc.x, ndc.y);

    // Calculate distortion
	float2 distortedTexCoords = SAMPLE_TEXTURE(_textureDUDV, float2(input.TextureCoords.x + _moveFactor, input.TextureCoords.y)).rg*0.1;
	distortedTexCoords = input.TextureCoords + float2(distortedTexCoords.x, distortedTexCoords.y + _moveFactor);
	float2 totalDistortion = (SAMPLE_TEXTURE(_textureDUDV, distortedTexCoords).rg * 2.0 - 1.0) * WaveStrength;

    // Apply distortion to coords
    refractTexCoords += totalDistortion;
    refractTexCoords.x = clamp(refractTexCoords.x, 0.001, 0.999);
    refractTexCoords.y = clamp(refractTexCoords.y, -0.999, -0.001);    

    reflectTexCoords += totalDistortion;
    reflectTexCoords = clamp(reflectTexCoords, 0.001, 0.999);

    // Refraction color
    float4 colorRefraction = SAMPLE_TEXTURE(_textureRefraction, refractTexCoords);

    // Reflection color is blurred
    float4 colorReflection = 0;
    for(int i = -BlurSize; i < BlurSize; i++)
    {
        float offset = i * TexelSize;
        colorReflection += SAMPLE_TEXTURE(_textureReflection, reflectTexCoords + float2(0.0, offset));
    }
    colorReflection /= BlurSampleCount;
    
    // Normal
    float4 normalMapColour = SAMPLE_TEXTURE(_textureNormals, distortedTexCoords);
	float3 normal = float3(normalMapColour.r * 2.0 - 1.0, normalMapColour.b*3.0, normalMapColour.g * 2.0 - 1.0);
	normal = normalize(normal);      

    // Fresnel effect
    float3 viewVector = normalize(input.ToCamera);
    float refractiveFactor = dot(viewVector, normal);
    refractiveFactor = pow(refractiveFactor, 0.5);
    refractiveFactor = clamp(refractiveFactor, 0.0, 1.0);
    
    // Specular lightning
    float3 reflectedLight = reflect(normalize(input.FromLight), normal);
	float specular = max(dot(reflectedLight, viewVector), 0.0);
	specular = pow(specular, shineDamper);
	float3 specularHighlights = _lightColor * specular * reflectivity;
    
    // Calculate result
    float4 result = lerp(colorReflection, colorRefraction, refractiveFactor);
    
    // Mix with some blueness and apply lightning
    result = lerp(result, float4(0, 0.3, 0.5, 1.0), 0.2) + float4(specularHighlights, 0);

    return result;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);