# Microcosmos de las eras civilizadas: solo insectos, edificios reales y funciones reutilizables (2026-11-01)

Berón (2026-11-01, resumido): «en el microcosmos todos los personajes son insectos, debería haber un **bloqueo** para
evitar meter humanos; **Bruce Lee** debe convertirse en su versión insecto (¿una mantis? ¿un camarón pistola?); en los
microcosmos de **eras civilizadas** los insectos-humanos tienen **caras más humanas y cabello**, se crean **edificaciones
reales** investigadas (planos, imágenes) e **interactuables** (restaurantes, cocinas, dojos, salas de yoga, hospitales,
jardines…) reutilizando la lógica del santuario; ¿hay una **lista de funciones por área**? organiza el código para tenerla;
las hormigas recolectoras con guarida en todos los mapas son un **error**; **jornadas** para los insectos-humanos; cada
personaje es **el insecto que mejor le representa**; todos conviven **a tamaño parecido** y se **reproducen entre todos**
(el hijo puede salir como cualquier insecto del árbol genealógico)».

Se sigue la regla del proyecto: **investigación → plan → construcción**. Este documento responde a las preguntas, fija el
modelo y lleva la **lista de comprobación por era** (§9).

---

## 1. ¿Hay hoy una lista de funciones por área?

Sí, pero **repartida en tres sitios** y pensada solo para el santuario:

| Dónde | Qué es | Límite |
|---|---|---|
| `AreaWork.Catalog` (`Assets/Scripts/Progression/AreaWork.cs`) | El **catálogo de misiones de trabajo** por área (`WorkRoster.Mission`: id, área, título, horas, obligatoria, grupo, dirige, «también va a…»). 52 misiones fijas (Hospital, Cocina, Huerto, Sala de Yoga, Mecánica, Textil, Carne Cultivada, Cría, Gimnasio, Planta Atómica, Química, Biología, Central Nuclear, Guardia) + las del día (obras, material, elaboraciones de la cocina, frutales, fotobiorreactores, una reparación). | Solo texto y horas: no dice **qué se necesita** (fogón, cama, esterilla…) ni **qué pasa** al hacerla. |
| `WorkdayDirector.Apply` | El **efecto** de terminar una misión (`switch` por id: cortar guadua, sembrar micelio, iniciar/cosechar un lote, servir la carta, `prep_*`, `obra_*`). | Un `switch` cerrado: cada función nueva obliga a tocar el director. |
| `SanctuaryArea.availableTasks` (`AreaTask`) | Tareas sueltas por escena (inspector), del sistema viejo de promociones. | No se usa en la jornada. |

Y la **lógica de cada área** vive en sus componentes: cocina (`CookingStation`, `KitchenLarder`, `RestaurantService`),
hospital (`HospitalStation`, `InfirmaryGate`, `HealthRecord`, `VetCare`), yoga (`YogaSpells`, la clase de grupo en
`WorkdayDirector`), dojo (`JeetKuneDo`, `MartialForms`), huertos (`GardenPlot`, `FruitPlant`, `FruitGardener`).

## 2. El modelo: un REGISTRO DE FUNCIONES DE ÁREA (y por qué no solo listas)

Una lista por área se queda corta en cuanto el microcosmos necesita la misma función en otra época y otro edificio (la
cocina de Soyer en Escútari, el *cha chaan teng* de Hong Kong, el fogón de Kapilavastu). Se adopta un **registro de datos**
(`AreaFunctions`, `Assets/Scripts/Progression/AreaFunctions.cs`), con tres piezas:

