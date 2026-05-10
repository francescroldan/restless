using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Restless.Core
{
    public class MonochromeAccentFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [ColorUsage(false, true)]
            public Color accentColor = new Color(0.961f, 0.773f, 0.259f, 1f); // #F5C542

            [Range(0f, 0.3f)]
            public float hueRange = 0.07f;

            [Range(0f, 1f)]
            public float desatStrength = 1f;

            [Range(1f, 2f)]
            public float accentBoost = 1.25f;

            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public Settings settings = new Settings();

        private MonochromeAccentPass _pass;

        public override void Create()
        {
            _pass = new MonochromeAccentPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Preview) return;
            if (_pass == null) return;
            _pass.UpdateSettings(settings);
            renderer.EnqueuePass(_pass);
        }


        protected override void Dispose(bool disposing)
        {
            _pass?.Dispose();
        }

        // ── Render Pass ──────────────────────────────────────────────────

        class MonochromeAccentPass : ScriptableRenderPass
        {
            private Material _material;
            private RTHandle _tempRT;
            private Settings _settings;

            private static readonly int AccentHue        = Shader.PropertyToID("_AccentHue");
            private static readonly int HueRange         = Shader.PropertyToID("_HueRange");
            private static readonly int DesatStrength    = Shader.PropertyToID("_DesatStrength");
            private static readonly int AccentBoost      = Shader.PropertyToID("_AccentBoost");
            private static readonly int CamCenter        = Shader.PropertyToID("_CamCenter");
            private static readonly int CamExtent        = Shader.PropertyToID("_CamExtent");
            private static readonly int ConePos          = Shader.PropertyToID("_ConePos");
            private static readonly int ConeDir          = Shader.PropertyToID("_ConeDir");
            private static readonly int ConeCosHalfAngle = Shader.PropertyToID("_ConeCosHalfAngle");
            private static readonly int ConeRadius       = Shader.PropertyToID("_ConeRadius");
            private static readonly int ConeMinRadius    = Shader.PropertyToID("_ConeMinRadius");
            private static readonly int ConeActive       = Shader.PropertyToID("_ConeActive");

            private Dream.VisionCone _cone;

            // Render Graph pass data
            private class PassData
            {
                public Material material;
                public TextureHandle source;
                public TextureHandle temp;
            }

            public MonochromeAccentPass(Settings settings)
            {
                _settings = settings;
                renderPassEvent = settings.renderPassEvent;
                profilingSampler = new ProfilingSampler("MonochromeAccent");

                var shader = Shader.Find("Restless/MonochromeAccent");
                if (shader != null)
                    _material = CoreUtils.CreateEngineMaterial(shader);
                else
                    Debug.LogWarning("[MonochromeAccentPass] Shader 'Restless/MonochromeAccent' not found.");
            }

            public void UpdateSettings(Settings s) => _settings = s;

            private void SetMaterialProperties()
            {
                if (_material == null) return;

                Color.RGBToHSV(_settings.accentColor, out float hue, out _, out _);
                _material.SetFloat(AccentHue,     hue);
                _material.SetFloat(HueRange,      _settings.hueRange);
                _material.SetFloat(DesatStrength, _settings.desatStrength);
                _material.SetFloat(AccentBoost,   _settings.accentBoost);

                // Locate the VisionCone if not cached
                if (_cone == null)
                    _cone = Object.FindAnyObjectByType<Dream.VisionCone>();

                if (_cone != null)
                {
                    var cam = Camera.main;
                    if (cam != null && cam.orthographic)
                    {
                        float halfH = cam.orthographicSize;
                        float halfW = halfH * cam.aspect;
                        _material.SetVector(CamCenter, new Vector4(cam.transform.position.x, cam.transform.position.y, 0, 0));
                        _material.SetVector(CamExtent, new Vector4(halfW, halfH, 0, 0));
                    }

                    Vector2 dir          = _cone.transform.up;
                    float   halfAngleRad = _cone.OuterAngle * 0.5f * Mathf.Deg2Rad;

                    _material.SetVector(ConePos,    new Vector4(_cone.transform.position.x, _cone.transform.position.y, 0, 0));
                    _material.SetVector(ConeDir,    new Vector4(dir.x, dir.y, 0, 0));
                    _material.SetFloat(ConeCosHalfAngle, Mathf.Cos(halfAngleRad));
                    _material.SetFloat(ConeRadius,       _cone.Range);
                    _material.SetFloat(ConeMinRadius,    _cone.MinRadius);
                    _material.SetFloat(ConeActive,       1f);
                }
                else
                {
                    _material.SetFloat(ConeActive, 0f);
                }
            }

            // ── Render Graph path (URP 17 / Unity 6) ─────────────────────

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (_material == null) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer) return;

                SetMaterialProperties();

                var source = resourceData.activeColorTexture;
                var desc = renderGraph.GetTextureDesc(source);
                desc.name = "_MonoAccentTemp";
                desc.clearBuffer = false;
                var temp = renderGraph.CreateTexture(desc);

                using (var builder = renderGraph.AddUnsafePass<PassData>("MonochromeAccent", out var passData))
                {
                    passData.material = _material;
                    passData.source   = source;
                    passData.temp     = temp;

                    builder.UseTexture(source, AccessFlags.ReadWrite);
                    builder.UseTexture(temp,   AccessFlags.ReadWrite);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc(static (PassData data, UnsafeGraphContext ctx) =>
                    {
                        var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                        Blitter.BlitCameraTexture(cmd, data.source, data.temp, data.material, 0);
                        Blitter.BlitCameraTexture(cmd, data.temp, data.source);
                    });
                }
            }

            // ── Compatibility path (Execute / RTHandle) ───────────────────

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                RenderingUtils.ReAllocateIfNeeded(ref _tempRT, desc, name: "_MonoAccentTemp");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_material == null || _tempRT == null) return;

                SetMaterialProperties();

                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, profilingSampler))
                {
                    var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
                    Blitter.BlitCameraTexture(cmd, source, _tempRT, _material, 0);
                    Blitter.BlitCameraTexture(cmd, _tempRT, source);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd) { }

            public void Dispose()
            {
                _tempRT?.Release();
                CoreUtils.Destroy(_material);
            }
        }
    }
}
