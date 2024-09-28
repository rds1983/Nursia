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

	// Get the current depth stored in the shadow map
	float shadowdepth = SAMPLE_TEXTURE(_shadowMap, ShadowTexCoord).r; 

	// Calculate the current pixel depth
	// The bias is used to prevent folating point errors that occur when
	// the pixel of the occluder is being drawn
	float ourdepth = lightingPosition.z - _shadowDepthBias;

	// Check to see if this pixel is in front or behind the value in the shadow map
	if (shadowdepth < ourdepth)
	{
		// Shadow the pixel by lowering the intensity
		return 1;
	};
	
	return 0;
}
