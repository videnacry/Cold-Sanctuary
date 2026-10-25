# Casos de uso y cobertura de pruebas

> Catálogo de lo que el jugador (y la simulación) puede hacer en el juego, con la prueba automática que lo comprueba.
> Generado el 2026-10-25 a partir de `docs/` (testing-checklist, testing-recent-features, checklist, jornada-y-rotacion,
> hospital, restaurant-plan, edificios-investigacion, microcosmos-historias, transporte-alfombras…) y de las sondas
> `TestProbe.Check/Greater(...)` de las unidades de prueba. Cada fila cita el archivo y la descripción de la sonda.

## Resumen

- **Casos de uso: 255.** Con prueba automática: **179** (70 %). Sin prueba: **76**. Las áreas peor cubiertas: microcosmos e historias (10 de 13 sin prueba), hospital (9 de 13) e investigación (6 de 13).
- Muchas pruebas de escena solo comprueban que algo **existe** (puestos, mesas, máquinas); lo marcamos igual como
  cubierto, pero el comportamiento al usarlo suele quedar sin prueba (filas aparte).
- `ReachGoalMission` no corre en el TestRunner: informa por `TestProbe` solo cuando alguien juega esa misión.

### Las 10 pruebas que más faltan

1. **CU-HOS-04/05** — Tratar a un paciente en un puesto del hospital (`HospitalStation`): urgencias estabiliza, quirófano cura solo con anestesista.
2. **CU-HOS-03** — Linduarda hace análisis y vacuna al recién llegado y, al terminar la iniciación, la salida queda libre.
3. **CU-COC-09** — Cocinar un plato entero con hechizos (`walkTo → take → putIn → beat → step`) y que sume al stock.
4. **CU-ARE-16** — Jugar una misión en la máquina de virtualización paso a paso (F) y que aplique su efecto (p. ej. la obra avanza).
5. **CU-JUG-12** — Volar en alfombra: tocarla, elegir destino, ir sentado y bajarse en la otra pista.
6. **CU-HEC-05** — `use`/`walkTo` con alcance: lejos falla («no alcanzo…»), cerca avanza el paso.
7. **CU-MIC-02/03** — Completar una `StoryMission` (Talk/Reach/Collect/VisitAll/Hold) y recibir XP de su marga y la `HistoryCard`.
8. **CU-ARE-13** — La Magnate teletransporta de vuelta al que abandona su puesto (hoy solo se prueba la medida del puesto).
9. **CU-CON-09** — Al terminar una obra (ballet, dojo, calistenia) se abre su entrenamiento y entra en la rotación.
10. **CU-REN-01** — Dar Play sin pedir pruebas deja el TestRunner apagado y el jugador se mueve desde el primer segundo.

### Cómo se corren hoy las pruebas

- En el Editor de Unity, en Play: menú **«Tools/Cold Sanctuary/Jugar con las pruebas»** (o el comando `test` del buzón).
  Crea `Temp/run-tests`; desde **2026-10-24** el `TestRunner` solo corre si se pide (antes corría en cada Play).
- `ColdSanctuary.TestRunner` vive en `TestRunner_AUTO` (lo crea `Assets/Editor/SampleSceneBuilder.cs`), reúne las
  `ITestUnit` y las corre por grupos en serie; cada sonda escribe `[TEST] PASS/FAIL` en `Editor.log` y al final un `[TEST] TOTAL`.
- **No hay CI**: nadie corre las pruebas en cada PR; dependen de que alguien dé Play con ellas en el Editor.

Las rutas de las pruebas: `Assets/Scripts/Tests/*.cs` (Checklist*, SanctuaryLife, Islands, BodyShape, GardenVisitor,
EraSpecies) y `Assets/Scripts/*.cs` (FaunaChecks, TraceFieldTest, EmergenceAuto, GrimoireTest, EstrusTest, LifeCycleAuto,
SicknessTest, StarvationTest, SleepTest, RiverCurrentTest, DietPreferenceTest, ObservationTest, StateSpellsTest,
IdealStateTest, SensesTest, YogaTest, BodyMetabolismTest, ChecklistAuto, ReachGoalMission).

