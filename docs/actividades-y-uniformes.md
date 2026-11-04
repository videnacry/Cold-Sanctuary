# Actividades de verdad, la orquesta de la comida y los uniformes

Berón (2026-11-04): «en la hora de la comida me ha parecido que los humanos solo se sientan en la mesa? comer debe tener su propia
orquesta/animaciones/objetos, me da la impresión de que pasa lo mismo con las demás actividades de la jornada? parece que solo van al
piso que les toca y no hacen nada, deberían realizar las actividades … sería posible que los humanos se coloquen uniformes para las
tareas que vayan a realizar de la jornada?, estos arreglos … también aplican a los insectos-humanos … deben tener sus
orquestas/animaciones para completar sus tareas/jornada/ocio/recuperación».

Rama `claude/edif-actividades`. Código en `Assets/Scripts/Activities/` (`ActivityKit`, `ActivityPerformer`, `Performances`,
`MealRitual`, `Uniforms`, `UniformWearer`, `UniformDirector`) y `CubeInsect.Uniform.cs`. Prueba: `ActividadesTest` (grupo 96).
Solo código: **sin probar en vivo** (lo prueba el probador único).

## 1. Auditoría (estado el 2026-11-04, antes del cambio)

«Hace» = el personaje hace la tarea en su sitio (postura o gesto propio, objetos en las manos, efecto u orquesta). «Llega y se queda» =
anda hasta el punto del área (`WorkdayDirector.WorkPoint`) y se queda de pie sin hacer nada.

### 1.1 Jornada del santuario (catálogo diario de `AreaFunctions` y misiones del día)

| Misión(es) | Quién la conduce | Antes | Ahora |
|---|---|---|---|
| `enf_iniciacion` | `NewcomerInitiator` (orquesta: análisis, vacuna, virtualización) | **Hace** (orquesta) | igual; en la espera, prepara la vacuna y la ficha |
| `enf_visita`, `hos_urgencias`, `hos_uci`, `hos_quirofano` | jornada → puesto médico (`HospitalStation.PostFor`); Linduarda, su conducta | Llega y se queda (Linduarda sí atiende) | **Hace**: auscultar (fonendo), temperatura, vendar, vía, monitor, operar (bisturí), suturar, anotar |
| `hos_veterinaria`, `enf_animales` | jornada + `VetCare` (lleva la ración a los ingresados) | Lleva la ración; si no, se queda | **Hace**: explorar, vacunar, dar de comer, anotar |
| `enf_inventario` | jornada | Llega y se queda | **Hace**: desinfectar (espray), reponer (caja), contar |
| `coc_servicio`, `coc_obrador`, `prep_*`, `coc_limpieza`, `coc_atender`, `coc_sala`, `coc_comedor` | `RestaurantService` (brigada) | **Hace** (comandas, platos, mesas) con un solo gesto de brazos y las manos vacías | igual + **utensilio y efecto por papel** (cucharón y vapor, amasar con harina, estropajo y espuma); sin servicio, las actuaciones propias |
| `hue_riego`, `hue_cosecha`, `hue_visitantes` | jornada | Llega y se queda | **Hace**: regar (regadera, gotas), deshierbar, cavar (azada, tierra), cortar (tijeras), cesta |
| ronda de frutales | `FruitGardener` | **Hace** (recorre los techos, poda, riega) | igual |
| `yog_guia`, `yog_demo`, `yog_corrige_*` | `YogaClass` / secuencia | **Hace** (la clase) | igual |
| `yog_sala`, `yog_aseos` | jornada | Llega y se queda | **Hace**: barrer, colocar esterillas, limpiar accesorios, fregar |
| `mec_vehiculos`, `mec_maquinas` | jornada | Llega y se queda | **Hace**: linterna, llave (chispas), martillo, anotar |
| `tex_tejer`, `tex_coser` | jornada | Llega y se queda | **Hace**: telar (ovillo, hilos), teñir, coser (aguja), cortar (tijeras) |
| `lab_biorreactor`, `lab_iniciar`, `lab_cosechar`, `lab_limpieza` | jornada (+ efecto en `CultivatedMeatLab`) | Llega y se queda | **Hace**: consola, pipeta y matraz, sembrar, cosechar y dar forma, esterilizar |
| `con_almacen`, `obra_material_*`, `obra_trabajo_*`, `mat_*` | jornada (+ efecto en la obra) | Llega y se queda | **Hace**: cajas, sacos, martillar y atar cañas, revocar con llana, machete, barro, micelio, trenzar |
| `cri_tomas`, `cri_recintos`, `cri_vinculo` | jornada | Llega y se queda | **Hace**: biberón (corazones), pesar, pizarra, rastrillar, paja, bebedero, acariciar, leer, cepillar |
| `gym_limpieza`, `gym_mantener` | jornada | Llega y se queda | **Hace**: desinfectar, fregar, revisar poleas, ajustar (llave), engrasar |
| `pla_*`, `nuc_turno`, `fis_experimentos` | jornada | Llega y se queda | **Hace**: consola, indicadores, dosímetro, ronda con linterna, láser, montaje |
| `qui_sintesis`, `qui_residuos`, `bio_cultivo`, `bio_pcr` | jornada | Llega y se queda | **Hace**: pesar, remover, pipetear, microscopio, termociclador, bidones |
| `ifo_sysadmin`, `ifo_refrigeracion`, `ifo_energia` | jornada (+ efecto en `AiCluster`) | Llega y se queda | **Hace**: consola, paneles, fugas con linterna, filtros, baterías |
| `aul_clase`, `aul_estudio` | jornada | Llega y se queda | **Hace**: pizarra con tiza, apuntes, leer |
| `gua_carnivoros` | `CarnivoreKeeper` (patrulla y raciones) | **Hace** (anda y lleva raciones; sin gesto) | igual; en sus pausas, vigilar con linterna |
| Comida 11–12 | `DineSpell` | Pide, se sienta… **y se queda sentado** | **Orquesta de la comida** (§2) |
| Entrenamiento: gimnasio | `GymSession` / `GymFit` (otro trabajador) | **Hace** | igual + polo del gimnasio |
| Entrenamiento: yoga (Ashtanga) | `YogaClass` | **Hace** | igual |
| Entrenamiento: ballet, calistenia, dojo | jornada | Llega y se queda | **Hace**: barra, dominadas, golpes y formas |

