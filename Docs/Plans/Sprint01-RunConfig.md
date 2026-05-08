# Sprint 01 — RunConfig: parámetros modificables por run

**Estado:** 🔄 En curso  
**Fase:** Post-MOC  
**Prerequisito:** [M6 — MOC completo y jugable](MOC/M6-MOC-Completo.md) ✅ cerrado 2026-05-08

---

## Backlog UX heredado del playtest de M6

Feedback recogido el 2026-05-08 (3 jugadores externos, 8–13 años):

| # | Problema | Causa raíz | Solución propuesta |
|---|----------|------------|--------------------|
| U1 | Audio del enemigo no comunica daño | SFX demasiado sutil; sin feedback visual | Flash rojo de pantalla al recibir daño + SFX más impactante |
| U2 | Inquietud/timer no se entienden en caliente | La barra sube pero no "grita" | Pulso/color en barra y timer cuando inquietud > 60% |
| U3 | La urna y su condición de victoria no son obvias | Contador no visible en HUD | Añadir "Recuerdos: X/Y" siempre visible en HUD |

Estas tareas se pueden abordar antes, durante o después del refactor RunConfig, ya que son independientes.

---

---

## Objetivo

Introducir una capa de configuración que separe los **valores base del juego** (inmutables, definidos por diseño) de los **valores de una run concreta** (que pueden ser modificados por aliados, objetos o eventos).

Al terminar M7, cualquier parámetro del juego que un aliado o mecánica necesite modificar tiene un sitio claro donde vivir. Añadir un aliado que "aumenta el cono de visión un 20%" es un cambio de dos líneas, no una búsqueda en cinco scripts.

---

## Motivación

Actualmente los parámetros configurables están dispersos como `[SerializeField]` en cada componente. Esto funciona bien para tuning en el Inspector, pero cuando un aliado necesita modificar la velocidad del jugador o el tamaño del cono, hay que localizar el componente, añadir un método público y llamarlo manualmente desde `DreamPassiveApplier`. Con más aliados, esto escala mal.

La solución tiene dos piezas:

- **`GameConfig`** — ScriptableObject con todos los valores base. Solo lectura en runtime. Nunca se modifica durante el juego.
- **`RunConfig`** — objeto en memoria que se crea al inicio de cada sueño, inicializado desde `GameConfig`, y que los aliados pueden modificar libremente. Muere cuando la run termina. **No se serializa al disco** (las runs de Restless no se guardan a mitad).

```
GameConfig.asset   ← valores de diseño, tweakeables en Editor
       │
       └─ copia al inicio de run ──► RunConfig (en memoria)
                                          │
                              ┌───────────┼───────────┐
                              ▼           ▼           ▼
                        DreamPassiveApplier    futuros items/eventos
                        (escribe)              (escriben)
                              │
                     todos los sistemas leen de RunConfig
                     (ProtagonistController, DreamTimer, etc.)
```

---

## Tareas

### 1. GameConfig ScriptableObject

Crear `Assets/_Project/Data/GameConfig.asset` con todos los parámetros base agrupados por sistema.

- [ ] **Jugador**: `playerSpeed`, `playerSprintSpeed`, `playerSprintStamina`, `interactRange`
- [ ] **Cono de visión**: `visionConeAngle`, `visionConeRange`, `haloIntensity`, `haloRadius`
- [ ] **Sueño / Timer**: `dreamDuration`, `firstRunBonusTime`, `highRestlessnessAcceleration`, `maxRestlessnessAcceleration`
- [ ] **Inquietud**: `baseRestlessnessRate`, `minigameActiveMultiplier`; umbrales de zona ya están en `RestlessnessZone` — valorar si mover aquí
- [ ] **Entidades**: `entitySpeed`, `entityWaypointThreshold`
- [ ] **Minijuego (Timing)**: `markerSpeed`, `markerSpeedMax`, `greenZoneHalfWidth`, `greenZoneHalfWidthMin`, `successesRequired`, `failuresAllowed`
- [ ] **Audio**: `ambientVolume`, `sfxVolume`, `footstepVolume`
- [ ] **FX visuales**: `vignetteIdle`, `vignetteCritical`, `chromaticCritical`, `maxVeilBaseAlpha`, `maxVeilPulseDepth`