1. **Función** (`AreaFunction`): una cosa que se hace, definida UNA vez y reutilizable:
   - `id`, `title`, `hours`, `mandatory`, `group`, `lead`, `roams`, `also` (lo que ya tenía la misión);
   - `kind` (Cocinar, Servir, Limpiar, Curar, Enseñar, Practicar, Cultivar, Construir, Fabricar, Operar, Vigilar,
     Estudiar, Cuidar, Comerciar, Rezar/Meditar): de él sale el gesto que hace el cuerpo y el verbo del rótulo;
   - `stations`: **qué hace falta** (claves de puesto genéricas: `fogon`, `mesa`, `barra`, `cama`, `esterilla`,
     `tatami`, `muneco`, `bancal`, `pozo`, `banco_taller`, `telar`, `pizarra`, `altar`…), que un edificio de cualquier
     época cumple con sus propios muebles;
   - `roles` (quién: cocinero, camarero, médico, maestro, alumno…), `needs` (la capacidad de era, `Eras.Capability`:
     sin `Fire` no se cocina, sin `Agriculture` no se siembra, sin `Vaccination` no se vacuna);
   - `effect` (opcional): qué pasa en el santuario al terminarla (lo que antes era el `switch` de `WorkdayDirector.Apply`).
2. **Área / edificio** (`AreaFunctions.Template`): un conjunto de funciones con nombre (`Cocina`, `Restaurante`,
   `Hospital`, `Sala de Yoga`, `Dojo`, `Huerto`, `Taller`, `Escuela`, `Templo`, `Mercado`, `Lavandería`, `Casa`…). Las
   áreas del santuario son plantillas con sus funciones **en el mismo orden que el catálogo de antes**
   (`AreaWork.Catalog` ahora sale del registro: mismo contenido, sin cambio de conducta; lo vigila `AreaFunctionRegistryTest`).
3. **Sitio** (`AreaFunctionSite` + `AreaFunctionStation`, `Assets/Scripts/Microcosmos/AreaFunctionSite.cs`): en una escena,
   un edificio dice qué plantilla es y cada mueble qué puesto (`fogon`, `cama`…) cubre. El registro le dice qué funciones
   se pueden hacer ahí **en ese año** (`AreaFunctions.ForSite(plantilla, año)`). Los puestos son **interactuables** (tecla de
   interactuar): el jugador hace la función (cocinar, servir, curar, practicar…) con su rótulo y su gesto.

**Escala**: el registro no lleva medidas. El edificio se construye a la escala de quien lo habita (a escala humana en el
santuario; en el microcosmos, a la de los insectos-humanos, ~1,6–1,9 m de pie, con puertas de 2,4 m y salas de 6–10 m).

**Añadir funciones** (por el santuario o por una historia): una línea `Def(...)` en `AreaFunctions` y, si es de un
edificio nuevo, añadir su id a la plantilla. Ni el director ni los builders cambian.

**Jornadas** (`MicroJornada`): igual que el santuario reparte misiones con `WorkRoster`, cada insecto-humano de una era
civilizada tiene una jornada por horas del reloj del juego (8–11 trabajo, 11–12 comida, 12–14 trabajo, 14–16 práctica,
luego casa), con las funciones de **su** edificio (el cocinero en la cocina, el médico en el hospital) y la comida en el
comedor/restaurante de la era. Va a su puesto, hace el gesto de la función y lo rotula («Soyer · Cocinar»).

## 3. El bloqueo de humanos

Regla: **en un microcosmos solo hay cuerpos de insecto**. Un único guardián, `InsectOnly`
(`Assets/Scripts/Microcosmos/InsectOnly.cs`):

- `InsectOnly.Admit(go)` convierte a quien entra: oculta su cuerpo humano de cubos (`CuerpoCubos`) y le pone su cuerpo de
  insecto (`InsectCast.SpeciesFor(nombre)`, a su estatura, con cara humanizada y cabello); si no se puede convertir, se le
  **rechaza** (se desactiva y se avisa). `InsectOnly.Release(go)` lo devuelve a su cuerpo al salir.
