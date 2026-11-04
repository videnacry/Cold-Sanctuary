# Personajes del mesocosmos (santuario): quién vive aquí y quién se fue a su microcosmos

## 2026-11-04: los famosos salen a sus microcosmos

Berón: «los personajes famosos, elon musk, sadhguru, y demás sácalos del mesocosmos y crea sus microcosmos respectivos».

**Salen del santuario** (rama `claude/edif-personajes`):

| Id | Quién | Qué hacía en el santuario |
|---|---|---|
| `Sadhguru` | Sadhguru (persona real) | Maestro de yoga: la tarima de Hatha, la meditación junto al linga |
| `ElonMusk` | Elon Musk (persona real) | Mecánica: vehículos y maquinaria |
| `Goku` | Goku (ficción, Dragon Ball) | Entrenador de artes marciales; «Artes marciales con Goku» en el horario libre |
| `BillGates` | Bill Gates (persona real) | Hospital: registros y vacunación |
| `WarrenBuffett` | Warren Buffett (persona real) | Las cuentas del santuario, desde el restaurante |
| `MacKenzieScott` | MacKenzie Scott (persona real) | Huerto: alimentar a los visitantes |
| `IbrahimTraore` | Ibrahim Traoré (persona real) | Coordinar la guardia |

Se quitaron de:
- la escena del santuario (`MakeCharacter`) y `PlaceStaff`;
- `WorkdayDirector.staff` y `SanctuaryDirector.HomeArea`;
- las casas del árbol (`ResidentialBuilder.Owners`);
- el horario libre (sale de `WorkdayDirector.staff`; la actividad de artes marciales cambió de maestro);
- la galería de retratos (el menú «los seis del mesocosmos» ya no existe);
- los turistas del restaurante (`RestaurantService.TouristBodies`, `RestaurantServiceGame`);
- las pruebas que los buscaban.

**Se conservan para sus microcosmos:**
- su aspecto en `CubeHair.For` (por el id);
- sus datos en `SampleSceneBuilder.MesoCharacters` (id, rótulo, ropa y stats).

Sus microcosmos (rama `claude/edif-famosos`): [microcosmos-famosos.md](microcosmos-famosos.md): Sadhguru en Velliangiri (1999), Elon Musk en Hawthorne (2008), Goku en una aldea de montaña (homenaje), Bill Gates en Albuquerque (1975), Warren Buffett en Omaha (1983), MacKenzie Scott en Prairie View (2020) e Ibrahim Traoré en Uagadugú (2024).

**Otras figuras famosas** buscadas en las listas de personal y en los constructores del santuario: ninguna más.
- Las estatuas de los pisos de yoga (Buda, Ganesha, Adiyogi, Patanjali) son decoración, no personal.
- Newton, Curie, Einstein, Bruce Lee y los demás personajes históricos ya viven en sus microcosmos.

## Reparto de papeles (2026-11-04)

| Papel | Antes | Ahora | Por qué |
|---|---|---|---|
| **Maestro de yoga** (Hatha y Ashtanga; medita junto al linga en su tiempo libre) | Sadhguru | **Andrea** (`WorkdayDirector.YogaTeacher`) | Fue el único que hizo yoga de verdad con Leo, que practicaba unas 20 h por semana (`character-andrea.md`). Sale en la tarima de Hatha tradicional; su área es la Sala de Yoga. |
| Segunda maestra (para dar las dos clases a la vez) | — | **Irosene** (`WorkdayDirector.SecondYogaTeacher`) | Corre, escala y bucea a sus 70 y tantos: el Ashtanga, dinámico, le va. Sigue en el textil. |
| Mecánica: vehículos y maquinaria | Elon Musk | **Ducristin** | Área: Taller. |
| Artes marciales (horario libre: «Artes marciales con Gohageneis») | Goku | **Gohageneis** | Ágil y atlético, y ya es de la Guardia. |
| Hospital: registros y vacunación | Bill Gates | **Maite** | Área: Hospital. |
| Cuentas del santuario, desde el restaurante | Warren Buffett | **Nesvitor** | El papel de persona mayor que se pedía para él. Área: Cocina. |
| Huerto: alimentar a los visitantes | MacKenzie Scott | **Alicita** | Área: Huerto (con Panterilia). |
| Guardia | Ibrahim Traoré (coordinaba) | **Josedrilo**, con Gohageneis | Patrulla: como Gohageneis, sin área fija. |