## Jugador y controles

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-JUG-01 | Como jugador quiero moverme con WASD para recorrer el santuario | ReachGoalMission «el jugador se movió (hubo WASD)» |
| CU-JUG-02 | Como jugador quiero llegar a un objetivo marcado para completar la misión WASD | ReachGoalMission «llegó al refugio» |
| CU-JUG-03 | Como jugador quiero cambiar entre 1ª y 3ª persona (V) para ver a mi personaje entero | sin test |
| CU-JUG-04 | Como jugador quiero saltar un escalón sin hacerme daño para moverme con soltura | SanctuaryLifeTest «saltar un escalón no hace daño» |
| CU-JUG-05 | Como jugador quiero que una caída sobre hielo duela para que la altura importe | SanctuaryLifeTest «una caída de 7 m sobre hielo hace daño» |
| CU-JUG-06 | Como jugador quiero que el musgo amortigüe mis caídas para sobrevivir a una plataforma | SanctuaryLifeTest «sobre el colchón de musgo el golpe se amortigua» |
| CU-JUG-07 | Como jugador quiero que las barreras invisibles no me dejen caer de las plataformas | sin test |
| CU-JUG-08 | Como jugador quiero subir y bajar escaleras verticales para llegar a la casa del árbol | IslandsTest «bajar la escalera lo devuelve al suelo» |
| CU-JUG-09 | Como jugador quiero nadar entre casquetes para ir a cualquier isla | IslandsTest «camino completo del hub a {name} (cruzando a nado)» |
| CU-JUG-10 | Como jugador quiero trepar por superficies para explorar en vertical | sin test |
| CU-JUG-11 | Como jugador quiero encontrar una pista de alfombras encima de cada plataforma para viajar | SanctuaryLifeTest «hay pistas de alfombras voladoras (una encima de cada plataforma)» |
| CU-JUG-12 | Como jugador quiero tocar una alfombra, elegir destino y volar sentado para cambiar de grupo de plataformas | sin test |
| CU-JUG-13 | Como jugador quiero cruzar a pie el puente restaurante–hospital para no depender de la alfombra | SanctuaryLifeTest «el restaurante y el hospital están unidos por un puente de 8 m» |
| CU-JUG-14 | Como jugador quiero poseer a otro ser para manejarlo yo y que su IA calle | ChecklistFaunaTest «[1238] al poseer (PlayerBrain de mayor relevancia) su IA se SUPRIME» |
| CU-JUG-15 | Como jugador quiero soltar la posesión para que el ser vuelva a su vida | ChecklistFaunaTest «[1238] al soltar, la IA retoma el mando» |
| CU-JUG-16 | Como simulación quiero que, en apremio alto, el cuerpo tome el mando sobre las órdenes del jugador | ChecklistAuto «apremio alto → el CUERPO toma el mando (InTakeover)» |
| CU-JUG-17 | Como simulación quiero que al bajar el apremio el cuerpo devuelva el mando | ChecklistAuto «apremio bajo → suelta el mando» |
| CU-JUG-18 | Como jugador quiero interactuar con F con puestos y objetos para trabajar | sin test |
| CU-JUG-19 | Como jugador quiero enfocar algo con el hechizo «enfocar» para fijar la atención | SanctuaryLifeTest «el hechizo «enfocar» fija la atención visual del jugador» |
| CU-JUG-20 | Como jugador quiero que la atención acerque lo enfocado para verlo mejor en 1ª persona | SanctuaryLifeTest «la atención acerca lo enfocado (zoom de la cámara en 1ª persona)» |
| CU-JUG-21 | Como jugador quiero ver mi itinerario de la jornada en pantalla para saber dónde ir | sin test |
| CU-JUG-22 | Como jugador quiero que un diálogo abierto se quede con Intro para no lanzar hechizos sin querer | sin test |
| CU-JUG-23 | Como jugador quiero serenar a una criatura (V) para ganar recursos y XP de Stats | sin test |
| CU-JUG-24 | Como jugador quiero pulsar F1 para ver todos los paneles de prueba | sin test |

## Hechizos y consola

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-HEC-01 | Como jugador quiero escribir «walk» + Intro para elegir el hechizo y que me pida su parámetro | SanctuaryLifeTest «paso 1: «walk» + Intro elige walkTo y pide su primer parámetro» |
| CU-HEC-02 | Como jugador quiero fijar un valor y pasar al siguiente parámetro para construir el hechizo | SanctuaryLifeTest «paso 2: el valor se fija y pasa al siguiente parámetro» |
| CU-HEC-03 | Como jugador quiero lanzar con Intro vacío para usar lo ya dado | SanctuaryLifeTest «Intro con la entrada vacía lanza el hechizo con lo dado» |
| CU-HEC-04 | Como jugador quiero `walkTo` a un ser u objeto (y pensar «¿Dónde está?» si no lo veo) | sin test |
| CU-HEC-05 | Como jugador quiero `use` con alcance para que lejos falle y cerca avance el paso | sin test |
| CU-HEC-06 | Como jugador quiero `bond`/`thought` a distancia para cambiar vínculo o ánimo de otro | sin test |
| CU-HEC-07 | Como jugador quiero `open`/`close` un área para abrirla o cerrarla | sin test |
| CU-HEC-08 | Como jugador quiero `status` sobre un ser o área para ver su estado o sus stocks | sin test |
| CU-HEC-09 | Como desarrollador quiero lanzar hechizos desde `Temp/spell-inbox.txt` para probar sin teclado | sin test |
| CU-HEC-10 | Como ser quiero aprender un hechizo para tenerlo en el grimorio | GrimoireTest «Learn desbloquea el hechizo (vía mágica)» |
| CU-HEC-11 | Como ser quiero lanzar con el cuerpo si tengo la parte, aunque no conozca la magia | GrimoireTest «vía CORPORAL (bodyEnabled) → puede» |
| CU-HEC-12 | Como simulación quiero que sin vía corporal ni mágica no se pueda lanzar | GrimoireTest «sin ninguna vía → NO puede» |
| CU-HEC-13 | Como ser quiero despertar mis reservas mágicas para tener pools H/C/N/O | ChecklistMagicTest «[1117] el despertar siembra las pools H/C/N/O» |
| CU-HEC-14 | Como simulación quiero que comer antes del despertar vaya a grasa y no a magia | ChecklistMagicTest «[1108] comer ANTES de desbloquear no llena las pools de magia» |
| CU-HEC-15 | Como ser quiero llenar pools comiendo (carne → N y H, fruta → C) | ChecklistMagicTest «[1110] tras desbloquear, comer CARNE llena N y H» |
| CU-HEC-16 | Como ser quiero lanzar fuego solo si tengo reservas | ChecklistMagicTest «[1122] con reservas, el lanzallamas sale» |
| CU-HEC-17 | Como simulación quiero que lanzar un hechizo vuelva el aura destructiva (temida) | ChecklistMagicTest «[1129] lanzar un hechizo vuelve el aura DESTRUCTIVA (temido)» |
| CU-HEC-18 | Como ser quiero lanzar agua, tierra y viento con sus costes | ChecklistMagicTest «[1126] agua sale con reservas» |
| CU-HEC-19 | Como ser quiero convertir masa en energía solo con energía en el pool | ChecklistMagicTest «[1124] masa-energía con el pool lleno → sale» |
| CU-HEC-20 | Como simulación quiero cobrar materia y energía de forma atómica (todo o nada) | ChecklistMagicTest «[1128] Pay con todo → cobra materia Y energía» |
| CU-HEC-21 | Como ser quiero pasar energía y materia a otro (`SupplyTo`) | ChecklistMagicTest «[1130] SupplyTo transfiere» |
| CU-HEC-22 | Como ser quiero cargar un hechizo y soltarlo en ráfaga | ChecklistMagicTest «[1160] soltar la carga → burst (OnChargeReleased) con lo acumulado» |
| CU-HEC-23 | Como ser quiero que entrenar stats suba mis topes de magia | ChecklistMagicTest «[1150] subir resistencia/fuerza/masa sube EffectiveCapPerElement» |
| CU-HEC-24 | Como ser quiero crear elementos con quarks y restituir masa en energía (E=mc²) | ChecklistMagicTest «[1174] MakeElement crea el elemento en su pool» |
| CU-HEC-25 | Como ser quiero ver la energía de un hechizo solo tras aprender su física | ChecklistMagicTest «[1138] tras aprender la física → energía revelada» |
| CU-HEC-26 | Como simulación quiero que el obrero de un hechizo pague su parte | ChecklistMagicTest «[1137] con obrero: cobra su parte en sus pools (C 2 g + H 1 g)» |
| CU-HEC-27 | Como simulación quiero que un hechizo con subproducto deje rastro cada cierto tiempo | ChecklistFaunaTest «[1314] pasado byproductInterval, sí vuelve a depositar» |
| CU-HEC-28 | Como simulación quiero que el miedo cargue más al frágil que al templado | StateSpellsTest «el TEMPLE resiste el miedo (sereno < fragil)» |
| CU-HEC-29 | Como simulación quiero que el estupefaciente cree tolerancia y adicción | ChecklistFaunaTest «[1515] con tolerancia, la siguiente dosis seda MENOS» |
| CU-HEC-30 | Como simulación quiero que el duelo vuelva a intervalos y suba el estrés | ChecklistFaunaTest «[1515] …y REINCIDE al siguiente intervalo» |
| CU-HEC-31 | Como jugador quiero lanzar `yoga` para encolar el Saludo al Sol (12 posturas) | YogaTest «el Saludo al Sol son 12 posturas» |
| CU-HEC-32 | Como jugador quiero ver la postura en el cuerpo para que el yoga se note | YogaTest «la postura ROTA el hueso Chest (>5°)» |
| CU-HEC-33 | Como jugador quiero `teach`, `analysis` y `vaccinate` para enseñar y cuidar a otros | sin test |
| CU-HEC-34 | Como jugador quiero hacer la jornada de descomposición (identificar → romper → clasificar) | ChecklistMagicTest «[1142] tras clasificar todas → jornada terminada» |
| CU-HEC-35 | Como jugador quiero cambiar los stats de un ser con `TransformationSpell` | sin test |
| CU-HEC-36 | Como jugador quiero lanzar `progreso` para ver el checklist de historias, misiones y prácticas | SanctuaryLifeTest «el hechizo «progreso» abre el checklist de historias y misiones» |

