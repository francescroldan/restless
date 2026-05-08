# M5 — Primer aliado end-to-end

**Estado:** ✅ Completado  
**Hito anterior:** [M4 — Hub de Vigilia](M4-Hub-Vigilia.md)  
**Hito siguiente:** [M6 — MOC completo](M6-MOC-Completo.md)

---

## Objetivo

Implementar el sistema de aliados de principio a fin con **un aliado completo**: el jugador lo encuentra en el Sueño, aparece en la Vigilia, y su pasiva modifica la experiencia de la siguiente run. Este hito valida que la arquitectura del sistema de aliados es sólida antes de añadir más.

El aliado del MOC debe ser uno de los arquetipos junganianos. Propuesta: **el Sabio** (aporta estabilidad mental, reduce la tasa de subida de Inquietud).

---

## Sistemas a crear

### 1. Arquitectura de datos de aliados

- [x] `AllyData.cs` — ScriptableObject con id, displayName, archetype, portraitSprite, roomSprite, passiveDescription, incompatibleWith, restlessnessRateModifier
- [x] `AllyRegistry.cs` — ScriptableObject lista de todos los aliados del juego
- [x] `IncompatibilityChecker.cs` — `AreCompatible(AllyData a, AllyData b)` implementado
- [x] `SaveData.selectedAllyIds` — lista de hasta 2 IDs para el siguiente sueño
- [x] `SaveManager.SetSelectedAllies()` — helper para persistir selección
- [x] `AllyData_sage.asset` — ScriptableObject del Sabio (crear en Unity Editor)
- [x] `AllyRegistry.asset` — registrar al Sabio (crear en Unity Editor)

### 2. Encuentro con el aliado en el Sueño

- [x] `AllyEncounter.cs` — interactuable en Dream; pausa DreamTimer, muestra panel, acepta/ignora
- [x] `AllyEncounterPanel.cs` — overlay UI con retrato, pasiva, botones Aceptar/Ignorar
- [x] Objeto encuentro del Sabio colocado en Dream scene (requiere setup MCP)
- [x] `AllyEncounterPanel` Canvas wired en Dream scene (requiere setup MCP)

### 3. Aliado en la Vigilia

- [x] `RoomAllyPresence.cs` — muestra sprite del aliado si está desbloqueado en SaveData
- [x] Objeto de presencia del Sabio colocado en Vigilia scene (requiere setup MCP)

### 4. Pasiva del Sabio

- [x] `RestlessnessManager.SetPassiveMultiplier()` — aplicado en Update
- [x] `DreamPassiveApplier.cs` — lee selectedAllyIds, calcula multiplicador combinado
- [x] `DreamSceneBootstrap.cs` — llama a ApplyPassives() en Start
- [x] DreamPassiveApplier wired en Dream scene _Managers con AllyRegistry asignado (MCP)

### 5. Slot de aliado en la pantalla de preparación

- [x] `PreDreamSelectionPanel.cs` — 2 slots funcionales, cicla aliados desbloqueados, warning de incompatibilidad, Confirm guarda selección
- [x] `VigiliaRoomController.RequestEnterDream()` — muestra PreDreamSelectionPanel antes de entrar
- [x] PreDreamSelectionPanel Canvas wired en Vigilia scene (requiere setup MCP)

### 6. Fundamentos del sistema de incompatibilidades

- [x] `IncompatibilityChecker.AreCompatible()` implementado
- [x] `PreDreamSelectionPanel` llama a `IncompatibilityChecker` al asignar — muestra warning visual

---

## Assets del Sabio

- [x] Retrato / sprite de habitación (16×24 px pixel art, estilo placeholder)
- [x] Sprite en el Sueño (existente del M3)
- [ ] Animación idle (pospuesto a M6)

---

## Criterios de salida de M5

- [x] El jugador puede encontrar al Sabio en el Sueño, aceptarlo e ignorarlo
- [x] Si lo acepta, el Sabio aparece en la habitación en la siguiente Vigilia
- [x] El Sabio puede ser seleccionado en un slot de aliado en la pantalla de preparación
- [x] Con el Sabio activo, la Inquietud sube más lento (verificable en el HUD de debug)
- [x] Sin el Sabio activo, la Inquietud sube a la tasa base
- [x] El Sabio persiste entre sesiones
- [x] `IncompatibilityChecker` está implementado aunque no haya incompatibilidades reales aún
- [x] Sistema de 4 aliados placeholder (Sabio, Héroe, Sombra, Cuidador) con sprites y pasivas
- [x] PreDreamSelectionPanel: ciclo completo, slot vacío opcional, botón Volver, Escape
- [x] AllyInfoPanel: muestra sprite, nombre y pasiva al hacer clic en un aliado de la habitación
