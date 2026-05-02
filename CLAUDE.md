# Restless — Project Context

Restless is a psychological horror game with roguelite elements. The protagonist is an elderly man on the verge of suicide who enters a recurring dream world to find his son, trapped under the influence of an entity known as the Yellow King. The game cycles between two interconnected states: **Vigilia** (waking hub) and **Sueño** (dream gameplay).

The current design target is a simplified MOC (Minimum Original Concept) — the V1 metroidvania-style exploration loop has been replaced by a simpler gameplay loop yet to be defined.

Built in Unity 6. Pixel art, monochromatic palette with selective color accents.

---

## Documentation

```
Docs/
├── Dirección narrativa.md
└── GDD/
    ├── README.md
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
    │   └── Arte.md
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

## Design Pillars

1. **Dual-world cycle** — Dream/Vigilia loop creates constant tension and resource management.
2. **Jungian psychology** — Allies are archetypes: Hero, Shadow, Caregiver, Sage, Anima, Mystic.
3. **Mental state mechanics** — Player conditions alter perception and gameplay in both worlds.
4. **Ally-based builds** — No class system; builds emerge from ally combinations and incompatibilities.
5. **Minimal narrative** — Environmental storytelling, no explicit text walls.
6. **Lovecraftian atmosphere** — Cosmic horror, impossible geometries, incomprehensible entities.
