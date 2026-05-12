Shader "Restless/DreamFog"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color      ("Tint",            Color)        = (0.55, 0.68, 0.88, 0.6)
        _NoiseScale ("Noise Scale",     Float)        = 4.0
        _NoiseSpeed ("Animation Speed", Float)        = 0.4
        _Distortion ("Edge Distortion", Range(0,1))   = 0.28
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull    Off
        Lighting Off
        ZWrite  Off
        Blend   One OneMinusSrcAlpha   // premultiplied alpha — matches SpriteRenderer default

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

            fixed4 _Color;
            float  _NoiseScale;
            float  _NoiseSpeed;
            float  _Distortion;

            // ── Noise ─────────────────────────────────────────────────────

            float2 _hash(float2 p)
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
                    lerp(dot(_hash(i),               f),
                         dot(_hash(i + float2(1,0)), f - float2(1,0)), u.x),
                    lerp(dot(_hash(i + float2(0,1)), f - float2(0,1)),
                         dot(_hash(i + float2(1,1)), f - float2(1,1)), u.x),
                    u.y);
            }

            // 3-octave fBm — gives wispy, multi-scale turbulence
            float _fbm(float2 p)
            {
                return _gnoise(p)                                      * 0.500
                     + _gnoise(p * 2.1 + float2(3.7,  1.9))           * 0.250
                     + _gnoise(p * 4.3 + float2(7.2,  8.1))           * 0.125;
            }

            // ── Vertex ────────────────────────────────────────────────────

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color    = IN.color * _Color;
                return OUT;
            }

            // ── Fragment ──────────────────────────────────────────────────

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord - 0.5;          // center at (0,0)
                float  t  = _Time.y * _NoiseSpeed;

                // Two layers of animated noise for independent turbulence axes
                float2 nUV = uv * _NoiseScale;
                float  n1  = _fbm(nUV + float2(t * 0.40, t * 0.30));
                float  n2  = _fbm(nUV + float2(t * 0.25, t * 0.45) + float2(5.3, 2.7));

                // Distort the radial distance with noise → wobbly edges
                float2 warp = float2(n1, n2) * _Distortion * 0.5;
                float  dist = length(uv + warp) * 2.0;

                // Soft outer fade
                float alpha = 1.0 - smoothstep(0.45, 1.0, dist);

                // Inner density variation (wisps and gaps)
                float inner = _fbm(nUV * 1.4 - float2(t * 0.18, t * 0.32));
                alpha      *= saturate(0.45 + 0.75 * (inner + 0.5));

                // Slow breathing pulse
                float pulse = 0.88 + 0.12 * sin(t * 1.5 + n1 * 2.0);
                alpha      *= pulse;

                // Premultiply (matches Blend One OneMinusSrcAlpha)
                fixed4 col  = IN.color;
                float  a    = col.a * alpha;
                col.rgb    *= a;
                col.a       = a;

                return col;
            }
            ENDCG
        }
    }
}
