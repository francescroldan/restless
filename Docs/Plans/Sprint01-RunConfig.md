# Sprint 01 — RunConfig: parámetros modificables por run

**Estado:** ✅ Completado 2026-05-08  
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

### 1. GameConfig ScriptableObject ✅

`Assets/_Project/Data/GameConfig.asset` creado con todos los parámetros del juego en 17 secciones:

- [x] Protagonista: escala, velocidades, rango de interacción
- [x] Cono de visión: alcance, ángulo, intensidad, halo
- [x] Iluminación Sueño y Vigilia
- [x] Inquietud: tasa base, multiplicador minijuego, multiplicadores zona 2 y 3
- [x] Timer: aceleraciones, bonus primera run
- [x] Enemigos: escala, velocidad, spike, radios de detección
- [x] Aliados: escala, radio de encuentro, animación
- [x] Fragmentos: escala, rango, pulso visual, sonido proximidad
- [x] Minijuego Timing: todos los parámetros de dificultad
- [x] Minijuego Retención: fill, decay, bonus
- [x] Inventario: tamaño grid y UI
- [x] Costes de salud: daños por tipo de despertar, target demo
- [x] Pasos: intervalos y volumen
- [x] Cámara: tamaño ortográfico, suavidad
- [x] Transiciones: duraciones de fade, shake, flash
- [x] Audio: volúmenes ambient, SFX, snapshots
- [x] FX visuales: vignette, chromatic, distorsión, velo rojo, buzz

### 2. RunConfig — clase de runtime ✅

- [x] Clase plain C# con los mismos campos que `GameConfig`
- [x] `RunConfig.Current` — creado en `DreamSceneBootstrap.Awake`, nulleado en `OnDestroy`
- [x] Constructor `RunConfig(GameConfig)` que copia todos los valores
- [x] Campos públicos mutables

### 3. DreamSceneBootstrap — inicialización ✅

- [x] Referencia directa a `GameConfig` en `_Managers`
- [x] `RunConfig.Create(gameConfig)` en `Awake` antes de `Start`
- [x] `dreamDuration` se inicializa desde `ProtagonistState.BaseDreamDuration`

### 4. DreamPassiveApplier — escribir en RunConfig ✅

- [x] `ApplyPassives()` escribe en `RunConfig.Current` directamente
- [x] `restlessnessPassiveMultiplier`, `dreamDuration`, `minigameSpeedMultiplier`, `healthCostMultiplier`, `inventoryBonusCells`

### 5. Refactorizar sistemas para leer RunConfig

- [x] `ProtagonistController` — velocidades
- [x] `VisionCone` — ángulo, rango
- [x] `DreamTimer` — aceleraciones
- [x] `RestlessnessManager` — tasa base, multiplicador minijuego, pasiva de aliado
- [x] `DreamEntity` — velocidad, threshold
- [x] `TimingMinigame` — todos sus parámetros
- [x] `AmbientAudioPlayer` — volumen base
- [x] `RestlessnessVisualFX` — vignette, chromatic, velo

### 6. Validación

- [x] Aliado Sabio: `restlessnessRateModifier` y `minigameSpeedMultiplier` de AllyData se acumulan en `DreamPassiveApplier` y se escriben en RunConfig — cadena íntegra
- [x] Aliado Héroe: `dreamDurationBonus` se suma a `RunConfig.dreamDuration` antes de llamar a `DreamTimer.StartTimer()` — orden correcto en DreamSceneBootstrap
- [x] Sin aliados: `selectedAllyIds` vacío → passives no aplican → RunConfig queda con los valores de GameConfig — comportamiento idéntico a M6
- [x] Dos runs seguidas: `RunConfig.Clear()` en `DreamSceneBootstrap.OnDestroy` y `RunConfig.Create()` en el siguiente `Awake` garantizan instancia limpia

---

## Criterios de salida

- [x] `GameConfig.asset` existe y contiene todos los parámetros listados arriba
- [x] `RunConfig.Current` es accesible desde cualquier sistema durante el sueño y null fuera de él
- [x] Añadir un aliado nuevo que modifique cualquier parámetro de la lista requiere solo cambiar `DreamPassiveApplier`, sin tocar el sistema destino
- [x] El comportamiento del juego sin aliados es idéntico al de M6
- [x] Los aliados Sabio y Héroe funcionan igual que antes del refactor

---

## Notas de diseño

**¿Por qué no serializar RunConfig en el save?**  
Las runs de Restless son cortas (~5 min) y no tienen pausa-y-continuar. `SaveData` solo persiste progresión entre runs (salud, aliados desbloqueados, contador de runs). Los parámetros de run son efímeros por diseño — si el jugador cierra el juego en mitad de un sueño, la run se pierde, que es coherente con la mecánica de consecuencias del despertar abrupto.

**¿Los [SerializeField] desaparecen?**  
No. Se mantienen como valores de referencia en el Inspector y como fallback cuando no hay `RunConfig.Current` (útil en Edit Mode, tests unitarios, etc.). GameConfig los centraliza para que el diseñador tenga un solo sitio donde mirar, pero los sistemas son robustos sin él.