- Lo llaman: `MobWorldLoader` (el jugador al entrar y al salir), `MicroEra` (la raíz de todo microcosmos: al cargar y cada
  2 s convierte a cualquier humano que aparezca —personajes de historia, spawners, copias de la jornada—) y los builders
  (`AreaMicrocosmos.Build` revisa la escena antes de guardarla, así ninguna escena se guarda con humanos).
- Prueba: `InsectOnlyTest` (grupo **61**).

## 4. Bruce Lee como insecto

| Candidato | ¿Insecto? | A favor | En contra |
|---|---|---|---|
| **Mantis** (*Hierodula patellifera*, mantis gigante asiática, nativa de Hong Kong) | Sí (Mantodea) | El **estilo de la mantis** (*tánglángquán*) es un arte marcial chino real; la leyenda dice que Wang Lang lo creó en Shandong viendo a una mantis cazar. La mantis **espera quieta y golpea primero**: el golpe raptorial va de ~60 a ~290 ms y se ajusta a la velocidad de la presa (Rossoni y Niven 2020, *Biology Letters*) = **interceptar** (*Jeet Kune Do*, «el puño que intercepta»). Se balancea como una hoja al viento y «mira» moviendo la cabeza para medir la distancia (paralaje): su juego de pies. Delgada, ligera y explosiva, como él. Es de **Hong Kong**, donde se crió. | Con cuatro brazos (dos raptoriales y un par medio) el Wing Chun se adapta, no se copia. |
| Libélula | Sí (Odonata) | La mejor **interceptora** del mundo animal: calcula la trayectoria de intercepción con modelos internos (Mischiati et al. 2015, *Nature* 517:333). | No pelea con las patas: caza en vuelo. Y no tenemos aún su ficha morfológica. Queda como segunda opción. |
| Camarón pistola (Alpheidae) | **No: crustáceo** | Su pinza cierra tan rápido que lanza un chorro y una burbuja de cavitación que estalla con ~218 dB y un destello (Versluis et al. 2000, *Science* 289:2114; Lohse et al. 2001, *Nature* 413:477): una gran fuerza en un recorrido mínimo, como el **golpe de una pulgada**. | No es insecto (regla del microcosmos) y vive en el agua. |
| Galera / camarón mantis (Stomatopoda) | **No: crustáceo** | El golpe más rápido conocido: aceleraciones de ~10 000 g y ~23 m/s con un resorte de enganche (Patek et al. 2004, *Nature* 428:819). | Ídem: crustáceo marino. |

**Elección: mantis (*Hierodula patellifera*)**. Los dos crustáceos se quedan como **fauna de la bahía de Hong Kong** en la
escena «sé agua» (cuando haya ficha de camarón), como ejemplo de la fuerza corta.

Construido (§9): Bruce Lee es una mantis humanizada (pelo negro corto, cara humana, ropa negra de entrenamiento); sus
formas (`MantisFist`, mismas claves que `JeetKuneDo`: `wingchun`, `cadena`, `muneco`, `guardia`, `pasos`, `pulgada`,
`notas`, `agua`) mueven los **brazos raptoriales**: guardia de «rezo» en la línea central, golpes en cadena alternando
los dos brazos raptoriales, el muñeco con los dos pares, el balanceo de hoja como juego de pies, el golpe de una pulgada
(carga plegado → extensión en 0,1 s), las notas sentado y «el agua» con el balanceo lento. Yip Man, su maestro, es una
mantis vieja (mismo linaje).

## 5. Cada personaje y su insecto (`InsectCast`)

Ley ya escrita en `microcosmos-eras-and-observation.md` §1: **el animal se elige por su etología**, no al revés.

