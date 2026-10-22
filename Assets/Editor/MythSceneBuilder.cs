using UnityEditor;
using UnityEngine;

/// <summary>
/// EGINA, EL REINO DE LOS MIRMIDONES (docs/microcosmos-myrmidons.md): el mito de Ovidio (Metamorfosis VII) contado de
/// forma humana, sobre la CONSTRUCCIÓN del santuario ×10 (levantar un reino = construir). Éaco sueña bajo la encina; Zeus,
/// su padre, recluta a los insecto-humanos; unidos traen piedra de la cantera y alzan un templo dórico como el de Afaya
/// (Egina, c. 500–490 a. C.: 6 × 12 columnas, estilóbato de 28,8 × 13,7 m). El elenco de la enfermería renace aquí con
/// su nombre: Ambrosio, Atlas, Medea, Momo, Ruth, Héspero, Sakshi y Shanidar.
/// </summary>
public static class MythSceneBuilder
{
    public const string SceneName = "Microcosmos_Egina";

    [MenuItem("Tools/Cold Sanctuary/Build Egina (mirmidones)")]
    public static void BuildAll()
    {
        MicrocosmosAreaSceneBuilder.BuildStoryScene("Construction_Area", "Egina", SceneName, new Color(0.85f, 0.72f, 0.4f), new Vector3(6000f, 0f, 9500f), Egina);
        Debug.Log("[Egina] escena de los mirmidones generada.");
    }

    static readonly Color Stone = new Color(0.92f, 0.89f, 0.82f), StoneDark = new Color(0.8f, 0.76f, 0.68f), Tile = new Color(0.72f, 0.38f, 0.25f), Paint = new Color(0.2f, 0.28f, 0.5f);

