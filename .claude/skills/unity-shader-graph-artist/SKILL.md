---
name: unity-shader-graph-artist
description: "Visual effects and material specialist — Masters Unity Shader Graph, HLSL, URP/HDRP rendering pipelines, and custom pass authoring for real-time visual effects. Use when creating shaders, materials, dissolve effects, outlines, post-processing passes, or monochromatic palettes with selective color. Trigger keywords: shader, shader graph, HLSL, URP, HDRP, dissolve, outline, post-process, renderer feature, VFX, material, fresnel, dithering, pixel art shader."
globs: ["**/*.shader", "**/*.hlsl", "**/*Shader*.cs", "**/*RendererFeature*.cs", "**/*RenderPass*.cs", "**/*Material*.cs"]
---

# Unity Shader Graph Artist

Visual effects and material specialist. Builds Shader Graph materials that artists can drive and converts them to optimized HLSL when performance demands it.

---

## Reglas críticas

### Shader Graph
- **MANDATORY**: Usar Sub-Graphs para lógica repetida — clusters de nodos duplicados son fallo de mantenimiento
- Organizar nodos en grupos etiquetados: `Texturing`, `Lighting`, `Effects`, `Output`
- Exponer solo parámetros que el artista toca — encapsular cálculos internos en Sub-Graphs
- Todo parámetro expuesto debe tener tooltip en el Blackboard

### URP / HDRP
- Nunca usar shaders del pipeline built-in en proyectos URP/HDRP — usar equivalentes Lit/Unlit o Shader Graph propio
- URP custom passes: `ScriptableRendererFeature` + `ScriptableRenderPass` — nunca `OnRenderImage` (solo built-in)
- HDRP custom passes: `CustomPassVolume` + `CustomPass` — API diferente a URP, no intercambiables
- Verificar que el Render Pipeline asset está asignado correctamente en Material settings

### HLSL
- Archivos HLSL usan extensión `.hlsl` para includes, `.shader` para wrappers ShaderLab
- Declarar todas las propiedades `cbuffer` coincidiendo con el bloque `Properties` — los desajustes causan materiales negros silenciosos
- Usar macros `TEXTURE2D` / `SAMPLER` de `Core.hlsl` — `sampler2D` directo no es compatible con SRP

### Rendimiento
- Todo fragment shader debe perfilarse en Frame Debugger y GPU Profiler antes de ship
- Mobile: máx 32 texture samples por fragment pass; máx 60 ALU por fragment opaco
- Evitar derivadas `ddx`/`ddy` en shaders móviles — comportamiento indefinido en GPUs tile-based
- Preferir Alpha Clipping sobre Alpha Blend donde la calidad visual lo permita — evita problemas de depth sorting

---

## Componentes principales

### 1. Dissolve Shader Graph

```
Blackboard Parameters:
  [Texture2D] Base Map        — Albedo texture
  [Texture2D] Dissolve Map    — Noise texture driving dissolve
  [Float]     Dissolve Amount — Range(0,1), artist-driven
  [Float]     Edge Width      — Range(0,0.2)
  [Color]     Edge Color      — HDR enabled for emissive edge

Node Graph Structure:
  [Sample Texture 2D: DissolveMap] → [R channel] → [Subtract: DissolveAmount]
  → [Step: 0] → [Clip]  (drives Alpha Clip Threshold)

  [Subtract: DissolveAmount + EdgeWidth] → [Step] → [Multiply: EdgeColor]
  → [Add to Emission output]

Sub-Graph: "DissolveCore" encapsulates above for reuse across character materials
```

### 2. Custom URP Renderer Feature — Outline Pass

```csharp
public class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public Material outlineMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public OutlineSettings settings = new OutlineSettings();
    private OutlineRenderPass m_OutlinePass;

    public override void Create()
    {
        m_OutlinePass = new OutlineRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_OutlinePass);
    }
}

public class OutlineRenderPass : ScriptableRenderPass
{
    private readonly OutlineRendererFeature.OutlineSettings m_Settings;
    private RTHandle m_OutlineTexture;

    public OutlineRenderPass(OutlineRendererFeature.OutlineSettings settings)
    {
        m_Settings = settings;
        renderPassEvent = settings.renderPassEvent;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get("Outline Pass");
        // Blit con material de outline — samplea depth y normales para detección de bordes
        Blitter.BlitCameraTexture(cmd,
            renderingData.cameraData.renderer.cameraColorTargetHandle,
            m_OutlineTexture,
            m_Settings.outlineMaterial, 0);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
```

### 3. HLSL URP Lit Custom

```hlsl
// CustomLit.hlsl — URP-compatible physically based shader
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
TEXTURE2D(_NormalMap);  SAMPLER(sampler_NormalMap);
TEXTURE2D(_ORM);        SAMPLER(sampler_ORM);

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _BaseColor;
    float  _Smoothness;
CBUFFER_END

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv         : TEXCOORD0;
    float3 normalOS   : NORMAL;
    float4 tangentOS  : TANGENT;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv          : TEXCOORD0;
    float3 normalWS    : TEXCOORD1;
    float3 positionWS  : TEXCOORD2;
};

Varyings Vert(Attributes IN)
{
    Varyings OUT;
    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
    OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
    OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
    OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
    return OUT;
}

half4 Frag(Varyings IN) : SV_Target
{
    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
    half3 orm    = SAMPLE_TEXTURE2D(_ORM, sampler_ORM, IN.uv).rgb;

    InputData inputData = (InputData)0;
    inputData.normalWS          = normalize(IN.normalWS);
    inputData.positionWS        = IN.positionWS;
    inputData.viewDirectionWS   = GetWorldSpaceNormalizeViewDir(IN.positionWS);
    inputData.shadowCoord       = TransformWorldToShadowCoord(IN.positionWS);

    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo      = albedo.rgb;
    surfaceData.metallic    = orm.b;
    surfaceData.smoothness  = (1.0 - orm.g) * _Smoothness;
    surfaceData.occlusion   = orm.r;
    surfaceData.alpha       = albedo.a;

    return UniversalFragmentPBR(inputData, surfaceData);
}
```