## Personajes y mente

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-MEN-01 | Como simulación quiero que el ayuno dé hambre | BodyMetabolismTest «7 h sin comer → hambre» |
| CU-MEN-02 | Como simulación quiero que el sueño suba más de madrugada | BodyMetabolismTest «de madrugada el sueño sube más que a mediodía» |
| CU-MEN-03 | Como simulación quiero que dormir baje el sueño | BodyMetabolismTest «dormido el sueño baja» |
| CU-MEN-04 | Como simulación quiero que el cuerpo con hambre encole comer | BodyMetabolismTest «con hambre el cuerpo encola COMER (carril propio)» |
| CU-MEN-05 | Como simulación quiero que el cuerpo con sueño encole dormir | BodyMetabolismTest «con sueño el cuerpo encola DORMIR» |
| CU-MEN-06 | Como simulación quiero que un niño bien comido crezca más | BodyMetabolismTest «un niño bien comido crece más que uno hambriento» |
| CU-MEN-07 | Como simulación quiero que el estrés envejezca y el desgaste total mate de vejez | BodyMetabolismTest «desgaste 1 → muere de vieja» |
| CU-MEN-08 | Como jugador quiero que los stats den forma al cuerpo (altura, músculo) | BodyShapeTest «alto, fuerte y ágil → más alto que el medio» |
| CU-MEN-09 | Como jugador quiero que la grasa se vea en el tronco | BodyShapeTest «la grasa va al tronco más que a los antebrazos» |
| CU-MEN-10 | Como simulación quiero que falte un nutriente y crezca su antojo | ChecklistMagicTest «[1114] el antojo es lo que más falta (protein)» |
| CU-MEN-11 | Como simulación quiero que el frío queme más grasa | ChecklistMagicTest «[1113] el FRÍO (temp<37) quema más grasa (más antojo de grasa)» |
| CU-MEN-12 | Como ser quiero ganar fuerza poco a poco comiendo | ChecklistMagicTest «[1112] …y sube GRADUAL (no salta de golpe)» |
| CU-MEN-13 | Como simulación quiero que sin grasa la energía salga de la masa del cuerpo | SanctuaryLifeTest «…pero gastando masa del propio cuerpo» |
| CU-MEN-14 | Como jugador quiero subir la marga de Yoga para desbloquear el maná | ChecklistProgressTest «[758] marga de Yoga nivel 2 → maná DESBLOQUEADO» |
| CU-MEN-15 | Como jugador quiero que subir margas (Yoga/Stats/Vínculos) suba mis puntos del alma | ChecklistProgressTest «[763] subir Vínculos sube los puntos del alma (SoulLevels 3, vida ↑)» |
| CU-MEN-16 | Como simulación quiero que una mente más poderosa piense frases más largas o calle | ChecklistProgressTest «[834] una mente muy poderosa llega al SILENCIO (no formula texto)» |
| CU-MEN-17 | Como simulación quiero que un campo de pensamiento cambie humor y tono de quien está dentro | ChecklistProgressTest «[840] el campo mueve el HUMOR de quien está dentro (serotonina ↓)» |
| CU-MEN-18 | Como simulación quiero repartir vivencias en modo estricto o libre | ChecklistProgressTest «[846] Libre: el pool PÚBLICO se reparte entero entre los libres» |
| CU-MEN-19 | Como diseñador quiero mezclar almas (blend) y que salga una mente coherente | ChecklistSoulTest «[1186] Panterilia_Blend (Human 90 + Lion 5 + resto) sale ~humana (mente ±10%)» |
| CU-MEN-20 | Como diseñador quiero un cuerpo de oso con mente humana | ChecklistSoulTest «[1187] OsoMenteHumana: mente HUMANA (razón/creatividad altas, > oso puro)» |
| CU-MEN-21 | Como jugador quiero que mis reencarnaciones compartan alma, stats y vínculos | ChecklistSoulTest «[1204] entrena poder (+0.5) → suben los stats de LOS DOS» |
| CU-MEN-22 | Como simulación quiero que el karma de especie fije el vínculo inicial | ChecklistSoulTest «[1211] con un perro (karma +45) el bond arranca alto» |
| CU-MEN-23 | Como simulación quiero que la cercanía haga crecer el vínculo | ChecklistSoulTest «[1210] la cercanía sigue haciendo crecer el bond con el tiempo» |
| CU-MEN-24 | Como simulación quiero que los NPC lean su fuerza del Anima | ChecklistProgressTest «[880] los NPC (WorldCharacter) leen su fuerza de Anima.physicalResistance (0–1)» |
| CU-MEN-25 | Como ser sereno quiero observar para sufrir menos por la misma carga | ObservationTest «el sereno SUFRE MENOS por la misma carga (factor < promedio)» |
| CU-MEN-26 | Como jugador quiero ver si mis elementos están en su nivel ideal | IdealStateTest «cantidad ideal → nivel Ideal» |
| CU-MEN-27 | Como jugador quiero saber si entreno bien, de más o de menos (ACWR) | IdealStateTest «pico brusco → SOBRECARGA (ACWR > 1.3)» |
| CU-MEN-28 | Como simulación quiero que cada órgano capte sus canales al observar | SensesTest «el OJO capta Color» |
| CU-MEN-29 | Como simulación quiero que una atrapada con excitación alta pierda el control y dañe | ChecklistProgressTest «[733] quedarse pegado con la excitación alta → PIERDE EL CONTROL y daña al jugador» |
| CU-MEN-30 | Como jugador quiero ver el pensamiento del ser que manejo y que cambie al poseer otro | sin test |
| CU-MEN-31 | Como jugador quiero ver emociones en las caras de cubos | sin test |
| CU-MEN-32 | Como jugador quiero que la Magnate calme a los que pelean cerca de ella | sin test |
| CU-MEN-33 | Como jugador rezagado quiero que la Magnate me enseñe el Saludo al Sol | sin test |
| CU-MEN-34 | Como jugador quiero encontrar a Lizmibet, Sadhguru, Andrea y el jefe de obra en el santuario | SanctuaryLifeTest «el jefe de construcción, Sadhguru y Andrea están en el santuario» |
| CU-MEN-35 | Como personaje quiero un dormitorio propio | SanctuaryLifeTest «hay un dormitorio por persona (12)» |
| CU-MEN-36 | Como personaje quiero ir al baño | SanctuaryLifeTest «hay inodoros a los que ir al baño» |
| CU-MEN-37 | Como simulación quiero que los compañeros hablen entre ellos (charlas) | sin test |

