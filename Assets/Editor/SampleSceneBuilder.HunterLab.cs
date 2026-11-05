using UnityEngine;

/// <summary>
/// EL LABORATORIO DE CONTENCIÓN, EN EL SANTUARIO MARINO (Berón 2026-11-05: «quizás el piso del árbol carnivoro se podría realizar bajo el agua? el enjambre no podría salir de alli»;
/// docs/contencion-arbol-cazador.md). Una sala propia en la roca, unida a la galería, a presión de una atmósfera. De dentro afuera: la celda de cristal, la antesala, la esclusa de aire,
/// la cámara de salida inundable y el pozo inundado al lecho. Un enjambre no pasa de la cámara: hay agua.
/// </summary>
public static partial class SampleSceneBuilder
{
    const float LabHeight = 23f;

    static void BuildHunterLab(Transform root, Vector3 foot, float westLimit)
    {
        Vector3 size = new Vector3(34f, LabHeight, 30f);
        Vector3 at = new Vector3(westLimit - 46f, foot.y, foot.z);
        Color conc = new Color(0.6f, 0.6f, 0.58f), steel = TreeSteel, panel = new Color(0.2f, 0.26f, 0.3f), yel = new Color(0.95f, 0.8f, 0.2f);
        Transform lab = new GameObject("Laboratorio_contencion").transform; lab.SetParent(root, false); lab.position = at;
        HunterLab view = lab.gameObject.AddComponent<HunterLab>();
        Hull(KitchenProps.Root(lab, "Caja_laboratorio", new Vector3(0f, LabHeight * 0.5f, 0f)), size, 0.4f, conc, "Revestimiento_laboratorio");
        Solid(lab, new Vector3(0f, -0.25f, 0f), new Vector3(size.x, 0.5f, size.z), new Color(0.74f, 0.76f, 0.78f), "Suelo_epoxi");

        // La galería, desde el límite oeste de las salas, y los límites de la sala (con su hueco).
        float galleryLen = westLimit - (at.x + size.x * 0.5f);
        Vector3 galleryAt = new Vector3(at.x + size.x * 0.5f + galleryLen * 0.5f, foot.y, foot.z);
        Solid(root, galleryAt + Vector3.down * 0.25f, new Vector3(galleryLen, 0.5f, 4f), new Color(0.32f, 0.33f, 0.34f), "Suelo_galeria_contencion");
        Hull(KitchenProps.Root(lab, "Galeria_contencion", galleryAt - at + Vector3.up * 1.6f), new Vector3(galleryLen, 3.2f, 4f), 0.2f, conc, "Revestimiento_galeria_contencion");
        void Limit(Vector3 c, Vector3 s) { BoxCollider w = new GameObject("Limite_marino").AddComponent<BoxCollider>(); w.transform.SetParent(root, false); w.transform.position = c; w.size = s; }
        for (int s = -1; s <= 1; s += 2)
        {
            Limit(galleryAt + new Vector3(0f, 3f, s * 2.5f), new Vector3(galleryLen, 6f, 1f));
            Limit(at + new Vector3(0f, 3f, s * (size.z * 0.5f + 0.5f)), new Vector3(size.x, 6f, 1f));
            Limit(at + new Vector3(size.x * 0.5f + 0.5f, 3f, s * (size.z * 0.25f + 1f)), new Vector3(1f, 6f, size.z * 0.5f - 2f));
        }
        Limit(at + new Vector3(-size.x * 0.5f - 0.5f, 3f, 0f), new Vector3(1f, 6f, size.z));

        // LA CELDA: cristal por S6 (sala que debe aislarse), con su tapa. Nadie entra.
        Vector3 c0 = new Vector3(-6f, 0f, 0f), box = new Vector3(18f, 21f, 16f);
        Transform cell = new GameObject("Celda_arbol_cazador").transform; cell.SetParent(lab, false); cell.localPosition = c0;
        void Pane(string n, Vector3 p, Vector3 s)
        {
            Transform t = KitchenProps.C(cell, p, s, GlassWall.GlassTint, 0f, GlassWall.Prefix + "_celda_" + n);
            t.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Pane("norte", new Vector3(0f, box.y * 0.5f, box.z * 0.5f), new Vector3(box.x, box.y, GlassWall.Thick));
        Pane("sur", new Vector3(0f, box.y * 0.5f, -box.z * 0.5f), new Vector3(box.x, box.y, GlassWall.Thick));
        Pane("este", new Vector3(box.x * 0.5f, box.y * 0.5f, 0f), new Vector3(GlassWall.Thick, box.y, box.z));
        Pane("oeste", new Vector3(-box.x * 0.5f, box.y * 0.5f, 0f), new Vector3(GlassWall.Thick, box.y, box.z));
        Pane("tapa", new Vector3(0f, box.y, 0f), new Vector3(box.x, GlassWall.Thick, box.z));
        KitchenProps.C(cell, new Vector3(0f, 0.2f, 0f), new Vector3(9f, 0.4f, 9f), new Color(0.3f, 0.32f, 0.36f), 0f, "Suelo_de_cultivo");
        for (int i = -1; i <= 1; i++) KitchenProps.C(cell, new Vector3(i * 5f, box.y - 0.4f, 0f), new Vector3(3f, 0.3f, 12f), new Color(1f, 0.92f, 0.7f), 0f, "Luz_de_cultivo");

        // EL ÁRBOL, con todo lo suyo: su alcance entero, sus panales y sus arañas.
        System.Func<Vector3, bool> outside = w => { Vector3 l = cell.InverseTransformPoint(w); return Mathf.Abs(l.x) > box.x * 0.33f || Mathf.Abs(l.z) > box.z * 0.33f; };
        GameObject tree = CubeTree.Build(lab, "ArbolCazador", cell.position + Vector3.up * 0.4f, 1234, 10f, carnivorous: true, blocked: outside, maxNests: HunterRule.Hives);
        IceCarnivorousTree hunter = tree.GetComponent<IceCarnivorousTree>();
        hunter.lureRadius = HunterRule.LureRadius; hunter.killRadius = HunterRule.KillRadius;
        view.tree = hunter;
        view.cell = new Bounds(c0 + Vector3.up * box.y * 0.5f, box);
        WorldLabel tl = tree.AddComponent<WorldLabel>(); tl.text = "Árbol cazador, con sus abejas y arañas"; tl.height = 16f;
        Vector3 nest = cell.position + new Vector3(3f, 0f, 0f);
        GameObject bee = LoadAnimalPrefab("SnowBee"), spider = LoadAnimalPrefab("SnowSpider");
        if (bee != null)
        {
            GameObject bg = new GameObject("Abejas_celda"); bg.transform.SetParent(lab, false);
            bg.AddComponent<FamilyGenerator>().families = new[] { new FamilyGenerator.Family { animalPrefab = bee, position = nest, quantity = 5, minParentsCount = 1, radius = 3f } };
        }
        if (spider != null)
        {
            GameObject sg = new GameObject("Aranas_celda"); sg.transform.SetParent(lab, false);
            sg.AddComponent<FamilyGenerator>().families = new[] { new FamilyGenerator.Family { animalPrefab = spider, position = nest + new Vector3(-2f, 0f, 1f), quantity = HunterRule.Spiders, minParentsCount = 1, radius = 2f } };
        }

        // LA ANTESALA, al este. Todo el trabajo se hace desde aquí.
        Transform Piece(string name, Vector3 p, Vector3 s, Color col, string label)
        {
            Transform t = KitchenProps.Root(lab, name, p);
            KitchenProps.C(t, new Vector3(0f, s.y * 0.5f, 0f), s, col, 0f, "Cuerpo");
            KitchenProps.Fit(t);
            WorldLabel w = t.gameObject.AddComponent<WorldLabel>(); w.text = label; w.height = s.y + 0.7f;
            return t;
        }
        Piece("Consola_contencion", new Vector3(13f, 0f, 11f), new Vector3(0.9f, 1.1f, 3.6f), panel, "Consola: sellos y colonia");
        Piece("Portillo_de_raciones", new Vector3(5f, 0f, 4f), new Vector3(1f, 1.4f, 1.2f), steel, "Portillo de raciones");
        Piece("Guantes_de_muestra", new Vector3(5f, 0f, -4f), new Vector3(1f, 1.6f, 1.4f), panel, "Guantes: muestras y poda");
        Piece("Filtro_HEPA", new Vector3(13f, 0f, 6.5f), new Vector3(0.9f, 2.4f, 1.6f), steel, "Filtros del aire");
        Piece("Ducha_descontaminacion", new Vector3(13f, 0f, -6.5f), new Vector3(0.9f, 2.3f, 1.2f), steel, "Ducha de descontaminación");
        Piece("Armario_EPI", new Vector3(13f, 0f, -11f), new Vector3(0.7f, 2f, 1.2f), yel, "Equipo de protección");

        // HACIA EL MAR: esclusa de aire, cámara de salida inundable y pozo inundado hasta el lecho. Es a 57 bar: no cabe un moon pool abierto a una atmósfera.
        Transform sea = KitchenProps.Root(lab, "Esclusa_al_mar", new Vector3(8f, 0f, -12f));
        KitchenProps.C(sea, new Vector3(0f, 1.3f, 2f), new Vector3(3f, 2.6f, 0.3f), steel, 0f, "Esclusa_de_aire");
        KitchenProps.C(sea, new Vector3(0f, 1.3f, 0f), new Vector3(3f, 2.6f, 3.4f), new Color(0.2f, 0.45f, 0.6f), 0f, "Camara_de_salida_inundada");
        KitchenProps.C(sea, new Vector3(0f, 2.9f, 0f), new Vector3(1.6f, 0.2f, 1.6f), yel, 0f, "Escotilla_con_cierre_magnetico");
        float rise = SeaRule.MarineFloorY - at.y;
        KitchenProps.C(sea, new Vector3(0f, 3f + (rise - 3f) * 0.5f, 0f), new Vector3(1.4f, rise - 3f, 1.4f), new Color(0.2f, 0.45f, 0.6f), 0f, "Pozo_inundado_al_mar");
        KitchenProps.Fit(sea);
        WorldLabel sw = sea.gameObject.AddComponent<WorldLabel>(); sw.text = "Salida al mar: un enjambre no cruza el agua"; sw.height = 3.6f;

        Transform sun = new GameObject("Luz_laboratorio").transform; sun.SetParent(lab, false); sun.localPosition = new Vector3(0f, LabHeight - 2f, 0f);
        Light l2 = sun.gameObject.AddComponent<Light>(); l2.type = LightType.Point; l2.range = 60f; l2.intensity = 2f;
        Label(lab, "Laboratorio de contención: el árbol cazador", 5f);
    }
}
