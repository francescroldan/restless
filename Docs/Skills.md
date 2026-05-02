# Skills instaladas — Restless

Skills de referencia disponibles para Claude Code en este proyecto. Instaladas en `.claude/skills/`.

---

## Diseño de juego y arte

### `sickn33-antigravity-awesome-skills-game-design`
Principios de diseño de juegos: estructura de GDD, balanceo de stats y economía, psicología del jugador (tipos de motivación, schedules de recompensa) y sistemas de progresión. Útil para revisar el core loop de Restless, el Medidor de Inquietud y el diseño de builds por aliados.

### `davila7-claude-code-templates-game-art`
Principios de arte para videojuegos: árbol de decisión de estilo visual, pipeline de assets 2D/3D, teoría del color, los 12 principios de animación aplicados a juegos, resoluciones por plataforma y convenciones de nombrado. Referencia para el estilo pixel art monocromático de Restless.

### `davila7-claude-code-templates-2d-games`
Guía de desarrollo 2D agnóstica de motor: sistemas de sprites y atlas, diseño de tilemaps por capas, formas de colisión, tipos de cámara (follow, look-ahead, room-based) y patrones de género (plataformas, top-down). Referencia general para la capa de gameplay 2D.

### `davila7-claude-code-templates-feature-design-assistant`
Asistente de diseño de features mediante diálogo estructurado. Recoge requisitos en fases (objetivos, capas técnicas, dependencias), propone enfoques alternativos, genera documento de diseño y lista de tareas de implementación. Usar al planificar sistemas nuevos de Restless.

### `a5c-ai-babysitter-director-ai`
Director AI para control de pacing y dificultad dinámica: monitoriza el estrés del jugador y dispara eventos en consecuencia. Directamente aplicable al sistema de inquietud y los tipos de despertar (tranquilo vs abrupto) de Restless.

### `0x0funky-agent-sprite-forge-generate2dmap`
Orquestador de mapas 2D: elige la estrategia más simple (imagen bakeada, mapa por capas con props, tilemap o híbrido) según las necesidades del juego. Gestiona colisiones, zonas de spawn/NPC y previsualizaciones aplanadas. Para los escenarios del Sueño.

---

## Unity — Fundamentos

### `unity-foundations`
Conceptos core de Unity 6: GameObjects, Components, Transforms, Scenes, Prefabs, ScriptableObjects y estructura de proyecto. Arquitectura entidad-componente, jerarquía de objetos, tags, layers y convenciones. Basado en Unity 6.3 LTS.

### `unity-scripting`
Scripting C# en Unity 6: MonoBehaviour, eventos de ciclo de vida (Awake, Start, Update, FixedUpdate), coroutines, async/await (Awaitable), ScriptableObjects, eventos, delegados y APIs core (Vector3, Quaternion, Time, Debug). Basado en Unity 6.3 LTS.

### `unity-lifecycle`
Patrones de corrección para orden de ejecución y ciclo de vida: errores comunes en inicialización, timing de destrucción, fake-null, componentes desactivados, init en editor vs runtime, DontDestroyOnLoad y destrucción asíncrona. Formato WHEN/WRONG/RIGHT/GOTCHA.

### `unity-async-patterns`
Patrones de corrección para async y coroutines: doble-await en Awaitable, tokens de cancelación, contexto de hilo tras BackgroundThreadAsync, errores silenciados en coroutines, WaitForEndOfFrame en batch mode y leaks de handles de Addressables. Formato WHEN/WRONG/RIGHT/GOTCHA.

### `unity-packages-services`
Guía de paquetes y servicios Unity 6: Package Manager, registros con scope, lista de paquetes esenciales y compatibilidad de versiones. Unity Gaming Services (Authentication, Cloud Save, Analytics, Leaderboards, Matchmaker). Basado en Unity 6.3 LTS.

---

## Unity — Sistemas de juego

### `unity-2d`
Desarrollo 2D en Unity 6: sprites, sprite atlas, SpriteRenderer, tilemaps, física 2D (Rigidbody2D, Collider2D), iluminación 2D, sorting layers y sorting groups. Basado en Unity 6.3 LTS.

### `unity-animation`
Sistema de animación Unity 6: Animator Controllers, state machines de animación, blend trees, Avatar system, humanoid rigs, root motion, animation events, Timeline y Cinemachine. Basado en Unity 6.3 LTS.

