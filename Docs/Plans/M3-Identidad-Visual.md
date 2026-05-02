# M3 — Identidad visual base

**Estado:** 🔄 En progreso (código completo, arte diferido)  
**Hito anterior:** [M2 — Prototipo gris del loop](M2-Prototipo-Loop.md)  
**Hito siguiente:** [M4 — Hub de Vigilia](M4-Hub-Vigilia.md)

---

## Objetivo

Establecer el look & feel definitivo del juego: paleta monocromática, shaders característicos, sprites del protagonista y efectos visuales del Medidor de Inquietud. Al terminar este hito, una captura de pantalla del juego debe ser inconfundiblemente *Restless*.

No es necesario tener todos los sprites del juego — solo los suficientes para que el loop de M2 tenga aspecto final.

---

## Sistemas y assets a crear

### 1. Shader monocromático con acentos de color (URP)

- [x] `MonochromeAccentFeature.cs` — `ScriptableRendererFeature` que desatura toda la escena excepto colores en un rango de tono definido.
- [x] `MonochromeAccent.shader` — shader de post-proceso con conversión RGB→HSV y detección de acento.
- [x] Parámetros configurables en Inspector: `AccentColor` (HDR), `HueRange`, `DesatStrength`, `AccentBoost`.
- [x] Asignado al URP Renderer Data (`Assets/Settings/Renderer2D.asset`).
- [ ] Validar que funciona en Build. *(diferido a builds finales)*

**Acento principal del MOC:** amarillo (`#F5C542`) para elementos del Yellow King y fragmentos de memoria del hijo.

### 2. Efectos visuales por nivel de Inquietud

- [x] `RestlessnessVisualFX.cs` — Volume global con Vignette, ChromaticAberration y LensDistortion animados por `RestlessnessManager.NormalizedValue`.
- [x] **Bajo** — vignette leve (0.25), sin chromatic ni distorsión.
- [x] **Medio** — chromatic aberration aparece gradualmente.
- [x] **Alto** — lens distortion + vignette intensificada.
- [x] **Crítico** — chromatic 0.85, distorsión -0.35, vignette 0.62.
- [x] **Transición de umbral** — screen flash breve al cruzar de un nivel al siguiente.

### 3. Sprites del protagonista

*(Diferido — se usarán placeholders hasta tener una maqueta jugable)*

- [ ] **Idle** — 4 frames, respiración sutil.
- [ ] **Walk** — 6 frames, andar encorvado.
- [ ] **Interact** — 3 frames, gesto de mano extendida.
- [ ] **Awake (en cama)** — 2 frames para la pantalla de Vigilia.

### 4. Tileset del Sueño — primer bioma

*(Diferido — placeholders de Unity primitivas)*

- [ ] Tile de suelo (2-3 variantes).
- [ ] Tile de pared (2-3 variantes con grietas).
- [ ] Elemento decorativo: ventana tapiada, puerta sellada.
- [ ] Sprite de zona de inquietud / zona segura.

### 5. Efectos de transición Sueño / Vigilia

- [x] `TransitionFX.cs` — singleton con corrutinas para los tres tipos de transición.
- [x] **Ir a dormir** — pulso de luz blanca + fade a negro.
- [x] **Despertar tranquilo** — fade suave a negro.
- [x] **Despertar abrupto** — flash rojo + screen shake + fade rápido a negro.
- [x] `WakeUpManager` actualizado para delegar en `TransitionFX`.

### 6. Paleta oficial del proyecto

- [x] Documentada en `Docs/GDD/05_ART_AND_AUDIO/Paleta.md`.

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

- [ ] Una captura del loop con los nuevos visuals es reconocible como *Restless*. *(pendiente arte)*
- [x] El shader monocromático está activo en la escena del Sueño y desatura todo excepto el acento amarillo.
- [x] Los 4 niveles de Inquietud tienen un efecto visual diferente y distinguible.
- [ ] El protagonista tiene sus animaciones funcionando. *(diferido — placeholder)*
- [x] El despertar abrupto y el tranquilo se sienten visualmente distintos.
- [ ] No hay artefactos visuales en los sprites a resolución nativa. *(diferido — sin sprites finales)*

---

## Decisión de arte

Los sprites y tilesets se mantienen como primitivas de Unity. El arte final se abordará cuando el loop sea jugable y esté validado desde diseño. Los sistemas de código (shader, efectos de Inquietud, transiciones) están implementados y listos para recibir los assets cuando lleguen.