    static int Egina(Transform t)
    {
        int n = 0;
        // EL TEMPLO (la obra), al fondo; su entrada mira a la llegada.
        var siteGo = new GameObject("Obra del templo");
        siteGo.transform.SetParent(t, false); siteGo.transform.localPosition = new Vector3(0f, 0f, 4f);
        siteGo.AddComponent<MicroLair>();
        var site = siteGo.AddComponent<GreekBuildSite>();
        site.stages = Temple(siteGo.transform, new Vector3(0f, 0f, 16f));
        Label(siteGo.transform, "Obra del templo (traed piedra)", 3f);

        // LA CANTERA: bloques de piedra que se recogen y vuelven a aparecer.
        Transform quarry = Spot(t, "Cantera", new Vector3(16f, 0f, -6f));
        Material rock = AreaMicrocosmos.Mat("Egina_Roca", StoneDark);
        for (int k = 0; k < 5; k++) AreaMicrocosmos.Box(quarry, "Roca", new Vector3(-3f + k * 1.6f, 1f + (k % 2) * 0.6f, -3f), new Vector3(1.5f, 2f + (k % 2) * 1.2f, 1.5f), rock, true);
        for (int k = 0; k < 10; k++)
        {
            Transform b = AreaMicrocosmos.Box(quarry, "Bloque de piedra", new Vector3(-2.5f + (k % 5) * 1.2f, 0.3f, (k / 5) * 1.3f), new Vector3(0.8f, 0.5f, 0.6f), rock, false);
            var mf = b.gameObject.AddComponent<MicroFood>(); mf.spread = 3f;
        }
        Label(quarry, "Cantera", 4f);

        // LA ENCINA SAGRADA (de una semilla de Dodona, dice Ovidio), con hormigas subiendo grano por el tronco.
        Transform oak = Spot(t, "Encina sagrada", new Vector3(-14f, 0f, 4f));
        AreaMicrocosmos.Box(oak, "Tronco", new Vector3(0f, 3f, 0f), new Vector3(1.2f, 6f, 1.2f), AreaMicrocosmos.Mat("Egina_Tronco", new Color(0.36f, 0.26f, 0.18f)), true);
        Material leaf = AreaMicrocosmos.Mat("Egina_Encina", new Color(0.25f, 0.42f, 0.2f));
        for (int k = 0; k < 6; k++) AreaMicrocosmos.Box(oak, "Copa", new Vector3(Mathf.Cos(k * 1.05f) * 2.2f, 7f + (k % 2), Mathf.Sin(k * 1.05f) * 2.2f), new Vector3(3.4f, 2.2f, 3.4f), leaf, false);
        Material grain = AreaMicrocosmos.Mat("Egina_Grano", new Color(0.88f, 0.76f, 0.45f));
        for (int k = 0; k < 8; k++) AreaMicrocosmos.Box(oak, "Grano", new Vector3(0.65f, 0.6f + k * 0.65f, Mathf.Sin(k) * 0.3f), Vector3.one * 0.12f, grain, false);
        Label(oak, "Encina sagrada (aquí sueña Éaco)", 9.5f);

        // Los PUESTOS que no son cargar piedra: el hogar común (pritaneo), las terrazas, la mesa de las leyes y la stoa.
        Transform hearth = Spot(t, "Hogar común (pritaneo)", new Vector3(-12f, 0f, 14f));
        AreaMicrocosmos.Box(hearth, "Ara", new Vector3(0f, 0.5f, 0f), new Vector3(1.6f, 1f, 1.6f), AreaMicrocosmos.Mat("Egina_Ara", Stone), true);
        AreaMicrocosmos.Box(hearth, "Fuego", new Vector3(0f, 1.25f, 0f), new Vector3(0.7f, 0.5f, 0.7f), AreaMicrocosmos.Mat("Egina_Fuego", new Color(1f, 0.5f, 0.15f)), false);
        Label(hearth, "Hogar común", 2.4f);
        Transform terraces = Spot(t, "Terrazas", new Vector3(17f, 0f, 14f));
        Material soil = AreaMicrocosmos.Mat("Egina_Tierra", new Color(0.45f, 0.33f, 0.22f)), crop = AreaMicrocosmos.Mat("Egina_Cultivo", new Color(0.5f, 0.66f, 0.3f));
        for (int k = 0; k < 3; k++) { AreaMicrocosmos.Box(terraces, "Terraza", new Vector3(0f, 0.25f + k * 0.5f, k * 2.4f), new Vector3(8f, 0.5f + k * 1f, 2.4f), soil, true); AreaMicrocosmos.Box(terraces, "Cultivo", new Vector3(0f, 0.55f + k * 1f, k * 2.4f), new Vector3(7.4f, 0.12f, 2f), crop, false); }
        Label(terraces, "Terrazas (tierra subida a la roca)", 4f);
        Transform laws = Spot(t, "Mesa de las leyes", new Vector3(-10.5f, 0f, 1f));
        AreaMicrocosmos.Box(laws, "Mesa", new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.8f), AreaMicrocosmos.Mat("Egina_Mesa", StoneDark), true);
        Transform stoa = Stoa(t, new Vector3(-15f, 0f, 24f));

        // LOS PROTAGONISTAS: Éaco (el soñador) y Zeus (su padre, el que convence).
        GameObject eaco = AreaMicrocosmos.Ant(t, "Éaco", new Vector3(-12f, 0f, 5.5f), new Color(0.45f, 0.3f, 0.2f), 1.8f, 1f);
        DressGreek(eaco.transform, 1.8f, new Color(0.95f, 0.93f, 0.88f), wreath: false, beard: false); n++;
        GameObject zeus = AreaMicrocosmos.Ant(t, "Zeus", new Vector3(-15.5f, 0f, 6f), new Color(0.35f, 0.25f, 0.18f), 2.1f, 1.2f);
        DressGreek(zeus.transform, 2.1f, new Color(0.75f, 0.68f, 0.85f), wreath: true, beard: true); n++;
        zeus.AddComponent<MythRecruiter>().home = oak;

