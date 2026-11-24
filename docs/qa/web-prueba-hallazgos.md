---
resumen: "Primera pasada local de probar-web.ps1 -Local sobre las 64 páginas (santuario + 63 microcosmos). Todas cargan; 9 OK y el resto FALLOS 1–4. Este documento separa los falsos positivos de la…"
temas: [microcosmos, pruebas, web]
estado: vigente
codigo: [Animal, WebPrueba, AreaFunctionSite, AreaFunctionStation, CubeInsect, FamilyGenerator, InicioSceneBuilder, MicroPiece]
---
# Hallazgos de la prueba en la página (rweb2, 2026-11-17)

Primera pasada local de `probar-web.ps1 -Local` sobre las 64 páginas (santuario + 63 microcosmos). Todas cargan; 9 OK y el resto
`FALLOS 1–4`. Este documento separa los **falsos positivos de la prueba** (ya corregidos en `WebPrueba`) de los **fallos reales** que quedan.

## Falsos positivos (corregidos en la prueba)
| Fallo | Escenas | Por qué era falso | Cambio en `WebPrueba` |
|---|---|---|---|
| Flora «sin suelo» | 18 (Atahualpa, BadBunny, Beethoven, Blackpink, Descartes, EloyAlfaro, GranLeyDeLaPaz, GrandesCanciones, Hazanas, JohnLennon, Langley, Lincoln, MichaelJackson, PedroSanchez, PublicEnemy, Shakira, U2, Wailers): de 24 a 270 plantas cada una | Son plantas **«(sobre mueble)»**: la regla 5 las pone en la meseta de un mueble gigante, sin collider debajo. Las pruebas del pueblo tampoco las cuentan como flotantes. | Las «(sobre mueble)» no se miran (se cuentan aparte). El rayo de «sin suelo» llega a 1000 m. |
| «Hay gente en la escena»: 0 | Cocina, Hospital, Mecánica, Textil, Sakshi (río), Ambrosio | La gente de los microcosmos de área y de las historias de la tribu son **mirmidones** (`DressMyrmidon`: insectos-humanos sin `Animal`), no gente del pueblo (`folk`). | Persona = del pueblo o mirmidón (`CubeInsect` sin `Animal` encima, fuera de la fauna, que no sea el jugador). La estatura se mide solo a los del pueblo: los mirmidones van en postura de centauro. |
| «Nadie con cuerpo humano» (46) y «hay gente» | Enfermería, Veterinaria | Están **retiradas** (`InicioSceneBuilder.Retired`): sus `.unity` viejos siguen en la carpeta. | El menú de la web las salta: 61 microcosmos. |
| Gente «enterrada» o «dentro» de una mesa | AlJuarismi («Copista … bajo Mesa de los traductores»), Sadhguru («Vecina … dentro de Mesa baja con cojines») | La gente se sienta a la mesa y se pone a su puesto. | Para la gente, los muebles y puestos (`AreaFunctionSite`, `AreaFunctionStation`, `MicroPiece`) no «entierran» ni «contienen». |
| Bichos «dentro» de muebles, hojas o plantas | Varias (mariquitas y hormigas sobre equipos: Ecógrafo, Rotavapor, Hormigonera…) | La esfera de la regla de la flora está 0,25 m sobre el pie: un bicho **posado encima** de un mueble la cruzaba. Los bichos sobre muebles y plantas están permitidos (las mesetas tienen su fauna). | Bichos y gente: «dentro» solo si el **centro de su cuerpo** (+5 cm de apoyo) está dentro de un collider. Posado encima, no. El detalle separa «hundidas en el suelo» de «en muebles o paredes». |
| «Cae» | santuario (4) | El rayo de 60 m no veía suelo bajo las aves que vuelan alto. | Cae = baja más de 2 m y no hay suelo en 1000 m. |

## Fallos reales
- **Santuario: 97 animales del plan con tope 30.** Era un error de la web, no de la prueba: `TrimFauna` corría al cargar, cuando los
  animales aún no tienen especie y antes de que `FamilyGenerator` genere familias.
  - **Corregido en `WebLigera`:** se repasa a los 2, 8 y 20 s, y usa el nombre del prefab si aún no hay especie.