| Personaje | Insecto | Por qué |
|---|---|---|
| Kushal (el jugador) | Hormiga (mirmidón) | Canon: el mirmidón de Egina; el jugador entra como hormiga. |
| Bruce Lee, Yip Man | Mantis | §4. |
| Florence Nightingale | Luciérnaga | «La dama de la lámpara»: hacía la ronda de noche con su luz. |
| Siddhārtha (Buda), Sadhguru | Cigarra | Años bajo tierra y una muda: deja la vieja piel (el despertar). |
| Patanjali | Caracol | Quietud, «sthira-sukham āsanam»; la concha enrollada recuerda a Ādiśeṣa, la serpiente de la que la leyenda le hace encarnación. |
| Eratóstenes | Escarabajo pelotero | Se orienta por el sol y la Vía Láctea (Dacke et al. 2013, *Current Biology* 23:298): el que mide la Tierra con el sol. |
| Wilbur y Orville Wright | Saltamontes | Saltan y planean con alas: los que unieron salto, ala y control. |
| Irena Sendler | Tijereta | Una de las pocas madres insecto que guardan y cuidan a sus crías: escondió a niños. |
| Le Chambon | Abejas | La colmena entera protege: el pueblo escondió a miles. |
| Soldados de 1914 | Hormigas (dos colonias, dos colores) | Las hormigas hacen la guerra entre colonias; la tregua es lo humano. |
| Atlas | Escarabajo pelotero | El insecto más fuerte por su peso (*Onthophagus taurus*, Knell y Allen 2004). |
| Momo | Grillo | El bromista que canta. |
| Héspero | Luciérnaga | Vigía de estrellas y señor del fuego: la luz. |
| Ambrosio | Pulgón | Canon (Nasatya). |
| Medea, Ruth, Sakshi, Shanidar | Hormiga, hormiga, mantis, cochinilla | La tirana de las feromonas; la sembradora (hormigas que cultivan hongo); la observadora que espera inmóvil; el anciano (linaje antiquísimo). |
| Linduarda, Lizmibet | Luciérnaga, mariquita | Médicas: la luz; la mariquita protege el huerto. |
| La Magnate | Pulgón | Se divide en clones: los pulgones se clonan (partenogénesis). |
| Jefe de construcción | Termita | Las constructoras de montículos. |
| Goluis | Escarabajo pelotero | Fuerza (y cocina). |
| Panterilia | Hormiga león | Percepción, el acecho del león. |
| Gohageneis | Saltamontes | Nómada: la langosta migratoria. |
| Irosene | Araña | Tejedora (el textil). |
| Andrea | Mariquita | Elegante. |
| Cualquier otro | Una especie variada por su nombre, entre las de su bioma | — |

## 6. Tamaños parecidos y caras humanas (eras civilizadas)

- `CubeInsect.DressFolk(host, especie, estatura, nombre)`: viste la especie y **normaliza su estatura erguida** a la de la
  persona (1,6–1,9 m): una mariquita y una mantis miden lo mismo de pie y se miran a los ojos.
- **Cara humanizada**: placa de piel en la cara (del tono de `CubeHair.For(nombre)`), nariz, labios, orejas, cejas del color
  del pelo, y **cabello** con los mismos peinados de cubos del santuario (`CubeHair.Build`, en un marco escalado a la cabeza
  del insecto). Las antenas y mandíbulas siguen ahí: se reconocen como insectos.
- Fuera de las eras civilizadas (antes del fuego, la tribu) siguen los mirmidones de siempre.

## 7. Reproducción entre especies y genealogía

`InsectGenealogy` (`Assets/Scripts/Microcosmos/InsectGenealogy.cs`) + `FolkLineage` (componente):

- Cada insecto-humano lleva su **linaje**: su especie y la de sus antepasados con su generación (padres 1, abuelos 2,
  bisabuelos 3, tatarabuelos 4; hasta 5).
- Un hijo hereda los dos linajes (sus padres a 1, los antepasados de cada padre a +1). **Su especie se sortea** entre todas
  las del árbol, con peso ½ᵍ por antepasado (cada padre ½, cada abuelo ¼, cada tatarabuelo 1/16): lo más probable es salir
  como uno de los padres, pero **una mariquita y una araña pueden tener un hijo ciempiés** si había un ciempiés tatarabuelo
  (probabilidad = suma de los pesos de los ciempiés del árbol).
