using UnityEngine;

/// <summary>
/// LA RESIDENCIA, UN EDIFICIO-ÁRBOL NUEVO EN LA ISLA (Berón 2026-11-05: «la residencia se puede pasar a un arbol-edificio y las casas arbol quedan para otro santuary»; docs/residencia.md)
/// y EL LABORATORIO DE CONTENCIÓN del árbol cazador, en lo alto de Producción (docs/contencion-arbol-cazador.md).
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Lóbulo Isla:Cabo: lejos de la cueva de los lobos y de la de la osa, y a ≥ 16 m de Producción y del Árbol de los Campos.</summary>
    public static readonly Vector3 ResidenciaCenter = new Vector3(-66f, 0f, -212f);
    public static readonly Vector2 ResidenciaSize = new Vector2(40f, 38f);
    public const float ResidenciaYaw = 0f;

    static void BuildResidenceTower(Transform areasGroup)
    {
        int roomFloors = ResidentialBuilder.RoomFloors;
        int levels = Mathf.CeilToInt((roomFloors + 1) / 2f);
        Helix hx = BeginTreeTower(areasGroup, ResidentialBuilder.Name, ResidenciaCenter, ResidenciaSize, ResidenciaYaw, levels, 0f, 4f);
        hx.santuarioLabel = $"{ResidentialBuilder.Name}: habitaciones, plaza y ring de almohadas";
        hx.floorTitle = k => "Residencia";
        Transform plaza = new GameObject(ResidentialBuilder.PlazaName).transform; plaza.SetParent(areasGroup, false);
        Vector2 s = TowerFloor(plaza);
        ResidentialBuilder.FurnishPlaza(plaza, s);
        Aseos(plaza, new Vector3(s.x * 0.5f - 2.1f, 0.1f, -s.y * 0.5f + 5.5f), 0f, 1);
        for (int f = 0; f < roomFloors; f++)
        {
            Transform floor = new GameObject($"Planta_habitaciones_{f + 1}").transform; floor.SetParent(areasGroup, false);
            s = TowerFloor(floor);
            ResidentialBuilder.FurnishRoomFloor(floor, s, f);
            Aseos(floor, new Vector3(-s.x * 0.5f + 2.1f, 0.1f, s.y * 0.5f - 0.9f), 180f, 1);
        }
        Debug.Log($"[Residencia] Edificio-árbol: {ResidentialBuilder.RoomCount} habitaciones en {roomFloors} pisos y la plaza; {levels} niveles.");
    }

    /// <summary>El piso más alto que quede libre (el de abajo sigue siendo el garaje del submarino).</summary>
    static Vector2 TowerFloorTop(Transform floor)
    {
        for (int k = _towerHelix.Platforms; k >= 1; k--)
        {
            if (TreeSpec.Platform(k).tip || _towerHelix.decks[k] != null) continue;
            HelixFloor(_towerHelix, k, floor, _towerSize);
            return _towerSize;
        }
        Debug.LogError($"[Torres] «{_towerHelix.building}» no tiene piso libre para «{floor.name}».");
        return _towerSize;
    }

    static void BuildContainmentLab(Transform areasGroup)
    {
        Transform floor = new GameObject("Laboratorio_contencion").transform; floor.SetParent(areasGroup, false);
        Vector2 s = TowerFloorTop(floor);
        Transform g = new GameObject("Planta_contencion").transform; g.SetParent(floor, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.x - 0.2f), new Color(0.74f, 0.76f, 0.78f), "Suelo_epoxi");
        float hx = s.x * 0.5f;
        const float H = 21f;
        Vector3 c = new Vector3(0f, 0f, 4f), size = new Vector3(18f, H, 16f);

        // LA CELDA: cristal por S6 (excepción de sala que debe aislarse), de 21 m para que el árbol crezca entero. Puerta doble, cerrada: nadie entra.
        Transform cell = new GameObject("Celda_arbol_cazador").transform; cell.SetParent(g, false); cell.localPosition = c;
        void Pane(string n, Vector3 at, Vector3 sz)
        {
            Transform p = KitchenProps.C(cell, at, sz, GlassWall.GlassTint, 0f, GlassWall.Prefix + "_celda_" + n);
            p.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Pane("norte", new Vector3(0f, H * 0.5f, size.z * 0.5f), new Vector3(size.x, H, GlassWall.Thick));
        Pane("sur", new Vector3(0f, H * 0.5f, -size.z * 0.5f), new Vector3(size.x, H, GlassWall.Thick));
        Pane("este", new Vector3(size.x * 0.5f, H * 0.5f, 0f), new Vector3(GlassWall.Thick, H, size.z));
        Pane("oeste", new Vector3(-size.x * 0.5f, H * 0.5f, 0f), new Vector3(GlassWall.Thick, H, size.z));
        Pane("tapa", new Vector3(0f, H, 0f), new Vector3(size.x, GlassWall.Thick, size.z));
        KitchenProps.C(cell, new Vector3(0f, 0.2f, 0f), new Vector3(9f, 0.4f, 9f), new Color(0.3f, 0.32f, 0.36f), 0f, "Bandeja_de_efluentes");

        // EL ÁRBOL, con todo lo suyo (señuelo, sopor, cualquier presa y crecer): los radios y el tope, los de CubeTree. Sin panales: un enjambre no cabe en una contención.
        Vector3 worldCell = cell.position;
        System.Func<Vector3, bool> outside = w =>
        {
            Vector3 l = cell.InverseTransformPoint(w);
            return Mathf.Abs(l.x) > size.x * 0.33f || Mathf.Abs(l.z) > size.z * 0.33f;
        };
        GameObject tree = CubeTree.Build(g, "ArbolCazador", worldCell + Vector3.up * 0.3f, 1234, 10f, carnivorous: true, blocked: outside, maxNests: 0);
        NestTree nt = tree.GetComponent<NestTree>(); if (nt != null) Object.DestroyImmediate(nt);
        WorldLabel tl = tree.AddComponent<WorldLabel>(); tl.text = "Árbol cazador (contenido)"; tl.height = 16f;

        // LA ANTESALA, fuera de la celda: dos tramos, con el paso a la puerta en medio.
        Transform west = new GameObject("Antesala_oeste").transform; west.SetParent(g, false); west.gameObject.AddComponent<FurnitureLine>();
        Transform east = new GameObject("Antesala_este").transform; east.SetParent(g, false); east.gameObject.AddComponent<FurnitureLine>();
        Transform Piece(Transform line, string name, float x, Vector3 box, Color col, string label)
        {
            Transform t = KitchenProps.Root(line, name, new Vector3(x, 0f, -7f));
            KitchenProps.C(t, new Vector3(0f, box.y * 0.5f, 0f), box, col, 0f, "Cuerpo");
            KitchenProps.Fit(t);
            WorldLabel w = t.gameObject.AddComponent<WorldLabel>(); w.text = label; w.height = box.y + 0.7f;
            return t;
        }
        Color steel = new Color(0.6f, 0.63f, 0.66f), panel = new Color(0.2f, 0.26f, 0.3f);
        Transform console = Piece(west, "Consola_contencion", -8f, new Vector3(3.6f, 1.1f, 0.9f), panel, "Consola: presión negativa y sellos");
        AreaSeat.Add(console.gameObject, "contencion", AreaSeat.Category.Trabajo, 4);
        Piece(west, "Filtro_HEPA", -4.6f, new Vector3(1.6f, 2.4f, 0.9f), steel, "Filtros del aire");
        Piece(east, "Portillo_de_raciones", 4.2f, new Vector3(1.2f, 1.4f, 0.8f), steel, "Portillo de raciones");
        Piece(east, "Guantes_de_muestra", 6.2f, new Vector3(1.4f, 1.6f, 0.8f), panel, "Guantes: muestras y poda");
        Piece(east, "Ducha_descontaminacion", 8.2f, new Vector3(1.2f, 2.3f, 0.9f), steel, "Ducha de descontaminación");
        Piece(east, "Armario_EPI", 10f, new Vector3(1.2f, 2f, 0.7f), new Color(0.9f, 0.7f, 0.15f), "Equipo de protección");

        Transform post = new GameObject("Puesto_contencion").transform; post.SetParent(floor, false); post.localPosition = new Vector3(0f, 0.1f, -9f);
        HunterCell hc = floor.gameObject.AddComponent<HunterCell>();
        hc.cell = new Bounds(new Vector3(c.x, H * 0.5f, c.z) + Vector3.up * 0.1f, size);
        hc.tree = tree.GetComponent<IceCarnivorousTree>();
        hc.post = post;
        floor.gameObject.AddComponent<WorldLabel>().text = "Laboratorio de contención: el árbol cazador";
        Aseos(floor, new Vector3(hx - 0.9f, 0.1f, 8f), -90f, 1);
    }
}
