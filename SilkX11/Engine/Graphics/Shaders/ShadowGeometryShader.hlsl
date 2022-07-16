#include "ShadowCommon.hlsl"

cbuffer GeometryShaderBuffer : register(b0)
{
    float4x4 View[6];
    float4x4 Projection;
};

[maxvertexcount(18)]
void GSMain(triangle PS_INPUT input[3], inout TriangleStream<GS_OUTPUT> CubeMapStream)
{
	[unroll]
    for (int i = 0; i < 6; i++)
    {
        GS_OUTPUT output = (GS_OUTPUT) 0;
        output.RTIndex = i;
		[unroll]
        for (int v = 0; v < 3; v++)
        {
            float4 worldPosition = input[v].position;
            
            float4 viewPosition = mul(worldPosition, View[i]);
            
            output.position = mul(viewPosition, Projection);
            
            CubeMapStream.Append(output);
        }
        CubeMapStream.RestartStrip();
    }
}