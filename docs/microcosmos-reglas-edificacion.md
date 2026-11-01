# Reglas de edificación de los microcosmos y mesocosmos (2026-11-01)

Berón: «en el microcosmos de Bruce Lee la cocina y los muebles parecían cubos». Desde ahora todo edificio de un microcosmos o
mesocosmos (los de `CivicBuilder`, los pueblos nuevos de las figuras históricas y las historias que traen sus propias
estructuras) cumple estas **cinco reglas**. El código que las aplica y las comprueba:

| Pieza | Dónde | Qué hace |
|---|---|---|
| `MicroBuildRules` | `Assets/Scripts/Microcosmos/MicroBuildRules.cs` | Las constantes (paso, calles, alturas, era avanzada) y `Validate`. |
| `MicroPiece` | `Assets/Scripts/Microcosmos/MicroPiece.cs` | Marca la raíz de un mueble de varias piezas: `kind`, `need`, `walkable`. |
| `MicroFurniture` | `Assets/Editor/MicroFurniture.cs` | Los muebles detallados (cocina con sus utensilios, mesa con sillas, cama, retrete, lavabo…), reutilizando `KitchenProps`. |
| `CivicBuilder.Building` | `Assets/Editor/CivicBuilder.cs` | Edificio ABIERTO (suelo, sin muros ni techo) que coloca sus muebles con el paso de 2 m y añade las necesidades básicas. |
| `AreaMicrocosmos.FloorBounds` / `FloorRectLocal` | `Assets/Editor/AreaMicrocosmos.cs` | El rectángulo útil del piso, para que el pueblo lo llene entero. |

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

## Las cinco reglas

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

### 5. Eras avanzadas: el pueblo o la ciudad real ocupa el piso entero
Desde las primeras ciudades (`MicroBuildRules.IsTownEra(year)`: año ≥ −3500), el microcosmos **no** es el área del santuario
con sus muebles gigantes: `AreaMicrocosmos.Build` quita los muebles gigantes (deja el suelo del área y sus plazas
reservadas) y la historia **recrea el pueblo o la ciudad real** sobre todo el piso (`FloorBounds`). Investiga el lugar real
(planos, fotos, calles, materiales) y cítalo en el doc del microcosmos. Lo **habitan solo insectos-humanos civilizados**;
**flora y fauna salvajes solo si el lugar y la época las tenían** y salen en la historia (el puerto de Hong Kong con sus
milanos; un laboratorio de París de 1898 no tiene ninguna). En ese caso, la flora y fauna van fieles a la historia y al lugar
(`MicroBiome` del microcosmos).

---

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
internal static Transform MicroFurniture.Build(Transform parent, string key, Vector3 pos, float yaw = 0f, string label = null, int year = 1950, Vector2? size = null);
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

// Assets/Editor/AreaMicrocosmos.cs: el sexo de un insecto-humano
internal static GameObject AreaMicrocosmos.Folk(..., Color? body = null, bool? female = null);   // null → FemaleByName → hash
internal static bool? AreaMicrocosmos.FemaleByName(string name);   // «Hannah», «Teano», «Vecina…» → true; «Cocinero…» → false
```

### Lo que hace `CivicBuilder.Civilize` al final de cada era

1. `Settle`: todo edificio abierto que choque (a menos de una calle) con otro o con la historia, o que se salga del piso, se
   corre al solar libre más cercano con su gente.
2. Eras avanzadas con relleno (`FillStyles`: Escútari, Dayton, Kapilavastu, Alejandría, Patanjali, Planta Atómica, Varsovia,
   Le Chambon, Uruk): 10 casas más de la era por el piso.
3. `EnsureNeeds`: los aseos de la era si falta alguna necesidad básica.
4. `MicroBuildRules.Validate` en el registro.

**Excepción a la regla 2**: una barrera que ES la historia (el muro del gueto de Varsovia, cuyas salidas son los pasos de la
misión) va bajo un `MicroPiece` con `kind = MicroBuildRules.Historic` («historico»): no cuenta como muro. Pendiente de que
Berón lo confirme.
