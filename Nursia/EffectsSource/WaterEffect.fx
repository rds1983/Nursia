#include "Macros.fxh"
#include "Lightning.fxh"

DECLARE_TEXTURE_LINEAR_WRAP(_textureDudv);
DECLARE_TEXTURE_LINEAR_WRAP(_textureNormals);
DECLARE_TEXTURE_LINEAR_WRAP(_textureRefraction);
DECLARE_TEXTURE_LINEAR_WRAP(_textureReflection);
DECLARE_TEXTURE_LINEAR_WRAP(_textureDepth);
DECLARE_CUBEMAP_LINEAR_CLAMP(_textureSkybox);

const static int BlurSize = 2;
const static float BlurSampleCount = (BlurSize * 2.0) + 1.0;
const static float TexelSize = 1.0 / 360;

float4 _colorShallow;
float4 _colorDeep;
float _edgeFactor;
float _murkinessStart;
float _murkinessFactor;
float _tiling;
float _moveFactor;
float _waveStrength;

float4x4 _world;
float4x4 _worldViewProj;
float4x4 _reflectViewProj;
float3x3 _worldInverseTranspose;
float3 _cameraPosition;
float _far, _near;

END_CONSTANTS

struct VSInput
{
	float4 Position: SV_POSITION;
	float2 TexCoord: TEXCOORD0;
};

struct VSOutput
{
	float4 Position: SV_POSITION;
	float4 SourcePosition: TEXCOORD0;
	float4 ClipSpace: TEXCOORD1;
	float2 TextureCoords: TEXCOORD2;
	float3 ToCamera: TEXCOORD3;
};

VSOutput VertexShaderFunction(VSInput input)
{
	VSOutput output = (VSOutput)0;

	input.Position.w = 1.0;
	float4 worldPosition = mul(input.Position, _world);

	output.Position = mul(input.Position, _worldViewProj);
	output.SourcePosition = worldPosition;
	output.ClipSpace = output.Position;
	output.TextureCoords = float2(input.Position.x/2.0 + 0.5, input.Position.z/2.0 + 0.5) * _tiling;
	output.ToCamera = _cameraPosition - worldPosition.xyz;

	return output;
}

float Edge(float depth)
{
	return 2.0 * _near * _far / (_far + _near - (2.0 * depth - 1.0) * (_far - _near));
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
	// Calculate refract/reflect coords
	float2 ndc = (input.ClipSpace.xy / input.ClipSpace.w)/2.0 + 0.5;

	float2 refractTexCoords = float2(ndc.x, -ndc.y);
	float2 reflectTexCoords = float2(ndc.x, ndc.y);

#ifdef DEPTH_BUFFER
	float depth = SAMPLE_TEXTURE(_textureDepth, refractTexCoords).r;
	float floorDistance = Edge(SAMPLE_TEXTURE(_textureDepth, refractTexCoords).r);
	float waterDistance = Edge(input.ClipSpace.z / input.ClipSpace.w);
	float waterDepth = floorDistance - waterDistance;

	float depthBlend = exp((waterDepth - _murkinessStart) * -(_murkinessFactor / 10));
	depthBlend = 1.0 - clamp(depthBlend, 0.0, 1.0);
#else
	float depthBlend = 0.5;
#endif

	// Calculate distortion
	float2 distortedTexCoords = SAMPLE_TEXTURE(_textureDudv, float2(input.TextureCoords.x + _moveFactor, input.TextureCoords.y)).rg * 0.1;
	distortedTexCoords = input.TextureCoords + float2(distortedTexCoords.x, distortedTexCoords.y + _moveFactor);
	float2 totalDistortion = (SAMPLE_TEXTURE(_textureDudv, distortedTexCoords).rg * 2.0 - 1.0) * _waveStrength;

	// Apply distortion to coords
	refractTexCoords += totalDistortion;
	refractTexCoords.x = clamp(refractTexCoords.x, 0.001, 0.999);
	refractTexCoords.y = clamp(refractTexCoords.y, -0.999, -0.001);    

	reflectTexCoords += totalDistortion;
	reflectTexCoords = clamp(reflectTexCoords, 0.001, 0.999);

	// Refraction color
	float4 colorRefraction = SAMPLE_TEXTURE(_textureRefraction, refractTexCoords);
	
	// Beer-Lambert law
	colorRefraction = lerp(_colorShallow * colorRefraction, _colorDeep, depthBlend);

	// Normal
	float4 normalMapColour = SAMPLE_TEXTURE(_textureNormals, distortedTexCoords);
	float3 normal = float3(normalMapColour.r * 2.0 - 1.0, normalMapColour.b*3.0, normalMapColour.g * 2.0 - 1.0);
	normal = normalize(normal);      

#if CUBEMAP_REFLECTION
	float3 R = reflect(-input.ToCamera, normal);
	float4 colorReflection = _colorDeep * SAMPLE_CUBEMAP(_textureSkybox, R);
#else
	// Reflection color is blurred
	float4 colorReflection = 0;
	
	for(int i = -BlurSize; i < BlurSize; i++)
	{
		float offset = i * TexelSize;
		colorReflection += SAMPLE_TEXTURE(_textureReflection, reflectTexCoords + float2(0.0, offset));
	}
	colorReflection /= BlurSampleCount;
#endif

	// Fresnel effect
	float3 viewVector = normalize(input.ToCamera);
	float refractiveFactor = dot(viewVector, normal);
	refractiveFactor = pow(refractiveFactor, 0.5);
	refractiveFactor = clamp(refractiveFactor, 0.0, 1.0);
	  
	// Calculate result
	float4 result = lerp(colorReflection, colorRefraction, refractiveFactor);

	LightPower lightPower = CalculateLightPower(normal, input.SourcePosition, input.ToCamera);
	result.rgb *= lightPower.Diffuse;
	result.rgb += lightPower.Specular;

#ifdef DEPTH_BUFFER
	float alpha = clamp(waterDepth / _edgeFactor, 0.0f, 1.0f); // increase the soft Edges by increasing denominator
#else
	float alpha = 1.0;
#endif

	result.a = alpha;

	return result;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);