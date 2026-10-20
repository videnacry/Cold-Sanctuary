using UnityEngine;

/// <summary>
/// ÁREAS DE ENTRENAMIENTO (docs/jornada-y-rotacion.md): las 2 h diarias de entrenamiento son virtualizaciones de estas
/// áreas. Al empezar el juego solo están abiertos el YOGA y el GIMNASIO; la sala de BALLET, el DOJO de artes marciales y
/// el parque de CALISTENIA están EN OBRAS: cada uno tiene a pie de obra el material de un día de trabajo, andamio y
/// paredes a medio levantar (<see cref="ConstructionProject"/>), y el equipo de Construcción los va terminando.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color TRubber = new Color(0.18f, 0.18f, 0.2f), TIron = new Color(0.35f, 0.36f, 0.4f), TChrome = new Color(0.78f, 0.8f, 0.84f),
        TRed = new Color(0.75f, 0.2f, 0.2f), TWood = new Color(0.7f, 0.52f, 0.34f);

    /// <summary>GIMNASIO abierto: suelo de caucho, jaula de sentadillas, bancos, mancuernas, discos, cintas de correr,
    /// remo, poleas, sacos y espejos.</summary>
    /// <summary>GIMNASIO abierto: 10 tipos de máquina (una por grupo muscular) × 3 de cada = 30 máquinas, espejos en la pared
    /// del fondo, cuadros de gorilas en los laterales y una ZONA LIBRE (esterillas, kettlebells, barra de dominadas, sacos).</summary>
    static void FurnishGym(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Gimnasio").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        KitchenProps.C(r, new Vector3(0f, 0.01f, 0f), new Vector3(size.x - 0.4f, 0.02f, size.y - 0.4f), TRubber, 0f, "Suelo_caucho");
        // Sin espejos ni cuadros (no hay paredes): el gorila, símbolo de fuerza tranquila, está en ESTATUAS de cubos sobre
        // un pedestal, a los lados de la sala.
        Color fur = new Color(0.15f, 0.15f, 0.16f), skin = new Color(0.35f, 0.3f, 0.28f), stone = new Color(0.8f, 0.78f, 0.74f);
        for (int s = -1; s <= 1; s += 2)
            for (int i = 0; i < 3; i++)
            {
                Transform g = KitchenProps.Root(r, "Estatua_gorila", new Vector3(s * (hx - 0.8f), 0f, -hz + 3f + i * 4.5f), s * -90f);
                KitchenProps.C(g, new Vector3(0f, 0.25f, 0f), new Vector3(0.9f, 0.5f, 0.9f), stone, 0f, "Pedestal");
                KitchenProps.C(g, new Vector3(0f, 0.95f, 0f), new Vector3(0.7f, 0.9f, 0.55f), fur, 0f, "Gorila_cuerpo");
                KitchenProps.C(g, new Vector3(0f, 1.6f, -0.08f), new Vector3(0.36f, 0.36f, 0.34f), fur, 0f, "Gorila_cabeza");
                KitchenProps.C(g, new Vector3(0f, 1.56f, -0.26f), new Vector3(0.22f, 0.16f, 0.04f), skin, 0f, "Gorila_cara");
                for (int a = -1; a <= 1; a += 2) KitchenProps.C(g, new Vector3(a * 0.45f, 0.85f, -0.05f), new Vector3(0.2f, 0.8f, 0.2f), fur, a * 10f, "Gorila_brazo");
                WorldLabel gl = g.gameObject.AddComponent<WorldLabel>(); gl.text = "Gorila"; gl.height = 2.1f;
            }

        // 10 tipos de máquina (grupo muscular) × 3: filas de 6, en la mitad izquierda; la derecha queda LIBRE.
        string[] types = { "Press de pecho", "Press de hombros", "Jalón al pecho (espalda)", "Remo sentado", "Prensa de piernas",
                           "Extensión de cuádriceps", "Curl femoral", "Hip thrust (glúteos)", "Gemelos", "Curl de bíceps y tríceps" };
        int k = 0;
        for (int t = 0; t < types.Length; t++)
            for (int c = 0; c < 3; c++, k++)
            {
                int col = k % 6, row = k / 6;
                Vector3 p = new Vector3(-hx + 1.4f + col * 1.9f, 0f, -hz + 1.9f + row * 2.5f);
                GymMachine(r, $"{types[t]} {c + 1}", t, p);
            }

        // ZONA LIBRE: esterillas, kettlebells, mancuernas, barra de dominadas, sacos.
        float fx = hx - 4f;
        for (int i = 0; i < 4; i++) KitchenProps.C(r, new Vector3(fx - 1.5f + (i % 2) * 2.2f, 0.03f, -hz + 3f + (i / 2) * 3.2f), new Vector3(1f, 0.02f, 2f), new Color(0.3f, 0.45f, 0.4f), 0f, "Esterilla");
        Transform kb = KitchenProps.Root(r, "Kettlebells", new Vector3(fx + 2.4f, 0f, -hz + 1.2f));
        for (int i = 0; i < 6; i++) KitchenProps.C(kb, new Vector3(-1f + i * 0.4f, 0.12f + i * 0.01f, 0f), new Vector3(0.2f + i * 0.02f, 0.22f + i * 0.02f, 0.2f + i * 0.02f), Color.black, 0f, "Kettlebell");
        KitchenProps.Fit(kb); kb.gameObject.AddComponent<WorldLabel>().text = "Kettlebells";
        Transform dbs = KitchenProps.Root(r, "Mancuernas", new Vector3(fx, 0f, -hz + 0.9f));
        KitchenProps.C(dbs, new Vector3(0f, 0.4f, 0f), new Vector3(3f, 0.06f, 0.5f), TIron, 0f, "Estante");
        for (int i = 0; i < 10; i++) KitchenProps.C(dbs, new Vector3(-1.35f + i * 0.3f, 0.5f, 0f), new Vector3(0.08f + i * 0.008f, 0.12f + i * 0.008f, 0.3f), Color.black, 0f, "Mancuerna");
        KitchenProps.Fit(dbs); dbs.gameObject.AddComponent<WorldLabel>().text = "Mancuernas";
        Transform pull = KitchenProps.Root(r, "Barra_dominadas", new Vector3(fx, 0f, hz - 3f));
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(pull, new Vector3(s * 0.8f, 1.2f, 0f), new Vector3(0.08f, 2.4f, 0.08f), TIron, 0f, "Poste");
        KitchenProps.C(pull, new Vector3(0f, 2.3f, 0f), new Vector3(1.7f, 0.05f, 0.05f), TChrome, 0f, "Barra");
        KitchenProps.Fit(pull); pull.gameObject.AddComponent<WorldLabel>().text = "Barra de dominadas";
        for (int i = 0; i < 2; i++)
        {
            Transform bag = KitchenProps.Root(r, "Saco", new Vector3(fx + 2.6f, 0f, hz - 4.5f + i * 2f));
            KitchenProps.C(bag, new Vector3(0f, 1.1f, 0f), new Vector3(0.4f, 1.1f, 0.4f), TRed, 0f, "Saco_boxeo");
            KitchenProps.C(bag, new Vector3(0f, 2.2f, 0f), new Vector3(0.02f, 0.5f, 0.02f), TIron, 0f, "Cadena");
            KitchenProps.Fit(bag);
        }
        WorldLabel zl = KitchenProps.Root(r, "Zona_libre", new Vector3(fx, 0f, 0f)).gameObject.AddComponent<WorldLabel>(); zl.text = "Zona libre"; zl.height = 2.6f;
        area.gameObject.AddComponent<WorldLabel>().text = "Gimnasio (abierto)";
    }


    /// <summary>Una máquina de gimnasio: bastidor, asiento y respaldo, torre de placas con su cable y el agarre o la
    /// palanca según el músculo (empuje, tirón, pierna sentada o tumbada, glúteo, gemelo, brazo).</summary>
    static void GymMachine(Transform parent, string name, int type, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        Color frame = TIron, pad = new Color(0.12f, 0.12f, 0.14f), accent = type % 2 == 0 ? new Color(0.85f, 0.65f, 0.15f) : new Color(0.75f, 0.2f, 0.2f);
        KitchenProps.C(m, new Vector3(0f, 0.05f, 0f), new Vector3(1.1f, 0.1f, 1.5f), frame, 0f, "Base");
        KitchenProps.C(m, new Vector3(0f, 1.05f, 0.62f), new Vector3(0.36f, 2.1f, 0.2f), frame, 0f, "Torre");
        for (int i = 0; i < 10; i++) KitchenProps.C(m, new Vector3(0f, 0.25f + i * 0.07f, 0.62f), new Vector3(0.3f, 0.05f, 0.16f), Color.black, 0f, "Placa");
        KitchenProps.C(m, new Vector3(0f, 2.1f, 0.3f), new Vector3(0.06f, 0.06f, 0.7f), frame, 0f, "Brazo_polea");
        KitchenProps.C(m, new Vector3(0f, 1.7f, 0f), new Vector3(0.01f, 0.8f, 0.01f), new Color(0.2f, 0.2f, 0.22f), 0f, "Cable");
        bool lying = type == 6 || type == 7;   // curl femoral e hip thrust: tumbado
        if (lying) KitchenProps.C(m, new Vector3(0f, 0.5f, -0.1f), new Vector3(0.45f, 0.12f, 1.3f), pad, 0f, "Banco");
        else
        {
            KitchenProps.C(m, new Vector3(0f, 0.5f, 0f), new Vector3(0.45f, 0.1f, 0.45f), pad, 0f, "Asiento");
            KitchenProps.C(m, new Vector3(0f, 0.95f, 0.27f), new Vector3(0.45f, 0.8f, 0.1f), pad, 0f, "Respaldo");
        }
        switch (type)
        {
            case 0: case 1: case 9:   // empuje (pecho/hombros) y brazos: dos agarres delante
                for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.35f, type == 1 ? 1.4f : 1.05f, -0.25f), new Vector3(0.06f, 0.06f, 0.35f), accent, 0f, "Agarre");
                break;
            case 2: KitchenProps.C(m, new Vector3(0f, 1.9f, -0.15f), new Vector3(1.1f, 0.05f, 0.05f), accent, 0f, "Barra_jalon"); break;
            case 3: KitchenProps.C(m, new Vector3(0f, 0.85f, -0.55f), new Vector3(0.45f, 0.06f, 0.06f), accent, 0f, "Agarre_remo"); break;
            case 4: KitchenProps.C(m, new Vector3(0f, 0.9f, -0.6f), new Vector3(0.7f, 0.6f, 0.06f), accent, -20f, "Plataforma_pies"); break;
            case 5: case 6: KitchenProps.C(m, new Vector3(0f, 0.35f, -0.35f), new Vector3(0.6f, 0.12f, 0.12f), accent, 0f, "Rodillo"); break;
            case 7: KitchenProps.C(m, new Vector3(0f, 0.75f, -0.3f), new Vector3(0.7f, 0.08f, 0.2f), accent, 0f, "Barra_cadera"); break;
            case 8: KitchenProps.C(m, new Vector3(0f, 0.2f, -0.45f), new Vector3(0.6f, 0.08f, 0.3f), accent, 0f, "Escalon_gemelos"); break;
        }
        KitchenProps.Fit(m);
        WorldLabel wl = m.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = 2.4f;
    }
    /// <summary>OBRA: paredes a medio levantar, andamio y el material de UN día de trabajo a pie de obra.</summary>
    static void MakeConstructionSite(Transform area, Vector2 size, string title, int days)
    {
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        Transform scaf = KitchenProps.Root(area, "Andamio_obra", new Vector3(-hx + 1.4f, 0.1f, -hz + 1f));
        for (int i = 0; i < 4; i++) KitchenProps.C(scaf, new Vector3(i % 2 == 0 ? -0.9f : 0.9f, 1.5f, i < 2 ? -0.4f : 0.4f), new Vector3(0.05f, 3f, 0.05f), TChrome, 0f, "Tubo");
        for (int y = 0; y < 2; y++) KitchenProps.C(scaf, new Vector3(0f, 1.4f + y * 1.4f, 0f), new Vector3(1.9f, 0.05f, 0.85f), TWood, 0f, "Plataforma");
        Transform mat = KitchenProps.Root(area, "Material_del_dia", new Vector3(hx - 2f, 0.1f, 1f));
        KitchenProps.C(mat, new Vector3(-0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), TWood, 0f, "Pale");
        for (int y = 0; y < 4; y++) for (int i = 0; i < 4; i++)
            KitchenProps.C(mat, new Vector3(-1.08f + i * 0.25f, 0.18f + y * 0.1f, 0f), new Vector3(0.22f, 0.09f, 1f), new Color(0.62f, 0.5f, 0.38f), 0f, "Barro_con_paja");
        KitchenProps.C(mat, new Vector3(0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), TWood, 0f, "Pale");
        for (int y = 0; y < 3; y++) KitchenProps.C(mat, new Vector3(0.7f, 0.2f + y * 0.15f, 0f), new Vector3(0.95f, 0.14f, 0.45f), new Color(0.93f, 0.9f, 0.82f), 0f, "Panel_micelio");
        for (int i = 0; i < 5; i++) KitchenProps.C(mat, new Vector3(0f, 0.08f + i * 0.1f, 1.1f), new Vector3(2.8f, 0.09f, 0.09f), new Color(0.78f, 0.74f, 0.42f), 0f, "Guadua");
        WorldLabel ml = mat.gameObject.AddComponent<WorldLabel>(); ml.text = "Material de un día de obra: 4 guaduas, barro con paja, 2 paneles de micelio, cuerda"; ml.height = 1f;
        for (int i = 0; i < 3; i++) KitchenProps.C(area, new Vector3(-1f + i * 0.8f, 0.3f, hz - 0.6f), new Vector3(0.25f, 0.4f, 0.25f), new Color(0.95f, 0.45f, 0.15f), 45f, "Cono");
        ConstructionProject cp = area.gameObject.AddComponent<ConstructionProject>();
        cp.title = title; cp.daysRequired = days;
        WorldLabel wl = area.gameObject.AddComponent<WorldLabel>(); wl.text = $"{title} — EN OBRAS (0/{days} días)"; wl.height = 4.5f;
    }
}