Amahuz, Karen, Shalva y Adaguchi siguen de personal general.

**Las clases de yoga** funcionan igual que antes:
- la de Hatha la guía quien tenga la misión «yog_guia» en la rotación;
- la de Ashtanga (14 h) la da el primero que llega.

Lo que pasa a Andrea es lo que era fijo de Sadhguru:
- la salida en la tarima y el área de Yoga;
- la meditación del tiempo libre;
- ser el maestro que eligen las pruebas (`YogaClassTest`, `YogaFloorsTest`, con Irosene de segunda maestra);
- la escena de prueba `Prueba_Yoga` y la misión virtual de la Sala de Yoga.

## La jornada y el horario libre (regla W9)

- **Personal del santuario** (`WorkdayDirector.staff`, 18 con el jugador):
  - Kushal, Linduarda, Lizmibet, Goluis, Panterilia, Gohageneis, Irosene, Tote (`JefeConstruccion`) y Andrea;
  - los nueve de las fotos de Berón: Nesvitor, Amahuz, Karen, Alicita, Ducristin, Josedrilo, Maite, Shalva y Adaguchi.
- Antes eran 16 sin el jugador; ahora, 17. La rotación reparte las misiones de todas las áreas entre todos: cada misión le toca a
  cada uno algo menos a menudo.
- **Horario libre**: el catálogo tiene 54 plazas a la vez, más «Descansar en casa» sin límite. Con 17 personas sobra sitio. Solo
  cambió el maestro de artes marciales (de Goku a Gohageneis).
- **Casas del árbol**: las 18 están ocupadas (10 del personal que queda y 8 de la familia y los amigos de Berón; Amahuz y Karen
  comparten una). Ya no queda casa libre para recién llegados: si llega alguien, hay que añadir casas.

## Historia: los seis del mesocosmos (2026-11-03, ya no están)

Berón (2026-11-03) pidió añadir al santuario a Elon Musk, Goku, Bill Gates, Warren Buffett, MacKenzie Scott e Ibrahim Traoré, con
un papel por su trabajo público y conocido, con respeto y sin caricatura ni juicio político. Se añadieron como el resto del
personal, con dos piezas de aspecto que siguen en `CubeHair`: el pelo en puntas (`Style.Spiky`) y la boina (`Look.Beret`). El
2026-11-04 salieron a sus microcosmos.

| Id | Aspecto (cubos) | Stats (fuerza · agilidad · resistencia · masa) |
|---|---|---|
| `ElonMusk` | 1,88 m, pelo castaño oscuro corto, camiseta negra, vaqueros oscuros | 1,05 · 0,95 · 1,0 · 1,1 |
| `Goku` | Pelo negro en grandes puntas; gi naranja con camiseta, cinturón y botas azules | 1,6 · 1,5 · 1,6 · 1,1 |
| `BillGates` | Gafas finas, pelo claro con canas, jersey azul, pantalón caqui | 0,95 · 0,95 · 1,0 · 1,0 |
| `WarrenBuffett` | Canoso, gafas de montura gruesa, traje gris con camisa blanca | 0,85 · 0,8 · 0,9 · 1,05 |
| `MacKenzieScott` | Media melena castaña, blusa azul claro, pantalón oscuro | 0,95 · 1,05 · 1,05 · 0,95 |
| `IbrahimTraore` | Piel oscura, barba corta, boina roja, uniforme de campaña verde oliva | 1,25 · 1,2 · 1,3 · 1,05 |
| `Sadhguru` | Turbante crema con franja granate, barba larga blanca, camiseta sin mangas verde agua, dhoti y chal blancos, guirnalda | 1,0 · 1,2 · 1,4 · 1,0 |
