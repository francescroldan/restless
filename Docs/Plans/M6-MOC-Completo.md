# M6 — MOC completo y jugable

**Estado:** ⬜ Pendiente  
**Hito anterior:** [M5 — Primer aliado end-to-end](M5-Primer-Aliado.md)  
**Hito siguiente:** — (Post-MOC: a definir)

---

## Objetivo

Cerrar el MOC: el juego es jugable de principio a fin por alguien externo al proyecto sin necesitar explicaciones. Este hito no añade sistemas nuevos — ensambla, pule y extiende lo construido en M1–M5 hasta alcanzar un estado que pueda ser playtested.

Al terminar M6, *Restless* tiene una identidad clara, un loop que funciona y suficiente contenido para una sesión de 20-30 minutos.

---

## Tareas

### 1. Segundo aliado con incompatibilidad

Añadir un segundo aliado para que exista una elección de build real.

- [ ] **El Héroe** — aumenta el tiempo máximo del sueño pero incrementa la tasa base de Inquietud. Riesgo/recompensa.
- [ ] Assets completos (retrato, sprite habitación, sprite Sueño, animación idle).
- [ ] Encuentro en el Sueño: zona diferente al Sabio, nivel de Inquietud medio para acceder.
- [ ] Pasiva: `HeroPassive.cs` — `DreamCapacity +25%`, tasa de Inquietud `+15%`.
- [ ] **Incompatibilidad**: el Héroe y el Sabio son incompatibles (no pueden estar activos a la vez). El `IncompatibilityChecker` lo bloquea en la pantalla de preparación con mensaje explicativo.

### 2. Nivel del Sueño definitivo para el MOC

Reemplazar el nivel de prueba de M2 por el nivel "real" del MOC:

- [ ] 3 zonas diferenciadas: zona de entrada (segura), zona central (Inquietud media), zona profunda (Inquietud alta + encuentro con aliado).
- [ ] Cada zona tiene tileset y ambientación propios.
- [ ] La zona profunda solo es alcanzable en ~60% del tiempo disponible — obliga a decidir si seguir o volver.
- [ ] Punto de salida voluntaria en la zona central y en la zona profunda.
- [ ] Fragmento de memoria (objeto narrativo) en la zona profunda: recompensa de riesgo.

### 3. Efectos visuales de Inquietud pulidos

Revisar los efectos de M3 con el nivel definitivo:

- [ ] Los umbrales de Inquietud producen efectos proporcionales y coherentes con el entorno.
- [ ] El efecto Crítico no impide leer el entorno (jugabilidad sobre espectáculo).
- [ ] Testear que ningún efecto provoca fatiga visual tras 5 minutos de juego.

### 4. Audio completo del loop

- [ ] Ambient del Sueño: dron oscuro, sonidos distantes imposibles.
- [ ] Música adaptativa: sube en intensidad con la Inquietud (via Audio Mixer snapshots).
- [ ] SFX: pasos del protagonista, activar zona de Inquietud, encuentro con aliado, despertar tranquilo, despertar abrupto.
- [ ] Audio de la Vigilia del hito M4 revisado y balanceado.

### 5. UI de la Vigilia con aliados

Actualizar la pantalla de habitación para que funcione con 2 aliados:

- [ ] La habitación tiene espacio para el Sabio y el Héroe con sus posiciones asignadas.
- [ ] La selección pre-sueño muestra los aliados disponibles, sus pasivas, y el warning de incompatibilidad.
- [ ] Feedback visual claro cuando se intenta seleccionar dos aliados incompatibles.

### 6. Onboarding mínimo

El juego no tiene texto de historia, pero el jugador debe entender qué hacer:

- [ ] Primera vez que se abre el juego: la habitación aparece sola, con el protagonista en cama. Un prompt sutil indica "Dormir" (`[E]` o botón A).
- [ ] Primera run: el `DreamTimer` empieza más lento y la primera zona de Inquietud tiene una señal visual clara.
- [ ] El encuentro con el primer aliado pausa el timer y el panel es autoexplicativo.
- [ ] Sin tutoriales de texto. Si algo no se entiende sin texto, rediseñar el elemento.

### 7. Sesión de playtest

- [ ] Al menos 2 personas externas juegan una sesión completa de 20-30 minutos sin ayuda.
- [ ] Documentar: qué no entendieron, dónde murieron sin saber por qué, qué les pareció interesante.
- [ ] Ajustes post-playtest: solo los que afectan a la comprensión básica del loop, no los que "estarían bien tener".

---

## Criterios de salida de M6 (= Criterios del MOC)

- [ ] El jugador puede iniciar el juego, jugar una run y volver a la Vigilia sin instrucciones externas.
- [ ] El Medidor de Inquietud tiene consecuencias visuales y mecánicas reconocibles en los 4 umbrales.
- [ ] Despertar tranquilo y abrupto se sienten y se ven distintos, con consecuencias diferentes en la Vigilia.
- [ ] El Sabio y el Héroe existen como aliados, tienen pasivas funcionales y son incompatibles entre sí.
- [ ] La elección de build (Sabio vs Héroe) cambia cómo se juega el nivel del Sueño de forma perceptible.
- [ ] La paleta monocromática con acento amarillo está activa y es consistente en todo el juego.
- [ ] El audio está presente en el loop completo (no hay silencio involuntario).
- [ ] El playtest externo confirma que el loop se entiende y engancha durante al menos 20 minutos.
- [ ] No hay crashes en las rutas principales documentadas.
