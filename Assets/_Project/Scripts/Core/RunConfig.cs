namespace Restless.Core
{
    /// <summary>
    /// Per-run configuration: created at dream start from GameConfig, then mutated by
    /// DreamPassiveApplier. All dream systems read from RunConfig.Current when available,
    /// falling back to their own [SerializeField] values in Edit Mode / test scenes.
    /// Nulled automatically when the Dream scene bootstrapper is destroyed.
    /// </summary>
    public class RunConfig
    {
        public static RunConfig Current { get; private set; }

        // ── Player ────────────────────────────────────────────────────────────
        public float walkSpeed;
        public float runSpeed;

        // ── Vision cone ───────────────────────────────────────────────────────
        public float visionConeRange;
        public float visionConeOuterAngle;
        public float visionConeMinRadius;

        // ── Dream Timer ───────────────────────────────────────────────────────
        public float dreamDuration;
        public float highRestlessnessAcceleration;
        public float maxRestlessnessAcceleration;

        // ── Restlessness ──────────────────────────────────────────────────────
        public float baseRestlessnessRate;
        public float minigameActiveMultiplier;
        /// <summary>Written by DreamPassiveApplier from ally restlessnessRateModifier.</summary>
        public float restlessnessPassiveMultiplier = 1f;

        // ── Entity ────────────────────────────────────────────────────────────
        public float entitySpeed;
        public float entityWaypointThreshold;
        public int   entitySpawnCount;
        public float entityHauntedFraction;

        // ── Minigame ──────────────────────────────────────────────────────────
        public float markerSpeed;
        public float markerSpeedMax;
        public float greenZoneHalfWidth;
        public float greenZoneHalfWidthMin;
        public int   successesRequired;
        public int   failuresAllowed;
        /// <summary>Written by DreamPassiveApplier from ally minigameSpeedMultiplier.</summary>
        public float minigameSpeedMultiplier = 1f;

        // ── Audio ─────────────────────────────────────────────────────────────
        public float ambientVolumeCalm;
        public float ambientVolumeCritical;

        // ── Visual FX ─────────────────────────────────────────────────────────
        public float vignetteIdle;
        public float vignetteCritical;
        public float vignettePulseAmplitude;
        public float vignettePulseSpeedHigh;
        public float vignettePulseSpeedCritical;
        public float chromaticMedium;
        public float chromaticHigh;
        public float chromaticCritical;
        public float lensDistortionHigh;
        public float lensDistortionCritical;
        public float thresholdFlashDuration;
        public float thresholdFlashAlphaMax;
        public float maxVeilBaseAlpha;
        public float maxVeilPulseDepth;
        public float maxVeilPulseRate;
        public float buzzChromaticStrength;
        public float buzzVignetteStrength;
        public float buzzDecaySpeed;
        public float fxLerpSpeed;

        // ── Ally modifiers (misc) ─────────────────────────────────────────────
        /// <summary>Written by DreamPassiveApplier. Applied by GameManager on exit.</summary>
        public float healthCostMultiplier = 1f;
        public int   inventoryBonusCells  = 0;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public static RunConfig Create(GameConfig config)
        {
            Current = new RunConfig(config);
            return Current;
        }

        public static void Clear() => Current = null;

        private RunConfig(GameConfig cfg)
        {
            walkSpeed                    = cfg.walkSpeed;
            runSpeed                     = cfg.runSpeed;
            visionConeRange              = cfg.visionConeRange;
            visionConeOuterAngle         = cfg.visionConeOuterAngle;
            visionConeMinRadius          = cfg.visionConeMinRadius;
            highRestlessnessAcceleration = cfg.highRestlessnessAcceleration;
            maxRestlessnessAcceleration  = cfg.maxRestlessnessAcceleration;
            dreamDuration                = 0f; // set by DreamSceneBootstrap from ProtagonistState
            baseRestlessnessRate         = cfg.baseRestlessnessRate;
            minigameActiveMultiplier     = cfg.minigameActiveMultiplier;
            entitySpeed                  = cfg.entitySpeed;
            entityWaypointThreshold      = cfg.entityWaypointThreshold;
            entitySpawnCount             = cfg.entitySpawnCount;
            entityHauntedFraction        = cfg.entityHauntedFraction;
            markerSpeed                  = cfg.markerSpeed;
            markerSpeedMax               = cfg.markerSpeedMax;
            greenZoneHalfWidth           = cfg.greenZoneHalfWidth;
            greenZoneHalfWidthMin        = cfg.greenZoneHalfWidthMin;
            successesRequired            = cfg.successesRequired;
            failuresAllowed              = cfg.failuresAllowed;
            ambientVolumeCalm            = cfg.ambientVolumeCalm;
            ambientVolumeCritical        = cfg.ambientVolumeCritical;
            vignetteIdle                 = cfg.vignetteIdle;
            vignetteCritical             = cfg.vignetteCritical;
            vignettePulseAmplitude       = cfg.vignettePulseAmplitude;
            vignettePulseSpeedHigh       = cfg.vignettePulseSpeedHigh;
            vignettePulseSpeedCritical   = cfg.vignettePulseSpeedCritical;
            chromaticMedium              = cfg.chromaticMedium;
            chromaticHigh                = cfg.chromaticHigh;
            chromaticCritical            = cfg.chromaticCritical;
            lensDistortionHigh           = cfg.lensDistortionHigh;
            lensDistortionCritical       = cfg.lensDistortionCritical;
            thresholdFlashDuration       = cfg.thresholdFlashDuration;
            thresholdFlashAlphaMax       = cfg.thresholdFlashAlphaMax;
            maxVeilBaseAlpha             = cfg.maxVeilBaseAlpha;
            maxVeilPulseDepth            = cfg.maxVeilPulseDepth;
            maxVeilPulseRate             = cfg.maxVeilPulseRate;
            buzzChromaticStrength        = cfg.buzzChromaticStrength;
            buzzVignetteStrength         = cfg.buzzVignetteStrength;
            buzzDecaySpeed               = cfg.buzzDecaySpeed;
            fxLerpSpeed                  = cfg.fxLerpSpeed;
        }
    }
}
