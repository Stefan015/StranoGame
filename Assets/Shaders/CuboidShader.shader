Shader "Unlit/CuboidShader"
{
    Properties
    {
        _Color1("Color1", Color) = (0, 0, 0, 1)
        _Color2("Color2", Color) = (1, 1, 1, 1)
        _Scale("Factor", Range(-1,1)) = 0.1
        _InnerTexture("inner Texture", 2D) = "white" {}
        _BorderTexture("border Texture", 2D) = "white" {}
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
                float2 innerTextureCords : TEXCOORD1;
                float2 borderTextureCords : TEXCOORD2;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 innerTextureCords : TEXCOORD1;
                float2 borderTextureCords : TEXCOORD2;
                float3 normal : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
            };
            
            float4 _Color1;
            float4 _Color2;
            float _Scale;
            float _Shininess;
            
            sampler2D _InnerTexture;
            sampler2D _BorderTexture;
            float4 _InnerTexture_ST;
            float4 _BorderTexture_ST;
            
            
            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.innerTextureCords = TRANSFORM_TEX(v.innerTextureCords, _InnerTexture);
                o.borderTextureCords = v.borderTextureCords;
                o.normal = v.normal;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = v.uv*2-1;
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

                
                float4 innerColor = tex2D(_InnerTexture,i.innerTextureCords);
                float4 borderColor = tex2D(_BorderTexture,i.borderTextureCords);
                float2 diff = abs(i.uv);
                float dst = max(diff.x, diff.y) - _Scale;
                
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                
                float3 lighting = CalculateBlinnPhong(normalize(i.normal), viewDir, GetMainLight(), _Shininess);

                float4 resultColor = lerp(innerColor *  _Color2,  borderColor * _Color1,dst);

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
