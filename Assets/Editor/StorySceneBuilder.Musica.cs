using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MICROCOSMOS DE LA MÚSICA (docs/microcosmos-musica.md; Berón 2026-11-05). Como los famosos (StorySceneBuilder.Famosos): UNA historia
/// de la vida pública de cada artista o banda, documentada, respetuosa y con su ficha de fuentes, en el PUEBLO REAL donde ocurrió
/// (CivicBuilder.Musica, su plano; MicroStoryMaps.Musica, su mapa a mano), más «Grandes aportes de las canciones», que reúne en un
/// sitio inventado las canciones que cambiaron el mundo. Reglas de este lote: NINGUNA LETRA de canción (títulos y tema contado con
/// palabras propias), ninguna carátula ni logotipo; las frases son de juego, en la voz de cada uno; lo discutido, en una línea y sin juicio.
/// </summary>
public static partial class StorySceneBuilder
{
    /// <summary>Genera los microcosmos de la música uno tras otro (con la escena principal abierta). Con poca memoria, mejor uno a uno con su menú.</summary>
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música (todos, uno tras otro)")]
    public static void BuildMusic()
    {
        foreach (var s in Stories)
            if (s.display == "Barranquilla" || s.display == "Vega Baja" || s.display == "Montreal" || s.display == "Gary" || s.display == "Seúl" || s.display == "Kingston" || s.display == "Belfast" || s.display == "Long Island" || s.display == "Grandes aportes de las canciones")
                try { BuildOne(s.scene); } catch (System.Exception e) { Debug.LogWarning($"[Historias] {s.scene}: {e.Message}"); }
    }

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/Shakira (Barranquilla, 2009)")]
    public static void BuildShakira() => BuildOne("Microcosmos_Shakira");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/Bad Bunny (Vega Baja, 2016)")]
    public static void BuildBadBunny() => BuildOne("Microcosmos_BadBunny");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/John Lennon (Montreal, 1969)")]
    public static void BuildJohnLennon() => BuildOne("Microcosmos_JohnLennon");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/Michael Jackson y los Jackson 5 (Gary, 1966)")]
    public static void BuildMichaelJackson() => BuildOne("Microcosmos_MichaelJackson");
    // ══FIN DE LOS MENÚS══

    // Un micrófono de pie, un amplificador y una tarima baja (piezas de la historia, en el claro).
    static void Mic(Transform p, Vector3 at, Material metal)
    {
        Prop(p, "Pie de micrófono", at + new Vector3(0f, 0.8f, 0f), new Vector3(0.05f, 1.6f, 0.05f), metal, false);
        Prop(p, "Micrófono", at + new Vector3(0f, 1.65f, 0f), new Vector3(0.09f, 0.18f, 0.09f), M("Rejilla_oscura", 0.15f, 0.15f, 0.17f), false);
    }
    static void Amp(Transform p, Vector3 at, Material box) => Prop(p, "Amplificador", at + new Vector3(0f, 0.4f, 0f), new Vector3(0.9f, 0.8f, 0.5f), box);
    static Transform Stage(Transform t, string name, Vector3 pos, string label, float w, float d, Material wood)
    {
        Transform s = Spot(t, name, pos, label, 3f);
        Prop(s, "Tarima", new Vector3(0f, 0.15f, 0f), new Vector3(w, 0.3f, d), wood, false);
        return s;
    }

    // ══ SHAKIRA · el colegio Pies Descalzos de La Playa (Barranquilla), 2009 · Aula ══════════════════════════════════════════════

    static int Shakira(Transform t)
    {
        Material white = M("Pared_del_colegio", 0.95f, 0.93f, 0.85f), blue = M("Azul_del_colegio", 0.25f, 0.5f, 0.6f), wood = M("Madera_de_pupitre", 0.6f, 0.45f, 0.3f);
        Material book = M("Cuaderno", 0.9f, 0.75f, 0.3f), cement = M("Cemento_del_patio", 0.72f, 0.7f, 0.66f), pot = M("Olla_de_aluminio", 0.75f, 0.76f, 0.78f), yellow = M("Pollera", 0.92f, 0.72f, 0.2f);

        // 1 · LA ENTRADA Y EL AULA: la pizarra y los pupitres del primer día.
        Transform gate = Spot(t, "Entrada del colegio", new Vector3(0f, 0f, 16f), "Colegio Pies Descalzos de La Playa (2009)", 4f);
        Prop(gate, "Pilar de la entrada", new Vector3(-3f, 1.2f, 0f), new Vector3(0.6f, 2.4f, 0.6f), blue);
        Prop(gate, "Pilar de la entrada", new Vector3(3f, 1.2f, 0f), new Vector3(0.6f, 2.4f, 0.6f), blue);
        Transform room = Spot(t, "Aula de primero", new Vector3(-11f, 0f, 10f), "El aula de primero: el primer día", 3f);
        Prop(room, "Pizarra", new Vector3(0f, 1.2f, 3f), new Vector3(3f, 1.2f, 0.1f), M("Pizarra_verde", 0.15f, 0.3f, 0.22f), false);
        for (int k = 0; k < 4; k++) Prop(room, "Pupitre", new Vector3(-2.4f + k * 1.6f, 0.38f, 0f), new Vector3(0.9f, 0.76f, 0.6f), wood);

        // 2 · EL CENSO: casa por casa, antes de levantar una pared (2007).
        Transform census = Spot(t, "Mesa del censo", new Vector3(-18f, 0f, 0f), "El censo de La Playa: casa por casa (2007)", 3f);
        Prop(census, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 0.9f), wood);
        Prop(census, "Carpetas del censo", new Vector3(0f, 0.86f, 0f), new Vector3(0.9f, 0.12f, 0.5f), white, false);

