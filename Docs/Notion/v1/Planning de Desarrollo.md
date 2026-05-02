# Planning de Desarrollo

## Semana 1: Movimiento y control básico

**Objetivo:** Movimiento completo del personaje (idle, caminar, saltar, escalar, lanzar).

### Día 1 – Lunes
- [x] Crear el proyecto Unity 2D y configurar el renderer para pixel art (URP opcional).
- [x] Ajustar resolución del juego a algo como 320x180 (para simular 16:9 en pixel art).
- [x] Activar Pixel Perfect Camera.

### Día 2 – Martes
- [x] Crear sprite temporal de 32x32 px del personaje.
- [x] Configurar animator con estados: idle, walk, jump.
- [x] Programar el movimiento horizontal con Rigidbody2D o sistema personalizado.

### Día 3 – Miércoles
- [x] Programar el salto (con gravedad y fuerza ajustables).
- [x] Añadir animaciones de transición entre idle, walk y jump.
- [x] Hacer pruebas de salto en plataformas.

### Día 4 – Jueves
- [x] Programar detección de cuerdas/escaleras.
- [x] Programar subida/bajada por cuerda (movimiento vertical libre o por peldaños).
- [x] Añadir animación de trepar.

### Día 5 – Viernes (No necesario todavía)
- [ ] Programar lanzamiento de objeto (puede ser solo visual por ahora).
- [ ] Lanzamiento en arco hacia donde mira el personaje.
- [x] Revisar y testear que el personaje se mueve sin errores.

---

## Semana 2: Primer entorno jugable

**Objetivo:** Un pequeño nivel jugable (ciudad deformada) con colisiones y exploración.

### Día 6 – Lunes
- [x] Crear tileset básico para paredes, suelo y fondo.
- [x] Usar Tilemap de Unity para construir el primer nivel de prueba.

### Día 7 – Martes
- [x] Configurar colisiones de los tiles del Tilemap.
- [x] Añadir plataformas y saltos simples.
- [x] Ajustar físicas del personaje para que se sienta bien moverse.

### Día 8 – Miércoles
- [ ] Colocar algunos objetos decorativos o interactivos simples.
- [ ] Añadir límites al nivel (muros invisibles, respawn si cae).
- [ ] Implementar puntos de spawn para el jugador.

### Día 9 – Jueves
- [ ] Preparar transición inicial: pantalla negra → fade-in al nivel.
- [ ] Probar loop de entrada al nivel, caminar, escalar, saltar, lanzar.

### Día 10 – Viernes
- [ ] Corregir errores en físicas, colisiones o animaciones.
- [ ] Crear prefab de objetos interactivos para siguientes semanas (llaves, puertas, etc.).

---

## Semana 3: Mecánica de tiempo + puzzle simple

**Objetivo:** Temporizador y primera interacción con objetivo simple.

### Día 11 – Lunes
- [x] Programar temporizador visible en pantalla (tipo HUD).
- [x] El temporizador empieza al entrar al nivel.

### Día 12 – Martes
- [x] Programar evento de "muerte" si el tiempo llega a 0 (pantalla negra o animación).
- [x] Reiniciar el nivel o volver a la pantalla de vigilia (aunque esté vacía).

### Día 13 – Miércoles
- [ ] Implementar objeto recolectable (ej: llave).
- [ ] Al recogerlo, desaparece del nivel y se guarda en inventario temporal.

### Día 14 – Jueves
- [ ] Añadir puerta cerrada que solo se abre con la llave.
- [ ] Al abrir la puerta, mostrar que el nivel está "completado".

### Día 15 – Viernes
- [ ] Revisar el flujo completo: tiempo limitado + recoger + usar + completar.
- [ ] Añadir feedback visual/audio simple al recoger y usar objetos.

---

## Semana 4: Vigilia + conexión con el mundo de sueños

**Objetivo:** Hub estático, conexión con el nivel, y una mejora desbloqueable.

### Día 16 – Lunes
- [ ] Crear escena separada "vigilia" (habitación sucia, cama, fondo negro).
- [ ] Crear el sprite de un NPC y colocarlo en la escena.

### Día 17 – Martes
- [ ] Al completar el nivel, volver a la vigilia.
- [ ] El NPC aparece solo si se ha completado el objetivo (llave → puerta).

### Día 18 – Miércoles
- [ ] Al hacer clic en el NPC, mostrar UI simple con un botón de "mejora".
- [ ] Comprar o desbloquear la mejora.

### Día 19 – Jueves
- [ ] Guardar el estado del NPC/mejora al volver al sueño.
- [ ] Mostrar la mejora visualmente en la habitación (ej: objeto nuevo, detalle en el entorno).

### Día 20 – Viernes
- [ ] Repasar todo:
  - Movimiento ✔
  - Nivel ✔
  - Tiempo ✔
  - Puzzle ✔
  - Transición ✔
  - Vigilia funcional ✔
- [ ] Preparar un build de prueba o grabar un gameplay de test para evaluación.
