# Loop del MOC — Diseño

**Estado:** Definido  
**Versión:** 1.0  

---

## El loop en una frase

El jugador se interna en el sueño, navega un espacio oscuro con visión limitada, extrae fragmentos de memoria de su hijo mediante un minijuego, y regresa antes de que la Inquietud lo destruya.

---

## Una run completa, paso a paso

1. **Vigilia — preparación:** El jugador selecciona aliados, ritual y droga desde la habitación. La selección determina capacidad del inventario, tasa de Inquietud y efectos especiales.

2. **Transición:** El protagonista se tumba. Fundido a negro. El sueño comienza.

3. **Sueño — navegación:** Vista top-down. El protagonista percibe solo un cono de visión frontal (~90-120°). Lo que queda fuera del cono existe pero no se ve. La Inquietud sube progresivamente; más deprisa en las zonas más profundas del nivel.

4. **Sueño — localización de memoria:** El jugador navega hasta encontrar un **punto de memoria** — una localización fija que emite una señal sutil (sonido, destello leve, anomalía en el suelo). Hay varios en el nivel; los más valiosos están en las zonas con mayor Inquietud.

5. **Sueño — minijuego de extracción:** Al interactuar con un punto, arranca el minijuego. El cono de visión desaparece o se congela (el protagonista está concentrado). La Inquietud sube más rápido durante el minijuego. Si el jugador lo completa, el fragmento pasa al inventario. Si la Inquietud llega al máximo antes de completarlo: despertar abrupto.

6. **Sueño — decisión de inventario:** El inventario es finito, con slots de formas distintas (tetris). Cuando se llena, el jugador debe decidir: salir con lo que tiene, o buscar un fragmento que encaje en el hueco disponible.

7. **Salida:** Voluntaria (el jugador activa la salida en cualquier momento) o abrupta (Inquietud máxima o timer agotado).

8. **Vigilia — consecuencias:** Los fragmentos recolectados se guardan permanentemente. Los despertares abruptos degradan el estado del protagonista. Los aliados modifican el estado post-run.

---

## El cono de visión

- El protagonista emite un cono de percepción frontal. Fuera del cono: oscuridad total.
- El cono gira con la dirección de movimiento (stick/ratón apunta, cuerpo sigue con inercia lenta).
- **Tensión central:** hacer el minijuego requiere orientar el cono hacia la memoria, dejando la espalda a la oscuridad.
- Algunos aliados o rituales pueden ampliar el ángulo del cono o añadir una pequeña zona de detección lateral.

---

## El inventario (estilo Dredge)

- Grid finito de slots con formas irregulares.
- Cada fragmento de memoria ocupa una forma distinta (1×1, 1×2, L, T…).
- El tamaño base del grid lo determina el estado físico del protagonista y los aliados activos.
- Fragmentos más valiosos (zona profunda) tienden a ser más grandes, más difíciles de encajar.
- Al llenarse el inventario, el juego lo indica visualmente. El jugador decide si sale o busca un fragmento que encaje en el espacio restante.

---

## Las entidades del sueño

- Presencias que merodean por el nivel. No atacan directamente en el MOC.
- Si una entidad entra en el cono de visión del jugador: la Inquietud sube en un pulso brusco.
- Si una entidad se acerca mientras el jugador hace el minijuego (cono congelado): interrumpe el minijuego y dispara un aumento de Inquietud.
- El jugador puede evitarlas moviéndose o esperando a que se alejen.
- En el MOC: 1-2 tipos de entidad, comportamiento simple (patrulla o deriva).

---

## El minijuego de extracción — tres variantes a prototipar

El MOC probará las tres y seleccionará la que mejor funcione antes de M6.

### Variante A — Timing
Un marcador oscila dentro de una barra. El jugador pulsa cuando el marcador está en la zona verde. Cuanto más inestable la memoria (más Inquietud), más oscila y más estrecha es la zona verde. Rápido, tenso, habilidad muscular.

### Variante B — Reconstrucción
La memoria aparece como una imagen fragmentada en piezas dispersas. El jugador las arrastra a su posición correcta. Más lento, contemplativo. La presión viene del tiempo (Inquietud sube mientras lo haces), no de la dificultad de ejecución. Emocionalmente más impactante.

### Variante C — Retención
El jugador mantiene pulsado un botón/tecla. Una barra de "concentración" sube mientras lo mantiene. Factores externos (Inquietud alta, entidades cerca) generan "interferencias" que la reducen. Físico, simple, acumula tensión sostenida.

**Criterio de selección:** la variante que en playtest produzca más momentos de "casi lo tenía" y decepción real al fallar. El minijuego debe hacer que perder un fragmento duela.

---

## Mecánicas en scope para el MOC

- Movimiento top-down con cono de visión
- Inquietud: 4 umbrales con efectos visuales, acelera el timer
- Timer del sueño
- Puntos de memoria con minijuego (1 variante de las 3 a seleccionar en M2)
- Inventario tipo tetris
- 1-2 tipos de entidad (patrulla simple)
- Despertar tranquilo y abrupto con consecuencias distintas
- 2 aliados (Sabio y Héroe) con incompatibilidad
- Vigilia: habitación, estado del protagonista, preparación pre-sueño

## Mecánicas fuera de scope (pospuestas al post-MOC)

- Sueño lúcido vs sueño profundo
- Rituales y drogas funcionales (solo slots vacíos en el MOC)
- Mascota funcional
- Generación procedural del nivel
- Múltiples biomas
- Puzles de entorno
- Combate
- Historia completa / fragmentos narrativos finales

---

## Criterios de aceptación del prototipo gris (M2)

- [ ] El protagonista se mueve en top-down y el cono de visión sigue su dirección.
- [ ] Hay al menos 3 puntos de memoria en el nivel de prueba.
- [ ] El minijuego (de la variante elegida) se puede completar y falla si la Inquietud llega a máximo.
- [ ] El inventario tetris acepta fragmentos, rechaza los que no encajan y comunica visualmente cuándo está lleno.
- [ ] Al menos una entidad merodea por el nivel y genera un pulso de Inquietud al entrar en el cono.
- [ ] Salida voluntaria y abrupta funcionan con consecuencias distintas.
- [ ] Se puede jugar una run completa de principio a fin.
