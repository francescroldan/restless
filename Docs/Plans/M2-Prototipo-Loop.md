# M2 — Prototipo gris del loop core

**Estado:** ✅ Completado  
**Hito anterior:** [M1 — Setup técnico](M1-Setup-Tecnico.md)  
**Hito siguiente:** [M3 — Identidad visual base](M3-Identidad-Visual.md)

---

## Objetivo

Hacer jugable el loop completo **Vigilia → Sueño → Despertar → Vigilia** con mecánicas funcionales pero sin arte final. Todo con primitivas de Unity (quad negro, cículo de luz, colores planos). El objetivo es verificar que el loop se siente bien y seleccionar la variante de minijuego antes de invertir tiempo en gráficos.

Referencia de diseño: [Docs/GDD/MOC-Loop.md](../GDD/MOC-Loop.md)

---

## Sistemas a implementar

### 1. Movimiento top-down

- [x] `ProtagonistController.cs` — movimiento top-down con `Rigidbody2D`. Velocidad base configurable.
- [x] Dirección de movimiento con stick izquierdo o WASD.
- [x] La dirección de visión sigue la dirección de movimiento con lerp suave.
- [x] Apuntar con stick derecho o ratón sobreescribe la dirección de visión.

### 2. Cono de visión

- [x] `VisionCone.cs` — ángulo (~110°) y rango (~8 unidades) configurables. Implementado con `Light2D` (Point) en lugar de malla custom — suficiente para M2.
- [x] El cono usa sombras de `Shadow Caster 2D` en paredes (no raycasts radiales — queda para M3 si se necesita precisión).
- [x] Oscuridad fuera del cono mediante `GlobalLight2D` a intensidad 0.02.
- [x] Radio mínimo iluminado configurable (`_minVisibleRadius`).
- [x] `VisionConeVisual.cs` — overlay de fan mesh semi-transparente para visibilidad en desarrollo.

### 3. Medidor de Inquietud

