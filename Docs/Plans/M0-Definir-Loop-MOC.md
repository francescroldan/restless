# M0 — Definir el loop del MOC

**Estado:** ⬜ Pendiente  
**Hito anterior:** —  
**Hito siguiente:** [M1 — Setup técnico](M1-Setup-Tecnico.md)

---

## Objetivo

El GDD tiene todos los sistemas diseñados (Medidor de Inquietud, Vigilia, aliados, builds) pero el **loop de gameplay del Sueño no está definido** después de descartar la exploración metroidvania. Este hito cierra esa pregunta antes de escribir una sola línea de código.

La salida de M0 es un documento de diseño corto y preciso que describe qué hace el jugador dentro del Sueño en el MOC, de principio a fin, en una sola run.

---

## Preguntas que hay que responder

### 1. ¿Qué hace el jugador en el Sueño?

La exploración metroidvania está descartada. Las opciones más coherentes con el GDD son:

| Opción | Descripción | Pros | Contras |
|--------|-------------|------|---------|
| **Exploración lineal con bifurcaciones** | Rooms en cadena, cada una con un evento/decisión. El jugador avanza hasta despertar. | Simple de implementar, fácil de iterar. | Poco diferencial. |
| **Gestión de recursos en espacio acotado** | Una sala o zona pequeña donde el jugador busca "fragmentos de memoria" mientras gestiona la Inquietud. | La Inquietud es el loop, no el mapa. | Puede sentirse estático. |
| **Navegación táctica top-down** | Espacio pequeño top-down, el jugador decide qué explorar sabiendo que la Inquietud sube al moverse por zonas inestables. | Tensión espacial natural. | Requiere diseño de niveles. |
| **Loop de decisiones** | Pantallas de elección (tipo carta/evento) que van acumulando Inquietud. Sin movimiento libre. | Rapidísimo de prototipar. | Puede perder identidad de "juego de acción". |

### 2. ¿Cuánto dura una run del Sueño en el MOC?

Propuesta de referencia: **3-7 minutos** por run. Suficiente para que la Inquietud tenga tensión real pero sin que se vuelva un compromiso de tiempo.

### 3. ¿Qué representa el "progreso" dentro de una run?

Opciones:
- Fragmentos de memoria del hijo (coleccionables narrativos).
- Avance espacial (llegar más lejos que la run anterior).
- Encontrar a un aliado (desbloquearlo para la Vigilia).
- Una combinación de las anteriores.

### 4. ¿Qué hay en el Sueño del MOC además del Medidor de Inquietud?

El MOC debe tener el mínimo necesario para que el loop sea jugable y significativo. Propuesta:

- Una mecánica de movimiento básica (caminar, sin combate por ahora).
- 1-2 tipos de "zonas de inquietud" que aceleran el medidor.
- 1 evento de aliado por run (encuentro narrativo/mecánico).
- Salida voluntaria y colapso forzado como los dos finales posibles.

---

## Tareas de este hito

- [ ] Leer y sintetizar los documentos de GDD relevantes:
  - `02_DREAM_MECHANICS/Mecánicas del sueño.md`
  - `02_DREAM_MECHANICS/Tiempo limitado en el sueño.md`
  - `02_DREAM_MECHANICS/Sueño lúcido vs sueño profundo.md`
  - `03_VIGILIA/Selección previa al sueño (builds y preparación).md`
- [ ] Responder las 4 preguntas de arriba con una decisión clara.
- [ ] Escribir el **documento de diseño del loop MOC** (ver plantilla abajo).
- [ ] Definir los **criterios de aceptación de M2** (prototipo gris), para que quede claro qué es lo mínimo que debe funcionar.
- [ ] Revisar y dar el visto bueno al documento antes de pasar a M1.

---

## Documento de salida (plantilla)

Al terminar M0, crear `Docs/GDD/MOC-Loop.md` con esta estructura:

```markdown
# Loop del MOC — Diseño

## El loop en una frase
[Una frase que describa qué hace el jugador en una run completa]

## Una run completa, paso a paso
1. [Vigilia: qué hace el jugador antes de dormir]
2. [Transición a Sueño]
3. [Sueño: qué hace el jugador, cómo sube la Inquietud]
4. [Evento de aliado: cómo ocurre]
5. [Salida: cómo se desencadena el despertar tranquilo o abrupto]
6. [Vigilia: qué consecuencias tiene]

## Mecánicas en scope para el MOC
- [Lista de mecánicas que SÍ están en el MOC]

## Mecánicas fuera de scope (pospuestas)
- [Lista de mecánicas del GDD que NO van en el MOC]

## Criterios de aceptación del prototipo gris (M2)
- [ ] [Criterio concreto y verificable]
```

---

## Criterios de salida de M0

- [ ] El documento `Docs/GDD/MOC-Loop.md` existe y está completo.
- [ ] Las 4 preguntas tienen una respuesta única y clara (no "puede ser A o B").
- [ ] El scope del MOC está delimitado: hay una lista de mecánicas que SÍ van y una de mecánicas que NO van.
- [ ] Los criterios de aceptación de M2 están escritos y son verificables por alguien que no diseñó el juego.
