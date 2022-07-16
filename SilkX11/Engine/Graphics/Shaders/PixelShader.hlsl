#include "Common.hlsl"

struct PointLight
{
    float3 Position; // 12 bytes
    float Intensity; // 4 bytes
    float3 Color; // 12 bytes
    float padding1; // 4 bytes
    float3 Attenuation; // 12 bytes
    float padding2; // 4 bytes
    float4 padding3; // 16 bytes
};

cbuffer LightBuffer : register(b0)
{
    PointLight light; // 64 bytes
};

cbuffer DebugBuffer : register(b4)
{
    float3 value1;
};



SamplerState LinearSamplerView : SAMPLER : register(s0);
SamplerComparisonState ShadowSamplerView : SAMPLER : register(s1);
Texture2D MainTextureView : TEXTURE : register(t0);
TextureCube ShadowDepthTextureView : TEXTURE : register(t1);


float VectorToDepth(float3 vec, float nearZ, float farZ)
{
    float3 AbsVec = abs(vec);
    float LocalZcomp = max(AbsVec.x, max(AbsVec.y, AbsVec.z));
    float NormZComp = (farZ + nearZ) / (farZ - nearZ) - (2 * farZ * nearZ) / (farZ - nearZ) / LocalZcomp;
    return (NormZComp + 1.0) * 0.5;
}
float SampleShadowCube(float3 lightDirection, float shadowBias)
{
    float3 l = normalize(lightDirection);
    float sD = VectorToDepth(lightDirection, 0.1, 250.0);
    return ShadowDepthTextureView.SampleCmpLevelZero(ShadowSamplerView, float3(l.xy, -l.z), sD - shadowBias).r;
}
float SampleShadowCubeAA(float3 lightDirection, float eps)
{
    float3 l = normalize(lightDirection);
    
    float3 SideVector = normalize(cross(l, float3(0.0, 0.0, 1.0)));
    float3 UpVector = cross(SideVector, l);
    SideVector *= 1.0 / 2048.0;
    UpVector *= 1.0 / 2048.0;
    
    float sD = NormalizeDepth(VectorToDepth(lightDirection, 0.1, 250.0));
    float3 sDir = float3(l.xy, -l.z);
    float totalShadow = 0;

    [UNROLL]
    for (int i = 0; i < 5; ++i)
    {
        float3 SamplePos = sDir + SideVector * DiscSamples5[i].x + UpVector * DiscSamples5[i].y;
        totalShadow += ShadowDepthTextureView.SampleCmpLevelZero(
				ShadowSamplerView,
				SamplePos,
				sD - eps).r;
    }
    
    return totalShadow / 5.0;

}

PS_OUTPUT PSMain(PS_INPUT input) : SV_TARGET
{
    PS_OUTPUT output;
    
    float4 finalColor = float4(0.0, 0.0, 0.0, 1.0);
    
    float4 textureColor = MainTextureView.Sample(LinearSamplerView, input.uv);
    
    float3 lightVector = (input.worldPosition.xyz - light.Position);
    
    float3 lightDirection = normalize(light.Position - input.worldPosition.xyz);
    float3 ambientLight = float3(0.1, 0.1, 0.1);
    float NdotL = dot(lightDirection, input.normal);
    float diffuse = max(dot(lightDirection, input.normal), 0);
    float distanceToLight = distance(light.Position, input.worldPosition.xyz);
    float attenuationFactor = 1 /
        (light.Attenuation.x +
        light.Attenuation.y * distanceToLight +
        light.Attenuation.z * (distanceToLight * distanceToLight));
    
    diffuse *= attenuationFactor;
    float3 diffuseLighting = diffuse * light.Intensity * light.Color;
    
    // old shadow code
    //float2 shadowTexCoords;
    //shadowTexCoords.x = 0.5 + (input.lightWorldPosition.x / input.lightWorldPosition.w * 0.5);
    //shadowTexCoords.y = 0.5 - (input.lightWorldPosition.y / input.lightWorldPosition.w * 0.5);
    //float pixelDepth = input.lightWorldPosition.z / input.lightWorldPosition.w;
    //if (pixelDepth > 0.0)
    //{
     //ShadowDepthTextureView.SampleCmpLevelZero(ShadowSamplerView, shadowTexCoords, pixelDepth - epsilon).r;
    //float3 samplePos = float3(input.uv.x, value1.y, input.uv.y);
    //float depth = normalizeDepth(ShadowDepthTextureView.Sample(LinearSamplerView, samplePos).r);
    //output.color = float4(depth, depth, depth, 1.0);
    //}
    
    //float margin = acos(saturate(NdotL));
    //float epsilon = 0.003 / margin;
    //epsilon = clamp(epsilon, 0.0, 0.007);
    
    float shadowValue = SampleShadowCubeAA(lightVector, 0.004);
    
    finalColor = float4(textureColor.xyz * (ambientLight + (diffuseLighting * shadowValue)), textureColor.a);
    
    output.color = finalColor;
    
    return output;
}