## Fauna y ecología

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-FAU-01 | Como presa quiero huir de un depredador más fuerte | ChecklistFaunaTest «[1231] la presa ante un depredador más fuerte → HUYE» |
| CU-FAU-02 | Como depredador quiero cazar a quien puedo vencer | ChecklistFaunaTest «[1231] el depredador agresivo y claramente más fuerte → ATACA (caza)» |
| CU-FAU-03 | Como lobo quiero plantar cara si tengo manada cerca | ChecklistFaunaTest «[1231] con aliados cerca (manada) planta cara» |
| CU-FAU-04 | Como madre quiero defender a mis crías (acosar o plantarme) | ChecklistFaunaTest «[1257] osa fuerte con crías → se PLANTA (Fight)» |
| CU-FAU-05 | Como herbívoro quiero ir a la hierba más cercana | ChecklistFaunaTest «[1227] un herbívoro elige el GrassPatch más cercano» |
| CU-FAU-06 | Como depredador quiero elegir presas según reglas (no mi especie, sin vínculo, en radio) | ChecklistFaunaTest «[1228] la presa elegida respeta las reglas…» |
| CU-FAU-07 | Como simulación quiero que los stats decidan quién puede con quién | FaunaChecks «el depredador PUEDE con la presa (stats)» |
| CU-FAU-08 | Como simulación quiero que la percepción module la amenaza | FaunaChecks «Assess gateado por sentidos (menos percepción → menos amenaza)» |
| CU-FAU-09 | Como animal quiero ganar confianza al vencer y atacar solo si la tengo | EmergenceAuto «con confianza alta SÍ ataca (temperamento histórico)» |
| CU-FAU-10 | Como foca o ballena quiero moverme según mi medio (agua/tierra) y sufrir si quedo varada | ChecklistFaunaTest «[1272] ballena varada en tierra PIERDE vida progresivamente» |
| CU-FAU-11 | Como animal quiero deambular cerca de casa y hacia delante | ChecklistFaunaTest «[1420] el wander sesga hacia ADELANTE (sigue explorando)» |
| CU-FAU-12 | Como adulto quiero entrar en celo y dejar rastro | ChecklistFaunaTest «[1395] …y deposita Estrus en la rejilla (Sniff)» |
| CU-FAU-13 | Como especie quiero reproducirme y que la cría nazca niña y de mi especie | LifeCycleAuto «la cría arranca en etapa CHILD» |
| CU-FAU-14 | Como simulación quiero que un enfermo sea presa fácil y se pueda curar | SicknessTest «Heal → sano de nuevo» |
| CU-FAU-15 | Como simulación quiero que la inanición gaste masa, enferme y mate bajo un mínimo | ChecklistFaunaTest «[1449] si la masa cae bajo baseMass × lethalMassFrac → MUERE» |
| CU-FAU-16 | Como animal diurno quiero dormir de noche y despertar ante una amenaza | SleepTest «una amenaza (aware) ROMPE el sueño aunque sea de noche» |
| CU-FAU-17 | Como simulación quiero que el río arrastre y debilite a las crías | RiverCurrentTest «la cria es ARRASTRADA aguas abajo (+Z)» |
| CU-FAU-18 | Como animal quiero preferir mi comida favorita | DietPreferenceTest «la mariquita prefiere el pulgon a la propia (Aphid>1)» |
| CU-FAU-19 | Como animal quiero seguir rastros que decaen con el tiempo | TraceFieldTest «gradiente hacia el deposito» |
| CU-FAU-20 | Como cochinilla quiero hacerme bola ante un depredador ágil | EraSpeciesTest «ante la araña (más ágil) se hace BOLA en vez de huir» |
| CU-FAU-21 | Como luciérnaga quiero destellar y que la hembra me conteste | EraSpeciesTest «y le CONTESTA con su destello» |
| CU-FAU-22 | Como grillo o cigarra quiero cantar en celo | EraSpeciesTest «{sp} en celo CANTA (AudioSource sonando)» |
| CU-FAU-23 | Como larva de luciérnaga quiero cazar caracoles (la adulta no caza) | EraSpeciesTest «la LARVA de luciérnaga sí caza (caracoles y babosas)» |
| CU-FAU-24 | Como cría de escorpión quiero ir a lomos de mi madre | EraSpeciesTest «va subida al lomo (por encima del pivote de la madre)» |
| CU-FAU-25 | Como especie nocturna quiero dormir de día y estar despierta de noche | EraSpeciesTest «a mediodía la luciérnaga DUERME» |
| CU-FAU-26 | Como visitante del huerto quiero comer el cultivo que más me apetece | GardenVisitorTest «por su cuenta elige la ZANAHORIA (la que más le apetece)…» |
| CU-FAU-27 | Como abeja de las nieves quiero volar por el bosque | SanctuaryLifeTest «las abejas de las nieves viven (vuelan) en el bosque» |
| CU-FAU-28 | Como colonia quiero una reina que ponga huevos en el panal | SanctuaryLifeTest «la reina pone huevos en las celdillas del panal» |
| CU-FAU-29 | Como araña de las nieves quiero tejer junto a los panales | SanctuaryLifeTest «las arañas de las nieves tejen su tela junto a los panales» |
| CU-FAU-30 | Como fauna marina quiero vivir en la laguna | IslandsTest «la fauna marina vive en la laguna» |
| CU-FAU-31 | Como diseñador quiero fauna en proporción a los humanos | SanctuaryLifeTest «la fauna va en proporción a los humanos (más pingüinos que osos)» |
| CU-FAU-32 | Como jugador quiero un resumen del ecosistema (vivos, tendencia, herbívoros/depredadores) | ChecklistProgressTest «[1437] el resumen trae vivos + tendencia (Δ) + herbívoros/depredadores» |
| CU-FAU-33 | Como diseñador quiero que el ecosistema se mantenga equilibrado (no 33 lobos y 1 ciervo) | sin test |
| CU-FAU-34 | Como animal quiero que mi IA decida cuando nadie me posee | ChecklistFaunaTest «[1238] un animal real sin poseer lo conduce su IA» |
| CU-FAU-35 | Como animal quiero defecar según mi dieta y reprimirlo en peligro | sin test |
| CU-FAU-36 | Como árbol carnívoro quiero atraer con señuelo y adormecer | sin test |
| CU-FAU-37 | Como fauna nueva quiero nacer sin armamento ni confianza de combate | ChecklistFaunaTest «[1255] fauna recién spawneada: armamento 0» |

