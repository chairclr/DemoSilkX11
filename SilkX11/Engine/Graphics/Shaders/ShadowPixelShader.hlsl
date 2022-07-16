#include "ShadowCommon.hlsl"

PS_OUTPUT PSMain(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;
    
    output.depth = NormalizeDepth(input.position.z);
    
    return output;
}