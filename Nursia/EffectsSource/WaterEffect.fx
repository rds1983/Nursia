#include "Macros.fxh"

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

float _reflectivity;
float _shininess;

float _fresnelFactor;

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

// Transform the projective refraction texcoords to NDC space
// and scale and offset xy to correctly sample a DX texture
float4 ToNDC(float4 input)
{
	float4 result = input;
	result.xyz /= result.w;
	result.x = 0.5f * result.x + 0.5f;
	result.y = -0.5f * result.y + 0.5f;
	
	// refract more based on distance from the camera
	result.z = .1f / result.z; 
	
	return result;
}

float3 ColorToNormal(float3 c)
{
	return float3(c.r * 2.0f - 1.0f, c.b * 3.0f, c.g * 2.0f - 1.0f);
}

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.Position.w = 1.0f;

    output.Position = mul(input.Position, _worldViewProj);
	
	float4 worldPos = mul(input.Position, _world);
	output.ToCamera = _cameraPosition - worldPos.xyz;
	output.ReflectionPosition = mul(input.Position, _reflectViewProj);
	output.WavePosition0 = (input.TexCoord * _waveTextureScale) + _waveMapOffset0;
	output.WavePosition1 = (input.TexCoord * _waveTextureScale) + _waveMapOffset1;
	output.RefractionPosition = output.Position;

	return output;
}

float4 PSStandard(VSOutput input) : SV_Target0
{
	float3 lightVector = normalize(-_lightDirection);
	input.ToCamera = normalize(input.ToCamera);
	
#ifdef WAVES
	// Sample wave normal map
	float3 normalT0 = ColorToNormal(SAMPLE_TEXTURE(_textureWave0, input.WavePosition0));
	float3 normalT1 = ColorToNormal(SAMPLE_TEXTURE(_textureWave1, input.WavePosition1));

	float3 normalT = normalize(0.5f * (normalT0 + normalT1));
#else
	float3 normalT = float3(0, 1.0f, 0);
#endif

#ifdef SPECULAR
	// Compute the reflection from sunlight
	float3 n = normalize(float3(normalT.x, normalT.y * 3, normalT.z));
	float3 R = normalize(reflect(-input.ToCamera, n));
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
	float4 refractionTexCoord = ToNDC(input.RefractionPosition);
#ifdef WAVES
	float4 refractionColor = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy - refractionTexCoord.z * normalT.xz);
#else
	float4 refractionColor = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy);
#endif
#endif

#ifdef REFLECTION
	// Transform the projective reflection texcoords to NDC space
	// and scale and offset xy to correctly sample a DX texture
	float4 reflectionTexCoord = ToNDC(input.ReflectionPosition);
#ifdef WAVES
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy + reflectionTexCoord.z * normalT.xz);
#else
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy);
#endif
#endif

#if REFLECTION && REFRACTION
	#ifdef FRESNEL
		/* Fresnel Effect */
		float refractiveFactor = dot(input.ToCamera, normalT); // the distance between vectors camera and pointing straight up
		refractiveFactor = pow(refractiveFactor, _fresnelFactor); // the greater the power the more reflective it is
		refractiveFactor = clamp(refractiveFactor, 0.0f, 1.0f); // rids of black artifacts	
	#else
		float refractiveFactor = RefractionReflectionMergeTerm;
	#endif

	color.rgb = _waterColor * lerp(reflectionColor, refractionColor, refractiveFactor) + sunLight;
#elif REFRACTION
	color.rgb = _waterColor * refractionColor + sunLight;
#endif
#endif

	// color = float4(refractiveFactor, refractiveFactor, refractiveFactor, 1);

	// alpha canal
	color.a = 1;

	return color;
}

float4 PSRefractionTexture(VSOutput input) : SV_Target0
{
	float4 refractionTexCoord = ToNDC(input.RefractionPosition);

	float4 c = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy);
	c.r *= 0.5;

	return c;
}

float4 PSReflectionTexture(VSOutput input) : SV_Target0
{
	float4 reflectionTexCoord = ToNDC(input.ReflectionPosition);

	return SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy);
}

TECHNIQUE(Standard, VS, PSStandard);
TECHNIQUE(RefractionTexture, VS, PSRefractionTexture);
TECHNIQUE(ReflectionTexture, VS, PSReflectionTexture);