## Áreas y trabajo (jornada, misiones, virtualización)

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-ARE-01 | Como director de jornada quiero repartir misiones al personal cada día | SanctuaryLifeTest «la jornada reparte misiones al personal» |
| CU-ARE-02 | Como santuario quiero que todas las misiones obligatorias queden cubiertas | SanctuaryLifeTest «todas las obligatorias quedan cubiertas» |
| CU-ARE-03 | Como personal quiero que el orden de áreas rote cada día | SanctuaryLifeTest «el orden de áreas rota cada día (la última pasa a ser la primera)» |
| CU-ARE-04 | Como primera persona quiero empezar cada día por un área distinta | SanctuaryLifeTest «la primera persona empieza cada día por un área distinta» |
| CU-ARE-05 | Como trabajador quiero no pasar de 5 h de trabajo | SanctuaryLifeTest «nadie pasa de sus 5 h de trabajo» |
| CU-ARE-06 | Como trabajador quiero recibir primero la misión que nunca he hecho | SanctuaryLifeTest «se da primero la misión que esa persona nunca ha hecho» |
| CU-ARE-07 | Como santuario quiero que, si falta gente, la Magnate se divida en clones | SanctuaryLifeTest «si faltan personas, la Magnate se divide y cubre todas las obligatorias» |
| CU-ARE-08 | Como santuario quiero que lo no cubierto pase a mañana, primero | SanctuaryLifeTest «las que no se cubren pasan a mañana y son las primeras» |
| CU-ARE-09 | Como jugador quiero que el informe diga si faltan personas | SanctuaryLifeTest «el informe dice si faltan personas» |
| CU-ARE-10 | Como trabajador quiero mi entrenamiento diario (yoga o gimnasio) | SanctuaryLifeTest «cada persona tiene entrenamiento (yoga o gimnasio)» |
| CU-ARE-11 | Como trabajador quiero que mi puesto sea el edificio entero de mi área | SanctuaryLifeTest «el puesto es el edificio entero (gimnasio → dojo de arriba sí; bajo el hospital, no)» |
| CU-ARE-12 | Como trabajador quiero que cuenten las áreas por las que la tarea me hace ir | SanctuaryLifeTest «la Magnate cuenta el espacio de la tarea (reparar en la Cocina sí, el almacén no)» |
| CU-ARE-13 | Como santuario quiero que la Magnate devuelva al que abandona su puesto | sin test |
| CU-ARE-14 | Como sistema quiero bloquear el alcance de la Magnate (alfombra, microcosmos) | SanctuaryLifeTest «el alcance de la Magnate se puede bloquear (y se libera al quitar el bloqueo)» |
| CU-ARE-15 | Como trabajador quiero que la Magnate me inicie en una misión nueva (mímica) | sin test |
| CU-ARE-16 | Como jugador quiero jugar una virtualización paso a paso (F) y que aplique su efecto | sin test |
| CU-ARE-17 | Como jugador quiero que la máquina me ofrezca las misiones de su área | SanctuaryLifeTest «la máquina ofrece misiones de virtualización del área» |
| CU-ARE-18 | Como diseñador quiero que cada misión de trabajo tenga su escena | SanctuaryLifeTest «todas las misiones de trabajo tienen su escena de virtualización» |
| CU-ARE-19 | Como jugador quiero hacer misiones de otras áreas desde cualquier máquina | sin test |
| CU-ARE-20 | Como trabajador quiero volar solo en alfombra a mi área si está en otro grupo | sin test |
| CU-ARE-21 | Como jugador quiero acelerar el tiempo | ChecklistAuto «más velocidad = menos segundos reales por minuto de juego» |
| CU-ARE-22 | Como área quiero producir recursos y más rápido a más velocidad | ChecklistAuto «a x20 produce MÁS que a x1» |
| CU-ARE-23 | Como jugador quiero que un día dure 1 h real | SanctuaryLifeTest «un día de juego dura lo configurado en el reloj (1 h por defecto)» |
| CU-ARE-24 | Como área quiero misiones de limpieza | SanctuaryLifeTest «hay misiones de limpieza en las áreas» |
| CU-ARE-25 | Como guardia quiero patrullar y alimentar a los carnívoros | SanctuaryLifeTest «un guardia patrulla y alimenta a los carnívoros» |
| CU-ARE-26 | Como jugador quiero hacer la patrulla de la torre (4 puestos) | sin test |
| CU-ARE-27 | Como jugador quiero aceptar o no empezar turno | sin test |
| CU-ARE-28 | Como huerto quiero tener plantados los cultivos de la carta | SanctuaryLifeTest «los huertos tienen los cultivos de la carta» |
| CU-ARE-29 | Como jugador quiero alimentar a los visitantes del huerto y que cuente la misión | GardenVisitorTest «la misión la cuenta (resuelta) y la liebre SIGUE en el santuario» |
| CU-ARE-30 | Como área quiero un horario (cierra de noche y los trabajadores paran) | sin test |
| CU-ARE-31 | Como trabajador quiero un gimnasio con 10 tipos de máquina × 3 y pasillo | SanctuaryLifeTest «las máquinas del gimnasio tienen su pasillo (no se apilan)» |
| CU-ARE-32 | Como alumno quiero una sala de yoga con tarima y 30 esterillas | SanctuaryLifeTest «la sala de yoga tiene la plataforma en el centro y 30 esterillas» |
| CU-ARE-33 | Como alumno quiero seguir una clase de yoga (rótulo «inhala…» solo para mí) | sin test |
| CU-ARE-34 | Como tejedor quiero telar, rueca y tintes | SanctuaryLifeTest «el estudio textil tiene telar, rueca y tintes» |
| CU-ARE-35 | Como personaje quiero aseos en las áreas humanas | SanctuaryLifeTest «hay aseos en las áreas humanas» |

