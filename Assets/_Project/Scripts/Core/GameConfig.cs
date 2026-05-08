using UnityEngine;

namespace Restless.Core
{
    [CreateAssetMenu(menuName = "Restless/Game Config")]
    public class GameConfig : ScriptableObject
    {
        // =====================================================================
        // PROTAGONIST
        // =====================================================================
        [Header("── Protagonist ──────────────────────────────────────")]
        [Tooltip("Visual scale of the protagonist sprite (1 = base size).")]
        public float protagonistScale   = 1f;
        [Tooltip("Movement speed while walking (units/s).")]
        public float walkSpeed          = 3f;
        [Tooltip("Movement speed while running (units/s).")]
        public float runSpeed           = 5.5f;
        [Tooltip("Look direction interpolation speed.")]
        public float lookLerpSpeed      = 8f;
        [Tooltip("Interaction range for MemoryPoints and other objects.")]
        public float interactRange      = 1.5f;

        // =====================================================================
        // VISION CONE
        // =====================================================================
        [Header("── Vision Cone ──────────────────────────────────────")]
        [Tooltip("Maximum range of the vision cone light (units).")]
        public float visionConeRange          = 8f;
        [Tooltip("Outer angle of the vision cone (degrees).")]
        public float visionConeOuterAngle     = 110f;
        [Tooltip("Minimum always-visible radius around the protagonist.")]
        public float visionConeMinRadius      = 1.2f;
        [Tooltip("Intensity of the cone light.")]
        public float visionConeIntensity      = 1f;
        [Tooltip("Radius of the always-on halo light around the protagonist.")]
        public float haloRadius               = 1.2f;
        [Tooltip("Intensity of the halo light.")]
        public float haloIntensity            = 0.4f;

        // =====================================================================
        // LIGHTING — DREAM
        // =====================================================================
        [Header("── Lighting — Dream ────────────────────────────────")]
        [Tooltip("Base intensity of the global ambient light in the Dream (darkness floor).")]
        public float dreamAmbientBaseIntensity   = 0.05f;
        [Tooltip("Maximum ambient intensity (at low restlessness).")]
        public float dreamAmbientMaxIntensity    = 0.18f;
        [Tooltip("Speed at which ambient light transitions with restlessness.")]
        public float dreamAmbientTransitionSpeed = 1f;

        // =====================================================================
        // LIGHTING — VIGIL
        // =====================================================================
        [Header("── Lighting — Vigil ─────────────────────────────────")]
        [Tooltip("Room brightness when mental health is at maximum.")]
        public float vigiliaLightHealthy  = 0.18f;
        [Tooltip("Room brightness when mental health is at minimum.")]
        public float vigiliaLightDepleted = 0.05f;

        // =====================================================================
        // RESTLESSNESS
        // =====================================================================
        [Header("── Restlessness ─────────────────────────────────────")]
        [Tooltip("Base restlessness increase rate per second in zone 1 (no modifiers).")]
        public float baseRestlessnessRate     = 0.5f;
        [Tooltip("Rate multiplier while a minigame is active.")]
        public float minigameActiveMultiplier = 2.5f;
        [Tooltip("Rate multiplier in zone 2 (medium).")]
        public float zone2RateMultiplier      = 1.5f;
        [Tooltip("Rate multiplier in zone 3 (deep).")]
        public float zone3RateMultiplier      = 2.0f;

        // =====================================================================
        // DREAM TIMER
        // =====================================================================
        [Header("── Dream Timer ──────────────────────────────────────")]
        [Tooltip("Timer drain multiplier when restlessness is at the High threshold.")]
        public float highRestlessnessAcceleration = 2f;
        [Tooltip("Timer drain multiplier when restlessness is at Maximum.")]
        public float maxRestlessnessAcceleration  = 4f;
        [Tooltip("Extra seconds added on the first run (onboarding bonus).")]
        public float firstRunBonusTime            = 60f;

        // =====================================================================
        // ENEMIES (DreamEntity)
        // =====================================================================
        [Header("── Enemies ──────────────────────────────────────────")]
        [Tooltip("Visual scale of the enemy sprite.")]
        public float entityScale              = 1f;
        [Tooltip("Patrol movement speed (units/s).")]
        public float entitySpeed              = 1.8f;
        [Tooltip("Distance to a waypoint at which it is considered reached.")]
        public float entityWaypointThreshold  = 0.1f;
        [Tooltip("Restlessness points added per second while the enemy is in the vision cone.")]
        public float entitySpikePerSecond     = 8f;
        [Tooltip("Radius within which the enemy interrupts the retention minigame.")]
        public float entityInterruptRadius    = 1.2f;
        [Tooltip("Interruption magnitude applied to the minigame (0–1).")]
        public float entityInterruptMagnitude = 0.35f;
        [Tooltip("Detection radius for the 'enemy nearby' ambient sound.")]
        public float entityNearbyRadius       = 6f;
        [Tooltip("Cooldown between 'enemy nearby' sound repetitions (s).")]
        public float entityNearbyCooldown     = 3f;

