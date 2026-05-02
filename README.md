# Restless

Horror psicológico con elementos roguelite. Un anciano al borde del suicidio entra en un mundo onírico recurrente para encontrar a su hijo, atrapado bajo la influencia de una entidad conocida como el **Yellow King**.

El juego alterna entre dos estados interconectados: **Vigilia** (hub diurno) y **Sueño** (gameplay en el mundo onírico).

Desarrollado en **Unity 6** · Pixel art · Paleta monocromática con acentos de color selectivos.

---

## Pilares de diseño

| Pilar | Descripción |
|-------|-------------|
| Ciclo dual | El bucle Vigilia/Sueño genera tensión y gestión de recursos constante |
| Psicología jungiana | Los aliados son arquetipos: Héroe, Sombra, Cuidador, Sabio, Ánima, Místico |
| Estados mentales | Las condiciones del protagonista alteran la percepción y el gameplay en ambos mundos |
| Builds por aliados | Sin clases; los builds emergen de combinaciones e incompatibilidades entre aliados |
| Narrativa mínima | Storytelling ambiental, sin muros de texto explícito |
| Atmósfera lovecraftiana | Horror cósmico, geometrías imposibles, entidades incomprensibles |

---

## Estructura del proyecto

```
Assets/                  — Assets del juego (en desarrollo)
Docs/
├── Dirección narrativa.md
└── GDD/
    ├── 01_NARRATIVE/    — Narrativa, personajes, Songlines
    ├── 02_DREAM_MECHANICS/ — Mecánicas del Sueño, Medidor de Inquietud
    ├── 03_VIGILIA/      — Hub, estados del protagonista, preparación
    ├── 04_ALLIES_AND_BUILDS/ — Sistema de aliados y builds
    ├── 05_ART_AND_AUDIO/ — Dirección de arte y audio
    ├── 06_WORLD_BUILDING/ — Worldbuilding lovecraftiano
    └── _ARCHIVE_V1/     — Diseño V1 metroidvania (no activo)
Packages/                — Paquetes Unity (incluye com.unity.ai.assistant local)
ProjectSettings/         — Configuración del proyecto Unity
```

---

## Requisitos

- **Unity 6** (6000.0.x LTS o superior)
- **Universal Render Pipeline (URP)**
- Rider o Visual Studio 2022

---

## Estado

> MOC (Minimum Original Concept) en desarrollo — el loop de gameplay simplificado está siendo definido.