## Hospital

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-HOS-01 | Como santuario quiero que el recién llegado no salga del hospital sin iniciación | SanctuaryLifeTest «la salida de la enfermería está vigilada (iniciación bloqueante)» |
| CU-HOS-02 | Como médico quiero que iniciar turistas sea una misión con su escena | SanctuaryLifeTest «la iniciación de turistas del hospital es una misión de virtualización con su escena» |
| CU-HOS-03 | Como recién llegado quiero análisis, vacuna e iniciación con Linduarda para quedar libre | sin test |
| CU-HOS-04 | Como médico quiero estabilizar en urgencias (−enfermedad, −estrés) | sin test |
| CU-HOS-05 | Como cirujano quiero curar del todo en quirófano si hay anestesista | sin test |
| CU-HOS-06 | Como médico quiero diagnosticar con imagen y laboratorio | sin test |
| CU-HOS-07 | Como veterinario quiero que los puestos de veterinaria atiendan solo animales | sin test |
| CU-HOS-08 | Como médico de turno quiero ocupar un puesto libre de mi servicio (4 por servicio) | sin test |
| CU-HOS-09 | Como paciente quiero recuperarme en el anillo de camas de mi planta | SanctuaryLifeTest «la Magnate cuenta la entreplanta de las camas como parte del hospital (y fuera, no)» |
| CU-HOS-10 | Como médico quiero las misiones hos_urgencias/quirofano/uci/veterinaria con pasos | sin test |
| CU-HOS-11 | Como médico quiero un dato de ciencia en cada puesto | sin test |
| CU-HOS-12 | Como oso enfermo quiero llegar a la planta veterinaria (montacargas/alfombra) | sin test |
| CU-HOS-13 | Como jugador quiero encontrar a Lizmibet (médica) en el santuario | SanctuaryLifeTest «Lizmibet (médica, amiga de Irosene) está en el santuario» |

