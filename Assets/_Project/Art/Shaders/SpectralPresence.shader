Shader "Restless/SpectralPresence"
{
    Properties
    {
        [PerRendererData] _MainTex  ("Sprite Texture",    2D)          = "white" {}
        _Color          ("Spectral Tint",   Color)        = (0.55, 0.68, 0.88, 1.0)
        _Manifestation  ("Manifestation",   Range(0,1))   = 0.0
        _NoiseScale     ("Noise Scale",     Float)        = 3.0
        _NoiseSpeed     ("Noise Speed",     Float)        = 0.7
        _Distortion     ("UV Distortion",   Range(0,3))   = 1.4
        _FlickerAmp     ("Flicker Amplitude", Range(0,1)) = 0.40
        _FlickerSpeed   ("Flicker Speed",   Float)        = 5.5
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull    Off
        Lighting Off
        ZWrite  Off
        Blend   One OneMinusSrcAlpha   // premultiplied — matches SpriteRenderer default

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4    _Color;
            float     _Manifestation;
            float     _NoiseScale;
            float     _NoiseSpeed;
            float     _Distortion;
            float     _FlickerAmp;
            float     _FlickerSpeed;

            // ── Noise ─────────────────────────────────────────────────────────

            float2 _hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453) * 2.0 - 1.0;
            }

            float _gnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(dot(_hash2(i),               f),
                         dot(_hash2(i + float2(1,0)), f - float2(1,0)), u.x),
                    lerp(dot(_hash2(i + float2(0,1)), f - float2(0,1)),
                         dot(_hash2(i + float2(1,1)), f - float2(1,1)), u.x),
                    u.y);
            }

            float _fbm(float2 p)
            {
                return _gnoise(p)                                    * 0.500
                     + _gnoise(p * 2.1 + float2(3.7, 1.9))          * 0.250
                     + _gnoise(p * 4.3 + float2(7.2, 8.1))          * 0.125;
            }

            // ── Vertex ────────────────────────────────────────────────────────

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color    = IN.color;
                return OUT;
            }

            // ── Fragment ──────────────────────────────────────────────────────

            fixed4 frag(v2f IN) : SV_Target
            {
                float  t    = _Time.y * _NoiseSpeed;
                float2 uv   = IN.texcoord;
                float2 cuv  = uv - 0.5;

                // instability = 1 when fully spectral, 0 when fully manifested
                float instability = 1.0 - _Manifestation;

                // ── Two noise layers for warp ─────────────────────────────────
                float2 nUV = cuv * _NoiseScale;
                float  n1  = _fbm(nUV + float2(t * 0.38, t * 0.29));
                float  n2  = _fbm(nUV + float2(t * 0.24, t * 0.43) + float2(5.1, 2.9));

                // UV distortion — heavy when spectral, gone when manifested
                float2 warp       = float2(n1, n2) * _Distortion * instability * 0.5;
                float2 distortedUV = uv + warp;

                // Sample the actual sprite at distorted UV
                // (clamp so edges don't wrap)
                distortedUV = clamp(distortedUV, 0.001, 0.999);
                fixed4 sprite = tex2D(_MainTex, distortedUV);

                // ── Spectral blob shape (visible even when sprite is blank) ───
                // Used as the base alpha when manifestation is low
                float dist     = length(cuv) * 2.0;
                float blobMask = 1.0 - smoothstep(0.25, 0.95, dist);
                float blobDensity = _fbm(nUV * 1.4 - float2(t * 0.18, t * 0.31));
                float blobAlpha = blobMask * saturate(0.45 + 0.75 * (blobDensity + 0.5));

                // ── Organic flicker ────────────────────────────────────────────
                float flicker = 1.0 - _FlickerAmp * instability
                              * (0.5 + 0.5 * sin(t * _FlickerSpeed + n1 * 7.0));

                // ── Combine alpha ──────────────────────────────────────────────
                // lerp: at manifestation=0 → blob shape; at 1 → actual sprite alpha
                float alpha = lerp(blobAlpha, sprite.a, _Manifestation) * flicker * IN.color.a;

                // ── Combine color ──────────────────────────────────────────────
                // At manifestation=0: pure spectral tint
                // At manifestation=1: actual sprite color
                fixed3 col = lerp(_Color.rgb, sprite.rgb * IN.color.rgb, _Manifestation);

                // Scale tint alpha influence: spectral tint darkens when unobserved
                float tintAlphaScale = lerp(0.75, 1.0, _Manifestation);
                float a = saturate(alpha * _Color.a * tintAlphaScale);

                // Premultiply (matches Blend One OneMinusSrcAlpha)
                return fixed4(col * a, a);
            }
            ENDCG
        }
    }
}
