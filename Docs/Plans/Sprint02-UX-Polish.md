# Sprint 02 — UX Polish: feedback, HUD y tileset

**Estado:** 🔄 En curso  
**Fase:** Post-MOC  
**Prerequisito:** [Sprint 01 — RunConfig](Sprint01-RunConfig.md) ✅ cerrado 2026-05-10

---

## Objetivo

Cerrar el backlog de UX recogido en el playtest de M6 y resolver el problema visual del tileset del sueño. El resultado debe ser una build donde el feedback al jugador sea claro y los gráficos del sueño tengan el color correcto.

---

## Tareas

### U1 — Feedback visual al daño del protagonista

**Problema:** El audio del enemigo al detectar al jugador no comunica daño con suficiente claridad. El sonido ya está mejorado; falta respaldo visual.

**Solución:** Añadir una animación de reacción al protagonista cuando recibe daño/es detectado: el personaje se tapa la cara con las manos (animación corta, ~0.4 s). Se dispara desde el sistema de detección de entidades, compatible con el estado actual del animator.

**Criterios de salida:**
- [ ] Sprite/frames de la animación "asustado/se tapa la cara" creados
- [ ] Estado `Scared` añadido al `ProtagonistAnimator`
- [ ] `EntityDetection` (o el sistema de daño) dispara la transición al detectar al jugador
- [ ] La animación vuelve al estado de movimiento/idle correctamente al terminar
- [ ] No interfiere con el movimiento ni con otras transiciones del animator

---

### U2 — HUD del sueño: ojo animado para inquietud y tiempo

**Problema:** Las barras de inquietud y tiempo no se entienden en caliente. El concepto abstracto de "inquietud creciente" y "tiempo que se agota" no se lee de un vistazo.

**Solución:** Reemplazar (o complementar) las barras actuales con un ojo animado que comunique ambos estados de forma intuitiva:

- **Inquietud** → controla los movimientos del ojo cerrado:
  - Baja (0–40 %): ojo quieto, respiración suave, movimientos lentos ocasionales
  - Media (40–70 %): movimientos más frecuentes y marcados bajo el párpado
  - Alta (70–100 %): espasmos, temblores, movimientos rápidos e irregulares
- **Tiempo restante** → controla la apertura del párpado:
  - Mucho tiempo: ojo completamente cerrado
  - Tiempo medio: párpado entreabierto, parpadeos espaciados
  - Poco tiempo: ojo muy abierto, parpadeos rápidos, como si estuviera a punto de despertar

El ojo puede convivir con las barras de debug (F1) pero reemplaza las barras en el HUD de juego.

**Criterios de salida:**
- [ ] Sprites/frames del ojo animado creados (cerrado, entreabierto, abierto, parpadeo, espasmo)
- [ ] Componente `DreamEyeHUD` que lee `RestlessnessManager` y `DreamTimer`
- [ ] Animación procedural o por curvas que mapea % de inquietud → intensidad de movimiento
- [ ] Animación procedural o por curvas que mapea tiempo restante → apertura del párpado
- [ ] El ojo reemplaza las barras de inquietud y tiempo en el HUD de juego (las barras permanecen en el debug HUD F1)
- [ ] Funciona a 60 fps sin allocations en Update

---

### U3 — Urna: diseño mejorado y efecto visual de llenado

**Problema:** El contador de fragmentos/urna no se hace notar. El sonido ya funciona bien, pero visualmente la urna no comunica que se está llenando.

**Solución:** Dos partes independientes:
1. **Rediseño visual de la urna** en la escena Vigilia — sprite más legible y con personalidad, que invite a interactuar.
2. **Efecto de llenado** al añadir fragmentos: partículas o animación de "líquido que sube" dentro de la urna, sincronizado con el SFX `sfx_vigil_urn_fill` ya existente.

**Criterios de salida:**
- [ ] Nuevo sprite de urna diseñado e importado
- [ ] Efecto visual de llenado implementado (partículas, shader fill, o animación de frames)
- [ ] El efecto se dispara desde `MemoryUrnController` al recibir fragmentos
- [ ] La duración del efecto está alineada con la duración del clip `sfx_vigil_urn_fill`
- [ ] El nivel de llenado visible refleja el progreso real (fragmentos actuales / objetivo)

---

### V1 — Color del tileset del sueño

**Problema:** Los sprites del tileset (`dream_cliff.png`) tienen píxeles en sRGB ~`#1C00 1C`–`#590059` (brillo lineal ~10 %). En juego se ven negros incluso con luz intensa porque cualquier multiplicación por luz da valores cercanos a 0.

**Causa raíz:** El arte fue creado con una paleta demasiado oscura. No es un problema de luces ni de shader.

**Solución:** Aclarar el tileset en un editor externo (Aseprite, Photoshop o GIMP) para que el color dominante del suelo sea un violeta visible (~`#8B5CF6` o similar), manteniendo las variaciones de sombra/luz relativas. Alternativa rápida provisional: ajustar `Tilemap_Cliff.color` en el Inspector de Unity como multiplicador de tinte hasta que el arte esté rehecho.

**Criterios de salida:**
- [ ] Tileset exportado con colores visibles a brillo lineal ≥ 0.3 en el canal dominante
- [ ] En juego, dentro del cono de visión los tiles muestran el tono violeta correcto
- [ ] La iluminación del sueño (GlobalLight + VisionCone) está calibrada para el nuevo rango de brillo
- [ ] Los valores de luz resultantes documentados en `GameConfig` según las reglas del proyecto

---

## Orden sugerido

1. **V1** (desbloqueante visual — todo lo demás se ve mejor con el color correcto)
2. **U3** (menor scope, da feedback inmediato)
3. **U1** (depende de tener sprites nuevos)
4. **U2** (mayor scope, requiere diseño + programación procedural)

---

## Criterios de salida del sprint

- [ ] Los cuatro puntos anteriores cumplen sus criterios individuales
- [ ] La build es jugable de inicio a fin sin regressions en el loop principal
- [ ] El HUD del sueño no muestra barras de inquietud/tiempo al jugador (solo en debug F1)
