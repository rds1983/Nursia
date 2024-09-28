float4x4 _lightViewProj;
float _shadowDepthBias = 0.001f;
DECLARE_TEXTURE_POINT_CLAMP(_shadowMap);

float CalculateShadowFactor(float3 lightingPosition)
{
	if (lightingPosition.z > 1)
	{
		// Outside of shadow map
		return 0;
	}

	// Find the position in the shadow map for this pixel
	float2 ShadowTexCoord = 0.5 * lightingPosition.xy + float2( 0.5, 0.5 );
	ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

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
			float pcfDepth = SAMPLE_TEXTURE(_shadowMap, ShadowTexCoord + float2(x, y) * texelSize).r; 
			shadow += currentDepth > pcfDepth ? 1.0 : 0.0;        
		}    
	}

	shadow /= 9.0;

	return shadow;
}
