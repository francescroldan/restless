# Paleta oficial — Restless

Monocromática con un único acento de color. Todo lo que pertenece al mundo ordinario es gris. El acento amarillo marca lo que tiene que ver con el hijo y el Yellow King.

---

## Colores base

| Nombre | Hex | RGB | Uso |
|--------|-----|-----|-----|
| Negro base | `#0A0A0A` | 10, 10, 10 | Fondo, siluetas, oscuridad total |
| Gris oscuro | `#2A2A2A` | 42, 42, 42 | Sombras de entorno, paredes profundas |
| Gris medio | `#5A5A5A` | 90, 90, 90 | Detalles de tiles, UI secundaria |
| Gris claro | `#B0B0B0` | 176, 176, 176 | Highlights del protagonista, bordes |
| Blanco suave | `#E8E8E8` | 232, 232, 232 | UI, textos, efectos de luz |

## Acento

| Nombre | Hex | RGB | Uso |
|--------|-----|-----|-----|
| Amarillo King | `#F5C542` | 245, 197, 66 | Fragmentos de memoria, Yellow King, elementos del hijo |

El shader monocromático desatura todo excepto los píxeles cuyo tono esté cerca del amarillo (`hue ≈ 0.13` en HSV). El rango de tolerancia es configurable.

---

## Reglas de uso

- **Protagonista** — gris claro sobre negro. Sin color propio.
- **Entidades** — gris oscuro, casi siluetas. Presencia, no detalle.
- **Memory Points** — amarillo King. Son lo único que brilla en el sueño.
- **UI de juego** — blanco suave sobre fondos casi negros. Sin color salvo el acento.
- **Zona segura** — gris medio con vignette suavizada. No usa el acento.
- **Zona peligrosa** — más oscura que el entorno neutro.

---

## Notas para arte final

- Pixel art, resolución base 320×180 upscaleada a pantalla completa (Point filter).
- Sin antialiasing en ningún sprite.
- El acento amarillo debe usarse con moderación — si está en todas partes pierde significado.
