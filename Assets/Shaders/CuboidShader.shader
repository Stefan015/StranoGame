Shader "Unlit/CuboidShader"
{
    Properties
    {
        _Color1("Color1", Color) = (0, 0, 0, 1)
        _Color2("Color2", Color) = (1, 1, 1, 1)
        _Scale("Factor", Range(-1,1)) = 0.1
        _MainTexture("Main Texture", 2D) = "white" {}
        [NoSCaleOffset] _NormalMap("normal map", 2D) = "bump" {}
        _Shininess("Shininess", Range(0,256)) = 4

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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 mainTexture : TEXCOORD1;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 mainTexture : TEXCOORD1;
                float3 normal : TEXCOORD3;
                float3 tangent : TEXCOORD4;
                float3 biTangent : TEXCOORD5;
                float3 worldPos : TEXCOORD6;
            };
            
            float4 _Color1;
            float4 _Color2;
            float _Scale;
            float _Shininess;
            
            sampler2D _MainTexture;
            sampler2D _BorderTexture;
            sampler2D _NormalMap;
            float4 _MainTexture_ST;
            float4 _BorderTexture_ST;
            float4 _NormalMap_ST;
            
            
            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.uv = v.uv*2-1;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.mainTexture = TRANSFORM_TEX(v.mainTexture, _MainTexture);
                o.normal = v.normal;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.tangent = TransformObjectToWorldDir(v.tangent.xyz);
                o.biTangent = cross(o.normal,o.tangent)*(v.tangent.w * unity_WorldTransformParams.w);
                
                return o;
            }
            
            float3 CalculateBlinnPhong(float3 normal, float3 viewDir, Light light, float shininess)
            {
                float3 lightDir = normalize(light.direction);
                float3 halfDir = normalize(lightDir + viewDir);
            
                float diffuse = saturate(dot(normal, lightDir));
            
                float specular = pow(saturate(dot(normal, halfDir)), shininess);
            
                return light.color.rgb * (diffuse + specular);
            }
            
            float4 frag (Interpolators i) : SV_Target
            {
                float4 color = tex2D(_MainTexture,i.mainTexture);
                float2 diff = abs(i.uv);
                float dst = max(diff.x, diff.y) - _Scale;

                float3 tangentSpaceNormal = UnpackNormal(tex2D(_NormalMap,i.mainTexture));

                float3x3 mtxTangToWorld ={
                    i.tangent.x, i.biTangent.x, i.normal.x,
                    i.tangent.y, i.biTangent.y, i.normal.y,
                    i.tangent.z, i.biTangent.z, i.normal.z
                };

                float3 normal = mul(mtxTangToWorld,tangentSpaceNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 lighting = CalculateBlinnPhong(normal, viewDir, GetMainLight(), _Shininess);
                for (int j=0 ; j < GetAdditionalLightsCount(); j ++)
                {
                    Light light = GetAdditionalLight(j,i.worldPos);
                    lighting+= CalculateBlinnPhong(normal, viewDir, light, _Shininess);
                }
                
                
                float4 resultColor = lerp(color *  _Color2,  color * _Color1, dst );
                
                return float4(resultColor.rgb * lighting, resultColor.a);
                
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
