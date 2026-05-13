# Sprint 04 — Generación procedural de escenarios

**Estado:** 🔄 En curso  
**Prerequisito:** [Sprint 03 — Entidades del sueño](Sprint03-Entities.md) ✅ cerrado

---

## Objetivo

Reemplazar el mapa estático actual por un sistema de ensamblaje modular que construya runs distintas en cada partida. El objetivo del sprint **no** es construir un sistema procedural completo ni definitivo — es validar que un mundo generado produce tensión, incertidumbre y sensación onírica.

Referencia de diseño: [GDD — Generación procedural de escenarios](../GDD/02_DREAM_MECHANICS/Generacion%20procedural%20de%20escenarios.md)

---

## Decisiones cerradas

| Parámetro | Valor |
|---|---|
| Rooms por run | 8–12 |
| Rooms handcrafted a construir | 6–8 |
| Sistema | Grafo modular con sockets |
| Arquitectura imposible | Topología inconsistente (no geometría física real) |
| Primer biome | Hospital onírico |

---

## Tareas

### P1 — Infraestructura de rooms y sockets

**Objetivo:** definir el contrato técnico que conecta rooms entre sí.

- [x] `RoomDefinition` ScriptableObject con metadata: id, size, type[], tags[], dangerLevel, surrealism, supportsThreats, supportsFragments, supportsAllies
- [x] `DoorSocket` component en cada room: posición, dirección cardinal, ancho compatible
- [x] Prefab base de room con estructura estándar: suelo, paredes, sockets, zona de spawn de presencias
- [x] Validación en editor: gizmos que muestran sockets y área de spawn

---

### P2 — Generador de grafo

**Objetivo:** construir la estructura de la run como un grafo antes de colocar ninguna room.

- [x] `RunGraphGenerator` — genera nodos tipados (safe, corridor, encounter, memory, dead_end, landmark) con las conexiones entre ellos
- [x] Parámetros configurables: longitud mínima/máxima, probabilidad de fork, probabilidad de dead end, posición garantizada del landmark
- [x] Garantías de diseño: siempre hay entrada, siempre hay salida, siempre hay al menos 1 room tipo `memory` (para fragmentos)
- [x] Reproducible por seed

---

### P3 — Ensamblador de rooms

**Objetivo:** convertir el grafo abstracto en rooms concretas colocadas en el mundo.

- [x] `RoomAssembler` — recorre el grafo, selecciona prefab compatible para cada nodo (por tipo, tamaño y tags), alinea sockets entre rooms adyacentes
- [x] Detección de overlap entre rooms colocadas — reintentar con otro prefab si colisionan
- [ ] Generación de conectores (pasillos cortos) cuando los sockets no quedan exactamente alineados
- [ ] Validación de navegabilidad: camino continuo de entrada a salida

---

### P4 — Rooms handcrafted (hospital onírico, biome 1)

**Objetivo:** producir el contenido mínimo necesario para que el sistema tenga algo que ensamblar.

6–8 rooms diseñadas manualmente con tileset existente:

- [x] `entrance_hall` — Medium, safe, sockets N+S
- [x] `hospital_corridor_a` — Small, corridor, sockets N+S
- [ ] `hospital_corridor_b` — Small, corridor, variante húmeda/Este-Oeste
- [x] `ward_room` — Medium, encounter+safe, sockets N+S+E
- [ ] `operating_theatre` — Medium, ritual/memory, alta tensión
- [ ] `flooded_basement` — Medium, traversal, unsafe
- [ ] `nurses_station` — Small, dead_end, safe relativo
- [x] `memory_ward` — Medium, memory, supportsFragments, sockets N+S
- [x] `ritual_room` — Medium, ritual+encounter, sockets N+S
- [x] `collapsed_wing` — Small, dead_end, socket S (dead end)
- [x] `landmark_exit` — Large, landmark+collapse, socket S

Cada room debe tener sockets definidos, zona de spawn de presencias y metadata completa.

---

### P5 — Integración con DreamPresenceSpawner

**Objetivo:** que el spawner de presencias lea las rooms generadas en vez del mapa estático.

- [x] `DreamPresenceSpawner` lee las rooms instanciadas y sus zonas de spawn (via `SetRooms`)
- [x] Respeta `supportsFragments` de la metadata (flag `requiresFragment` en `FindFreePosition`)
- [ ] Respeta `supportsThreats`, `supportsAllies` por room (actualmente global)
- [ ] Escala la cantidad de presencias según `dangerLevel` de cada room
- [ ] El fragmento garantizado se coloca en una room tipo `memory`

---

### P6 — Topología inconsistente (arquitectura imposible básica)

**Objetivo:** que el mundo generado no se comporte siempre como un espacio lógico.

- [ ] `RoomMutator` — al revisitar una room, puede alterar: iluminación, props activos, variante de tileset
- [ ] Al menos una conexión "mentirosa" por run: una puerta que en la segunda visita lleva a una room distinta
- [ ] Configurable: probabilidad de mutación por room, intensidad

---

## Criterios de salida del sprint

- [ ] Una run completa se genera proceduralmente al iniciar el sueño (no hay mapa estático)
- [ ] Cada run produce un layout diferente con la misma seed siempre igual
- [ ] El jugador puede navegar de entrada a salida sin quedarse bloqueado
- [ ] Las presencias se distribuyen respetando la metadata de las rooms
- [ ] Al menos una room cambia perceptiblemente en una segunda visita
- [ ] El conjunto de 6–8 rooms no se siente como una mazmorra genérica — tiene atmósfera de hospital onírico
- [ ] No hay regresiones en el loop principal (presencias, minijuego, inquietud, timer, despertar)

---

## Orden sugerido

1. **P1** — sockets y contrato técnico (todo depende de esto)
2. **P4** — rooms handcrafted en paralelo (trabajo de arte/diseño independiente)
3. **P2** — generador de grafo
4. **P3** — ensamblador
5. **P5** — integración con presencias
6. **P6** — topología inconsistente (puede quedar parcial si el tiempo lo requiere)
