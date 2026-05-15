# Sprint 03 — Entidades del sueño: presencias espectrales

**Estado:** ✅ Completado  
**Fase:** Post-MOC  
**Prerequisito:** [Sprint 02 — UX Polish](Sprint02-UX-Polish.md) ✅ cerrado

> **Nota de implementación:** el plan original contemplaba un sistema de neblina/revelación con `DreamFog`. Tras evaluar el scope y las prioridades del Sprint 04 (generación procedural), se adoptó un enfoque más directo: presencias tipadas gestionadas por `DreamPresenceSpawner`, sin capa de revelación. El sistema de niebla queda como candidato para un sprint posterior si el playtest lo demanda.

---

## Objetivo

Transformar las entidades del sueño de obstáculos simples y predecibles en elementos de incertidumbre activa. El jugador debe decidir constantemente si vale la pena revelar lo que tiene delante. El cono de visión pasa de ser herramienta defensiva a herramienta de exploración con coste.

---

## Concepto central: la neblina

Cualquier entidad del sueño puede aparecer inicialmente como una **neblina** — una presencia translúcida, indefinida, sin forma clara. Al entrar en el cono de visión del protagonista durante un tiempo mínimo (dwell time), la neblina se **revela**: colapsa en lo que realmente es.

El jugador nunca sabe de antemano qué es. Mirar tiene coste potencial; no mirar también.

### Tipos de revelación

| Tipo | Al revelarse | Efecto | Frecuencia orientativa |
|------|-------------|--------|----------------------|
| **Amenaza** | Entidad activa (la estatua u otras) | Se activa, sube inquietud al estar en cono | ~35% |
| **NPC errante** | Presencia inofensiva del sueño | Deambula o desaparece, sin efecto | ~30% |
| **Fragmento** | Punto de memoria visible | Marca la localización; el minijuego sigue siendo necesario | ~20% |
| **Aliado** | Eco de un aliado conocido | Beneficio temporal (reducción de tasa, pulso de calma…) | ~15% |

Las frecuencias son configurables en `GameConfig`. El objetivo de diseño es que el jugador nunca automatice la decisión de mirar.

### NPCs errantes como entidades visibles

Algunos NPCs errantes pueden **no ser neblinas** — ya son visibles desde el principio, sin revelación. Son ruido ambiental del sueño: figuras que deambulan sin propósito, ignoran al protagonista, y refuerzan la atmósfera de espacio habitado pero ajeno. Su presencia hace que el jugador no pueda fiarse de que "lo que ya se ve" sea siempre inofensivo.

---

## Tareas

### E1 — Sistema base de neblina y revelación

**Problema:** las entidades actuales son estáticamente visibles y predecibles. No hay incertidumbre sobre qué son.

**Solución:** Crear un componente `DreamFog` que envuelve cualquier entidad del sueño. En estado sin revelar, muestra un sprite translúcido genérico. Al entrar en el cono de visión el tiempo suficiente, dispara la revelación y activa el comportamiento real del objeto subyacente.

**Criterios de salida:**
- [ ] Componente `DreamFog` implementado con estados: `Hidden`, `Revealing`, `Revealed`
- [ ] Dwell time configurable en `GameConfig`
- [ ] Efecto visual de transición (fade / materialización) al revelar
- [ ] Compatible con los cuatro tipos de revelación
- [ ] Las entidades ya existentes (estatua/DreamPresence) pueden funcionar como neblina o sin ella

---

### E2 — Tipo: Amenaza (integración con sistema existente)

**Solución:** La entidad `DreamPresence` existente puede arrancar como neblina. Al revelarse, activa su comportamiento de patrulla normal. El sistema de detección y subida de inquietud no cambia.

**Criterios de salida:**
- [ ] `DreamPresence` puede configurarse para aparecer como neblina o directamente visible
- [ ] Al revelar una amenaza, la transición visual comunica claramente el peligro
- [ ] El comportamiento post-revelación es idéntico al actual

---

### E3 — Tipo: NPC errante

