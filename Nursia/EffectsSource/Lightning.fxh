#define MAX_LIGHTS 32
#define MAX_POINT_LIGHT_LENGTH 100.0

int _lightType[MAX_LIGHTS];
float3 _lightPosition[MAX_LIGHTS];
float3 _lightDirection[MAX_LIGHTS];
float3 _lightColor[MAX_LIGHTS];
int _lightCount;

struct LightPower
{
	float3 Diffuse;
};

LightPower CalculateLightPower(float3 normal, float3 sourcePosition)
{
	LightPower result;
	result.Diffuse = float3(0, 0, 0);

	int lightCount = min(_lightCount, MAX_LIGHTS);
	for(int i = 0; i < lightCount; ++i)
	{
		int type = _lightType[i];

		float3 lightDirection = -_lightDirection[i];
		float3 lightColor = _lightColor[i];
		if (type == 1)
		{
			// Point Light
			float3 toLight = _lightPosition[i] - sourcePosition;
			float dist = length(toLight);
			
			if (dist >= MAX_POINT_LIGHT_LENGTH)
			{
				continue;
			}
			
			lightDirection = normalize(toLight);
			lightColor *= (MAX_POINT_LIGHT_LENGTH - dist) / MAX_POINT_LIGHT_LENGTH;
		}

		// Blinn-Phong
		float diffuseFactor = max(dot(normal, lightDirection), 0.0);
		float3 value = diffuseFactor * lightColor;
		result.Diffuse += value;
	}
	
	return result;
}