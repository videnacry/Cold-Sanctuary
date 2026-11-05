using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA GRAN LEY DE LA PAZ (docs/microcosmos-historias-3.md, con la justificación de la elección). La historia de los pueblos nativos de
/// Norteamérica que resume mejor lo que fueron y siguen siendo: cinco naciones enfrentadas (mohawk, oneida, onondaga, cayuga y seneca) se unen
/// en la Confederación Haudenosaunee, gracias al Pacificador, a Ayenwatha (Hiawatha), a Jigonsaseh, la madre de las naciones, y a la curación
/// de Tadodaho. Es una TRADICIÓN VIVA de un pueblo que existe hoy: se cuenta como la cuentan sus naciones, resumida, sin recrear ninguna
/// ceremonia; al Pacificador no se le da su nombre propio (muchos haudenosaunee lo reservan para la ceremonia). Las frases son de juego.
/// </summary>
public static partial class StorySceneBuilder
{
    static int GranLeyDeLaPaz(Transform t)
    {
        Material pine = M("Pino_blanco", 0.2f, 0.4f, 0.3f), bark = M("Corteza_del_pino", 0.4f, 0.32f, 0.26f), elm = M("Corteza_de_olmo", 0.5f, 0.42f, 0.32f), shellW = M("Concha_blanca", 0.93f, 0.92f, 0.88f);
        Material shellP = M("Concha_morada", 0.35f, 0.15f, 0.4f), earth = M("Tierra_del_claro", 0.36f, 0.27f, 0.2f), water = M("Agua_del_lago_Onondaga", 0.3f, 0.48f, 0.55f), stone = M("Caliza_de_Onondaga", 0.6f, 0.6f, 0.56f), maize = M("Mazorca", 0.9f, 0.8f, 0.35f);

        // 1 · EL ÁRBOL DE LA PAZ: el gran pino blanco, sus cuatro raíces blancas y el hoyo donde caen las armas.
        Transform tree = Spot(t, "Árbol de la Paz", new Vector3(0f, 0f, 17f), "El Árbol de la Paz: el gran pino blanco", 7f);
        Prop(tree, "Tronco del pino blanco", new Vector3(0f, 3f, 0f), new Vector3(1.1f, 6f, 1.1f), bark);
        for (int k = 0; k < 3; k++) Prop(tree, "Copa del pino", new Vector3(0f, 4.5f + k * 1.3f, 0f), new Vector3(5.5f - k * 1.5f, 1.2f, 5.5f - k * 1.5f), pine, false);
        Prop(tree, "Raíz blanca del norte", new Vector3(0f, 0.08f, 3.5f), new Vector3(0.4f, 0.16f, 6f), shellW, false);
        Prop(tree, "Raíz blanca del sur", new Vector3(0f, 0.08f, -3.5f), new Vector3(0.4f, 0.16f, 6f), shellW, false);
        Prop(tree, "Raíz blanca del este", new Vector3(3.5f, 0.08f, 0f), new Vector3(6f, 0.16f, 0.4f), shellW, false);
        Prop(tree, "Raíz blanca del oeste", new Vector3(-3.5f, 0.08f, 0f), new Vector3(6f, 0.16f, 0.4f), shellW, false);
        Prop(tree, "Hoyo bajo el árbol", new Vector3(2.2f, 0.02f, -2.2f), new Vector3(1.8f, 0.04f, 1.8f), earth, false);

        // 2 · LA ORILLA: Ayenwatha ensarta sus conchas junto al lago.
        Transform shore = Spot(t, "Orilla del lago Onondaga", new Vector3(-18f, 0f, 8f), "La orilla: las conchas de Ayenwatha", 2.6f);
        Prop(shore, "Lago", new Vector3(-3f, 0.02f, 0f), new Vector3(5f, 0.04f, 16f), water, false);
        Prop(shore, "Vara con tres sartas de conchas", new Vector3(2f, 0.8f, 0f), new Vector3(2f, 0.1f, 0.1f), bark, false);
        var shells = Items(t, shellP, new Vector3(0.4f, 0.12f, 0.4f), ("Sarta de conchas para el consuelo", new Vector3(-20f, 0.08f, -1f)), ("Sarta de conchas para las palabras", new Vector3(-17f, 0.08f, -7f)), ("Sarta de conchas para la paz", new Vector3(-12f, 0.08f, -9f)));

        // 3 · LOS FUEGOS DE LAS CUATRO NACIONES que ya aceptaron.
        var fires = new List<Transform>();
        (string name, Vector3 at, string label)[] nations =
        {
            ("Fuego mohawk", new Vector3(14f, 0f, 12f), "Fuego mohawk: guardianes de la puerta del este"),
            ("Fuego oneida", new Vector3(19f, 0f, 5f), "Fuego oneida: los de la piedra de pie"),
            ("Fuego cayuga", new Vector3(18f, 0f, -3f), "Fuego cayuga: los de la gran pantanosa"),
            ("Fuego seneca", new Vector3(13f, 0f, -9f), "Fuego seneca: guardianes de la puerta del oeste"),
        };
        foreach (var (name, at, label) in nations)
        {
            Transform f = Spot(t, name, at, label, 2.2f);
            Prop(f, "Fuego", new Vector3(0f, 0.15f, 0f), new Vector3(1f, 0.3f, 1f), stone, false);
            Prop(f, "Tronco para sentarse", new Vector3(0f, 0.2f, -1.6f), new Vector3(2f, 0.4f, 0.5f), bark);
            fires.Add(f);
        }

        // 4 · LA CASA DE JIGONSASEH y la LOMA DE TADODAHO.
        Transform house = Spot(t, "Casa de Jigonsaseh", new Vector3(-9f, 0f, -6f), "La casa de Jigonsaseh, en el camino de los guerreros: aquí comen todos", 3f);
        for (int k = 0; k < 4; k++) Prop(house, "Arco de la casa larga", new Vector3(-3f + k * 2f, 1.4f, 2f), new Vector3(0.25f, 2.8f, 0.25f), elm);
        Prop(house, "Olla de maíz, frijol y calabaza", new Vector3(0f, 0.3f, 0f), new Vector3(1f, 0.6f, 1f), maize);
        Transform tadodaho = Spot(t, "Pantano de Tadodaho", new Vector3(-15f, 0f, 16f), "La loma de Tadodaho", 2.6f);
        Prop(tadodaho, "Asiento de piedra", new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 1.2f), stone);