        // =====================================================================
        // ALLIES (Dream encounter)
        // =====================================================================
        [Header("── Allies — Encounter ───────────────────────────────")]
        [Tooltip("Visual scale of the ally sprite in the Dream.")]
        public float allyScale          = 1f;
        [Tooltip("Activation radius for the ally encounter trigger.")]
        public float allyTriggerRadius  = 1.2f;
        [Tooltip("Idle animation frame interval for allies (s).")]
        public float allyIdleInterval   = 0.55f;

        // =====================================================================
        // MEMORY FRAGMENTS (MemoryPoint)
        // =====================================================================
        [Header("── Memory Fragments ───────────────────────────────")]
        [Tooltip("Visual scale of the MemoryPoint sprite.")]
        public float fragmentScale           = 1f;
        [Tooltip("Interaction range with the MemoryPoint.")]
        public float fragmentInteractRange   = 1.5f;
        [Tooltip("Radius at which the 'fragment nearby' sound plays.")]
        public float fragmentNearbyRadius    = 3f;
        [Tooltip("Cooldown between 'fragment nearby' sound repetitions (s).")]
        public float fragmentNearbyCooldown  = 2.5f;
        [Tooltip("Pulse animation speed of the MemoryPoint visual.")]
        public float fragmentPulseSpeed      = 2f;
        [Tooltip("Minimum scale of the pulse animation.")]
        public float fragmentPulseScaleMin   = 0.7f;
        [Tooltip("Maximum scale of the pulse animation.")]
        public float fragmentPulseScaleMax   = 1.1f;

        // =====================================================================
        // TIMING MINIGAME
        // =====================================================================
        [Header("── Minigame — Timing ──────────────────────────────")]
        [Tooltip("Marker speed at minimum restlessness (0–1 per second).")]
        public float markerSpeed           = 0.55f;
        [Tooltip("Marker speed at maximum restlessness.")]
        public float markerSpeedMax        = 1.6f;
        [Tooltip("Center position of the green zone (0–1).")]
        public float greenZoneCenter       = 0.5f;
        [Tooltip("Half-width of the green zone at minimum restlessness.")]
        public float greenZoneHalfWidth    = 0.10f;
        [Tooltip("Minimum half-width of the green zone at maximum restlessness.")]
        public float greenZoneHalfWidthMin = 0.04f;
        [Tooltip("Number of correct hits required to extract the fragment.")]
        public int   successesRequired     = 3;
        [Tooltip("Number of misses allowed before the minigame fails.")]
        public int   failuresAllowed       = 2;

        // =====================================================================
        // RETENTION MINIGAME
        // =====================================================================
        [Header("── Minigame — Retention ──────────────────────────")]
        [Tooltip("Fill speed of the retention bar.")]
        public float retentionFillRate          = 0.4f;
        [Tooltip("Decay speed of the retention bar.")]
        public float retentionDecayRate         = 0.2f;
        [Tooltip("Restlessness reduction bonus on retention success.")]
        public float retentionRestlessnessBonus = 0.3f;

        // =====================================================================
        // INVENTORY
        // =====================================================================
        [Header("── Inventory ────────────────────────────────────────")]
        [Tooltip("Number of grid columns at run start.")]
        public int   inventoryWidth    = 4;
        [Tooltip("Number of grid rows at run start.")]
        public int   inventoryHeight   = 5;
        [Tooltip("Pixel size of each inventory cell (UI).")]
        public float inventoryCellSize = 48f;
        [Tooltip("Gap between cells (pixels).")]
        public float inventoryCellGap  = 4f;

        // =====================================================================
        // HEALTH COSTS
        // =====================================================================
        [Header("── Health Costs ─────────────────────────────────────")]
        [Tooltip("Mental damage on a calm wake-up.")]
        [Range(0f, 30f)] public float normalRunMentalCost        = 3f;
        [Tooltip("Physical damage on a calm wake-up.")]
        [Range(0f, 30f)] public float normalRunPhysicalCost      = 2f;
        [Tooltip("Mental damage on an abrupt wake-up.")]
        [Range(0f, 50f)] public float abruptWakeUpMentalDamage   = 20f;
        [Tooltip("Physical damage on an abrupt wake-up.")]
        [Range(0f, 50f)] public float abruptWakeUpPhysicalDamage = 15f;
        [Tooltip("Fragments lost on an abrupt wake-up.")]
        [Range(0, 5)]    public int   abruptFragmentLoss         = 1;
        [Tooltip("Number of fragments required to complete the demo.")]
        [Range(1, 30)]   public int   demoFragmentTarget         = 12;

        // =====================================================================
        // FOOTSTEPS
        // =====================================================================
        [Header("── Footsteps ────────────────────────────────────────")]
        [Tooltip("Interval between footstep sounds while walking (s).")]
        public float footstepWalkInterval = 0.42f;
        [Tooltip("Interval between footstep sounds while running (s).")]
        public float footstepRunInterval  = 0.26f;
        [Tooltip("Footstep volume.")]
        [Range(0f, 1f)] public float footstepVolume = 0.5f;

