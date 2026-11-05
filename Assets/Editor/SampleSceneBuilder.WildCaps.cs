using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS CASQUETES SILVESTRES SIN BOSQUE (Berón 2026-11-05: «imagino que ahora en su lugar tendrían las estructuras propias para la fauna polar»; docs/territorio-polar.md §Casquetes
/// silvestres). En cada lóbulo «Bosque» va lo que la fauna polar usa de verdad, sin repetir lo de <see cref="PolarHabitats"/> ni de <see cref="BuildTerritorio"/>:
/// - CRESTA DE PRESIÓN: el abrigo del oso y del zorro;
/// - VENTISQUERO a sotavento: donde la osa cava su guarida (la de Bosque Norte ya la pone PolarHabitats: aquí, solo en Bosque Oeste);
/// - AFLORAMIENTO DE ROCA (nunatak): el nido del petrel de las nieves y su posadero;
/// - HUECO DE VIENTO junto a la roca: el viento limpia la nieve y deja el liquen a la vista;
/// - DESCANSADERO en la orilla: foca y morsa.
/// BARATO: todas las piezas de un casquete van en una malla por material, sin un objeto por cubo. Solo son objetos los puntos que usa la fauna.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Piezas por lóbulo: 12 de cresta, 3 de ventisquero, 6 de roca, 4 de hueco y 3 de descansadero.</summary>
    public const int WildPiecesPerLobe = 28;

    class WildCap
    {
        public Transform root;
        public readonly Dictionary<Material, List<CombineInstance>> pieces = new Dictionary<Material, List<CombineInstance>>();
        public void Add(Material m, Vector3 at, Vector3 size, Quaternion rot)
        {
            if (!pieces.TryGetValue(m, out var l)) pieces[m] = l = new List<CombineInstance>();
            l.Add(new CombineInstance { mesh = PolarLandscapeBuilder.CubeMesh(), transform = Matrix4x4.TRS(at, rot, size) });
        }
    }

    static GameObject WildPoint(Transform parent, string name, Vector3 at, AffordanceKind kind, int capacity, float stock = 1f, float regrow = 0.002f)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.position = at;
        Affordance.Put(g, kind, capacity, stock, regrow);
        return g;
    }

    /// <summary>Un sitio libre en el lóbulo: suelo de hielo, lejos de troncos y sin nada encima. Prueba 12 ángulos.</summary>
    static bool WildSpot(in (string name, float x, float z, float r) lobe, float angle, float radius, Vector3 half, out Vector3 at)
    {
        for (int k = 0; k < 12; k++)
        {
            at = new Vector3(lobe.x, 0f, lobe.z) + TreeSpec.Dir(angle + k * 30f) * radius;
            bool ice = true;
            for (int c = 0; c < 4 && ice; c++)
            {
                Vector3 p = at + new Vector3((c & 1) == 0 ? -half.x : half.x, 0f, (c & 2) == 0 ? -half.z : half.z);
                ice = PolarLandscapeBuilder.IceAt(p.x, p.z);
            }
            if (ice && !NearTrunk(at, TrunkMargin) && Clear(at + Vector3.up * (half.y + 0.3f), half, Quaternion.identity)) return true;
        }
        at = Vector3.zero;
        return false;
    }

    static int WildLobe(WildCap cap, (string name, float x, float z, float r) lobe, int index, Material ice, Material snow, Material rock, Material bare, bool den)
    {
        var rnd = new System.Random(4001 + index * 37);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        float a0 = index * 73f;
        Vector3 lee = NuclearSiteRule.PrevailingWindTo;
        int placed = 0;

        // ── La cresta y su ventisquero ──
        if (WildSpot(lobe, a0, lobe.r * 0.45f, new Vector3(13f, 2f, 7f), out Vector3 ridge))
        {
            Vector3 along = Vector3.Cross(Vector3.up, lee);
            for (int i = 0; i < 12; i++)
            {
                float h = R(1.2f, 2.6f);
                cap.Add(ice, ridge + along * (-11f + i * 2f) + Vector3.up * h * 0.35f + lee * R(-0.8f, 0.8f), new Vector3(R(2.2f, 3.6f), h, R(0.6f, 1.1f)),
                        Quaternion.LookRotation(along) * Quaternion.Euler(R(25f, 55f), R(60f, 120f), R(-15f, 15f)));
                placed++;
            }
            Quaternion drift = Quaternion.LookRotation(along);
            for (int i = 0; i < 3; i++)
            {
                cap.Add(snow, ridge + lee * (4.5f + i * 0.4f) + Vector3.up * (0.1f + i * 0.2f), new Vector3(8f - i * 2f, 0.2f, 18f - i * 5f), drift);
                placed++;
            }
            Vector3 leeSide = ridge + lee * 6f;
            WildPoint(cap.root, "Abrigo_cresta", leeSide, AffordanceKind.Shelter, 3);
            if (den)
            {
                GameObject d = WildPoint(cap.root, "Guarida_ventisquero", leeSide + lee * 3f, AffordanceKind.Den, 1);
                d.AddComponent<Den>().species = "Bear";
            }
        }

        // ── La roca, sus nidos y el hueco del viento ──
        if (WildSpot(lobe, a0 + 130f, lobe.r * 0.35f, new Vector3(9f, 3f, 9f), out Vector3 outcrop))
        {
            for (int i = 0; i < 6; i++)
            {
                float s = R(2.5f, 5f), h = i == 0 ? 5f : R(1.2f, 3f);
                Vector3 off = i == 0 ? Vector3.zero : Quaternion.Euler(0f, i * 72f, 0f) * Vector3.forward * R(2f, 3.6f);
                cap.Add(rock, outcrop + off + Vector3.up * h * 0.5f, new Vector3(s, h, s * R(0.7f, 1.1f)), Quaternion.Euler(0f, R(0f, 90f), 0f));
                placed++;
            }
            for (int i = 0; i < 2; i++)
            {
                var nest = new GameObject("Nido_petrel"); nest.transform.SetParent(cap.root, false);
                nest.transform.position = outcrop + Quaternion.Euler(0f, 40f + i * 150f, 0f) * Vector3.forward * 3f + Vector3.up * 4f;
                nest.AddComponent<SnowPetrelNest>();
            }
            WildPoint(cap.root, "Posadero_roca", outcrop + Vector3.up * 5.1f, AffordanceKind.Perch, 2);
            for (int i = 0; i < 4; i++)   // a barlovento, sin nieve
            {
                cap.Add(bare, outcrop - lee * (6f + i * 0.5f) + Vector3.up * 0.02f, new Vector3(3f + i, 0.03f, 8f - i), Quaternion.LookRotation(lee) * Quaternion.Euler(0f, i * 18f - 27f, 0f));
                placed++;
            }
            WildPoint(cap.root, "Liquen_viento", outcrop - lee * 7f, AffordanceKind.Lichen, 2, 10f, 0.0005f);
        }

        // ── El descansadero: en la orilla que da al mar ──
        for (float a = a0 + 250f, tried = 0f; tried < 360f; a += 30f, tried += 30f)
        {
            Vector3 dir = TreeSpec.Dir(a), shore = Vector3.zero; bool ok = false;
            for (float r = lobe.r * 1.08f; r > lobe.r * 0.7f && !ok; r -= 1f)
            {
                Vector3 p = new Vector3(lobe.x, 0f, lobe.z) + dir * r;
                if (!PolarLandscapeBuilder.IceAt(p.x, p.z) || !PolarLandscapeBuilder.IceAt(p.x - dir.x * 7f, p.z - dir.z * 7f)) continue;
                shore = p - dir * 3f; ok = true;
            }
            if (!ok || NearTrunk(shore, TrunkMargin) || !Clear(shore + Vector3.up * 0.8f, new Vector3(5f, 0.6f, 5f), Quaternion.LookRotation(dir))) continue;
            for (int i = 0; i < 3; i++)
            {
                cap.Add(snow, shore - dir * (i * 1.6f) + Vector3.up * (0.075f + i * 0.15f), new Vector3(9f - i * 1.5f, 0.15f, 3.2f), Quaternion.LookRotation(dir));
                placed++;
            }
            WildPoint(cap.root, "Descansadero_silvestre", shore, AffordanceKind.HaulOut, 3);
            break;
        }
        return placed;
    }

    /// <summary>Levanta los dos casquetes. Devuelve cuántas piezas puso.</summary>
    static int BuildWildCaps(Transform t, Material ice, Material snow, Material rock)
    {
        Material bare = MakeMaterial("Hueco_viento_MAT", new Color(0.46f, 0.5f, 0.46f));
        int total = 0, meshes = 0, index = 0;
        foreach (string capName in new[] { "BosqueOeste", "BosqueNorte" })
        {
            var cap = new WildCap { root = new GameObject("Casquete_silvestre_" + capName).transform };
            cap.root.SetParent(t, false);
            foreach (var lobe in PolarLandscapeBuilder.Islands)
                if (lobe.name.StartsWith(capName + ":")) total += WildLobe(cap, lobe, index++, ice, snow, rock, bare, capName == "BosqueOeste");
            foreach (var kv in cap.pieces)
            {
                var mesh = new Mesh { name = $"{capName}_{kv.Key.name}", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.CombineMeshes(kv.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                var g = new GameObject($"Estructuras_{kv.Key.name}"); g.transform.SetParent(cap.root, false); g.isStatic = true;
                g.AddComponent<MeshFilter>().sharedMesh = mesh;
                g.AddComponent<MeshRenderer>().sharedMaterial = kv.Key;
                if (kv.Key != bare) g.AddComponent<MeshCollider>().sharedMesh = mesh;   // (el hueco del viento se pisa: es solo color)
                else PolarLandscapeBuilder.IgnoreForNav(g);
                meshes++;
            }
        }
        Debug.Log($"[Territorio] Casquetes silvestres: {total} piezas en {meshes} mallas.");
        return total;
    }
}