- En el microcosmos: `FolkCouple` (una pareja de una era civilizada) tiene un hijo cada pocos días de juego, con tope de
  hijos por pareja y de población por escena (sin bucles sin fin); el hijo nace de 0,6× de estatura, crece y sigue a su madre.
- (El ciempiés aún no tiene ficha morfológica: hasta entonces se dibuja como la especie más cercana. Pendiente con la
  habilidad `species-sheet`.)

## 8. Eras: con estructuras o sin ellas

`CivicEras.HasStructures(año)` = desde el Neolítico con aldeas (~−9500, primeras casas redondas del Natufiense / PPNA). En
las eras **sin estructuras** (antes del fuego, el fuego, el Paleolítico) se mantiene la **tribu con guarida** (refugio
natural). En las **civilizadas** se quita la cuadrilla de recolectoras con guarida genérica (era el error que Berón vio:
`AreaMicrocosmos.ForagingBand` se añadía a TODAS las historias) y se construyen sus **edificios reales**.

| Microcosmos | Escena | Era | ¿Estructuras? |
|---|---|---|---|
| Sakshi (el río) | `Microcosmos_Scene0_SakshiRio` | −1,1 Ma | No (tribu) |
| Ambrosio / Hospital | `Microcosmos_Scene1_Ambrosio`, `Microcosmos_Hospital` | −1 Ma | No (cueva natural) |
| Cocina | `Microcosmos_Cocina` | −400 000 (el fuego) | No (guarida) |
| Sala de Yoga, Textil, Mecánica | `Microcosmos_Textil`, `Microcosmos_Mecanica` | −40 000 | No (campamento) |
| Huerto | `Microcosmos_Huerto` | −9500 | Sí: primeras casas redondas y bancales |
| Mesopotamia | `MobWorld_Mesopotamia` | −3500 (Uruk) | Sí |
| Egina | `Microcosmos_Egina` | −1250 (mito) | Sí |
| Kapilavastu (Buda) | `Microcosmos_Buda` | −530 | Sí |
| Alejandría (Eratóstenes) | `Microcosmos_Eratostenes` | −240 | Sí |
| Patanjali | `Microcosmos_Patanjali` | −150 | Sí |
| Escútari (Nightingale) | `Microcosmos_Nightingale` | 1854 | Sí |
| Dayton (Wright) | `Microcosmos_Wright` | 1903 | Sí |
| Planta Atómica | `Microcosmos_PlantaAtomica` | 1905 | Sí |
| Tregua 1914 | `Microcosmos_Tregua1914` | 1914 | Sí (trincheras, refugios; sin edificios: es el frente) |
| Irena Sendler, Le Chambon | `Microcosmos_IrenaSendler`, `Microcosmos_LeChambon` | 1942 | Sí |
| Bruce Lee | `Microcosmos_BruceLee` | 1964 | Sí |

### Las estructuras de cada era (investigación y lo que se recrea)

