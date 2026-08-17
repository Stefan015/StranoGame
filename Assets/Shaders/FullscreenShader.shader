Shader "Fullscreen/VolumetricFog"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _ZFar("Camera Far", Float) = 50000
        _MaxDistance("Max distance", Float) = 100
        _Exponent("Exponent", Range(0,5)) = 2
        _StepSize("Step size", Range(0.1, 20)) = 1
        _DensityMultiplier("Density Multiplier", Range(0,1)) = 0.01
        _NoiseOffset("Noise offset", float) = 0
        _LightContribution("light contribution", Color) = (1,1,1,1)
        
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _Color;
            float _MaxDistance;
            float _StepSize;
            float _DensityMultiplier;
            float _zNear;
            float _ZFar;
            float _NoiseOffset;
            float _Exponent;
            float4 _LightContribution;

            float get_density()
            {
                return _DensityMultiplier;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                float col =  SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,IN.texcoord);

                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture,IN.texcoord);
                float linearDepth = Linear01Depth(rawDepth,_ZBufferParams);
                float logDepth = pow(1 - log(linearDepth * _ZFar + 1.0) / log(_ZFar + 1.0),_Exponent);

                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord,logDepth,UNITY_MATRIX_I_VP);

                float3 entryPoint = _WorldSpaceCameraPos;
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distLimit = min(viewLength,_MaxDistance);
                float distTravelled = InterleavedGradientNoise(pixelCoords,(int)(_Time.y/max(HALF_EPS, unity_DeltaTime.x)))*_NoiseOffset;
                float transmittance = 1;
                float4 fogCol = _Color;

                while (distTravelled < distLimit)
                {
                    float3 rayPos = entryPoint + rayDir *distTravelled;
                    float density = get_density();
                    if ( density > 0)
                    {
                        Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos));
                        fogCol.rgb += mainLight.color.rgb * _LightContribution.rgb  * density * mainLight.shadowAttenuation * _StepSize;
                        transmittance *= exp(-density*_StepSize);
                    }
                    distTravelled += _StepSize;
                }
                

                return lerp(col,fogCol,1.0- saturate(transmittance));
                
            }
            ENDHLSL
        }
    }
}