### 1.2 Horario libre (`LeisureDirector`)

| Actividad | Antes | Ahora |
|---|---|---|
| Pesas, yoga, Ashtanga, baile, artes marciales | **Hace** (postura A↔B: sentadillas, giro, meditar) | igual |
| Guerra de almohadas, pijamada | **Hace** (`PillowFight`, `Sleepover`) | igual |
| Descansar en casa | **Hace** (tumbado en su cama) | igual |
| Leer en la arboleda, manualidades, tertulia | Solo **sentado** | **Hace**: leer (libro, té), bordar y tejer (aguja, ovillo), charlar con su taza y beber |
| Pasear y cuidar el huerto | Solo **de pie** | **Hace**: regar y mirar las plantas |
| Jugar con las crías | Solo **de pie** | **Hace**: acariciar, jugar (corazones) |
| (micro) dojo, yoga, templo, huerto, escuela, mercado, tertulia, descanso, cuidar | Gesto de brazos del insecto (`AreaFunctionStation.Gesture`), sin objetos | **Hace**: su actuación (libro, cesta, taza, rezar, practicar…) con objetos y efecto |

### 1.3 Recuperación

| Qué | Antes | Ahora |
|---|---|---|
| Hatha y meditación junto al linga, Ashtanga autoguiado | **Hace** (meditar / ejercicio) | igual |
| Meditar del maestro de yoga en su tiempo libre | **Hace** (meditación junto al linga) | igual |
| Dormir, baño, lavarse (casa y santuario) | **Hace** (`SleepSpell`, `ToiletSpell`, `HomeNeedSpell`) | igual |
| Comer en casa (`HomeNeedSpell`) | Sentado ~20 min y **se sacia de golpe** al final | **Orquesta de la comida** (plato de la despensa → fregadero) |
| (micro) templo, yoga | Gesto de rezo del insecto | rezar con las manos juntas (bucle de su clase) |

