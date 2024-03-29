﻿#include "ShadowCommon.hlsl"

cbuffer VertexShaderBuffer : register(b0)
{
    float4x4 World; // 128 bytes
};


PS_INPUT VSMain(VS_INPUT input)
{
    PS_INPUT output;
    
    output.position = mul(float4(input.position, 1.0), World);
    
    return output;
}