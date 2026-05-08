# M6 — MOC completo y jugable

**Estado:** ✅ Completado — 2026-05-08  
**Hito anterior:** [M5 — Primer aliado end-to-end](M5-Primer-Aliado.md)  
**Hito siguiente:** [Sprint 01 — RunConfig](../Sprint01-RunConfig.md)

---

## Objetivo

Cerrar el MOC: el juego es jugable de principio a fin por alguien externo al proyecto sin necesitar explicaciones. Este hito no añade sistemas nuevos — ensambla, pule y extiende lo construido en M1–M5 hasta alcanzar un estado que pueda ser playtested.

Al terminar M6, *Restless* tiene una identidad clara, un loop que funciona y suficiente contenido para una sesión de 20-30 minutos.

---

## Tareas

### 1. Segundo aliado con incompatibilidad

Añadir un segundo aliado para que exista una elección de build real.

- [x] **El Héroe** — aumenta el tiempo máximo del sueño (+30s) pero incrementa la tasa de Inquietud (+10%). Riesgo/recompensa.
- [x] Assets completos (retrato sprite 16×24, sprite habitación, sprite Sueño).
- [x] Encuentro en el Sueño: Zona 3 — posición (9, -4).
- [x] Pasiva aplicada por `DreamPassiveApplier`: `dreamDurationBonus +30`, `restlessnessRateModifier +0.1`.
- [x] **Incompatibilidad**: `AllyData_hero.incompatibleWith` referencia `AllyData_sage`. `IncompatibilityChecker` activo con warning visual en PreDreamSelectionPanel.

### 2. Nivel del Sueño definitivo para el MOC

Reemplazar el nivel de prueba de M2 por el nivel "real" del MOC:

- [x] 3 zonas diferenciadas: Zona 1 segura (spawn, salida, sin restlessness extra), Zona 2 media (×1.5 Inquietud, SageEncounter, Entity_02), Zona 3 profunda (×2.0 Inquietud, HeroEncounter, Entity_01).
- [x] Cada zona tiene tileset visual propio (floor/wall base + floor_medium/floor_deep overlay).
- [x] Divisores de zona con colisionadores en x=-5 y x=+5, con paso central libre.
- [ ] La zona profunda solo es alcanzable en ~60% del tiempo disponible — obliga a decidir si seguir o volver. *(requiere tuning del timer con el nivel real)*
- [x] Punto de salida voluntaria en zona central (ExitPoint_Zone2 en -4,-8) y zona 1 (ExitPoint en -13,-8.5).
- [x] Fragmentos de memoria en cada zona: MemoryPoint_A (-10,-7), MemoryPoint_B (0,2), MemoryPoint_C (11,-3).

### 3. Efectos visuales de Inquietud pulidos

Revisar los efectos de M3 con el nivel definitivo:

- [x] Los umbrales de Inquietud producen efectos proporcionales y coherentes con el entorno.
- [x] El efecto Crítico no impide leer el entorno (jugabilidad sobre espectáculo). *(vignette cap 0.58, chromatic cap 0.65)*
- [ ] Testear que ningún efecto provoca fatiga visual tras 5 minutos de juego. *(requiere playtest)*

### 4. Audio completo del loop

- [x] Ambient del Sueño: drones sintéticos 4 capas (Calm/Tense/Critical/Overload), crossfade automático por umbral. `AmbientAudioPlayer.cs` en _Managers.
- [x] Infraestructura de música adaptativa: `RestlessnessAudioController.cs` — 4 snapshots asignados al mixer. Componente en _Managers de Dream scene.
- [x] SFX: zona de Inquietud (sweep ascendente), encuentro aliado (acorde mayor), despertar voluntario (tono descendente), despertar abrupto (burst ruido). `DreamSFXPlayer.cs` en _Managers, disparado desde `RestlessnessZone`, `WakeUpManager`, `AllyEncounter`.
- [x] Clips generados vía `Restless > Generate Placeholder Audio` (8 WAV sintéticos, sustituibles por assets finales).
- [x] Pasos del protagonista — `FootstepPlayer.cs` + 3 variantes WAV sintéticas.
- [x] Audio de la Vigilia: `VigiliaAudioPlayer.cs` — ambient loop, return tranquilo/abrupto, sleep SFX.

### 5. UI de la Vigilia con aliados

Actualizar la pantalla de habitación para que funcione con 2 aliados:

- [x] La habitación tiene espacio para el Sabio y el Héroe con sus posiciones asignadas.
- [x] La selección pre-sueño muestra los aliados disponibles, sus pasivas, y el warning de incompatibilidad.
- [x] Feedback visual claro cuando se intenta seleccionar dos aliados incompatibles. *(flash rojo 3× en warnings)*

### 6. Onboarding mínimo

El juego no tiene texto de historia, pero el jugador debe entender qué hacer:

- [x] Primera vez que se abre el juego: la habitación aparece sola, con el protagonista en cama. Un prompt sutil indica "Dormir" (`[E]` o botón A). *(icono pulsante en cama en run 0)*
- [x] Primera run: el `DreamTimer` empieza más lento y la primera zona de Inquietud tiene una señal visual clara. *(+60s bonus run 0; flash naranja/rojo en entrada de zona)*
- [x] El encuentro con el primer aliado pausa el timer y el panel es autoexplicativo.
- [x] Sin tutoriales de texto. Si algo no se entiende sin texto, rediseñar el elemento.

### 7. Sesión de playtest

- [x] Al menos 2 personas externas juegan una sesión completa sin ayuda. *(3 jugadores: 8, 8 y 13 años — 2026-05-08)*
- [x] Documentar: qué no entendieron, dónde murieron sin saber por qué, qué les pareció interesante.
- [x] Ajustes post-playtest: se trasladan a Sprint 01 (no bloquean el cierre del MOC).

**Feedback del playtest:**
- General positivo; la demo se completó sin instrucciones externas.
- Demo percibida como fácil — consecuencia directa de los puntos siguientes.
- El audio del enemigo no comunica claramente que está causando daño.
- La subida de inquietud y su efecto sobre el tiempo restante no son legibles en caliente.
- No queda claro que la urna se llena con los fragmentos y que al llenarse acaba la run.

Estos tres puntos son el backlog de UX de Sprint 01 (feedback y legibilidad), no fallos de mecánica.

---

## Criterios de salida de M6 (= Criterios del MOC)

- [x] El jugador puede iniciar el juego, jugar una run y volver a la Vigilia sin instrucciones externas.
- [x] El Medidor de Inquietud tiene consecuencias visuales y mecánicas reconocibles en los 4 umbrales.
- [x] Despertar tranquilo y abrupto se sienten y se ven distintos, con consecuencias diferentes en la Vigilia.
- [x] El Sabio y el Héroe existen como aliados, tienen pasivas funcionales y son incompatibles entre sí.
- [x] La elección de build (Sabio vs Héroe) cambia cómo se juega el nivel del Sueño de forma perceptible.
- [x] La paleta monocromática con acento amarillo está activa y es consistente en todo el juego.
- [x] El audio está presente en el loop completo (no hay silencio involuntario).
- [x] El playtest externo confirma que el loop se completa sin ayuda externa.
- [x] No hay crashes en las rutas principales documentadas.
