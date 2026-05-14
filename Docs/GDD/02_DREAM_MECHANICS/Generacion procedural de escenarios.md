# Sistema de Generación Procedural de Escenarios — Restless

---

## Decisiones de producción (cerradas)

| Decisión | Valor |
|---|---|
| Rooms por run | 8–12 |
| Rooms handcrafted para primer sprint | 6–8 |
| Sistema | Grafo modular con sockets |
| Arquitectura imposible | Topología inconsistente controlada (no geometría física real) |
| Primer biome | Dungeon onírico |
| Objetivo del sprint | Validar atmósfera, pacing, navegación y tensión psicológica |

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

Para producción inicial: **6–8 rooms handcrafted**, reutilizadas proceduralmente mediante conexiones variables. El objetivo es validar el sistema, no acumular contenido.

---

## Primer biome — Dungeon onírico

El primer biome es un **dungeon onírico** con estética de mazmorra oscura (tileset DarkDungeon). Motivos:

- Encaja perfectamente con horror psicológico y atmósfera lovecraftiana
- Permite gran variedad visual con módulos reutilizables
- Combina bien espacios abiertos y cerrados
- Facilita iluminación inquietante y surrealismo progresivo
- Facilita loops y pérdida de orientación

El dungeon permite variaciones internas sin necesidad de un segundo biome:

| Variante | Atmósfera |
|---|---|
| Piedra limpia | Tensión contenida, frialdad perturbadora |
| Abandonado | Deterioro, tiempo detenido |
| Húmedo / inundado | Claustrofobia, pérdida de control |
| Ritualizado | Horror cósmico, presencia activa, sangre |
| Orgánico / vivo | Cuerpo, infección, lo imposible |

El objetivo no es un dungeon realista, sino un espacio que se siente **parcialmente mazmorra, parcialmente recuerdo roto, parcialmente sueño**.

---

## Arquitectura imposible — Enfoque

El sistema NO intentará en esta fase:
- Geometría no euclidiana real
- Espacios físicamente superpuestos
- Rooms coexistiendo en múltiples estados simultáneos
- Interpenetración física real

El enfoque es **arquitectura imposible percibida** — mismo efecto psicológico, implementación viable:

- **Topología inconsistente** — una puerta lleva a sitios distintos según el contexto
- **Loops mentirosos** — el jugador cree avanzar pero vuelve al mismo sitio ligeramente alterado
- **Revisits mutados** — una habitación cambia iluminación, props o layout al volver
- **Corredores relativos** — un pasillo parece más largo o distinto en una segunda pasada
- **Conexiones ilógicas** — transiciones que no tienen sentido geométrico

Esto consigue desorientación, inseguridad espacial y sensación onírica sin complejidad técnica innecesaria.

---

## Arquitectura general del sistema

El sistema se divide en capas. Las primeras tres son el objetivo del sprint inicial; las últimas dos son fases posteriores.

### Capa 1 — Generación estructural *(sprint inicial)*
Grafo de rooms, sockets, ensamblaje, validación de navegabilidad.

### Capa 2 — Generación gameplay *(sprint inicial)*
Colocación de presencias, fragmentos, aliados y eventos en las rooms generadas.

### Capa 3 — Dressing ambiental *(sprint inicial, básico)*
Props, variaciones visuales, iluminación por room.

### Capa 4 — Corrupción onírica *(fase posterior)*
Mutación de rooms en revisit, loops imposibles, geometría alterada.

### Capa 5 — Director dinámico *(fase posterior)*
Control de pacing, densidad, agresividad y surrealismo en tiempo real.

---

## Rooms modulares

Cada habitación es un prefab independiente diseñado manualmente.

Cada room debe tener:
- Identidad visual propia
- Composición y navegación validadas
- Metadata semántica
- Sockets de conexión

### Tamaños

| Tamaño | Uso |
|---|---|
| Small | Corredores, transiciones, dead ends |
| Medium | Rooms estándar de exploración |
| Large | Rooms de encuentro, ritualizada, landmark |
| Landmark | Una por run, punto memorable y orientador |

### Tipos

`corridor` · `puzzle` · `safe` · `ritual` · `encounter` · `traversal` · `collapse` · `memory` · `dead_end` · `landmark`

---

## Sockets y conexiones

Cada room contiene `DoorSocket` components que definen:
- Posición
- Dirección
- Tamaño compatible
- Tipo compatible

No todas las rooms pueden conectar con todas. Ejemplo:
- Una room `large` no conecta con un pasillo estrecho
- Rooms rituales requieren pacing específico previo
- Ciertas rooms solo conectan dentro del mismo biome

---

## Metadata semántica

**Crítico.** La generación no se basa solo en geometría sino también en emoción, tensión y narrativa ambiental.

```json
{
  "id": "dungeon_corridor_01",
  "size": "medium",
  "biome": "dungeon",
  "type": ["corridor", "traversal"],
  "tags": ["claustrophobic", "wet", "unsafe", "low_visibility"],
  "supportsThreats": true,
  "supportsFragments": false,
  "supportsAllies": false,
  "dangerLevel": 0.7,
  "surrealism": 0.3
}
```

### Tags emocionales disponibles

`oppressive` · `silent` · `ritual` · `rotten` · `alive` · `impossible` · `narrow` · `abandoned` · `flooded` · `unstable` · `sacred` · `infected` · `claustrophobic` · `wet` · `low_visibility`

---

## Grafo de rooms

El mapa se genera como un **grafo de habitaciones**, no como un grid.

Cada room es un nodo. Las conexiones son transiciones. Esto permite controlar ritmo, crear loops, diseñar backtracking e insertar eventos.

Estructura tipo de una run (8–10 rooms):

```
Entrada
  ↓
Safe (orientación)
  ↓
Fork
 ↙        ↘
Corridor   Memory room
 ↓              ↓
Encounter   Dead end
 ↓
Landmark
  ↓
Collapse / Exit
```

---

## Proceso de generación

### Paso 1 — Seed y parámetros
La run recibe: seed, biome, profundidad, modificadores de aliados activos.

### Paso 2 — Layout estructural
Se genera el grafo: nodos, forks, loops, dead ends. Sin gameplay todavía.

### Paso 3 — Asignación de rooms
El sistema selecciona prefabs compatibles, alinea sockets, evita overlaps, valida navegabilidad.

### Paso 4 — Gameplay
Se colocan presencias, fragmentos, aliados y triggers según la metadata de cada room.

### Paso 5 — Dressing básico
Props, variaciones de iluminación, audio por room.

### Paso 6 — Corrupción onírica *(fase posterior)*
Mutación de geometría, iluminación, lógica espacial.

---

## Objetivo de validación del primer sprint

Antes de construir más contenido, el sistema debe demostrar que explorar el mundo generado produce:

- tensión
- incertidumbre
- sensación onírica
- miedo a seguir avanzando

Ese es el núcleo real de Restless. Si el sistema no consigue eso con 6–8 rooms, añadir más rooms no lo arreglará.