### 1.4 Jornada del insecto-humano (`MicroJornada`)

| Qué | Antes | Ahora |
|---|---|---|
| Trabajo y práctica (todas las funciones del registro) | Gesto de brazos por clase (empujar / forma marcial), **sin objetos ni efecto** | Actuación de su función: la propia si la tiene (las del santuario), si no **el bucle de su clase** con la herramienta **de su era** (sin tabletas antes de 1900, vendas antes de 1850…) y su efecto; **delantal de su oficio** |
| Comida del mediodía en el comedor de la era | De pie junto a la mesa; a los 20 min se sacia de golpe | **Orquesta de la comida**: mostrador (bandeja) → mesa → cuchara y bocados → devolver al mostrador; el plato de su época |
| Casa (descansar, cuidar) | Gesto | quieto descansando / cuidar (bucle de su clase) |

## 2. La orquesta de la comida (`MealRitual`)

Los pasos, en orden (los registra `MealRitual.steps`; la prueba exige este orden):

1. **bandeja** — recoge la bandeja con su plato (la ración real: `KitchenProps.Dish` con «Comida» y «Guarnición») y un vaso de agua; la
   lleva delante del pecho con los brazos sujetándola (en el restaurante, en la barra; en casa, solo el plato de la despensa; en el
   microcosmos, en el mostrador del comedor —la estación `barra`, `mostrador`, `granero`, `fogon` o `puesto`— o se la sirven en la mesa).
2. **sentarse** — el plato y el vaso pasan a la mesa, delante de él (la altura del tablero por un rayo hacia abajo; si no hay colisionador,
   por los renderizadores de la mesa; si no, 0,75 de su estatura); la bandeja vacía, al lado.
3. **cubiertos** — tenedor en la derecha y cuchillo en la izquierda (el insecto-humano, cuchara).
4. **bocado** — 9 bocados repartidos por la comida (~20 min de juego): el brazo baja al plato y sube a la boca; en cada bocado la comida
   **mengua** (escala ×(bocados que quedan)), la guarnición desde la mitad, migas al morder; el hambre baja `meal/9` por bocado
   (`BodyMetabolism.Feed`) y, si tiene `Metabolism`, absorbe los nutrientes del plato por sus palabras (`MealRitual.Nutrients`).
5. **beber** — tras cada tercer bocado: el vaso a la boca con la izquierda, la cabeza un poco atrás.
6. **recoger** — de pie: plato vacío, vaso y cubiertos a la bandeja.
7. **devolver** — la lleva al punto de devolución (el extremo de la barra del restaurante, `DineSpell.ReturnPoint`; el lavabo de casa;
   el mostrador del comedor de la era) y la deja; se recoge sola a los 4 s.

Topes: la vuelta a la barra 60 s (`DineSpell.returnTimeout`), al fregadero de casa 20 s; si una comida se corta (desayuno, necesidad,
la historia), `UniformDirector` llama a `MealRitual.Sweep` y la vajilla desaparece a los 6 s.

## 3. Hacer la actividad (`ActivityPerformer` + `Performances`)

- Una **actuación** es una partitura de pasos que se repite: postura base (de pie, sentado, meditando), gesto A↔B por hueso
  (`ActivityKit.Arms`), utensilio en cada mano (`ActivityKit.Prop`: 44 piezas de cubos), duración, efecto (`ActivityKit.Fx`: vapor,
  chispas, polvo, gotas, hojas, tierra, papel, burbujas, hilos, corazones, brillo de pantalla, tiza, migas, sudor) y acción al empezar.
- Quien la conduce llama a `ActivityPerformer.Keep` **cada fotograma** mientras el personaje está en su sitio; si deja de llamarlo
  (`KeepAlive` = 0,6 s) se para solo, suelta los utensilios y se pone de pie. Nadie se lleva una escoba al comedor.
