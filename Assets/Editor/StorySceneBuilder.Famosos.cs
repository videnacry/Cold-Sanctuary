using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MICROCOSMOS DE LOS FAMOSOS (docs/microcosmos-famosos.md; Berón 2026-11-04: «los personajes famosos, elon musk, sadhguru, y demás
/// sácalos del mesocosmos y crea sus microcosmos respectivos»). Cada figura vive UNA historia de su vida pública, bien documentada,
/// respetuosa e inspiradora, en el PUEBLO REAL donde ocurrió (CivicBuilder.Famosos: su plano, su estilo de época y lugar), sobre el
/// piso del santuario que mejor la cuenta (MicroFloors). Como los científicos: insectos-humanos (su insecto por etología, su ropa de
/// referencia por los colores de su versión humana), sitios de la misión en el claro y la HistoryCard con fuentes al terminar.
/// Las frases son de juego, en la voz de cada uno; los hechos y las fechas, de las fuentes del doc.
/// </summary>
public static partial class StorySceneBuilder
{
    internal static readonly string[] FamousScenes =
    {
        "Microcosmos_Sadhguru",
    };

    /// <summary>Genera SOLO los microcosmos de los famosos, uno tras otro (con la escena principal abierta). Con poca memoria, mejor
    /// uno a uno con su propio menú.</summary>
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos (todos, uno tras otro)")]
    public static void BuildFamous() { foreach (string sc in FamousScenes) { try { BuildOne(sc); } catch (System.Exception e) { Debug.LogWarning($"[Historias] {sc}: {e.Message}"); } } }

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos/Sadhguru (Velliangiri, 1999)")]
    public static void BuildSadhguru() => BuildOne("Microcosmos_Sadhguru");

    // ══ SADHGURU · Isha Yoga Center, al pie de los montes Velliangiri (Coimbatore), 24 de junio de 1999 · Sala de Yoga ══════════════

    static int Sadhguru(Transform t)
    {
        Material granite = M("Granito_negro", 0.16f, 0.16f, 0.17f), brick = M("Ladrillo_de_barro", 0.66f, 0.42f, 0.3f), mud = M("Mortero_de_barro", 0.55f, 0.42f, 0.3f);
        Material stone = M("Piedra_clara", 0.85f, 0.82f, 0.74f), copper = M("Cobre", 0.75f, 0.45f, 0.25f), water = M("Agua_Noyyal", 0.3f, 0.5f, 0.6f), cloth = M("Tela_blanca", 0.95f, 0.94f, 0.9f);

        // 1 · EL DHYANALINGA: el linga de granito bajo su cúpula de ladrillo y barro (sin acero ni hormigón). La cúpula es la historia:
        //     se queda abierta (sin muros: regla 2), solo su anillo de arranque y los arcos.
        Transform linga = Spot(t, "Dhyanalinga", new Vector3(0f, 0f, 15f), "El Dhyanalinga (consagrado el 24 de junio de 1999)", 4.5f);
        Prop(linga, "Avudaiyar (base de granito)", new Vector3(0f, 0.3f, 0f), new Vector3(2.6f, 0.6f, 2.6f), granite);
        Prop(linga, "Linga de granito", new Vector3(0f, 2f, 0f), new Vector3(1.1f, 2.8f, 1.1f), granite);
        Prop(linga, "Anillo de cobre", new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 0.12f, 1.2f), copper, false);
        for (int k = 0; k < 8; k++)
        {
            float a = k * Mathf.PI / 4f;
            Prop(linga, "Arranque de la cúpula (ladrillo y barro)", new Vector3(Mathf.Cos(a) * 7f, 0.4f, Mathf.Sin(a) * 7f), new Vector3(1.6f, 0.8f, 0.6f), brick);
        }
        for (int k = 0; k < 6; k++) Prop(linga, "Cueva de meditación (nicho)", new Vector3(-5f + k * 2f, 0.15f, -5.5f), new Vector3(1.2f, 0.3f, 1.2f), stone, false);

        // 2 · EL SARVA DHARMA STAMBHA: la columna de la entrada, con los símbolos de muchas religiones (el templo es para todos).
        Transform pillar = Spot(t, "Sarva Dharma Stambha", new Vector3(-12f, 0f, 2f), "Sarva Dharma Stambha (la columna de todas las religiones)", 6f);
        Prop(pillar, "Columna de granito", new Vector3(0f, 2.5f, 0f), new Vector3(0.8f, 5f, 0.8f), stone);
        for (int k = 0; k < 4; k++) Prop(pillar, "Símbolo tallado", new Vector3(0f, 1f + k * 1f, -0.42f), new Vector3(0.45f, 0.45f, 0.04f), granite, false);

        // 3 · LA OBRA: ladrillos de barro cocido para terminar el anillo de la cúpula (los voluntarios).
        var bricks = Items(t, brick, new Vector3(0.5f, 0.25f, 0.25f), ("Ladrillo de barro", new Vector3(10f, 0.12f, -8f)), ("Ladrillo de barro", new Vector3(13f, 0.12f, -6f)), ("Ladrillo de barro", new Vector3(16f, 0.12f, -9f)));
        Prop(t, "Montón de mortero de barro", new Vector3(13f, 0.2f, -10f), new Vector3(1.6f, 0.4f, 1.2f), mud, false);

        // 4 · EL ASHRAM: la cocina (comida para todos, sentados en el suelo), el huerto y el arroyo.
        Transform kitchen = Spot(t, "Cocina del ashram", new Vector3(14f, 0f, 6f), "La cocina del ashram (comer en silencio, en el suelo)", 3f);
        Prop(kitchen, "Fogón de gas con olla de acero", new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.8f), M("Acero", 0.7f, 0.72f, 0.75f));
        Prop(kitchen, "Hojas de plátano (platos)", new Vector3(0f, 0.02f, -2f), new Vector3(2.4f, 0.04f, 0.6f), M("Hoja_platano", 0.3f, 0.55f, 0.25f), false);
        Transform garden = Spot(t, "Huerto del ashram", new Vector3(17f, 0f, 16f), "El huerto (cocos, plátanos y verduras)", 2.5f);
        Prop(garden, "Bancal", new Vector3(0f, 0.15f, 0f), new Vector3(4f, 0.3f, 2.2f), M("Tierra_roja", 0.55f, 0.32f, 0.22f));
        Transform stream = Spot(t, "Arroyo de la Noyyal", new Vector3(-17f, 0f, 14f), "El arroyo que baja de Velliangiri (la Noyyal)", 2.5f);
        Prop(stream, "Agua del arroyo", new Vector3(0f, 0.02f, 0f), new Vector3(3f, 0.04f, 14f), water, false);
        var chores = new List<Transform> { kitchen, garden, stream };

        // 5 · EL MIRADOR: los siete montes de Velliangiri, el «Kailash del sur».
        Transform view = Spot(t, "Mirador de Velliangiri", new Vector3(-16f, 0f, -6f), "Mirador: los siete montes de Velliangiri", 3f);
        Prop(view, "Piedra para sentarse", new Vector3(0f, 0.25f, 0f), new Vector3(1.6f, 0.5f, 1f), stone);
        Prop(view, "Nandi (el toro que espera, de cara al linga)", new Vector3(3f, 0.6f, 4f), new Vector3(0.9f, 1.2f, 1.8f), granite);

        // La gente: insectos-humanos. Sadhguru, una CIGARRA (años quieta bajo tierra y una muda: la transformación interior; su canto),
        // con los colores de su versión humana: túnica y turbante blancos, barba.
        Color white = new Color(0.95f, 0.94f, 0.9f), ochre = new Color(0.85f, 0.6f, 0.3f), sage = new Color(0.5f, 0.72f, 0.62f);
        GameObject sg = Ancient(t, "Sadhguru (Jaggi Vasudev)", new Vector3(2.5f, 0f, 8f), false, white, beard: true, h: 1.72f, species: "Cicada");
        CubeInsect.WearHat(sg, CubeInsect.Hat.Turban);
        GameObject vol = Ancient(t, "Voluntaria del ashram (Sakshi)", new Vector3(-10f, 0f, 0f), true, sage);
        GameObject mason = Ancient(t, "Albañil voluntario (Atlas)", new Vector3(11f, 0f, -11f), false, ochre); Routine(mason, linga);
        GameObject cook = Ancient(t, "Cocinera del ashram (Ruth)", new Vector3(14f, 0f, 3f), true, white); Routine(cook, kitchen, garden);
        GameObject villager = Ancient(t, "Vecino de Ikkarai Boluvampatti (Momo)", new Vector3(-14f, 0f, -9f), false, new Color(0.6f, 0.5f, 0.38f)); Routine(villager, view, stream);
        int n = 5;

        Mission(t, "Sadhguru en Velliangiri: el Dhyanalinga (1999)", StoryMission.Marga.Yoga, 60f,
            new[]
            {
                Talk("Habla con Sadhguru junto al Dhyanalinga", sg.transform, "Sadhguru", "Llevamos tres años preparando este espacio. No es para rezar ni para pedir nada: es para sentarse en silencio. Quien entre, de la fe que sea o de ninguna, puede quedarse quieto aquí."),
                Reach("Ve a la columna de la entrada, el Sarva Dharma Stambha", pillar, "Voluntaria del ashram", "Lleva los símbolos de muchas religiones. Es la puerta: aquí nadie pregunta de dónde vienes."),
                Collect("La obra: lleva tres ladrillos de barro al anillo de la cúpula", bricks, linga, "Albañil voluntario", "Ladrillo y mortero de barro, sin acero ni hormigón. La cúpula se sostiene sola por su forma."),
                Visit("Seva: ayuda en la cocina, en el huerto y en el arroyo", chores, "Cocinera del ashram", "Aquí el trabajo también es práctica: lo que haces con atención te cambia tanto como lo que haces sentado."),
                Hold("Siéntate bajo la cúpula, en silencio", linga, 12f, "Sadhguru", "No hagas nada. Ni siquiera intentes meditar. Solo quédate."),
                Talk("Habla con el vecino de Ikkarai Boluvampatti", villager.transform, "Vecino", "Antes aquí solo había matorral y elefantes que bajaban del monte. Ahora vienen de muy lejos; los del pueblo traemos verdura y nos quedamos a comer."),
                Hold("En el mirador, mira los siete montes de Velliangiri", view, 8f, "Voluntaria del ashram", "Los peregrinos suben los siete montes descalzos. Dicen que es el Kailash del sur."),
                Talk("Vuelve con Sadhguru", sg.transform, "Sadhguru", "El templo ya está hecho. Ahora lo importante es lo que cada uno haga consigo mismo."),
            },
            "El Isha Yoga Center, al pie de los montes Velliangiri, a unos 40 km de Coimbatore (Tamil Nadu, India), el 24 de junio de 1999, sobre la Sala de Yoga del santuario a escala de hormiga. Ese día termina la consagración del Dhyanalinga, un espacio de meditación abierto a todos. Recórrelo con Sadhguru y los voluntarios del ashram. E para hablar, recoger y ayudar; para meditar, quédate quieto.",
            new[]
            {
                "Jaggi Vasudev, conocido como Sadhguru, nació en Mysuru (Karnataka) en 1957. Cuenta que en 1982, sentado en una roca de la colina de Chamundi, tuvo una experiencia que cambió su vida; desde entonces enseña yoga y meditación. En 1992 fundó la Fundación Isha.",
                "En 1994 la fundación compró tierra al pie de los montes Velliangiri, en los Ghats occidentales, cerca de la aldea de Ikkarai Boluvampatti, y allí se levantó el Isha Yoga Center. Los Velliangiri son un lugar de peregrinación (la subida de los siete montes) y en ellos nace el río Noyyal.",
                "El Dhyanalinga es un linga de granito bajo una cúpula de ladrillo y mortero de barro, construida sin acero ni hormigón. Según la fundación, su consagración (prāṇa pratiṣṭhā) duró tres años y terminó el 24 de junio de 1999 (algunas fuentes dan el 23); se abrió al público el 23 de noviembre de 1999. No es un templo de culto: no hay ritos ni ofrendas, solo silencio, y está abierto a personas de cualquier fe o de ninguna; en la entrada, el Sarva Dharma Stambha muestra símbolos de varias religiones.",
                "Las dimensiones exactas de la cúpula y del linga, y lo que ocurre durante una consagración, se dan aquí como los cuenta la fundación. Isha y su fundador también han recibido críticas (por ejemplo, sobre permisos de construcción en la falda del monte); esta historia se limita a 1999.",
            },
            new[]
            {
                "Isha Foundation, «Dhyanalinga – His Only Obsession» (isha.sadhguru.org/en/blog/article/dhyanalinga-his-only-obsession).",
                "Isha Foundation, «Dhyanalinga Consecration Anniversary» (isha.sadhguru.org/events/annual-events/dhyanalinga-consecration-anniversary).",
                "Covai Post (24-6-2019), «20th anniversary of Isha Dhyanalinga celebrated» (covaipost.com).",
                "Sadhguru (2016). Inner Engineering: A Yogi's Guide to Joy. Spiegel & Grau (la colina de Chamundi, 1982).",
            },
            "Sadhguru", "Lo que buscas no está en este lugar. Este lugar solo te ayuda a dejar de buscar fuera.");
        return n;
    }
}
