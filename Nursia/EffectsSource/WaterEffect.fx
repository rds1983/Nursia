#include "Macros.fxh"
#include "Lightning.fxh"

DECLARE_TEXTURE_LINEAR_WRAP(_textureWave0);
DECLARE_TEXTURE_LINEAR_WRAP(_textureWave1);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureRefraction);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureReflection);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureDepth);

float4 _waterColor;
float2 _waveMapOffset0;
float2 _waveMapOffset1;
float _waveTextureScale;
float _fresnelFactor;
float _edgeFactor;

float4x4 _world;
float4x4 _worldViewProj;
float4x4 _reflectViewProj;
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
    float2 WavePosition0: TEXCOORD1;
    float2 WavePosition1: TEXCOORD2;
	float4 ReflectionPosition: TEXCOORD3;
	float4 RefractionPosition: TEXCOORD4;
	float3 SourcePosition: TEXCOORD5;
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
    output.Position = mul(input.Position, _worldViewProj);
	float4 worldPos = mul(input.Position, _world);
	output.ToCamera = _cameraPosition - worldPos.xyz;
	output.ReflectionPosition = mul(input.Position, _reflectViewProj);
	output.WavePosition0 = (input.TexCoord * _waveTextureScale) + _waveMapOffset0;
	output.WavePosition1 = (input.TexCoord * _waveTextureScale) + _waveMapOffset1;
	output.RefractionPosition = output.Position;

	return output;
}

float4 PSColor(VSOutput input) : SV_Target0
{
	input.ToCamera = normalize(input.ToCamera);

	float4 refractionTexCoord = ToNDC(input.RefractionPosition);
	float4 reflectionTexCoord = ToNDC(input.ReflectionPosition);

#ifdef SOFT_EDGES
	/* Soft Edges */
	float depth = SAMPLE_TEXTURE(_textureDepth, refractionTexCoord).r;
	float floorDistance = edge(SAMPLE_TEXTURE(_textureDepth, refractionTexCoord).r);
	float waterDistance = edge(input.RefractionPosition.z / input.RefractionPosition.w);
	float waterDepth = floorDistance - waterDistance;
#endif

#ifdef WAVES
	// Sample wave normal map
	float3 normalT0 = ColorToNormal(SAMPLE_TEXTURE(_textureWave0, input.WavePosition0).rgb);
	float3 normalT1 = ColorToNormal(SAMPLE_TEXTURE(_textureWave1, input.WavePosition1).rgb);

	float3 normalT = normalize(0.5f * (normalT0 + normalT1));
	float4 refractionColor = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy - refractionTexCoord.z * normalT.xz);
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy + reflectionTexCoord.z * normalT.xz);
#else
	float3 normalT = float3(0, 1.0f, 0);
	float4 refractionColor = SAMPLE_TEXTURE(_textureRefraction, refractionTexCoord.xy);
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy);
#endif

	float4 color = 0;
	
	/* Fresnel Effect */
	float refractiveFactor = abs(dot(input.ToCamera, normalT)); // the distance between vectors camera and pointing straight up
	refractiveFactor = pow(refractiveFactor, _fresnelFactor); // the greater the power the more reflective it is
	refractiveFactor = clamp(refractiveFactor, 0.0f, 1.0f); // rids of black artifacts	

	color = _waterColor * lerp(reflectionColor, refractionColor, refractiveFactor);
	
	LightPower lightPower = CalculateLightPower(normalize(float3(normalT.x, normalT.y * 3, normalT.z)), input.SourcePosition, input.ToCamera);
	color.rgb *= lightPower.Diffuse;
	color.rgb += lightPower.Specular;

#ifdef SOFT_EDGES
	color.a *= clamp(waterDepth / _edgeFactor, 0.0f, 1.0f); // increase the soft edges by increasing denominator
#endif

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

float4 PSDepthTexture(VSOutput input) : SV_Target0
{
	float4 refractionTexCoord = ToNDC(input.RefractionPosition);

	float4 c = SAMPLE_TEXTURE(_textureDepth, refractionTexCoord.xy);

	return c;
}

TECHNIQUE(Color, VS, PSColor);
TECHNIQUE(RefractionTexture, VS, PSRefractionTexture);
TECHNIQUE(ReflectionTexture, VS, PSReflectionTexture);
TECHNIQUE(DepthTexture, VS, PSDepthTexture);