### `unity-audio`
Sistema de audio Unity 6: AudioSource, AudioClip, AudioListener, Audio Mixer, audio espacial 3D, grupos de mixer, snapshots y configuración de importación de audio. Basado en Unity 6.3 LTS.

### `unity-input`
Input System Unity 6: Input Actions, Action Maps, Control Schemes, componente PlayerInput, gamepad, teclado, ratón, touch y XR. Basado en Unity 6.3 LTS.

### `unity-input-correctness`
Patrones de corrección para el New Input System: errores comunes con lectura de acciones (triggered vs IsPressed vs WasPressedThisFrame), cambio de action maps, persistencia de rebindings, lifetime de InputValue, PassThrough vs Value, multiplayer local y auto-switching de control scheme. Formato WHEN/WRONG/RIGHT/GOTCHA.

### `unity-cinemachine`
Sistema de cámaras Cinemachine Unity 6: cámaras virtuales, blending, follow cameras, FreeLook, camera shake y cámaras controladas por estado. API de Cinemachine 3.x. Basado en Unity 6.3 LTS.

### `unity-lighting-vfx`
Iluminación y efectos visuales Unity 6: luces bakeadas/realtime/mixed, light probes, reflection probes, Adaptive Probe Volumes (APV), global illumination, Particle System, VFX Graph y post-processing. Basado en Unity 6.3 LTS.

### `unity-ui`
Desarrollo de UI en Unity 6: UI Toolkit (USS, UXML, UI Builder, data binding — recomendado para proyectos nuevos), uGUI/Canvas (UI legacy), IMGUI. Basado en Unity 6.3 LTS.

---

## Unity — Arquitectura y patrones

### `unity-game-architecture`
Patrones de decisión de arquitectura: Service Locator vs Singleton vs DI, Event Bus vs ScriptableObject channels, MonoBehaviour vs C# puro, composición de componentes y secuencias de bootstrap de managers. Formato WHEN/DECISION/SCAFFOLD/GOTCHA.

### `unity-game-loop`
Traducción de diseño a código para el game loop: scaffolding del core loop, ciclo de vida de sesión, arquitectura de condiciones de victoria/derrota, hooks de meta loop, dificultad ajustable y timing de encuentros. Formato DESIGN INTENT. Relevante para el ciclo Vigilia/Sueño de Restless.

### `unity-state-machines`
Arquitectura de sistemas de estado: FSM, FSM jerárquica, Behavior Trees, state machines con pila, decisiones Animator-vs-código y testing de máquinas de estado. Formato WHEN/DECISION/SCAFFOLD/GOTCHA. Clave para los estados mentales del protagonista.

### `unity-data-driven`
Arquitectura data-driven: jerarquías de configuración con ScriptableObjects, pipelines de datos JSON, workflows de handoff con diseñadores, versionado y migración de datos, atributos de Inspector para configuraciones autodocumentadas. Formato WHEN/DECISION/SCAFFOLD/GOTCHA.

### `unity-game-loop`
Traducción de diseño a código del game loop: scaffolding del core loop, ciclo de vida de sesión, arquitectura win/lose, hooks de meta loop, dificultad ajustable, pacing y timing de encuentros. Formato DESIGN INTENT. Basado en Unity 6.3 LTS.

### `unity-save-system`
Arquitectura de sistema de guardado: selección de formato de serialización, DTOs de datos de guardado, versionado y migración, sincronización en cloud, scope de PlayerPrefs, estrategias de auto-guardado y persistencia en mobile. Formato WHEN/DECISION/SCAFFOLD/GOTCHA. Para persistencia entre runs del roguelite.

### `unity-scene-assets`
Decisiones de arquitectura de escenas y assets: composición de escenas aditivas, Addressables vs Resources, workflow de AssetReference, coordinación del ciclo de vida de assets y pantallas de carga. Formato WHEN/DECISION/SCAFFOLD/GOTCHA.

### `unity-level-design`
Traducción de diseño de niveles a código: arquitectura de triggers y eventos, contratos de diseño de encuentros, checkpoint y respawn, secuencias cinemáticas y scriptadas, hooks de storytelling ambiental y seams de level streaming. Formato DESIGN INTENT. Relevante para los escenarios del Sueño.

### `unity-ui-patterns`
Patrones de UI/UX design-to-code: arquitectura de flujo de pantallas, separación View/ViewModel, arquitectura de HUD, sistemas de feedback y juice, vistas dinámicas de lista/grid y contratos de transición y animación. Solo UI Toolkit. Formato DESIGN INTENT.

