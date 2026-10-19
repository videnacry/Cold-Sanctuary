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
    static void FurnishGym(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Gimnasio").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        KitchenProps.C(r, new Vector3(0f, 0.01f, 0f), new Vector3(size.x - 0.4f, 0.02f, size.y - 0.4f), TRubber, 0f, "Suelo_caucho");
        KitchenProps.C(r, new Vector3(0f, 1.2f, -hz + 0.25f), new Vector3(size.x - 1f, 1.8f, 0.03f), new Color(0.75f, 0.85f, 0.9f), 0f, "Espejo");
        for (int k = 0; k < 2; k++)   // jaulas de sentadillas con su barra
        {
            Transform rack = KitchenProps.Root(r, $"Jaula_{k + 1}", new Vector3(-hx + 2f + k * 2.6f, 0f, -hz + 1.8f));
            for (int i = 0; i < 4; i++) KitchenProps.C(rack, new Vector3(i % 2 == 0 ? -0.6f : 0.6f, 1.1f, i < 2 ? -0.5f : 0.5f), new Vector3(0.07f, 2.2f, 0.07f), TIron, 0f, "Poste");
            KitchenProps.C(rack, new Vector3(0f, 1.4f, -0.5f), new Vector3(2f, 0.04f, 0.04f), TChrome, 0f, "Barra");
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(rack, new Vector3(s * 0.85f, 1.4f, -0.5f), new Vector3(0.06f, 0.42f, 0.42f), Color.black, 0f, "Disco");
            KitchenProps.Fit(rack); rack.gameObject.AddComponent<WorldLabel>().text = "Jaula de sentadillas";
        }
        for (int k = 0; k < 2; k++)
        {
            Transform bench = KitchenProps.Root(r, $"Banco_{k + 1}", new Vector3(hx - 2.2f, 0f, -hz + 1.6f + k * 2f), 90f);
            KitchenProps.C(bench, new Vector3(0f, 0.45f, 0f), new Vector3(0.3f, 0.08f, 1.2f), TRed, 0f, "Tapizado");
            KitchenProps.C(bench, new Vector3(0f, 0.22f, 0f), new Vector3(0.1f, 0.44f, 1f), TIron, 0f, "Pata");
            KitchenProps.Fit(bench);
        }
        Transform dbs = KitchenProps.Root(r, "Mancuernas", new Vector3(0f, 0f, -hz + 0.8f));
        KitchenProps.C(dbs, new Vector3(0f, 0.4f, 0f), new Vector3(3f, 0.06f, 0.5f), TIron, 0f, "Estante");
        for (int i = 0; i < 10; i++) KitchenProps.C(dbs, new Vector3(-1.35f + i * 0.3f, 0.5f, 0f), new Vector3(0.08f + i * 0.008f, 0.12f + i * 0.008f, 0.3f), Color.black, 0f, "Mancuerna");
        KitchenProps.Fit(dbs); dbs.gameObject.AddComponent<WorldLabel>().text = "Mancuernas";
        for (int k = 0; k < 3; k++)
        {
            Transform tm = KitchenProps.Root(r, $"Cinta_{k + 1}", new Vector3(-hx + 1.6f + k * 1.4f, 0f, hz - 2.4f));
            KitchenProps.C(tm, new Vector3(0f, 0.15f, 0f), new Vector3(0.8f, 0.2f, 1.8f), TRubber, 0f, "Banda");
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(tm, new Vector3(s * 0.38f, 0.75f, -0.75f), new Vector3(0.05f, 1.1f, 0.05f), TIron, 0f, "Barra");
            KitchenProps.C(tm, new Vector3(0f, 1.3f, -0.75f), new Vector3(0.8f, 0.3f, 0.1f), new Color(0.2f, 0.3f, 0.45f), 0f, "Consola");
            KitchenProps.Fit(tm);
        }
        Transform row = KitchenProps.Root(r, "Remo", new Vector3(hx - 2.2f, 0f, hz - 2.4f), 90f);
        KitchenProps.C(row, new Vector3(0f, 0.2f, 0f), new Vector3(0.3f, 0.12f, 2.2f), TIron, 0f, "Rail");
        KitchenProps.C(row, new Vector3(0f, 0.45f, -1f), new Vector3(0.5f, 0.5f, 0.2f), new Color(0.2f, 0.3f, 0.5f), 0f, "Volante");
        KitchenProps.Fit(row);
        Transform bag = KitchenProps.Root(r, "Saco", new Vector3(hx - 1f, 0f, 0f));
        KitchenProps.C(bag, new Vector3(0f, 1.1f, 0f), new Vector3(0.4f, 1.1f, 0.4f), TRed, 0f, "Saco_boxeo");
        KitchenProps.C(bag, new Vector3(0f, 2.2f, 0f), new Vector3(0.02f, 0.5f, 0.02f), TIron, 0f, "Cadena");
        KitchenProps.Fit(bag);
        area.gameObject.AddComponent<WorldLabel>().text = "Gimnasio (abierto)";
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
