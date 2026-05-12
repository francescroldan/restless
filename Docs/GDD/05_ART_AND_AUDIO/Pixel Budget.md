# Pixel Budget — Restless

Estándar de referencia para todos los assets visuales del juego. Cualquier sprite nuevo debe respetar estas medidas antes de ser importado.

---

## Las dos escenas son mundos visuales distintos

| | Dream | Vigilia |
|---|---|---|
| Tipo de cámara | Ortográfica top-down, sigue al protagonista | Ortográfica fija, perspectiva elevada desde el pie de la cama |
| Campo visible | 20 × 11.25 tiles en movimiento | Habitación completa, encuadre estático |
| Fondo | Tilemap generado | Ilustración pintada en perspectiva (una sola imagen) |
| Personajes | Sprites sobre tilemap, grid de 16 px | Sprites colocados sobre el fondo en posiciones fijas |
| Resolución interna | 320 × 180 px | 320 × 180 px |
| Upscale | ×6 a 1080p, Point filter | ×6 a 1080p, Point filter |

La resolución interna es la misma en ambas escenas. Lo que cambia es la cámara, el fondo y las convenciones de los sprites.

---

## Dream

### Unidad base

**16 × 16 px = 1 × 1 unidades de mundo.** PPU global: 16. Todo lo demás se expresa en múltiplos de tile.

### Entorno

| Categoría | Tamaño px | Tiles | Mundo |
|---|---|---|---|
| Tile base (suelo, pared) | 16 × 16 | 1 × 1 | 1 × 1 u |
| Prop pequeño (objeto, símbolo) | 16 × 16 | 1 × 1 | 1 × 1 u |
| Prop mediano (mueble, altar) | 32 × 32 | 2 × 2 | 2 × 2 u |
| Prop grande (estatua, pilar alto) | 32 × 48 | 2 × 3 | 2 × 3 u |

### Personajes

| Categoría | Tamaño px | Tiles | Mundo | Notas |
|---|---|---|---|---|
| Protagonista | 32 × 32 | 2 × 2 | 2 × 2 u | Celda de animación; el dibujo interior puede ser menor |
| Aliado | 32 × 32 | 2 × 2 | 2 × 2 u | Misma celda que protagonista |
| Entidad pequeña | 16 × 32 | 1 × 2 | 1 × 2 u | Silueta angosta, más amenazante |
| Entidad grande | 32 × 48 | 2 × 3 | 2 × 3 u | Jefes / presencias |
| Entidad colosal | 64 × 64 | 4 × 4 | 4 × 4 u | Reservado para el Yellow King |

### Animaciones (Dream)

| Regla | Detalle |
|---|---|
| Celda fija | Nunca fuera del bounding box de la categoría |
| PPU | 16 siempre |
| Pivot | Centro-inferior `(0.5, 0)` para personajes; centro `(0.5, 0.5)` para tiles y props |
| Filter mode | Point |
| Compresión | None |
| FPS | 8–10 fps |

---

## Vigilia

La habitación se presenta como una **ilustración de perspectiva elevada fija** (vista desde el pie de la cama, ángulo desde el techo). El fondo no es un tilemap — es una imagen pintada. Los personajes son sprites 2D colocados sobre ella en posiciones predefinidas.

### Fondo

| Parámetro | Valor |
|---|---|
| Tipo | Sprite único (imagen pintada) |
| Tamaño px | A definir según la composición final; debe ser múltiplo de 320×180 |
| PPU | 16 |
| Perspectiva | Dibujada a mano en perspectiva elevada — no sigue la grid de tiles |

### Personajes

Los sprites de vigilia se dibujan con proporciones coherentes con la perspectiva del fondo, no con la grid top-down del Dream. El PPU sigue siendo 16 para mantener consistencia visual.

| Categoría | Tamaño px | Notas |
|---|---|---|
| Protagonista en cama | 48 × 24 | Tumbado, apaisado |
| Aliado de pie | 16 × 40 | Proporciones verticales, vistos ligeramente de lado |
| Icono de aliado (UI) | 16 × 16 | Grid de tile — encaja en slots de interfaz |

### Posiciones en escena

Los aliados ocupan posiciones fijas alrededor de la cama (no se mueven libremente). Cada posición se define una sola vez en el editor y no cambia. La interacción es por hover/click, sin texto en pantalla.

---

## Estado actual vs. estándar (deuda técnica)

| Asset | Estado actual | Estándar | Acción |
|---|---|---|---|
| `protagonist.png` | 13 × 16 px, PPU 16 | 32 × 32, PPU 16 | Reemplazar con arte final |
| `protagonist_walk.png` | celda 48 × 48, PPU 48 | 32 × 32, PPU 16 | Redibujar spritesheet |
| `protagonist_scared.png` | celda 48 × 48, PPU 48 | 32 × 32, PPU 16 | Redibujar spritesheet |
| `entity.png` | 16 × 16, PPU 16 | 16 × 32, PPU 16 | Ajustar altura |
| `Statue_Presence.png` | 32 × 54, PPU 16 | 32 × 48, PPU 16 | Aceptable, ajuste menor |
| Aliados vigilia | 24 × 40, PPU 16 | 16 × 40, PPU 16 | Ajuste menor de ancho |
| Fondo vigilia | Tilemap placeholder | Ilustración pintada en perspectiva | Pendiente de arte final |

La deuda técnica se resolverá progresivamente al sustituir placeholders por arte final.

---

## Checklist para nuevos assets

**Dream:**
- [ ] Tamaño en px corresponde a una categoría de la tabla Dream
- [ ] PPU = 16
- [ ] Filter mode = Point
- [ ] Compresión = None
- [ ] Pivot centro-inferior para personajes, centro para tiles y props
- [ ] Celda de animación fija aunque el dibujo no rellene todo el espacio

**Vigilia:**
- [ ] Fondo: sprite único, PPU 16, proporciones coherentes con perspectiva elevada
- [ ] Personajes: PPU 16, proporciones adaptadas a la perspectiva del fondo
- [ ] Filter mode = Point
- [ ] Compresión = None
