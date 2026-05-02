# Mecánicas del sueño

## Descripción breve
El sueño es un espacio top-down oscuro que el protagonista explora con visión de cono, buscando fragmentos de memoria de su hijo. La tensión nace de dos presiones simultáneas: el Medidor de Inquietud y un inventario finito que obliga a decidir cuándo volver.

## Contexto
El loop metroidvania original fue descartado por complejidad. El loop actual está definido en `MOC-Loop.md` y se inspira en la estructura de Dredge: vas, recoges hasta no poder más, y vuelves antes de perderlo todo.

## Contenido original

# Mecánicas del sueño

El plano onírico representa el espacio donde ocurre la exploración, el riesgo y el avance activo. Es el núcleo jugable, donde el protagonista se enfrenta a un mundo cambiante, amenazante e inestable, generado por su propia mente alterada.

---

## 👁️ Vista y movimiento

El sueño usa perspectiva **top-down**. El protagonista no ve todo el espacio — percibe solo un **cono de visión frontal** (~110°) que se recorta contra las paredes. Todo lo que queda fuera del cono es oscuridad total.

El cono gira con la dirección de movimiento. El jugador puede orientarlo independientemente del movimiento con el stick derecho o el ratón.

**La tensión central del juego nace aquí:** extraer un fragmento de memoria requiere orientar el cono hacia él, lo que significa dar la espalda a la oscuridad donde están las entidades.

---

## 🧩 Puntos de memoria y minijuego de extracción

Distribuidos por el nivel hay **puntos de memoria** — localizaciones que contienen fragmentos del hijo (imágenes, objetos, voces). Para recoger un fragmento, el jugador activa el minijuego de extracción:

- El cono de visión se congela (el protagonista está concentrado).
- La Inquietud sube más deprisa durante el minijuego.
- Si se completa: el fragmento pasa al inventario.
- Si la Inquietud llega al máximo antes de completarlo: despertar abrupto.

Hay tres variantes de minijuego a explorar (ver `MOC-Loop.md`): timing, reconstrucción y retención.

---

## 🎒 Inventario (grid tetris)

El jugador puede cargar un número limitado de fragmentos. El inventario es un **grid finito** con slots de formas distintas — cada fragmento ocupa una forma específica y debe encajarse manualmente como en un puzzle de piezas.

Cuando el inventario se llena, el jugador debe elegir: salir con lo que tiene, o buscar un fragmento que quepa en el espacio restante.

El tamaño del grid depende del estado físico del protagonista y de los aliados activos.

---

## 👻 Entidades del sueño

Presencias que merodean por el nivel. No atacan directamente, pero generan presión a través de la Inquietud:

- Si una entidad entra en el cono de visión: pulso de Inquietud.
- Si una entidad se acerca mientras el minijuego está activo: lo interrumpe y genera un pulso mayor.
- El jugador puede evitarlas girando el cono, esperando o alejándose.

---

## 🔗 Principales mecánicas relacionadas

- [Sistema de estrés y despertar](Sistema%20de%20estr%C3%A9s%20y%20despertar%2022a50b6236c180a4906ae681a7ed8dca.md)
- [Medidor de inquietud (Restlessness)](Medidor%20de%20inquietud%20(Restlessness)%2022a50b6236c18012bb3af6f2726a7692.md)
- [Tiempo limitado en el sueño](Tiempo%20limitado%20en%20el%20sue%C3%B1o%2022950b6236c1809bb776d56084caa5ea.md)

---

## 🔄 Interacciones con la vigilia

Las acciones realizadas dentro del sueño afectan directamente al estado del protagonista en la realidad. Este vínculo es una de las bases del juego y se manifiesta de múltiples formas:

- **Tiempo excesivo en el sueño** → deterioro físico o mental en la vigilia.
- **Despertares abruptos** → consecuencias graves como traumas, fobias o enfermedades.
- **Elecciones dentro del sueño** (ayudar o ignorar a un aliado, completar rituales, usar sigilos…) → desbloquean personajes, habilidades o eventos en la vigilia.
- **Condiciones mentales del sueño** → pueden trasladarse como efectos persistentes al mundo real.
- **Objetos oníricos especiales** → pueden materializarse como mejoras en la habitación.

Dormir no es simplemente descansar: es una **herramienta de avance** y a la vez **una amenaza constante** para el cuerpo y la mente del protagonista.

[Aliados dentro del sueño](Aliados%20dentro%20del%20sue%C3%B1o%2022a50b6236c1808dbde5d5acc395998a.md)

---

## Observaciones
(Añadir notas y relaciones con otros documentos)
