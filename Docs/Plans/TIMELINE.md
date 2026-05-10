# Restless — Timeline de desarrollo

## Cómo usar este documento

- Cada hito tiene su propio plan detallado en esta misma carpeta. Los hitos M0–M6 están en la subcarpeta [MOC/](MOC/).
- Un hito se marca como **completado** cuando el resultado es jugable y verificado, no cuando el código existe.
- Los hitos son secuenciales: no empieza el siguiente hasta dar por bueno el anterior.
- A partir de S1 los hitos se llaman Sprints (S1, S2…) para reflejar que el MOC ya está cerrado.
- Dentro de cada hito puede haber sub-tareas en paralelo, pero el hito en sí es la condición de salida.

---

## Hitos

| # | Hito | Estado | Plan |
|---|------|--------|------|
| M0 | Definir el loop del MOC | ✅ Completado | [MOC/M0-Definir-Loop-MOC.md](MOC/M0-Definir-Loop-MOC.md) |
| M1 | Setup técnico del proyecto | ✅ Completado | [MOC/M1-Setup-Tecnico.md](MOC/M1-Setup-Tecnico.md) |
| M2 | Prototipo gris del loop core | ✅ Completado | [MOC/M2-Prototipo-Loop.md](MOC/M2-Prototipo-Loop.md) |
| M3 | Identidad visual base | ✅ Completado | [MOC/M3-Identidad-Visual.md](MOC/M3-Identidad-Visual.md) |
| M4 | Hub de Vigilia completo | ✅ Completado | [MOC/M4-Hub-Vigilia.md](MOC/M4-Hub-Vigilia.md) |
| M5 | Primer aliado end-to-end | ✅ Completado | [MOC/M5-Primer-Aliado.md](MOC/M5-Primer-Aliado.md) |
| M6 | MOC completo y jugable | ✅ Completado | [MOC/M6-MOC-Completo.md](MOC/M6-MOC-Completo.md) |
| S1 | RunConfig: parámetros modificables por run | ✅ Completado | [Sprint01-RunConfig.md](Sprint01-RunConfig.md) |
| S2 | UX Polish: feedback, HUD ojo y tileset | 🔄 En curso | [Sprint02-UX-Polish.md](Sprint02-UX-Polish.md) |

**Leyenda:** ⬜ Pendiente · 🔄 En curso · ✅ Completado

---

## Visión general del arco

```
M0  ──►  M1  ──►  M2  ──►  M3
Diseño   Setup   Loop gris  Visuals
                    │
                    ▼
              M4 ──► M5 ──► M6
             Vigilia  Aliado  MOC
```

### ¿Por qué este orden?

- **M0 antes de M1**: no hay nada que implementar si no sabemos qué loop vamos a construir.
- **M2 antes de M3**: el loop gris tiene que funcionar antes de vestirlo. El arte sobre mecánicas rotas es tiempo perdido.
- **M3 antes de M4/M5**: la identidad visual define cómo se ve la habitación, los aliados y los efectos de inquietud. Mejor tenerla resuelta antes de construir esas pantallas.
- **M5 antes de M6**: el sistema de aliados es el corazón del juego. El MOC no tiene sentido sin al menos un aliado que modifique el loop.

---

## Criterios de salida globales (MOC)

Al finalizar M6, el juego debe poder ser jugado de principio a fin por alguien que no ha visto el código:

- [x] El jugador puede iniciar una run desde la Vigilia.
- [x] El Sueño tiene un loop reconocible con inicio, tensión y salida.
- [x] El Medidor de Inquietud afecta visualmente al entorno y tiene consecuencias reales.
- [x] Despertar tranquilo y abrupto producen resultados distintos en la Vigilia.
- [x] Al menos 2 aliados con builds diferentes cambian cómo se juega el Sueño.
- [x] La paleta monocromática con acento de color está implementada.
- [x] El juego no crashea en las rutas principales.

**MOC cerrado — 2026-05-08.** Playtest con 3 jugadores externos (8, 8 y 13 años). Resultado: experiencia jugable satisfactoria. Feedback recogido en Sprint 01.