---

## Unity — Sistemas avanzados

### `unity-ecs-dots`
ECS/DOTS en Unity 6: Entity Component System, diseño orientado a datos, Jobs system, Burst Compiler, Entities, IComponentData, ISystem, EntityManager, baking. Para sistemas con muchas entidades simultáneas. Basado en Unity 6.3 LTS.

### `unity-procedural-gen`
Generación procedural design-to-code en Unity: terreno y placement basados en ruido, sistemas de tiles y grids, generación de mazmorras/habitaciones, seeds y reproducibilidad, presupuesto de contenido y generación runtime vs bakeada. Formato DESIGN INTENT. Para los escenarios procedurales del Sueño.

### `unity-npc-behavior`
Comportamiento de NPCs design-to-code: arquitectura de sistema de percepción, patrones de capa de decisión, pipeline de ejecución de acciones, sistemas de facciones y relaciones, memoria y olvido de NPCs, coordinación de grupos. Formato DESIGN INTENT. Para los aliados y entidades del Sueño.

### `unity-ai-navigation`
Navegación AI en Unity 6: NavMesh, pathfinding, NavMeshAgent, NavMeshSurface, NavMeshObstacle, off-mesh links, Unity Sentis (inferencia de modelos ML), bakeado de NavMesh en runtime. Basado en Unity 6.3 LTS.

### `unity-performance`
Profiling y optimización en Unity 6: Profiler, Memory Profiler, Frame Debugger, optimización de frame rate, reducción de uso de memoria y resolución de cuellos de botella. Basado en Unity 6.3 LTS.

### `unity-editor-tools`
Herramientas de editor en Unity 6: custom inspectors, EditorWindows, PropertyDrawers, Gizmos, Handles, menu items, extensiones del editor, patrón SerializedObject/SerializedProperty, AssetDatabase y código exclusivo de editor con `#if UNITY_EDITOR`. Basado en Unity 6.3 LTS.

### `unity-testing`
Testing en Unity 6: Unity Test Framework, tests en Edit Mode y Play Mode, atributos NUnit ([Test], [UnityTest], [SetUp], [TearDown]), testing de MonoBehaviours y coroutines, integración con CI/CD. Basado en Unity 6.3 LTS.

---

## Arte y estilo visual

### `unity-shader-graph-artist`
Especialista en efectos visuales y materiales en Unity 6 URP: Shader Graph con Sub-Graphs obligatorios, conversión a HLSL optimizado con macros SRP-compatibles (`TEXTURE2D`, `CBUFFER_START`, `Core.hlsl`), custom render passes via `ScriptableRendererFeature`. Incluye implementaciones completas de: shader de dissolve (para apariciones del Yellow King), outline pass por detección de bordes (look pixel art), shader monocromático con acentos de color selectivos (el estilo visual central de Restless), y plantilla HLSL URP Lit. Presupuestos de rendimiento para PC y mobile. Creado a partir del skill `unity-shader-graph-artist` del marketplace, adaptado y ampliado.

### `pixel-art`
Maestría en pixel art basada en Pedro Medeiros (saint11): sin anti-aliasing automático (píxeles con bordes duros), hue shifting en paletas limitadas (sombras frías, luces cálidas), patrones de dithering (Bayer/checkerboard para atmósferas, Floyd-Steinberg para imágenes), animación subpíxel (movimiento por color, no por posición), selective outlining/selout (contornos sombreados según fuente de luz), constraints de hardware retro (NES/SNES/GBA) y técnicas HD-2D. Incluye tres ficheros de referencia: `references/patterns.md` (cómo construir), `references/sharp_edges.md` (fallos críticos y por qué ocurren), `references/validations.md` (reglas estrictas). Directamente aplicable a la paleta monocromática con acentos de color de Restless.

---

## Horror y narrativa

### `horror-game`
Blueprint experto para juegos de horror en Unity 6: modelo de pacing Sawtooth (buildup/peak/alivio), Director System (macro IA que controla el ritmo), IA sensorial (visión/sonido con raycasting en FixedUpdate), sistema de sanidad/estrés (camera shake, distorsión de audio, post-processing URP), fog volumétrico y IA de doble cerebro (Director omnisciente + Senses honestos). Incluye patrones C# completos para `HorrorDirector`, `SensoryComponent`, `SanityManager` y `MonsterAI`. Adaptado de Godot a Unity 6 URP. Directamente aplicable al Medidor de Inquietud, el Yellow King y los tipos de despertar de Restless.