        // EL PUEBLO: el elenco de la enfermería, renacido, y otros mirmidones; cada uno con su condición para unirse.
        (string name, MythRecruit.Role role, Transform spot, float frac, int stage, float speed, string no, string yes)[] people =
        {
            ("Ambrosio", MythRecruit.Role.Carrier, null, 0f, 0, 2.6f, "", "Yo ya subía grano por la encina. Puedo subir piedra."),
            ("Atlas", MythRecruit.Role.Carrier, null, 0f, 0, 4.5f, "", "Dadme la piedra más pesada."),
            ("Héspero", MythRecruit.Role.Hearth, hearth, 0f, 0, 0f, "", "Que en este reino el fuego sea de todos."),
            ("Ruth", MythRecruit.Role.Terraces, terraces, 0f, 0, 0f, "", "Subiremos tierra a la roca: aquí crecerá trigo."),
            ("Sakshi", MythRecruit.Role.Scribe, laws, 0.25f, 0, 0f, "Primero quiero ver quiénes vienen.", "Escribiré las leyes de Éaco: que sean justas para el que menos tiene."),
            ("Shanidar", MythRecruit.Role.Elder, stoa, 0f, 0, 0f, "", "Ya no puedo cargar, pero puedo enseñar a los jóvenes en la stoa."),
            ("Medea", MythRecruit.Role.Carrier, null, 0.5f, 0, 3.2f, "Mi clan no trabaja para el sueño de otro.", "Si la mitad del pueblo ya trabaja, mi clan no se quedará mirando."),
            ("Momo", MythRecruit.Role.Carrier, null, 0f, 2, 3f, "¿Un reino de virtud? ¡Primero que Zeus cargue una piedra!", "Vale, vale: esas columnas no se rieron de mí. Me apunto."),
            ("Mirmidón 1", MythRecruit.Role.Carrier, null, 0f, 0, 3f, "", "Por el bien de todos."),
            ("Mirmidón 2", MythRecruit.Role.Carrier, null, 0f, 0, 3f, "", "Mis manos son vuestras."),
            ("Mirmidón 3", MythRecruit.Role.Carrier, null, 0.25f, 0, 3f, "Cuando vea que esto va en serio.", "Va en serio. Voy."),
            ("Mirmidón 4", MythRecruit.Role.Carrier, null, 0.25f, 0, 3f, "Ya veremos.", "Ya lo he visto: voy."),
            ("Mirmidona 5", MythRecruit.Role.Carrier, null, 0f, 0, 3f, "", "Construiremos juntos."),
            ("Mirmidona 6", MythRecruit.Role.Carrier, null, 0.5f, 0, 3f, "Mi familia aún duda.", "Mi familia viene conmigo."),
        };
        Color[] tunics = { new Color(0.95f, 0.93f, 0.88f), new Color(0.85f, 0.55f, 0.4f), new Color(0.55f, 0.65f, 0.8f), new Color(0.8f, 0.75f, 0.55f) };
        for (int i = 0; i < people.Length; i++)
        {
            var p = people[i];
            float a = (200f + i * (320f / people.Length)) * Mathf.Deg2Rad;   // en anillo alrededor del pueblo, dejando libre la llegada
            Vector3 pos = new Vector3(Mathf.Cos(a) * 24f, 0f, 8f + Mathf.Sin(a) * 20f);
            bool elder = p.role == MythRecruit.Role.Elder;
            GameObject g = AreaMicrocosmos.Ant(t, p.name, pos, new Color(0.4f, 0.26f, 0.18f), elder ? 1.6f : 1.8f, p.name == "Atlas" ? 1.4f : 1f);
            DressGreek(g.transform, elder ? 1.6f : 1.8f, tunics[i % tunics.Length], wreath: false, beard: elder);
            var r = g.AddComponent<MythRecruit>();
            r.displayName = p.name; r.role = p.role; r.workSpot = p.spot; r.joinsAtFraction = p.frac; r.joinsAtStage = p.stage;
            if (!string.IsNullOrEmpty(p.no)) r.refusal = p.no;
            r.acceptance = p.yes;
            if (p.role == MythRecruit.Role.Carrier) { var f = g.AddComponent<MicroForager>(); f.speed = p.speed; f.enabled = false; }
            n++;
        }