        // La gente: insectos-humanos. El Pacificador, una ABEJA (el enjambre decide por consenso); Jigonsaseh, una HORMIGA (la madre de la
        // colonia); Ayenwatha, una CIGARRA (la voz que vuelve tras años bajo tierra); Tadodaho, una ARAÑA (se enreda y se desenreda).
        Color deer = new Color(0.62f, 0.48f, 0.32f), white = new Color(0.9f, 0.88f, 0.82f), dark = new Color(0.25f, 0.2f, 0.18f), green = new Color(0.3f, 0.4f, 0.28f);
        GameObject peacemaker = Ancient(t, "El Pacificador", new Vector3(3f, 0f, 9f), false, white, false, 1.74f, "SnowBee");
        GameObject jigonsaseh = Ancient(t, "Jigonsaseh", new Vector3(-7f, 0f, -3f), true, deer, false, 1.62f, "Ant"); Routine(jigonsaseh, house);
        GameObject ayenwatha = Ancient(t, "Ayenwatha (Hiawatha)", new Vector3(-14f, 0f, 5f), false, deer, false, 1.72f, "Cicada");
        GameObject tado = Ancient(t, "Tadodaho", new Vector3(-13f, 0f, 14f), false, dark, false, 1.76f, "Spider");
        GameObject mother = Ancient(t, "Madre de clan (Ruth)", new Vector3(8f, 0f, 2f), true, green, false, 1.58f, "Ant"); Routine(mother, fires[0], fires[1], fires[2], fires[3]);
        GameObject runner = Ancient(t, "Corredor mohawk (Momo)", new Vector3(16f, 0f, 9f), false, deer, false, 1.7f, "Grasshopper"); Routine(runner, fires[0], fires[3]);
        int n = 6;

