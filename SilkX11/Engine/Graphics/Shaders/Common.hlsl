static const float2 DiscSamples5[] =
{
    float2(0.000000, 2.500000),
    float2(2.377641, 0.772542),
    float2(1.469463, -2.022543),
    float2(-1.469463, -2.022542),
    float2(-2.377641, 0.772543),
};


struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 worldPosition : W_POSITION;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
};
struct PS_OUTPUT
{
    float4 color : SV_Target;
};

struct VS_INPUT
{
    float3 position : POSITION;
    float2 uv : TEXCOORD;
    float3 normal : NORMAL;
};

float NormalizeDepth(float depth)
{
    return 0.01 / (1.01 - depth);
}