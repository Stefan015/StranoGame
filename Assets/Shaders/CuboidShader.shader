Shader "Unlit/CuboidShader"
{
    Properties
    {
        _Color1("Color1", Color) = (0, 0, 0, 1)
        _Color2("Color2", Color) = (1, 1, 1, 1)
        _NoiseScale("Noise Scale", Float) = 0.001
        _Ambient("Ambient", Color) = (0.02, 0.02, 0.03, 1)
        _BumpScale("Bump Scale", Float) = 0.02
        _BumpStrength("Bump Strength", Range(0, 3)) = 0.6
        _BumpFadeDistance("Bump Fade Distance", Float) = 8000

    }
    SubShader
    {
        Tags {
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float4 _Color1;
            float4 _Color2;
            float _NoiseScale;
            float4 _Ambient;
            float _BumpScale;
            float _BumpStrength;
            float _BumpFadeDistance;


            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            float3 hash3(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yxz + 33.33);
                return -1.0 + 2.0 * frac((p.xxy + p.yxx) * p.zyx);
            }

            float gnoise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                float3 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(lerp(dot(hash3(i + float3(0,0,0)), f - float3(0,0,0)),
                                    dot(hash3(i + float3(1,0,0)), f - float3(1,0,0)), u.x),
                                lerp(dot(hash3(i + float3(0,1,0)), f - float3(0,1,0)),
                                    dot(hash3(i + float3(1,1,0)), f - float3(1,1,0)), u.x), u.y),
                            lerp(lerp(dot(hash3(i + float3(0,0,1)), f - float3(0,0,1)),
                                    dot(hash3(i + float3(1,0,1)), f - float3(1,0,1)), u.x),
                                lerp(dot(hash3(i + float3(0,1,1)), f - float3(0,1,1)),
                                    dot(hash3(i + float3(1,1,1)), f - float3(1,1,1)), u.x), u.y), u.z);
            }

            float fbm(float3 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                for (int k = 0; k < 5; k++)
                {
                    value += amplitude * gnoise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float3 bp = i.worldPos * _BumpScale;
                float e = 0.01;
                float h0 = fbm(bp);
                float3 grad = float3(fbm(bp + float3(e, 0, 0)) - h0,
                                     fbm(bp + float3(0, e, 0)) - h0,
                                     fbm(bp + float3(0, 0, e)) - h0) / e;

                float camDist = length(_WorldSpaceCameraPos - i.worldPos);
                float bumpFade = saturate(1.0 - camDist / _BumpFadeDistance);

                float3 normal = normalize(i.normal);
                float3 surfaceGrad = grad - dot(grad, normal) * normal;
                normal = normalize(normal - _BumpStrength * bumpFade * surfaceGrad);

                float3 lighting = _Ambient.rgb;
                Light mainLight = GetMainLight();
                lighting += mainLight.color.rgb * saturate(dot(normal, normalize(mainLight.direction)));
                for (int j = 0; j < GetAdditionalLightsCount(); j++)
                {
                    Light light = GetAdditionalLight(j, i.worldPos);
                    lighting += light.color.rgb * saturate(dot(normal, normalize(light.direction))) * light.distanceAttenuation;
                }

                float noise = fbm(i.worldPos * _NoiseScale) * 0.5 + 0.5;
                float3 surfaceColor = lerp(_Color1.rgb, _Color2.rgb, noise);

                return float4(surfaceColor * lighting, 1.0);
            }
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }
}
