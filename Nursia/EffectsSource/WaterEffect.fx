#include "Macros.fxh"
#include "Lightning.fxh"

DECLARE_TEXTURE_LINEAR_WRAP(_textureNormals1);
DECLARE_TEXTURE_LINEAR_WRAP(_textureNormals2);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureRefraction);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureReflection);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureDepth);
DECLARE_CUBEMAP_LINEAR_CLAMP(_textureSkybox);

float4 _waterColor;
float2 _waveDirection1;
float2 _waveDirection2;
float _timeScale;
float _reflectionFactor;
float _fresnelFactor;
float _edgeFactor;
float _murkinessStart;
float _murkinessFactor;

float _time;

float4x4 _world;
float4x4 _worldViewProj;
float4x4 _reflectViewProj;
float3x3 _worldInverseTranspose;
float3 _cameraPosition;
float _far, _near;

struct VSInput
{
    float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float3 ToCamera: TEXCOORD0;
	float4 ReflectionPosition: TEXCOORD1;
	float4 RefractionPosition: TEXCOORD2;
	float3 SourcePosition: TEXCOORD3;
	float2 TexCoord: TEXCOORD4;
	float3 WorldPosition: TEXCOORD5;
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
	return float3(c.r * 2.0f - 1.0f, c.b * 2.0f - 1.0f, c.g * 2.0f - 1.0f);
}

float edge(float depth)
{
	return 2.0 * _near * _far / (_far + _near - (2.0 * depth - 1.0) * (_far - _near));
}

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.Position.w = 1.0f;

	output.SourcePosition = input.Position.xyz;
	output.TexCoord = input.TexCoord;
    output.Position = mul(input.Position, _worldViewProj);
	output.WorldPosition = mul(input.Position, _world);
	output.ToCamera = _cameraPosition - output.WorldPosition.xyz;
	output.ReflectionPosition = mul(input.Position, _reflectViewProj);
	output.RefractionPosition = output.Position;

	return output;
}

float4 PSColor(VSOutput input) : SV_Target0
{
	input.ToCamera = normalize(input.ToCamera);

	float4 refractionTexCoord = ToNDC(input.RefractionPosition);
	float4 reflectionTexCoord = ToNDC(input.ReflectionPosition);
	
	// Time calculations for wave (normal map) movement
	float2 time = (_time * _waveDirection1) * _timeScale; // Movement rate of first wave
	float2 time2 = (_time * _waveDirection2) * _timeScale; // Movement rate of second wave
	
	// Blend normal maps into one
	float3 normal1 = ColorToNormal(SAMPLE_TEXTURE(_textureNormals1, input.TexCoord + time).rgb);
	float3 normal2 = ColorToNormal(SAMPLE_TEXTURE(_textureNormals2, input.TexCoord + time2).rgb);
	float3 normal = lerp(normal1, normal2, 0.5);

#ifdef DEPTH_BUFFER
	/* Soft Edges */
	float depth = SAMPLE_TEXTURE(_textureDepth, refractionTexCoord).r;
	float floorDistance = edge(SAMPLE_TEXTURE(_textureDepth, refractionTexCoord).r);
	float waterDistance = edge(input.RefractionPosition.z / input.RefractionPosition.w);
	float waterDepth = floorDistance - waterDistance;

	float depth_blend = exp((waterDepth - _murkinessStart) * -(_murkinessFactor / 10));
	float depth_blend_pow = 1.0 - clamp(depth_blend, 0.0, 1.0);
#else
	float depth_blend_pow = 0.5;
#endif

	float4 refractionColor = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy);

	float3 R = reflect(input.WorldPosition - _cameraPosition, normal);
	float4 reflectedSkyColor = SAMPLE_CUBEMAP(_textureSkybox, R);
	
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy);
	reflectionColor = lerp(_waterColor, reflectionColor, _reflectionFactor) + (1 - reflectionColor.a) * reflectedSkyColor;
	
	float4 color = _waterColor;
	color *= lerp(refractionColor, reflectionColor, depth_blend_pow);
	
	LightPower lightPower = CalculateLightPower(normal, input.SourcePosition, input.ToCamera);
	color.rgb *= lightPower.Diffuse;
	color.rgb += lightPower.Specular;

#ifdef DEPTH_BUFFER
	color.a *= clamp(waterDepth / _edgeFactor, 0.0f, 1.0f); // increase the soft edges by increasing denominator
#else
	color.a = 1.0;
#endif

	// color = float4(depth_blend_pow, depth_blend_pow, depth_blend_pow, 1.0);
	// color = float4(normal.x, normal.y, normal.z, 1.0);

	return color;
}

float4 PSRefractionTexture(VSOutput input) : SV_Target0
{
	float4 refractionTexCoord = ToNDC(input.RefractionPosition);

	return SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy);
}

float4 PSReflectionTexture(VSOutput input) : SV_Target0
{
	float4 reflectionTexCoord = ToNDC(input.ReflectionPosition);

	return SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy);
}

float4 PSDepthTexture(VSOutput input) : SV_Target0
{
	float4 refractionTexCoord = ToNDC(input.RefractionPosition);

	return SAMPLE_TEXTURE(_textureDepth, refractionTexCoord.xy);
}

TECHNIQUE(Color, VS, PSColor);
TECHNIQUE(RefractionTexture, VS, PSRefractionTexture);
TECHNIQUE(ReflectionTexture, VS, PSReflectionTexture);
TECHNIQUE(DepthTexture, VS, PSDepthTexture);