## Restaurante y cocina

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-COC-01 | Como cocinero quiero una cocina elevada a 12 m | SanctuaryLifeTest «la cocina está elevada (12 m)» |
| CU-COC-02 | Como cocinero quiero todo el equipo que pide la carta | SanctuaryLifeTest «la cocina tiene todo el equipo que pide la carta» |
| CU-COC-03 | Como diseñador quiero que cada ingrediente salga de un cultivo, el laboratorio o una elaboración | SanctuaryLifeTest «cada ingrediente sale de un cultivo, del laboratorio o de una elaboración» |
| CU-COC-04 | Como cocinero quiero despensa y obrador | SanctuaryLifeTest «hay despensa y obrador en la cocina» |
| CU-COC-05 | Como obrador quiero elaborar bases (yogur, quesos, masas, tofu) | SanctuaryLifeTest «el obrador elabora sus bases (yogur, quesos, masas, tofu…)» |
| CU-COC-06 | Como cocina quiero tener casi toda la carta lista tras la mise en place | SanctuaryLifeTest «tras 80 h de mise en place casi toda la carta está lista» |
| CU-COC-07 | Como comensal quiero una lasaña completa | SanctuaryLifeTest «la lasaña sale (pasta fresca, ragú de carne cultivada, bechamel, parmesano)» |
| CU-COC-08 | Como trabajador quiero que las elaboraciones sean misiones | SanctuaryLifeTest «las elaboraciones de la cocina (queso, harina, pasta…) son misiones» |
| CU-COC-09 | Como jugador quiero cocinar un plato entero con hechizos (take/putIn/beat/step) | sin test |
| CU-COC-10 | Como jugador quiero que los fallos de cocina avisen (lejos, manos llenas, no es recipiente) | sin test |
| CU-COC-11 | Como jugador quiero que al llenar el stock salga MISIÓN CUMPLIDA con XP e inventario | sin test |
| CU-COC-12 | Como comensal quiero un comedor con mesas vestidas | SanctuaryLifeTest «el restaurante tiene comedor con mesas vestidas» |
| CU-COC-13 | Como comensal quiero subir al anillo del comedor con caja y escalera desde el puente | SanctuaryLifeTest «el comedor va en un anillo de 10 m sobre la cocina, con caja y escalera desde el puente» |
| CU-COC-14 | Como restaurante quiero sillas para 30 | SanctuaryLifeTest «el comedor tiene sillas para 30 comensales» |
| CU-COC-15 | Como cocina quiero ocho puestos completos en anillo | SanctuaryLifeTest «la cocina ocupa la planta: ocho puestos completos…» |
| CU-COC-16 | Como santuario quiero cocinas de inducción (sin llama) | SanctuaryLifeTest «las cocinas son de inducción (sin llama de gas)» |
| CU-COC-17 | Como personal quiero comer en el restaurante a las 11:00 | sin test |
| CU-COC-18 | Como recién llegado quiero que Goluis me inicie en cocina (sígueme → limpiar → recetas) | sin test |
| CU-COC-19 | Como cocinero NPC quiero cocinar solo y llenar el stock | sin test |

## Construcción

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-CON-01 | Como obrero quiero un área de Construcción con maquinaria y material | SanctuaryLifeTest «hay área de Construcción con maquinaria y material» |
| CU-CON-02 | Como obrero quiero un almacén de material | SanctuaryLifeTest «hay almacén de material de construcción» |
| CU-CON-03 | Como santuario quiero misiones de obra para las áreas en construcción | SanctuaryLifeTest «hay misiones de obra para las áreas en construcción» |
| CU-CON-04 | Como obrero quiero que llevar material y trabajar la obra la avance un día | SanctuaryLifeTest «llevar el material y trabajar la obra la avanza un día» |
| CU-CON-05 | Como obra quiero gastar guadua, bahareque, micelio y cuerda cada día | SanctuaryLifeTest «un día de obra gasta guadua, bahareque, micelio y cuerda» |
| CU-CON-06 | Como obrero quiero cortar guadua madura y ponerla a curar | SanctuaryLifeTest «cortar guadua saca cañas maduras del bambusal y las pone a curar» |
| CU-CON-07 | Como santuario quiero un bambusal de guadua | SanctuaryLifeTest «hay un bambusal de guadua» |
| CU-CON-08 | Como santuario quiero ballet, dojo y calistenia en obras y el gimnasio abierto | SanctuaryLifeTest «ballet, dojo y calistenia están en obras; el gimnasio abierto» |
| CU-CON-09 | Como santuario quiero que una obra terminada abra su entrenamiento y entre en rotación | sin test |
| CU-CON-10 | Como jugador quiero ver subir las paredes y el cartel de días | sin test |
| CU-CON-11 | Como obrero quiero una reparación diaria de maquinaria en alguna área | sin test |
| CU-CON-12 | Como diseñador quiero que ninguna rama atraviese una plataforma | SanctuaryLifeTest «ninguna rama de árbol atraviesa una plataforma de área» |
| CU-CON-13 | Como diseñador quiero colchones de musgo bajo lo construido | SanctuaryLifeTest «hay colchones de musgo bajo lo construido» |
| CU-CON-14 | Como diseñador quiero huertos elevados para no dar sombra al bosque | SanctuaryLifeTest «el huerto también está elevado (los cultivos no dan sombra al bosque)» |
| CU-CON-15 | Como santuario quiero paneles solares por edificio y un campo | SanctuaryLifeTest «hay paneles solares (uno por edificio + campo)» |
| CU-CON-16 | Como IA quiero trepar escaleras verticales por enlaces del NavMesh | IslandsTest «las escaleras verticales tienen su enlace del NavMesh (la IA trepa)» |
| CU-CON-17 | Como diseñador quiero que anillos vecinos queden a ≥16 m con su escalera | sin test |
| CU-CON-18 | Como guardia quiero una casa del árbol no carnívora con escalera | IslandsTest «la casa del árbol no es carnívora» |
| CU-CON-19 | Como jugador quiero levantar un refugio en el microcosmos | sin test |

