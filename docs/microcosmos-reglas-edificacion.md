# Reglas de edificación de los microcosmos y mesocosmos (2026-11-01)

Berón: «en el microcosmos de Bruce Lee la cocina y los muebles parecían cubos». Desde ahora todo edificio de un microcosmos o
mesocosmos (los de `CivicBuilder`, los pueblos nuevos de las figuras históricas y las historias que traen sus propias
estructuras) cumple estas **siete reglas** (la sexta, el hogar de cada personaje, y la séptima, muebles a la medida de quien los usa y en el estilo de su época y lugar, desde 2026-11-02). El código que las aplica y las comprueba:

| Pieza | Dónde | Qué hace |
|---|---|---|
| `MicroBuildRules` | `Assets/Scripts/Microcosmos/MicroBuildRules.cs` | Las constantes (paso, calles, alturas, era avanzada) y `Validate`. |
| `MicroPiece` | `Assets/Scripts/Microcosmos/MicroPiece.cs` | Marca la raíz de un mueble de varias piezas: `kind`, `need`, `walkable`. |
| `MicroFurniture` | `Assets/Editor/MicroFurniture.cs` | Los muebles detallados (cocina con sus utensilios, mesa con sillas, cama, retrete, lavabo…), reutilizando `KitchenProps`. |
| `CivicBuilder.Building` | `Assets/Editor/CivicBuilder.cs` | Edificio ABIERTO (suelo, sin muros ni techo) que coloca sus muebles con el paso de 2 m y añade las necesidades básicas. |
| `AreaMicrocosmos.FloorBounds` / `FloorRectLocal` | `Assets/Editor/AreaMicrocosmos.cs` | El rectángulo útil del piso, para que el pueblo lo llene entero. |
| `CivicBuilder.Homes` / `Household` | `Assets/Editor/CivicBuilder.Homes.cs`, `Assets/Scripts/Body/Household.cs` | Regla 6: un hogar (camas, retrete, lavabo, mesa con sillas, despensa) para cada personaje; `Household.Check` lo comprueba. |
| `MicroStyles` | `Assets/Scripts/Microcosmos/MicroStyles.cs` | Regla 7: el catálogo de estilos de época y lugar (año + región → cada mueble). |
| `MicroBody` | `Assets/Scripts/Microcosmos/MicroBody.cs` | Regla 7: el cuerpo de quien usa el mueble (plan y medidas). |

---

## Las medidas (a la escala de los insectos-humanos)

Los insectos-humanos miden como una persona (1,6–1,85 m, `AreaMicrocosmos.Folk`): cápsula de **0,35 m** de radio y
NavMeshAgent de **0,4 m** de radio. De ahí salen todos los números:

| Medida | Valor | De dónde sale |
|---|---|---|
| Ancho de un personaje (agente) | 0,8 m | 2 × 0,4 m |
| **Paso libre entre dos muebles** (`MicroBuildRules.Passage`) | **2,0 m** | dos agentes hombro con hombro (4 × 0,4 = 1,6 m) + 0,2 m de holgura a cada lado (evitación y giro del cuerpo). Es el mismo estándar del santuario (`FurnitureSpacing.Standard`). |
| Calle de pueblo (`Street`) | 4,0 m | dos pasos: se cruzan dos parejas (o un carro y una persona) |
| Sitio de trabajo delante de un puesto (`WorkSpot`) | 0,9 m | donde se pone quien usa el fogón, la mesa o el banco (dentro del paso) |
| Alto que ya es muro (`WallHeight`) | 1,2 m | los ojos de un insecto-humano van a ~1,6 m: por encima de 1,2 m un panel largo tapa la vista |
| Alto que ya es techo (`RoofHeight`) | 2,2 m | una losa por encima de las cabezas |
| Era avanzada (`TownFrom`) | desde −3500 | las primeras ciudades (Uruk). Antes, las aldeas conviven con los muebles gigantes del área |

---

## Las siete reglas

### 1. Paso de 2 m entre objetos
Entre dos muebles cualesquiera quedan **≥ 2,0 m libres** (en planta), dentro de un edificio y entre edificios. Lo que va
unido cuenta como UN mueble y no se separa: las sillas con su mesa, la sartén y la olla sobre la cocina, el cuchillo sobre
la tabla, la encimera corrida con su fregadero (son hijos de la misma raíz `MicroPiece`). Lo que se pisa (tatami,
esterilla, bancal, alfombra: `MicroPiece.walkable`) no cuenta. Entre edificios, una calle de 4 m.

**Cómo aplicarla**: `CivicBuilder.Building` coloca los muebles en filas con 2 m entre ellos y deja 2 m de margen al borde del
suelo del edificio (el tamaño que se le pide es un mínimo: crece si los muebles no caben). En un pueblo, los solares van
separados por calles de `Street`.

### 2. Sin muros y sin techo
Los edificios son **suelos abiertos**: la losa del suelo con el color del material real (adobe, ladrillo, terrazo, tatami…),
su rótulo y sus muebles; ni muros, ni puertas, ni techos, ni edificios macizos (tapan la vista y el paso), como en el
santuario. Se permiten postes y columnas sueltos (finos: no tapan) y las piezas bajas (bancos, parapetos de menos de
1,2 m). Los **muros del borde del piso** (`Muros_del_piso`, regla 11 de la plantilla) no son de un edificio y se quedan.

**Excepción: las paredes de cristal** (Berón 2026-11-03: «para salas que deban estar aisladas se pueden utilizar paredes de
cristal de modo que no bloquean visibilidad, y se debe mantener las reglas de espaciado»; docs/hospital.md §6). Un paño `GlassWall`
(`Pared_cristal…`, con su dintel) no cuenta como muro (no tapa la vista), pero **sí guarda el paso de la regla 1**: `MicroBuildRules.Pieces`
lo mide contra los muebles, de dentro y de fuera de su sala (dos paños, entre sí, no). En el santuario hace lo mismo `FurnitureSpacing`.
Solo para salas que deben aislarse (quirófanos, aislamientos, UCI, salas frías) y en épocas en las que tiene sentido.


**Excepción: los muros que la historia necesita** (confirmado por Berón, 2026-11-02: «se pueden quedar los muros que sean
necesarios para la historia»). Una barrera que ES la historia va bajo un `MicroPiece` con `kind = MicroBuildRules.Historic`
(«historico»): no cuenta como muro ni para el paso. Hoy son dos:
- el **muro del gueto de Varsovia** (`CompassionSceneBuilder`, «Muro del gueto»: sus salidas son los pasos de la misión de
  Irena Sendler);
