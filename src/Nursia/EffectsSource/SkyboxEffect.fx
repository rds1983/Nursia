#include "Macros.fxh"

DECLARE_CUBEMAP(_texture, 0);

BEGIN_CONSTANTS

#ifdef CLIP_PLANE

float4 _clipPlane;

#endif

MATRIX_CONSTANTS

float4x4 _transform;

END_CONSTANTS

struct VSInput
{
    float4 Position : SV_POSITION;
};

struct VSOutput
{
    float4 Position: SV_POSITION;
    float3 TexCoord: TEXCOORD0;

#ifdef CLIP_PLANE
    float4 ClipDistances: TEXCOORD2;
#endif
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output = (VSOutput)0;
    
    output.Position = mul(input.Position, _transform);
    output.TexCoord = input.Position.xyz;

#ifdef CLIP_PLANE
    output.ClipDistances.yzw = 0;
    output.ClipDistances.x = dot(output.Position, _clipPlane); 
#endif

    return output;
}

float4 PixelShaderFunction(VSOutput input) : SV_Target0
{
#ifdef CLIP_PLANE
    clip(input.ClipDistances); 
#endif

    float4 color = SAMPLE_CUBEMAP(_texture, input.TexCoord);

	return color;
}

TECHNIQUE(Default, VertexShaderFunction, PixelShaderFunction);