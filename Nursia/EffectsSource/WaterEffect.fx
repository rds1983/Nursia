#include "Macros.fxh"
#include "Lightning.fxh"

DECLARE_TEXTURE_LINEAR_WRAP(_textureNormals1);
DECLARE_TEXTURE_LINEAR_WRAP(_textureNormals2);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureScreen);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureReflection);
DECLARE_TEXTURE_LINEAR_CLAMP(_textureDepth);
DECLARE_CUBEMAP_LINEAR_CLAMP(_textureSkybox);

float4 _color;
float2 _waveDirection1;
float2 _waveDirection2;
float _planarReflectionDistortion;
float _timeScale;
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
	float4 PositionCopy: TEXCOORD2;
	float3 SourcePosition: TEXCOORD3;
	float2 TexCoord: TEXCOORD4;
};

// Transform the projective screen texcoords to NDC space
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

float Fresnel(float amount, float3 normal, float3 view)
{
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0 )), amount);
}

float Edge(float depth)
{
	return 2.0 * _near * _far / (_far + _near - (2.0 * depth - 1.0) * (_far - _near));
}

VSOutput VS(VSInput input)
{
	VSOutput output = (VSOutput)0;

	// Change the position vector to be 4 units for proper matrix calculations.
	input.Position.w = 1.0f;

	output.SourcePosition = input.Position.xyz;
	float3 worldPosition = mul(input.Position, _world).xyz;
	output.TexCoord = input.TexCoord;
	output.Position = mul(input.Position, _worldViewProj);
	output.PositionCopy = output.Position;
	output.ToCamera = _cameraPosition - worldPosition;
	output.ReflectionPosition = mul(input.Position, _reflectViewProj);

	return output;
}

float4 PSColor(VSOutput input) : SV_Target0
{
	input.ToCamera = normalize(input.ToCamera);
	
	float4 screenTexCoord = ToNDC(input.PositionCopy);

	// Depth variables and calc
#ifdef DEPTH_BUFFER
	float depth = SAMPLE_TEXTURE(_textureDepth, screenTexCoord).r;
	float floorDistance = Edge(SAMPLE_TEXTURE(_textureDepth, screenTexCoord).r);
	float waterDistance = Edge(input.PositionCopy.z / input.PositionCopy.w);
	float waterDepth = floorDistance - waterDistance;

	float depthBlend = exp((waterDepth - _murkinessStart) * -(_murkinessFactor / 10));
	depthBlend = 1.0 - clamp(depthBlend, 0.0, 1.0);
#else
	float depthBlend = 0.5;
#endif

	// Time calculations for wave (normal map) movement
	float2 time = (_time * _waveDirection1) * _timeScale; // Movement rate of first wave
	float2 time2 = (_time * _waveDirection2) * _timeScale; // Movement rate of second wave
	
	// Blend normal maps into one
	float3 normal1 = ColorToNormal(SAMPLE_TEXTURE(_textureNormals1, input.TexCoord + time).rgb);
	float3 normal2 = ColorToNormal(SAMPLE_TEXTURE(_textureNormals2, input.TexCoord + time2).rgb);
	float3 normal = lerp(normal1, normal2, 0.5);
	//normal = float3(normal.r * 2.0 - 1.0, normal.g * 3.0, normal.b * 2.0 - 1.0);
	normal = normalize(normal);
	
	
	float3 up = float3(0, 1, 0);
	float cos = dot(normal, up);
	float3 normalInPlane = normal - up * dot(normal, up);
	float3 offset = normalInPlane * _planarReflectionDistortion;
	
	float2 dudv = float2(offset.x, offset.y);

	// Retrieving depth color and applying the deep and shallow colors
	float3 screenColor = SAMPLE_TEXTURE(_textureScreen, screenTexCoord).rgb;
	float3 refractionColor = lerp(screenColor, float3(1, 1, 1), depthBlend);

	// Calculate reflection color
#if CUBEMAP_REFLECTION
	float3 R = reflect(-input.ToCamera, normal);
	float4 reflectionColor = SAMPLE_CUBEMAP(_textureSkybox, R);
#else
	float4 reflectionTexCoord = ToNDC(input.ReflectionPosition);
	float4 reflectionColor = SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy + dudv);
#endif
	
	// Fresnel effect
	float refractiveFactor = dot(input.ToCamera, normal);
	refractiveFactor = pow(refractiveFactor, 1);
	refractiveFactor = clamp(refractiveFactor, 0.0, 1.0);
	float3 color = lerp(reflectionColor, refractionColor, refractiveFactor) * _color;

	LightPower lightPower = CalculateLightPower(normal, input.SourcePosition, input.ToCamera);
	color *= lightPower.Diffuse;
	color += lightPower.Specular;

#ifdef DEPTH_BUFFER
	float alpha = clamp(waterDepth / _edgeFactor, 0.0f, 1.0f); // increase the soft Edges by increasing denominator
#else
	float alpha = 1.0;
#endif

	// color = float4(depthBlend, depthBlend, depthBlend, 1.0);
	// color = float4(normal.x, normal.y, normal.z, 1.0);
	// color = float4(refractiveFactor, refractiveFactor, refractiveFactor, 1.0);

	return float4(color, alpha);
}

float4 PSRefractionTexture(VSOutput input) : SV_Target0
{
	float4 screenTexCoord = ToNDC(input.PositionCopy);

	return SAMPLE_TEXTURE(_textureScreen, screenTexCoord.xy);
}

float4 PSReflectionTexture(VSOutput input) : SV_Target0
{
	float4 reflectionTexCoord = ToNDC(input.ReflectionPosition);

	return SAMPLE_TEXTURE(_textureReflection, reflectionTexCoord.xy);
}

float4 PSDepthTexture(VSOutput input) : SV_Target0
{
	float4 screenTexCoord = ToNDC(input.PositionCopy);

	return SAMPLE_TEXTURE(_textureDepth, screenTexCoord.xy);
}

TECHNIQUE(Color, VS, PSColor);
TECHNIQUE(RefractionTexture, VS, PSRefractionTexture);
TECHNIQUE(ReflectionTexture, VS, PSReflectionTexture);
TECHNIQUE(DepthTexture, VS, PSDepthTexture);