        Mission(t, "La Gran Ley de la Paz (lago Onondaga)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Jigonsaseh en su casa, junto al camino", jigonsaseh.transform, "Jigonsaseh", "Daba de comer a los guerreros de todos los bandos. Un día llegó un extranjero y me dijo: «no alimentes más la guerra». Yo fui la primera en aceptar su mensaje. Ve: en la orilla hay un hombre que ya no quiere hablar."),
                Talk("Ve a la orilla y habla con Ayenwatha", ayenwatha.transform, "Ayenwatha", "Perdí a mis hijas y no pude seguir. Me fui al bosque y ensarté conchas en una vara, y pensé: si alguien sufriera como yo, le diría estas palabras. Nadie me las dijo. Hasta que él llegó y me las dijo a mí."),
                Collect("Lleva las tres sartas de conchas al Árbol", shells, tree, "Ayenwatha", "Con estas palabras se limpian los ojos, para volver a ver; los oídos, para volver a oír; la garganta, para volver a hablar. Antes de la paz, hay que consolar."),
                Visit("Visita los fuegos de las cuatro naciones", fires, "Madre de clan", "Mohawk, oneida, cayuga, seneca: se mataban unos a otros por venganza. Han aceptado. Cinco naciones serán una casa larga: una sola casa con cinco fuegos. Pero falta la quinta. Falta Onondaga, y Onondaga es Tadodaho."),
                Talk("Sube a ver a Tadodaho", tado.transform, "Tadodaho", "Dicen que tengo serpientes en el pelo, y la mente torcida de tanto odio. Tres veces eché a Ayenwatha. Y ahora veníos los dos a cantarme... ¿Queréis peinarme las serpientes?"),
                Talk("Habla con el Pacificador junto al fuego de Onondaga", peacemaker.transform, "El Pacificador", "No venimos a vencerlo. Venimos a curarlo. El más duro será quien guarde el fuego del consejo. Aquí nadie se queda fuera: todos caben bajo el árbol."),
                Hold("Bajo el Árbol de la Paz, mira cómo caen las armas al hoyo", tree, 12f, "El Pacificador", "Bajo este pino echamos las armas, y una corriente se las lleva. Sus raíces van a los cuatro vientos: quien busque la paz, que las siga hasta aquí. Encima, un águila vigila."),
                Talk("Habla con la madre de clan", mother.transform, "Madre de clan", "Los jefes hablan en el consejo. Pero a los jefes los eligen las mujeres de cada clan, y las mismas mujeres pueden quitarles la cornamenta. Y antes de decidir, piensan en los que aún no han nacido: siete generaciones por delante."),
            },
            "El lago Onondaga, en lo que hoy es el estado de Nueva York, en el tiempo en que nació la Confederación Haudenosaunee (la fecha exacta no se conoce), sobre el Aula del santuario a escala de hormiga (pendiente de su piso: una sala de asambleas). Cinco naciones que se mataban entre sí están a punto de hacer la paz. Es una tradición viva de un pueblo que existe hoy: se cuenta con respeto y en resumen. E para hablar y recoger; bajo el árbol, quédate quieto.",
            new[]
            {
                "Los Haudenosaunee, «el pueblo de la casa larga» (los franceses los llamaron iroqueses), eran en origen cinco naciones del noreste de Norteamérica: mohawk, oneida, onondaga, cayuga y seneca; en el siglo XVIII se les unieron los tuscarora, y son las Seis Naciones. Vivían en casas largas de corteza, por clanes que se heredan de la madre, y cultivaban las «tres hermanas»: maíz, frijol y calabaza.",
                "Según su tradición, en una época de guerras de venganza llegó el Pacificador. Jigonsaseh fue la primera en aceptar su mensaje. Ayenwatha (Hiawatha), hundido por la muerte de sus hijas, fue consolado por él con sartas de conchas: de ahí nace el rito de condolencia que aún hoy abre los consejos. Juntos fueron de nación en nación y curaron al último y más duro, Tadodaho, que quedó como guardián del fuego del consejo; ese título sigue existiendo. Junto al lago Onondaga plantaron el Árbol de la Paz, un pino blanco, y enterraron bajo él las armas.",
                "La Gran Ley (Kayanerenkó:wa) reparte el gobierno entre 50 jefes de las cinco naciones, elegidos por las madres de clan, que también pueden destituirlos. Las decisiones se toman por consenso, pensando en quienes vendrán. La fecha es discutida: la tradición habla de hace más de mil años; se han propuesto 1142, hacia 1190 y hacia 1450, que es la que usa esta escena. Se discute también cuánto influyó en la Constitución de Estados Unidos (una resolución del Congreso de 1988 reconoce esa influencia; muchos historiadores la matizan).",
                "Esta escena no recrea ninguna ceremonia ni reproduce palabras rituales. Las versiones cambian según quién las cuente, y la tradición pertenece a los Haudenosaunee, cuyo consejo sigue reuniéndose en Onondaga. Nombramos al Pacificador por su título, como hacen muchos de ellos fuera de la ceremonia.",
            },
            new[]
            {
                "Oneida Indian Nation, «Great Law of Peace» (oneida-nsn.gov/our-ways/great-law-of-peace).",
                "New York State Education Department, «The Great Law of Peace» (unidad didáctica, nysed.gov).",
                "Onondaga Nation, «Keepers of the Central Fire» (historia de la nación).",
                "Skä•noñh, Great Law of Peace Center (Syracuse, Nueva York).",
                "Mann, B. y Fields, J. (1997). «A Sign in the Sky: Dating the League of the Haudenosaunee». American Indian Culture and Research Journal 21 (2).",
                "Parker, A. C. (1916). The Constitution of the Five Nations. New York State Museum; Snow, D. (1994). The Iroquois. Blackwell.",
            },
            "El Pacificador", "Se empieza por consolar. La paz viene después, y dura si la cuidan siete generaciones.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style Longhouse = new Style(C(0.5f, 0.42f, 0.32f), C(0.42f, 0.34f, 0.26f), C(0.45f, 0.38f, 0.3f), Roof.Gable, 3.4f);   // casa larga de corteza de olmo

    // ONONDAGA (hacia 1450): el lago Onondaga al noroeste, el arroyo Onondaga que baja del sur y los cerros del valle; la aldea de casas largas,
    // sus campos de las tres hermanas y sus arboledas. Plano APROXIMADO: el lago y el arroyo son reales; la aldea se recrea por las excavadas de
    // la región (Snow 1994; Bradley 1987, Evolution of the Onondaga Iroquois). Sin calles: senderos.
    const string PlanOnondaga = @"