---

## Sistemas de juego — Unity específico (adicionales)

### `roguelike`
Patrones roguelite/roguelike: dungeons procedurales, permadeath, meta-progresión entre runs, sistemas de loot, loop de habitaciones, gestión de pisos. Aplica globs a `**/Rogue*.cs`, `**/Dungeon*.cs`, `**/Loot*.cs`. Directamente aplicable al ciclo de runs de Restless.

### `procedural-generation`
Patrones de generación procedural en C# para Unity: ruido Perlin/Simplex, BSP para dungeons, random walk, loot tables con pesos, wave function collapse básico y reproducibilidad con seeds. Aplica globs a `**/Procedural*.cs`, `**/Generate*.cs`, `**/Noise*.cs`.

### `event-systems`
Patrones de sistemas de eventos en Unity: C# events, UnityEvent, ScriptableObject event channels, static EventBus. Cuándo usar cada uno, patrones zero-allocation y prevención de memory leaks. `alwaysApply: true` — se carga en todas las sesiones.

### `inventory-system`
Patrones de inventario, equipamiento y crafting: definiciones de items con ScriptableObject, inventario por slots, sistema de equipamiento, recetas de crafting y binding con UI. Aplicable al sistema de builds/aliados de Restless. Aplica globs a `**/Inventory*.cs`, `**/Item*.cs`, `**/Equipment*.cs`.

### `dotween`
Librería de tweening DOTween (Demigiant): composición de secuencias, ciclo de vida de tweens, easing, estrategias de kill. Incluye patrones para animaciones de UI, feedback de botones, transiciones de pantalla, punch/shake para game juice. **Crítico: siempre matar tweens en OnDestroy.** Aplica globs a `**/*Tween*.cs`, `**/*Animation*.cs`.

---

## Referencia rápida — ¿Qué skill usar para qué?

| Tarea | Skill |
|-------|-------|
| Definir o revisar el core loop del juego | `game-design`, `unity-game-loop` |
| Sistema de estados mentales (Vigilia/Sueño) | `unity-state-machines`, `unity-game-architecture` |
| Escenarios procedurales del Sueño | `unity-procedural-gen`, `0x0funky-agent-sprite-forge-generate2dmap` |
| Persistencia entre runs (aliados, progresión) | `unity-save-system`, `unity-data-driven` |
| Comportamiento del Yellow King y aliados | `horror-game`, `unity-npc-behavior`, `unity-ai-navigation`, `a5c-ai-babysitter-director-ai` |
| Sistema de sanidad / Medidor de Inquietud | `horror-game`, `unity-state-machines` |
| Tensión y pacing del Sueño | `horror-game`, `a5c-ai-babysitter-director-ai` |
| Estilo visual monocromático con acentos | `unity-shader-graph-artist`, `pixel-art`, `unity-lighting-vfx` |
| Shader de dissolve (apariciones/desapariciones) | `unity-shader-graph-artist` |
| Outline pass pixel art | `unity-shader-graph-artist` |
| Crear sprites y animaciones | `pixel-art`, `unity-animation`, `unity-2d` |
| Dithering y paletas limitadas | `pixel-art` |
| Cámara del juego | `unity-cinemachine`, `davila7-claude-code-templates-2d-games` |
| UI de Vigilia (habitación, selección previa al sueño) | `unity-ui`, `unity-ui-patterns` |
| Audio adaptativo y horror sonoro | `unity-audio`, `game-design` |
| Animación del protagonista y entidades | `unity-animation`, `unity-2d` |
| Input (teclado, gamepad) | `unity-input`, `unity-input-correctness` |
| Diseñar una feature nueva | `davila7-claude-code-templates-feature-design-assistant` |
| Optimizar rendimiento | `unity-performance`, `unity-ecs-dots` |
| Tests y CI | `unity-testing` |
| Sistema de runs roguelite | `roguelike`, `unity-procedural-gen`, `procedural-generation` |
| Sistema de aliados/builds | `inventory-system`, `unity-data-driven`, `unity-state-machines` |
| Animaciones de UI y juice | `dotween`, `unity-ui-patterns` |
| Comunicación entre sistemas | `event-systems`, `unity-game-architecture` |