### 2. RunConfig — clase de runtime

Crear `Assets/_Project/Scripts/Core/RunConfig.cs`.

- [ ] Clase plain C# (no MonoBehaviour) con los mismos campos que `GameConfig`
- [ ] Singleton de run: `RunConfig.Current` — creado en `DreamSceneBootstrap`, nulleado al volver a Vigilia
- [ ] Constructor `RunConfig(GameConfig base)` que copia todos los valores
- [ ] Campos públicos mutables (no propiedades con lógica, simplicidad primero)

### 3. DreamSceneBootstrap — inicialización

- [ ] Cargar `GameConfig` desde Resources o referencia directa en `_Managers`
- [ ] Crear `RunConfig.Current = new RunConfig(gameConfig)` antes de cualquier otra inicialización
- [ ] Mover la lógica de `firstRunBonusTime` aquí (ya estaba, solo reubicarla en RunConfig)

### 4. DreamPassiveApplier — escribir en RunConfig

- [ ] Refactorizar para que los modificadores de aliados escriban en `RunConfig.Current` en lugar de llamar métodos en sistemas individuales
- [ ] Ejemplo: en vez de `DreamTimer.Instance.StartTimer(base + bonus)`, hacer `RunConfig.Current.dreamDuration += bonus` antes de que DreamTimer lo lea

### 5. Refactorizar sistemas para leer RunConfig

Cada sistema deja sus `[SerializeField]` como **valores de fallback en Editor** (útil para tests rápidos), pero en runtime usa `RunConfig.Current` si existe.

- [ ] `ProtagonistController` — velocidades
- [ ] `VisionCone` — ángulo, rango; `Halo` — intensidad, radio  
- [ ] `DreamTimer` — duración, aceleraciones
- [ ] `RestlessnessManager` — tasa base, multiplicador de minijuego
- [ ] `DreamEntity` — velocidad
- [ ] `TimingMinigame` — todos sus parámetros de dificultad
- [ ] `AmbientAudioPlayer` — volumen base
- [ ] `RestlessnessVisualFX` — caps de vignette y chromatic, parámetros del velo

### 6. Validación

- [ ] Aliado Sabio: verificar que sus pasivas siguen aplicando correctamente vía RunConfig
- [ ] Aliado Héroe: verificar `dreamDurationBonus` y `restlessnessRateModifier`
- [ ] Sin aliados: RunConfig con valores base produce el mismo comportamiento que antes del refactor
- [ ] Dos runs seguidas no se contaminan entre sí (RunConfig se recrea limpio cada vez)

---

## Criterios de salida

- [ ] `GameConfig.asset` existe y contiene todos los parámetros listados arriba
- [ ] `RunConfig.Current` es accesible desde cualquier sistema durante el sueño y null fuera de él
- [ ] Añadir un aliado nuevo que modifique cualquier parámetro de la lista requiere solo cambiar `DreamPassiveApplier`, sin tocar el sistema destino
- [ ] El comportamiento del juego sin aliados es idéntico al de M6
- [ ] Los aliados Sabio y Héroe funcionan igual que antes del refactor

---

## Notas de diseño

**¿Por qué no serializar RunConfig en el save?**  
Las runs de Restless son cortas (~5 min) y no tienen pausa-y-continuar. `SaveData` solo persiste progresión entre runs (salud, aliados desbloqueados, contador de runs). Los parámetros de run son efímeros por diseño — si el jugador cierra el juego en mitad de un sueño, la run se pierde, que es coherente con la mecánica de consecuencias del despertar abrupto.

**¿Los [SerializeField] desaparecen?**  
No. Se mantienen como valores de referencia en el Inspector y como fallback cuando no hay `RunConfig.Current` (útil en Edit Mode, tests unitarios, etc.). GameConfig los centraliza para que el diseñador tenga un solo sitio donde mirar, pero los sistemas son robustos sin él.
