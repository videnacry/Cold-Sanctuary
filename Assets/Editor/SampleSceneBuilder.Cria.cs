using UnityEditor;
using UnityEngine;

/// <summary>
/// ÁREA DE CRÍA (docs/cria-simulation.md): el corazón del santuario. Una sala de crías del HIELO, como un centro de
/// rescate: recintos con CRÍAS DE VERDAD (`Animal` en etapa de cría, no cápsulas) de las especies del Santuario 1
/// —oseznos polares en su guarida cálida, focas con su piscina, la guardería de pingüinos y los cachorros de
/// malamute—, la cocina de leche (biberones, báscula, nevera), el almacén (biberón / comida / paja) y un pasillo ancho.
/// Cada recinto tiene sus PUESTOS de cuidado (leer estado → calmar → alimentar → asear → arrullar) que tocan los
/// drives reales de sus crías (<see cref="CriaCareTarget"/>: el bond SE GANA), y sus TOMAS programadas
/// (<see cref="CriaFeeder"/>) para que no tengan que cazar ni pescar dentro.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color CrStraw = new Color(0.86f, 0.77f, 0.5f), CrWall = new Color(0.78f, 0.84f, 0.86f), CrWood = new Color(0.62f, 0.5f, 0.38f),
        CrWater = new Color(0.35f, 0.6f, 0.78f, 0.8f), CrSnow = new Color(0.93f, 0.95f, 0.97f);

    static void FurnishCria(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Cria").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;

        // ── RECINTOS (fondo y lado oeste), cada uno con la puerta al pasillo ──
        Transform bear = CriaPen(r, "Recinto_oseznos", new Vector3(-8.5f, 0f, -6.5f), new Vector2(7f, 6f), CrStraw,
            "Oseznos polares", "nacen en invierno en una guarida de nieve, ciegos y de ~0,6 kg");
        Transform den = KitchenProps.Root(bear, "Guarida_calida", new Vector3(-1.8f, 0f, -1.6f));   // caja térmica con entrada
        KitchenProps.C(den, new Vector3(0f, 0.6f, -0.5f), new Vector3(2.4f, 1.2f, 0.1f), CrSnow, 0f, "Pared_guarida");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(den, new Vector3(s * 1.15f, 0.6f, 0f), new Vector3(0.1f, 1.2f, 1.1f), CrSnow, 0f, "Pared_guarida");
        KitchenProps.C(den, new Vector3(0f, 1.25f, 0f), new Vector3(2.4f, 0.1f, 1.1f), CrSnow, 0f, "Techo_guarida");
        KitchenProps.C(den, new Vector3(0f, 0.03f, 0f), new Vector3(2.2f, 0.06f, 1f), new Color(0.95f, 0.55f, 0.4f), 0f, "Manta_termica");
        CriaCubs(bear, "Bear", "Osezno", 2, "papilla de pescado y leche");

        Transform seal = CriaPen(r, "Recinto_focas", new Vector3(0f, 0f, -6.5f), new Vector2(7f, 6f), CrSnow,
            "Crías de foca", "aprenden a nadar y a pescar en la piscina antes de volver al mar");
        KitchenProps.C(seal, new Vector3(0f, 0.05f, -0.6f), new Vector3(5f, 0.1f, 3.2f), CrWood, 0f, "Borde_piscina");
        KitchenProps.C(seal, new Vector3(0f, 0.11f, -0.6f), new Vector3(4.6f, 0.02f, 2.8f), CrWater, 0f, "Agua_piscina");
        CriaCubs(seal, "Seal", "Cria_foca", 2, "papilla de pescado");

        Transform peng = CriaPen(r, "Recinto_pinguinos", new Vector3(8.5f, 0f, -6.5f), new Vector2(7f, 6f), CrSnow,
            "Guardería de pingüinos", "los polluelos se juntan en grupo para darse calor");
        for (int k = 0; k < 4; k++) KitchenProps.C(peng, new Vector3(-2.2f + k * 1.4f, 0.15f, -1.8f), new Vector3(1f, 0.3f + (k % 2) * 0.2f, 1f), CrSnow, k * 20f, "Roca_nieve");
        CriaCubs(peng, "Penguin", "Polluelo", 3, "pescado triturado");

        Transform dogs = CriaPen(r, "Recinto_cachorros", new Vector3(-9.5f, 0f, 4.5f), new Vector2(6f, 6f), CrStraw,
            "Cachorros de malamute", "perros de trineo; maman unas ocho semanas", doorEast: true);
        Transform bed = KitchenProps.Root(dogs, "Cama_cachorros", new Vector3(-1.4f, 0f, -1.4f));
        KitchenProps.C(bed, new Vector3(0f, 0.12f, 0f), new Vector3(1.6f, 0.24f, 1.2f), new Color(0.55f, 0.35f, 0.3f), 0f, "Cama");
        KitchenProps.C(bed, new Vector3(0f, 0.26f, 0f), new Vector3(1.4f, 0.06f, 1f), new Color(0.9f, 0.85f, 0.75f), 0f, "Manta");
        CriaCubs(dogs, "Malamute", "Cachorro", 3, "leche de cachorro", doorEast: true);

        // ── COCINA DE LECHE (lado este): mesa con biberones, báscula de crías, nevera de leche y fregadero ──
        Transform milk = KitchenProps.Root(r, "Cocina_de_leche", new Vector3(hx - 3.2f, 0f, 3.5f), -90f);
        milk.gameObject.AddComponent<FurnitureLine>();   // nevera, mesa de biberones y fregadero: un tramo corrido (van pegados)
        Transform bench = KitchenProps.CounterRun(milk, "Mesa_biberones", new Vector3(0f, 0f, 0f), 3.2f);
        for (int i = 0; i < 6; i++)
        {
            KitchenProps.C(bench, new Vector3(-1.2f + i * 0.45f, KitchenProps.CounterTop + 0.12f, 0f), new Vector3(0.09f, 0.22f, 0.09f), new Color(0.95f, 0.95f, 0.9f), 0f, "Biberon");
            KitchenProps.C(bench, new Vector3(-1.2f + i * 0.45f, KitchenProps.CounterTop + 0.26f, 0f), new Vector3(0.05f, 0.06f, 0.05f), new Color(0.9f, 0.7f, 0.4f), 0f, "Tetina");
        }
        Transform scale = KitchenProps.C(bench, new Vector3(1.3f, KitchenProps.CounterTop + 0.04f, 0f), new Vector3(0.45f, 0.06f, 0.35f), KitchenProps.Steel, 0f, "Bascula_crias");
        scale.gameObject.AddComponent<WorldLabel>().text = "Báscula (pesar cada día)";
        KitchenProps.Fridge(milk, "Nevera_leche", new Vector3(-2.3f, 0f, 0f));
        KitchenProps.Sink(milk, "Fregadero", new Vector3(2.4f, 0f, 0f));
        milk.gameObject.AddComponent<WorldLabel>().text = "Cocina de leche";
        milk.GetComponent<WorldLabel>().height = 2.2f;

        // ── ALMACÉN (biberón / comida / paja), junto a la cocina de leche ──
        Transform store = KitchenProps.Shelving(r, "Almacen_cria", new Vector3(hx - 0.6f, 0f, -1.4f), -90f, 31);
        store.gameObject.AddComponent<WorldLabel>().text = "Almacén: biberones, comida y paja";

        // Pizarra de tomas (horarios y pesos: en un centro de rescate todo se anota).
        Transform board = KitchenProps.Root(r, "Pizarra_tomas", new Vector3(3.5f, 0f, -2.9f));
        KitchenProps.C(board, new Vector3(0f, 1.3f, 0f), new Vector3(2.4f, 1.1f, 0.05f), new Color(0.2f, 0.3f, 0.25f), 0f, "Pizarra");
        for (int i = 0; i < 4; i++) KitchenProps.C(board, new Vector3(0f, 1.6f - i * 0.22f, -0.03f), new Vector3(2f, 0.03f, 0.01f), Color.white, 0f, "Linea_tiza");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(board, new Vector3(s * 1.1f, 0.6f, 0f), new Vector3(0.06f, 1.2f, 0.06f), CrWood, 0f, "Pata");
        board.gameObject.AddComponent<WorldLabel>().text = "Tomas y pesos";

        // Aseos del área (al oeste del pasillo de entrada).
        Aseos(r, new Vector3(-4.2f, 0f, hz - 0.9f), 180f, 1);
    }

    /// <summary>Un recinto de crías: suelo (paja o nieve), muro de 1,2 m con su puerta hacia el pasillo (+Z, o +X si
    /// <paramref name="doorEast"/>) y su cartel. Los muros tienen collider: las crías no salen ni se ven unas a otras como presa.</summary>
    static Transform CriaPen(Transform parent, string name, Vector3 center, Vector2 size, Color floor, string title, string fact, bool doorEast = false)
    {
        Transform p = KitchenProps.Root(parent, name, center);
        float hx = size.x * 0.5f, hz = size.y * 0.5f, h = 1.2f, t = 0.12f, door = 1.4f;
        KitchenProps.C(p, new Vector3(0f, 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), floor, 0f, "Suelo_recinto");
        Solid(p, new Vector3(0f, h * 0.5f, -hz), new Vector3(size.x, h, t), CrWall, "Muro");
        if (doorEast)
        {
            Solid(p, new Vector3(0f, h * 0.5f, hz), new Vector3(size.x, h, t), CrWall, "Muro");
            Solid(p, new Vector3(-hx, h * 0.5f, 0f), new Vector3(t, h, size.y), CrWall, "Muro");
            float seg = (size.y - door) * 0.5f;
            Solid(p, new Vector3(hx, h * 0.5f, -hz + seg * 0.5f), new Vector3(t, h, seg), CrWall, "Muro");
            Solid(p, new Vector3(hx, h * 0.5f, hz - seg * 0.5f), new Vector3(t, h, seg), CrWall, "Muro");
        }
        else
        {
            Solid(p, new Vector3(-hx, h * 0.5f, 0f), new Vector3(t, h, size.y), CrWall, "Muro");
            Solid(p, new Vector3(hx, h * 0.5f, 0f), new Vector3(t, h, size.y), CrWall, "Muro");
            float seg = (size.x - door) * 0.5f;
            Solid(p, new Vector3(-hx + seg * 0.5f, h * 0.5f, hz), new Vector3(seg, h, t), CrWall, "Muro");
            Solid(p, new Vector3(hx - seg * 0.5f, h * 0.5f, hz), new Vector3(seg, h, t), CrWall, "Muro");
        }
        WorldLabel wl = p.gameObject.AddComponent<WorldLabel>(); wl.text = $"{title}\n{fact}"; wl.height = 2.2f;
        return p;
    }

    /// <summary>Pone <paramref name="count"/> crías DE VERDAD de <paramref name="species"/> (el prefab de la especie, en
    /// etapa de cría) en el recinto, con su cuidado (puestos + <see cref="CriaCareTarget"/>) y sus tomas.</summary>
    static void CriaCubs(Transform pen, string species, string baseName, int count, string food, bool doorEast = false)
    {
        string station = "Cria_" + species;
        GameObject prefab = LoadAnimalPrefab(species);
        if (prefab == null) Debug.LogWarning($"[Cría] sin prefab {species} (genera los prefabs de animales): el recinto queda sin crías.");
        else
            for (int i = 0; i < count; i++)
            {
                GameObject cub = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pen);
                cub.name = $"{baseName}_{i + 1}";
                cub.transform.position = pen.TransformPoint(new Vector3(-1f + i * 1f, 0.3f, 0.4f));
                Animal a = cub.GetComponent<Animal>();
                if (a != null) a.lifeStage = LifeStage.child;   // nace como cría (sin esto, el prefab arranca joven)
                CriaCareTarget care = cub.AddComponent<CriaCareTarget>(); care.station = station;
            }
        CriaFeeder feeder = pen.gameObject.AddComponent<CriaFeeder>(); feeder.what = food;
        // PUESTOS de cuidado junto a la puerta, por fuera (leer estado → calmar → alimentar → asear → arrullar).
        (string action, string label, Color c)[] steps =
        {
            ("LeerEstado", "lees su estado (hambre, estrés, vínculo)", new Color(0.7f, 0.75f, 0.85f)),
            ("Calmar",     "te sientas cerca, en calma",               new Color(0.65f, 0.8f, 0.7f)),
            ("Alimentar",  $"das la toma ({food})",                     new Color(0.9f, 0.85f, 0.7f)),
            ("Asear",      "la aseas",                                  new Color(0.75f, 0.75f, 0.6f)),
            ("Arrullar",   "respondes a su llamada",                    new Color(0.8f, 0.7f, 0.8f)),
        };
        Transform posts = new GameObject("Puestos_cuidado").transform; posts.SetParent(pen, false);
        for (int i = 0; i < steps.Length; i++)
        {
            Vector3 local = doorEast ? new Vector3(3.6f, 0.45f, -1.6f + i * 0.8f) : new Vector3(-1.6f + i * 0.8f, 0.45f, 3.6f);
            StationPart sp = MakeStationPart(posts, station, steps[i].action, steps[i].label, pen.TransformPoint(local), steps[i].c);
            sp.transform.localScale = Vector3.one * 0.4f;
        }
    }
}
