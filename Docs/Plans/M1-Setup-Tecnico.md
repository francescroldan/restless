# M1 — Setup técnico del proyecto

**Estado:** ⬜ Pendiente  
**Hito anterior:** [M0 — Definir el loop del MOC](M0-Definir-Loop-MOC.md)  
**Hito siguiente:** [M2 — Prototipo gris del loop](M2-Prototipo-Loop.md)

---

## Objetivo

Dejar el proyecto Unity en estado "listo para programar": estructura de carpetas, packages instalados, arquitectura base y escenas de referencia vacías. Nada de gameplay aquí — solo el andamiaje técnico sobre el que se construirán todos los hitos siguientes.

---

## Tareas

### Estructura de carpetas en Assets/

- [ ] Crear jerarquía de carpetas:
  ```
  Assets/
  ├── _Project/
  │   ├── Art/
  │   │   ├── Sprites/
  │   │   ├── Shaders/
  │   │   ├── Materials/
  │   │   └── UI/
  │   ├── Audio/
  │   │   ├── Music/
  │   │   └── SFX/
  │   ├── Data/
  │   │   └── ScriptableObjects/
  │   ├── Prefabs/
  │   │   ├── Characters/
  │   │   ├── Environment/
  │   │   └── UI/
  │   ├── Scenes/
  │   │   ├── Bootstrap.unity
  │   │   ├── Vigilia.unity
  │   │   └── Sueno.unity
  │   └── Scripts/
  │       ├── Core/
  │       ├── Dream/
  │       ├── Vigilia/
  │       ├── Allies/
  │       ├── UI/
  │       └── Shared/
  ```

### Configuración URP

- [ ] Verificar que el proyecto usa Universal Render Pipeline.
- [ ] Configurar el URP Asset: shadows habilitadas, 2D renderer para el gameplay, post-processing activo.
- [ ] Crear capas de sorting: `Background`, `Environment`, `Characters`, `Foreground`, `UI`.
- [ ] Configurar Physics 2D: gravity scale, collision matrix (solo las capas que se necesitan).

### Packages

Verificar que están instalados y en versión compatible con Unity 6:

- [ ] **Input System** — New Input System, acción "Player" con Move, Interact, Sleep.
- [ ] **Cinemachine** — para cámara del Sueño y shake por Inquietud.
- [ ] **DOTween** — tweening de UI y efectos visuales.
- [ ] **TextMeshPro** — textos de UI.
- [ ] **2D Tilemap** — para los escenarios del Sueño.
- [ ] **Addressables** — para gestión de assets entre escenas.

### Arquitectura base (Scripts/Core/)

- [ ] `GameManager.cs` — singleton que controla el estado global del juego (Vigilia / EnTransicion / Sueño).
- [ ] `EventBus.cs` — bus de eventos estático con ScriptableObject channels para comunicación entre sistemas sin acoplamiento directo.
- [ ] `SceneLoader.cs` — wrapper sobre Addressables para cargar/descargar escenas (Vigilia.unity ↔ Sueno.unity) con pantalla de transición.
- [ ] `SaveData.cs` + `SaveManager.cs` — estructura de datos persistentes entre runs (aliados desbloqueados, estado del protagonista). JSON serializado en `Application.persistentDataPath`.

### Escenas base

- [ ] `Bootstrap.unity` — escena de entrada que carga GameManager y lanza Vigilia.
- [ ] `Vigilia.unity` — escena vacía con cámara y canvas de UI.
- [ ] `Sueno.unity` — escena vacía con cámara, Tilemap vacío y placeholder del protagonista.

### Input Actions

- [ ] Crear `PlayerInputActions.inputactions` con:
  - Action Map `Player`: Move (Vector2), Interact (Button), Sleep/WakeUp (Button).
  - Bindings para teclado (WASD + E + Space) y gamepad (stick + South + Start).

---

## Criterios de salida de M1

- [ ] Las tres escenas existen y cargan sin errores en la consola.
- [ ] `GameManager` transita entre estados `Vigilia` y `Sueno` al llamar a sus métodos (verificable desde el Inspector en Play Mode).
- [ ] El `EventBus` tiene al menos un canal de test que funciona entre dos MonoBehaviours en escenas distintas.
- [ ] El `SaveManager` escribe y lee un fichero JSON en `persistentDataPath` sin errores.
- [ ] El Input System reconoce Move e Interact en teclado y gamepad (testeable con el Input Debugger).
- [ ] No hay warnings de compilación en el proyecto.
