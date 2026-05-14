# Sprint 04 — Generación procedural de escenarios

**Estado:** 🔄 En curso  
**Prerequisito:** [Sprint 03 — Entidades del sueño](Sprint03-Entities.md) ✅ cerrado

**Commits del sprint:**
- `647d37a` feat(s3): sistema de presencias espectrales — cierre Sprint 03 *(base)*
- `9c7c5e2` feat(s4): validación de navegabilidad + retry loop en RoomAssembler
- `7ef3efa` feat(s4): 4 nuevas variantes de sala (hospital biome) en CreateRoomVariants
- `1f0e8b5` feat(s4/p5): presence spawner respeta metadata de sala y nivel de peligro

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
- [x] Validación de navegabilidad: `IsNavigable` BFS entrada→salida; hasta 3 reintentos con seed distinto si el layout queda desconectado

**Notas de implementación:**
- `TryPlaceAll` extrae el BFS de placement (void, puede dejar nodos sin colocar)
- `IsNavigable` recorre sólo nodos con `PlacedRoom != null`; si el exit no es alcanzable, el intento se descarta y se destruyen los GOs
- Semilla del reintento: `(seed + attempt) ^ 0xDEAD`
- Las rooms de tipo Ritual reciben pintura de sangre (paredes y suelos) sólo en el intento exitoso (`PaintRitualRooms` post-validación)
- Conexiones con salas Ritual usan puerta (`PlaceSingleDoorTile`) en vez de pasillo; `IsDoorConnection` comprueba ambos extremos del edge

---

### P4 — Rooms handcrafted (hospital onírico, biome 1)

**Objetivo:** producir el contenido mínimo necesario para que el sistema tenga algo que ensamblar.

6–8 rooms diseñadas manualmente con tileset existente:

- [x] `entrance_hall` — Medium, safe, sockets N+S
- [x] `hospital_corridor_a` — Small, corridor, sockets N+S
- [x] `hospital_corridor_b` — Small, Safe, sockets E+W *(definición creada; ejecutar Create Room Variants + Rebuild Rooms en Unity)*
- [x] `ward_room` — Medium, encounter+safe, sockets N+S+E
- [x] `operating_theatre` — Medium, Ritual+Memory, supportsFragments, sockets N+S+E+W *(ídem)*
- [x] `flooded_basement` — Medium, Encounter, sockets N+S+E+W *(ídem)*
- [x] `nurses_station` — Small, DeadEnd+Safe, socket N solo *(ídem)*
- [x] `memory_ward` — Medium, memory, supportsFragments, sockets N+S
- [x] `ritual_room` — Medium, ritual+encounter, sockets N+S
- [x] `collapsed_wing` — Small, dead_end, socket S (dead end)
- [x] `landmark_exit` — Large, landmark+collapse, socket S

Cada room tiene sockets definidos, zona de spawn de presencias y metadata completa.

**Pendiente de ejecutar en Unity Editor:**
1. `Tools > Restless > Create Room Variants` — genera prefabs + RoomDefinition para las 4 nuevas
2. `Tools > Restless > Rebuild Rooms (DarkDungeon)` — pinta tilemaps y registra en Dream.unity

---

### P5 — Integración con DreamPresenceSpawner

**Objetivo:** que el spawner de presencias lea las rooms generadas en vez del mapa estático.

- [x] `DreamPresenceSpawner` lee las rooms instanciadas y sus zonas de spawn (via `SetRooms`)
- [x] Respeta `supportsFragments` de la metadata (flag `requiresFragment` en `FindFreePosition`)
- [x] Respeta `supportsThreats` por room — Threats y Undefined sólo se colocan en rooms con el flag activo
- [x] Escala la selección de sala según `dangerLevel` — rooms de mayor peligro reciben peso proporcional para spawns de amenaza (weighted pick)
- [x] El primer fragmento garantizado se coloca en una room tipo `memory`; fallback a cualquier room con `supportsFragments` si no hay posición libre

**Notas de implementación:**
- `SetRooms` acepta ahora `RunGraph graph = null` para resolver tipos de nodo en runtime
- `FindFreePosition(requiresThreat, requiresFragment, requiresMemoryRoom)` — los tres filtros son independientes y combinables
- `IsMemoryRoom(room)` busca en `graph.Nodes` el nodo cuyo `PlacedRoom == room` y comprueba `node.Type == Memory`
- `WeightedPick` hace selección aleatoria ponderada sobre la lista de candidatos
- `supportsAllies` está modelado en `RoomDefinition` pero aún no tiene uso activo (sin ally spawning)

---

### P6 — Topología inconsistente (arquitectura imposible básica)

**Objetivo:** que el mundo generado no se comporte siempre como un espacio lógico.

- [ ] `RoomMutator` — al revisitar una room, puede alterar: iluminación, props activos, variante de tileset
- [ ] Al menos una conexión "mentirosa" por run: una puerta que en la segunda visita lleva a una room distinta
- [ ] Configurable: probabilidad de mutación por room, intensidad

---

## Criterios de salida del sprint

- [x] Una run completa se genera proceduralmente al iniciar el sueño (no hay mapa estático)
- [x] Cada run produce un layout diferente con la misma seed siempre igual
- [x] El jugador puede navegar de entrada a salida sin quedarse bloqueado
- [x] Las presencias se distribuyen respetando la metadata de las rooms
- [ ] Al menos una room cambia perceptiblemente en una segunda visita *(P6 pendiente)*
- [ ] El conjunto de 6–8 rooms no se siente como una mazmorra genérica — tiene atmósfera de hospital onírico *(requiere prueba en Play mode)*
- [ ] No hay regresiones en el loop principal (presencias, minijuego, inquietud, timer, despertar) *(requiere prueba en Play mode)*

---

## Orden sugerido

1. **P1** — sockets y contrato técnico (todo depende de esto)
2. **P4** — rooms handcrafted en paralelo (trabajo de arte/diseño independiente)
3. **P2** — generador de grafo
4. **P3** — ensamblador
5. **P5** — integración con presencias
6. **P6** — topología inconsistente (puede quedar parcial si el tiempo lo requiere)