**Solución:** Nueva entidad `WanderingNPC` — presencia inofensiva que deambula por el nivel con movimiento suave e irregular. No interactúa con el jugador, no afecta a la inquietud. Puede aparecer como neblina (revelándose como NPC) o ya visible desde el principio.

**Criterios de salida:**
- [ ] Componente `WanderingNPC` con movimiento de deambulación (waypoints aleatorios o steering suave)
- [ ] Sprite / apariencia diferenciable de las amenazas una vez revelado
- [ ] Configurable: neblina o visible desde el inicio
- [ ] No afecta al sistema de inquietud ni a la detección

---

### E4 — Tipo: Fragmento revelable

**Solución:** Un `MemoryPoint` puede iniciarse oculto bajo una neblina. Al revelarla, el punto de memoria se hace visible e interactuable. El minijuego sigue siendo necesario para extraer el fragmento.

**Criterios de salida:**
- [ ] `MemoryPoint` puede configurarse con estado inicial oculto (neblina)
- [ ] Al revelar, la transición visual indica claramente que es un punto de memoria
- [ ] El flujo de interacción y minijuego no cambia
- [ ] El jugador no puede interactuar con el punto hasta que esté revelado

---

### E5 — Tipo: Eco de aliado

**Solución:** Nueva entidad `AllyEcho` — aparece como neblina. Al revelarse, muestra brevemente la silueta del aliado correspondiente y aplica un beneficio temporal al protagonista.

**Criterios de salida:**
- [ ] Componente `AllyEcho` con tipo de aliado configurable (o aleatorio entre los aliados activos de la run)
- [ ] Beneficio temporal implementado: al menos reducción de tasa de inquietud durante N segundos
- [ ] Duración y magnitud del beneficio configurables en `GameConfig`
- [ ] Visualmente distinguible del resto de revelaciones
- [ ] Solo aparece si el jugador tiene aliados activos en la run

---

### E6 — Spawner y distribución en escena

**Solución:** Sistema `DreamFogSpawner` que instancia neblinas en el nivel según las frecuencias configuradas, en posiciones predefinidas o con cierta aleatoriedad controlada.

**Criterios de salida:**
- [ ] `DreamFogSpawner` con tabla de frecuencias configurable en `GameConfig`
- [ ] Puede colocar neblinas en posiciones fijas (designer-placed) o en puntos aleatorios dentro de una zona
- [ ] Garantiza mínimo 1 fragmento revelable por nivel (para que el sistema tenga propósito)
- [ ] No coloca neblinas solapadas ni demasiado cerca del punto de inicio

---

## Orden sugerido

1. **E1** — sistema base (todo lo demás depende de él)
2. **E2** — integración con amenaza existente (valida el sistema con contenido ya funcional)
3. **E3** — NPC errante (el más sencillo, da cuerpo al nivel)
4. **E4** — fragmento revelable (impacto directo en el loop de exploración)
5. **E6** — spawner (para poder probar la distribución en juego)
6. **E5** — eco de aliado (el más complejo, puede dejarse para el final del sprint)

---

## Lo que se implementó

- [x] `DreamPresenceSpawner` gestiona presencias tipadas (Threat, Wanderer, Fragment, Ally) por run
- [x] `SetRooms(rooms, graph)` — el spawner lee las rooms instanciadas y sus zonas de spawn
- [x] Respeta `supportsThreats` y `supportsFragments` por room
- [x] Distribución ponderada por `dangerLevel` — rooms más peligrosas reciben más amenazas
- [x] Primer fragmento garantizado en room tipo `memory`; fallback a cualquier room con `supportsFragments`
- [x] `WanderingNPC` — presencia inofensiva con movimiento de deambulación por waypoints
- [x] Todas las frecuencias y parámetros en `GameConfig`, sin hardcoding

## Criterios de salida del sprint

- [x] Las presencias se distribuyen respetando la metadata de las rooms
- [x] Hay siempre al menos 1 fragmento de memoria colocable por run
- [x] Los NPCs errantes se mueven sin afectar a la inquietud
- [x] Sin hardcoding: todo configurable desde `GameConfig`
- [x] Sin regresiones en el loop principal (minijuego, inquietud, timer, despertar)
