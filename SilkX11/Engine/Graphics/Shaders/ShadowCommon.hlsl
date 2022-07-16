struct VS_INPUT
{
    float3 position : POSITION;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
};

struct GS_OUTPUT
{
    float4 position : SV_POSITION;
    uint RTIndex : SV_RenderTargetArrayIndex;
};

struct PS_OUTPUT
{
    float4 color : SV_Target;
    float depth : SV_Depth;
};

float NormalizeDepth(float depth)
{
    return 0.01 / (1.01 - depth);
}