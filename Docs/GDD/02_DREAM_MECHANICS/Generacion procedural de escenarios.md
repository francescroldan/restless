# Sistema de Generación Procedural de Escenarios — Restless

---

## Decisiones de producción (cerradas)

| Decisión | Valor |
|---|---|
| Rooms por run | 8–12 |
| Rooms handcrafted construidas | 11 (biome dungeon onírico) |
| Sistema | Grafo modular con sockets |
| Arquitectura imposible | Topología inconsistente: mutación en revisit + lying connections |
| Primer biome | Dungeon onírico (tileset DarkDungeon) |
| Estado | ✅ Implementado en Sprint 04 |

---

## Objetivo del sistema

El sistema procedural de Restless NO debe orientarse a crear mapas infinitos o puramente aleatorios al estilo roguelike clásico.

La prioridad del proyecto es:
- atmósfera
- tensión psicológica
- exploración
- narrativa ambiental
- composición visual
- incertidumbre
- sensación onírica

El enfoque es:

> Generación procedural dirigida mediante módulos handcrafted.

El objetivo no es generar "mapas aleatorios", sino:
- construir runs coherentes
- mantener pacing psicológico
- preservar identidad visual
- reutilizar contenido manual de forma flexible

---

## Filosofía de diseño

El sistema debe generar:
- lugares memorables
- recorridos tensos
- transiciones emocionales
- arquitectura parcialmente imposible
- exploración semidirigida

Debe evitar:
- mapas genéricos
- ruido procedural sin intención
- layouts excesivamente caóticos
- sensación de "mazmorra procedural clásica"

---

## Escala de una run

Las runs deben ser cortas pero densas. **8–12 rooms por run como máximo.**

En Restless una room tiene mucho más peso temporal que en un roguelike de acción. El jugador puede pasar varios minutos explorando una sola habitación si la atmósfera funciona. Aumentar el número de rooms no mejora la experiencia — la diluye.

---

## Primer biome — Dungeon onírico

El primer biome es un **dungeon onírico** con estética de mazmorra oscura (tileset DarkDungeon). Motivos:

- Encaja perfectamente con horror psicológico y atmósfera lovecraftiana
- Permite gran variedad visual con módulos reutilizables
- Combina bien espacios abiertos y cerrados
- Facilita iluminación inquietante y surrealismo progresivo
- Facilita loops y pérdida de orientación

| Variante | Atmósfera |
|---|---|
| Piedra limpia | Tensión contenida, frialdad perturbadora |
| Abandonado | Deterioro, tiempo detenido |
| Húmedo / inundado | Claustrofobia, pérdida de control |
| Ritualizado | Horror cósmico, presencia activa, pintura de sangre |
| Orgánico / vivo | Cuerpo, infección, lo imposible |

---

## Catálogo de rooms (Sprint 04)

| Prefab | Tamaño | Tipos | Sockets | Notas |
|---|---|---|---|---|
| `entrance_hall` | Medium | Safe | N + S | Punto de inicio siempre |
| `dungeon_corridor_a` | Small | Corridor | N + S | Pasillo vertical |
| `dungeon_corridor_b` | Small | Safe | E + W | Pasillo horizontal |
| `ward_room` | Medium | Encounter + Safe | N + S + E | Sala estándar de exploración |
| `operating_theatre` | Medium | Ritual + Memory | N + S + E + W | `supportsFragments`; pintura de sangre |
| `flooded_basement` | Medium | Encounter | N + S + E + W | Alta conectividad |
| `nurses_station` | Small | DeadEnd + Safe | N | Dead end ciego |
| `memory_ward` | Medium | Memory | N + S | `supportsFragments`; fragmento garantizado aquí |
| `ritual_room` | Medium | Ritual + Encounter | N + S | Pintura de sangre, puerta en conexiones |
| `ritual_room_medium` | Medium | Ritual + Encounter | N + S + E | Variante de ritual con más sockets |
| `collapsed_wing` | Small | DeadEnd | S | Dead end decorativo |
| `landmark_exit` | Large | Landmark + Collapse | S | Una por run; punto de salida |
| `encounter_corridor` | Small | Encounter | N + S | Corredor de encuentro |
| `encounter_hall` | Medium | Encounter | N + S + E | Sala de encuentro central |
| `encounter_junction` | Medium | Encounter | N + S + E + W | Cruce de caminos |
| `encounter_room_medium` | Medium | Encounter | N + S + E | Sala de encuentro estándar |
| `safe_corner` | Small | Safe | N + E | Esquina segura |
| `safe_corridor` | Small | Safe | N + S | Corredor seguro |
| `safe_room_large` | Large | Safe | N + S + E + W | Sala segura grande |
| `safe_room_medium` | Medium | Safe | N + S | Sala segura estándar |
| `dead_end_alcove` | Small | DeadEnd | N | Alcoba sin salida |
| `dead_end_small` | Small | DeadEnd | S | Dead end pequeño |
| `memory_vault` | Medium | Memory | N + S | Bóveda de memoria |

