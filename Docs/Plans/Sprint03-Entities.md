# Sprint 03 — Presencias espectrales y sistema de niebla

**Estado:** ✅ Completado  
**Fase:** 1 — Infraestructura del Sueño  
**Prerequisito:** [Sprint 02 — UX Polish](Sprint02-UX-Polish.md) ✅

---

## Objetivo

Transformar las entidades del sueño de obstáculos visibles y predecibles en elementos de incertidumbre activa. El jugador no sabe qué es lo que tiene delante hasta que lo mira el tiempo suficiente. Mirar tiene coste potencial; no mirar también.

---

## Lo que se construyó

### `DreamFog` — capa de niebla con revelación por dwell time

Cualquier entidad del sueño puede arrancarse envuelta en una niebla translúcida azulada. Al entrar en el cono de visión del protagonista durante el tiempo mínimo (`fogRevealDwellTime`), la niebla se desvanece en crossfade y el contenido subyacente se activa.

El timer drena lentamente cuando el jugador aparta la vista (no se resetea bruscamente), manteniendo la tensión sin penalizar el movimiento natural.

Tipos de contenido soportados (auto-detectados en el mismo GO):

| Tipo | Al revelarse |
|---|---|
| `Threat` | `DreamEntity` haunted se activa con su patrulla normal |
| `Wanderer` | `WanderingNPC` empieza a deambular |
| `Fragment` | `MemoryPoint` se vuelve interactuable (el minijuego sigue siendo necesario) |
| `AllyEcho` | Beneficio temporal al protagonista |

### `FocusReveal` — silhouette hasta que el cono ilumina

Componente complementario para elementos de escenario (no presencias). Renderiza el objeto como una silueta oscura sin iluminar. Al entrar en el cono de visión del jugador, hace un swap de material al original y lerp de color a blanco. La revelación es permanente.

### `DreamFogSpawner` — distribución por zonas

Spawner basado en zonas (`FogSpawnZone[]`) que instancia las presencias al inicio de la run:

| Categoría | Implementación |
|---|---|
| Threat (con niebla) | `DreamEntity` haunted + `DreamFog` |
| Wanderer (con niebla) | `DreamEntity` inerte + `DreamFog` |
| Wanderer visible | `DreamEntity` inerte sin niebla — ruido ambiental puro |
| Fragment (con niebla) | `MemoryPoint` + `DreamFog` |

Distribución configurable vía `GameConfig`: `fogThreatFraction`, `fogWandererFraction`, `fogWandererVisibleFraction`, `fogFragmentCount`. El número de fragmentos se aleatoriza entre `_minFragments` y `_maxFragments`.

**Nota:** en Sprint 04 se añadió `DreamPresenceSpawner` como sistema paralelo que distribuye presencias según la metadata de las rooms procedurales. `DreamFogSpawner` opera sobre la escena Dream con zonas fijas; `DreamPresenceSpawner` opera sobre el grafo de rooms generado.

---

## Criterios de salida

- [x] Presencias envueltas en niebla hasta que el jugador las mira el tiempo suficiente
- [x] Revelación con crossfade visual; el contenido se activa al completarse
- [x] El timer de dwell drena lentamente — no se resetea de golpe al apartar la vista
- [x] Cuatro tipos de presencia: Threat, Wanderer, Fragment, AllyEcho
- [x] Wanderers visibles sin niebla para refuerzo ambiental
- [x] `FocusReveal` para elementos de escenario (silhouette → material normal)
- [x] Distribución configurable desde `GameConfig`, sin hardcoding
- [x] Sin regresiones en el loop principal (minijuego, inquietud, timer, despertar)
