# M1 — Setup técnico del proyecto

**Estado:** ✅ Completado  
**Hito anterior:** [M0 — Definir el loop del MOC](M0-Definir-Loop-MOC.md)  
**Hito siguiente:** [M2 — Prototipo gris del loop](M2-Prototipo-Loop.md)

---

## Objetivo

Dejar el proyecto Unity en estado "listo para programar": estructura de carpetas, packages instalados, arquitectura base y escenas de referencia vacías. Nada de gameplay aquí — solo el andamiaje técnico sobre el que se construirán todos los hitos siguientes.

---

## Tareas

### Estructura de carpetas en Assets/

- [x] Crear jerarquía de carpetas en `Assets/_Project/` con subcarpetas para Art, Audio, Data, Prefabs, Scenes, Scripts e Input.

### Configuración URP

- [x] Verificar que el proyecto usa Universal Render Pipeline (URP 17.3.0 con 2D Renderer).
- [x] Post-processing activo.
- [x] Crear capas de sorting: `Background`, `Environment`, `Characters`, `Foreground`, `UI`.
- [x] Configurar Physics 2D según las necesidades del juego.

### Packages

- [x] **Input System** `1.18.0`
- [x] **Cinemachine** `3.1.4` — añadido a manifest.json
- [x] **DOTween** — importado desde Asset Store
- [x] **2D Tilemap** `1.0.0`
- [x] **Addressables** `2.4.5` — añadido a manifest.json

### Arquitectura base (Scripts/Core/)

- [x] `GameManager.cs` — singleton con estado global `Vigilia / Transitioning / Dream`.
- [x] `GameEvent.cs` + `GameEventFloat.cs` + `GameEventBool.cs` — canales de evento ScriptableObject.
- [x] `SceneLoader.cs` — carga/descarga de escenas vía Addressables.
- [x] `SaveData.cs` + `SaveManager.cs` — persistencia JSON en `Application.persistentDataPath`.

### Escenas base

- [x] `Bootstrap.unity` — escena de entrada.
- [x] `Vigil.unity` — hub de vigilia.
- [x] `Dream.unity` — escena del sueño.

### Input Actions

- [x] `PlayerInputActions.inputactions` — Action Maps `Player` (Move, Look, Interact, WakeUp, Run) y `UI` (Navigate, Submit, Cancel, Rotate). Bindings para teclado+ratón y gamepad.

---

## Criterios de salida de M1

- [x] Las tres escenas existen en el proyecto.
- [x] Los packages están instalados y el proyecto compila.
- [x] Los scripts Core existen en `Assets/_Project/Scripts/Core/`.
- [x] El Input Actions tiene los bindings de teclado y gamepad.
- [x] Sorting layers configuradas.
- [x] DOTween instalado y configurado.