---

## Arquitectura técnica

### Scripts principales

| Script | Rol |
|---|---|
| `RunGraphGenerator` | Genera el grafo abstracto de nodos antes de colocar rooms |
| `RoomAssembler` | Convierte el grafo en rooms instanciadas en el mundo |
| `RoomController` | Componente raíz de cada room; acceso a sockets y spawn bounds |
| `DoorSocket` | Punto de conexión en un prefab; dirección, ancho, tipos compatibles |
| `RoomCamera` | Cámara basada en room; fade entre rooms, visibilidad por room |
| `RoomEnterTrigger` | Trigger que dispara el evento `PlayerEnteredRoom` |
| `DoorCrossingTrigger` | Trigger en el hueco de puerta; gestiona la transición entre dos rooms |
| `RoomMutator` | Añadido por el assembler; detecta revisitas y aplica mutación visual |
| `DreamPresenceSpawner` | Distribuye presencias en las rooms según su metadata |

### Jerarquía de un prefab de room

```
[RoomRoot]                ← RoomController, RoomDefinition
├── Tilemaps/
│   └── Grid/
│       ├── Tilemap_Floor    ← suelo (también tintado por RoomMutator en revisit)
│       └── Tilemap_Cliff    ← paredes (TilemapCollider2D + CompositeCollider2D)
├── Sockets/
│   ├── Socket_North         ← DoorSocket, direction=North
│   ├── Socket_South         ← DoorSocket, direction=South
│   └── ...
└── SpawnZone               ← Bounds donde el spawner coloca presencias
```

---

## Proceso de generación en runtime

### Paso 1 — Seed y parámetros
La run recibe una seed desde `DreamSceneBootstrap`. Todos los pasos siguientes son deterministas para una misma seed.

### Paso 2 — Generación del grafo (`RunGraphGenerator`)
Se genera un grafo abstracto de nodos tipados:

```
Entrance → Safe → Fork
                  ↙        ↘
            Corridor     Memory
                ↓              ↓
           Encounter      Dead end
                ↓
           Landmark (Exit)
```

Parámetros configurables (en `GameConfig`): longitud mínima/máxima, probabilidad de fork, probabilidad de dead end, posición garantizada del landmark.

Garantías de diseño:
- Siempre hay nodo `Entrance` y nodo `Exit` (Landmark)
- Siempre hay al menos un nodo tipo `Memory`
- El grafo es reproducible: misma seed = mismo grafo

### Paso 3 — Ensamblaje de rooms (`RoomAssembler`)

BFS desde la entrada. Para cada nodo:
1. Se selecciona un prefab compatible (por tipo, tamaño y tags)
2. Se alinea un socket libre del nuevo prefab con el socket del room anterior
3. Se comprueba overlap con rooms ya colocadas → retry con otro prefab si colisionan
4. Hasta 3 intentos globales con seed derivada: `(seed + attempt) ^ 0xDEAD`

Después de colocar todas las rooms:
- Los sockets no ocupados se **cierran** (`CloseSocket`) — wall tiles rellenan el hueco
- Se valida navegabilidad con BFS: ¿hay camino de Entrance a Exit? Si no, nuevo intento
- Rooms tipo Ritual reciben **pintura de sangre** en paredes y suelo (`PaintRitualRooms`)
- Se añaden `RoomMutator` y lying connection (`WireUpMutators`)

### Paso 4 — Distribución de presencias (`DreamPresenceSpawner`)

`SetRooms(rooms, graph)` proporciona al spawner la lista de rooms y el grafo.