        // =====================================================================
        // CAMERA
        // =====================================================================
        [Header("── Camera ───────────────────────────────────────────")]
        [Tooltip("Orthographic size of the camera.")]
        public float cameraOrthoSize   = 6f;
        [Tooltip("Camera follow smooth speed.")]
        public float cameraSmoothSpeed = 5f;

        // =====================================================================
        // TRANSITIONS
        // =====================================================================
        [Header("── Transitions ─────────────────────────────────────")]
        [Tooltip("Duration of the fade when falling asleep.")]
        public float sleepFadeDuration            = 1.2f;
        [Tooltip("Duration of the fade on voluntary wake-up.")]
        public float voluntaryWakeUpFadeDuration  = 0.8f;
        [Tooltip("Duration of the fade on abrupt wake-up.")]
        public float abruptWakeUpFadeDuration     = 0.4f;
        [Tooltip("Duration of the white flash on abrupt wake-up.")]
        public float abruptPulseDuration          = 0.18f;
        [Tooltip("Duration of the camera shake on abrupt wake-up.")]
        public float abruptShakeDuration          = 0.35f;
        [Tooltip("Camera shake magnitude.")]
        public float abruptShakeMagnitude         = 0.18f;
        [Tooltip("Duration of the Vigil entry fade-in.")]
        public float vigiliaFadeInDuration        = 1.6f;
        [Tooltip("Duration of the abrupt entry flash in the Vigil.")]
        public float vigiliaAbruptFlashDuration   = 0.4f;

        // =====================================================================
        // AUDIO — VOLUMES
        // =====================================================================
        [Header("── Audio — Volumes ─────────────────────────────────")]
        [Tooltip("Ambient volume in the Dream at calm state.")]
        [Range(0f, 1f)] public float ambientVolumeCalm     = 0.40f;
        [Tooltip("Ambient volume in the Dream at critical state.")]
        [Range(0f, 1f)] public float ambientVolumeCritical = 0.85f;
        [Tooltip("Dream SFX volume.")]
        [Range(0f, 1f)] public float dreamSfxVolume        = 0.7f;
        [Tooltip("Vigil ambient volume.")]
        [Range(0f, 1f)] public float vigiliaAmbientVolume  = 0.4f;
        [Tooltip("Vigil SFX volume.")]
        [Range(0f, 1f)] public float vigiliaSfxVolume      = 0.7f;
        [Tooltip("Transition time between adaptive audio snapshots (s).")]
        public float audioSnapshotTransitionTime           = 2f;

        // =====================================================================
        // VISUAL FX — RESTLESSNESS
        // =====================================================================
        [Header("── Visual FX — Restlessness ───────────────────────")]
        [Tooltip("Vignette intensity at rest (low restlessness).")]
        [Range(0f, 1f)] public float vignetteIdle              = 0.25f;
        [Tooltip("Maximum vignette intensity (critical restlessness).")]
        [Range(0f, 1f)] public float vignetteCritical          = 0.58f;
        [Tooltip("Vignette heartbeat pulse amplitude.")]
        public float vignettePulseAmplitude                    = 0.04f;
        [Tooltip("Pulse speed at High threshold.")]
        public float vignettePulseSpeedHigh                    = 1.2f;
        [Tooltip("Pulse speed at Critical threshold.")]
        public float vignettePulseSpeedCritical                = 2.8f;
        [Tooltip("Chromatic aberration at Medium threshold.")]
        [Range(0f, 1f)] public float chromaticMedium           = 0.12f;
        [Tooltip("Chromatic aberration at High threshold.")]
        [Range(0f, 1f)] public float chromaticHigh             = 0.35f;
        [Tooltip("Maximum chromatic aberration (Critical threshold).")]
        [Range(0f, 1f)] public float chromaticCritical         = 0.65f;
        [Tooltip("Lens distortion at High threshold.")]
        public float lensDistortionHigh                        = -0.12f;
        [Tooltip("Lens distortion at Critical threshold.")]
        public float lensDistortionCritical                    = -0.28f;
        [Tooltip("Duration of the threshold crossing flash (s).")]
        public float thresholdFlashDuration                    = 0.25f;
        [Tooltip("Maximum alpha of the threshold flash.")]
        [Range(0f, 1f)] public float thresholdFlashAlphaMax    = 0.38f;
        [Tooltip("Base alpha of the red veil at maximum restlessness.")]
        [Range(0f, 1f)] public float maxVeilBaseAlpha          = 0.10f;
        [Tooltip("Pulse depth of the red veil.")]
        [Range(0f, 1f)] public float maxVeilPulseDepth         = 0.10f;
        [Tooltip("Pulse rate of the red veil.")]
        public float maxVeilPulseRate                          = 2.2f;
        [Tooltip("Chromatic aberration buzz strength on enemy detection.")]
        public float buzzChromaticStrength                     = 0.6f;
        [Tooltip("Vignette buzz strength on enemy detection.")]
        public float buzzVignetteStrength                      = 0.15f;
        [Tooltip("Buzz decay speed.")]
        public float buzzDecaySpeed                            = 3.5f;
        [Tooltip("General interpolation speed for visual effects.")]
        public float fxLerpSpeed                               = 2f;
    }
}
