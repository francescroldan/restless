# Sistema de Generación Procedural de Escenarios — Restless

# Objetivo del sistema

El sistema procedural de Restless NO debe orientarse a crear mapas infinitos o puramente aleatorios al estilo roguelike clásico.

La prioridad del proyecto es:
- atmósfera
- tensión psicológica
- exploración
- narrativa ambiental
- composición visual
- incertidumbre
- sensación onírica

Por este motivo, el enfoque recomendado es:

> Generación procedural dirigida mediante módulos handcrafted.

El objetivo no es generar “mapas aleatorios”, sino:
- construir runs coherentes
- mantener pacing psicológico
- preservar identidad visual
- reutilizar contenido manual de forma flexible

---

# Filosofía de diseño

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
- sensación de “mazmorra procedural clásica”

---

# Enfoque general

La generación procedural estará basada en:

- habitaciones modulares diseñadas manualmente
- ensamblaje procedural mediante conectores
- metadata semántica
- control de pacing
- director dinámico
- corrupción onírica progresiva

El mapa NO se generará tile a tile.

El mundo se construirá ensamblando “rooms” prefabricadas.

---

# Arquitectura general del sistema

El sistema se divide en varias capas:

1. Generación estructural
2. Generación gameplay
3. Dressing ambiental
4. Corrupción onírica
5. Director dinámico

---

# 1. Generación estructural

# Rooms modulares

Cada habitación será un prefab independiente diseñado manualmente.

Cada room debe tener:
- identidad visual
- composición propia
- gameplay específico
- iluminación propia
- navegación validada
- puntos de interés

Ejemplos:
- corredor inundado
- dormitorio abandonado
- bosque de estatuas
- aula destruida
- iglesia torcida
- quirófano
- biblioteca imposible

---

# Tamaños recomendados

Las habitaciones deberían clasificarse por tamaño:

- Small
- Medium
- Large
- Landmark

Esto ayudará al pacing y al control del layout.

---

# Tipos recomendados

Cada room puede pertenecer a uno o varios tipos:

- corridor
- puzzle
- safe
- ritual
- encounter
- traversal
- collapse
- memory
- dead_end
- landmark

---

# Conectores

Las habitaciones NO deben conectarse libremente.

Cada room debe contener sockets o puntos de conexión.

Ejemplo:

- North
- South
- East
- West

O preferiblemente:
- DoorSocket components

Cada socket define:
- posición
- dirección
- tamaño
- tipo compatible

---

# Compatibilidad de conexiones

No todas las rooms pueden conectar con todas.

Ejemplo:

- una habitación “large” no conecta con un pasillo estrecho
- ciertas zonas solo conectan dentro del mismo biome
- rooms rituales requieren pacing específico

---

# Construcción del mapa

El mapa debe generarse como un grafo de habitaciones.

NO como un grid puro.

Cada room representa un nodo.

Las conexiones representan transiciones.

Ejemplo conceptual:

Entrada
↓
SafeRoom
↓
Fork
↙      ↘
Puzzle   Threat
↓          ↓
Collapse Event
↓
Exit

Esto permite:
- controlar ritmo
- crear loops
- diseñar backtracking
- insertar eventos
- gestionar tensión

---

# Estructura recomendada de generación

La generación debe seguir pasos secuenciales:

## Paso 1 — Selección de seed

La run recibe:
- seed
- biome inicial
- profundidad
- modifiers

---

## Paso 2 — Generación de layout

Se genera:
- estructura principal
- caminos
- forks
- loops
- dead ends

Todavía sin gameplay.

---

## Paso 3 — Colocación de rooms

El sistema:
- selecciona prefabs compatibles
- alinea sockets
- evita overlaps
- valida navegabilidad

---

## Paso 4 — Generación gameplay

Se añaden:
- presencias
- amenazas
- fragmentos
- eventos
- aliados
- triggers

---

## Paso 5 — Dressing ambiental

Añadir:
- props
- decals
- partículas
- sonidos
- variaciones visuales

---

## Paso 6 — Corrupción onírica

Modificar:
- geometría
- iluminación
- audio
- lógica espacial
- estabilidad arquitectónica

---

# Metadata semántica

Cada room debe contener metadata descriptiva.

Esto es CRÍTICO.

La generación NO debe basarse solo en geometría.

Debe basarse también en:
- emoción
- tensión
- narrativa ambiental

---

# Ejemplo de metadata

```json
{
  "id": "hospital_corridor_01",
  "size": "medium",
  "biome": "hospital",
  "tags": [
    "claustrophobic",
    "wet",
    "unsafe",
    "low_visibility"
  ],
  "supportsThreats": true,
  "supportsFragments": false,
  "supportsAllies": false,
  "dangerLevel": 0.7,
  "surrealism": 0.3
}
```

---

# Tags emocionales

Cada room debería incluir tags emocionales.

Ejemplos:

- oppressive
- silent
- ritual
- rotten
- alive
- impossible
- narrow
- abandoned
- flooded
- unstable
- sacred
- infected

El generador debe usar estas tags para construir pacing psicológico.

---

# Pacing emocional

La generación debe construir una curva emocional.

NO debe generar rooms aleatoriamente sin ritmo.

---

# Arquitectura imposible

El sueño NO debe comportarse siempre como un espacio físico lógico.

Esto es una de las herramientas más importantes del juego.

---

# Director dinámico

El sistema debe incluir un “director” superior.

Responsabilidades:
- pacing
- densidad
- agresividad
- surrealismo
- respiración emocional

---

# Sistema de biomes

Cada biome debe tener:
- identidad visual
- reglas propias
- entidades específicas
- arquitectura propia
- audio específico
- tipo de surrealismo

---

# Recomendación técnica en Unity

NO usar un tilemap gigante procedural.

Usar:
- prefabs de rooms
- sockets
- ensamblaje modular

---

# Objetivo final

El sistema procedural NO debe sentirse como un generador de mapas.

Debe sentirse como:
- un sueño inestable
- parcialmente coherente
- parcialmente roto
- emocionalmente dirigido

La prioridad absoluta es:
- atmósfera
- incertidumbre
- identidad visual
- terror psicológico
