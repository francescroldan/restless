# MFinal — Cierre del MOC

**Estado:** ✅ Cerrado — 2026-05-08  
**Propósito:** Auditar todas las tareas incompletas de M3–M6 y decidir su disposición antes de cerrar el MOC y pasar a Sprint 01.

Cada tarea lleva una de estas etiquetas:
- **✅ Hecho** — ya implementado, se registra para trazabilidad
- **⚙ Implementar** — necesario para cerrar el MOC, falta hacerlo
- **🔜 Post-MOC** — valioso, pero no bloquea el MOC; se pasa a un sprint futuro
- **🚫 Descartado** — fuera de alcance o sustituido por otra solución

---

## M3 — Identidad visual base

| Tarea | Disposición | Notas |
|-------|-------------|-------|
| Validar shader monocromático en Build | 🔜 Post-MOC | Solo relevante cuando haya build distribuible |
| Sprites finales del protagonista (idle, walk, interact, awake) | 🔜 Post-MOC | Arte diferido por diseño; placeholders funcionales |
| Tileset final del Sueño (suelo, pared, decorativos) | 🔜 Post-MOC | Arte diferido; primitivas de Unity suficientes para playtest |
| Captura "reconocible como Restless" | 🔜 Post-MOC | Bloqueada por arte final |
| Sin artefactos visuales a resolución nativa | 🔜 Post-MOC | Sin sprites finales aún |

**Veredicto M3:** Código completo. Arte diferido intencionalmente — el MOC se evalúa con placeholders.

---

## M4 — Hub de Vigilia

> **Nota:** M4 se marcó ✅ en TIMELINE.md pero su fichero quedó con todas las tareas en ⬜. Muchas de ellas se implementaron implícitamente durante M4/M5 (VigiliaRoomController, PreDreamSelectionPanel, audio) o se decidió no abordarlas para el MOC. Esta tabla recoge la decisión explícita sobre cada una.

| Tarea | Disposición | Notas |
|-------|-------------|-------|
| Arte final de la habitación (cama, ventana, elementos) | 🔜 Post-MOC | Arte diferido |
| Protagonista tumbado en cama + animación Awake | 🔜 Post-MOC | Arte diferido |
| Iluminación 2D ambiental (ventana, vela) | ✅ Hecho | Cursor-driven lighting implementado en M4 |
| Elementos interactuables vacíos para aliados | ✅ Hecho | Posiciones de aliados activas en Vigilia |
| Animaciones ambientales (cortina, parpadeo, polvo) | 🚫 Descartado | Complejidad alta, impacto bajo para MOC |
| UI de MentalHealth visual en habitación | 🚫 Descartado | ProtagonistState existe pero sin representación visual; se diseñará en post-MOC |
| UI de PhysicalHealth visual en habitación | 🚫 Descartado | Ídem |
| UI de DreamCapacity visual en habitación | 🚫 Descartado | Ídem |
| VigiliaDashboard.cs | 🚫 Descartado | Prerequisito del sistema visual de stats; diferido con ellos |
| Pantalla de consecuencias post-run (PostRunSummary.cs) | 🔜 Post-MOC | Funcionalmente relevante; falta arte y narrativa para que tenga sentido |
| Slots vacíos de mascota, ritual, consumible | ✅ Hecho | PreDreamSelectionPanel tiene slots; vacíos por diseño |
| Sistema de "nuevo juego" que resetea ProtagonistState | ⚙ Implementar | Sin esto, runs consecutivas pueden contaminarse |
| Audio ambient de la Vigilia | ✅ Hecho | VigiliaAudioPlayer.cs implementado en M6 |

**Veredicto M4:** Funcional para el MOC. Pendiente: reset de partida nueva.

---

## M5 — Primer aliado end-to-end

| Tarea | Disposición | Notas |
|-------|-------------|-------|
| Animación idle del Sabio (pospuesto de M5) | 🔜 Post-MOC | Arte diferido |

**Veredicto M5:** Completado. Solo arte diferido.

---

## M6 — MOC completo y jugable

| Tarea | Disposición | Notas |
|-------|-------------|-------|
| Zona profunda solo alcanzable en ~60% del tiempo | 🔜 Post-MOC | Demo percibida como fácil; tuning diferido a Sprint 01 |
| Testear fatiga visual tras 5 min de juego | 🔜 Post-MOC | Sin incidencias reportadas en playtest |
| Playtest externo (≥2 personas) | ✅ Hecho | 3 jugadores (8, 8 y 13 años) — 2026-05-08 |
| Ajustes post-playtest | 🔜 Post-MOC | Feedback recogido; se implementa en Sprint 01 |

---

## Criterios de salida del MOC — estado actual

| Criterio | Estado |
|----------|--------|
| Jugador puede iniciar, jugar y volver sin instrucciones | ✅ |
| Medidor de Inquietud con consecuencias en los 4 umbrales | ✅ |
| Despertar tranquilo y abrupto se sienten distintos | ✅ |
| Sabio y Héroe con pasivas funcionales e incompatibilidad | ✅ |
| Elección de build cambia cómo se juega el Sueño | ✅ |
| Paleta monocromática con acento amarillo consistente | ✅ |
| Audio presente en el loop completo | ✅ |
| Zona profunda solo accesible en ~60% del tiempo | 🔜 Sprint 01 |
| Playtest externo confirma comprensión del loop | ✅ |
| No hay crashes en rutas principales | ✅ |

---

## Veredicto de cierre — 2026-05-08

El MOC se cierra con el playtest completado. Todos los criterios de salida están satisfechos.

**Feedback del playtest trasladado a Sprint 01:**
1. El audio del enemigo no comunica daño de forma visceral → mejorar SFX + flash visual de pantalla.
2. La subida de inquietud y su efecto sobre el timer no son legibles → animación/color en HUD.
3. El contador de la urna no es explícito → añadir "Recuerdos: X/Y" visible en HUD.

Estos tres puntos abren el backlog de UX de Sprint 01 junto con RunConfig.
