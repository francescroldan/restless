# M2 — Prototipo gris del loop core

**Estado:** ⬜ Pendiente  
**Hito anterior:** [M1 — Setup técnico](M1-Setup-Tecnico.md)  
**Hito siguiente:** [M3 — Identidad visual base](M3-Identidad-Visual.md)

---

## Objetivo

Hacer jugable el loop completo **Vigilia → Sueño → Despertar → Vigilia** con mecánicas funcionales pero sin arte final. Todo con primitivas de Unity (quad negro, cículo de luz, colores planos). El objetivo es verificar que el loop se siente bien y seleccionar la variante de minijuego antes de invertir tiempo en gráficos.

Referencia de diseño: [Docs/GDD/MOC-Loop.md](../GDD/MOC-Loop.md)

---

## Sistemas a implementar

### 1. Movimiento top-down

- [ ] `ProtagonistController.cs` — movimiento top-down con `Rigidbody2D` en modo Kinematic. Velocidad base configurable (lenta, pesada — hombre mayor).
- [ ] Dirección de movimiento con stick izquierdo o WASD.
- [ ] La dirección de visión sigue la dirección de movimiento con un lerp suave (no gira instantáneo).
- [ ] Apuntar con stick derecho o ratón sobreescribe la dirección de visión independientemente del movimiento.

### 2. Cono de visión

- [ ] `VisionCone.cs` — genera una malla (Mesh) en forma de cono frente al protagonista. Ángulo (~110°) y rango (~8 unidades) configurables.
- [ ] La malla se recorta contra los colliders del entorno (paredes) usando raycasts radiales — el cono no atraviesa paredes.
- [ ] Todo lo que queda fuera del cono está cubierto por una capa de oscuridad opaca (sprite negro sobre el resto de la escena).
- [ ] Un radio mínimo siempre iluminado alrededor del protagonista (~1 unidad) para evitar que se quede completamente ciego.

### 3. Medidor de Inquietud

- [ ] `RestlessnessManager.cs` — valor `[0, 100]`, sube pasivamente con el tiempo (tasa base configurable).
- [ ] Cuatro umbrales: Bajo (0-25), Medio (25-50), Alto (50-75), Crítico (75-100).
- [ ] `OnRestlessnessChanged(float)`, `OnThresholdCrossed(Threshold)`, `OnRestlessnessMax` en EventBus.
- [ ] `RestlessnessZone.cs` — trigger que modifica multiplicador de tasa al entrar/salir.
- [ ] Durante el minijuego activo: tasa multiplicada por un factor configurable (ej: ×2.5).

### 4. Timer del sueño

- [ ] `DreamTimer.cs` — countdown configurable (~6 minutos base).
- [ ] Se acelera cuando Inquietud supera el umbral Alto.
- [ ] `OnDreamTimerExpired` → despertar abrupto.
- [ ] Pausado durante el minijuego solo si la Inquietud es baja (si es alta, el timer sigue — el peligro no espera).

### 5. Puntos de memoria

- [ ] `MemoryPoint.cs` — objeto interactuable. Visible como un marcador placeholder (círculo de color).
- [ ] Estado: `Undiscovered` → `Available` (jugador en rango) → `Extracting` (minijuego activo) → `Collected` / `Failed`.
- [ ] Solo activable si el jugador orienta el cono hacia él y está a rango de interacción.
- [ ] 3-5 puntos en el nivel de prueba, con tasas de Inquietud distintas según su posición.

### 6. Minijuego de extracción — las tres variantes

Implementar las tres. Cada una es un componente independiente que implementa `IExtractionMinigame`. El `MemoryPoint` llama a la interfaz — cambiar de variante es cambiar qué componente está activo.

**Variante A — Timing**
- [ ] `TimingMinigame.cs` — un marcador oscila en una barra. El jugador pulsa `Interact` cuando está en la zona verde.
- [ ] La zona verde se estrecha y el marcador oscila más rápido según el nivel de Inquietud actual.
- [ ] 3 pulsaciones correctas completan la extracción. 2 fallos la cancelan.

**Variante B — Reconstrucción**
- [ ] `ReconstructionMinigame.cs` — 4-6 piezas aparecen en posiciones aleatorias alrededor de su posición correcta.
- [ ] El jugador arrastra cada pieza a su hueco (snap cuando está cerca). UI de grid simple, colores planos.
- [ ] Timer interno visible (no el timer del sueño — uno propio más corto, ~20 segundos).

