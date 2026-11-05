using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA FAUNA FRÍA DE LOS OTROS SANTUARIOS (Berón 2026-11-05; docs/fauna-fria-santuarios.md): ganchos pequeños que cada escena llama para poner sus especies. Presupuesto bajo y fijo: en
/// el fondo, grupos sin ciclo de vida ni NavMesh (unos 45 individuos); en el aire, <see cref="ColdBirds.AirBudget"/> aves. Cada hook crea lo que come quien ponga.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>EL SANTUARIO MARINO: sobre el lecho <paramref name="floorY"/>, alrededor de <paramref name="centre"/>. Los del fondo no se mueven; los peces derivan poco.</summary>
    public static void PlaceMarineColdFauna(Transform root, Vector3 centre, float floorY)
    {
        Transform t = new GameObject("Fauna_fria_marina").transform; t.SetParent(root, false);
        Vector3 P(float x, float h, float z) => new Vector3(centre.x + x, floorY + h, centre.z + z);
        Swarm Group(string name, Vector3 at, SwarmKind k, SwarmModels.Species s, float n, float spread) { Swarm g = Swarm.Create<Swarm>(t, name, at, k, s, n, n, spread); g.wanderRadius = 8f; return g; }
        Group("Kril_del_fondo", P(30f, 40f, 60f), SwarmKind.Krill, SwarmModels.Species.Krill, 12f, 4f);
        Group("Peces_hielo", P(-40f, 20f, 40f), SwarmKind.Fish, SwarmModels.Species.Icefish, 8f, 5f);
        Group("Austromerluzas", P(50f, 60f, -30f), SwarmKind.Fish, SwarmModels.Species.Toothfish, 3f, 6f);
        Group("Calamar_coloso", P(-60f, 90f, -50f), SwarmKind.Jellyfish, SwarmModels.Species.ColossalSquid, 1f, 3f).driftSpeed = 0.3f;
        Group("Aranas_de_mar", P(20f, 0.3f, -60f), SwarmKind.Clam, SwarmModels.Species.SeaSpider, 6f, 3f);
        // La fumarola con su colonia de cangrejos yeti: viven a metros de ella, cultivando bacterias en el pecho.
        Vector3 vent = P(-30f, 0f, -20f);
        KitchenProps.C(t, vent + Vector3.up * 2f, new Vector3(1.6f, 4f, 1.6f), new Color(0.12f, 0.1f, 0.1f), 0f, "Fumarola");
        Group("Cangrejos_yeti", vent + Vector3.up * 0.3f, SwarmKind.Clam, SwarmModels.Species.YetiCrab, 14f, 2.5f);
        Debug.Log("[Fauna fría] Santuario marino: krill, pez de hielo, austromerluza, calamar colosal, arañas de mar y cangrejo yeti.");
    }

    /// <summary>EL SANTUARIO AÉREO: llama la escena del santuario aéreo con su centro, su radio y la altura del agua. Pone los nidos en roca, las aves adultas (por parejas) y sus bancos,
    /// los que pide <see cref="FoodChain.BanksFor"/>. Sin NavMesh.</summary>
    public static void PlaceAirColdFauna(Transform root, Vector3 centre, float radius, float seaY)
    {
        Transform t = new GameObject("Fauna_fria_aerea").transform; t.SetParent(root, false);
        int placed = 0;
        foreach (var kv in ColdBirds.AirBudget)
        {
            GameObject prefab = LoadAnimalPrefab(kv.Key);
            if (prefab == null) continue;
            for (int i = 0; i < kv.Value; i++, placed++)
            {
                Vector3 at = centre + Quaternion.Euler(0f, placed * 47f, 0f) * Vector3.forward * radius * 0.6f;
                if (i % 2 == 0)
                {
                    KitchenProps.C(t, at + Vector3.down * 0.6f, new Vector3(3f, 1.2f, 3f), new Color(0.36f, 0.34f, 0.33f), 0f, "Acantilado");
                    new GameObject("Nido_" + kv.Key).AddComponent<SnowPetrelNest>().transform.SetParent(t, false);
                    t.GetChild(t.childCount - 1).position = at;
                }
                Adult(prefab, t, at + Vector3.up * 2f, i % 2 == 0 ? Sex.female : Sex.male, $"{kv.Key}_{i + 1}");
            }
        }
        foreach (var (kind, sp) in new[] { (SwarmKind.Fish, SwarmModels.Species.SilverFish), (SwarmKind.Krill, SwarmModels.Species.Krill) })
            for (int i = 0, n = FoodChain.BanksFor(ColdBirds.AirBudget, kind); i < n; i++)
                Swarm.Create<Swarm>(t, $"Banco_{kind}_{i}", centre + Quaternion.Euler(0f, 90f * i, 0f) * Vector3.right * radius * 0.4f + Vector3.up * (seaY - centre.y - 2f), kind, sp, 16f, 24f, 3f);
        Debug.Log($"[Fauna fría] Santuario aéreo: {placed} aves.");
    }
}