| Era | Estructuras reales | Funciones (registro) | Fuentes |
|---|---|---|---|
| **Bruce Lee** (Hong Kong 1957 – Los Ángeles 1971) | La **escuela de Yip Man** (las clases eran en pisos y azoteas de edificios *tong lau* de Kowloon, salas pequeñas con el muñeco de madera); un **cha chaan teng** (casa de té de Hong Kong: barra, cocina vista, mesas con bancos de reservado); el **restaurante de Ruby Chow** en Seattle (donde trabajó de camarero y vivió encima); el **Jun Fan Gung Fu Institute** (local a pie de calle en University Way). | Dojo (`dojo_*`: guardia, golpes, muñeco, combate), Cocina y Restaurante (`coc_*`), Escuela. | HistoryLink 3999; Polly (2018) *Bruce Lee: A Life*; Lee, P. (2001) sobre los *tong lau* de Hong Kong [verificar]; fotos del Jun Fan Gung Fu Institute (HistoryLink). |
| **Escútari** (1854–55) | El **cuartel de Selimiye** (finales de la década de 1820, Mahmud II): un gran cuadrilátero con un patio central y **torres en las esquinas**; tres alas de tres plantas y la del este de dos por la pendiente; salas-pasillo con camas a ambos lados; Nightingale vivía en una torre (hoy museo). La **cocina de Alexis Soyer** (sus fogones de campaña, 1855) y la **lavandería**. | Hospital (`hos_*`, `enf_visita`), Cocina, Lavandería. | Wikipedia «Selimiye Barracks»; Royal Collection Trust, «The Military Hospital, Scutari» (grabado) y «Miss Nightingale's Tower» (J. Robertson, foto 1855); Soyer, A. (1857) *Soyer's Culinary Campaign*. |
| **Dayton** (Wright, 1892–1903) | La **Wright Cycle Company** en 1127 W. Third St.: edificio de **dos plantas y dos escaparates** de ladrillo (hoy en Greenfield Village, Dearborn, desde 1937); tienda delante, **taller al fondo** con el motor y las máquinas, imprenta arriba; ahí hicieron el túnel de viento de 1901. El **campamento de Kitty Hawk** (cobertizo y hangar de madera, 1902–03). | Taller (`tal_*`: reparar bicicletas, fabricar piezas, túnel de viento), Tienda. | The Henry Ford, «Wright Cycle Shop in Dayton, Ohio, circa 1910»; NPS, *Wright Cycle Company Historic Furnishings Report*; Wikipedia «Wright Cycle Company». |
| **Kapilavastu** (s. V a. C.) | **Tilaurakot**: ciudad en **cuadrícula** con calles, casas y tiendas de ladrillo, **puertas orientadas a los puntos cardinales**, muralla de ladrillo cocido sobre una empalizada de madera (s. VI a. C.), un complejo administrativo central y estanques revestidos de ladrillo (el tanque, s. III a. C.). | Palacio (Casa), Mercado, Jardín, Enfermería (los cuatro encuentros), Sala de meditación. | Universidad de Durham, proyecto Tilaurakot-Kapilavastu (desde 2014); Wikipedia «Tilaurakot»; Mukherji (1901). La identificación con Kapilavastu no es unánime (Piprahwa). |
| **Alejandría** (~240 a. C.) | El **Museion** y la **Biblioteca** (salas con columnas y estanterías de rollos, sin planos conservados: se recrea según Estrabón, *Geografía* XVII.1.8), una **casa griega con patio** y el **pozo de Siene** con el gnomon. | Escuela (estudiar, enseñar), Biblioteca, Casa (cocinar). | Estrabón XVII.1.8; Canfora (1990) *The Vanished Library*; Wikipedia «Library of Alexandria» [verificar planta]. |
| **Patanjali** (s. II a. C.) | **Āśrama** junto al río: cabañas de adobe y paja, un **patio de práctica** bajo el árbol, la cocina de leña y un huerto; cuevas de meditación excavadas en roca (como las de Barabar, s. III a. C., las más antiguas de la India). | Sala de Yoga (`yog_*`), Cocina, Huerto. | Wikipedia «Barabar Caves»; Olivelle (1993) *The Āśrama System*. |
| **Mesopotamia** (Uruk, ~3500 a. C.) | Casas de **adobe** con patio, el recinto de **Eanna** y el **Templo Blanco** sobre la terraza de Anu, talleres, graneros. | Casa, Granero, Templo, Taller (cerámica, Pottery), Mercado. | Wikipedia «Uruk»; Liverani (2006) *Uruk: The First City*. (La escena es de `MobWorldSceneBuilder`; pendiente.) |
| **Egina** (mito, ~1250 a. C.) | Megaron micénico, almacenes, el altar de Zeus Helanio en el monte Oros. | Casa, Templo, Granero. | Ovidio, *Met.* VII; Wikipedia «Aegina» (Kolonna). (Escena de `MythSceneBuilder`; pendiente.) |
| **Tregua 1914** | Trincheras en zigzag con parapeto, **refugios** (*dugouts*) con tablas y sacos, alambradas. | Cocina de campaña, Puesto de socorro, Vigilar. | Imperial War Museums, «The Christmas Truce». |
| **Planta Atómica** (1905) | Woolsthorpe Manor (Newton), el **cobertizo-laboratorio** de los Curie en la rue Lhomond, la **Oficina de Patentes de Berna** (Einstein). | Laboratorio (Operar, Estudiar), Casa. | (Escena de `PhysicsSceneBuilder`; pendiente.) |
| **Irena Sendler / Le Chambon** (1942) | Varsovia: muro del gueto, el juzgado de la calle Leszno (la salida), casas; Le Chambon: la iglesia protestante, la escuela Cévenole, granjas de piedra con tejado de lauzas. | Casa, Escuela, Templo, Hospital. | (Escenas de `CompassionSceneBuilder`; pendiente.) |