        // 3 · LOS CUADERNOS, que hay que llevar al aula.
        var books = Items(t, book, new Vector3(0.5f, 0.2f, 0.4f), ("Caja de cuadernos", new Vector3(8f, 0.1f, -6f)), ("Caja de lápices", new Vector3(11f, 0.1f, -3f)), ("Caja de libros de lectura", new Vector3(14f, 0.1f, -7f)));

        // 4 · LA ESCUELA ENTERA: comida, cancha y familias (el modelo de la fundación).
        Transform lunch = Spot(t, "Comedor escolar", new Vector3(17f, 0f, 8f), "El comedor: nadie aprende con hambre", 3f);
        Prop(lunch, "Olla del almuerzo", new Vector3(0f, 0.5f, 0f), new Vector3(1f, 1f, 1f), pot);
        Transform court = Spot(t, "Cancha del colegio", new Vector3(14f, 0f, 18f), "La cancha", 3f);
        Prop(court, "Cancha de cemento", new Vector3(0f, 0.02f, 0f), new Vector3(10f, 0.04f, 6f), cement, false);
        Transform families = Spot(t, "Taller de las familias", new Vector3(-20f, 0f, 14f), "Taller de las familias (coser y vender)", 3f);
        Prop(families, "Mesa de costura", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 0.9f), wood);
        var school = new List<Transform> { lunch, court, families };

        // 5 · LA DANZA: el patio donde ensaya el grupo del colegio.
        Transform dance = Spot(t, "Patio de danza", new Vector3(4f, 0f, 2f), "El patio de danza del colegio", 3f);
        Prop(dance, "Suelo del patio", new Vector3(0f, 0.02f, 0f), new Vector3(7f, 0.04f, 5f), cement, false);
        for (int k = 0; k < 3; k++) Prop(dance, "Falda de cumbia (colgada)", new Vector3(-2f + k * 2f, 0.9f, 2.4f), new Vector3(0.7f, 1.2f, 0.05f), yellow, false);

        // La gente: insectos-humanos. Shakira, una ABEJA (la abeja baila para contar a la colmena dónde hay comida: baila y da), con su
        // melena y una camisa blanca sencilla; ropa de diario, ni vestuario de escenario ni nada copiado de sus portadas.
        Color shirt = new Color(0.95f, 0.94f, 0.9f), teacher = new Color(0.25f, 0.5f, 0.6f), fisher = new Color(0.55f, 0.6f, 0.5f);
        GameObject sh = Who(t, "Shakira", new Vector3(2.5f, 0f, 12f), true, 1.57f, "SnowBee"); CubeInsect.WearShirt(sh, shirt);
        GameObject rector = Who(t, "Rector del colegio (Néstor Martínez)", new Vector3(-6f, 0f, 14f), false, 1.72f, "Ant"); CubeInsect.WearShirt(rector, teacher);
        GameObject census1 = Who(t, "Censista de la fundación (Ruth)", new Vector3(-16f, 0f, -2f), true); CubeInsect.WearShirt(census1, teacher); Routine(census1, census, families);
        GameObject neighbor = Who(t, "Vecina de La Playa (Momo)", new Vector3(16f, 0f, 2f), true); CubeInsect.WearShirt(neighbor, fisher); Routine(neighbor, lunch, families);
        GameObject dancer = Who(t, "Maestra de danza (Medea)", new Vector3(2f, 0f, 6f), true, 1.6f); CubeInsect.WearShirt(dancer, new Color(0.92f, 0.72f, 0.2f));
        int n = 5;

        Mission(t, "Shakira y el colegio de La Playa: pies descalzos, la primera escuela en Barranquilla (2009)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Shakira en la entrada del colegio", sh.transform, "Shakira", "De niña vi a otros niños descalzos en un parque de mi ciudad, y no se me olvidó. Hoy abre sus puertas este colegio. La canción ya la hice; esto es lo que quería hacer con ella."),
                Reach("Ve a la mesa del censo", census, "Censista de la fundación", "Antes de poner un ladrillo fuimos casa por casa: quién vive aquí, de dónde llegó, cuántos niños no van a la escuela. Muchas familias llegaron huyendo de la violencia."),
                Collect("Lleva los cuadernos, los lápices y los libros al aula de primero", books, room, "Rector del colegio", "Mañana estrenan. Para muchos es el primer pupitre de su vida."),
                Visit("Recorre el comedor, la cancha y el taller de las familias", school, "Censista de la fundación", "Una escuela sola no basta: comida, juego, y trabajo para las madres y los padres. Si la familia está bien, el niño se queda."),
                Talk("Habla con el rector", rector.transform, "Rector del colegio", "Este colegio es público: aquí no se paga. Lo que hemos construido entre todos tiene que aguantar cuando se apaguen las cámaras."),
                Hold("Quédate mirando el ensayo del grupo de danza", dance, 10f, "Maestra de danza", "Cumbia y mapalé, descalzas, como se bailan aquí. El arte también es materia."),
                Talk("Habla con la vecina de La Playa", neighbor.transform, "Vecina de La Playa", "Llegué sin nada. Mis hijos andaban todo el día en la orilla. Hoy los vi salir con uniforme, y eso no me lo quita nadie."),
                Talk("Vuelve con Shakira", sh.transform, "Shakira", "Me dan las gracias por cantar, pero son ellos los que van a cambiar este país. Yo solo quiero que nadie les cierre la puerta."),
            },
            "El corregimiento de La Playa, junto a la Ciénaga de Mallorquín (Barranquilla, Colombia), en 2009, sobre el Aula del santuario a escala de hormiga. La Fundación Pies Descalzos, que Shakira creó en 1997, abre su primer colegio en la ciudad donde ella nació. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Shakira Isabel Mebarak Ripoll nació en Barranquilla el 2 de febrero de 1977. Ha contado que de niña, cuando su familia pasaba apuros, su padre la llevó a un parque de la ciudad para que viera a los niños que vivían en la calle, y que aquello la marcó. En 1995 publicó el álbum Pies Descalzos, y la canción del mismo título cuenta con otras palabras esa misma mirada (esta historia los nombra sin reproducir sus letras).",
                "En 1997, tras su primer éxito internacional, creó la Fundación Pies Descalzos, primero para atender a niños desplazados por la violencia y su nutrición. Desde 2003 se centró en la educación pública de calidad: colegio, alimentación, apoyo psicosocial, ingresos para las familias y trabajo con la comunidad. En 2004 empezó en Quibdó y Soacha; en 2007 compró el terreno de La Playa, hizo el censo educativo y el trabajo con el barrio; en 2009 abrió el colegio, el primero suyo en Barranquilla, para más de 1000 estudiantes al año. Néstor Martínez fue su rector desde el comienzo.",
                "La fundación cuenta 16 colegios en Colombia en 2024 (entre ellos Lomas del Peyé en Cartagena, 2014, y Nuevo Bosque en Barranquilla, 2023) y más de 124 000 niños y jóvenes al año; las cifras varían según se cuenten colegios construidos o adecuados. Shakira es embajadora de buena voluntad de UNICEF desde 2003.",
                "No consta el día exacto de la apertura ni una visita suya ese día: la escena junta en una jornada lo que pasó entre 2007 y 2009, y las frases son de juego. Shakira tuvo un pleito fiscal en España que cerró con un acuerdo en noviembre de 2023; esta historia se limita al colegio.",
            },
            new[]
            {
                "Fundación Pies Descalzos, «Historia» (fundacionpiesdescalzos.com/historia): cronología 1997–2024.",
                "El Heraldo (6-10-2012), «Llegar a Pies Descalzos es como regresar a casa: María Emma Mejía» (el rector Néstor Martínez, desde 2009; el énfasis en arte).",
                "El Colombiano, «Shakira y la Fundación Pies Descalzos: educación en Colombia» (elcolombiano.com).",
                "El Universal (2023), «Shakira entrega megacolegio en el barrio El Bosque de Barranquilla».",
                "Fundación Pies Descalzos (2024), Informe de gestión 2023.",
            },
            "Shakira", "Una canción dura tres minutos. Un colegio, toda la vida de quien pasa por él.");
        return n;
    }

    // ══ BAD BUNNY · Vega Baja (Puerto Rico), 2016 · Informática (pendiente: sala de grabación) ═══════════════════════════════════

    static int BadBunny(Transform t)
    {
        Material steel = M("Carrito_de_compra", 0.75f, 0.76f, 0.78f), bag = M("Bolsa_de_compra", 0.92f, 0.9f, 0.84f), wood = M("Madera_de_la_casa", 0.6f, 0.45f, 0.3f);
        Material zinc = M("Zinc_del_techo", 0.6f, 0.62f, 0.62f), screen = M("Pantalla_del_portatil", 0.12f, 0.16f, 0.22f), pew = M("Banco_de_iglesia", 0.5f, 0.35f, 0.25f), red = M("Rótulo_del_súper", 0.8f, 0.2f, 0.15f);

        // 1 · EL SUPERMERCADO: la caja, la cinta y las bolsas del turno de la tarde.
        Transform store = Spot(t, "Caja del supermercado", new Vector3(10f, 0f, 14f), "Econo Plaza Vega Baja: la caja 4", 3.5f);
        Prop(store, "Cinta de la caja", new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 0.7f), steel);
        Prop(store, "Rótulo de la caja", new Vector3(0f, 2f, 0f), new Vector3(0.8f, 0.5f, 0.06f), red, false);
        var bags = Items(t, bag, new Vector3(0.5f, 0.6f, 0.4f), ("Bolsas de arroz y habichuelas", new Vector3(12f, 0.3f, 10f)), ("Bolsas de la compra de la semana", new Vector3(15f, 0.3f, 12f)), ("Caja de agua", new Vector3(16f, 0.3f, 8f)));
        Transform car = Spot(t, "Carro de la clienta", new Vector3(18f, 0f, 1f), "El carro de doña Carmen, en el estacionamiento", 2.5f);
        Prop(car, "Baúl del carro", new Vector3(0f, 0.55f, 0f), new Vector3(1.8f, 1.1f, 1.4f), M("Carro_azul", 0.25f, 0.35f, 0.55f));

        // 2 · LA CASA Y LA IGLESIA de Almirante Sur.
        Transform home = Spot(t, "Casa de Almirante Sur", new Vector3(-12f, 0f, 12f), "La casa de la familia, en Almirante Sur", 3.5f);
        Prop(home, "Terraza", new Vector3(0f, 0.12f, 0f), new Vector3(5f, 0.24f, 3.5f), wood);
        Prop(home, "Techo de zinc de la marquesina (solo sus postes)", new Vector3(-2.2f, 1.2f, -1.5f), new Vector3(0.12f, 2.4f, 0.12f), zinc);
        Transform church = Spot(t, "Iglesia del barrio", new Vector3(-20f, 0f, 2f), "La iglesia: cantó en su coro hasta los 13 años", 3.5f);
        for (int k = 0; k < 3; k++) Prop(church, "Banco del coro", new Vector3(0f, 0.25f, -1.5f + k * 1.5f), new Vector3(2.4f, 0.5f, 0.45f), pew);

        // 3 · LO QUE LE MUEVE: la radio de la universidad, los discos de casa y el portátil.
        Transform radio = Spot(t, "Radio de la universidad", new Vector3(-6f, 0f, 19f), "UPR de Arecibo: quería ser locutor de radio", 3f);
        Prop(radio, "Mesa de locutor", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 0.8f), wood);
        Mic(radio, new Vector3(0.3f, 0.8f, 0f), steel);
        Transform records = Spot(t, "Discos de casa", new Vector3(-16f, 0f, 16f), "Los discos de su madre: salsa, merengue, baladas", 2.5f);
        Prop(records, "Estante de discos", new Vector3(0f, 0.6f, 0f), new Vector3(1.4f, 1.2f, 0.4f), wood);
        Transform road = Spot(t, "Parada de la PR-2", new Vector3(4f, 0f, 3f), "La PR-2: la carretera entre Vega Baja y Arecibo", 2.5f);
        Prop(road, "Poste de la parada", new Vector3(0f, 1.2f, 0f), new Vector3(0.12f, 2.4f, 0.12f), steel);
        var path = new List<Transform> { radio, records, road };

        // 4 · LA NOCHE: el portátil de su cuarto y SoundCloud.
        Transform desk = Spot(t, "Cuarto de Benito", new Vector3(-8f, 0f, 3f), "Su cuarto, de noche: el portátil y SoundCloud", 3f);
        Prop(desk, "Mesa del cuarto", new Vector3(0f, 0.38f, 0f), new Vector3(1.6f, 0.76f, 0.7f), wood);
        Prop(desk, "Portátil", new Vector3(0f, 0.85f, 0f), new Vector3(0.4f, 0.25f, 0.04f), screen, false);
        Mic(desk, new Vector3(1.2f, 0f, 0f), steel);

        // La gente: insectos-humanos. Benito, un GRILLO (canta de noche desde su madriguera y el barrio entero lo oye). Camisa de empaque: sin logotipos.
        Color apron = new Color(0.8f, 0.2f, 0.15f), blouse = new Color(0.55f, 0.6f, 0.75f), shirt = new Color(0.35f, 0.4f, 0.45f);
        GameObject bb = Who(t, "Bad Bunny (Benito Martínez)", new Vector3(6f, 0f, 11f), false, 1.8f, "Cricket"); CubeInsect.WearShirt(bb, apron);
        GameObject lady = Who(t, "Doña Carmen, clienta (Ruth)", new Vector3(17f, 0f, 4f), true, 1.58f); CubeInsect.WearShirt(lady, blouse);
        GameObject mom = Who(t, "Lysaurie, su madre, maestra de escuela", new Vector3(-10f, 0f, 9f), true, 1.62f, "SnowBee"); CubeInsect.WearShirt(mom, blouse); Routine(mom, home, church);
        GameObject dad = Who(t, "Tito, su padre, camionero", new Vector3(-14f, 0f, 9f), false, 1.75f, "Ant"); CubeInsect.WearShirt(dad, shirt);
        GameObject luian = Who(t, "DJ Luian", new Vector3(2f, 0f, 19f), false, 1.8f, "Mantis"); CubeInsect.WearShirt(luian, new Color(0.12f, 0.12f, 0.14f));
        int n = 5;

        Mission(t, "Bad Bunny en Vega Baja: del empaque del súper a SoundCloud (2016)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Benito en la caja del supermercado", bb.transform, "Benito", "Empaco compra por las tardes y estudio en Arecibo. No me quejo: aquí conozco a todo Vega Baja. Y mientras empaco, voy pensando canciones."),
                Collect("Lleva la compra al carro de doña Carmen", bags, car, "Doña Carmen", "Gracias, mijo. Tú eres el hijo de la maestra, ¿verdad? El que cantaba en la iglesia."),
                Reach("Ve a la iglesia del barrio", church, "Lysaurie", "Cantó en el coro hasta los trece. Aquí aprendió que una voz se junta con otras."),
                Talk("Habla con su madre en Almirante Sur", mom.transform, "Lysaurie", "En casa siempre sonaba música mientras limpiábamos. Su padre maneja camiones todo el día. Le pedimos una sola cosa: que estudie."),
                Visit("Recorre lo que le mueve: la radio, los discos y la carretera", path, "Benito", "Quería ser locutor. Salsa de mami, reguetón de la calle, rock, de todo. Puerto Rico entero cabe en una guagua por la PR-2."),
                Hold("De noche, en su cuarto: acompáñale mientras sube una canción", desk, 10f, "Benito", "Se llama «Diles». Sin sello ni estudio: una computadora, un micrófono y lo que hay aquí. Le doy a subir y me voy a dormir, que mañana empaco."),
                Talk("Un mensaje: habla con DJ Luian", luian.transform, "DJ Luian", "Oí tu tema en SoundCloud. ¿Tú sigues trabajando en el súper? Hay que hablar."),
                Talk("Vuelve con Benito", bb.transform, "Benito", "Si esto funciona, no me voy de aquí. Lo que cante va a sonar a Vega Baja."),
            },
            "Vega Baja, en la costa norte de Puerto Rico, en 2016, sobre el piso de Informática del santuario a escala de hormiga (pendiente de su piso: una sala de grabación). Benito Martínez tiene 22 años, empaca compras en un supermercado, estudia comunicación audiovisual en Arecibo y por la noche hace canciones en su cuarto. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Benito Antonio Martínez Ocasio nació el 10 de marzo de 1994 en Bayamón y creció en el barrio Almirante Sur de Vega Baja. Su padre era camionero; su madre, maestra de escuela. Cantó en el coro de su iglesia hasta los 13 años. Se graduó de la escuela superior en 2012 y estudió comunicación audiovisual en la Universidad de Puerto Rico en Arecibo, con la idea de ser locutor, sin terminar la carrera.",
                "Trabajaba como empacador y cajero en un supermercado Econo de Vega Baja. En 2016 la canción «Diles», que había subido a SoundCloud, llamó la atención del productor DJ Luian, que lo fichó para Hear This Music (esta historia nombra sus canciones sin reproducir sus letras). Su primer álbum, X 100pre, salió el 24 de diciembre de 2018.",
                "Nunca se fue del todo: en 2018 creó la Good Bunny Foundation, que cada Navidad reparte juguetes, instrumentos y material deportivo a jóvenes de la isla; el 22 de julio de 2019 se unió a las protestas que cerraron el Expreso Las Américas y pidieron la renuncia del gobernador; en 2022 el video de «El Apagón» denunció los cortes de luz y la gentrificación. En 2025, Debí Tirar Más Fotos y una residencia de 30 conciertos en San Juan celebraron Puerto Rico; en 2026 hizo el descanso de la Super Bowl y aquel disco fue el primero enteramente en español en ganar el Grammy al álbum del año.",
                "Las frases son de juego y el diálogo con DJ Luian resume lo que él ha contado; doña Carmen es inventada y no se sabe en qué cuarto ni con qué equipo grabó exactamente. Algunas de sus letras y gestos públicos han generado debate; esta historia se limita a 2016.",
            },
            new[]
            {
                "Wikipedia, «Bad Bunny» (biografía, activismo y fundación; con sus referencias de Rolling Stone y Billboard).",
                "AOL (2025), «Bad Bunny's Journey: From Grocery Clerk to Super Bowl Halftime Headliner».",
                "Boston Herald (3-3-2020), «Bad Bunny leaps from grocery bagger to rap star».",
                "LOS40 (2025), «De empacador a ídolo global: así comenzó la historia de Bad Bunny».",
            },
            "Benito", "Lo que hago lo hago desde aquí, y lo que gano vuelve aquí.");
        return n;
    }

    // ══ JOHN LENNON · el Bed-In de Montreal, 1 de junio de 1969 · Sala de Ballet (pendiente: escenario y sala de grabación) ════════
    //    Se eligió Montreal y no Liverpool: es un acto público, cívico y bien documentado, hecho a dos con Yoko Ono, y de esa habitación
    //    salió la canción que cantó medio millón de personas en Washington (docs/microcosmos-musica.md, «Lennon: por qué Montreal»).

    static int JohnLennon(Transform t)
    {
        Material linen = M("Sábana_blanca", 0.96f, 0.95f, 0.92f), wood = M("Madera_del_hotel", 0.55f, 0.4f, 0.28f), paper = M("Cartel_de_papel", 0.97f, 0.97f, 0.94f);
        Material steel = M("Cromo_del_pie", 0.75f, 0.76f, 0.78f), tape = M("Magnetófono", 0.45f, 0.47f, 0.5f), acorn = M("Bellota", 0.55f, 0.4f, 0.2f), ink = M("Letra_del_cartel", 0.1f, 0.1f, 0.12f);

        // 1 · LA HABITACIÓN 1742: la cama contra la ventana y los carteles escritos a mano.
        Transform bed = Spot(t, "Habitación 1742", new Vector3(0f, 0f, 15f), "Queen Elizabeth Hotel, habitación 1742", 3.5f);
        Prop(bed, "Colchón", new Vector3(0f, 0.3f, 0f), new Vector3(2.4f, 0.6f, 2.2f), linen);
        Prop(bed, "Cabecero", new Vector3(0f, 0.8f, 1.2f), new Vector3(2.6f, 1.2f, 0.12f), wood);
        for (int k = 0; k < 3; k++)
        {
            Prop(bed, "Cartel de papel en la ventana (escrito a mano, por la paz)", new Vector3(-2.2f + k * 2.2f, 2.3f, 1.3f), new Vector3(1.6f, 1f, 0.03f), paper, false);
            Prop(bed, "Trazo del cartel", new Vector3(-2.2f + k * 2.2f, 2.3f, 1.27f), new Vector3(1.2f, 0.25f, 0.02f), ink, false);
        }

        // 2 · LA PRENSA y 3 · LAS BELLOTAS (abril de 1969: dos bellotas a cada jefe de Estado, para plantar por la paz).
        Transform press = Spot(t, "Mesa de prensa", new Vector3(-16f, 0f, 6f), "Prensa de todo el mundo, de nueve de la mañana a nueve de la noche", 3f);
        Prop(press, "Mesa de teléfonos", new Vector3(0f, 0.4f, 0f), new Vector3(2.2f, 0.8f, 0.9f), wood);
        Transform post = Spot(t, "Paquetes de bellotas", new Vector3(14f, 0f, 3f), "Bellotas por la paz, para los jefes de Estado", 3f);
        Prop(post, "Mesa de correos", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 0.9f), wood);
        var acorns = Items(t, acorn, new Vector3(0.35f, 0.3f, 0.35f), ("Caja de bellotas", new Vector3(9f, 0.15f, -6f)), ("Caja de bellotas", new Vector3(12f, 0.15f, -8f)), ("Caja de bellotas", new Vector3(15f, 0.15f, -5f)));

        // 4 · LOS VISITANTES: tres mesas de conversación.
        Transform v1 = Spot(t, "Sillón de la entrevista", new Vector3(-20f, 0f, 14f), "Entrevistas por radio y teléfono (CBC)", 2.5f);
        Prop(v1, "Sillón", new Vector3(0f, 0.3f, 0f), new Vector3(1f, 0.6f, 1f), wood);
        Transform v2 = Spot(t, "Rincón de la guitarra", new Vector3(18f, 0f, 14f), "Tommy Smothers trae su guitarra", 2.5f);
        Prop(v2, "Taburete", new Vector3(0f, 0.25f, 0f), new Vector3(0.5f, 0.5f, 0.5f), wood);
        Transform v3 = Spot(t, "Puerta del pasillo", new Vector3(-8f, 0f, 22f), "La puerta: entra quien quiera hablar de paz", 2.5f);
        Prop(v3, "Marco de la puerta", new Vector3(0f, 1.1f, 0f), new Vector3(1.2f, 2.2f, 0.12f), wood, false);
        var visitors = new List<Transform> { v1, v2, v3 };

        // 5 · LA GRABACIÓN: cuatro micrófonos y un magnetófono de cuatro pistas.
        Transform rec = Spot(t, "Equipo de André Perry", new Vector3(8f, 0f, 9f), "Cuatro micrófonos y un magnetófono de cuatro pistas", 3f);
        Prop(rec, "Magnetófono de cuatro pistas", new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 0.8f), tape);
        for (int k = 0; k < 4; k++) Mic(rec, new Vector3(-4f + k * 2.5f, 0f, 2.5f), steel);

        // La gente: insectos-humanos. Lennon, una CIGARRA (la que más canta, tras años quieta: aquí quedarse quieto en una cama es la protesta),
        // de pijama blanco; Yoko Ono, una LUCIÉRNAGA (su arte conceptual es una señal pequeña en la oscuridad), de blanco.
        Color white = new Color(0.95f, 0.94f, 0.9f), suit = new Color(0.25f, 0.27f, 0.32f), plaid = new Color(0.55f, 0.35f, 0.3f);
        GameObject john = Who(t, "John Lennon", new Vector3(-1f, 0f, 12f), false, 1.79f, "Cicada"); CubeInsect.WearShirt(john, white);
        GameObject yoko = Who(t, "Yoko Ono", new Vector3(2f, 0f, 12f), true, 1.57f, "Firefly"); CubeInsect.WearShirt(yoko, white);
        GameObject taylor = Who(t, "Derek Taylor, jefe de prensa", new Vector3(-13f, 0f, 3f), false, 1.76f, "Ant"); CubeInsect.WearShirt(taylor, suit);
        GameObject perry = Who(t, "André Perry, técnico de sonido", new Vector3(10f, 0f, 6f), false, 1.72f, "Cricket"); CubeInsect.WearShirt(perry, plaid);
        GameObject smothers = Who(t, "Tommy Smothers", new Vector3(16f, 0f, 11f), false, 1.8f, "Grasshopper"); CubeInsect.WearShirt(smothers, suit); Routine(smothers, v2, rec);
        GameObject reporter = Who(t, "Periodista de Montreal (Momo)", new Vector3(-18f, 0f, 3f), true); CubeInsect.WearShirt(reporter, plaid); Routine(reporter, press, v1);
        int n = 6;

        Mission(t, "John Lennon y Yoko Ono en Montreal: una cama por la paz (1969)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con John y Yoko en la cama de la habitación 1742", john.transform, "John Lennon", "Los periódicos nos persiguen por casarnos. Pues que vengan: les damos un titular sobre la paz. No nos dejan entrar en Estados Unidos, así que nos quedamos justo al lado y hablamos desde aquí."),
                Reach("Ve a la mesa de prensa", press, "Derek Taylor", "Una semana de nueve a nueve. Cualquiera puede entrar y preguntar. Unos se ríen, otros se enfadan, y todos lo publican: de eso se trata."),
                Collect("Lleva las bellotas a la mesa de correos", acorns, post, "Yoko Ono", "Dos bellotas a cada jefe de Estado, una por John y otra por mí. Si plantan un árbol, habrán hecho algo por la paz."),
                Visit("Atiende a los visitantes: la entrevista, la guitarra y la puerta", visitors, "Derek Taylor", "Periodistas, un poeta, un rabino, una cantante, un cómico, y un dibujante que ha venido a pelearse con ellos. También él entra: esa es la idea."),
                Talk("Habla con Yoko Ono", yoko.transform, "Yoko Ono", "La idea de quedarse quieto es muy simple. Por eso cuesta tanto que la tomen en serio. La guerra se hace ocupada; la paz también necesita tiempo."),
                Reach("1 de junio: ve con André Perry a preparar la grabación", rec, "André Perry", "Cuatro micrófonos en el suelo y un magnetófono que he subido yo mismo. No es un estudio: es un cuarto de hotel lleno de gente."),
                Hold("Graba con todos: quédate cantando en la habitación", rec, 12f, "John Lennon", "Todos a la vez, aunque desafinen. Un estribillo sencillo que pueda cantar cualquiera. Aquel periodista preguntó para qué era esto, y la respuesta se ha convertido en canción."),
                Talk("Vuelve con John", john.transform, "John Lennon", "Si pudiera vender la paz como se vende un jabón, lo haría. Hoy hemos hecho el anuncio."),
            },
            "El Queen Elizabeth Hotel de Montreal (Canadá), del 26 de mayo al 1 de junio de 1969, sobre la Sala de Ballet del santuario a escala de hormiga (pendiente de su piso: escenario y sala de grabación). John Lennon y Yoko Ono convierten su cama en una protesta contra la guerra de Vietnam, y el último día graban en la habitación una canción con todos los que están allí. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "John Lennon nació en Liverpool en 1940 y fundó allí los Beatles; Yoko Ono, artista conceptual, nació en Tokio en 1933. Se casaron en marzo de 1969 y usaron la expectación de su boda para protestar contra la guerra de Vietnam: del 25 al 31 de marzo recibieron a la prensa en la cama del Hilton de Ámsterdam, cada día de nueve a nueve. En abril enviaron bellotas a jefes de Estado para que plantaran árboles por la paz.",
                "Querían repetirlo en Nueva York, pero a Lennon no le dejaron entrar en Estados Unidos por una condena por cannabis de 1968. Tras una noche en las Bahamas fueron a Canadá y eligieron Montreal, cerca de la prensa de Nueva York. Del 26 de mayo al 1 de junio ocuparon las habitaciones 1738–1744 del Queen Elizabeth Hotel; pasaron por allí Timothy Leary, Tommy Smothers, Dick Gregory, Allen Ginsberg, Petula Clark, el rabino Abraham Feinberg y Derek Taylor. El dibujante Al Capp discutió con ellos con dureza y no cantó.",
                "El 1 de junio, André Perry, dueño de un estudio de la ciudad, grabó en la habitación 1742, con cuatro micrófonos y un magnetófono de cuatro pistas, Give Peace a Chance (esta historia la nombra sin reproducir sus letras: el título salió de una respuesta de Lennon a un periodista que le preguntó para qué era aquello). Llegó al n.º 2 en el Reino Unido y al 14 en Estados Unidos. El 15 de noviembre de 1969, en la Moratoria contra la guerra de Vietnam, alrededor de medio millón de personas la cantaron en Washington, guiadas por Pete Seeger. Lennon dijo después que debió acreditarla a Yoko Ono y no a McCartney, que no intervino.",
                "La prensa de entonces recibió el acto con mucha burla: esa crítica es parte de la historia. Las frases son de juego, salvo la anécdota del título; los detalles de quién estaba en cada momento varían según las fuentes. Lennon reconoció en entrevistas haber sido violento con mujeres en su juventud; esta historia se limita a aquella semana.",
            },
            new[]
            {
                "Wikipedia, «Bed-in» y «Give Peace a Chance» (fechas, habitaciones, participantes, listas de ventas, Moratoria).",
                "Radio Canada International (26-5-2016), «History: May 26, 1969: Montreal, the bed-in for peace».",
                "Global News (2019), «Montreal marks 50th anniversary of John Lennon, Yoko Ono bed-in for peace».",
                "uDiscover Music, «‘Give Peace A Chance’: John Lennon and Yoko Ono bed down in Hot 100».",
                "Ultimate Classic Rock, «John Lennon's ‘Give Peace a Chance’ bed-in».",
            },
            "John Lennon", "No hace falta ser un experto para pedir que paren.");
        return n;
    }

    // ══ MICHAEL JACKSON · la familia Jackson en Gary (Indiana), 1966 · Sala de Ballet (pendiente: escenario) ═══════════════════════
    //    La historia es la de una familia obrera de una ciudad del acero y el primer concurso que ganan sus hijos. Michael tiene 7 años: nada
    //    de su vida adulta entra en la escena; la ficha nombra lo discutido en una línea y deja claro que aquí no se juzga.

    static int MichaelJackson(Transform t)
    {
        Material siding = M("Tablas_de_la_casa", 0.9f, 0.88f, 0.82f), wood = M("Madera_vieja", 0.5f, 0.38f, 0.28f), steel = M("Acero_de_Gary", 0.42f, 0.42f, 0.44f), rust = M("Óxido", 0.5f, 0.3f, 0.2f);
        Material guitar = M("Guitarra_de_Tito", 0.55f, 0.3f, 0.15f), conga = M("Conga", 0.6f, 0.42f, 0.25f), curtain = M("Telón_rojo", 0.55f, 0.12f, 0.14f);

        // 1 · LA CASA DE 2300 JACKSON STREET: dos dormitorios para once personas; la sala donde se ensaya.
        Transform house = Spot(t, "Casa de 2300 Jackson Street", new Vector3(-8f, 0f, 15f), "2300 Jackson Street: dos dormitorios, once personas", 3.5f);
        Prop(house, "Porche", new Vector3(0f, 0.12f, 0f), new Vector3(6f, 0.24f, 3f), siding);
        Transform kitchen = Spot(t, "Cocina de Katherine", new Vector3(-14f, 0f, 9f), "La cocina: Katherine canta y toca el clarinete", 2.5f);
        Prop(kitchen, "Mesa de la cocina", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 1f), wood);
        Transform room = Spot(t, "Sala de ensayo", new Vector3(0f, 0f, 9f), "La sala de estar: aquí ensayan", 3f);
        Prop(room, "Suelo de la sala", new Vector3(0f, 0.02f, 0f), new Vector3(6f, 0.04f, 5f), wood, false);

        // 2 · LA ACERÍA: la puerta de la US Steel, donde Joe maneja una grúa.
        Transform mill = Spot(t, "Puerta de la acería", new Vector3(16f, 0f, 14f), "US Steel, Gary Works: el turno de Joe", 4f);
        for (int k = -1; k <= 1; k += 2) Prop(mill, "Pilar de la puerta", new Vector3(k * 2.5f, 1.6f, 0f), new Vector3(0.6f, 3.2f, 0.6f), steel);
        Prop(mill, "Viga de la puerta", new Vector3(0f, 3.3f, 0f), new Vector3(5.6f, 0.4f, 0.5f), rust);

        // 3 · LOS INSTRUMENTOS de la familia.
        var kit = Items(t, guitar, new Vector3(0.4f, 0.9f, 0.2f), ("La guitarra de Tito", new Vector3(5f, 0.45f, -4f)), ("Las congas de Michael", new Vector3(9f, 0.45f, -2f)), ("La pandereta de Marlon", new Vector3(12f, 0.45f, -6f)));
        Prop(t, "Funda vacía de guitarra", new Vector3(5f, 0.1f, -6f), new Vector3(0.5f, 0.2f, 1.1f), conga, false);

        // 4 · EL INSTITUTO: el escenario de Roosevelt High School.
        Transform stage = Stage(t, "Escenario de Roosevelt High School", new Vector3(10f, 0f, 3f), "Theodore Roosevelt High School: el concurso de talentos", 8f, 4f, wood);
        Prop(stage, "Telón", new Vector3(0f, 2f, 2.1f), new Vector3(8f, 4f, 0.1f), curtain, false);
        Mic(stage, new Vector3(-1f, 0.3f, 0f), steel); Mic(stage, new Vector3(1f, 0.3f, 0f), steel);

        // La gente: insectos-humanos. Michael (7 años), una LUCIÉRNAGA de los veranos de Indiana (Photinus pyralis: cada especie tiene su patrón
        // exacto de destellos, una coreografía de luz). Ropa de diario de 1966, nada de sus vestuarios de escenario.
        Color white = new Color(0.92f, 0.92f, 0.9f), work = new Color(0.25f, 0.3f, 0.42f), dress = new Color(0.55f, 0.4f, 0.5f);
        GameObject mj = Who(t, "Michael Jackson (7 años)", new Vector3(2f, 0f, 7f), false, 1.22f, "Firefly"); CubeInsect.WearShirt(mj, white);
        GameObject kat = Who(t, "Katherine Jackson", new Vector3(-12f, 0f, 7f), true, 1.68f, "SnowBee"); CubeInsect.WearShirt(kat, dress); Routine(kat, kitchen, house);
        GameObject joe = Who(t, "Joe Jackson", new Vector3(14f, 0f, 11f), false, 1.78f, "DungBeetle"); CubeInsect.WearShirt(joe, work);
        GameObject tito = Who(t, "Tito Jackson", new Vector3(-2f, 0f, 11f), false, 1.45f, "Firefly"); CubeInsect.WearShirt(tito, new Color(0.45f, 0.5f, 0.35f));
        GameObject jermaine = Who(t, "Jermaine Jackson", new Vector3(-4f, 0f, 7f), false, 1.4f, "Firefly"); CubeInsect.WearShirt(jermaine, new Color(0.6f, 0.45f, 0.3f));
        GameObject teacher = Who(t, "Maestra de Roosevelt High School (Medea)", new Vector3(14f, 0f, 0f), true); CubeInsect.WearShirt(teacher, dress);
        int n = 6;

        Mission(t, "Los Jackson en Gary: una familia del acero y su primer concurso (1966)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Katherine en la cocina", kat.transform, "Katherine", "Somos muchos en esta casa y el dinero no sobra. Por las noches cantamos juntos, sobre todo cuando se va la luz. Yo les enseñé esas canciones viejas; lo demás lo traen ellos."),
                Talk("Habla con Tito", tito.transform, "Tito", "Papá guardaba su guitarra en el armario y estaba prohibido tocarla. La cogí y se me rompió una cuerda. Me pilló. Y en vez de terminar ahí, me hizo tocar: entonces vio que sabíamos."),
                Reach("Ve a la puerta de la acería, al cambio de turno", mill, "Joe Jackson", "Grúa, ocho horas, y después el grupo. Yo también toqué con los Falcons. Esta ciudad vive del acero, y mis hijos no van a hacer este turno si puedo evitarlo."),
                Collect("Lleva los instrumentos a la sala de estar", kit, room, "Jermaine", "La guitarra, las congas y la pandereta. Michael es el pequeño del grupo: empezó dando golpes en las congas y ya canta a mi lado."),
                Hold("Quédate en la sala durante el ensayo", room, 10f, "Tito", "Una y otra vez, los pasos y las voces, antes de cenar y después del colegio. Los vecinos ya se saben el repertorio."),
                Talk("Habla con la maestra de Roosevelt High School", teacher.transform, "Maestra", "Este año se apuntan unos hermanos de la calle Jackson. El pequeño es tan bajito que hay que bajarle el micrófono."),
                Hold("Mira el concurso de talentos desde el patio de butacas", stage, 12f, "Maestra", "Jermaine canta un tema de los Temptations y el pequeño, uno de Robert Parker, bailando descalzo por el escenario. El salón entero se ha puesto de pie."),
                Talk("Habla con Michael", mj.transform, "Michael", "Hemos ganado. Yo solo quería bailar como en la tele. Mañana hay colegio, pero mamá dice que hoy podemos cenar tarde."),
            },
            "Gary (Indiana, Estados Unidos), en 1966, sobre la Sala de Ballet del santuario a escala de hormiga (pendiente de su piso: un escenario). En una casa de dos dormitorios junto a la acería, una familia obrera forma un grupo con sus hijos, y gana su primer concurso de talentos en el instituto Theodore Roosevelt. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Gary nació en 1906 alrededor de la gran acería de la US Steel, a orillas del lago Míchigan. Joseph Jackson, antiguo boxeador, manejaba allí una grúa y tocaba la guitarra en los Falcons; Katherine Scruse, testigo de Jehová, tocaba el clarinete y el piano y trabajaba a tiempo parcial en Sears. Compraron la casa de 2300 Jackson Street en 1950: dos dormitorios para ellos y sus nueve hijos. Michael, el octavo, nació el 29 de agosto de 1958.",
                "En 1964 Joe vio que Tito tocaba su guitarra y formó un grupo con Jackie, Tito y Jermaine; Michael entró en las congas y Marlon, en agosto de 1965, en la pandereta. Ganaron su primer concurso de talentos en el instituto Theodore Roosevelt en 1966: Jermaine cantó My Girl y Michael, Barefootin' (esta historia nombra sus canciones sin reproducir sus letras). En agosto de 1967 ganaron también en el Regal de Chicago y en el Apollo de Harlem; en noviembre grabaron Big Boy para Steeltown Records, de Gordon Keith, en Gary, y el 26 de julio de 1968 firmaron con Motown.",
                "De adulto, Michael Jackson fue el cantante más vendido de su generación y coescribió con Lionel Richie We Are the World (1985), que recaudó unos 63 millones de dólares contra el hambre en África y Estados Unidos; en 1992 creó la Heal the World Foundation para los niños.",
                "Lo discutido: Michael contó de adulto que su padre fue violento con él en los ensayos; Joe lo reconoció en parte y sus hermanos lo matizaron. Y Michael fue acusado de abusos a menores: un caso acabó en acuerdo extrajudicial en 1994, otro en absolución en 2005, y siguió habiendo acusaciones después de su muerte en 2009. Esta historia no juzga nada de eso: se limita a una familia de Gary en 1966, cuando él tenía 7 años. Las frases son de juego; la fecha exacta del concurso y lo que cada uno dijo no constan.",
            },
            new[]
            {
                "Wikipedia, «The Jackson 5» y «Michael Jackson» (cronología del grupo; familia; legado y acusaciones).",
                "Wikipedia, «Steeltown Records»; Soultracks, «R.I.P. Gordon Keith».",
                "WBEZ (11-5-2017), «What's that building? The home of the Jackson 5».",
                "Britannica, «The Jackson 5».",
                "Globetrotting Grandpa (2018), «2300 Jackson Street: The humble origins of a musical dynasty».",
            },
            "Katherine", "Aquí no sobraba nada, pero a esta casa nunca le faltó música.");
        return n;
    }

    // ══FIN DE LAS HISTORIAS══
}