lago Lago Onondaga | 500 900 | -1000 620
rio Arroyo Onondaga | 12 | 500 -1300, 200 -500, -150 -60, -560 330
rio Arroyo de los nogales | 8 | 1200 300, 600 120, 200 40, -150 -60
colina Colinas del este | 320 90 | 1250 -620
colina Colinas del sur | 300 100 | -820 -950
campo Campos de las tres hermanas | 420 240 | 340 -680
campo Campos del norte | 300 200 | 560 760
campo Arboleda de arces | 300 240 | -620 -300
calle Sendero de la aldea | 4 | -280 40, 320 40
calle Sendero de las casas largas | 4 | 0 -230, 0 280
calle Sendero del lago | 4 | -200 120, -560 330
calle Sendero de los campos | 4 | 0 -230, 340 -520
calle Sendero de las naciones | 6 | -1300 -120, -280 40, 320 40, 1300 200
hito 0 | -60 210
hito 1 | -210 -60
hito 2 | -340 180
hito 3 | -230 290
hito 4 | 0 120
hito 5 | 340 -520
hito 6 | 160 170
hito 7 | 230 -60
Parque | -620 -300
Mercado | 120 -140
Pileta | 340 200
Entrenamiento | 500 -130 | actual: campos de lacrosse de la Nación Onondaga (Nedrow, Nueva York); posición aproximada
Piscina | -380 -140
Sanacion | -130 -210
Baile | 110 290
Estudio | 320 300
Guarderia | -80 -320
Carpinteria | -470 60
Taller | 420 90
Obra | 620 240
Estacion | -520 400
relleno | 38 | 0 40";

    static int OnondagaTown(Transform t)
    {
        var s = Town(t, "Onondaga", new[]
        {
            new Lot("Casa larga del consejo", "Plaza", "Casa larga del consejo (el fuego de Onondaga)", 22f, 12f, Longhouse,
                S("banco", -6f, 3f, 4f, 1.6f, "Banco de las naciones del este"), S("banco", 6f, 3f, 4f, 1.6f, "Banco de las naciones del oeste"), S("fuente", 0f, -3f, 4.4f, 4.4f, "Manantial de la casa"), S("estatua", 0f, 3f, 2.2f, 2.2f, "Poste del clan")),
            new Lot("Casa larga de Jigonsaseh", "Casa", "Casa larga de Jigonsaseh", 20f, 10f, Longhouse,
                S("cama", -6f, 2f, 1.2f, 2f, "Plataforma con esteras"), S("cama", -2f, 2f, 1.2f, 2f, "Plataforma con esteras"), S("cama", 2f, 2f, 1.2f, 2f, "Plataforma con esteras"), S("fogon", 6f, 2f, 1.6f, 0.8f, "Fuego del pasillo")),
            new Lot("Casa larga del clan de la Tortuga", "Casa", "Casa larga del clan de la Tortuga (Ayenwatha)", 20f, 10f, Longhouse,
                S("cama", -6f, 2f, 1.2f, 2f, "Plataforma con esteras"), S("cama", 0f, 2f, 1.2f, 2f, "Plataforma con pieles"), S("fogon", 6f, 2f, 1.6f, 0.8f, "Fuego del pasillo")),
            new Lot("Casa larga del clan del Oso", "Casa", "Casa larga del clan del Oso (Tadodaho)", 20f, 10f, Longhouse,
                S("cama", -6f, 2f, 1.2f, 2f, "Plataforma con pieles"), S("cama", 0f, 2f, 1.2f, 2f, "Plataforma con esteras"), S("fogon", 6f, 2f, 1.6f, 0.8f, "Fuego del pasillo")),
            new Lot("Claro del consejo", "Plaza", "Claro del Árbol de la Paz", 22f, 18f, Longhouse,
                S("banco", -5f, 0f, 4f, 1.6f, "Tronco para sentarse"), S("banco", 5f, 0f, 4f, 1.6f, "Tronco para sentarse"), S("fuente", 0f, -4f, 4.4f, 4.4f, "Manantial del claro")),
            new Lot("Campos de las tres hermanas", "Huerto", "Campos de las tres hermanas (maíz, frijol y calabaza)", 18f, 14f, Longhouse,
                S("bancal", -5f, 0f, 4f, 6f, "Montículos de maíz y frijol"), S("bancal", 2f, 0f, 4f, 6f, "Calabazas"), S("pozo", 7f, 3f, 1.2f, 1.2f, "Cántaro de riego")),
            new Lot("Casa de la comida", "Cocina", "Casa de la comida (sopa de maíz y pan de maíz con frijol)", 16f, 10f, Longhouse,
                S("fogon", -4f, 2f, 1.6f, 0.8f, "Olla de barro sobre el fuego"), S("granero", 2f, 2f, 2f, 2f, "Mortero de tronco para el maíz"), S("mesa", 0f, -3f, 2f, 1f, "Estera y cuencos de madera")),
            new Lot("Taller de conchas", "Taller", "Taller de cuentas de concha y cestos de fresno", 14f, 10f, Longhouse,
                S("banco_taller", -3f, 2f, 2f, 0.9f, "Banco de las cuentas de concha"), S("telar", 3f, 2f, 2f, 1f, "Cinturones de cuentas"), S("estante", 0f, -3f, 1.2f, 0.6f, "Cestos de corteza")),
        }, new[]
        {
            new Lot("Casa larga de la aldea", "Casa", "Casa larga de corteza de olmo", 16f, 9f, Longhouse),
            new Lot("Almacén de maíz", "Granero", "Almacén de maíz (trenzas colgadas)", 10f, 8f, Longhouse, S("granero", 0f, 0f, 2f, 2f, "Trenzas de maíz"), S("estante", 0f, 0f, 0.8f, 1.6f, "Cestos de semillas")),
            new Lot("Taller de cestos", "Taller", "Taller de cestos", 10f, 8f, Longhouse, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco de la cestera")),
            new Lot("Campo de la aldea", "Huerto", "Campo de la aldea", 12f, 10f, Longhouse, S("bancal", 0f, 0f, 4f, 6f, "Maíz, frijol y calabaza"), S("pozo", 0f, 0f, 1f, 1f, "Cántaro")),
        }, C(0.42f, 0.34f, 0.26f));
        int n = 0;
        n += W(t, "Guardiana del fuego del consejo", s[0], "", s[6], null, s[1]);
        n += W(t, "Sembradora de las tres hermanas", s[5], "hortelano", s[6], null, s[1]);
        n += W(t, "Cocinera de la casa de la comida", s[6], "cocinero", s[6], null, s[2]);
        n += W(t, "Ensartadora de cuentas", s[7], "", s[6], null, s[2]);
        n += W(t, "Narradora de los relatos", s[0], "sabio", s[6], null, s[3]);
        n += Townsfolk(t, s, 8, "Vecina de Onondaga", "Vecino de Onondaga", s[6], null);
        return n;
    }
}