## 9. Lista de comprobación por era

| Era | Humanos bloqueados | Elenco como su insecto (tamaño parecido, cara humana, pelo) | Sin recolectoras genéricas | Edificios reales e interactivos | Jornadas | Familias (genealogía) |
|---|---|---|---|---|---|---|
| Bruce Lee | ✓ | ✓ (Bruce mantis) | ✓ | ✓ escuela de Yip Man, cha chaan teng, restaurante de Ruby Chow | ✓ | ✓ |
| Escútari (Nightingale) | ✓ | ✓ | ✓ | ✓ ala del cuartel (sala de camas), torre, cocina de Soyer, lavandería | ✓ | ✓ |
| Dayton (Wright) | ✓ | ✓ | ✓ | ✓ Wright Cycle Co. (tienda + taller), cobertizo de Kitty Hawk | ✓ | ✓ |
| Kapilavastu (Buda) | ✓ | ✓ | ✓ | ✓ casas de ladrillo en cuadrícula, puerta, mercado, estanque | ✓ | ✓ |
| Alejandría (Eratóstenes) | ✓ | ✓ | ✓ | ✓ Museion (sala de columnas con rollos), casa con patio | ✓ | ✓ |
| Patanjali | ✓ | ✓ | ✓ | ✓ āśrama: cabañas, patio de práctica, cocina, huerto | ✓ | ✓ |
| Tregua 1914 | ✓ | ✓ (dos colonias de hormigas) | ✓ | ✓ refugios (dugouts) y cocina de campaña | ✓ | — (frente de guerra) |
| Huerto (−9500) | ✓ | — (mirmidones) | se mantiene (transición) | pendiente: casas redondas | — | — |
| Mesopotamia, Egina, Planta Atómica, Irena Sendler, Le Chambon | ✓ (el guardián es global) | pendiente | ✓ (ya no se añaden) | pendiente | pendiente | pendiente |
| Eras sin estructuras (Sakshi, Ambrosio, Cocina, Paleolítico) | ✓ | mirmidones (canon) | se mantiene: es correcto | no aplica | — | — |

## 10. Qué probar en Unity (pendiente: no se ha ejecutado en el Editor)

- Generar: «Tools/Cold Sanctuary/Build Microcosmos de historias» (todas) o «…: Bruce Lee (Dojo)» (con la escena principal
  abierta) y «Build Sala de Eras».
- Pruebas: grupo **61** (`InsectOnlyTest`, `AreaFunctionRegistryTest`) y grupo **60** (`BruceLeeStoryTest`, ahora con Bruce
  mantis). Las del santuario (jornada, grupos 50–58) deben seguir igual: el catálogo es el mismo.
- A ojo: entrar por la máquina del Gimnasio en «Simulación del Dojo»: Kushal debe llegar como hormiga y volver humano.
