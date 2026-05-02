# M2 — Prototipo gris del loop core

**Estado:** ⬜ Pendiente  
**Hito anterior:** [M1 — Setup técnico](M1-Setup-Tecnico.md)  
**Hito siguiente:** [M3 — Identidad visual base](M3-Identidad-Visual.md)

---

## Objetivo

Hacer jugable el loop completo **Vigilia → Sueño → Despertar → Vigilia** con mecánicas funcionales pero sin arte final. Todo con primitivas de Unity (cubos, cápsulas, colores planos). El objetivo es verificar que el loop se siente bien antes de invertir tiempo en gráficos.

Los sistemas a implementar se refinarán en hitos posteriores, pero deben estar en un estado "funciona y se puede probar".

---

## Sistemas a implementar

### 1. Protagonista — Controller básico

- [ ] `ProtagonistController.cs` — movimiento lateral (o top-down según decisión de M0) con Input System.
- [ ] Física con `Rigidbody2D` + `CapsuleCollider2D`.
- [ ] Estados básicos: `Idle`, `Moving`, `Interacting`.
- [ ] Sin animaciones aún — solo el sprite placeholder se mueve.

### 2. Medidor de Inquietud

- [ ] `RestlessnessManager.cs` — valor float `[0, 100]`, sube con el tiempo y por triggers, baja con zonas seguras.
- [ ] Cuatro umbrales con efectos de juego (sin efectos visuales aún — solo valores): Bajo, Medio, Alto, Crítico.
- [ ] Eventos del EventBus: `OnRestlessnessChanged(float)`, `OnRestlessnessThresholdCrossed(Threshold)`, `OnRestlessnessMax`.
- [ ] `RestlessnessZone.cs` — trigger 2D que modifica la tasa de subida/bajada al entrar/salir.

### 3. Timer del sueño

- [ ] `DreamTimer.cs` — countdown configurable, se acelera cuando la Inquietud supera el umbral Alto.
- [ ] Evento `OnDreamTimerExpired` → dispara despertar abrupto.
- [ ] Pausa al interactuar con aliados (para que los encuentros no penalicen).

### 4. Sistema de despertar

- [ ] `WakeUpManager.cs` — diferencia entre despertar tranquilo (voluntario) y abrupto (timer/Inquietud máxima).
- [ ] Despertar tranquilo: guarda todo el progreso de la run, transición suave a Vigilia.
- [ ] Despertar abrupto: aplica una `WakeUpConsequence` (struct con penalizaciones al estado del protagonista), transición brusca.
- [ ] Input de despertar voluntario (acción `Sleep/WakeUp` del Input System).

### 5. Estado del protagonista

- [ ] `ProtagonistState.cs` — ScriptableObject con stats persistentes: `MentalHealth [0,100]`, `PhysicalHealth [0,100]`, `DreamCapacity [0,100]` (tiempo máximo de sueño).
- [ ] Los despertares abruptos reducen `MentalHealth`.
- [ ] `DreamCapacity` determina el tiempo inicial del `DreamTimer`.

### 6. Escena del Sueño — nivel de prueba

- [ ] Un nivel lineal de 3-5 "salas" con geometría de placeholder (Tilemaps con tileset de debug).
- [ ] Al menos 2 `RestlessnessZone` de tipo "hostil" y 1 de tipo "segura".
- [ ] Un punto de salida voluntaria (trigger con un objeto interactuable).
- [ ] Sin enemigos, sin aliados aún.

### 7. Pantalla de Vigilia — placeholder

- [ ] Cámara fija sobre la habitación (sprite o color plano).
- [ ] UI mínima: mostrar `MentalHealth`, `PhysicalHealth`, `DreamCapacity` como barras o texto.
- [ ] Botón "Dormir" que lanza la transición a la escena del Sueño.
- [ ] Mensaje post-despertar: tipo de despertar y consecuencias aplicadas.

### 8. Transiciones Vigilia ↔ Sueño

- [ ] `SceneLoader` lanza transición con fade negro al cargar/descargar escenas.
- [ ] El estado del protagonista persiste entre escenas vía `ProtagonistState` (ScriptableObject).

---

## HUD de debug (solo en desarrollo)

- [ ] Un overlay siempre visible en Play Mode que muestre:
  - Valor numérico de Inquietud y su umbral actual.
  - Tiempo restante del sueño.
  - Tipo de despertar si ocurre.
  - Estado del protagonista (stats).
- [ ] Toggle con tecla F1.

---

## Criterios de salida de M2

- [ ] Se puede jugar una run completa de principio a fin: Vigilia → Sueño → Despertar → Vigilia.
- [ ] La Inquietud sube al entrar en zonas hostiles y baja en zonas seguras.
- [ ] El timer se acelera cuando la Inquietud supera el umbral Alto.
- [ ] Despertar voluntario (pulsar acción) y abrupto (timer/Inquietud max) producen resultados distintos en el estado del protagonista.
- [ ] Después de 3 runs, el estado del protagonista refleja el historial (MentalHealth degradado si hubo despertares abruptos).
- [ ] No hay errores ni warnings en consola durante una run normal.
