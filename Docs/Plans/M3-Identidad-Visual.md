# M3 — Identidad visual base

**Estado:** ⬜ Pendiente  
**Hito anterior:** [M2 — Prototipo gris del loop](M2-Prototipo-Loop.md)  
**Hito siguiente:** [M4 — Hub de Vigilia](M4-Hub-Vigilia.md)

---

## Objetivo

Establecer el look & feel definitivo del juego: paleta monocromática, shaders característicos, sprites del protagonista y efectos visuales del Medidor de Inquietud. Al terminar este hito, una captura de pantalla del juego debe ser inconfundiblemente *Restless*.

No es necesario tener todos los sprites del juego — solo los suficientes para que el loop de M2 tenga aspecto final.

---

## Sistemas y assets a crear

### 1. Shader monocromático con acentos de color (URP)

- [ ] `MonochromeAccentFeature.cs` — `ScriptableRendererFeature` que desatura toda la escena excepto colores en un rango de tono definido.
- [ ] `MonochromeAccent.hlsl` — shader de post-proceso con conversión RGB→HSV y detección de acento.
- [ ] Parámetros configurables en Inspector: `AccentColor` (HDR), `HueRange`, intensidad de desaturación.
- [ ] Asignar al URP Renderer Data del proyecto.
- [ ] Validar que funciona en Editor y en Build.

**Acento principal del MOC:** amarillo (`#F5C542`) para elementos del Yellow King y fragmentos de memoria del hijo.

### 2. Efectos visuales por nivel de Inquietud

Conectados al `RestlessnessManager` vía EventBus:

- [ ] **Bajo** — ningún efecto adicional.
- [ ] **Medio** — leve chromatic aberration (post-process Volume con weight animado por DOTween).
- [ ] **Alto** — lens distortion + vignette intensificada. Los bordes de pantalla se oscurecen.
- [ ] **Crítico** — glitch shader o desplazamiento de píxeles. Sprites del entorno vibran levemente.
- [ ] **Transición de umbral** — screen flash breve al cruzar de un nivel al siguiente.

Implementar en `InquietudVisualFeedback.cs`, escuchando `OnRestlessnessThresholdCrossed`.

### 3. Sprites del protagonista

Sprite sheet 32×32 px, paleta monocromática (negros/grises), sin antialiasing:

- [ ] **Idle** — 4 frames, respiración sutil.
- [ ] **Walk** — 6 frames, andar encorvado.
- [ ] **Run** — 6 frames (si aplica según M0).
- [ ] **Interact** — 3 frames, gesto de mano extendida.
- [ ] **Awake (en cama)** — 2 frames para la pantalla de Vigilia, respiración.

Configuración de importación: Filter Mode `Point`, Compression `None`, PPU 16.

### 4. Tileset del Sueño — primer bioma

Un tileset de placeholder-final para el nivel de prueba de M2: arquitectura art déco degradada.

- [ ] Tile de suelo (2-3 variantes).
- [ ] Tile de pared (2-3 variantes con grietas).
- [ ] Tile de fondo (oscuro, con textura sutil).
- [ ] Elemento decorativo: ventana tapiada, puerta sellada.
- [ ] Sprite de "zona de inquietud" (niebla oscura o símbolo perturbador).
- [ ] Sprite de "zona segura" (luz tenue, símbolo de calma).

### 5. Efectos de transición Sueño / Vigilia

- [ ] **Ir a dormir** — fade a negro con pulso de luz blanca, como cerrar los ojos.
- [ ] **Despertar tranquilo** — fade suave de negro a blanco a la habitación.
- [ ] **Despertar abrupto** — flash blanco intenso, screen shake (Cinemachine noise), corte brusco.

Implementado en `TransitionFX.cs`, llamado desde `SceneLoader`.

### 6. Paleta oficial del proyecto

Documentar en `Docs/GDD/05_ART_AND_AUDIO/Paleta.md`:

| Nombre | Hex | Uso |
|--------|-----|-----|
| Negro base | `#0A0A0A` | Fondo, siluetas |
| Gris oscuro | `#2A2A2A` | Sombras de entorno |
| Gris medio | `#5A5A5A` | Detalles de tiles, UI secundaria |
| Gris claro | `#B0B0B0` | Highlights del protagonista |
| Blanco | `#E8E8E8` | UI, textos, efectos de luz |
| Amarillo King | `#F5C542` | Acento — Yellow King, fragmentos |

---

## Criterios de salida de M3

- [ ] Una captura del loop de M2 con los nuevos visuals es reconocible como *Restless*.
- [ ] El shader monocromático está activo en la escena del Sueño y desatura todo excepto el acento amarillo.
- [ ] Los 4 niveles de Inquietud tienen un efecto visual diferente y distinguible.
- [ ] El protagonista tiene sus 5 animaciones funcionando correctamente en la escena.
- [ ] El despertar abrupto y el tranquilo se sienten visualmente distintos.
- [ ] No hay artefactos visuales (píxeles borrosos, aliasing) en los sprites a resolución nativa.
