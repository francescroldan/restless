Shader "Restless/MonochromeAccent"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Blend Off Cull Off

        Pass
        {
            Name "MonochromeAccent"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _AccentHue;
            float _HueRange;
            float _DesatStrength;
            float _AccentBoost;

            // Vision cone parameters (world space, ortho camera)
            float2 _CamCenter;        // camera XY world position
            float2 _CamExtent;        // half-width, half-height in world units
            float2 _ConePos;          // cone origin XY
            float2 _ConeDir;          // cone facing direction XY (normalised)
            float  _ConeCosHalfAngle; // cos(outerAngle * 0.5)
            float  _ConeRadius;       // max range
            float  _ConeMinRadius;    // always-full-colour radius around origin
            float  _ConeActive;       // 0 = off (just desaturate all), 1 = cone mask active

            // RGB → HSV (hue in 0..1)
            float3 RGBtoHSV(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)),
                              d / (q.x + e),
                              q.x);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);

                // ── Vision cone mask ─────────────────────────────────────────
                float coneMask = 0.0;
                if (_ConeActive > 0.5)
                {
                    // Reconstruct world XY from screen UV (orthographic camera)
                    float2 worldPos = _CamCenter + (input.texcoord * 2.0 - 1.0) * _CamExtent;
                    float2 toPixel  = worldPos - _ConePos;
                    float  dist     = length(toPixel);

                    if (dist <= _ConeMinRadius)
                    {
                        coneMask = 1.0;
                    }
                    else if (dist <= _ConeRadius)
                    {
                        // Angle check using dot product with cone direction
                        float cosAngle = dot(toPixel / (dist + 1e-6), _ConeDir);
                        // Soft edge on the angular boundary (±3° in cos space)
                        float angleMask = smoothstep(_ConeCosHalfAngle - 0.05, _ConeCosHalfAngle + 0.02, cosAngle);
                        // Soft falloff at the range boundary
                        float distMask  = 1.0 - smoothstep(_ConeRadius * 0.8, _ConeRadius, dist);
                        coneMask = angleMask * distMask;
                    }
                }

                // ── Hue-based accent ────────────────────────────────────────
                float3 hsv = RGBtoHSV(color.rgb);

                float hueDiff  = abs(hsv.x - _AccentHue);
                hueDiff = min(hueDiff, 1.0 - hueDiff);
                float accentMask = saturate(1.0 - hueDiff / max(_HueRange, 0.001));
                accentMask *= saturate(hsv.y * 4.0);

                // ── Combine: cone exempts from desaturation entirely ─────────
                float effectiveDesat = _DesatStrength * (1.0 - coneMask);

                float luma   = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float3 gray  = float3(luma, luma, luma);
                float3 result = lerp(gray, color.rgb, accentMask);
                result = lerp(color.rgb, result, effectiveDesat);
                result = lerp(result, result * _AccentBoost, accentMask * effectiveDesat);

                return half4(saturate(result), color.a);
            }
            ENDHLSL
        }
    }
}
