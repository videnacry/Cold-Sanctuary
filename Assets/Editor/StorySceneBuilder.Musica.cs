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

    // ══FIN DE LAS HISTORIAS══
}
