# M5 — Primer aliado end-to-end

**Estado:** ⬜ Pendiente  
**Hito anterior:** [M4 — Hub de Vigilia](M4-Hub-Vigilia.md)  
**Hito siguiente:** [M6 — MOC completo](M6-MOC-Completo.md)

---

## Objetivo

Implementar el sistema de aliados de principio a fin con **un aliado completo**: el jugador lo encuentra en el Sueño, aparece en la Vigilia, y su pasiva modifica la experiencia de la siguiente run. Este hito valida que la arquitectura del sistema de aliados es sólida antes de añadir más.

El aliado del MOC debe ser uno de los arquetipos junganianos. Propuesta: **el Sabio** (aporta estabilidad mental, reduce la tasa de subida de Inquietud).

---

## Sistemas a crear

### 1. Arquitectura de datos de aliados

- [ ] `AllyData.cs` — ScriptableObject con:
  - `id`, `displayName`, `archetype` (enum: Hero, Shadow, Caregiver, Sage, Anima, Mystic).
  - `portraitSprite`, `roomSprite` (representación en la habitación).
  - `passiveDescription` (texto para UI).
  - `incompatibleWith` (lista de `AllyData`, para M6).
- [ ] `AllyRegistry.cs` — ScriptableObject lista de todos los aliados del juego. Fuente única de verdad.
- [ ] `AllyRoster.cs` — componente en `ProtagonistState` que almacena los aliados desbloqueados en la run actual.

### 2. Encuentro con el aliado en el Sueño

- [ ] `AllyEncounter.cs` — objeto interactuable en la escena del Sueño. Al activarse:
  - Pausa el `DreamTimer`.
  - Muestra un panel con el retrato del aliado y una descripción corta (sin texto de historia, solo su pasiva).
  - Opciones: "Aceptar" o "Ignorar".
  - Si acepta, añade el aliado a `AllyRoster`.
  - Reanuda el `DreamTimer`.
- [ ] El encuentro ocurre una sola vez por aliado por partida.
- [ ] Sprite del Sabio en el Sueño: figura encapuchada, 32×32, con acento de color si está en zona segura.

### 3. Aliado en la Vigilia

- [ ] Al entrar en la Vigilia con el Sabio desbloqueado, aparece su sprite en la habitación (posición asignada en el fondo).
- [ ] El slot de aliado en la pantalla de preparación ahora muestra el Sabio como opción seleccionable.
- [ ] `RoomAllyPresence.cs` — instancia el sprite del aliado en su posición y activa su animación idle.

### 4. Pasiva del Sabio

- [ ] `SagePassive.cs` — reduce la tasa base de subida de Inquietud en un porcentaje configurable (ej: −30%).
- [ ] Se activa si el Sabio está en uno de los slots de aliado en `DreamConfig`.
- [ ] `DreamPassiveApplier.cs` — al cargar la escena del Sueño, lee `DreamConfig` y aplica todas las pasivas activas.
- [ ] El efecto es visible en el HUD de debug (la tasa de subida cambia).

### 5. Slot de aliado en la pantalla de preparación

- [ ] Los 2 slots de aliado en `PreDreamSelection` ahora son funcionales.
- [ ] Abrir un slot muestra una lista con los aliados desbloqueados.
- [ ] Seleccionar el Sabio lo asigna al slot y muestra su pasiva en el panel.
- [ ] Si el mismo aliado se intenta asignar a dos slots, el segundo se rechaza con feedback visual.

### 6. Fundamentos del sistema de incompatibilidades (scaffold)

No hace falta que funcione en M5, solo que la estructura esté preparada:

- [ ] `IncompatibilityChecker.cs` — método `AreCompatible(AllyData a, AllyData b)` que lee `incompatibleWith`. Devuelve `true`/`false`.
- [ ] `PreDreamSelection` llama a `IncompatibilityChecker` al asignar un aliado: si hay conflicto, muestra un warning visual pero permite continuar (se refinará en M6).

---

## Assets del Sabio

- [ ] Retrato (64×64 px) para el panel de encuentro.
- [ ] Sprite de habitación (32×32 o 48×48 px, sentado en una silla leyendo).
- [ ] Sprite en el Sueño (32×32 px, figura encapuchada de pie).
- [ ] Animación idle de habitación (4 frames, pasar página de libro).

---

## Criterios de salida de M5

- [ ] El jugador puede encontrar al Sabio en el Sueño, aceptarlo e ignorarlo.
- [ ] Si lo acepta, el Sabio aparece en la habitación en la siguiente Vigilia.
- [ ] El Sabio puede ser seleccionado en un slot de aliado en la pantalla de preparación.
- [ ] Con el Sabio activo, la Inquietud sube más lento (verificable en el HUD de debug).
- [ ] Sin el Sabio activo, la Inquietud sube a la tasa base.
- [ ] El Sabio persiste entre sesiones (cerrar y abrir el juego lo mantiene en la habitación).
- [ ] `IncompatibilityChecker` está implementado aunque no haya incompatibilidades reales aún.