- los **muros rotos del bloque 4 de Chernóbil** (`StorySceneBuilder.Scientists`, «El bloque 4»: la ruina del reactor que
  Ananenko, Bespálov y Baranov rodean para bajar a los sótanos inundados).
Cualquier otra historia que necesite un muro (una muralla que se defiende, una celda) lo marca igual y lo cita aquí.

`Validate` marca: piezas llamadas `Muro_*`, `Cubierta`, `Faldon`, `Dintel`, `Techo`, `Tejado`, `Chapitel`; y, dentro de
cada `AreaFunctionSite`, toda pieza (que no sea mueble ni persona) de ≥ 1,2 m de alto y ≥ 2,5 m de largo y ≤ 0,6 m de
grueso (muro), toda losa a ≥ 2,2 m sobre el suelo del edificio de ≥ 4 m² (techo) y todo bloque de ≥ 1,2 m de alto y ≥ 6 m²
(edificio macizo).

### 3. Muebles detallados, con sus utensilios
Nada de cubos lisos: cada mueble es una unión de piezas que se reconoce por su forma (`MicroFurniture`, que reutiliza
`KitchenProps` del santuario). La **cocina** lleva lo que el trabajador necesita: encimera con armarios, fogón o cocina de la
era, **sartén, olla, tabla con cuchillo, bote de cucharones, cuencos y platos**, fregadero o barreño, y la despensa. Cada
puesto interactuable (`AreaFunctionStation`) es la raíz de su mueble (con un `BoxCollider` que envuelve sus piezas), así
que las funciones del registro (`AreaFunctions`: `coc_fogon`, `res_comer`, `dojo_muneco`…) se hacen en el mueble detallado.

| Clave del puesto | Mueble (`MicroFurniture.Build`) |
|---|---|
| `fogon` | cocina: encimera + fogón de la era (cocina de leña, hornillo de carbón, wok chino, chulha, tannur) + sartén, olla, tabla y cuchillo, cucharones, cuencos |
| `horno` | horno de pan con su pala y la mesa de amasar |
| `barra` | barra con taburetes, tazas y tetera |
| `mesa` | mesa con 2–4 sillas o bancos, platos y cuencos |
| `mesa_laboratorio` | mesa de laboratorio con matraces, mechero y cuaderno |
| `cama` / `camilla` | cama con patas, colchón, sábana y almohada (catre, estera, litera según la era) |
| `lavadero` | pila o barreño con su agua, tabla de lavar y cubo |
| `estante` / `armario` | estantería con baldas llenas (frascos, rollos, libros) |
| `pizarra` | pizarra sobre caballete con tiza |
| `banco_taller` / `torno` | banco con tornillo y herramientas / torno con bancada y volante |
| `tatami` / `esterilla` / `bancal` | se pisan (walkable): tatami con borde, esterilla, bancal con matas |
| `muneco` | muñeco de madera (muk yan jong) con sus tres brazos y su pierna |
| `altar` | mesa de ofrendas de piedra con losa, vasijas y lamparilla |
| `puesto` | puesto de mercado: mesa, dos postes con toldo bajo (2 m), cestos con mercancía y banco del vendedor |
| `granero` | silo de barro escalonado con tapa, molino de mano con su mano y saco de grano |
| `torno_alfarero` | torno con su plato y la vasija, banco del alfarero y pila de cuencos |
| `pozo` | brocal de piedra con agua, dos postes con torno, manivela, soga y cubo, y la pila al lado (cubre «lavarse») |
| `telar` | telar de bajo lizo: bastidor, enjulios, hilos de urdimbre, tela tejida, lizos, peine, pedales, lanzadera y banco |
| `parapeto`, `tunel` | sacos terreros en cuatro hileras (1,1 m) / túnel de viento de tablas con ventilador y balanza |
| `inodoro`, `lavabo`, `silla`, `banco` | necesidades básicas (no son puestos): pozo negro, letrina o inodoro con cisterna; pila de piedra, palangana o lavabo con grifo y espejo; silla; banco |

### 4. Necesidades básicas
Todo pueblo tiene donde **dormir** (camas), **hacer sus necesidades** (retrete, letrina o bacinilla de la era),
**lavarse** (lavabo, palangana o pila), **comer** (mesas), **sentarse** (sillas o bancos) y **cocinar**. Son
`MicroPiece.need` = `dormir`, `aseo`, `lavarse`, `comer`, `sentarse`, `cocinar` (`MicroBuildRules.Needs`).
`CivicBuilder.Building` las añade solo a cada **Casa** (cama, retrete, lavabo, mesa con sillas, cocina) y
`CivicBuilder.Civilize` comprueba al final la escena entera y añade unos **aseos de la era** (letrina / retrete y
palangana / lavabo) si aún falta alguna.


### 6. Un hogar para cada personaje (2026-11-02)
Berón: «los pueblos de historia de los microcosmos deben ser reconstruidos para que haya hogares para los demás personajes,
además de los lugares de trabajo y los servicios básicos, para que cada personaje realice su ciclo de vida correctamente».

Todo insecto-humano de un pueblo civilizado (cualquier `FolkLineage` del pueblo, también los de la historia) vive en un
**hogar** (`Household`, runtime): una casa con **una cama por persona**, **retrete**, **lavabo**, **mesa con sillas** y una
**despensa** (`FoodContainer`, se repone cada día). Las **parejas y sus hijos comparten** el suyo; un niño puede dormir con su
madre. Ahí atiende su cuerpo (`NeedsDriver` → `HomeNeedSpell`): come en su mesa, duerme en su cama, va al baño y se lava; la
comida del mediodía es la de su jornada, en el comedor de la era (docs/ciclo-de-vida.md §Humanos).

`CivicBuilder.Homes` lo hace **después** de cada constructor de era o pueblo y de sus casas de relleno (`Civilize`, paso 3):
reparte a la gente en familias (las parejas juntas) y a cada familia le da (1) una casa que ya hay —la de su jornada, o la más
cercana— si le quedan camas libres para todos (para una pareja, una más para un hijo), o si no (2) **una casa nueva de la
era** (`CivicBuilder.NewHouse`: plantilla «Casa», abierta, con sus muebles detallados y el paso de 2 m) en el solar libre más
cercano a su trabajo, con una cama por persona y, si es una pareja, dos más para los hijos. La jornada (`MicroJornada.home`) va
a esa casa. El pueblo conserva además sus **lugares de trabajo** (edificios que no son casas) y sus **servicios básicos**
(regla 4).

Comprobación: `Household.Check(raíz)` devuelve quién no tiene hogar y qué le falta a cada hogar. La prueba **62**
(`CivilizedErasTest`) lo exige en cada escena (y que cada persona lleve el ciclo de vida de una persona, que las parejas
compartan casa y que haya lugares de trabajo); la **63** (`MicroBuildRulesTest`) prueba el validador con casos pequeños.

