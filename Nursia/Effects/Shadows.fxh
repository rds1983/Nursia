#define CASCADES_COUNT 4

float4x4 _lightViewProj;
float _shadowDepthBias = 0.001f;
DECLARE_TEXTURE_POINT_CLAMP(_shadowMap);
float2 _shadowMapPixelSize;

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

float CalculateShadowFactor(int cascadeIndex, float3 worldPos)
{
	// Determine pos in the light-view space
	float3 lightingPosition = mul(float4(worldPos, 1), _lightViewProjs[cascadeIndex]).xyz;
	if (lightingPosition.z < -1 || lightingPosition.z > 1)
	{
		// Outside of shadow map
		return 0;
	}

	// Find the position in the shadow map for this pixel
	float2 shadowTexCoord = 0.5 * lightingPosition.xy + float2( 0.5, 0.5 );
	shadowTexCoord.y = 1.0f - shadowTexCoord.y;

	// Apply cascadeIndex to x
	float cascadeSize = 1.0 / CASCADES_COUNT;
	shadowTexCoord.x = cascadeIndex * cascadeSize + shadowTexCoord.x / CASCADES_COUNT;

	// Calculate the current pixel depth
	// The bias is used to prevent folating point errors that occur when
	// the pixel of the occluder is being drawn
	float currentDepth = lightingPosition.z - _shadowDepthBias;
	float shadow = 0.0;
	float2 texelSize = float2(1.0 / 2048.0, 1.0 / 2048.0);

	[unroll]
	for(int x = -1; x <= 1; ++x)
	{
		[unroll]
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = SAMPLE_TEXTURE(_shadowMap, shadowTexCoord + float2(x, y) * _shadowMapPixelSize).r; 
			shadow += currentDepth > pcfDepth ? 1.0 : 0.0;        
		}    
	}

	shadow /= 9.0;

	return shadow;
}