## Microcosmos e historias

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-MIC-01 | Como jugador quiero entrar en el microcosmos del área desde su máquina («Simulación») | sin test |
| CU-MIC-02 | Como jugador quiero jugar una historia por pasos con E (Talk/Reach/Collect/VisitAll/Hold) | sin test |
| CU-MIC-03 | Como jugador quiero recibir XP de la marga y la HistoryCard al terminar | sin test |
| CU-MIC-04 | Como jugador quiero que los mirmidones vivan su rutina aunque no les hable | sin test |
| CU-MIC-05 | Como jugador quiero un marcador de misión con el objetivo más cercano | sin test |
| CU-MIC-06 | Como jugador quiero empezar Upa-yoga con H y dejarla con Esc | sin test |
| CU-MIC-07 | Como jugador quiero formar posturas en la Sala de Yoga del microcosmos | sin test |
| CU-MIC-08 | Como jugador quiero las simulaciones de compasión y Egina | sin test |
| CU-MIC-09 | Como alma quiero convertirme a otro cuerpo (Ambrosio_Convert) y que la forma cambie | ChecklistSoulTest «[1194] A/relativa: la forma la MODULA la hormiga (str/agi cambia)» |
| CU-MIC-10 | Como jugador quiero que mi tamaño sea de hormiga en el microcosmos | ChecklistSoulTest «[1194] ambos modos → tamaño hormiga» |
| CU-MIC-11 | Como diseñador quiero el elenco del microcosmos con sus aptitudes reales | ChecklistSoulTest «[1209] Panterilia_SinClase = Panterilia real…» |
| CU-MIC-12 | Como jugador quiero insectos de ambiente en la escena 0 (y que falte un modelo no rompa nada) | sin test |
| CU-MIC-13 | Como jugador quiero que las animas libres del elenco merodeen solas | sin test |

## Investigación y producción

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-INV-01 | Como santuario quiero un laboratorio de carne cultivada | SanctuaryLifeTest «hay laboratorio de carne cultivada» |
| CU-INV-02 | Como laboratorio quiero que el lote pase por la proliferación en biorreactor | SanctuaryLifeTest «el lote pasa por la proliferación en biorreactor» |
| CU-INV-03 | Como guardia quiero cosechar raciones de carne para los depredadores | SanctuaryLifeTest «…y cosecha raciones de carne para los depredadores» |
| CU-INV-04 | Como laboratorio quiero que alguien inicie y coseche los lotes | SanctuaryLifeTest «el laboratorio necesita a alguien para iniciar y cosechar los lotes» |
| CU-INV-05 | Como trabajador quiero misiones de iniciar y cosechar lotes | SanctuaryLifeTest «iniciar y cosechar lotes del laboratorio son misiones» |
| CU-INV-06 | Como santuario quiero un tanque de virtualización con escotilla magnética | SanctuaryLifeTest «la escotilla horizontal se abre con el campo magnético encendido» |
| CU-INV-07 | Como físico quiero hacer los experimentos (doble rendija, Michelson, Millikan, Foucault) | sin test |
| CU-INV-08 | Como químico quiero sintetizar y gestionar residuos | sin test |
| CU-INV-09 | Como biólogo quiero cultivar y hacer PCR | sin test |
| CU-INV-10 | Como operador quiero hacer el turno de la central nuclear | sin test |
| CU-INV-11 | Como laboratorio quiero 2 m entre bloques y ×4 puestos (FurnitureSpacing) | sin test |
| CU-INV-12 | Como hospital quiero recibir el Tc-99m de la planta atómica | sin test |
| CU-INV-13 | Como abejas quiero una colmena en cavidad con piquera | SanctuaryLifeTest «la colmena está en una cavidad con piquera (entrada)» |

## Rendimiento

| ID | Caso de uso | Prueba |
|---|---|---|
| CU-REN-01 | Como jugador quiero que Play sin pruebas no mueva fauna ni consola para poder jugar ya | sin test |
| CU-REN-02 | Como simulación quiero que las ánimas lejanas piensen más lento (LOD) | sin test |
| CU-REN-03 | Como simulación quiero que sin rejilla de rastros nada pete | ChecklistFaunaTest «[1286] sin TraceField: Leave/Sniff/Trail no petan» |
| CU-REN-04 | Como simulación quiero que un hechizo con subproducto sin rejilla no pete | ChecklistFaunaTest «[1315] hechizo con subproducto opt-in SIN rejilla → no peta (no-op)» |
| CU-REN-05 | Como jugador quiero que los paneles de sandbox salgan solo cerca | sin test |
| CU-REN-06 | Como juego quiero que una especie sin modelo salga como cubo sin crashear | sin test |
| CU-REN-07 | Como IA quiero NavMesh en el hub | IslandsTest «hay NavMesh en el hub» |
| CU-REN-08 | Como jugador quiero un framerate estable con miles de plantas y fauna | sin test |
| CU-REN-09 | Como simulación quiero que la laguna central sea agua sin NavMesh | IslandsTest «la laguna central es agua (sin NavMesh en su centro)» |
| CU-REN-10 | Como jugador quiero que la cámara se acerque si una pared la tapa | sin test |