### 5. Eras avanzadas: el pueblo o la ciudad real ocupa el piso entero (su barrio, a ×100)
**(2026-11-03) En un pueblo el barrio es el piso real × 60** (`AreaMicrocosmos.TownScale`: el 60 % del lado del piso ×100, de 1,4 a 2,8 km) y el pueblo se levanta sobre su **plano real** a escala 1:1 (calles, río, lago, colinas, campos, muralla y el sitio de cada edificio: [microcosmos-pueblos-mapas.md](microcosmos-pueblos-mapas.md)). Lo de abajo sigue valiendo para las eras sin pueblo.

Desde la escala ×100 (2026-11-02, `AreaMicrocosmos.AreaScale`) «el piso entero» es el **barrio de la historia**: el piso real
× 10 (lo que medía todo el piso a ×10) alrededor de la plaza de la historia (`AreaMicrocosmos.District`, `MicroFloor.districtSize`).
`FloorBounds` / `FloorRectLocal` dan ese barrio, así que los pueblos se levantan igual que antes; alrededor, el resto del piso
×100 es el santuario gigante (ver [microcosmos-areas.md §Escala](microcosmos-areas.md#escala)).

Desde las primeras ciudades (`MicroBuildRules.IsTownEra(year)`: año ≥ −3500) la historia **recrea el pueblo o la ciudad real** sobre todo el piso (`FloorBounds`). **(2026-11-03) Sin quitar ningún mueble del área: el pueblo se adapta al piso** (regla 13: los rodea o se sube encima, trepando). Investiga el lugar real
(planos, fotos, calles, materiales) y cítalo en el doc del microcosmos. Lo **habitan solo insectos-humanos civilizados**;
**flora y fauna salvajes solo si el lugar y la época las tenían** y salen en la historia (el puerto de Hong Kong con sus
milanos; un laboratorio de París de 1898 no tiene ninguna). En ese caso, la flora y fauna van fieles a la historia y al lugar
(`MicroBiome` del microcosmos).

**La flora del pueblo es la de un pueblo humano** (Berón 2026-11-01: «en Bruce Lee las flores gigantes tapan el pueblo; en Berna
el musgo son cubos verdes enormes»). Los insectos-humanos viven en un pueblo a escala humana, así que su flora no es la de
hormiga (×25: una flor de 1,5 m medía 37 m): `AreaMicrocosmos.Nature` en **modo pueblo** (la misma condición que quita los
muebles gigantes) la siembra **a escala real** (1 cm = 1 cm: un tilo de 15 m con la copa por encima de las cabezas, un seto de
0,8 m, un césped de 5 cm), **escasa** (una planta cada ~25 m²) y **fuera de calles y solares** (el tronco o la mata a ≥ 1 m de
la huella de cada edificio, que ya lleva 1 m de margen, y del borde de cada calle). Junto al pueblo (a < 15 m) crece **lo
plantado** (`Biome.town`: árboles de calle, setos, céspedes, parterres; sin piedras sueltas: el adoquín es la calle); en las
afueras, **lo del lugar** (`Biome.plants`). Las ciudades tienen además sus **animales del pueblo** (`Biome.townFauna`: ver
[microcosmos-fauna-eras.md](microcosmos-fauna-eras.md#los-animales-del-pueblo-2026-11-01)). `MicrocosmosFloorTest` comprueba
en cada pueblo que ninguna planta pase de 25 m, flote o esté en una calle o un solar, y que haya animales.

---
### 7. Muebles a la medida de quien los usa, en el estilo de su época y lugar (2026-11-02)
Berón: «las camas y demás servicios deben estar adaptados para los usuarios, en este caso las de humanos-insectos no me han
dado la impresión de que estén adaptadas… asegúrate de estar usando camas y demás servicios con el estilo de época/lugar,
deberíamos tener una lista así podemos reutilizar para épocas/lugares similares y el template de microcosmos podría escoger
automáticamente».

Cada mueble de `MicroFurniture.Build(parent, clave, pos, giro, rótulo, año, tamaño, región, cuerpo, usuarios)` sale de DOS cosas:

1. **El estilo de época y lugar** (`MicroStyles.For(año, región)`, el catálogo de abajo): qué cama, asiento, mesa, hogar,
   retrete, lavamanos, almacén y luz, y los colores de su madera, sus tejidos y su metal.
2. **El cuerpo de quien lo usa** (`MicroBody.Fit`, medido sobre el insecto-humano con su ficha de `InsectMorphology`): el
   plan (cómo descansa y se sienta) y sus medidas. El estilo pone los materiales y los detalles; el cuerpo, la forma y el tamaño.

Los insectos-humanos son **centauros** (`CubeInsect.DressFolk`): el torso se yergue a la estatura de una persona (1,6–1,85 m),
pero el abdomen (gáster, élitros, opistosoma) sigue horizontal detrás, sobre 4–6 patas. No caben tumbados de espaldas en una
cama humana ni en una silla con respaldo:

| Plan (`MicroBody.Plan`) | Especies | Cama (cómo duerme) | Asiento | Retrete | Lavamanos |
|---|---|---|---|---|---|
| Humano | personas (escenas de ciclo humano) | la del estilo, 2 × 1 m, de espaldas, con almohada | la silla, el taburete, el cojín o el banco del estilo | el del estilo | el del estilo, a 0,8 m |
| Centauro | hormiga, termita, tijereta, grillo, saltamontes | **lecho de vientre** del estilo a su largo (cabeza → punta del abdomen + 0,15 m) y a su ancho con las patas recogidas, con **apoyo del pecho** en la cabecera y la manta sobre el abdomen | **silla de montar**: sostiene el abdomen a la altura de su vientre (hacia atrás), sin respaldo, con las patas del estilo (cojín alto, tijera, klismós, caja de munición, tubo cromado…) | **plataforma de abdomen**: se pasa por encima y la punta del abdomen queda sobre el agujero, con estribos para las patas y un asidero delante; la cisterna, la jarra o la cal del estilo, al alcance | del estilo, a la altura de sus manos, más ancho, con **peine de antenas** |
| Cúpula | mariquita, pelotero | **nido de cúpula**: el lecho del estilo con un borde alrededor (la cúpula no se tumba de espaldas) | asiento bajo y ancho **de cuna**, con barandillas | ídem | ídem |
| Alado | abeja, luciérnaga, cigarra | **percha de dormir**: duerme agarrado (las abejas duermen colgadas de las mandíbulas) a una barra acolchada sobre una estera blanda | **barra de agarre** a la altura del vientre | ídem | ídem |
| Mantis | mantis (cuatro brazos) | **barra de colgarse** a su estatura (las mantis cuelgan para descansar y mudar) | barra de agarre | ídem | jofaina para cuatro brazos (0,75 m) |
| Araña | araña lobo (ocho patas) | **hamaca de seda** entre cuatro postes, con sus hilos radiales y en espiral, ancha como sus patas abiertas | **anillo** de cojines bajo el opistosoma | ídem | ídem |
| Ciempiés | ciempiés | lecho de vientre larguísimo | banco corrido | ídem | ídem |

La **mesa** va a la altura de las manos de quien come: 0,75 m para una persona sentada, 0,35 m en las culturas de suelo
(cojines, zabutón) y, para los insectos-humanos (erguidos sobre su asiento de abdomen), a la de sus manos (≈ 0,8–0,95 m), con
**un asiento a la medida de cada comensal**; el largo de la mesa crece con el ancho de sus asientos.

**Los hogares** (regla 6) lo aplican persona a persona: `CivicBuilder.MakeHousehold` mide a cada miembro (`MicroBody.Of`),
**rehace su cama** para él (`MicroFurniture.Refit`, mismo sitio y puesto) y, en la primera familia de la casa, la mesa (un asiento
por miembro), el retrete y el lavamanos para todos (`MicroBody.Union`: lo más largo y ancho, las manos a su media); las camas
de los hijos, para el cuerpo de sus padres a la vez. Luego **recoloca** los muebles con el paso de 2 m (`Relayout`; el suelo
crece si hace falta). Lo que se construye antes de saber quién lo usa (salas, barracones, comedores) se hace para el cuerpo por
defecto del microcosmos (`MicroFurniture.DefaultBody`: un insecto-humano centauro; una persona en la escena de ciclo humano).
Cada mueble apunta en su `MicroPiece` su estilo (`style`), para quién está hecho (`fitFor`) y, la cama, cómo se tumba
(`rest`: espalda, vientre o percha), que es lo que usa `HomeMove.Lie`: de vientre baja el torso y recoge las patas sobre el
colchón; en la percha se queda agarrado, de pie; de espaldas, como una persona.

La **mariquita** insecto-humano lleva los élitros recogidos (`InsectMorphology.ForFolk`: 0,5× y el pronoto el doble de alto; el pelotero, 0,8×): con los de la
ficha, a la escala de una persona de 1,7 m, era una losa roja de 2 × 2,5 m tumbada detrás del torso.

---
## Las reglas 8–12: los edificios del pueblo (2026-11-03)

Berón: «entran actividades del santuario como centros de estudio, centros de entrenamiento, de baile, plazas, estaciones de
transporte, parques, museos, centros de investigación, guarderías, estatuas, calles, edificios de varios pisos, centros de
policía, bomberos, mercados, mecánicas, talleres, milicia, piscinas, hoteles, carpintería, piletas, construcción, centros de
tecnología y estructuras naturales como ríos, lagos, montañas, etc., con su estilo de la época-lugar y adaptación para
humanos-insectos y humanos … que deben crearse junto a su interacción … para poder pertenecer a sus áreas de trabajo,
descanso, recuperación u ocio, de modo que formen parte de sus actividades semanales». Desde ahora todo edificio cumple,
además de las siete reglas de arriba, estas cinco. `MicroBuildRules.ValidateSites` (dentro de `Validate`) las comprueba.

### 8. Cada edificio nace con su interacción y su categoría de actividad
Todo `AreaFunctionSite` lleva (lo pone `CivicBuilder.Tag` al construirlo y otra vez al final de `Civilize`, ya con los muebles
rehechos para cada hogar):
- `activity`: **trabajo**, **descanso**, **recuperacion** u **ocio** (`AreaFunctionSite.ActivityOf(plantilla)`);
- `hasWork`: si además tiene puestos de trabajo (quien lo atiende trabaja allí);
- `capacity`: cuántos lo usan a la vez (camas, asientos de sus mesas, puestos, sitio en lo que se pisa);
- `interaction`: lo que se hace allí, en una frase (`AreaFunctionSite.InteractionOf`);
- `bodies`: los cuerpos para los que tiene muebles («Humano, Centauro (Ant)…»);
- y sus **funciones del registro** (`AreaFunctions.Buildings`), con sus puestos interactuables (`AreaFunctionStation`).
El horario semanal las lee (ver [microcosmos-actividades.md](microcosmos-actividades.md)). Violación: «sin etiqueta de actividad»,
«sin su interacción o su capacidad».

### 9. Adaptado a personas y a insectos-humanos
Cada edificio tiene al menos un mueble hecho a la medida de una persona y otro a la de un insecto-humano (`MicroPiece.fitted`;
`fitFor` vacío = persona). Los muebles se hacen para el cuerpo por defecto del microcosmos (el insecto-humano centauro); si a un
edificio no le queda ninguno para personas, `CivicBuilder.Adapt` le añade un **banco para personas** (y al revés, un **asiento de
abdomen**). Violación: «no adaptado a personas / a insectos-humanos».

### 10. El estilo de época y lugar sale del catálogo
`AreaFunctionSite.region` es una región de `MicroStyles.Region` y cada mueble lleva su `MicroPiece.style`, un `id` del catálogo
(abajo). El NOMBRE del edificio también es de su época y lugar (`MicroStyles.CivicName`, tabla de abajo): si en esa época no
existía, no se construye (ni comisaría en la Crotona de −530, ni estación de tren en Woolsthorpe en 1666). Violación: «sin región
del catálogo», «estilo … fuera del catálogo».

### 11. Toda residencia tiene su frutal
Berón: «las residencias del microcosmos deben tener plantas frutales». Cada Casa (y orfanato) planta un **frutal** de su época y
lugar (`MicroStyles.Fruits`, mueble «frutal»: alcorque, tronco, copa y fruta; palmera o mata según la especie), rotulado con su
nombre; y toda casa que recibe un hogar (`MakeHousehold`) sin frutal recibe uno. Violación: «residencia … sin frutal».

| Estilo | Frutales (uno por casa) |
|---|---|
| `prehistoria` | higuera silvestre, almendro (Gilgal I, ~9400 a. C.: los primeros higos cultivados) |
| `mesopotamia` | palmera datilera, granado |
| `egipto` | palmera datilera, sicómoro, granado |
| `egeo`, `roma` | olivo, higuera, granado |
| `sur_asia` | mango, banano, yambolán |
| `islam_abasi`, `otomano` | granado, palmera datilera, higuera, membrillo |
| `china_tradicional` | lichi, níspero, longán |
| `hongkong_tonglau` | papayo en maceta, níspero, banano |
| `japon_edo_meiji` | caqui, ume |
| `europa_medieval`, `estuardo`, `georgiano` | manzano (el de Woolsthorpe), peral, ciruelo |
| `alpino` | grosellero, serbal (a 1800 m no maduran las manzanas) |
| `europa_romantica`, `belle_epoque`, `entreguerras` | peral en espaldera, cerezo, manzano |
| `francia_rural` | manzano, ciruelo, castaño |
| `frente_1914` | el manzano de la granja en ruinas |
| `hispanoamerica` | mango, guayabo, lechosa |
| `maya_clasico` | cacao, aguacate, ramón, zapote, papaya |
| `norteamerica_victoriana` | manzano, melocotonero |
| `norteamerica_moderna` | naranjo, limonero, aguacate (los naranjales del valle de San Gabriel) |
| `sovietico` | manzano, cerezo ácido (los patios de Prípiat) |

### 12. Edificios de varias plantas: en terraza, sin techos
Un edificio de varios pisos (`CivicBuilder.Storeys`: hoteles, inmuebles de París y Berna, tong lau, bloques de Prípiat, el Palacio
y el Templo de las Inscripciones de Palenque) pone sus plantas **en terraza**: la planta k es un bloque de k × 3,2 m DETRÁS de la
anterior (MicroPiece «planta», se pisa), con un pretil bajo (0,8 m) al fondo y una **rampa** lateral de 20° (MicroPiece «rampa»).
Ninguna planta tapa a otra (no hay techos, regla 2) y cada una lleva sus muebles con el paso de 2 m. Violación: «la planta en
terraza de … tapa …».

### 13. El piso no se rompe: el pueblo se adapta a él (2026-11-03)
Berón: «en la captura de Bruce me ha dado la impresión de que el dojo se rompía para no interferir con la ciudad, en los requisitos de
microcosmos debe estar que el piso no debe romperse, las ciudades insecto deben adecuarse al piso y no al revés, los insecto-humanos
pueden trepar plataformas verticales como las paredes y patas de sillas, mesas, etc., de modo que no tienen problema con subir a mesas,
camas, etc., en caso que parte de los pueblos se cree encima de estructuras elevadas».

- **Ninguna pieza del piso se quita ni se mueve** para el pueblo ni para la historia: se acabaron el despeje del barrio
  (`ClearForTown`) y el apartar el mueble que pisa la historia (`ClearUnder`, ahora `TouchingUnder`: solo cuenta). Antes del pueblo
  solo se monta la forma finalizada del piso (regla de las dos formas) y se recorta lo que cae entero fuera de sus muros.
- El pueblo **rodea** los muebles que apoyan en el suelo: los solares (`CivicBuilder.Lots`) los marcan ocupados
  (`AreaMicrocosmos.FloorFurniture`, lo que empieza a menos de 5 m del suelo; bajo el tablero de una mesa sí se construye), y el plano
  no tiende calles, ríos, lagos, colinas, campos ni acantilados a través de ellos (se corta el tramo).
- O **se sube encima**: las caras de arriba de los muebles son suelo del pueblo (`CivicBuilder.Outposts`): dentro del barrio, un
  **barrio alto** (hasta 6 casas o un mercado en las mesetas más grandes que nada tapa); fuera, dos miradores.
- **Se trepa**: un `NavMeshLink` del suelo a cada cara de arriba (la IA trepa por las patas y los costados) y un `Climbable` en cada
  mueble alto (el jugador trepa con `PlayerClimber`).

Comprobación: `MicroFloor.floorPieces` (las piezas del piso al montarlo) y `MicroFloors.Validate` («el piso se rompió: N piezas al
montarlo y M ahora»); si hay edificios sobre los muebles, enlaces para trepar (`MicroFloor.climbLinks`). `BruceLeeStoryTest` (60) mira el
Dojo entero.

### 14. El relieve, con aspecto real (2026-11-03)
Berón: «¿un cono gris? ¿es una montaña? entonces debería tener una apariencia realista y lo mismo con lagos, colinas, acantilados».
`CivicBuilder.Terrain` (low-poly facetado, sin texturas):
- **montes y colinas** (`Peak`): malla radial con ruido de crestas, pie irregular; pradera, roca en las laderas de más de ~34°, canchal
  arriba y **nieve** si el lugar la tiene (`| nieve` en el plano: los Alpes de Sils, el Pikes Peak, el Rainier, la Sila); pradera seca
  en los lugares áridos (`| seco`: Saqqara, Pasadena, Colorado, el Oros). Alturas reales sobre el pueblo, recortadas a 300 m; las colinas,
  con pendiente que se sube andando (≤ 0,35 de su radio);
- **lagos** (`Lake`): orilla irregular, agua lisa y brillante, franja de orilla;
- **ríos** (`RiverReach`): cinta de ancho variable con sus dos riberas, en tramos de ≤ 60 m;
- **acantilados** (`Cliff`): pared de roca vertical facetada con su cornisa (`acantilado Nombre | alto | x z, …` en el plano: el cabo de
  Crotona, la barranca del Orinoco en Angostura, la ribera del Bósforo, las gargantas del Lignon, el escarpe de la sierra de Palenque).

### Solo los muros importantes
Siguen valiendo las reglas 1–2: paso de 2 m, sin muros ni techos. Lo único alto que se permite es lo que la historia necesita
(`MicroPiece.kind = "historico"`) y lo que el plano real tiene **a ras de suelo**: las murallas del plano son su cimiento (0,5 m:
Kapilavastu, la Ciudad Redonda de Bagdad, el cuartel de Selimiye), las colinas y los montes son terreno.

## Los edificios del pueblo (`MicroStyles.Civic`, `CivicBuilder.Civic`)

| Tipo | Plantilla del registro | Actividad | Puestos (muebles) | Nombres de época y lugar (ejemplos) |
|---|---|---|---|---|
| Estudio | Escuela | trabajo | pizarra, pupitres, estante | didaskaleion, Casa de la Vida, kuttāb, mektep, casa de los escribas (maya), escuela |
| Entrenamiento | Centro de entrenamiento | ocio | tatami (pista en el juego de pelota maya) | palestra, akhāṛā, tekke de luchadores, juego de pelota, Turnhalle, club deportivo |
| Baile | Sala de baile | ocio | pista de baile, mesa de músicos | orchestra, sala de joropo, plataforma de danza, salón de baile, Palacio de Cultura |
| Plaza | Plaza | ocio | fuente, estatua | ágora, foro, maydān, meydan, Plaza Mayor, Gran Plaza, plaza central |
| Monumento | Plaza | ocio | estatua, pila | kouros, coloso, estela, león guardián, monumento (no en el islam ni el otomano) |
| Pileta | Fuente pública | ocio | fuente, lavadero | krene, sabil, çeşme, pila del acueducto, grifo público |
| Parque | Parque | ocio | frutales, fuente | alsos, bustān, bahçe, ārāma, arboleda de cacao, alameda, parque |
| Estación | Estación | trabajo | andén, taquilla | puerto, caravasar, embarcadero, posta de diligencias, estación de ferrocarril |
| Museo | Museo | ocio | vitrinas, estatua | tesoro del templo, colección de Santa Irene, museo (desde 1683) |
| Investigación | Laboratorio | trabajo | mesa de laboratorio, estante, instrumentos | observatorio (Shammasiyya, la torre del Palacio), gabinete científico, laboratorio |
| Guardería | Guardería | trabajo | cunas, estera de juegos, cocina y aseo | casa de cuidado de los niños; crèche (desde 1840); detski sad |
| Policía | Comisaría | trabajo | mostrador, armero, registros | vigiles, medjay, shurta, karakol, alguaciles, comisaría (desde 1829), militsiya |
| Bomberos | Bomberos | trabajo | bomba sobre ruedas o autobomba, pila, armario | vigiles, tulumbacılar, bomba de la parroquia, parque de bomberos, firehouse |
| Mercado | Mercado | trabajo | puestos | mercado del ágora, zoco, mercado de la plaza |
| Mecánica | Taller | trabajo | banco, torno, carro | taller del carretero, taller mecánico, garaje (no maya: sin rueda de carga) |
| Taller | Taller | trabajo | bancos | taller de artesanos, de jade y obsidiana |
| Milicia | Cuartel | trabajo | armero, patio de instrucción, catre | hoplitas, castra, jund, casa de los guerreros, armería de la Guardia Nacional |
| Piscina | Baños | recuperación | pila de baño o temazcal | balaneion, termas, hammam, lago sagrado, puṣkariṇī, temazcal, piscina |
| Hotel | Hotel | descanso | recepción, comedor, habitaciones (3 plantas en terraza) | katagogion, mansio, funduq, han, dharmaśālā, posada, hotel |
| Carpintería | Carpintería | trabajo | caballetes, banco | carpintería |
| Obra | Obra | trabajo | material, caballete | obra: casa, bloque nuevo o templo en construcción |
| Tecnología | Centro de tecnología | trabajo | consolas (instrumentos, centralita o calculadora según el año) | taller de mecanismos, de astrolabios, central telefónica (1880), centro de cálculo (1950) |
| Edificio | Casa | descanso | camas y mesa en 3 plantas en terraza | tong lau, inmueble, khrushchyovka, konak, insula, palacio en terrazas |
| Mirador | Mirador | ocio | monumento, frutal | sobre las caras de arriba de los muebles gigantes (reglas de los microcosmos) |

Muebles nuevos (`MicroFurniture.Civic.cs`, todos de pocos cubos): `frutal`, `estatua` (estela, león guardián), `fuente`,
`vitrina`, `piscina` (se pisa), `temazcal`, `pista` (se pisa), `consola`, `cuna`, `armero`, `anden` (se pisa), `carro`,
`caballete`, `material`.

## El catálogo de estilos de época y lugar

`Assets/Scripts/Microcosmos/MicroStyles.cs`. Cada entrada cubre un intervalo de años y una o varias regiones o culturas;
`MicroStyles.For(año, región)` da la primera de esa región que cubre el año; si no hay, la de sus **vecinas culturales** (Hong
Kong → China; Alpes y Francia rural → Europa del norte → Europa urbana; Otomano ↔ Islam; Roma ↔ Egeo…); antes de −4000, la
prehistoria; y si nada, la más cercana en el tiempo. **Épocas y lugares parecidos reutilizan la misma entrada**: un pueblo nuevo
solo necesita su región (y una entrada nueva si su cultura no está).

La región de cada microcosmos sale de su nombre (`MicroStyles.RegionOf`) y va en su raíz (`MicroEra.region`), en la plantilla
(`MicrocosmSpec.region`: **la plantilla la escoge sola**; la ventana «Nuevo microcosmos» deja elegir otra) y en cada edificio
(`AreaFunctionSite.region`: el barrio de Seattle en el pueblo de Bruce Lee es de Norteamérica aunque el pueblo sea de Hong Kong).

| Entrada (`id`) | Años | Regiones | Cama | Asiento | Mesa | Hogar | Retrete | Lavamanos | Almacén | Luz | Fuentes |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `prehistoria` | … −4001 | Prehistoria | estera y piel | en el suelo | estera de comer | hogar de piedras en el suelo | pozo negro | pila de piedra | cestos | el fuego | camas de hierba de Ohalo II (~21 000 a. C.); Natufiense |
| `mesopotamia` | −5000 … −500 | Mesopotamia | lecho de juncos (armazón con cuerdas) | taburete | mesa baja | tannur | asiento sobre vasija y tubo | cántaro y jofaina | tinajas | lámpara de concha | camas de las tumbas reales de Ur; letrinas de Tell Asmar (~2200 a. C.) |
| `egipto` | −3100 … −30 | Egipto | cama inclinada con patas de león, tabla de los pies y reposacabezas | taburete de tijera | mesa baja | tannur (horno de pan) | asiento de caliza sobre vasija de arena | aguamanil | arcón | lucerna | camas de Tutankamón (Museo Egipcio); retrete de Amarna |
| `egeo` | −1600 … −30 | Egeo (Egina, Crotona, Alejandría) | kliné con fulcro y escabel | klismós | trapeza de tres patas | brasero (eschara) | orinal (amis) en su silla | louterion | arcón | lucerna | G. Richter, *The Furniture of the Greeks, Etruscans and Romans*; bañera de Pilos |
| `roma` | −500 … 476 | Roma | lectus | taburete (sella) | mesa redonda | brasero y trébede | letrina comunal con xylospongium | labrum (louterion) | arca | lucerna | letrinas de Ostia |
| `sur_asia` | −2600 … 1200 | Sur de Asia (Kapilavastu, Patanjali) | charpai (cama de cuerdas, khaṭvā) | taburete bajo (pīṭha) | mesa baja | chulha | asiento sobre el pozo de anillos | cántaro y lota | tinajas | diya | Vinaya Piṭaka (mañca, pīṭha); pozos de anillos de terracota |
| `islam_abasi` | 600 … 1499 | Islam (Bagdad) | minder sobre estrado | cojines | bandeja baja | tannur | letrina a la turca | ibrīq y ṭast | alacena de nicho | candil de bronce | casas abasíes de Samarra |
| `otomano` | 1500 … 1923 | Otomano (Escútari) | sedir y minder, con yorgan | cojines | sini (bandeja de cobre) | ocak y mangal | hela a la turca con ibrik | ibrik y leğen | dolap | candil | casa otomana de Üsküdar |
| `china_tradicional` | −200 … 1944 | China, Hong Kong | kang de ladrillo | taburete | mesa de kang | cocina de ladrillo con wok | bacín (matong) | palangana en su soporte | arcón | farol de papel | — |
| `hongkong_tonglau` | 1945 … 1979 | Hong Kong (Kowloon) | litera de madera | taburete | mesa redonda plegable | fogón de gas con wok | a la turca, compartido | palangana esmaltada en su soporte | estantería metálica | bombilla | pisos subdivididos de los tong lau |
| `japon_edo_meiji` | 1603 … 1925 | Japón | futón sobre tatami | zabutón | chabudai | irori | kawaya a la turca | tarai | tansu (arcón) | andon | — |
| `europa_medieval` | 476 … 1499 | Europa medieval (y sus vecinas) | jergón | banco | mesa de caballetes | hogar con caldero | orinal | aguamanil | arcón | vela | — |
| `alpino` | 1500 … 1918 | Alpes (Sils Maria) | cama de pino tallada | banco corrido | mesa larga | estufa de azulejos (Kachelofen) | orinal | aguamanil | arcón | quinqué | casa engadinesa (Nietzsche-Haus) |
| `estuardo` | 1500 … 1719 | Europa del norte y urbana, Francia rural | cama de dosel | taburete ensamblado | mesa larga | hogar con espetón y caldero | silla-orinal (close stool) | aguamanil | arcón | vela | Woolsthorpe Manor (National Trust) |
| `georgiano` | 1720 … 1836 | ídem | cama de cuatro postes | Windsor | mesa redonda | cocina abierta con espetón | cómoda-orinal | aguamanil | cómoda | vela | Jenner's House (Berkeley) |
| `europa_romantica` | 1837 … 1879 | ídem | cama de hierro | Thonet | mesa larga | cocina económica de hierro | orinal | aguamanil | cómoda | quinqué | Thonet n.º 14 (1859) |
| `belle_epoque` | 1880 … 1918 | Europa urbana y del norte (París, Berna, Planta Atómica) | cama de hierro | Thonet | mesa redonda | cocina de carbón | cisterna alta | aguamanil de mármol | cómoda | gas | París: tout-à-l'égout (1894) |
| `entreguerras` | 1919 … 1959 | ídem (Varsovia 1942) | cama de hierro con edredón | Thonet | mesa redonda | cocina de carbón | cisterna alta compartida | palangana | alacena | lámpara de petróleo (cortes de luz del gueto) | — |
| `francia_rural` | 1850 … 1960 | Francia rural (Le Chambon) | cama de madera alta con edredón | banco | mesa larga | cuisinière de leña | la caseta del huerto | broc et cuvette | alacena | lampe à pétrole | granjas del plateau Vivarais-Lignon |
| `frente_1914` | 1914 … 1918 | El frente (Tregua 1914) | litera de tela metálica | caja de munición | mesa de caballetes | brasero de cubo | cubo de letrina | palangana de lata | estantería | vela | Imperial War Museums |
| `hispanoamerica` | 1500 … 1899 | Hispanoamérica (Angostura) | chinchorro (hamaca) | butaca de cuero | mesa larga | fogón de topias | letrina del patio | tinajero | arcón | vela | — |
| `maya_clasico` | 250 … 900 | Maya (Palenque) | banca de mampostería con petate | banca | mesa baja | k'oben (tres piedras) con comal | pozo | cántaro | tinajas | tea de ocote | Barnhart 2001; Stuart y Stuart 2008 |
| `norteamerica_victoriana` | 1840 … 1919 | Norteamérica (Dayton, Colorado Springs) | cama de hierro o latón | Windsor (respaldo prensado) | mesa redonda | cocina de hierro | la caseta (outhouse) | aguamanil | cómoda | queroseno | casa de los Wright (7 Hawthorn St.) |
| `norteamerica_moderna` | 1920 … 1979 | Norteamérica (Pasadena, Seattle) | cama tapizada con mesilla | cromada | fórmica | cocina de gas | tanque bajo | lavabo de pie | alacena | lámpara eléctrica | — |
| `sovietico` | 1960 … 1991 | Soviético (Chernóbil) | sofá-cama (divan-krovat) | taburete de cocina | fórmica | eléctrica (los pisos de Prípiat) | inodoro compacto | lavabo de pie | mueble de pared (stenka) | lámpara | — |
| `occidental_moderna` | 1960 … | Europa, Norteamérica, Hispanoamérica | cama tapizada | cromada | fórmica | eléctrica | tanque bajo | lavabo de pie | alacena | lámpara | — |

Los pueblos de hoy, con su entrada: Huerto y las áreas del santuario → `prehistoria`; Mesopotamia (Uruk, −3500) →
`mesopotamia`; Menfis → `egipto`; Egina, Crotona, Alejandría → `egeo`; Kapilavastu, Patanjali → `sur_asia`; Bagdad →
`islam_abasi`; Escútari → `otomano`; Woolsthorpe → `estuardo`; Berkeley → `georgiano`; Angostura → `hispanoamerica`; Sils
Maria → `alpino`; París 1898, Berna, Planta Atómica → `belle_epoque`; Varsovia (Irena Sendler) → `entreguerras`; Le Chambon →
`francia_rural`; Dayton, Colorado Springs → `norteamerica_victoriana`; Pasadena y el barrio de Seattle → `norteamerica_moderna`;
Kowloon (Bruce Lee) → `hongkong_tonglau`; Tregua 1914 → `frente_1914`; Chernóbil → `sovietico`. La casa de la escena de ciclo
de vida de los insectos-humanos es de Kapilavastu (`sur_asia`); la de los humanos, de 1950 (`norteamerica_moderna`).


## Cómo aplicarlas (receta para un microcosmos nuevo)

1. **Investiga** el lugar real: calles y edificios de la historia, sus materiales y colores, qué había dentro (cocinas,
   camas, retretes de la época). Fuentes en el doc del microcosmos.
2. **El piso**: en la historia, `Rect r = AreaMicrocosmos.FloorRectLocal(t);` (`t` = el transform que recibe la historia)
   da el rectángulo útil en sus locales; el claro de la historia es (0, 0) con ~26 m de radio y la entrada y el portal en
   (0, −14…−20): no se construye ahí. Reparte el pueblo por el resto en solares separados por calles de `Street`.
3. **Edificios**: `CivicBuilder.Building(parent, nombre, plantilla, título, pos, giro, tamaño mínimo, estilo, puestos…)`.
   Son abiertos (regla 2) y colocan sus muebles detallados con el paso (reglas 1 y 3). Una Casa ya trae sus necesidades
   (regla 4).
4. **Muebles sueltos** (en la calle, en el claro de la historia): `MicroFurniture.Build(parent, clave, posLocal, giro, rótulo)`.
5. **Valida**: `MicroBuildRules.Validate(raíz, "Microcosmos_X")` registra `[Reglas] Microcosmos_X: OK …` o las violaciones.
   `CivicBuilder.Civilize` ya lo llama al terminar. La prueba **63** (`MicroBuildRulesTest`) valida las escenas civilizadas.

## API

```csharp
// Assets/Scripts/Microcosmos/MicroBuildRules.cs (runtime)
public const float MicroBuildRules.Passage;        // 2,0 m
public const float MicroBuildRules.Street;         // 4,0 m
public const int   MicroBuildRules.TownFrom;       // −3500
public static bool MicroBuildRules.IsTownEra(int year);
public static List<string> MicroBuildRules.Validate(Transform scope, string label = null, bool needs = true, bool log = true);
public static float MicroBuildRules.GapXZ(Bounds a, Bounds b);
public static bool  MicroBuildRules.Covers(Transform scope, string need);

// Assets/Editor/AreaMicrocosmos.cs (editor)
public static Bounds AreaMicrocosmos.FloorBounds(Transform anyInMicrocosm);   // mundo; size.y = 0
public static Rect   AreaMicrocosmos.FloorRectLocal(Transform t);             // XZ en locales de t
```

```csharp
// Assets/Editor/MicroFurniture.cs (editor)
internal static Transform MicroFurniture.Build(Transform parent, string key, Vector3 pos, float yaw = 0f, string label = null, int year = 1950, Vector2? size = null,
                                           string region = null, MicroBody.Fit? body = null, IList<MicroBody.Fit> users = null);
internal static void MicroFurniture.Refit(Transform piece, string key, int year, string region, MicroBody.Fit body, IList<MicroBody.Fit> users = null);
internal static string MicroFurniture.NameOf(string key, MicroStyles.Entry e, MicroBody.Fit? body = null);   // «Charpai (de vientre)», «Kliné»…
internal static MicroBody.Fit? MicroFurniture.DefaultBody;   // el cuerpo de lo que se construye sin saber quién lo usa

// Assets/Scripts/Microcosmos/MicroStyles.cs, MicroBody.cs (runtime)
public static MicroStyles.Entry MicroStyles.For(int year, MicroStyles.Region region);   // y For(int, string)
public static MicroStyles.Region MicroStyles.RegionOf(string display);
public static MicroBody.Fit MicroBody.Of(GameObject g);   // OfSpecies(string, float), Human(float), Union(IList<Fit>)
internal static readonly string[] MicroFurniture.Keys;   // fogon cocina horno barra mesa mesa_laboratorio cama camilla inodoro lavabo
                                                          // lavadero silla banco estante armario pizarra banco_taller torno tunel muneco
                                                          // altar puesto granero torno_alfarero parapeto pozo telar tatami esterilla bancal
internal static Bounds MicroFurniture.LocalBounds(Transform piece);

// Assets/Editor/CivicBuilder*.cs (editor)
internal static int CivicBuilder.Year;                    // el año de la escena (lo pone Civilize; elige la versión de cada mueble)
internal static AreaFunctionSite CivicBuilder.Building(...);   // misma firma; ahora abierto, con Layout y necesidades
internal static Vector2 CivicBuilder.Layout(Transform b, List<Transform> pieces, Vector2 minSize, float zOffset);
internal sealed class CivicBuilder.Lots { Lots(Transform t, HashSet<Transform> skip = null); bool Free(Vector2 c, Vector2 size); Vector3? Take(Vector2 size, Vector2? near = null); void Mark(Rect r); }
internal static AreaFunctionSite CivicBuilder.Place(Transform t, Lots lots, AreaFunctionSite site, Vector2 near, List<Transform> placed = null);
internal static int CivicBuilder.FillTown(Transform t, Lots lots, Vector2 near, Style st, string title, int count);
internal static AreaFunctionSite CivicBuilder.EnsureNeeds(Transform t, Lots lots, Style st);   // en el solar libre más cercano al claro
internal static int CivicBuilder.Homes(Transform t, Lots lots, string display);   // regla 6: hogar para cada FolkLineage; devuelve las casas nuevas
internal static AreaFunctionSite CivicBuilder.NewHouse(Transform t, Lots lots, List<GameObject> fam, Style st, string title, int beds);
internal static Household CivicBuilder.MakeHousehold(AreaFunctionSite site, List<GameObject> fam, HashSet<Transform> used = null);

// Assets/Scripts/Body/Household.cs (runtime)
public static Household Household.Of(GameObject g);
public static List<string> Household.Check(Transform scope);   // «… sin hogar», «sin retrete», «1 cama(s) para 2 personas»…

// Assets/Editor/AreaMicrocosmos.cs: el sexo de un insecto-humano
internal static GameObject AreaMicrocosmos.Folk(..., Color? body = null, bool? female = null);   // null → FemaleByName → hash
internal static bool? AreaMicrocosmos.FemaleByName(string name);   // «Hannah», «Teano», «Vecina…» → true; «Cocinero…» → false
```

### Lo que hace `CivicBuilder.Civilize` al final de cada era

0. (2026-11-03) Con plano (`CivicBuilder.Plans`): `BuildPlan` levanta el paisaje y las calles del mapa ANTES que nada.
1. `Settle`: todo edificio abierto que choque (a menos de una calle) con otro o con la historia, o que se salga del piso, se
   corre al solar libre más cercano con su gente.
2. Eras avanzadas con relleno (`FillStyles`: Escútari, Dayton, Kapilavastu, Alejandría, Patanjali, Planta Atómica, Varsovia,
   Le Chambon, Uruk): 10 casas más de la era por el piso.
3. `Homes` (regla 6): un hogar para cada personaje —una casa del pueblo con camas libres o una casa nueva de la era en un
   solar libre—, las parejas juntas.
4. `EnsureNeeds`: los aseos de la era si falta alguna necesidad básica.
4b. (2026-11-03) `PlanCivic` (los edificios del pueblo, en su sitio del plano) va tras `Settle`; con plano, el relleno va a lo largo de las calles (`PlanFill`); al final, `Tag` en todos los edificios (reglas 8–9).
5. `MicroBuildRules.Validate` en el registro.

