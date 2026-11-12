using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA HISTORIA MAYA: PAKAL DE PALENQUE, EL REY QUE RENACE COMO EL MAÍZ (docs/microcosmos-maya-pakal.md; Berón 2026-11-03:
/// «investiga sobre los mayas para recrear una de sus historias más bonitas en el microcosmos»).
/// K'inich Janaab Pakal (603–683) recibió la corona de su madre, Ix Sak K'uk', a los 12 años (29-7-615, el Tablero Oval de la Casa
/// E del Palacio); gobernó 68 años Lakamha', «el lugar de las grandes aguas», levantó el Palacio y su acueducto sobre el Otolum, y
/// mandó construir su tumba bajo el Templo de las Inscripciones, cuyos tableros cuentan la historia de su dinastía en 617 glifos.
/// Murió el 28-8-683 (9.12.11.5.18) y su hijo K'inich Kan Bahlam lo enterró; la LÁPIDA de su sarcófago lo muestra cayendo a las
/// fauces del inframundo y, a la vez, renaciendo como el maíz, al pie del árbol del mundo (la ceiba) con el ave celeste encima: la
/// misma muerte y resurrección del dios del maíz que cuenta el Popol Vuh. Un tubo de piedra (el «psicoducto») une la tumba con el
/// templo de arriba. (Martin y Grube 2008; Stuart y Stuart 2008; Schele y Freidel 1990; Ruz Lhuillier 1973; Tedlock 1996.)
/// Las frases son de juego, en la voz de cada uno; los hechos y las fechas, de las inscripciones leídas.
/// </summary>
public static partial class StorySceneBuilder
{
    internal const string PakalName = "K'inich Janaab Pakal";

    static int Pakal(Transform t)
    {
        Material stone = AreaMicrocosmos.Mat("Hist_Caliza", new Color(0.84f, 0.81f, 0.72f)), red = AreaMicrocosmos.Mat("Hist_Estuco_rojo", new Color(0.75f, 0.3f, 0.24f));
        Material water = AreaMicrocosmos.Mat("Hist_Agua", new Color(0.3f, 0.5f, 0.7f)), maize = AreaMicrocosmos.Mat("Hist_Mazorca", new Color(0.95f, 0.8f, 0.3f));
        Material jade = AreaMicrocosmos.Mat("Hist_Jade", new Color(0.25f, 0.6f, 0.45f)), bark = AreaMicrocosmos.Mat("Hist_Corteza_ceiba", new Color(0.62f, 0.6f, 0.52f));
        Material leaf = AreaMicrocosmos.Mat("Hist_Hoja", new Color(0.25f, 0.45f, 0.22f)), glyph = AreaMicrocosmos.Mat("Hist_Glifo", new Color(0.55f, 0.52f, 0.45f));

        // 1 · 615: EL TABLERO OVAL (Casa E del Palacio): Ix Sak K'uk' le da a su hijo de 12 años el tocado real (sak hunal).
        Transform oval = Spot(t, "Tablero Oval", new Vector3(-13f, 0f, 8f), "Tablero Oval: la corona de su madre (615)", 3f);
        AreaMicrocosmos.Box(oval, "Banca del trono", new Vector3(0f, 0.25f, 0f), new Vector3(3f, 0.5f, 1.2f), stone, true);
        AreaMicrocosmos.Box(oval, "Tablero", new Vector3(0f, 1.6f, 0.65f), new Vector3(1.6f, 1.2f, 0.12f), stone, false);
        AreaMicrocosmos.Box(oval, "Tocado de jade", new Vector3(0.3f, 1.65f, 0.58f), new Vector3(0.35f, 0.25f, 0.05f), jade, false);

        // 2 · LAKAMHA', «EL LUGAR DE LAS GRANDES AGUAS»: el Otolum entra bajo el Palacio por su acueducto abovedado.
        Transform river = Spot(t, "Acueducto del Otolum", new Vector3(14f, 0f, 6f), "El Otolum y su acueducto (Lakamha', «grandes aguas»)", 2.6f);
        AreaMicrocosmos.Box(river, "Agua del Otolum", new Vector3(0f, 0.02f, 0f), new Vector3(3f, 0.04f, 16f), water, false);
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(river, "Bóveda del acueducto", new Vector3(0f, 0.6f, 4f + k * 1.4f), new Vector3(3.6f, 0.35f, 1f), stone, false);

        // 3 · LA MILPA: el maíz, la carne de las personas (Popol Vuh); tres mazorcas al altar de la Cruz Foliada.
        Transform altar = Spot(t, "Altar del maíz", new Vector3(13f, 0f, 17f), "Altar de la Cruz Foliada (el maíz)", 2.4f);
        AreaMicrocosmos.Box(altar, "Altar", new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 1f), stone, true);
        AreaMicrocosmos.Box(altar, "Planta de maíz con rostros", new Vector3(0f, 1.6f, 0.3f), new Vector3(0.25f, 1.4f, 0.25f), jade, false);
        var cobs = new List<Transform>();
        foreach (var p in new[] { new Vector3(-7f, 0.15f, -9f), new Vector3(5f, 0.15f, -10f), new Vector3(9f, 0.15f, -4f) })
        { Transform c = AreaMicrocosmos.Box(t, "Mazorca", p, new Vector3(0.25f, 0.25f, 0.6f), maize, false); Label(c, "Mazorca de maíz", 0.6f); cobs.Add(c); }

        // 4 · LOS TABLEROS DE LAS INSCRIPCIONES: tres tableros, 617 glifos, la historia de la dinastía (los escribas, aj tz'ib).
        var panels = new List<Transform>();
        foreach (var (name, p) in new[] { ("Tablero este", new Vector3(-18f, 0f, -6f)), ("Tablero central", new Vector3(-14f, 0f, -11f)), ("Tablero oeste", new Vector3(-9f, 0f, -15f)) })
        {
            Transform s = Spot(t, name, p, $"{name} de las Inscripciones", 2.6f);
            AreaMicrocosmos.Box(s, "Tablero", new Vector3(0f, 1.1f, 0f), new Vector3(1.4f, 2f, 0.25f), stone, true);
            for (int k = 0; k < 6; k++) AreaMicrocosmos.Box(s, "Glifo", new Vector3(-0.45f + (k % 3) * 0.45f, 1.5f - (k / 3) * 0.5f, -0.14f), new Vector3(0.3f, 0.3f, 0.04f), glyph, false);
            panels.Add(s);
        }

        // 5 · 683: LA LÁPIDA (3,8 × 2,2 m): Pakal cae y renace como el maíz al pie de la ceiba, el árbol del mundo, con el ave celeste.
        Transform lid = Spot(t, "Lápida de Pakal", new Vector3(0f, 0f, 16f), "La lápida de Pakal: cae al inframundo y renace como el maíz (683)", 2.4f);
        AreaMicrocosmos.Box(lid, "Sarcófago", new Vector3(0f, 0.5f, 0f), new Vector3(3.8f, 1f, 2.2f), red, true);
        AreaMicrocosmos.Box(lid, "Lápida", new Vector3(0f, 1.1f, 0f), new Vector3(3.8f, 0.25f, 2.2f), stone, true);
        AreaMicrocosmos.Box(lid, "Relieve: el árbol del mundo", new Vector3(0f, 1.24f, 0.2f), new Vector3(0.18f, 0.03f, 1.6f), glyph, false);
        AreaMicrocosmos.Box(lid, "Relieve: los brazos del árbol", new Vector3(0f, 1.24f, 0.55f), new Vector3(1.6f, 0.03f, 0.15f), glyph, false);
        AreaMicrocosmos.Box(lid, "Relieve: Pakal renaciendo", new Vector3(0f, 1.25f, -0.45f), new Vector3(0.7f, 0.04f, 0.5f), jade, false);
        AreaMicrocosmos.Box(lid, "Psicoducto (tubo de piedra al templo)", new Vector3(1.6f, 1.2f, -0.9f), new Vector3(0.2f, 0.2f, 1.2f), stone, false);
        Transform ceiba = Tree(t, "Ceiba (el árbol del mundo)", new Vector3(-7f, 0f, 19f), bark, leaf, 9f);

        // ── La gente: insectos-humanos de Lakamha' (el elenco renacido). Pakal es una HORMIGA CORTADORA (Atta: la que cultiva, como
        //    el pueblo del maíz), anciano y erguido; su madre, una luciérnaga; su hijo, un saltamontes; su esposa, una mariquita.
        int n = 0;
        GameObject F(string name, Vector3 pos, string species, float h, Color shirt, bool female)
        { GameObject g = AreaMicrocosmos.Folk(t, name, pos, h, 1f, species, null, female); CubeInsect.WearShirt(g, shirt); n++; return g; }
        Color white = new Color(0.94f, 0.92f, 0.86f), blue = new Color(0.25f, 0.45f, 0.65f), reed = new Color(0.75f, 0.6f, 0.35f);
        GameObject pakal = F($"{PakalName} (Atlas)", new Vector3(-10f, 0f, 4f), "Ant", 1.62f, white, false);
        GameObject mother = F("Ix Sak K'uk' (Ruth)", new Vector3(-16f, 0f, 6f), "Firefly", 1.6f, blue, true);
        GameObject son = F("K'inich Kan Bahlam (Héspero)", new Vector3(4f, 0f, 13f), "Grasshopper", 1.75f, reed, false);
        GameObject wife = F("Ix Tz'akbu Ajaw (Medea)", new Vector3(-4f, 0f, 12f), "Ladybug", 1.6f, new Color(0.7f, 0.3f, 0.3f), true);
        GameObject scribe = F("Escriba de Pakal (Sakshi)", new Vector3(-12f, 0f, -7f), "Cicada", 1.7f, white, false); Routine(scribe, panels[0], panels[1], panels[2]);
        GameObject farmer = F("Campesina de la milpa (Momo)", new Vector3(8f, 0f, -8f), "Termite", 1.6f, reed, true); Routine(farmer, altar, cobs[2]);
        Label(pakal.transform, PakalName, 2.1f);

        var steps = new[]
        {
            Talk("Lakamha', 683: habla con el anciano K'inich Janaab Pakal", pakal.transform, PakalName, "Tengo ochenta años y he gobernado sesenta y ocho. Ven: te enseño mi ciudad antes de irme."),
            Reach("615: ve al Tablero Oval de la Casa Blanca", oval, PakalName, "Aquí, con doce años, mi madre me puso la corona."),
            Talk("Habla con Ix Sak K'uk', su madre", mother.transform, "Ix Sak K'uk'", "Gobernamos cuando nadie más podía. Esta corona blanca no es tuya: la llevas por todos."),
            Hold("Quédate ante el Tablero Oval: la madre le da el tocado al hijo", oval, 8f, PakalName, "La piedra lo cuenta para siempre: ella sentada en su trono, yo recibiendo el tocado de jade."),
            Reach("Ve al Otolum, el río que corre bajo el Palacio", river, PakalName, "Lakamha' quiere decir «grandes aguas». Encauzamos el río en un acueducto de piedra: el agua pasa por debajo de la ciudad."),
            Hold("Escucha el agua bajo la bóveda", river, 8f, PakalName, "Una ciudad vive de su agua y de su maíz. Lo demás son piedras bonitas."),
            Collect("La milpa: lleva tres mazorcas al altar de la Cruz Foliada", cobs, altar, "Campesina de la milpa", "Los dioses hicieron a las personas de masa de maíz. Lo que sembramos nos hace."),
            Visit("Los tableros del Templo: toca los tres", panels, "Escriba de Pakal", "Seiscientos diecisiete glifos: quién reinó, cuándo, qué ofrendó. Escribimos para que los nietos sepan de dónde vienen."),
            Talk("683: habla con K'inich Kan Bahlam, su hijo", son.transform, "K'inich Kan Bahlam", "Mi padre ha muerto hoy. Bajaremos su cuerpo por la escalera escondida, al corazón del templo."),
            Hold("Ante la lápida: Pakal cae al inframundo y renace como el maíz, al pie de la ceiba", lid, 12f, "K'inich Kan Bahlam", "Mira: cae a las fauces de la tierra, como cae la semilla. Y de él sale el árbol del mundo, como sale la planta de maíz."),
            Talk("Habla con Ix Tz'akbu Ajaw", wife.transform, "Ix Tz'akbu Ajaw", "Dejamos un tubo de piedra desde su tumba hasta el templo: para que pueda seguir hablando con los vivos."),
        };

        Mission(t, "Pakal de Palenque: el rey que renace como el maíz", StoryMission.Marga.Bonds, 60f, steps,
            "Lakamha' (Palenque), en la selva de Chiapas, el 28 de agosto de 683, sobre el Huerto del santuario a escala de hormiga (pendiente de su piso: un templo de los antepasados). El anciano rey K'inich Janaab Pakal te enseña su vida: la corona que le dio su madre, el agua y el maíz de su ciudad, los glifos que la cuentan, y su muerte, que es también un renacer. E para hablar, recoger y tocar; en los pasos de mirar, quédate quieto.",
            new[]
            {
                "K'inich Janaab Pakal nació el 23 de marzo de 603 y subió al trono de Lakamha' (Palenque) el 29 de julio de 615, con 12 años. Su madre, Ix Sak K'uk', había gobernado antes que él; el Tablero Oval de la Casa E del Palacio la muestra dándole el tocado real. Palenque venía de dos derrotas ante Calakmul (599 y 611).",
                "En sus 68 años de reinado reconstruyó el Palacio (la Casa E, la «Casa Blanca», se dedicó en 654). La ciudad encauzó el arroyo Otolum en un acueducto abovedado bajo el Palacio (del Clásico tardío; su fecha exacta no se conoce); el nombre antiguo de la ciudad, Lakamha', significa «grandes aguas». Se casó con Ix Tz'akbu Ajaw en 626.",
                "Mandó construir su tumba bajo el Templo de las Inscripciones, cuyos tres tableros tienen 617 glifos: la historia de su dinastía, una de las inscripciones mayas más largas. Murió el 28 de agosto de 683 (9.12.11.5.18 en la Cuenta Larga) y su hijo K'inich Kan Bahlam terminó el templo.",
                "En 1952 el arqueólogo Alberto Ruz Lhuillier encontró la escalera escondida y la cripta. La lápida del sarcófago (3,8 × 2,2 m) muestra a Pakal en el momento de morir y renacer: cae a las fauces del inframundo y de él surge el árbol del mundo, con el ave celeste encima; se ha leído como el renacer del dios del maíz, el mismo que cuenta el Popol Vuh. Un conducto de piedra une la tumba con el templo.",
            },
            new[]
            {
                "Martin, S. y Grube, N. (2008). Chronicle of the Maya Kings and Queens, 2.ª ed. Thames & Hudson (Palenque: K'inich Janaab Pakal I).",
                "Stuart, D. y Stuart, G. (2008). Palenque: Eternal City of the Maya. Thames & Hudson.",
                "Schele, L. y Freidel, D. (1990). A Forest of Kings. William Morrow (la lápida y el árbol del mundo).",
                "Ruz Lhuillier, A. (1973). El Templo de las Inscripciones, Palenque. INAH.",
                "Barnhart, E. (2001). The Palenque Mapping Project. FAMSI (el plano de la ciudad).",
                "Tedlock, D. (trad.) (1996). Popol Vuh. Simon & Schuster (el maíz y el renacer de los gemelos y de su padre).",
            },
            "Ix Tz'akbu Ajaw", "No se fue: está en el maíz que crece cada año, y en las piedras que lo cuentan.");
        return n;
    }
}