- [x] `RestlessnessManager.cs` — valor `[0, 100]`, sube pasivamente con tasa base configurable.
- [x] Cinco umbrales: Low / Medium / High / Critical / Max.
- [x] Eventos `OnRestlessnessChanged` (GameEventFloat) y `OnMaxReached` (C# event).
- [x] `RestlessnessZone.cs` — trigger que modifica multiplicador de tasa al entrar/salir.
- [x] Durante minijuego activo: tasa ×2.5 configurable.

### 4. Timer del sueño

- [x] `DreamTimer.cs` — countdown configurable (300s por defecto en prototipo).
- [x] Se acelera ×2 cuando Inquietud ≥ High.
- [x] `OnExpired` → despertar abrupto vía `WakeUpManager`.
- [ ] Pausar timer durante minijuego si Inquietud es baja — **diferido a M3** (actualmente nunca se pausa).

### 5. Puntos de memoria

- [x] `MemoryPoint.cs` — objeto interactuable con marcador visual pulsante (`MemoryPointVisual`).
- [x] Estados: `Available` → `Extracting` → `Collected` / `Failed`. *(Estado `Undiscovered` diferido a M3.)*
- [x] Solo activable si el jugador orienta el cono hacia él y está en rango.
- [x] 4 puntos en el nivel de prueba.

### 6. Minijuego de extracción — las tres variantes

- [x] Las tres implementan `IExtractionMinigame`; cambiar variante = cambiar componente activo.

**Variante A — Timing**
- [x] `TimingMinigame.cs` — marcador oscilante, pulsa `E` en la zona verde.
- [x] Zona verde se estrecha y marcador acelera según Inquietud.
- [x] 3 aciertos completan / más de 2 fallos cancelan.

**Variante B — Reconstrucción**
- [x] `ReconstructionMinigame.cs` — navegar piezas con flechas y colocarlas con Enter.
- [x] Timer interno de 20s visible en HUD.

**Variante C — Retención**
- [x] `RetentionMinigame.cs` — mantener `E` para llenar barra de concentración.
- [x] Baja más rápido con Inquietud Alta/Crítica o entidad cercana.
- [x] 100% completa la extracción.

### 7. Inventario tetris

- [x] `DreamInventory.cs` — grid 4×5 configurable.
- [x] `MemoryFragment` ScriptableObject con forma en celdas relativas y rotación.
- [x] `InventoryPlacementUI.cs` — pausa el juego, flechas para mover cursor, `R` para rotar, `E` para colocar.
- [x] Escape descarta el fragmento.
- [x] 3 formas distintas: forma L, forma I, forma S.

### 8. Entidad — comportamiento básico

- [x] `DreamEntity.cs` — patrulla por waypoints con `Rigidbody2D`.
- [x] `EntityDetection.cs` — entidad en cono → spike de Inquietud continuo por segundo.
- [x] Entidad cercana interrumpe `RetentionMinigame` (drain de concentración).
- [ ] Cancelar todos los minijuegos al entrar en rango con spike +25 — **diferido a M3**.

### 9. Sistema de despertar

- [x] `WakeUpManager.cs` — diferencia voluntario (Escape) y abrupto (timer/Inquietud máxima).
- [x] Voluntario: fade a negro con texto *"despertando..."*, transición a Vigilia.
- [x] Abrupto: fade rojo con texto *"DESPERTAR ABRUPTO"*.
- [ ] Consecuencias en `ProtagonistState` (reducir salud/inventario) — **diferido a M4**.

### 10. Estado del protagonista y Vigilia placeholder

- [x] `ProtagonistState.cs` — ScriptableObject con `MentalHealth`, `PhysicalHealth`, tamaño de inventario.
- [x] Pantalla de Vigilia mínima: fondo negro, barras de stats, fragmentos recogidos, botón "DORMIR".
- [ ] Reducir inventario por despertar abrupto — **diferido a M4**.

---

## Nivel de prueba

- [x] Sala top-down con paredes y dos pilares interiores.
- [x] 4 puntos de memoria distribuidos (cerca, medio, lejos).
- [x] 2 entidades con rutas de patrulla distintas.
- [x] 1 `RestlessnessZone` segura en la esquina inferior izquierda (×0.3).
- [x] Punto de salida amarillo visible (Escape para despertar voluntario).

---

## HUD de debug (solo desarrollo)

- [x] Toggle con F1: Inquietud + umbral, timer, inventario, minijuego activo.
- [x] Panel siempre visible: checklist M2 con 11 ítems marcables desde el Inspector.
- [x] Panel siempre visible: leyenda de controles.

---

## Criterios de salida de M2

- [x] El protagonista se mueve en top-down y el cono de visión sigue su dirección con inercia.
- [x] El cono oscurece lo que queda fuera (Light2D + GlobalLight2D a 0.02).
- [x] Las tres variantes de minijuego son jugables y seleccionables sin cambiar código.
- [x] El inventario acepta fragmentos, muestra cuando está lleno, y permite rotar y colocar piezas.
- [x] Al menos una entidad sube la Inquietud al entrar en el cono.
- [x] Salida voluntaria y abrupta funcionan con transiciones visuales distintas.
- [x] Se puede jugar una run completa de principio a fin.
- [x] Decisión de variante de minijuego documentada.

---

## Decisión de variante de minijuego

**Variante elegida:** A — Timing  
**Motivo:** Las tres variantes están implementadas y son funcionales. La Variante A (marcador oscilante) resultó ser la más legible e inmediata durante el playtest de M2. Ofrece la tensión correcta sin requerir UI compleja. Se ajustó la velocidad del marcador (0.28 base → 0.9 a máxima inquietud) y la zona verde (0.16 → 0.08 en crítico).  
**Ajustes pendientes para M3+:** Reemplazar la barra OnGUI por UI visual con arte; añadir feedback sonoro en acierto/fallo; considerar si la Variante C (Retención) puede coexistir en puntos de memoria de alta dificultad.
