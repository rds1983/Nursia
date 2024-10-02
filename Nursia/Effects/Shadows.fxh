#define CASCADES_PER_ROW 2
#define CASCADES_COUNT 4

float4x4 _lightViewProj;
float _shadowDepthBias = 0.001f;
DECLARE_TEXTURE_POINT_CLAMP(_shadowMap1);
DECLARE_TEXTURE_POINT_CLAMP(_shadowMap2);
DECLARE_TEXTURE_POINT_CLAMP(_shadowMap3);
DECLARE_TEXTURE_POINT_CLAMP(_shadowMap4);
float _shadowMapSize;

float _cascadesDistances[CASCADES_COUNT];
float4x4 _lightViewProjs[CASCADES_COUNT];

int DetermineCascadeIndex(float distance)
{
	int cascadeIndex = 0;
	for(int i = 0;i < CASCADES_COUNT; ++i)
	{
		if (distance < _cascadesDistances[i])
		{
			cascadeIndex = i;
			break;
		}
	}

	return cascadeIndex;
}

float SampleShadowMap(sampler2D shadowMapSampler, float2 texCoord, float currentDepth)
{
	float shadowDepth = SAMPLE_TEXTURE(shadowMap, texCoord).r;

	return shadowDepth < currentDepth ? 1.0 : 0.0;
}

float CalculateShadowPcf2x2(sampler2D shadowMapSampler, float2 shadowTexCoord, float currentDepth)
{
	float pixelSize = 0.5 / _shadowMapSize;

	float samples[4];
	samples[0] = SampleShadowMap(shadowMapSampler, shadowTexCoord, currentDepth);
	samples[1] = SampleShadowMap(shadowMapSampler, shadowTexCoord + float2(pixelSize, 0), currentDepth);
	samples[2] = SampleShadowMap(shadowMapSampler, shadowTexCoord + float2(0, pixelSize), currentDepth);
	samples[3] = SampleShadowMap(shadowMapSampler, shadowTexCoord + float2(pixelSize, pixelSize), currentDepth);

	float2 vLerpFactor = frac(_shadowMapSize * shadowTexCoord.xy);
	return lerp(lerp(samples[0], samples[1], vLerpFactor.x),
				lerp(samples[2], samples[3], vLerpFactor.x),
				vLerpFactor.y);
}

float CalculateShadowFactor(int cascadeIndex, float3 worldPos)
{
	// Determine pos in the light-view space
	float3 lightingPosition = mul(float4(worldPos, 1), _lightViewProjs[cascadeIndex]).xyz;
	if (lightingPosition.x < -1 || lightingPosition.x > 1 ||
		lightingPosition.y < -1 || lightingPosition.y > 1 ||
		lightingPosition.z < -1 || lightingPosition.z > 1)
	{
		// Outside of shadow map
		return 0;
	}

	// Find the position in the shadow map for this pixel
	float2 shadowTexCoord = 0.5 * lightingPosition.xy + float2( 0.5, 0.5 );
	shadowTexCoord.y = 1.0f - shadowTexCoord.y;

	// Calculate the current pixel depth
	// The bias is used to prevent folating point errors that occur when
	// the pixel of the occluder is being drawn
	float currentDepth = lightingPosition.z;

	if (cascadeIndex < 1) {
		return CalculateShadowPcf2x2(_shadowMap1Sampler, shadowTexCoord, currentDepth);
	} else if (cascadeIndex < 2) {
		return CalculateShadowPcf2x2(_shadowMap2Sampler, shadowTexCoord, currentDepth);
	} else if (cascadeIndex < 3) {
		return CalculateShadowPcf2x2(_shadowMap3Sampler, shadowTexCoord, currentDepth);
	}

	return CalculateShadowPcf2x2(_shadowMap4Sampler, shadowTexCoord, currentDepth);

	// return SAMPLE_TEXTURE(_shadowMap, shadowTexCoord).r < currentDepth ? 1.0 : 0.0; 
}
