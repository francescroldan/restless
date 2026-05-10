# Restless — Project Context

Restless is a psychological horror game with roguelite elements. The protagonist is an elderly man on the verge of suicide who enters a recurring dream world to find his son, trapped under the influence of an entity known as the Yellow King. The game cycles between two interconnected states: **Vigilia** (waking hub) and **Sueño** (dream gameplay).

The current design target is a simplified MOC (Minimum Original Concept). The core loop: the player prepares in Vigil (selects allies, items), enters the dream, navigates a top-down space with a limited vision cone, extracts memory fragments of their son via minigames while a Restlessness meter climbs, and returns before the meter maxes out. Abrupt awakenings degrade the protagonist's state; allies modify the run's conditions and consequences.

Built in Unity 6. Pixel art, monochromatic palette with selective color accents.

---

## Documentation

```
Docs/
├── Dirección narrativa.md
├── Plans/                           ← milestones de desarrollo
│   ├── TIMELINE.md
│   ├── M0-Definir-Loop-MOC.md       ✅ completado
│   ├── M1-Setup-Tecnico.md          ✅ completado
│   ├── M2-Prototipo-Loop.md         ✅ completado
│   ├── M3-Identidad-Visual.md       ✅ completado
│   ├── M4-Hub-Vigilia.md            ✅ completado
│   ├── M5-Primer-Aliado.md          🔄 en curso
│   └── M6-MOC-Completo.md
└── GDD/
    ├── README.md
    ├── MOC-Loop.md                  ← loop definitivo del MOC (v1.0)
    ├── 01_NARRATIVE/
    │   ├── Narrativa.md
    │   ├── Página principal.md
    │   ├── Songlines (líneas de canto).md
    │   └── Songlines como recurso narrativo.md
    ├── 02_DREAM_MECHANICS/
    │   ├── Condiciones mentales en el sueño.md
    │   ├── Mecánicas del sueño.md
    │   ├── Medidor de inquietud (restlessness).md
    │   ├── Sistema de estrés y despertar.md
    │   ├── Sueño lúcido vs sueño profundo.md
    │   ├── Tiempo limitado en el sueño.md
    │   ├── Tipos de despertar (tranquilo vs abrupto).md
    │   └── Transformación del entorno por estado mental.md
    ├── 03_VIGILIA/
    │   ├── Condiciones mentales en la vigilia.md
    │   ├── Estado del protagonista.md
    │   ├── Estado físico en la vigilia.md
    │   ├── Estados del personaje en la vigilia.md
    │   ├── Interfaz y entorno.md
    │   ├── Mecánicas de juego.md
    │   ├── Mecánicas de la vigilia.md
    │   ├── Pantalla central de la habitación.md
    │   └── Selección previa al sueño (builds y preparación).md
    ├── 04_ALLIES_AND_BUILDS/
    │   ├── Aliados dentro del sueño.md
    │   ├── Conexión entre sueño y vigilia.md
    │   ├── Encuentro con aliados (npcs).md
    │   ├── Incompatibilidad entre aliados.md
    │   ├── Interacción entre mundos.md
    │   ├── Mejoras a través de aliados.md
    │   ├── Mejoras builds y economía.md
    │   └── Sistema de builds mediante aliados.md
    ├── 05_ART_AND_AUDIO/
    │   ├── Arte.md
    │   └── Paleta.md
    ├── 06_WORLD_BUILDING/
    │   ├── Estudio de mercado.md
    │   └── Inspiración de parajes lovecraftianos para restles.md
    └── _ARCHIVE_V1/          ← metroidvania-specific, not active
        ├── Exploración 2d con plataformas.md
        ├── Exploración y navegación.md
        ├── Habilidades y progresión.md
        ├── Niveles.md
        ├── Planning de desarrollo.md
        ├── Puzles progresión y habilidades.md
        ├── Puzles y backtracking.md
        └── Retos y puzles.md
```

---

## Project Structure

```
Assets/
├── Editor/                          ← editor scripts (VigiliaSceneSetup, etc.)
├── Plugins/Demigiant/DOTween/       ← DOTween animation library
├── TextMesh Pro/                    ← TMP resources
└── _Project/                        ← todo el contenido del juego
    ├── Art/
    │   ├── Animations/Protagonist/  ← ProtagonistAnimator.controller + anims
    │   ├── Materials/
    │   ├── Shaders/                 ← MonochromeAccent.shader
    │   ├── Sprites/
    │   │   └── Placeholder/
    │   │       ├── Anim/            ← frames protagonista (idle/walk)
    │   │       ├── Dream/           ← sprites escena sueño
    │   │       └── Vigilia/         ← sprites aliados + room
    │   └── UI/
    ├── Audio/
    │   ├── Music/
    │   └── SFX/
    ├── Data/
    │   ├── Allies/                  ← AllyData_*.asset + AllyRegistry.asset
    │   └── ScriptableObjects/Events/
    ├── Input/
    │   └── PlayerInputActions.inputactions
    ├── Prefabs/
    │   ├── Characters/
    │   ├── Environment/
    │   └── UI/
    ├── Scenes/
    │   ├── Bootstrap.unity
    │   ├── Dream.unity
    │   └── Vigil.unity
    ├── ScriptableObjects/
    │   ├── Events/                  ← GameEvent assets (restlessness, timer…)
    │   └── MemoryFragments/
    └── Scripts/
        ├── Core/                    ← GameManager, SceneLoader, SaveManager,
        │                               ProtagonistState, CameraFollow, events
        ├── Dream/                   ← ProtagonistController, RestlessnessManager,
        │                               DreamTimer, WakeUpManager, minigames,
        │                               AllyEncounter, MemoryPoint, VisionCone…
        ├── Vigil/                   ← VigiliaRoomController, AllyData, AllySlot,
        │                               PreDreamSelectionPanel, RoomAllyPresence…
        └── Shared/

Docs/
├── GDD/                             ← diseño activo (01–06 + README)
├── Plans/                           ← hitos M0–M6 + TIMELINE
└── Notion/ + _ARCHIVE_V1/          ← histórico, no activo
```

---

## Development Rules

- **All tunable values must be defined in `GameConfig.cs`** — no hardcoded defaults left on MonoBehaviour components. Every float/int/bool that affects gameplay, timing, audio, or visuals goes in GameConfig, is copied into `RunConfig` at dream start, and read via `RunConfig.Current?.field ?? _serializeFieldFallback` at runtime.

---

## Design Pillars

1. **Dual-world cycle** — Dream/Vigilia loop creates constant tension and resource management.
2. **Jungian psychology** — Allies are archetypes: Hero, Shadow, Caregiver, Sage, Anima, Mystic.
3. **Mental state mechanics** — Player conditions alter perception and gameplay in both worlds.
4. **Ally-based builds** — No class system; builds emerge from ally combinations and incompatibilities.
5. **Minimal narrative** — Environmental storytelling, no explicit text walls.
6. **Lovecraftian atmosphere** — Cosmic horror, impossible geometries, incomprehensible entities.