### 4. Post-process monocromático con acentos de color (URP)

```csharp
// MonochromeAccentFeature.cs — desatura toda la escena excepto colores en rango definido
public class MonochromeAccentFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material monochromaticMaterial;
        [ColorUsage(false, true)] public Color accentColor = Color.red;
        [Range(0f, 1f)] public float hueRange = 0.1f;
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings settings = new();
    private MonochromePass m_Pass;

    public override void Create() => m_Pass = new MonochromePass(settings);

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.monochromaticMaterial != null)
            renderer.EnqueuePass(m_Pass);
    }
}

public class MonochromePass : ScriptableRenderPass
{
    private static readonly int k_AccentColor = Shader.PropertyToID("_AccentColor");
    private static readonly int k_HueRange    = Shader.PropertyToID("_HueRange");

    private readonly MonochromeAccentFeature.Settings m_Settings;

    public MonochromePass(MonochromeAccentFeature.Settings settings)
    {
        m_Settings = settings;
        renderPassEvent = settings.passEvent;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        m_Settings.monochromaticMaterial.SetColor(k_AccentColor, m_Settings.accentColor);
        m_Settings.monochromaticMaterial.SetFloat(k_HueRange, m_Settings.hueRange);

        var cmd = CommandBufferPool.Get("Monochrome Accent");
        Blitter.BlitCameraTexture(cmd,
            renderingData.cameraData.renderer.cameraColorTargetHandle,
            renderingData.cameraData.renderer.cameraColorTargetHandle,
            m_Settings.monochromaticMaterial, 0);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
```

```hlsl
// MonochromeAccent.hlsl — fragment shader para el pase de post-proceso
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_BlitTexture); SAMPLER(sampler_BlitTexture);

CBUFFER_START(UnityPerMaterial)
    float4 _AccentColor;
    float  _HueRange;
CBUFFER_END

// Convierte RGB a HSV
float3 RGBtoHSV(float3 c)
{
    float4 K = float4(0, -1.0/3.0, 2.0/3.0, -1);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + 1e-10)), d / (q.x + 1e-10), q.x);
}

half4 Frag(Varyings IN) : SV_Target
{
    half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv);
    float3 hsv  = RGBtoHSV(color.rgb);
    float3 accentHSV = RGBtoHSV(_AccentColor.rgb);

    // Distancia de tono en espacio circular
    float hueDist = abs(frac(hsv.x - accentHSV.x + 0.5) - 0.5) * 2.0;
    float isAccent = step(hueDist, _HueRange) * step(0.2, hsv.y); // saturation mínima

    // Desaturar todo excepto el acento
    float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
    half3 mono = half3(luminance, luminance, luminance);
    color.rgb = lerp(mono, color.rgb, isAccent);

    return color;
}
```

---

## Auditoría de complejidad de shader

```markdown
## Shader Review: [Nombre]

**Pipeline**: [ ] URP  [ ] HDRP  [ ] Built-in
**Plataforma objetivo**: [ ] PC  [ ] Console  [ ] Mobile

Texture Samples
- Fragment texture samples: ___ (límite mobile opaco: 32)

ALU Instructions
- ALU estimado: ___
- Budget mobile: ≤ 60 opaco / ≤ 40 transparente

Render State
- Blend Mode: [ ] Opaque  [ ] Alpha Clip  [ ] Alpha Blend
- Depth Write: [ ] On  [ ] Off
- Two-Sided: [ ] Yes

Sub-Graphs usados: ___
Parámetros expuestos documentados: [ ] Sí  [ ] No — BLOQUEADO hasta Sí
Variante fallback mobile existe: [ ] Sí  [ ] No  [ ] No requerido
```

---

## Workflow

1. **Design Brief → Shader Spec** — acordar target visual, plataforma y budget de rendimiento antes de abrir Shader Graph
2. **Shader Graph Authorship** — construir Sub-Graphs primero, cablear master graph con Sub-Graphs, no node soups planos
3. **Conversión a HLSL** (si se requiere) — usar "Copy Shader" de Shader Graph como referencia, aplicar macros URP/HDRP, eliminar code paths muertos
4. **Profiling** — Frame Debugger para verificar placement de draw calls, GPU profiler para tiempo de fragment
5. **Artist Handoff** — documentar todos los parámetros expuestos con rangos y descripción visual

---

## Skills relacionadas

| Necesidad | Skill |
|-----------|-------|
| Paleta pixel art y dithering | `pixel-art` |
| Iluminación y post-processing URP | `unity-lighting-vfx` |
| Animaciones de VFX | `unity-animation` |
| Performance y profiling | `unity-performance` |
| Efectos de sanidad/estrés visual | `horror-game` |
