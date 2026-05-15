# Restless — Timeline de desarrollo

Un hito se marca como **completado** cuando el resultado es jugable y verificado por alguien ajeno al código, no cuando el sistema existe.

---

## Fases

| Fase | Objetivo | Estado | Cierre |
|---|---|---|---|
| **Fase 0 — MOC** | Demostrar que el concepto es jugable | ✅ Completado | 2026-05-08 |
| **Fase 1 — Infraestructura del Sueño** | Convertir el prototipo en un sistema que aguante iteración | ✅ Completado | 2026-05-15 |
| **Fase 2 — Atmósfera** | Que el mundo generado se sienta como Restless, no como una demo técnica | ⬜ Por empezar | — |

---

## Fase 0 — MOC

> Objetivo: que el concepto sea jugable de principio a fin por alguien externo.

| Hito | Estado | Plan |
|---|---|---|
| M0 — Definir el loop | ✅ | [MOC/M0-Definir-Loop-MOC.md](MOC/M0-Definir-Loop-MOC.md) |
| M1 — Setup técnico | ✅ | [MOC/M1-Setup-Tecnico.md](MOC/M1-Setup-Tecnico.md) |
| M2 — Prototipo gris del loop core | ✅ | [MOC/M2-Prototipo-Loop.md](MOC/M2-Prototipo-Loop.md) |
| M3 — Identidad visual base | ✅ | [MOC/M3-Identidad-Visual.md](MOC/M3-Identidad-Visual.md) |
| M4 — Hub de Vigilia completo | ✅ | [MOC/M4-Hub-Vigilia.md](MOC/M4-Hub-Vigilia.md) |
| M5 — Primer aliado end-to-end | ✅ | [MOC/M5-Primer-Aliado.md](MOC/M5-Primer-Aliado.md) |
| M6 — MOC completo y jugable | ✅ | [MOC/M6-MOC-Completo.md](MOC/M6-MOC-Completo.md) |

**Cerrado 2026-05-08.** Playtest con 3 jugadores externos. Experiencia jugable satisfactoria.

---

## Fase 1 — Infraestructura del Sueño

> Objetivo: RunConfig configurable, entidades vivas, world procedural. El sueño deja de ser un nivel estático.

| Sprint | Contenido | Estado | Plan |
|---|---|---|---|
| S1 — RunConfig | Parámetros modificables por run vía GameConfig | ✅ | [Sprint01-RunConfig.md](Sprint01-RunConfig.md) |
| S2 — UX Polish | Feedback visual, HUD ojo, tileset dungeon | ✅ | [Sprint02-UX-Polish.md](Sprint02-UX-Polish.md) |
| S3 — Presencias | Spawner de entidades tipadas, WanderingNPC, distribución por metadata | ✅ | [Sprint03-Entities.md](Sprint03-Entities.md) |
| S4 — Generación procedural | Grafo, ensamblador, 11 rooms handcrafted, mutación, lying connections | ✅ | [Sprint04-ProceduralGen.md](Sprint04-ProceduralGen.md) |

**Cerrado 2026-05-15.**

---

## Fase 2 — Atmósfera

> Objetivo: que una partida completa produzca tensión, incertidumbre y sensación onírica reconocibles. El jugador no debe poder predecir lo que va a encontrar, ni visual ni narrativamente.

Candidatos para esta fase (a priorizar juntos antes de arrancar):

- **Sistema de niebla y revelación** — presencias que no revelan su naturaleza hasta que el jugador las mira (plan original S3, aplazado)
- **Narrativa ambiental mínima** — props, notas, indicios del hijo en las rooms
- **Audio por room** — ambiente sonoro diferenciado según el tipo de sala
- **Más rooms con identidad propia** — ampliar el catálogo del biome dungeon

Sprints concretos: por definir al empezar la fase.

---

**Leyenda:** ⬜ Por empezar · 🔄 En curso · ✅ Completado