Para cada tipo de presencia:
- **Threats** — solo en rooms con `supportsThreats=true`; ponderadas por `dangerLevel`
- **Fragments** — primer fragmento garantizado en room tipo `Memory`; resto en rooms con `supportsFragments=true`
- **Wanderers** — NPCs inofensivos; no restringidos por metadata
- **Ally echoes** — solo si hay aliados activos en la run

### Paso 5 — Navegación en juego (`RoomCamera` + triggers)

`RoomCamera` solo muestra la room activa; las demás tienen sus renderers desactivados.

Al cruzar el hueco entre dos rooms, `DoorCrossingTrigger` llama a `RoomEnterTrigger.Notify(targetRoom, spawnPos)`. `RoomCamera` escucha el evento, hace fade a negro, activa la nueva room y teleporta al jugador.

---

## Socket system

Cada `DoorSocket` define:
- `direction` — cardinal (North / South / East / West)
- `width` — ancho en unidades de mundo; solo sockets con mismo ancho conectan
- `compatibleTypes` — opcional; restringe qué tipos de room pueden conectar aquí
- `isOccupied` — marcado por el assembler al conectar

`OpenSocket` elimina los wall tiles en el hueco de la puerta para que el jugador pueda pasar.  
`CloseSocket` restaura los wall tiles en sockets no conectados.

Las conexiones entre rooms de tipo Ritual usan una puerta (`PlaceSingleDoorTile`) en vez de un pasillo abierto.

---

## Arquitectura imposible — Implementación actual

### Mutación en revisit (`RoomMutator`)

Añadido dinámicamente a cada room por `WireUpMutators`.

- Escucha `RoomEnterTrigger.PlayerEnteredRoom`
- Primer visita: sin efecto
- Segunda visita: tintea `Tilemap_Floor` con un shift de hue y reducción de valor vía DOTween

Parámetros en `GameConfig`:
- `roomMutationProbability` — probabilidad de que una room mute (default 0.7)
- `mutationHueShift` — desplazamiento de hue (default 0.10)
- `mutationValueMult` — multiplicador de brillo (default 0.70, más oscuro)
- `mutationFadeDuration` — duración del fade (default 2.5s)

### Lying connection (`DoorCrossingTrigger`)

Un trigger de conexión por run es marcado como "mentiroso" por `WireUpMutators`.

- Primera vez que el jugador lo cruza: comportamiento normal
- Segunda vez: el jugador llega a una room **distinta** de la esperada

`SetLying(lyingRoomA, lyingSpawnA, lyingRoomB, lyingSpawnB)` registra los destinos alternativos.

`lyingConnectionProbability` en `GameConfig` controla si se crea la lying connection en la run (default 1.0 = siempre).

### Fases posteriores (no implementadas)

- Loops mentirosos — el jugador cree avanzar pero vuelve al mismo sitio ligeramente alterado
- Corredores relativos — un pasillo parece más largo en una segunda pasada
- Director dinámico — control de pacing, densidad y agresividad en tiempo real

---

## Herramientas de editor

| Herramienta | Ruta de menú | Función |
|---|---|---|
| `CreateRoomVariants` | Tools → Restless → Create Room Variants | Genera prefabs + RoomDefinition para salas definidas en código |
| `RebuildRoomsDarkDungeon` | Tools → Restless → Rebuild Rooms (DarkDungeon) | Repinta tilemaps, asegura colliders y registra salas en Dream.unity |
| `SetupRoomWorkshop` | Tools → Restless → Setup RoomWorkshop Scene | Configura la escena RoomWorkshop con cámara y Bootstrap |

La escena `RoomWorkshop.unity` permite iterar salas de forma standalone sin pasar por el flujo Bootstrap → Dream. `PlayFromActiveScene` fuerza Play desde esa escena cuando está activa.

---

## Parámetros configurables (GameConfig)

Los valores están en `GameConfig` y se copian a `RunConfig` al iniciar cada run.

### Grafo
`graphMinLength`, `graphMaxLength`, `forkProbability`, `deadEndProbability`

### Ensamblaje
`maxPlacementRetries`, `overlapCheckPadding`, `spawnOffset`

### Mutación
`roomMutationProbability`, `mutationHueShift`, `mutationValueMult`, `mutationFadeDuration`, `lyingConnectionProbability`

### Distribución de presencias
`entitySpawnCount`, `entityHauntedFraction`, `entityActivationDwellTime`