- Conducen: `WorkdayDirector.Drive` (al llegar al punto del área: `Perform`), `LeisureDirector.Drive`, `MicroJornada.Update` y la brigada
  del restaurante (`RestaurantService.Gesture` → `Performances.ForService`).
- Insecto-humano: brazos con `CubeInsect.Push` / `Martial` (empujar, golpe, fino, alzar, quieto) y la herramienta en la punta de su
  brazo delantero, a su escala.

### Qué actúa ahora y qué queda pendiente

- **Con pasos propios: 51 de las 55 del catálogo diario** (las 4 restantes son la clase de Hatha —guía, demostrador y dos
  correctores—, que ya hace `YogaClass`), **las 4 del día** (`mat_*`) y **las 3 de prefijo** (`obra_material_*`, `obra_trabajo_*`,
  `prep_*`). Ver `Performances.OwnMissions`.
- **Entrenamientos sin sistema propio:** ballet, calistenia, dojo.
- **Ocio:** leer, manualidades, tertulia, huerto, crías (santuario); escuela, mercado, cuidar (micro).
- **Bucle genérico (PENDIENTE de pasos propios):** las ~70 funciones de los microcosmos (`AreaFunctions` sin `S(...)`: comisaría,
  bomberos, museo, alfarería, granero, trinchera, oficina de patentes, taller de papeles, orfanato, escuela, mercado, templo…) hacen
  el bucle de su clase (`Performances.ForKind`): Cocinar (cucharón + cuchillo), Servir (bandeja + bayeta), Limpiar (escoba + cubo),
  Curar (venda + remedio + anotar), Enseñar (tiza), Practicar (golpes), Meditar/Rezar (manos juntas), Cultivar (azada, regadera),
  Construir (martillo, caja), Taller (llave, martillo), Operar (consola), Vigilar (linterna), Estudiar (libro, pluma), Cuidar, Vender.
  Siguiente paso: pasos propios para las funciones de los edificios del pueblo más frecuentes (mercado, alfarería, bomberos, comisaría).
- **Pendiente:** el jugador (lo lleva el trabajador de la interacción E); los turistas del restaurante (comen con el gesto viejo de
  `RestaurantService`); los clones de la Magnate (sin esqueleto); los pacientes que el personal trata no reciben `TreatPatient` desde
  la actuación (solo el submarino y la interacción).

## 4. Los uniformes (`Uniforms`, `UniformWearer`)

- **Cuándo:** al llegar a su tarea (en el edificio de su área o a ≤ 6 m de su punto; la guardia y la ronda de frutales, en cuanto empieza)
  se pone el de esa tarea; al acabar (libre, la comida, una tarea sin uniforme) vuelve a su ropa. Nadando, **neopreno** con prioridad
  (también el jugador: es solo ropa; `UniformDirector.PlayerToo`); al salir del agua, lo que tocaba.
- **Identidad:** solo se recolorean los cubos de la ropa (torso, escápulas, brazos, cadera, piernas, pies; las manos solo con guantes) y
  los relieves de músculo/grasa que llevan su material; la ropa de diario que asoma («Detalle»: collares, chalecos, cadenas; tatuajes bajo
  la manga) se oculta y vuelve al quitárselo; la cabeza, el pelo y la cara no se tocan; el gorro va encima del peinado (`HeadTop`).
- **Reparto** (`Uniforms.ForWork`):