- **Bichos hundidos en el suelo del piso** («dentro de Suelo_de_madera / Suelo_tecnico»): casi todos los de los microcosmos de pueblo.
  - Cuentas (hundidos de los mirados): AlJuarismi 11/16, BadBunny 15/19, Beethoven 17/21, BillGates 6/13, Bolívar 24/26,
    Descartes 18/24, Eratóstenes 25/28, GranLeyDeLaPaz 22/28, GrandesCanciones 17/18, Hazañas 13/19, Heródoto 23/23,
    JohnLennon 12/13, Langley 16/18, Lincoln 16/20, MacKenzieScott 9/18, MichaelJackson 16/17, Nietzsche 16/20, PedroSánchez 17/21,
    Pitágoras 19/23, PublicEnemy 10/14, Shakira 19/21, U2 9/14, Wailers 22/23.
  - **Causa probable:** la fauna del pueblo nace a la altura del suelo más bajo del área, y el pueblo (con su tierra) va levantado encima.
  - **Ya está el arreglo en el constructor:** `2c9f4d2` (peticiones-sonda) rechaza los sitios de fauna dentro de un collider. Vale al
    **reconstruir** esos microcosmos.
  - Las cuentas son las de la pasada vieja (esfera a 0,25 m); la nueva medida (centro del cuerpo) separará suelo y muebles.
- **Bichos dentro de muebles o paredes** (sin contar los posados encima: hay que repetir la pasada para las cifras nuevas):
  - Buda 9 (Paritorio 4), LeChambon 10 (Wall_Left), Pakal 8 (Wall_Back), IbrahimTraore 6 (Wall_Left), Chernóbil 5,
    PlantaAtómica 5, Ambrosio 5 (Camilla 3), Atahualpa 4 (Wall_Left), Cocina 4 (cuba de quesería), Curie 3, Egina 3, Galvarino 3,
    Hospital 3, IrenaSendler 3, Tesla 3, Jenner 2, Newton 2, Nightingale 2, Patterson 2, Tregua1914 2.
  - Los de las **paredes** del piso (`Wall_*`) son reales. También se corrigen al reconstruir con `2c9f4d2`.
- **Bichos «sin suelo»** (pocos):
  - Blackpink 3, Atahualpa 2, CheGuevara 2, Descartes 2, PedroSánchez 2, Shakira 2, Langley 1, Lincoln 1, PlantaAtómica 1.
  - Con el rayo a 1000 m, los posados en una meseta sin collider dejarán de contar. Si quedan, flotan de verdad.
- **Estatura:**
  - **Goku, 3,0 m:** un escarabajo pelotero de 1,75 m nominales (`Who(…, 1,75, "DungBeetle")`) que dibujado mide 3 m. Es un fallo de
    escala real.
  - **Cleopatra, «Cocinero del barrio Delta», 2,5 m:** probablemente el gorro de cocinero cuenta en su caja. Hay que mirarlo en la escena;
    si es el gorro, la prueba debería medir sin la ropa.
- **Tregua 1914:** 5 de 11 soldados «enterrados bajo el Talud» (9,0 m bajo 9,8). La caja del talud tapa la trinchera, o los soldados
  están dentro de él. Hay que revisarlo en la escena.
- **BillGates:** solo 8 de 35 se mueven o giran en 12 s (la prueba pide un 25 %). En el piso de Informática la gente está quieta en sus
  puestos. Hay que decidir si es real o si ese piso necesita otro umbral.

## Siguiente pasada
1. Reconstruir los microcosmos de pueblo con `2c9f4d2` (fauna fuera de colliders) cuando se fusione.
2. Rehacer la web con `claude/web-continua` (la prueba nueva, el tope del santuario y sin las escenas retiradas).
3. `probar-web.ps1 -Local` y actualizar este documento con las cifras nuevas.
