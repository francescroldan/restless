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

            // RGB → HSV (returns hue in 0..1)
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

                float3 hsv = RGBtoHSV(color.rgb);

                // Hue distance (wraps at 0/1 boundary)
                float hueDiff = abs(hsv.x - _AccentHue);
                hueDiff = min(hueDiff, 1.0 - hueDiff);

                // 1 = fully accent, 0 = no accent
                float accentMask = saturate(1.0 - hueDiff / max(_HueRange, 0.001));

                // Only treat as accent if saturation is meaningful (avoids grey pixels matching)
                accentMask *= saturate(hsv.y * 4.0);

                // Desaturate non-accent pixels
                float luma = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float3 gray   = float3(luma, luma, luma);
                float3 result = lerp(gray, color.rgb, accentMask);

                // Mix desaturation strength (0 = full colour, 1 = full mono+accent)
                result = lerp(color.rgb, result, _DesatStrength);

                // Slightly boost accent saturation so it pops against grey
                result = lerp(result, result * _AccentBoost, accentMask * _DesatStrength);

                return half4(saturate(result), color.a);
            }
            ENDHLSL
        }
    }
}