| Tarea | Uniforme |
|---|---|
| `coc_servicio`, `coc_obrador`, `coc_limpieza`, `prep_*` | chaquetilla blanca, delantal y gorro de cocina |
| `coc_atender`, `coc_sala` | camisa blanca, delantal corto negro y acreditación |
| `coc_comedor`, `*_limpieza` (no lab), `yog_sala`, `yog_aseos`, `gym_limpieza` | casaca de limpieza y guantes de goma |
| `hos_quirofano`, `hos_uci`, `hos_urgencias`, `enf_inventario` | pijama verde de quirófano, gorro y guantes |
| `enf_visita`, `enf_iniciacion` y el resto del hospital | bata blanca con faldones, fonendo y acreditación |
| `hos_veterinaria`, `enf_animales` | pijama azul de veterinaria y fonendo |
| `hue_*`, `mat_guadua` | peto vaquero, camisa roja, guantes verdes y sombrero de paja |
| `obra_*`, `con_*`, `mat_bahareque` | chaleco naranja con bandas reflectantes, casco amarillo, guantes, botas y cinturón de herramientas |
| Entrenamiento en el gimnasio | polo azul con cuello y logo, pantalón corto |
| `lab_*`, `qui_*`, `bio_*`, `fis_*`, `mat_micelio` | bata de laboratorio, gafas y guantes de nitrilo |
| `mec_*`, `gym_mantener` | mono de mecánico y cinturón de herramientas |
| `pla_*`, `nuc_*` | mono blanco, casco blanco, dosímetro y guantes |
| `tex_*`, `mat_cuerda` | delantal de taller y cinta métrica |
| `cri_*` | camisa caqui y delantal de cuidador |
| `gua_*` | camisa caqui y gorra |
| `ifo_*` | polo oscuro con acreditación |
| Nadar | neopreno negro con franjas azules |
| Comida, yoga, aula, libre | su ropa |

- **Insecto-humano** (`Uniforms.DressInsect`, `CubeInsect.WearWorkApron`): delantal y peto con su cinta sobre el torso centauro, el tocado
  del oficio y, para laboratorio y medicina modernos, manguitos. Tejido de la era: antes de 1800 lino crudo o cuero; el cirujano antes
  de 1850, mandil oscuro (antes de la asepsia); cofia de enfermera entre 1850 y 1950; algodón blanco o de color después.

## 5. Reglas (para el manual de diseño)

| # | Regla | Se comprueba en |
|---|---|---|
| J18 | **Comer es una orquesta**: bandeja → sentarse → cubiertos → bocados (la comida mengua, la saciedad va por bocado) → beber → recoger → devolver la bandeja; igual en el restaurante, en casa y en el comedor de la era (insectos-humanos con cuchara). | `ActividadesTest` (96) |
| J19 | **Nadie llega y se queda quieto**: en su puesto, cada misión, actividad libre o entrenamiento se HACE (postura, herramienta en la mano, gesto y efecto visible); lo que no tiene pasos propios hace el bucle de su clase y se lista como pendiente. | 96 (N misiones a los T s de llegar) |
| J20 | **Uniforme por tarea, identidad intacta**: al llegar a la tarea se pone su uniforme y al acabar vuelve a su ropa; nunca cambian el pelo, la cara ni la piel; nadando, neopreno. | 96 |
| J21 | **El insecto-humano trabaja con lo de su era**: herramientas y delantal de la época del microcosmos (sin tabletas antes de 1900). | 96 (comida y trabajo del insecto) |

## 6. Para probar en vivo

1. Sin reconstruir nada para la comida, las actuaciones y los uniformes del santuario: es todo en tiempo de juego. Basta Play en
   SampleScene (jornada de un día) y un microcosmos civilizado ya construido (p. ej. la Cocina o el Hospital), de uno en uno.
2. Grupo **96** (`ActividadesTest`); y por regresión **50** (`JornadaTest`: la comida tarda un poco más por la vuelta a la barra), **28**,
   **65** (horario libre), **41** (iniciación en la cocina: la brigada ahora usa la actuación), y el pase de los microcosmos (60, 62, 66).
3. Fotos: (a) el comedor a las 11:20 — bandejas, platos en la mesa, cubiertos en la mano, un plato a medias; (b) la barra a las 11:50 con
   bandejas devueltas; (c) la cocina con chaquetillas y gorros; (d) el quirófano (pijama verde) y una sala de camas (bata y fonendo);
   (e) el huerto (peto, sombrero, regadera con gotas); (f) la obra (chaleco, casco, martillo con polvo); (g) un laboratorio (bata,
   gafas, pipeta); (h) alguien nadando en la raíz de un árbol (neopreno); (i) un insecto-humano en el comedor de su era con la cuchara y
   otro trabajando con su delantal; (j) la arboleda en el horario libre (leyendo con libro y taza).