**Variante C — Retención**
- [ ] `RetentionMinigame.cs` — barra de concentración que sube mientras se mantiene `Interact` pulsado.
- [ ] La barra baja automáticamente, más rápido si hay una entidad cerca o la Inquietud es Alta/Crítica.
- [ ] Llegar al 100% completa la extracción.

### 7. Inventario tetris

- [ ] `DreamInventory.cs` — grid 2D configurable (ej: 4×5 celdas iniciales).
- [ ] `MemoryFragment` ScriptableObject con: forma (array de celdas relativas), tamaño visual, valor (placeholder).
- [ ] Al recoger un fragmento: abrir UI de inventario, el jugador coloca el fragmento en el grid (rotar con `R`, soltar con `Interact`).
- [ ] Si no hay hueco: el fragmento queda pendiente — el jugador debe descartar uno existente o salir sin él.
- [ ] 3 formas de fragmento distintas en el nivel de prueba: 1×1, 1×2, forma L.

### 8. Entidad — comportamiento básico

- [ ] `DreamEntity.cs` — merodea por el nivel con NavMesh2D o movimiento de waypoints simple.
- [ ] `EntityDetection.cs` — si la entidad entra en el cono de visión del protagonista: dispara `OnEntitySpotted`, Inquietud sube en pulso (+15).
- [ ] Si la entidad está a rango de interacción mientras el minijuego está activo: cancela el minijuego y genera un pulso de Inquietud mayor (+25).
- [ ] La entidad no "mata" — solo genera presión a través de la Inquietud.

### 9. Sistema de despertar

- [ ] `WakeUpManager.cs` — diferencia tranquilo (acción voluntaria) y abrupto (timer/Inquietud máxima).
- [ ] Tranquilo: guarda inventario actual, transición suave.
- [ ] Abrupto: pierde los fragmentos no guardados, aplica `WakeUpConsequence` al estado del protagonista, transición brusca.

### 10. Estado del protagonista y Vigilia placeholder

- [ ] `ProtagonistState.cs` — ScriptableObject con `MentalHealth`, `PhysicalHealth`, `InventorySize` (número de celdas del grid).
- [ ] Pantalla de Vigilia mínima: fondo negro, texto con stats actuales, botón "Dormir", listado de fragmentos recolectados.
- [ ] El `InventorySize` base es 20 celdas. Cada despertar abrupto lo reduce en 1 (hasta mínimo de 12).

---

## Nivel de prueba

- Espacio top-down simple, ~30×20 unidades, con paredes que cortan el cono de visión.
- 4 puntos de memoria (1 cerca — tasa baja, 2 en zona media — tasa normal, 1 al fondo — tasa alta).
- 2 entidades con rutas de patrulla simples.
- 1 `RestlessnessZone` de tipo seguro (zona de entrada).
- Punto de salida voluntaria marcado con un placeholder visible.

---

## HUD de debug (solo desarrollo)

- [ ] Toggle con F1:
  - Valor numérico de Inquietud y umbral actual.
  - Tiempo restante del sueño.
  - Qué variante de minijuego está activa.
  - Fragmentos recogidos y celdas de inventario usadas.

---

## Criterios de salida de M2

- [ ] El protagonista se mueve en top-down y el cono de visión sigue su dirección con inercia.
- [ ] El cono se recorta contra las paredes correctamente.
- [ ] Las tres variantes de minijuego son jugables y seleccionables sin cambiar código (solo componente activo).
- [ ] El inventario acepta fragmentos, muestra cuando está lleno, y permite rotar y colocar piezas.
- [ ] Al menos una entidad sube la Inquietud al entrar en el cono y cancela el minijuego al acercarse.
- [ ] Salida voluntaria y abrupta funcionan con consecuencias distintas en el estado del protagonista.
- [ ] Se puede jugar una run completa de principio a fin.
- [ ] Tras jugar las tres variantes, hay una decisión sobre cuál va al MOC (documentada en este fichero).

---

## Decisión de variante de minijuego

*(rellenar tras el playtest interno de M2)*

**Variante elegida:** —  
**Motivo:** —  
**Ajustes pendientes:** —
