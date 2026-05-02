# Restless — Timeline de desarrollo

## Cómo usar este documento

- Cada hito tiene su propio plan detallado en esta misma carpeta.
- Un hito se marca como **completado** cuando el resultado es jugable y verificado, no cuando el código existe.
- Los hitos son secuenciales: no empieza el siguiente hasta dar por bueno el anterior.
- Dentro de cada hito puede haber sub-tareas en paralelo, pero el hito en sí es la condición de salida.

---

## Hitos

| # | Hito | Estado | Plan |
|---|------|--------|------|
| M0 | Definir el loop del MOC | ⬜ Pendiente | [M0-Definir-Loop-MOC.md](M0-Definir-Loop-MOC.md) |
| M1 | Setup técnico del proyecto | ⬜ Pendiente | [M1-Setup-Tecnico.md](M1-Setup-Tecnico.md) |
| M2 | Prototipo gris del loop core | ⬜ Pendiente | [M2-Prototipo-Loop.md](M2-Prototipo-Loop.md) |
| M3 | Identidad visual base | ⬜ Pendiente | [M3-Identidad-Visual.md](M3-Identidad-Visual.md) |
| M4 | Hub de Vigilia completo | ⬜ Pendiente | [M4-Hub-Vigilia.md](M4-Hub-Vigilia.md) |
| M5 | Primer aliado end-to-end | ⬜ Pendiente | [M5-Primer-Aliado.md](M5-Primer-Aliado.md) |
| M6 | MOC completo y jugable | ⬜ Pendiente | [M6-MOC-Completo.md](M6-MOC-Completo.md) |

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

- [ ] El jugador puede iniciar una run desde la Vigilia.
- [ ] El Sueño tiene un loop reconocible con inicio, tensión y salida.
- [ ] El Medidor de Inquietud afecta visualmente al entorno y tiene consecuencias reales.
- [ ] Despertar tranquilo y abrupto producen resultados distintos en la Vigilia.
- [ ] Al menos 2 aliados con builds diferentes cambian cómo se juega el Sueño.
- [ ] La paleta monocromática con acento de color está implementada.
- [ ] El juego no crashea en las rutas principales.
