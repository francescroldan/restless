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

        // ── Minigame ──────────────────────────────────────────────────────────
        public float markerSpeed;
        public float markerSpeedMax;
        public float greenZoneHalfWidth;
        public float greenZoneHalfWidthMin;
        public int   successesRequired;
        public int   failuresAllowed;
        /// <summary>Written by DreamPassiveApplier from ally minigameSpeedMultiplier.</summary>
        public float minigameSpeedMultiplier = 1f;

        // ── Ally modifiers (misc) ─────────────────────────────────────────────
        /// <summary>Written by DreamPassiveApplier. Applied by GameManager on exit.</summary>
        public float healthCostMultiplier  = 1f;
        public int   inventoryBonusCells   = 0;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public static RunConfig Create(GameConfig config)
        {
            Current = new RunConfig(config);
            return Current;
        }

        public static void Clear() => Current = null;

        private RunConfig(GameConfig cfg)
        {
            walkSpeed                     = cfg.walkSpeed;
            runSpeed                      = cfg.runSpeed;
            highRestlessnessAcceleration  = cfg.highRestlessnessAcceleration;
            maxRestlessnessAcceleration   = cfg.maxRestlessnessAcceleration;
            baseRestlessnessRate          = cfg.baseRestlessnessRate;
            minigameActiveMultiplier      = cfg.minigameActiveMultiplier;
            entitySpeed                   = cfg.entitySpeed;
            entityWaypointThreshold       = cfg.entityWaypointThreshold;
            markerSpeed                   = cfg.markerSpeed;
            markerSpeedMax                = cfg.markerSpeedMax;
            greenZoneHalfWidth            = cfg.greenZoneHalfWidth;
            greenZoneHalfWidthMin         = cfg.greenZoneHalfWidthMin;
            successesRequired             = cfg.successesRequired;
            failuresAllowed               = cfg.failuresAllowed;
        }
    }
}
