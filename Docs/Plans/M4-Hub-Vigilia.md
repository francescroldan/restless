# M4 — Hub de Vigilia completo

**Estado:** ⬜ Pendiente  
**Hito anterior:** [M3 — Identidad visual base](M3-Identidad-Visual.md)  
**Hito siguiente:** [M5 — Primer aliado end-to-end](M5-Primer-Aliado.md)

---

## Objetivo

Construir la pantalla de Vigilia como hub jugable y visualmente completo. El jugador debe poder llegar aquí después de cada run, ver el estado de su protagonista, tomar decisiones de preparación y lanzar la siguiente incursión al Sueño. Es el espacio de "respiración" del juego y debe sentirse cargado de atmósfera sin ser agobiante.

---

## Sistemas y assets a crear

### 1. Pantalla de la habitación

- [ ] Fondo pixel art de la habitación: cama central, ventana, elementos decadentes.
- [ ] Protagonista tumbado en la cama — animación `Awake` en bucle.
- [ ] Iluminación 2D ambiental: luz tenue de ventana, vela o lámpara.
- [ ] La habitación tiene al menos 2-3 elementos interactuables vacíos (preparados para aliados de M5).
- [ ] Animaciones ambientales: cortina que oscila, parpadeo de luz, polvo en suspensión.

### 2. UI de estado del protagonista

Sin HUD tradicional — los medidores se integran visualmente en la habitación:

- [ ] `MentalHealth` — representado visualmente (ej: estado del protagonista, grietas en la pared, espejo distorsionado).
- [ ] `PhysicalHealth` — representado visualmente (ej: postura en la cama, respiración).
- [ ] `DreamCapacity` — representado visualmente (ej: reloj de arena, frasco con líquido).
- [ ] Tooltip al pasar el cursor sobre cada elemento que muestra el valor numérico.
- [ ] `VigiliaDashboard.cs` — controller que actualiza todos los elementos visuales al entrar en la escena.

### 3. Pantalla de consecuencias post-run

Al llegar desde el Sueño, mostrar un resumen breve antes de entrar en la habitación:

- [ ] Tipo de despertar (tranquilo / abrupto).
- [ ] Cambios en el estado del protagonista (delta de stats).
- [ ] Fragmentos de memoria recogidos (si aplica según M0).
- [ ] Animación de entrada: pantalla de negro que se abre lentamente.
- [ ] `PostRunSummary.cs` — lee los datos del último run desde `SaveManager` y los muestra.

### 4. Selección pre-sueño (pantalla de preparación)

Al pulsar "Dormir", abrir un panel de preparación antes de la transición:

- [ ] Slot de **mascota** — vacío en M4, se llenará en M5+.
- [ ] Slots de **aliados** (2 slots) — vacíos en M4.
- [ ] Slot de **ritual** — vacío en M4.
- [ ] Slot de **droga/consumible** — vacío en M4.
- [ ] Botón "Confirmar y dormir" — lanza la transición al Sueño con la configuración seleccionada.
- [ ] Botón "Cancelar" — vuelve a la habitación.
- [ ] `PreDreamSelection.cs` — almacena la configuración elegida en un `DreamConfig` ScriptableObject que el Sueño lee al cargarse.

### 5. Persistencia y guardado

- [ ] Al entrar en la Vigilia, `SaveManager` escribe el estado actual.
- [ ] Al cerrar el juego y volver, el estado del protagonista y los fragmentos recogidos persisten.
- [ ] Sistema de "nuevo juego" que resetea el `ProtagonistState` y los desbloqueos.

### 6. Audio de la Vigilia

- [ ] Ambient loop de la habitación: lluvia exterior, madera crujiendo, silencio pesado.
- [ ] Sonido al interactuar con objetos.
- [ ] Música o drone ambiental bajo, opresivo.
- [ ] Transición de audio al entrar en el Sueño (fade out de ambient, fade in de música onírica).

---

## Criterios de salida de M4

- [ ] La pantalla de Vigilia se ve y se siente como el hub del juego, no como un menú.
- [ ] El estado del protagonista es legible visualmente sin leer números.
- [ ] El resumen post-run aparece correctamente tras un despertar tranquilo y uno abrupto.
- [ ] La pantalla de preparación abre, muestra los 4 slots (vacíos) y permite confirmar o cancelar.
- [ ] El estado persiste: cerrar y reabrir el juego mantiene los stats.
- [ ] El audio ambiental está activo y hace transición al entrar en el Sueño.