        var card = t.gameObject.AddComponent<HistoryCard>();
        var m = t.gameObject.AddComponent<MythMission>();
        m.site = site; m.kingLabel = eaco.GetComponent<WorldLabel>();
        card.title = m.title;
        card.intro = "Egina, en el microcosmos. Éaco sueña bajo la encina con un reino lleno de virtud y grandeza. Su padre, Zeus, " +
                     "conoce a todo el mundo: conmovido por el sueño de su hijo, va a ver a cada insecto-humano y le pide que se una. " +
                     "Ayúdale (E junto a alguien que aún no se ha unido). Unidos, traerán piedra de la cantera y alzarán el templo. " +
                     "Algunos esperan a ver que la mitad del pueblo ya trabaja; otros, a ver las columnas en pie.";
        card.history = new[]
        {
            "El mito: en las «Metamorfosis» de Ovidio (libro VII), una peste deja vacía la isla de Egina. El rey Éaco, hijo de Zeus, reza junto a una encina sagrada nacida de una semilla de Dodona, por cuyo tronco suben hormigas cargando grano, y pide tantos ciudadanos como hormigas.",
            "Esa noche sueña que las hormigas caen al suelo, crecen, se ponen en pie y toman forma humana; al despertar, el pueblo está ahí. Los llama mirmidones (de «mýrmēx», hormiga). Ovidio dice que conservaron las costumbres de antes: trabajan con paciencia, guardan el fruto de su trabajo y lo defienden con valor.",
            "Otras versiones antiguas cuentan lo mismo: Apolodoro (Biblioteca III, 12, 6) y la fábula 52 de Higino. Según la tradición, Éaco fue tan justo que tras su muerte fue juez de los muertos; y los mirmidones siguieron después a su nieto Aquiles a Troya.",
            "Aquí el mito se cuenta de forma humana: no hay magia, sino un sueño compartido, un padre que convence y un pueblo que trabaja por el bien común.",
            "El templo: el de Afaya, en Egina, se construyó hacia el 500–490 a. C. Es dórico y períptero, de 6 × 12 columnas sobre un estilóbato de 28,8 × 13,7 m, con columnas 5,33 veces más altas que su diámetro en la base, de caliza local cubierta de estuco y con tejas de arcilla.",
        };
        card.sources = new[]
        {
            "Ovidio, Metamorfosis VII, 517–660 (trad. inglesa de Brookes More, 1922, en Perseus/Scaife).",
            "Apolodoro, Biblioteca III, 12, 6; Higino, Fábulas 52 (vía Wikipedia, «Aegina (mythology)»).",
            "Brown University, Joukowsky Institute, «Temple of Aphaia»; Athens & Attica, «Temple of Athena Aphaia».",
        };
        return n;
    }

    // ── El templo dórico (modelo: Afaya), por etapas ─────────────────────────────────────────────────────────────

    static GameObject[] Temple(Transform site, Vector3 c)
    {
        const float W = 13.7f, L = 28.8f, D = 1f, ColH = 5.33f, Step = 0.35f;   // estilóbato real; columna = 5,33 × diámetro [A: D = 1 m]
        Material st = AreaMicrocosmos.Mat("Egina_Caliza", Stone), sd = AreaMicrocosmos.Mat("Egina_Caliza_oscura", StoneDark);
        Material tile = AreaMicrocosmos.Mat("Egina_Teja", Tile), paint = AreaMicrocosmos.Mat("Egina_Triglifo", Paint);
        GameObject Stage(string name) { var g = new GameObject(name); g.transform.SetParent(site, false); g.transform.localPosition = c; return g; }

        // 1) CREPIS: tres escalones.
        GameObject crepis = Stage("Etapa 1 · Crepis");
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(crepis.transform, "Escalon", new Vector3(0f, Step * (k + 0.5f), 0f), new Vector3(W + (2 - k) * 0.8f, Step, L + (2 - k) * 0.8f), sd, true);
        float top = Step * 3f;

        // 2) COLUMNAS: 6 × 12, perípteras (solo el perímetro: 32), de fuste octogonal, equino y ábaco.
        GameObject cols = Stage("Etapa 2 · Columnas");
        float hx = W * 0.5f - 0.75f, hz = L * 0.5f - 0.75f;
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 12; j++)
            {
                if (i != 0 && i != 5 && j != 0 && j != 11) continue;
                Transform col = new GameObject("Columna").transform; col.SetParent(cols.transform, false);
                col.localPosition = new Vector3(-hx + i * (2f * hx / 5f), top, -hz + j * (2f * hz / 11f));
                float shaft = ColH - 0.45f;
                AreaMicrocosmos.Box(col, "Fuste", new Vector3(0f, shaft * 0.5f, 0f), new Vector3(D * 0.86f, shaft, D * 0.86f), st, true);
                AreaMicrocosmos.Box(col, "Fuste", new Vector3(0f, shaft * 0.5f, 0f), new Vector3(D * 0.86f, shaft, D * 0.86f), Quaternion.Euler(0f, 45f, 0f), st, false);
                AreaMicrocosmos.Box(col, "Equino", new Vector3(0f, shaft + 0.12f, 0f), new Vector3(D * 1.1f, 0.25f, D * 1.1f), st, false);
                AreaMicrocosmos.Box(col, "Abaco", new Vector3(0f, shaft + 0.35f, 0f), new Vector3(D * 1.2f, 0.2f, D * 1.2f), sd, false);
            }

        // 3) CELLA: el muro interior, con la puerta hacia la entrada (-Z).
        GameObject cella = Stage("Etapa 3 · Cella");
        float cw = 4.2f, cz = 10f, wt = 0.5f;
        AreaMicrocosmos.Box(cella.transform, "Muro", new Vector3(-cw, top + ColH * 0.5f, 0f), new Vector3(wt, ColH, cz * 2f), st, true);
        AreaMicrocosmos.Box(cella.transform, "Muro", new Vector3(cw, top + ColH * 0.5f, 0f), new Vector3(wt, ColH, cz * 2f), st, true);
        AreaMicrocosmos.Box(cella.transform, "Muro", new Vector3(0f, top + ColH * 0.5f, cz), new Vector3(cw * 2f, ColH, wt), st, true);
        AreaMicrocosmos.Box(cella.transform, "Muro", new Vector3(-(cw + 1.1f) * 0.5f, top + ColH * 0.5f, -cz), new Vector3(cw - 1.1f, ColH, wt), st, true);
        AreaMicrocosmos.Box(cella.transform, "Muro", new Vector3((cw + 1.1f) * 0.5f, top + ColH * 0.5f, -cz), new Vector3(cw - 1.1f, ColH, wt), st, true);

        // 4) ENTABLAMENTO: arquitrabe, friso de triglifos (pintados, como lo estaban) y cornisa.
        GameObject ent = Stage("Etapa 4 · Entablamento");
        float ey = top + ColH;
        void Ring(float y, float h, float grow, Material m, string name)
        {
            AreaMicrocosmos.Box(ent.transform, name, new Vector3(0f, y + h * 0.5f, -hz), new Vector3(2f * hx + 1.2f + grow, h, 1.2f + grow), m, false);
            AreaMicrocosmos.Box(ent.transform, name, new Vector3(0f, y + h * 0.5f, hz), new Vector3(2f * hx + 1.2f + grow, h, 1.2f + grow), m, false);
            AreaMicrocosmos.Box(ent.transform, name, new Vector3(-hx, y + h * 0.5f, 0f), new Vector3(1.2f + grow, h, 2f * hz + 1.2f + grow), m, false);
            AreaMicrocosmos.Box(ent.transform, name, new Vector3(hx, y + h * 0.5f, 0f), new Vector3(1.2f + grow, h, 2f * hz + 1.2f + grow), m, false);
        }
        Ring(ey, 0.8f, 0f, st, "Arquitrabe");
        Ring(ey + 0.8f, 0.8f, 0f, st, "Friso");
        for (int i = 0; i < 6; i++) foreach (float z in new[] { -hz - 0.62f, hz + 0.62f }) AreaMicrocosmos.Box(ent.transform, "Triglifo", new Vector3(-hx + i * (2f * hx / 5f), ey + 1.2f, z), new Vector3(0.45f, 0.7f, 0.05f), paint, false);
        Ring(ey + 1.6f, 0.25f, 0.5f, sd, "Cornisa");

        // 5) FRONTÓN Y TEJADO: dos aguas de tejas de arcilla (15°) y los frontones triangulares (escalonados).
        GameObject roof = Stage("Etapa 5 · Frontón y tejado");
        float ry = ey + 1.85f, half = hx + 1.2f, rise = half * Mathf.Tan(15f * Mathf.Deg2Rad), len = half / Mathf.Cos(15f * Mathf.Deg2Rad);
        AreaMicrocosmos.Box(roof.transform, "Tejado", new Vector3(-half * 0.5f, ry + rise * 0.5f, 0f), new Vector3(len, 0.25f, 2f * hz + 2.2f), Quaternion.Euler(0f, 0f, 15f), tile, false);
        AreaMicrocosmos.Box(roof.transform, "Tejado", new Vector3(half * 0.5f, ry + rise * 0.5f, 0f), new Vector3(len, 0.25f, 2f * hz + 2.2f), Quaternion.Euler(0f, 0f, -15f), tile, false);
        for (int k = 0; k < 5; k++)
        {
            float f = 1f - k / 5f, y = ry + rise * (k + 0.5f) / 5f;
            foreach (float z in new[] { -hz - 0.4f, hz + 0.4f }) AreaMicrocosmos.Box(roof.transform, "Fronton", new Vector3(0f, y, z), new Vector3(2f * half * f, rise / 5f, 0.4f), st, false);
        }
        return new[] { crepis, cols, cella, ent, roof };
    }

    // STOA: un pórtico de columnas con tejado a un agua, donde Shanidar enseña.
    static Transform Stoa(Transform t, Vector3 pos)
    {
        Transform s = Spot(t, "Stoa", pos);
        Material st = AreaMicrocosmos.Mat("Egina_Caliza", Stone), tile = AreaMicrocosmos.Mat("Egina_Teja", Tile);
        AreaMicrocosmos.Box(s, "Suelo", new Vector3(0f, 0.15f, 0f), new Vector3(11f, 0.3f, 4f), st, true);
        for (int i = 0; i < 6; i++) AreaMicrocosmos.Box(s, "Columna", new Vector3(-5f + i * 2f, 1.9f, -1.6f), new Vector3(0.45f, 3.2f, 0.45f), st, true);
        AreaMicrocosmos.Box(s, "Muro", new Vector3(0f, 1.9f, 1.8f), new Vector3(11f, 3.2f, 0.3f), st, true);
        AreaMicrocosmos.Box(s, "Tejado", new Vector3(0f, 3.7f, 0f), new Vector3(11.6f, 0.2f, 4.6f), Quaternion.Euler(-8f, 0f, 0f), tile, false);
        Label(s, "Stoa (aquí enseña Shanidar)", 4.6f);
        return s;
    }

    // Ropa griega sencilla: QUITÓN (túnica) con cinturón; Zeus, corona de hojas de encina y barba. (El cuerpo de cubos la
    // dibujará en detalle.)
    static void DressGreek(Transform p, float height, Color chiton, bool wreath, bool beard)
    {
        AreaMicrocosmos.Box(p, "Quiton", new Vector3(0f, height * 0.5f, 0f), new Vector3(0.5f, height * 0.42f, 0.38f), AreaMicrocosmos.Mat($"Egina_Quiton_{ColorUtility.ToHtmlStringRGB(chiton)}", chiton), false);
        AreaMicrocosmos.Box(p, "Cinturon", new Vector3(0f, height * 0.55f, 0f), new Vector3(0.52f, 0.05f, 0.4f), AreaMicrocosmos.Mat("Egina_Cinturon", new Color(0.55f, 0.4f, 0.2f)), false);
        if (wreath)
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI / 4f;
                AreaMicrocosmos.Box(p, "Corona_encina", new Vector3(Mathf.Cos(a) * 0.17f, height + 0.02f, Mathf.Sin(a) * 0.17f), new Vector3(0.09f, 0.05f, 0.09f), AreaMicrocosmos.Mat("Egina_Encina", new Color(0.25f, 0.42f, 0.2f)), false);
            }
        if (beard) AreaMicrocosmos.Box(p, "Barba", new Vector3(0f, height * 0.86f, 0.14f), new Vector3(0.2f, 0.18f, 0.08f), AreaMicrocosmos.Mat("Egina_Barba", new Color(0.92f, 0.92f, 0.9f)), false);
    }

    static Transform Spot(Transform parent, string name, Vector3 local)
    {
        Transform s = new GameObject(name).transform; s.SetParent(parent, false); s.localPosition = local; return s;
    }

    static void Label(Transform t, string text, float height)
    {
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = text; wl.height = height;
    }
}
