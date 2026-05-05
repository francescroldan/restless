# MFinal — Cierre del MOC

**Estado:** 🔄 En curso  
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
| Zona profunda solo alcanzable en ~60% del tiempo | ⚙ Implementar | Requiere tuning del timer con el nivel real; sin esto la tensión de elección no existe |
| Testear fatiga visual tras 5 min de juego | ⚙ Implementar | Validación interna antes del playtest externo |
| Playtest externo (≥2 personas) | ⚙ Implementar | Criterio de cierre del MOC |
| Ajustes post-playtest | ⚙ Implementar | Solo los que afecten comprensión básica del loop |

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
| Zona profunda solo accesible en ~60% del tiempo | ⚙ Falta tuning |
| Playtest externo confirma comprensión del loop | ⚙ Sin hacer |
| No hay crashes en rutas principales | ⚙ Sin verificar sistemáticamente |

---

## Plan de acción para cerrar el MOC

Ordenado por prioridad:

1. **Reset de partida nueva** — sin esto los testers empiezan con estado sucio.
2. **Tuning del timer** — ajustar `dreamDuration` para que la zona profunda sea una decisión, no un paseo.
3. **Verificación de crashes** — un run completo con cada combinación de aliados: sin aliados, solo Sabio, solo Héroe.
4. **Revisión de fatiga visual** — 5 min continuados con restlessness elevada.
5. **Playtest externo** — ≥2 personas, documentar resultados.
6. **Ajustes post-playtest** — solo comprensión básica, no nice-to-haves.

Cuando los 6 pasos estén completos, M6 se cierra y el MOC está listo.