public static partial class MicroBiome
{
    // Onondaga, finales de verano: bosque templado de arces y hayas, pino blanco, olmos y nogales; juncos y eneas en el lago; maíz, frijol y
    // calabaza en los campos. Cigarras de agosto, luciérnagas, abejorros nativos, hormigas, saltamontes. Sin caballos, sin manzanos, sin abejas
    // europeas: nada de eso había llegado. (Snow 1994; Engelbrecht 2003, Iroquoia.)
    static Biome OnondagaBiome() => new Biome { title = "Lago Onondaga, bosque templado (finales de verano)", ground = C(0.34f, 0.27f, 0.2f), plants = new[] {
            new Plant(Kind.Shrub, 7, "Pino blanco (Pinus strobus)", 2500, C(0.2f, 0.4f, 0.3f), C(0.42f, 0.32f, 0.26f)),
            new Plant(Kind.Shrub, 7, "Arce de azúcar (Acer saccharum)", 2200, C(0.3f, 0.5f, 0.26f), C(0.5f, 0.4f, 0.3f)),
            new Plant(Kind.Shrub, 5, "Olmo americano (Ulmus americana)", 2200, C(0.3f, 0.48f, 0.28f), C(0.5f, 0.42f, 0.32f)),
            new Plant(Kind.Grass, 10, "Enea (Typha latifolia), en la orilla", 200, C(0.42f, 0.55f, 0.3f), C(0.42f, 0.28f, 0.18f)),
            new Plant(Kind.Flower, 5, "Vara de oro (Solidago canadensis)", 120, C(0.35f, 0.5f, 0.28f), C(0.95f, 0.8f, 0.2f)),
            new Plant(Kind.Fern, 7, "Helecho del bosque (Dryopteris)", 70, C(0.3f, 0.52f, 0.3f), C(0.35f, 0.3f, 0.2f)),
            new Plant(Kind.Stone, 8, "Caliza de Onondaga", 0, C(0.6f, 0.6f, 0.56f), Stone) },
          town = new[] {
            new Plant(Kind.Grass, 14, "Maíz (Zea mays), de las tres hermanas", 220, C(0.4f, 0.6f, 0.3f), C(0.9f, 0.8f, 0.3f)),
            new Plant(Kind.Grass, 8, "Calabaza (Cucurbita pepo), de las tres hermanas", 40, C(0.3f, 0.5f, 0.26f), C(0.9f, 0.55f, 0.15f)),
            new Plant(Kind.Shrub, 4, "Nogal (Carya ovata), de la aldea", 1500, C(0.3f, 0.48f, 0.28f), C(0.55f, 0.5f, 0.3f)) },
          bugs = new[] {
            new Bug("Cicada", 3, 35, C(0.25f, 0.3f, 0.22f), "Cigarra de agosto (Neotibicen)"),
            new Bug("Firefly", 4, 12, C(0.25f, 0.2f, 0.18f), "Luciérnaga (Photinus pyralis)"),
            new Bug("SnowBee", 4, 20, C(0.8f, 0.65f, 0.2f), "Abejorro oriental (Bombus impatiens)"),
            new Bug("Ant", 5, 10, C(0.2f, 0.15f, 0.12f), "Hormiga carpintera (Camponotus pennsylvanicus)"),
            new Bug("Grasshopper", 3, 30, C(0.5f, 0.55f, 0.3f), "Saltamontes (Melanoplus)") },
          townFauna = new[] { new Pet("Woodlouse", 3, 0.7f, C(0.5f, 0.42f, 0.34f), "Perro de la aldea (en el microcosmos, una cochinilla hormiguera)") } };
}
