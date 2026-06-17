﻿Shader "Custom/ToonCelShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _LightColor ("Light Color", Color) = (1, 1, 1, 1)
        _Texture ("Main Texture", 2D) = "white" {}

        _HighlightThreshold ("Highlight Threshold", Range(0, 1)) = 0.8
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.4
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.3

        _UseOutline ("Outline", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.1)) = 0.02

        _UseWorldCoordUV ("Use World Coords for UV", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual
            ColorMask RGB
			Offset 2, 2

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float _OutlineThickness;
            float4 _OutlineColor;
			float _UseOutline;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 norm = normalize(IN.normalOS);
                float3 offset = norm * _OutlineThickness;
                float4 pos = IN.positionOS + float4(offset, 0);
                OUT.positionHCS = TransformObjectToHClip(pos);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
				if (_UseOutline < 0.5 || _OutlineThickness == 0.0)
					discard;

                return _OutlineColor;
            }
            ENDHLSL
        }

        // Pass 2: Main Toon Lighting
        Pass
        {
            Name "ToonPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            sampler2D _Texture;
            float4 _Texture_ST;
            float4 _BaseColor;
            float4 _LightColor;
            float _HighlightThreshold;
            float _ShadowThreshold;
            float _ShadowStrength;
            float _UseWorldCoordUV;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _Texture);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float3 normal = normalize(IN.normalWS);
                float NdotL = saturate(dot(normal, lightDir));

                float4 texColor;

                if (_UseWorldCoordUV < 0.5) {
                    texColor = tex2D(_Texture, IN.uv);
                } else {
                    float3 worldPos = IN.positionWS;
                    float scale = .5; // Controls tiling

                    // Sample in each plane
                    float2 uvXZ = worldPos.xz * scale;
                    float2 uvXY = worldPos.xy * scale;
                    float2 uvYZ = worldPos.yz * scale;

                    float3 xProj = tex2D(_Texture, uvYZ).rgb; // X projection (YZ plane)
                    float3 yProj = tex2D(_Texture, uvXZ).rgb; // Y projection (XZ plane)
                    float3 zProj = tex2D(_Texture, uvXY).rgb; // Z projection (XY plane)

                    // Blend weights based on normal
                    float3 blend = abs(normal);
                    blend = pow(blend, 4.0); // Sharpen blending for cleaner edges
                    blend /= (blend.x + blend.y + blend.z); // Normalize

                    float3 triplanarColor = xProj * blend.x + yProj * blend.y + zProj * blend.z;
                    texColor = float4(triplanarColor, 1.0);
                }


                float4 baseColor = texColor * _BaseColor;

                float3 finalColor;
                if (NdotL >= _HighlightThreshold)
                {
                    finalColor = baseColor.rgb * _LightColor.rgb * 1.2;
                }
                else if (NdotL >= _ShadowThreshold)
                {
                    finalColor = baseColor.rgb * _LightColor.rgb;
                }
                else
                {
                    finalColor = baseColor.rgb * _LightColor.rgb * _ShadowStrength;
                }

                return float4(finalColor, baseColor.a);
            }
            ENDHLSL
        }
    }
}
