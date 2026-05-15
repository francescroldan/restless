# Sprint 03 — Presencias espectrales

**Estado:** ✅ Completado  
**Fase:** 1 — Infraestructura del Sueño  
**Prerequisito:** [Sprint 02 — UX Polish](Sprint02-UX-Polish.md) ✅

---

## Objetivo

Poblar el sueño con entidades que respondan al contexto: qué tipo de sala es, qué nivel de peligro tiene, qué aliados lleva el jugador. Las entidades dejan de ser decorado estático y pasan a ser parte del sistema.

---

## Lo que se construyó

### `DreamPresenceSpawner`

Gestiona la distribución de presencias por run. Recibe la lista de rooms instanciadas y el grafo (`SetRooms(rooms, graph)`) y coloca entidades según la metadata de cada sala.

Tipos de presencia:

| Tipo | Comportamiento | Restricción de sala |
|---|---|---|
| **Threat** | Patrulla activa; sube inquietud en el cono | Solo rooms con `supportsThreats=true` |
| **Wanderer** | Deambula sin propósito; no afecta al jugador | Sin restricción |
| **Fragment** | Punto de memoria interactuable | Rooms con `supportsFragments=true` |
| **Ally echo** | Beneficio temporal al acercarse | Solo si hay aliados activos en la run |

Distribución ponderada por `dangerLevel`: las rooms más peligrosas reciben proporcionalmente más amenazas.

El primer fragmento de memoria está **garantizado** en una room de tipo `Memory`. Fallback a cualquier room con `supportsFragments` si no hay posición libre.

### `WanderingNPC`

Presencia inofensiva con movimiento por waypoints aleatorios dentro de los spawn bounds de la sala. No interactúa con el jugador, no afecta a la inquietud. Refuerza la sensación de espacio habitado.

### Integración con GameConfig

Todas las frecuencias, conteos y parámetros de distribución viven en `GameConfig` y se copian a `RunConfig` al iniciar la run. Sin hardcoding.

---

## Criterios de salida

- [x] Las presencias se distribuyen respetando la metadata de cada room
- [x] Siempre hay al menos 1 fragmento de memoria colocable por run
- [x] Los NPCs errantes se mueven sin afectar a la inquietud ni al sistema de detección
- [x] Sin hardcoding: todo configurable desde `GameConfig`
- [x] Sin regresiones en el loop principal (minijuego, inquietud, timer, despertar)

---

## Plan original — no implementado

El plan inicial contemplaba un sistema de **niebla y revelación** (`DreamFog`): presencias que aparecen como formas translúcidas indefinidas y solo revelan su naturaleza al entrar en el cono de visión del jugador el tiempo suficiente.

Se aplazó para priorizar la generación procedural (S4), que requería que el spawner funcionara sobre rooms dinámicas antes que añadir una capa visual encima. El sistema de niebla es candidato directo para la **Fase 2 — Atmósfera**.

El diseño original sigue siendo válido: [ver concepto en el GDD](../GDD/02_DREAM_MECHANICS/Condiciones%20mentales%20en%20el%20sue%C3%B1o.md).
