#include "Macros.fxh"

// Constant for Fresnel computation
static const float R0 = 0.02037f;
static const float RefractionReflectionMergeTerm = 0.5f;

DECLARE_TEXTURE_LINEAR_WRAP(_textureWave0);
DECLARE_TEXTURE_LINEAR_WRAP(_textureWave1);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureRefraction);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureReflection);

float2 _waveMapOffset0;
float2 _waveMapOffset1;

float _waveTextureScale;

float3 _lightDirection;
float3 _lightColor;
float _shininess;
float _reflectivity;

float4x4 _world;
float4x4 _worldViewProj;
float4x4 _reflectViewProj;
float3 _cameraPosition;
float4 _waterColor;

struct VSInput
{
    float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float3 ToCamera: TEXCOORD0;
    float2 WavePosition0: TEXCOORD1;
    float2 WavePosition1: TEXCOORD2;
	float4 ReflectionPosition: TEXCOORD3;
	float4 RefractionPosition: TEXCOORD4;
};

// Function calculating fresnel term.
float ComputeFresnelTerm(float3 eyeVec, float3 cameraPosition)
{
	// We'll just use the y unit vector for spec reflection.
	float3 up = float3(0, 1, 0);

	// Compute the fresnel term to blend reflection and refraction maps
	float angle = saturate(dot(-eyeVec, up));
	float f = R0 + (1.0f - R0) * pow(1.0f - angle, 5.0);

	//also blend based on distance
	f = min(1.0f, f + 0.007f * cameraPosition.y);
	
	return f;
}

VSOutput VertexShaderFunction(VSInput input)
{
	VSOutput output = (VSOutput)0;
	
	// Change the position vector to be 4 units for proper matrix calculations.
	input.Position.w = 1.0f;

    output.Position = mul(input.Position, _worldViewProj);
	
	float4 worldPos = mul(input.Position, _world);
	output.ToCamera = worldPos.xyz - _cameraPosition;
	output.ReflectionPosition = mul(input.Position, _reflectViewProj);
	output.WavePosition0 = (input.TexCoord * _waveTextureScale) + _waveMapOffset0;
	output.WavePosition1 = (input.TexCoord * _waveTextureScale) + _waveMapOffset1;
	output.RefractionPosition = output.Position;

	return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
	float3 lightVector = normalize(-_lightDirection);
	input.ToCamera = normalize(input.ToCamera);
	
#ifdef WAVES
	// Sample wave normal map
	float3 normalT0 = SAMPLE_TEXTURE(_textureWave0, input.WavePosition0);
	float3 normalT1 = SAMPLE_TEXTURE(_textureWave1, input.WavePosition1);

	// Unroll the normals retrieved from the normal maps
	normalT0.yz = normalT0.zy;
	normalT1.yz = normalT1.zy;

	normalT0 = 2.0f * normalT0 - 1.0f;
	normalT1 = 2.0f * normalT1 - 1.0f;

	float3 normalT = normalize(0.5f * (normalT0 + normalT1));
	float3 R = normalize(reflect(input.ToCamera, normalT));
#else
	float3 R = normalize(reflect(input.ToCamera, float3(0, 1.0f, 0)));
#endif

#ifdef SPECULAR
	// Compute the reflection from sunlight
	float3 sunLight = _reflectivity * pow(saturate(dot(R, lightVector)), _shininess) * _lightColor;
#else
	float3 sunLight = 0;
#endif

float4 color = 0;

#if !REFLECTION && !REFRACTION
	color.rgb = _waterColor + sunLight;
#else

#ifdef REFRACTION
	// Transform the projective refraction texcoords to NDC space
	// and scale and offset xy to correctly sample a DX texture
	float4 refractionTexCoord = input.RefractionPosition;
	refractionTexCoord.xyz /= refractionTexCoord.w;
	refractionTexCoord.x = 0.5f * refractionTexCoord.x + 0.5f;
	refractionTexCoord.y = -0.5f * refractionTexCoord.y + 0.5f;
	// refract more based on distance from the camera
	refractionTexCoord.z = .1f / refractionTexCoord.z; 
#ifdef WAVES
	float4 refractionColor = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy - refractionTexCoord.z * normalT.xz);
#else
	float4 refractionColor = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy);
#endif
#endif

#ifdef REFLECTION
	// Transform the projective reflection texcoords to NDC space
	// and scale and offset xy to correctly sample a DX texture
	float4 reflectionTexCoord = input.ReflectionPosition;
	reflectionTexCoord.xyz /= reflectionTexCoord.w;
	reflectionTexCoord.x = 0.5f * reflectionTexCoord.x + 0.5f;
	reflectionTexCoord.y = -0.5f * reflectionTexCoord.y + 0.5f;
	// reflect more based on distance from the camera
	reflectionTexCoord.z = .1f / reflectionTexCoord.z;
#ifdef WAVES
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy + reflectionTexCoord.z * normalT.xz);
#else
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy);
#endif
#endif

#if REFLECTION && REFRACTION
	#ifdef FRESNEL
		float fresnelTerm = ComputeFresnelTerm(input.ToCamera, _cameraPosition);
	#else
		float fresnelTerm = RefractionReflectionMergeTerm;
	#endif

	color.rgb = _waterColor * lerp(refractionColor, reflectionColor, fresnelTerm) + sunLight;
#elif REFRACTION
	color.rgb = _waterColor * refractionColor + sunLight;
#endif
#endif

	// alpha canal
	color.a = 1;

	return color;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);