# M4 — Vigilia Room

**Estado:** Completado  
**Objetivo:** Reemplazar el placeholder OnGUI de Vigilia por una escena 2D real con interacción cursor-driven, aliados como presencias, iluminación atmosférica y feedback visual de despertar.

---

## Referencia visual

- Cámara fija cenital (desde el pie de la cama, altura de techo).
- Protagonista siempre tumbado en cama, centrado en pantalla.
- Aliados como presencias alrededor, cada uno con su propia fuente de luz.
- Sin texto en pantalla — todo se comunica mediante iconos, iluminación y animaciones.
- Tonos desaturados y fríos; toques cálidos en las luces de los aliados.

---

## Aliados del MOC

| Aliado         | Arquetipo   | Posición      | Luz                   | Pasiva MOC                                      |
|----------------|-------------|---------------|-----------------------|-------------------------------------------------|
| Médico         | Doctor      | Izquierda     | Blanco cálido (lámpara)| Reduce penalización de despertar abrupto        |
| Ama de llaves  | Housekeeper | Abajo-izquierda| Ámbar tenue           | Aumenta duración base del sueño                 |
| Ocultista      | Occultist   | Arriba-derecha| Violeta/vela           | Desbloquea caminos (post-MOC)                   |
| Drogadicto     | Addict      | Abajo-derecha  | Verde enfermizo        | Aumenta riesgo y duración del sueño lúcido      |
| Mascota (gato) | Pet         | Encima cama   | Ninguna                | Compañía pasiva (TBD)                           |

En el MOC únicamente Médico y Ama de Llaves están disponibles para desbloquear.

---

## Arquitectura de scripts

```
Assets/_Project/Scripts/Vigil/
├── AllyData.cs              ScriptableObject — identidad, sprite, luz, modificadores
├── AllySlot.cs              Presencia de aliado en escena (sprite, Light2D, hover/click)
├── ProtagonistBed.cs        Interacción cama — breathing DOTween, hover icon, click → sueño
├── VigiliaRoomController.cs Orquestador — init desde SaveData, OnAllyClicked, RequestEnterDream
├── VigiliaTransitionFX.cs   Fade/flash OnGUI — entrada tranquila, entrada abrupta (flash rojo), salida dormir
└── AllyInfoPanel.cs         Panel Canvas deslizante — icono aliado, sin texto
```

---

## Setup de escena

### Jerarquía recomendada

```
Vigil [scene root]
├── _Managers
│   ├── VigiliaRoomController     ← ref: allySlots[], protagonistBed, allyInfoPanel, lights
│   ├── VigiliaTransitionFX
│   └── (SaveManager vive en Bootstrap, no aquí)
│
├── Room
│   ├── Floor                     Sprite: suelo habitación
│   ├── Walls                     Sprite: paredes
│   └── Furniture                 (cama, estantería, reloj, etc.)
│
├── Characters
│   ├── ProtagonistBed            SpriteRenderer + Collider2D + ProtagonistBed.cs
│   │   └── SleepIcon             SpriteRenderer (icono lunar/Z, centrado sobre protagonista)
│   ├── Ally_Doctor               SpriteRenderer + PolygonCollider2D + AllySlot.cs
│   │   ├── Light_Doctor          Light2D (Point)
│   │   └── HoverIndicator        SpriteRenderer (icono bolsa médica)
│   ├── Ally_Housekeeper          SpriteRenderer + PolygonCollider2D + AllySlot.cs
│   │   ├── Light_Housekeeper     Light2D (Point)
│   │   └── HoverIndicator        SpriteRenderer (icono tetera)
│   └── ... (Occultist, Addict, Pet — presentes aunque no desbloqueados)
│
├── Lighting
│   ├── GlobalLight               Light2D (Global) — intensidad 0.05–0.18 según salud mental
│   └── BedLight                  Light2D (Spot) — luz principal sobre la cama
│
└── UI [Canvas — Screen Space Overlay]
    └── AllyInfoPanel             RectTransform + AllyInfoPanel.cs
        └── Icon                  Image — icono del aliado seleccionado
```

### Cámara

- Orthographic, size ~5–6 según resolución objetivo.
- Sin CameraFollow — posición fija.
- Rotation (0, 0, 0) — vista cenital pura; la perspectiva la da el pixel art.

### Iluminación 2D (URP)

- **Global Light 2D**: intensity 0.05–0.18 (varía con salud mental), color gris frío.
- **Spot Light 2D** sobre la cama: intensity ~0.9, color blanco cálido, radio ~2.5.
- **Point Light 2D** por aliado: según `AllyData.lightColor/lightIntensity/lightRadius`.

---

## ScriptableObjects necesarios

Crear en `Assets/_Project/Data/Allies/`:

```
AllyData_Doctor.asset
AllyData_Housekeeper.asset
AllyData_Occultist.asset      (sin desbloquear en MOC)
AllyData_Addict.asset         (sin desbloquear en MOC)
AllyData_Pet.asset            (sin desbloquear en MOC)
```

Campos clave por aliado (Médico):
- id: `"doctor"`
- archetype: Doctor
- lightColor: (255, 220, 180) aprox.
- lightIntensity: 0.9
- lightRadius: 3.5
- dreamDurationBonus: 0
- restlessnessRateModifier: -0.15 (25% menos penalización)

---

## Criterios de aceptación (M4)

- [x] La escena Vigil carga sin errores y sin ningún OnGUI de VigiliaPlaceholder.
- [x] El protagonista en cama tiene animación de respiración sutil (DOTween scale Y).
- [x] Al hacer hover sobre el protagonista aparece un icono de "dormir"; al hacer click entra al sueño con fade a negro.
- [x] Los aliados desbloqueados (`unlockedAllyIds` en SaveData) aparecen en sus posiciones con su sprite y luz.
- [x] Los aliados no desbloqueados no son visibles ni interactuables.
- [x] Hover sobre un aliado: ligero scale up + indicador icono visible.
- [x] Click sobre un aliado: flash de luz del aliado + `AllyInfoPanel` desliza mostrando icono.
- [x] Entrada tras despertar abrupto: flash rojo breve antes del fade-in.
- [x] Entrada tras despertar tranquilo: fade-in suave desde negro.
- [x] La iluminación global varía según `ProtagonistState.mentalHealth` (más oscuro con menos salud).
- [x] `SaveManager.UnlockAlly("doctor")` desbloquea al médico y persiste entre sesiones.

---

## Fuera de scope en M4

- Arte pixel art final (se usarán placeholders de color sólido).
- Panel de upgrades del aliado (solo icono en M4).
- Mascota funcional.
- Rituales y drogas.
- Animaciones de entrada de aliados nuevos.
