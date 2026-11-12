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
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/BLACKPINK (Seúl, 2016)")]
    public static void BuildBlackpink() => BuildOne("Microcosmos_Blackpink");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/Bob Marley y los Wailers (Kingston, 1978)")]
    public static void BuildWailers() => BuildOne("Microcosmos_Wailers");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/U2 (Belfast, 1998)")]
    public static void BuildU2() => BuildOne("Microcosmos_U2");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/Public Enemy (Long Island, 1989)")]
    public static void BuildPublicEnemy() => BuildOne("Microcosmos_PublicEnemy");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de música/Grandes aportes de las canciones")]
    public static void BuildGrandesCanciones() => BuildOne("Microcosmos_GrandesCanciones");
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
                Talk("Vuelve con Shakira", sh.transform, "Shakira", "Con lo primero que gané fuera, a los veinte años, empecé esto. Me dan las gracias por cantar, pero son ellos los que van a cambiar este país. Yo solo quiero que nadie les cierre la puerta."),
            },
            "El corregimiento de La Playa, junto a la Ciénaga de Mallorquín (Barranquilla, Colombia), en 2009, sobre el Aula del santuario a escala de hormiga. La Fundación Pies Descalzos, que Shakira creó en 1997, abre su primer colegio en la ciudad donde ella nació. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Shakira Isabel Mebarak Ripoll nació en Barranquilla el 2 de febrero de 1977. Ha contado que de niña, cuando su familia pasaba apuros, su padre la llevó a un parque de la ciudad para que viera a los niños que vivían en la calle, y que aquello la marcó. En 1995 publicó el álbum Pies Descalzos, y la canción del mismo título cuenta con otras palabras esa misma mirada (esta historia los nombra sin reproducir sus letras).",
                "En 1997, tras su primer éxito internacional, creó la Fundación Pies Descalzos, primero para atender a niños desplazados por la violencia y su nutrición. Desde 2003 se centró en la educación pública de calidad: colegio, alimentación, apoyo psicosocial, ingresos para las familias y trabajo con la comunidad. En 2004 empezó en Quibdó y Soacha; en 2007 compró el terreno de La Playa, hizo el censo educativo y el trabajo con el barrio; en 2009 abrió el colegio, el primero suyo en Barranquilla, para más de 1000 estudiantes al año. Néstor Martínez fue su rector desde el comienzo.",
                "La fundación cuenta 16 colegios en Colombia en 2024 (entre ellos Lomas del Peyé en Cartagena, 2014, y Nuevo Bosque en Barranquilla, 2023) y más de 124 000 niños y jóvenes al año; las cifras varían según se cuenten colegios construidos o adecuados. Shakira es embajadora de buena voluntad de UNICEF desde 2003.",
                "No consta el día exacto de la apertura ni una visita suya ese día: la escena junta en una jornada lo que pasó entre 2007 y 2009, y las frases son de juego. Shakira tuvo un pleito fiscal en España que cerró con un acuerdo en noviembre de 2023; esta historia se limita al colegio. El momento de esta historia: una cantante de 20 años dedica lo primero que gana a que otros niños tengan escuela; es real, como la fundación y el censo.",
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

    // ══ BAD BUNNY · Vega Baja (Puerto Rico), 2016 · Música ═══════════════════════════════════

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
        Prop(home, "Poste de la marquesina", new Vector3(-2.2f, 1.2f, -1.5f), new Vector3(0.12f, 2.4f, 0.12f), zinc);
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
                Talk("Vuelve con Benito", bb.transform, "Benito", "Si esto funciona, no me voy de aquí. Lo que cante va a sonar a Vega Baja, y lo que gane va a volver: que a ningún chamaco de la isla le falte un instrumento en Navidad."),
            },
            "Vega Baja, en la costa norte de Puerto Rico, en 2016, sobre el piso de Música del santuario a escala de hormiga. Benito Martínez tiene 22 años, empaca compras en un supermercado, estudia comunicación audiovisual en Arecibo y por la noche hace canciones en su cuarto. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Benito Antonio Martínez Ocasio nació el 10 de marzo de 1994 en Bayamón y creció en el barrio Almirante Sur de Vega Baja. Su padre era camionero; su madre, maestra de escuela. Cantó en el coro de su iglesia hasta los 13 años. Se graduó de la escuela superior en 2012 y estudió comunicación audiovisual en la Universidad de Puerto Rico en Arecibo, con la idea de ser locutor, sin terminar la carrera.",
                "Trabajaba como empacador y cajero en un supermercado Econo de Vega Baja. En 2016 la canción «Diles», que había subido a SoundCloud, llamó la atención del productor DJ Luian, que lo fichó para Hear This Music (esta historia nombra sus canciones sin reproducir sus letras). Su primer álbum, X 100pre, salió el 24 de diciembre de 2018.",
                "Nunca se fue del todo: en 2018 creó la Good Bunny Foundation, que cada Navidad reparte juguetes, instrumentos y material deportivo a jóvenes de la isla; el 22 de julio de 2019 se unió a las protestas que cerraron el Expreso Las Américas y pidieron la renuncia del gobernador; en 2022 el video de «El Apagón» denunció los cortes de luz y la gentrificación. En 2025, Debí Tirar Más Fotos y una residencia de 30 conciertos en San Juan celebraron Puerto Rico; en 2026 hizo el descanso de la Super Bowl y aquel disco fue el primero enteramente en español en ganar el Grammy al álbum del año.",
                "Las frases son de juego y el diálogo con DJ Luian resume lo que él ha contado; doña Carmen es inventada y no se sabe en qué cuarto ni con qué equipo grabó exactamente. Algunas de sus letras y gestos públicos han generado debate; esta historia se limita a 2016. El momento de esta historia: un empacador que no deja el trabajo ni la isla, y que dos años después reparte juguetes e instrumentos cada Navidad; la promesa es de juego, pero lo que hizo es real.",
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

    // ══ JOHN LENNON · el Bed-In de Montreal, 1 de junio de 1969 · Música ════════
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
                Hold("Graba con todos: quédate cantando en la habitación", rec, 12f, "John Lennon", "Todos a la vez, aunque desafinen. Un estribillo sencillo que pueda cantar cualquiera. Aquel periodista preguntó para qué era esto, y la respuesta se ha convertido en canción. Mira: hasta la que vino a reírse de nosotros tiene el papel con la letra en la mano."),
                Talk("Vuelve con John", john.transform, "John Lennon", "Si pudiera vender la paz como se vende un jabón, lo haría. Hoy hemos hecho el anuncio."),
            },
            "El Queen Elizabeth Hotel de Montreal (Canadá), del 26 de mayo al 1 de junio de 1969, sobre el piso de Música del santuario a escala de hormiga. John Lennon y Yoko Ono convierten su cama en una protesta contra la guerra de Vietnam, y el último día graban en la habitación una canción con todos los que están allí. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "John Lennon nació en Liverpool en 1940 y fundó allí los Beatles; Yoko Ono, artista conceptual, nació en Tokio en 1933. Se casaron en marzo de 1969 y usaron la expectación de su boda para protestar contra la guerra de Vietnam: del 25 al 31 de marzo recibieron a la prensa en la cama del Hilton de Ámsterdam, cada día de nueve a nueve. En abril enviaron bellotas a jefes de Estado para que plantaran árboles por la paz.",
                "Querían repetirlo en Nueva York, pero a Lennon no le dejaron entrar en Estados Unidos por una condena por cannabis de 1968. Tras una noche en las Bahamas fueron a Canadá y eligieron Montreal, cerca de la prensa de Nueva York. Del 26 de mayo al 1 de junio ocuparon las habitaciones 1738–1744 del Queen Elizabeth Hotel; pasaron por allí Timothy Leary, Tommy Smothers, Dick Gregory, Allen Ginsberg, Petula Clark, el rabino Abraham Feinberg y Derek Taylor. El dibujante Al Capp discutió con ellos con dureza y no cantó.",
                "El 1 de junio, André Perry, dueño de un estudio de la ciudad, grabó en la habitación 1742, con cuatro micrófonos y un magnetófono de cuatro pistas, Give Peace a Chance (esta historia la nombra sin reproducir sus letras: el título salió de una respuesta de Lennon a un periodista que le preguntó para qué era aquello). Llegó al n.º 2 en el Reino Unido y al 14 en Estados Unidos. El 15 de noviembre de 1969, en la Moratoria contra la guerra de Vietnam, alrededor de medio millón de personas la cantaron en Washington, guiadas por Pete Seeger. Lennon dijo después que debió acreditarla a Yoko Ono y no a McCartney, que no intervino.",
                "La prensa de entonces recibió el acto con mucha burla: esa crítica es parte de la historia. Las frases son de juego, salvo la anécdota del título; los detalles de quién estaba en cada momento varían según las fuentes. Lennon reconoció en entrevistas haber sido violento con mujeres en su juventud; esta historia se limita a aquella semana. El momento de esta historia: dos personas gastan la atención del mundo en pedir paz, y abren la puerta también a quien viene a burlarse. Recreación: la periodista que acaba cantando es inventada.",
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

    // ══ MICHAEL JACKSON · la familia Jackson en Gary (Indiana), 1966 · Música ═══════════════════════
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
                Hold("Mira el concurso de talentos desde el patio de butacas", stage, 12f, "Maestra", "Jermaine canta un tema de los Temptations y el pequeño, uno de Robert Parker, bailando por el escenario. Mira a Tito: se ha agachado a bajarle el micrófono y se ha quedado atrás, para que al pequeño se le vea. El salón entero se ha puesto de pie."),
                Talk("Habla con Michael", mj.transform, "Michael", "Hemos ganado. Yo solo quería bailar como en la tele. Mañana hay colegio, pero mamá dice que hoy podemos cenar tarde. Tito dice que el premio es de todos, y yo también."),
            },
            "Gary (Indiana, Estados Unidos), en 1966, sobre el piso de Música del santuario a escala de hormiga. En una casa de dos dormitorios junto a la acería, una familia obrera forma un grupo con sus hijos, y gana su primer concurso de talentos en el instituto Theodore Roosevelt. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Gary nació en 1906 alrededor de la gran acería de la US Steel, a orillas del lago Míchigan. Joseph Jackson, antiguo boxeador, manejaba allí una grúa y tocaba la guitarra en los Falcons; Katherine Scruse, testigo de Jehová, tocaba el clarinete y el piano y trabajaba a tiempo parcial en Sears. Compraron la casa de 2300 Jackson Street en 1950: dos dormitorios para ellos y sus nueve hijos. Michael, el octavo, nació el 29 de agosto de 1958.",
                "En 1964 Joe vio que Tito tocaba su guitarra y formó un grupo con Jackie, Tito y Jermaine; Michael entró en las congas y Marlon, en agosto de 1965, en la pandereta. Ganaron su primer concurso de talentos en el instituto Theodore Roosevelt en 1966: Jermaine cantó My Girl y Michael, Barefootin' (esta historia nombra sus canciones sin reproducir sus letras). En agosto de 1967 ganaron también en el Regal de Chicago y en el Apollo de Harlem; en noviembre grabaron Big Boy para Steeltown Records, de Gordon Keith, en Gary, y el 26 de julio de 1968 firmaron con Motown.",
                "De adulto, Michael Jackson fue el cantante más vendido de su generación y coescribió con Lionel Richie We Are the World (1985), que recaudó unos 63 millones de dólares contra el hambre en África y Estados Unidos; en 1992 creó la Heal the World Foundation para los niños.",
                "Lo discutido: Michael contó de adulto que su padre fue violento con él en los ensayos; Joe lo reconoció en parte y sus hermanos lo matizaron. Y Michael fue acusado de abusos a menores: un caso acabó en acuerdo extrajudicial en 1994, otro en absolución en 2005, y siguió habiendo acusaciones después de su muerte en 2009. Esta historia no juzga nada de eso: se limita a una familia de Gary en 1966, cuando él tenía 7 años. Las frases son de juego; la fecha exacta del concurso y lo que cada uno dijo no constan. El momento de esta historia: una familia de once en dos dormitorios, tras el turno de la acería, sigue ensayando junta. Recreación: el gesto de Tito con el micrófono es inventado.",
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

    // ══ BLACKPINK · los años de aprendizas en YG (Hapjeong, Seúl), hasta el 8 de agosto de 2016 · Sala de Ballet (representativa) ═════

    static int Blackpink(Transform t)
    {
        Material mirror = M("Espejo_de_ensayo", 0.72f, 0.8f, 0.85f), parquet = M("Suelo_de_ensayo", 0.72f, 0.58f, 0.42f), board = M("Tablón_de_notas", 0.9f, 0.88f, 0.8f);
        Material book = M("Cuaderno_de_coreano", 0.55f, 0.7f, 0.85f), wood = M("Mesa_clara", 0.75f, 0.62f, 0.45f), steel = M("Cromo_del_pie", 0.75f, 0.76f, 0.78f), speaker = M("Altavoz_negro", 0.12f, 0.12f, 0.14f);

        // 1 · LA SALA DE ENSAYO: espejo, parqué y altavoz.
        Transform hall = Spot(t, "Sala de ensayo", new Vector3(0f, 0f, 14f), "La sala de ensayo: catorce horas al día", 3.5f);
        Prop(hall, "Parqué", new Vector3(0f, 0.02f, 0f), new Vector3(12f, 0.04f, 7f), parquet, false);
        Prop(hall, "Espejo", new Vector3(0f, 1.3f, 3.6f), new Vector3(12f, 2.6f, 0.06f), mirror, false);
        Prop(hall, "Altavoz", new Vector3(5.4f, 0.5f, -3f), new Vector3(0.7f, 1f, 0.6f), speaker);

        // 2 · LA EVALUACIÓN DEL MES: la mesa de los profesores y el tablón.
        Transform eval = Spot(t, "Mesa de la evaluación", new Vector3(-16f, 0f, 8f), "La evaluación del mes", 3f);
        Prop(eval, "Mesa de los profesores", new Vector3(0f, 0.4f, 0f), new Vector3(2.4f, 0.8f, 0.9f), wood);
        Prop(eval, "Tablón con las notas del mes", new Vector3(0f, 1.3f, 1f), new Vector3(2f, 1.2f, 0.05f), board, false);

        // 3 · EL AULA DE COREANO: Lisa y Rosé llegaron sin hablarlo bien.
        Transform lesson = Spot(t, "Aula de coreano", new Vector3(15f, 0f, 6f), "Aula de coreano, japonés e inglés", 3f);
        Prop(lesson, "Mesa de estudio", new Vector3(0f, 0.38f, 0f), new Vector3(2f, 0.76f, 1f), wood);
        var books = Items(t, book, new Vector3(0.4f, 0.1f, 0.3f), ("Cuaderno de coreano de Lisa", new Vector3(8f, 0.1f, -6f)), ("Diccionario de Rosé", new Vector3(11f, 0.1f, -3f)), ("Cuaderno de canto", new Vector3(14f, 0.1f, -7f)));

        // 4 · LA CABINA: donde se graba por primera vez.
        Transform booth = Spot(t, "Cabina de voz", new Vector3(-10f, 0f, 19f), "La cabina de voz", 2.5f);
        Mic(booth, Vector3.zero, steel);

        // La gente: insectos-humanos. Las cuatro, MARIQUITAS ASIÁTICAS (Harmonia axyridis, nativa de Corea: pasan el invierno juntas, apiñadas, durante
        // meses, y salen a la vez en primavera). Ropa de ensayo de diario: ningún vestuario ni logotipo del grupo.
        Color black = new Color(0.14f, 0.14f, 0.16f), rose = new Color(0.9f, 0.62f, 0.68f), grey = new Color(0.6f, 0.6f, 0.62f), white = new Color(0.93f, 0.93f, 0.92f);
        GameObject jennie = Who(t, "Jennie", new Vector3(-3f, 0f, 11f), true, 1.63f, "Ladybug"); CubeInsect.WearShirt(jennie, black);
        GameObject lisa = Who(t, "Lisa", new Vector3(3f, 0f, 11f), true, 1.66f, "Ladybug"); CubeInsect.WearShirt(lisa, white);
        GameObject rose1 = Who(t, "Rosé", new Vector3(12f, 0f, 3f), true, 1.68f, "Ladybug"); CubeInsect.WearShirt(rose1, rose);
        GameObject jisoo = Who(t, "Jisoo", new Vector3(-7f, 0f, 17f), true, 1.62f, "Ladybug"); CubeInsect.WearShirt(jisoo, grey);
        GameObject teacher = Who(t, "Profesora de la evaluación (Medea)", new Vector3(-14f, 0f, 5f), true); CubeInsect.WearShirt(teacher, black);
        GameObject korean = Who(t, "Profesora de coreano (Ruth)", new Vector3(17f, 0f, 3f), true); CubeInsect.WearShirt(korean, grey); Routine(korean, lesson, eval);
        int n = 6;

        Mission(t, "BLACKPINK en Seúl: cuatro aprendizas de tres países, años antes del debut (2016)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Jennie en la sala de ensayo", jennie.transform, "Jennie", "Llegué en 2010, de vuelta de Nueva Zelanda. Han pasado seis años. Nos dijeron que debutaríamos en 2012, éramos muchas, y fueron saliendo. Quedamos estas."),
                Reach("Ve a la mesa de la evaluación del mes", eval, "Profesora de la evaluación", "Baile, canto y lengua, cada mes, delante de todas. Quien no avanza se va. No es un concurso de televisión: es la rutina de esta casa."),
                Collect("Lleva los cuadernos al aula de coreano", books, lesson, "Profesora de coreano", "Lisa llegó de Tailandia y Rosé de Australia. Primero aprenden a pedir la comida; luego, a cantar sin acento."),
                Talk("Habla con Lisa", lisa.transform, "Lisa", "En Bangkok se presentaron unas cuatro mil y me cogieron solo a mí. Tenía catorce años cuando vine. Fui la primera extranjera de la agencia, y por las noches llamaba a casa."),
                Talk("Habla con Rosé", rose1.transform, "Rosé", "Mi padre me animó a la audición de Sídney. Éramos setecientas. Un día libre cada dos semanas, y otros trece de ensayo. Se aguanta porque estamos juntas."),
                Hold("Quédate ante el espejo durante el ensayo", hall, 12f, "Jennie", "Otra vez desde el principio. Hasta que las cuatro pisemos en el mismo instante."),
                Talk("Habla con Jisoo", jisoo.transform, "Jisoo", "Yo soy de aquí, entré en 2011. Las demás están lejos de casa. La familia, aquí, es la que ensaya contigo."),
                Talk("8 de agosto de 2016: vuelve con Jennie", jennie.transform, "Jennie", "Hoy sale el disco. Eran nueve y somos cuatro; seis años por tres minutos. Ahora toca ver si ha valido."),
            },
            "El barrio de Hapjeong (Mapo, Seúl), sobre la Sala de Ballet del santuario a escala de hormiga, en los años de aprendizas de la agencia YG hasta el 8 de agosto de 2016, el día del debut. Cuatro chicas de Corea, Tailandia y Australia ensayan catorce horas al día. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "En 2010 la agencia YG Entertainment hizo audiciones por todo el mundo. Jennie volvió ese año a Corea desde Nueva Zelanda y entró como aprendiz; Lisa fue la única elegida entre unas 4000 aspirantes en Tailandia y en 2011 se convirtió en la primera aprendiz extranjera de YG; Jisoo entró en julio de 2011; Rosé, criada en Melbourne, quedó primera entre 700 en la audición de Australia de 2012.",
                "YG anunció en noviembre de 2011 un grupo de al menos siete chicas para 2012; el debut se retrasó años y otras aprendizas se quedaron por el camino. En el documental Light Up the Sky (Netflix, 2020) cuentan las catorce horas diarias, un día libre cada dos semanas y las evaluaciones mensuales. Las cuatro se presentaron en junio de 2016 y debutaron el 8 de agosto con Square One, con Boombayah y Whistle (esta historia nombra sus canciones sin reproducir sus letras).",
                "En 2019 fueron el primer grupo femenino de K-pop en Coachella. En 2021 fueron portavoces de la cumbre del clima COP26 y en 2023 el rey Carlos III les dio la Orden del Imperio Británico honorífica por esa labor.",
                "El sistema de aprendices de la industria coreana ha sido muy criticado por la presión a menores de edad, y el documental apenas entra en ello: lo que se cuenta aquí sale sobre todo de la agencia y de ellas mismas. Las frases son de juego; las notas y los nombres de las profesoras son inventados. La comparación con otras bandas está en el doc del proyecto. El momento de esta historia: años de ensayo sin saber si se debutará, y una niña de 14 años que cruza el mar y aprende otra lengua para seguir; es real, según ellas mismas.",
            },
            new[]
            {
                "Wikipedia, «Blackpink» (audiciones, aprendices, anuncios, debut).",
                "Vogue Singapore (2020), «Blackpink: Light Up the Sky: everything you need to know» (14 horas, evaluaciones mensuales).",
                "The Korea Times (14-10-2020), «BLACKPINK documentary offers honest look at K-pop's biggest girl group».",
                "Jezebel (2020), «In Netflix's Blackpink: Light Up the Sky, the World's Biggest Girl Group Reintroduces Itself» (la crítica al sistema).",
                "Soompi (2021), «BLACKPINK Appointed Official Advocates For UN Climate Change Conference»; AFP (2023) en The Borneo Post, «K-pop stars Blackpink get UK honours for climate advocacy».",
            },
            "Jennie", "Nadie ve los años: se ve el minuto en el que todo sale bien.");
        return n;
    }

    // ══ BOB MARLEY Y LOS WAILERS · el One Love Peace Concert, Kingston, 22 de abril de 1978 · Música ════
    //    La otra banda de la comparación con BLACKPINK (docs/microcosmos-musica.md): de los patios de Trench Town a un estadio que pide la paz en plena
    //    guerra entre barrios. Sin armas en escena: los partidos y la violencia se cuentan de palabra.

    static int Wailers(Transform t)
    {
        Material zinc = M("Zinc_del_patio", 0.6f, 0.62f, 0.62f), wood = M("Tablas_del_escenario", 0.55f, 0.42f, 0.3f), mango = M("Hoja_de_mango", 0.2f, 0.42f, 0.22f), bark = M("Corteza_de_mango", 0.4f, 0.3f, 0.22f);
        Material steel = M("Cromo_del_pie", 0.75f, 0.76f, 0.78f), box = M("Amplificador_negro", 0.13f, 0.13f, 0.15f), paper = M("Sobres_de_la_taquilla", 0.88f, 0.82f, 0.66f), green = M("Telón_verde", 0.15f, 0.42f, 0.22f), gold = M("Telón_dorado", 0.9f, 0.72f, 0.2f), red = M("Telón_rojo_jamaica", 0.7f, 0.15f, 0.12f);

        // 1 · EL ESCENARIO del Estadio Nacional, con los tres colores del movimiento rastafari.
        Transform stage = Stage(t, "Escenario del Estadio Nacional", new Vector3(0f, 0f, 16f), "Estadio Nacional de Kingston, 22 de abril de 1978", 10f, 5f, wood);
        Prop(stage, "Telón rojo", new Vector3(-3.4f, 2f, 2.6f), new Vector3(3.2f, 4f, 0.08f), red, false);
        Prop(stage, "Telón dorado", new Vector3(0f, 2f, 2.6f), new Vector3(3.2f, 4f, 0.08f), gold, false);
        Prop(stage, "Telón verde", new Vector3(3.4f, 2f, 2.6f), new Vector3(3.2f, 4f, 0.08f), green, false);
        Mic(stage, new Vector3(0f, 0.3f, 0f), steel); Amp(stage, new Vector3(-3f, 0.3f, 1f), box); Amp(stage, new Vector3(3f, 0.3f, 1f), box);

        // 2 · EL PATIO DE TRENCH TOWN y el mango de Second Street.
        Transform yard = Spot(t, "Patio de First Street", new Vector3(-17f, 0f, 6f), "Government Yard de Trench Town: donde empezó todo", 3.5f);
        Prop(yard, "Pila común", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 0.8f), zinc);
        Prop(yard, "Tronco del mango", new Vector3(3f, 1.2f, 2f), new Vector3(0.6f, 2.4f, 0.6f), bark);
        Prop(yard, "Copa del mango", new Vector3(3f, 3f, 2f), new Vector3(3.2f, 1.6f, 3.2f), mango, false);

        // 3 · LA TAQUILLA: lo recaudado es para saneamiento y viviendas del oeste de Kingston.
        Transform office = Spot(t, "Mesa del Comité de la Paz", new Vector3(16f, 0f, 5f), "Comité de la Paz: saneamiento y viviendas para el oeste de Kingston", 3f);
        Prop(office, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(2f, 0.8f, 0.9f), wood);
        var money = Items(t, paper, new Vector3(0.45f, 0.2f, 0.3f), ("Sobres de la taquilla norte", new Vector3(8f, 0.1f, -6f)), ("Sobres de la taquilla sur", new Vector3(11f, 0.1f, -3f)), ("Sobres de la taquilla oeste", new Vector3(14f, 0.1f, -7f)));

        // 4 · LOS TRES PUESTOS del cartel, de los 16 grupos de la noche.
        Transform w1 = Spot(t, "Puesto de Peter Tosh", new Vector3(-9f, 0f, 19f), "Peter Tosh: su actuación, un discurso", 2.5f);
        Amp(w1, Vector3.zero, box);
        Transform w2 = Spot(t, "Puesto de Jacob Miller", new Vector3(10f, 0f, 20f), "Jacob Miller e Inner Circle", 2.5f);
        Amp(w2, Vector3.zero, box);
        Transform w3 = Spot(t, "Puesto de Dennis Brown", new Vector3(17f, 0f, 16f), "Dennis Brown", 2.5f);
        Amp(w3, Vector3.zero, box);
        var acts = new List<Transform> { w1, w2, w3 };

        // La gente: insectos-humanos. Marley, un GRILLO (en Jamaica el cri-cri de la noche es la banda sonora de los patios). Sin símbolos de partido y sin armas.
        Color denim = new Color(0.25f, 0.32f, 0.48f), khaki = new Color(0.55f, 0.5f, 0.38f), suit = new Color(0.3f, 0.3f, 0.34f), orange = new Color(0.85f, 0.5f, 0.2f), greenS = new Color(0.25f, 0.5f, 0.3f);
        GameObject bob = Who(t, "Bob Marley", new Vector3(2f, 0f, 11f), false, 1.7f, "Cricket"); CubeInsect.WearShirt(bob, denim);
        GameObject massop = Who(t, "Claudie Massop", new Vector3(-6f, 0f, 3f), false, 1.8f, "Ant"); CubeInsect.WearShirt(massop, greenS);
        GameObject marshall = Who(t, "Bucky Marshall", new Vector3(-3f, 0f, 3f), false, 1.78f, "Ant"); CubeInsect.WearShirt(marshall, orange);
        GameObject higgs = Who(t, "Joe Higgs", new Vector3(-15f, 0f, 3f), false, 1.72f, "Cricket"); CubeInsect.WearShirt(higgs, khaki);
        GameObject tosh = Who(t, "Peter Tosh", new Vector3(-7f, 0f, 17f), false, 1.85f, "Mantis"); CubeInsect.WearShirt(tosh, khaki); Routine(tosh, w1, stage);
        GameObject seller = Who(t, "Vendedora de Coronation Market (Ruth)", new Vector3(13f, 0f, 2f), true); CubeInsect.WearShirt(seller, suit);
        int n = 6;

        Mission(t, "Bob Marley y los Wailers: de Trench Town al One Love Peace Concert (1978)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Claudie Massop y Bucky Marshall", massop.transform, "Claudie Massop", "Él es de un bando y yo del otro. Nos metieron en la misma celda y allí hablamos: nuestros barrios se están matando por los partidos. Solo una persona podía juntarlos. Fui a Londres a buscarle."),
                Talk("Habla con Bob Marley, que ha vuelto tras más de un año en Londres", bob.transform, "Bob Marley", "Me dispararon en esta ciudad, en mi casa, dos días antes de un concierto; y lo di igual. Me fui. Si los dos lados de la calle me llaman juntos, vuelvo."),
                Reach("Ve al patio de First Street, en Trench Town", yard, "Joe Higgs", "Aquí les enseñé armonía a Bob, a Bunny y a Peter, por nada, en el patio. Ensayaban bajo el mango de Second Street. Esos muchachos salieron de un patio con una sola pila para todos."),
                Collect("Lleva los sobres de la taquilla a la mesa del Comité de la Paz", money, office, "Vendedora de Coronation Market", "Lo que se recaude es para letrinas y casas del oeste de Kingston. Aquí, una letrina también es política."),
                Visit("Recorre los puestos: Peter Tosh, Jacob Miller y Dennis Brown", acts, "Peter Tosh", "Dieciséis grupos en una noche. Yo no vine a sonreírles: tengo a los dos jefes sentados delante y les voy a decir lo que el barrio les diría."),
                Hold("Medianoche pasada: mira el escenario", stage, 12f, "Bob Marley", "«Para que todo se haga realidad, tenemos que estar juntos.» En plena canción llamo a los dos, a Michael Manley y a Edward Seaga. Suben. Les cojo las manos y las levanto. Treinta mil personas se quedan sin aire."),
                Talk("Habla con la vendedora", seller.transform, "Vendedora de Coronation Market", "No sé si esto va a durar. Pero esta noche he visto a los del otro barrio bailar a mi lado, y eso no lo había visto nunca."),
                Talk("Vuelve con Bob Marley", bob.transform, "Bob Marley", "Una canción no para una bala. Pero hoy nadie aquí se ha olvidado de que somos vecinos."),
            },
            "Kingston (Jamaica), el 22 de abril de 1978, sobre el piso de Música del santuario a escala de hormiga. Tras años de violencia entre los barrios de los dos partidos, Bob Marley vuelve del exilio y encabeza un concierto por la paz en el Estadio Nacional. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Trench Town nació en los años treinta como barrio modelo sobre la antigua finca de Daniel Trench: patios del gobierno (Government Yards) con cocina y baños comunes. Bob Marley creció en el de First Street. Allí Joe Higgs daba clases informales de canto, en las que coincidieron Bob, Bunny Livingston y Peter Tosh; en 1963 Higgs los llevó a Coxsone Dodd, y Simmer Down llegó al primer puesto en Jamaica. Beverley Kelso recordaba ensayar bajo un mango de Second Street. En 1974 Tosh y Bunny dejaron el grupo (esta historia nombra sus canciones sin reproducir sus letras).",
                "En los setenta, los dos grandes partidos, el PNP de Michael Manley y el JLP de Edward Seaga, se apoyaban en bandas de sus barrios. En diciembre de 1976 Marley fue tiroteado en su casa de Hope Road y, aun así, cantó dos días después en Smile Jamaica; luego se fue a Londres. En prisión, Claudie Massop (JLP) y Aston «Bucky» Marshall (PNP) pensaron en un gran concierto, y Massop fue a Londres a convencerle.",
                "El One Love Peace Concert fue el 22 de abril de 1978 en el Estadio Nacional: 16 grupos, más de 32 000 personas y la recaudación destinada a saneamiento y viviendas del oeste de Kingston. Peter Tosh dedicó 66 minutos a criticar de frente a los dos líderes. Hacia las 00:30, durante Jammin', Marley llamó a Manley y a Seaga al escenario y unió sus manos en alto.",
                "La paz no duró: Massop y Marshall murieron asesinados en menos de dos años y Jamaica tuvo 889 asesinatos en 1980, durante la campaña electoral; las fuentes discuten si los dos líderes subieron de buena gana. Marley murió en 1981. Las frases son de juego; Massop, Marshall y Higgs hablan aquí en resumen de lo que cuentan las crónicas. La historia no elige bando. El momento de esta historia: un hombre al que dispararon en su propia ciudad vuelve a ella para juntar las manos de los dos lados. Recreación: la vendedora es inventada, y lo que dice Massop resume las crónicas.",
            },
            new[]
            {
                "Wikipedia, «One Love Peace Concert» y «Trenchtown».",
                "Biography.com, «Bob Marley's One Love Peace Concert: historic impact».",
                "The Jamaica Gleaner (22-4-2009), «When peace came: three decades later, a country awaits its return».",
                "History.com, «Bob Marley's One Love Peace Concert» (22 de abril).",
                "Wikipedia, «Joe Higgs»; Wikipedia, «Simmer Down» (Kelso y el mango de Second Street).",
            },
            "Bob Marley", "Mientras dure la canción, esta calle no tiene dos lados.");
        return n;
    }

    // ══ U2 · el concierto del Sí, Waterfront Hall, Belfast, 19 de mayo de 1998 · Música ═════════════════
    //    El ROCK que denuncia: de Sunday Bloody Sunday (1983) a poner un escenario al servicio de un referéndum de paz. Por qué U2 y no otros: doc, «Rock y rap».
    //    Sin armas ni escenas del Domingo Sangriento: se recuerda de palabra.

    static int U2(Transform t)
    {
        Material wood = M("Tablas_del_escenario", 0.55f, 0.42f, 0.3f), brick = M("Ladrillo_de_Belfast", 0.58f, 0.3f, 0.24f), steel = M("Cromo_del_pie", 0.75f, 0.76f, 0.78f), box = M("Amplificador_negro", 0.13f, 0.13f, 0.15f);
        Material ticket = M("Entradas_gratis", 0.92f, 0.9f, 0.82f), yes = M("Cartel_del_Sí", 0.95f, 0.8f, 0.2f), white = M("Bandera_blanca", 0.96f, 0.96f, 0.95f), pole = M("Asta", 0.5f, 0.38f, 0.26f), blue = M("Butaca", 0.25f, 0.3f, 0.5f);

        // 1 · LA OFICINA DEL SÍ, el 12 de mayo: las encuestas no dan.
        Transform office = Spot(t, "Oficina de la campaña del Sí", new Vector3(-16f, 0f, 6f), "Campaña del Sí: el referéndum es el 22 de mayo", 3f);
        Prop(office, "Mesa de la campaña", new Vector3(0f, 0.4f, 0f), new Vector3(2.2f, 0.8f, 0.9f), wood);
        Prop(office, "Cartel del Sí", new Vector3(0f, 1.4f, 1f), new Vector3(1.8f, 1f, 0.05f), yes, false);

        // 2 · LAS ENTRADAS GRATIS, a los dos lados de la ciudad.
        var tickets = Items(t, ticket, new Vector3(0.4f, 0.15f, 0.3f), ("Entradas para Falls Road", new Vector3(8f, 0.1f, -6f)), ("Entradas para Shankill Road", new Vector3(11f, 0.1f, -3f)), ("Entradas para la universidad", new Vector3(14f, 0.1f, -7f)));
        Transform door = Spot(t, "Puerta del Waterfront", new Vector3(8f, 0f, 4f), "La puerta: católicos y protestantes, en la misma cola", 2.5f);
        Prop(door, "Mesa de la entrada", new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.7f), wood);

        // 3 · LA MEMORIA: Maysfield, 1982; la bandera blanca del escenario.
        Transform maysfield = Spot(t, "Maysfield Leisure Centre", new Vector3(17f, 0f, 12f), "Maysfield (1982): aquí la cantaron por primera vez en Belfast", 3f);
        Prop(maysfield, "Pared de ladrillo baja", new Vector3(0f, 0.5f, 2f), new Vector3(5f, 1f, 0.3f), brick);
        Prop(maysfield, "Asta", new Vector3(0f, 1.5f, 0f), new Vector3(0.1f, 3f, 0.1f), pole);
        Prop(maysfield, "Bandera blanca", new Vector3(0.7f, 2.5f, 0f), new Vector3(1.3f, 0.9f, 0.03f), white, false);

        // 4 · EL ESCENARIO del Waterfront Hall.
        Transform stage = Stage(t, "Escenario del Waterfront Hall", new Vector3(0f, 0f, 17f), "Waterfront Hall, Belfast, 19 de mayo de 1998", 10f, 5f, wood);
        Mic(stage, new Vector3(0f, 0.3f, 0f), steel); Amp(stage, new Vector3(-3f, 0.3f, 1f), box); Amp(stage, new Vector3(3f, 0.3f, 1f), box);
        for (int k = 0; k < 5; k++) Prop(t, "Fila de butacas", new Vector3(0f, 0.25f, 10f - k * 1.5f), new Vector3(10f, 0.5f, 0.6f), blue);

        // La gente: insectos-humanos. Bono, una CIGARRA (el insecto que más fuerte canta); Hume y Trimble, dos HORMIGAS de la MISMA especie: dos comunidades
        // que comparten suelo. Ropa de calle; sin banderas de ningún bando.
        Color black = new Color(0.1f, 0.1f, 0.12f), suit = new Color(0.28f, 0.3f, 0.36f), tie = new Color(0.45f, 0.4f, 0.4f), green = new Color(0.4f, 0.5f, 0.35f), brickS = new Color(0.55f, 0.35f, 0.3f);
        GameObject bono = Who(t, "Bono", new Vector3(2f, 0f, 14f), false, 1.68f, "Cicada"); CubeInsect.WearShirt(bono, black);
        GameObject hume = Who(t, "John Hume", new Vector3(-7f, 0f, 13f), false, 1.75f, "Ant"); CubeInsect.WearShirt(hume, suit);
        GameObject trimble = Who(t, "David Trimble", new Vector3(8f, 0f, 13f), false, 1.72f, "Ant"); CubeInsect.WearShirt(trimble, tie);
        GameObject attwood = Who(t, "Tim Attwood", new Vector3(-13f, 0f, 3f), false, 1.76f, "Grasshopper"); CubeInsect.WearShirt(attwood, green);
        GameObject student = Who(t, "Estudiante de Queen's (Medea)", new Vector3(10f, 0f, 1f), true); CubeInsect.WearShirt(student, brickS); Routine(student, door, maysfield);
        GameObject neighbor = Who(t, "Vecina de Falls Road (Ruth)", new Vector3(6f, 0f, 7f), true); CubeInsect.WearShirt(neighbor, green); Routine(neighbor, door);
        int n = 6;

        Mission(t, "U2 y el concierto del Sí: una canción contra la violencia, un escenario para la paz (Belfast, 1998)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Tim Attwood en la oficina de la campaña del Sí", attwood.transform, "Tim Attwood", "Faltan diez días. El Acuerdo se firmó en Viernes Santo, pero los protestantes dudan y Trimble tiene problemas. Llamé a Bono y le dije cómo estaba la cosa. Dijo que haría lo que Hume quisiera."),
                Collect("Reparte las entradas gratis: llévalas a la puerta del Waterfront", tickets, door, "Estudiante de Queen's", "Gratis y a medias: católicos y protestantes. Dos mil personas que quizá nunca se han sentado juntas. Y de esta edad, la mayoría no ha conocido otra cosa que las bombas."),
                Reach("Ve al Maysfield Leisure Centre, a dos pasos", maysfield, "Vecina de Falls Road", "Aquí mismo, en diciembre del 82, Bono les dijo que esa canción no era para ningún bando, que era contra la matanza venga de quien venga. Yo estaba, y tenía miedo de cómo iba a acabar."),
                Hold("Quédate mirando la bandera blanca", maysfield, 8f, "Vecina de Falls Road", "Al año siguiente llevaba una bandera blanca por el escenario, cuando cantaba aquello del Domingo Sangriento. Trece muertos en Derry en un día. Enterramos a mucha gente después."),
                Talk("Habla con David Trimble, entre bastidores", trimble.transform, "David Trimble", "Yo no hago esto. No sé qué hace un líder unionista en un concierto de rock, ni me voy a quitar la chaqueta. Pero sé lo que me juego el viernes, y lo que se juega mi familia. Subo igual."),
                Talk("Habla con John Hume", hume.transform, "John Hume", "David, tranquilo. Llevo treinta años esperando esto. Se quita la chaqueta y se sale. No hace falta que cantes."),
                Hold("Mira el concierto desde las butacas", stage, 12f, "Bono", "Os presento a dos hombres que han dado un salto de fe. Y les levanto los brazos: uno a cada lado, los tres sin decir nada. Esa foto da la vuelta al mundo."),
                Talk("Vuelve con Bono", bono.transform, "Bono", "No he venido a deciros qué votar. Solo que esta noche estáis sentados al lado del otro. El viernes, vosotros."),
            },
            "Belfast (Irlanda del Norte), en mayo de 1998, sobre el piso de Música del santuario a escala de hormiga. A tres días del referéndum del Acuerdo de Viernes Santo, U2 y Ash tocan gratis en el Waterfront Hall, y Bono sube al escenario a John Hume y David Trimble. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "El 30 de enero de 1972, en Derry, soldados británicos dispararon contra una marcha por los derechos civiles: murieron 13 personas ese día y otra después, todas desarmadas, como concluyó la investigación Saville en 2010. U2, un grupo de Dublín, grabó entre septiembre y noviembre de 1982 Sunday Bloody Sunday, sobre el horror de esa violencia (esta historia la nombra sin reproducir sus letras): The Edge escribió el riff y el primer texto, y Bono lo rehízo. Publicada en War (1983), en concierto Bono avisaba de que no era una canción de rebeldes, y en la gira de 1983 llevaba una bandera blanca. El 8 de noviembre de 1987, el día de la bomba de Enniskillen, condenó el atentado en mitad de la canción, en Denver.",
                "El Acuerdo de Viernes Santo se firmó el 10 de abril de 1998 y debía votarse en referéndum el 22 de mayo. Los partidarios temían que no llegara la mayoría de los protestantes. Tim Attwood (SDLP) pidió ayuda a Bono, David Kerr (colaborador de Trimble) se sumó, y en pocos días se organizó un concierto gratuito en el Waterfront Hall, el 19 de mayo, con U2 y Ash y unas 2000–2500 personas de las dos comunidades.",
                "Bono subió al escenario a John Hume (SDLP, nacionalista) y David Trimble (UUP, unionista) y levantó sus brazos. A Trimble, muy nervioso, costó convencerle de quitarse la chaqueta. El Sí ganó con el 71 %; Hume y Trimble recibieron el Nobel de la Paz ese mismo año.",
                "Su efecto se discute: un colaborador de Trimble lo llama el momento decisivo de la campaña; el director de la campaña del Sí cree que ayudó al ambiente sin cambiar votos. Bono también ha recibido críticas por mezclar música y política. Las frases son de juego, salvo la anécdota de la chaqueta y el recado de Attwood, en resumen; la vecina y la estudiante son inventadas. El momento de esta historia: dos rivales de toda la vida suben juntos a un escenario sabiendo lo que arriesgan (la familia de Trimble recibió una amenaza de muerte el día del resultado). Recreación: las palabras de Hume y Trimble entre bastidores.",
            },
            new[]
            {
                "The Irish Times (10-9-2025), «Belfast Agreement referendum vote saved by U2's concert, new book says» (S. Walker, David Trimble: Peacemaker).",
                "The Irish Times (7-4-2023), «I found myself in between two great men: Bono finds pride in the name of peace».",
                "The Irish News (17-5-2018), «Opinion split over influence of historic Waterfront gig».",
                "Hot Press, «On this day in 1998: U2, Ash, John Hume and David Trimble took part in the historic Good Friday Agreement gig in Belfast».",
                "Wikipedia, «Sunday Bloody Sunday»; UK Government (2010), Report of the Bloody Sunday Inquiry (Saville).",
            },
            "Bono", "Una canción puede decir «basta». Lo demás lo votáis vosotros.");
        return n;
    }

    // ══ PUBLIC ENEMY · Roosevelt y Hempstead (Long Island), 1989 · Música ═══════════════════════
    //    El RAP que denuncia: un estudiante de diseño, una radio universitaria y un collage de sonido que quiere ser el noticiero de su barrio. Por qué ellos: doc, «Rock y rap».

    static int PublicEnemy(Transform t)
    {
        Material wood = M("Mesa_de_radio", 0.55f, 0.42f, 0.3f), vinyl = M("Disco_de_vinilo", 0.08f, 0.08f, 0.1f), crate = M("Caja_de_discos", 0.6f, 0.48f, 0.32f), steel = M("Cromo_del_pie", 0.75f, 0.76f, 0.78f);
        Material tape = M("Magnetófono", 0.45f, 0.47f, 0.5f), paper = M("Papel_de_dibujo", 0.95f, 0.94f, 0.9f), asphalt = M("Cancha_asfaltada", 0.32f, 0.32f, 0.34f), hoop = M("Aro_naranja", 0.85f, 0.45f, 0.15f), news = M("Periódico", 0.8f, 0.8f, 0.76f);

        // 1 · LA MESA DE DIBUJO DE CHUCK: estudió diseño gráfico en Adelphi.
        Transform desk = Spot(t, "Mesa de dibujo", new Vector3(-10f, 0f, 14f), "La mesa de dibujo de Chuck (diseño gráfico, Adelphi)", 3f);
        Prop(desk, "Tablero inclinado", new Vector3(0f, 0.8f, 0f), new Vector3(1.6f, 0.08f, 1.1f), wood);
        Prop(desk, "Hoja de dibujo", new Vector3(0f, 0.86f, 0f), new Vector3(1.2f, 0.02f, 0.8f), paper, false);

        // 2 · WBAU: la cabina de la radio de la universidad.
        Transform radio = Spot(t, "Cabina de WBAU", new Vector3(0f, 0f, 18f), "WBAU, la radio de la Universidad Adelphi", 3f);
        Prop(radio, "Mesa de locución", new Vector3(0f, 0.4f, 0f), new Vector3(2f, 0.8f, 0.9f), wood);
        Mic(radio, new Vector3(0.4f, 0.8f, 0f), steel);

        // 3 · LOS DISCOS: un sonido hecho de cientos de trozos de otros sonidos.
        var crates = Items(t, crate, new Vector3(0.6f, 0.4f, 0.5f), ("Caja de discos de soul", new Vector3(9f, 0.2f, -6f)), ("Caja de discursos y noticias", new Vector3(12f, 0.2f, -3f)), ("Caja de ruidos de la calle", new Vector3(15f, 0.2f, -7f)));
        Transform studio = Spot(t, "Estudio del Bomb Squad", new Vector3(14f, 0f, 8f), "510 South Franklin: el estudio del Bomb Squad", 3f);
        Prop(studio, "Magnetófono de pistas", new Vector3(0f, 0.5f, 0f), new Vector3(1.6f, 1f, 0.8f), tape);
        for (int k = 0; k < 3; k++) Prop(studio, "Disco", new Vector3(-1.4f + k * 1.4f, 0.03f, 1.6f), new Vector3(0.3f, 0.02f, 0.3f), vinyl, false);

        // 4 · EL NOTICIERO DEL BARRIO: tres sitios donde se escuchan las noticias de verdad.
        Transform court = Spot(t, "Cancha de Roosevelt", new Vector3(-18f, 0f, 2f), "La cancha: lo que se habla en el barrio", 3f);
        Prop(court, "Cancha", new Vector3(0f, 0.02f, 0f), new Vector3(7f, 0.04f, 5f), asphalt, false);
        Prop(court, "Aro", new Vector3(0f, 2.3f, 2.4f), new Vector3(0.7f, 0.08f, 0.7f), hoop, false);
        Transform stop = Spot(t, "Parada de Nassau Road", new Vector3(-4f, 0f, 2f), "Nassau Road: las vecinas de la parada", 2.5f);
        Prop(stop, "Poste de la parada", new Vector3(0f, 1.2f, 0f), new Vector3(0.12f, 2.4f, 0.12f), steel);
        Transform library = Spot(t, "Biblioteca de Roosevelt", new Vector3(-21f, 0f, 12f), "La biblioteca: los periódicos", 2.5f);
        Prop(library, "Mesa de periódicos", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 0.9f), wood);
        Prop(library, "Periódicos", new Vector3(0f, 0.84f, 0f), new Vector3(1f, 0.05f, 0.6f), news, false);
        var hood = new List<Transform> { court, stop, library };

        // La gente: insectos-humanos. Chuck D, un ESCARABAJO PELOTERO (hace rodar y convierte lo que otros tiran: el sampler es eso). Ropa de calle, sin el logotipo del grupo.
        Color black = new Color(0.12f, 0.12f, 0.14f), red = new Color(0.6f, 0.15f, 0.12f), grey = new Color(0.55f, 0.55f, 0.56f), denim = new Color(0.25f, 0.32f, 0.48f);
        GameObject chuck = Who(t, "Chuck D", new Vector3(-6f, 0f, 12f), false, 1.83f, "DungBeetle"); CubeInsect.WearShirt(chuck, black);
        GameObject flav = Who(t, "Flavor Flav", new Vector3(5f, 0f, 3f), false, 1.68f, "Grasshopper"); CubeInsect.WearShirt(flav, red);
        GameObject hank = Who(t, "Hank Shocklee", new Vector3(12f, 0f, 5f), false, 1.78f, "Ant"); CubeInsect.WearShirt(hank, grey);
        GameObject bill = Who(t, "Bill Stephney", new Vector3(3f, 0f, 15f), false, 1.76f, "Cricket"); CubeInsect.WearShirt(bill, denim);
        GameObject neighbor = Who(t, "Vecina de Roosevelt (Ruth)", new Vector3(-5f, 0f, -1f), true); CubeInsect.WearShirt(neighbor, denim); Routine(neighbor, stop, library);
        GameObject kid = Who(t, "Chico de la cancha (Momo)", new Vector3(-16f, 0f, -1f), false, 1.65f); CubeInsect.WearShirt(kid, red); Routine(kid, court);
        int n = 6;

        Mission(t, "Public Enemy en Long Island: el noticiero del barrio (1989)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Chuck D en su mesa de dibujo", chuck.transform, "Chuck D", "Estudié diseño gráfico, no música. Pero en los barrios como este lo que pasa no sale en la tele. Si la gente pudiera poner la radio y oírse a sí misma, sería un noticiero."),
                Reach("Ve a la cabina de WBAU", radio, "Bill Stephney", "La radio de la universidad apenas pasaba rap. Un programa los sábados por la noche, y las cintas de la calle empezaron a llegar. Una promo de Chuck se hizo famosa aquí en Long Island."),
                Collect("Lleva las cajas de discos al estudio", crates, studio, "Hank Shocklee", "Cuidado con eso: soul, discursos, noticias, sirenas. Cada segundo de nuestro sonido es un trozo de otra cosa."),
                Talk("Habla con Hank Shocklee", hank.transform, "Hank Shocklee", "A esto lo llaman el Bomb Squad. Se trata de que suene a como se siente vivir aquí: cien cosas a la vez, y ninguna en paz."),
                Visit("Escucha el barrio: la cancha, la parada y la biblioteca", hood, "Vecina de Roosevelt", "El trabajo que se va, la droga en la esquina, la policía, los de siempre sin voz. ¿Quién lo cuenta? Ese chico alto, el de los Ridenhour, viene aquí a escuchar."),
                Talk("Habla con Flavor Flav", flav.transform, "Flavor Flav", "Él es la voz seria y yo el que te despierta. Si no te ríes un poco, no aguantas oír esto entero."),
                Hold("1989: Spike Lee ha pedido un himno para su película. Quédate en la grabación", studio, 12f, "Chuck D", "Lo escribí casi todo en un avión sobre Italia. Se llama como aquella canción de los Isley Brothers. Es decirle a la gente que no se quede sentada."),
                Talk("Verano de 1989: vuelve con Chuck D", chuck.transform, "Chuck D", "Yo no digo qué pensar. Digo que apagues lo que te cuentan y mires tu calle. Y este año he tenido que pedir perdón en público por lo que dijo uno de los nuestros. Denunciar también es mirar en tu propia casa."),
            },
            "Roosevelt y Hempstead (Long Island, Nueva York), en 1989, sobre el piso de Música del santuario a escala de hormiga. Public Enemy, un grupo salido de la radio de una universidad, graba para una película el tema que le hará famoso. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "Carlton Ridenhour (Chuck D) creció en Roosevelt, Long Island, y estudió diseño gráfico en la Universidad Adelphi, donde conoció a Hank Shocklee y a William Drayton (Flavor Flav). Desde 1982–83 hizo programas en WBAU, la emisora universitaria, con Bill Stephney como director de programas; una promo para la radio, Public Enemy #1, dio nombre al grupo. Rick Rubin lo fichó para Def Jam y en 1987 salió su primer disco.",
                "Con el Bomb Squad (Hank y Keith Shocklee, Eric Sadler) inventaron un sonido hecho de capas de samples. It Takes a Nation of Millions to Hold Us Back (1988) fue el primer disco de hip hop elegido el mejor del año por la crítica de The Village Voice. Chuck D describió el rap como un noticiero del Estados Unidos negro. Sus temas: el racismo, la policía, la droga en los barrios, los medios de comunicación.",
                "En 1989, Spike Lee les pidió un himno para Do the Right Thing. Chuck D lo escribió sobre todo en un vuelo sobre Italia y el grupo lo grabó con Branford Marsalis: Fight the Power, que se publicó el 4 de julio de 1989 (esta historia la nombra sin reproducir sus letras). El video, una marcha contra la violencia racial, se rodó el 22 de abril en Bedford-Stuyvesant, Brooklyn, dirigido por Lee. En 2005 Fear of a Black Planet (1990) entró en el Registro Nacional de Grabaciones de Estados Unidos; en 2013 entraron en el Salón de la Fama del Rock.",
                "Lo discutido: en 1989 Professor Griff, miembro del grupo, hizo declaraciones antisemitas; Chuck D se disculpó y lo apartó del grupo, y algunas letras también fueron criticadas, igual que la estética militar de su seguridad. Las frases son de juego, y la vecina y el chico son inventados. No se usa el logotipo del grupo. El momento de esta historia: un estudiante de diseño pone su voz al servicio de su barrio, y cuando el grupo falla se disculpa en público; las dos cosas son reales.",
            },
            new[]
            {
                "Wikipedia, «Public Enemy» y «Fight the Power (Public Enemy song)».",
                "Encyclopedia.com, «Shocklee, Hank» (WBAU, Spectrum).",
                "Okayplayer, «The Secret History of Public Enemy's Yo! Bum Rush the Show».",
                "American Songwriter, «Behind the Band Name: Public Enemy».",
                "Library of Congress, National Recording Registry (2005): Fear of a Black Planet.",
            },
            "Chuck D", "Si tu barrio no sale en las noticias, hazle su noticiero.");
        return n;
    }

    // ══ GRANDES APORTES DE LAS CANCIONES · la Plaza de las Canciones (sitio inventado), 21 de junio · Música ══════════
    //    Seis canciones que cambiaron algo, cada una con su historia y su rincón de época (en el claro, y su edificio en el pueblo, con el año y la región de su rincón):
    //    We Shall Overcome, Bella Ciao, Imagine, Biko, Redemption Song y Latinoamérica. Ninguna letra: título, autor y tema, con palabras propias.

    static int GrandesCanciones(Transform t)
    {
        Material wood = M("Madera_vieja", 0.5f, 0.38f, 0.28f), paper = M("Rótulo_del_rincón", 0.92f, 0.88f, 0.76f), rice = M("Agua_del_arrozal", 0.45f, 0.6f, 0.5f), steel = M("Cromo_del_pie", 0.75f, 0.76f, 0.78f);
        Material porch = M("Porche_de_tablas", 0.6f, 0.5f, 0.38f), white = M("Piano_vertical", 0.2f, 0.18f, 0.18f), earth = M("Tierra_roja", 0.6f, 0.36f, 0.25f), yardC = M("Cemento_del_patio", 0.7f, 0.68f, 0.62f), wool = M("Telar_de_los_Andes", 0.75f, 0.3f, 0.25f), stone = M("Piedra_andina", 0.55f, 0.53f, 0.5f);

        Transform Corner(string name, string label, Vector3 at, Material ground, float w = 6f, float d = 5f)
        {
            Transform c = Spot(t, name, at, label, 3.5f);
            Prop(c, "Suelo del rincón", new Vector3(0f, 0.02f, 0f), new Vector3(w, 0.04f, d), ground, false);
            Prop(c, "Poste del rótulo", new Vector3(-w * 0.5f + 0.3f, 0.7f, -d * 0.5f + 0.3f), new Vector3(0.1f, 1.4f, 0.1f), wood);
            Prop(c, "Placa del rincón", new Vector3(-w * 0.5f + 0.3f, 1.4f, -d * 0.5f + 0.25f), new Vector3(1f, 0.5f, 0.04f), paper, false);
            return c;
        }

        // Los seis rincones, en círculo alrededor de la tarima del centro (la entrada, al sur, queda libre).
        Transform stage = Stage(t, "Tarima de la Plaza de las Canciones", new Vector3(0f, 0f, 6f), "La Plaza de las Canciones", 6f, 4f, wood);
        Mic(stage, new Vector3(0f, 0.3f, 0f), steel);
        Transform high = Corner("Rincón de We Shall Overcome", "We Shall Overcome: Highlander, Tennessee, 1960", new Vector3(-19f, 0f, 4f), porch);
        Prop(high, "Banco del porche", new Vector3(0.5f, 0.25f, 1.5f), new Vector3(3f, 0.5f, 0.5f), wood);
        Transform mond = Corner("Rincón de Bella Ciao", "Bella Ciao: los arrozales de Vercelli, hacia 1906", new Vector3(-14f, 0f, 15f), rice);
        for (int k = 0; k < 8; k++) Prop(mond, "Mata de arroz", new Vector3(-2.4f + k * 0.7f, 0.2f, (k % 2) * 1.2f - 0.6f), new Vector3(0.2f, 0.4f, 0.2f), M("Arroz_verde", 0.45f, 0.65f, 0.3f), false);
        Transform asc = Corner("Rincón de Imagine", "Imagine: Tittenhurst Park, Ascot, 1971", new Vector3(0f, 0f, 20f), wood);
        Prop(asc, "Piano vertical", new Vector3(0f, 0.65f, 1f), new Vector3(1.6f, 1.3f, 0.6f), white);
        Transform gin = Corner("Rincón de Biko", "Biko: Ginsberg, Cabo Oriental, 1977", new Vector3(14f, 0f, 15f), earth);
        Prop(gin, "Mesa de la clínica y de la imprenta", new Vector3(0.5f, 0.4f, 1f), new Vector3(1.8f, 0.8f, 0.9f), wood);
        Transform yard = Corner("Rincón de Redemption Song", "Redemption Song: Kingston, 1980", new Vector3(19f, 0f, 4f), yardC);
        Prop(yard, "Taburete de la guitarra", new Vector3(0f, 0.25f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), wood);
        Transform andes = Corner("Rincón de Latinoamérica", "Latinoamérica: los Andes del Perú, 2011", new Vector3(15f, 0f, -6f), stone);
        Prop(andes, "Telar de cintura", new Vector3(0f, 0.8f, 1f), new Vector3(1.6f, 1.6f, 0.1f), wool, false);

        // La gente: insectos-humanos (cada uno su insecto de InsectCast); el cronista, un GRILLO, que nunca deja de contar. Fuera de los rincones no hay nadie famoso.
        Color bluish = new Color(0.35f, 0.45f, 0.55f), cream = new Color(0.85f, 0.8f, 0.68f), straw = new Color(0.75f, 0.65f, 0.4f), dark = new Color(0.2f, 0.2f, 0.24f), green = new Color(0.25f, 0.5f, 0.3f), red = new Color(0.65f, 0.25f, 0.2f);
        GameObject cron = Who(t, "Cronista de las canciones", new Vector3(2f, 0f, 2f), true, 1.65f, "Cricket"); CubeInsect.WearShirt(cron, bluish);
        GameObject guy = Who(t, "Guy Carawan", new Vector3(-17f, 0f, 2f), false, 1.8f, "Cricket"); CubeInsect.WearShirt(guy, cream);
        GameObject mondina = Who(t, "Mondina de Vercelli (Ruth)", new Vector3(-12f, 0f, 13f), true, 1.58f, "Ant"); CubeInsect.WearShirt(mondina, straw);
        GameObject gabriel = Who(t, "Peter Gabriel", new Vector3(12f, 0f, 13f), false, 1.72f, "Grasshopper"); CubeInsect.WearShirt(gabriel, dark);
        GameObject bob = Who(t, "Bob Marley", new Vector3(17f, 0f, 2f), false, 1.7f, "Cricket"); CubeInsect.WearShirt(bob, green);
        GameObject baca = Who(t, "Susana Baca", new Vector3(13f, 0f, -8f), true, 1.6f, "Cicada"); CubeInsect.WearShirt(baca, red);
        GameObject john = Who(t, "John Lennon (1971)", new Vector3(-2f, 0f, 18f), false, 1.79f, "Cicada"); CubeInsect.WearShirt(john, cream);
        int n = 7;

        Mission(t, "Grandes aportes de las canciones: seis que cambiaron algo", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con la cronista en la Plaza de las Canciones", cron.transform, "Cronista", "Aquí, cada 21 de junio, se cuenta cómo una canción movió el mundo. Sin cantarlas: ellas ya están en la memoria de quien las necesita. Recorre los rincones: cada uno guarda su época."),
                Talk("Rincón de Highlander: habla con Guy Carawan", guy.transform, "Guy Carawan", "Unas trabajadoras del tabaco de Charleston la cantaban en 1945, en piquetes. Zilphia Horton la trajo a esta escuela, Pete Seeger la llevó al norte y yo se la enseñé a los estudiantes en 1960. Cinco años después, el presidente de Estados Unidos dijo su título ante el Congreso."),
                Talk("Rincón de los arrozales: habla con la mondina", mondina.transform, "Mondina", "Dicen que la melodía viene de nosotras, de las que escardábamos el arroz con el agua hasta las rodillas. Otros, que de un canto de los partisanos. Los historiadores aún discuten. A mí me gusta que la cante quien no se rinde."),
                Hold("Rincón de Tittenhurst: quédate junto al piano", asc, 8f, "John Lennon", "La escribí en este cuarto, y Yoko debería figurar como autora: sus poemas están dentro. Pide imaginar un mundo sin lo que nos separa. Pedir no cuesta nada, por eso molesta tanto."),
                Talk("Rincón de Ginsberg: habla con Peter Gabriel", gabriel.transform, "Peter Gabriel", "Oí en las noticias que Steve Biko había muerto bajo custodia policial. Yo no soy sudafricano: lo que hice fue contarlo con un tambor, y dar lo que ganara al movimiento. A veces lo único útil es que alguien más lo sepa."),
                Hold("Rincón de Kingston: quédate escuchando la guitarra", yard, 8f, "Bob Marley", "Solo una guitarra y yo, sin la banda, ya enfermo. La idea es de Marcus Garvey: nadie más que tú puede liberarte la mente. Es lo último que quise decir."),
                Talk("Rincón de los Andes: habla con Susana Baca", baca.transform, "Susana Baca", "Calle 13 llamó a tres mujeres: Totó, de Colombia; Maria Rita, de Brasil, y yo, de Perú. Es una canción sobre lo que no se puede comprar: la tierra, la gente, la memoria de un continente."),
                Talk("Vuelve con la cronista", cron.transform, "Cronista", "Ninguna paró una guerra sola. Pero cada una le dio voz a mucha gente, y una voz compartida ya es un principio de algo."),
            },
            "La Plaza de las Canciones, un sitio inventado, sobre el piso de Música del santuario a escala de hormiga, un 21 de junio, Día de la Música. Una cronista te guía por seis rincones de época, cada uno con la historia de una canción que cambió algo. E para hablar; para escuchar, quédate quieto.",
            new[]
            {
                "Este microcosmos es un lugar inventado: reúne en un mismo sitio seis canciones reales, sin reproducir sus letras: de cada una, su autor, su época y su tema contado con palabras propias.",
                "We Shall Overcome. Viene de himnos afroamericanos. En 1945 Lucille Simmons y otras huelguistas del tabaco de Charleston la cantaban lentamente en el piquete; Zilphia Horton la aprendió de ellas y la llevó a la Escuela Highlander (Tennessee); Pete Seeger la difundió, y Guy Carawan la enseñó en 1960 en la fundación del SNCC. Fue el himno de los derechos civiles. El 15 de marzo de 1965, tras Selma, Lyndon Johnson pronunció sus palabras ante el Congreso. Bella Ciao. Su melodía se atribuye a un canto de las mondine, las escardadoras de arroz del valle del Po, aunque el origen se discute (el investigador Fausto Giovannardi apunta a una melodía yidis); la versión partisana hoy se canta en el mundo entero, pero los historiadores dudan de cuánto se cantó de verdad durante la Resistencia (Cesare Bermani la sitúa en la Brigada Maiella). Se hizo global con la serie La casa de papel.",
                "Imagine. John Lennon la grabó en mayo de 1971 en su estudio de Tittenhurst Park (Ascot), inspirada en los poemas de Yoko Ono, a quien reconoció después como coautora. Pide imaginar un mundo sin fronteras ni posesiones. Biko. Steve Biko, líder de la Conciencia Negra, murió bajo custodia policial el 12 de septiembre de 1977 en Sudáfrica. Peter Gabriel publicó esta canción en 1980 y donó lo recaudado (más de 50 000 libras) al movimiento; inspiró Sun City y las campañas contra el apartheid. Redemption Song. Bob Marley la grabó sola, con guitarra acústica, en Uprising (1980), ya enfermo; recoge una idea de un discurso de Marcus Garvey en Nueva Escocia en 1937, sobre liberarse de la esclavitud mental. Latinoamérica. Calle 13 (Puerto Rico) la publicó en 2011 con Totó la Momposina, Susana Baca y Maria Rita; el video se rodó en Perú y ganó el Grammy Latino a la grabación y la canción del año. Canta la historia común del continente y lo que no se vende.",
                "Las frases son de juego, también las de personas reales: resumen lo que dijeron o lo que cuentan sus biografías. Sobre lo discutido: ninguna canción de estas paró una guerra por sí sola, y varias se han usado también con fines que sus autores no eligieron. El momento de esta historia: unas huelguistas cantan en el piquete y su canción acaba ante el Congreso; un músico dona lo que gana por contar una muerte lejana; un hombre enfermo graba solo su último mensaje. Recreación: la cronista y la mondina son inventadas.",
            },
            new[]
            {
                "Library of Congress, Folklife Today (2014), «Tracing the Long Journey of We Shall Overcome»; Highlander Center; PBS American Experience, «LBJ: We Shall Overcome».",
                "Wikipedia, «Bella ciao»; Tribune (2021), «Bella Ciao: the song of the partisans».",
                "Wikipedia, «Imagine (John Lennon album)»; The Beatles Bible, «Recording: Imagine by John Lennon»; MusicRadar sobre el papel de Yoko Ono.",
                "Wikipedia, «Biko (song)»; PRI The World (2024), «Peter Gabriel remakes classic protest song Biko».",
                "CBC (2019), «Bob Marley's Canadian connection to Redemption Song»; Tattrie, J. (2016), Redemption Songs, Pottersfield Press.",
                "Wikipedia, «Latinoamérica (song)».",
            },
            "Cronista", "Una canción no cambia el mundo. Cambia a quien la canta, y ese sí.");
        return n;
    }

    // ══FIN DE LAS HISTORIAS══
}
