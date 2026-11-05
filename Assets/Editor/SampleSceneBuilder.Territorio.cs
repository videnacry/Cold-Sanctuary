using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL TERRITORIO POLAR (Berón 2026-11-05; docs/territorio-polar.md): sobre los casquetes y la isla de tierra de
/// <see cref="PolarLandscapeBuilder"/> pone lo que hace rico a un lugar polar real: frentes de barrera (Ross), cuevas de hielo y de roca,
/// acantilados de aves (Alkefjellet), lomas y charcas de tundra, la colonia del emperador al abrigo de icebergs varados, los bivalvos de la
/// morsa. Va dentro de <see cref="BuildPolarIce"/>, con los árboles y las áreas ya puestos (para esquivarlos) y antes del NavMesh.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Colonia del emperador: en el lóbulo Centro:Textil (hielo libre, a unos 75 m de los bancos de la laguna: hasta donde pesca).</summary>
    public static readonly Vector3 EmperorColony = new Vector3(50f, 0f, -55f);
    const float TrunkMargin = 36f;   // lo que se deja al pie de cada árbol: su puerta y sus alfombras del suelo

    static Territorio BuildTerritorio(Transform root)
    {
        Transform t = new GameObject("Territorio_AUTO").transform; t.SetParent(root, false);
        Territorio ter = t.gameObject.AddComponent<Territorio>();
        Physics.SyncTransforms();
        Material ice = MakeMaterial("Barrera_hielo_MAT", new Color(0.72f, 0.86f, 0.95f)), snow = MakeMaterial("Barrera_nieve_MAT", new Color(0.94f, 0.96f, 0.98f));
        Material deep = MakeMaterial("Cueva_azul_MAT", new Color(0.42f, 0.68f, 0.86f));
        Material rock = MakeMaterial("Roca_isla_MAT", new Color(0.36f, 0.34f, 0.33f)), soil = MakeMaterial("Tierra_tundra_MAT", new Color(0.44f, 0.45f, 0.3f));
        Material guano = MakeMaterial("Guano_MAT", new Color(0.86f, 0.84f, 0.74f));

        // ── Los dos suelos: área real de cada uno ──
        int landCells = 0, iceCells = 0;
        for (float x = -PolarLandscapeBuilder.Half; x < PolarLandscapeBuilder.Half; x += 2.5f)
            for (float z = -PolarLandscapeBuilder.Half; z < PolarLandscapeBuilder.Half; z += 2.5f)
            {
                if (!PolarLandscapeBuilder.NaturalIce(x, z)) continue;
                if (PolarLandscapeBuilder.LandAt(x, z)) landCells++; else iceCells++;
            }
        ter.landArea = landCells * 6.25f; ter.iceArea = iceCells * 6.25f;

        // ── Frentes de barrera: de cara al mar abierto, como el frente de la barrera de Ross ──
        ter.cliffs += CliffArc(t, "Centro:Textil", 90f, 250f, 4f, 7f, ice, snow, true);
        ter.cliffs += CliffArc(t, "BosqueOeste:Bosque_3", 200f, 320f, 5f, 8f, ice, snow, true);
        ter.cliffs += CliffArc(t, "BosqueNorte:Bosque_4", 300f, 400f, 4f, 7f, ice, snow, true);
        ter.cliffs += CliffArc(t, "BosqueNorte:Bosque_6", 340f, 470f, 5f, 8f, ice, snow, true);
        // Acantilados de aves de la isla (Alkefjellet: basalto de 100 m; aquí, a escala del santuario).
        int birds = CliffArc(t, "Isla:Cabo", 120f, 250f, 7f, 12f, rock, guano, false);
        birds += CliffArc(t, "Isla:Poniente", 150f, 280f, 6f, 10f, rock, guano, false);
        ter.cliffs += birds;

        // ── Cuevas: tres de hielo y una de roca (la guarida de los lobos); una de hielo es guarida de oso ──
        var caves = new List<Transform>();
        void Add(string lobe, string name, Material m, Material inner, string den, string label)
        {
            Transform c = Cave(t, lobe, name, m, inner, den, label);
            if (c != null) caves.Add(c); else Debug.LogWarning($"[Territorio] No cabe la cueva «{name}» en {lobe}.");
        }
        Add("Centro:Arrozal", "Cueva_hielo_Centro", ice, deep, "", "Cueva de hielo");
        Add("BosqueOeste:Bosque_1", "Cueva_hielo_Oeste", ice, deep, "", "Cueva de hielo");
        Add("BosqueNorte:Bosque_5", "Cueva_hielo_Norte", ice, deep, "", "Cueva de hielo");
        Add("Isla:Poniente", "Cueva_roca_Poniente", rock, rock, "Wolf", "Cueva de roca (guarida de los lobos)");
        Add("Isla:Valle", "Cueva_roca_Valle", rock, rock, "Bear", "Cueva de roca (la osa pare aquí; en la isla de Wrangel hay cientos de guaridas)");
        ter.caves = caves.ToArray();

        // ── La isla: lomas, charcas y madriguera ──
        Hill(t, new Vector3(-222f, 0f, -118f), 26f, soil);
        Hill(t, new Vector3(-140f, 0f, -103f), 30f, soil);
        Hill(t, new Vector3(-55f, 0f, -178f), 24f, soil);
        Hill(t, new Vector3(-165f, 0f, -190f), 34f, soil);
        Material pond = MakeWaterMaterial("Charca_MAT", new Color(0.32f, 0.5f, 0.6f, 0.75f));
        foreach (Vector3 p in new[] { new Vector3(-150f, 0f, -175f), new Vector3(-105f, 0f, -190f), new Vector3(-20f, 0f, -165f), new Vector3(-200f, 0f, -200f) })
        {
            Transform c = Block(t, "Charca", p + Vector3.up * 0.02f, new Vector3(9f, 0.03f, 6f), pond, 0f, false);
            c.gameObject.AddComponent<WaterSource>();
            PolarLandscapeBuilder.IgnoreForNav(c.gameObject);
        }
        Transform mound = new GameObject("Madriguera_zorro_artico").transform; mound.SetParent(t, false); mound.position = new Vector3(-118f, 0f, -118f);
        Block(mound, "Monticulo", mound.position + Vector3.up * 0.5f, new Vector3(7f, 1f, 6f), soil, 0f, true);
        Block(mound, "Monticulo", mound.position + Vector3.up * 1.2f, new Vector3(4f, 0.5f, 3.5f), soil, 0f, true);
        for (int i = 0; i < 4; i++) Block(mound, "Boca_madriguera", mound.position + Quaternion.Euler(0f, 45f + i * 90f, 0f) * Vector3.forward * 4.2f + Vector3.up * 0.03f, new Vector3(0.7f, 0.05f, 0.7f), rock, 0f, false);
        // La guarida es la boca sur (el centro del montículo no se pisa).
        Transform foxDoor = new GameObject("Entrada_madriguera").transform; foxDoor.SetParent(mound, false); foxDoor.localPosition = new Vector3(0f, 0f, -5.2f);
        Den foxDen = foxDoor.gameObject.AddComponent<Den>(); foxDen.species = "ArcticFox";
        Affordance.Put(foxDoor.gameObject, AffordanceKind.Den, 2);

        // ── La colonia del emperador: icebergs varados de cortavientos (crían en el hielo fijo, al abrigo) ──
        ter.emperorColony = EmperorColony;
        for (int i = 0; i < 4; i++)
        {
            Vector3 at = EmperorColony + Quaternion.Euler(0f, 110f + i * 34f, 0f) * Vector3.forward * 11f;
            if (!PolarLandscapeBuilder.IceAt(at.x, at.z)) continue;
            Block(t, "Iceberg_varado", at + Vector3.up * (2.5f + i * 0.4f), new Vector3(7f, 5f + i * 0.8f, 5f), ice, 20f * i, true);
        }

        // ── Bivalvos de la morsa: en los bajíos de grava de la laguna (PolarHabitats pone la grava) ──
        for (int k = 0, n = FoodChain.Banks(SwarmKind.Clam); k < n; k++)   // tantos como pide la cadena
        {
            Vector3 at = PolarLandscapeBuilder.Lagoon + Quaternion.Euler(0f, 70f + k * 150f, 0f) * Vector3.forward * 16f; at.y = -2.8f;
            Swarm.Create<Swarm>(t, $"Bivalvos_{k}", at, SwarmKind.Clam, k % 2 == 0 ? SwarmModels.Species.Clam : SwarmModels.Species.Mya, 30f, 40f, 2.5f);
        }

        // Los casquetes silvestres: lo que la fauna usa donde estaba el bosque (antes de repartir los petreles: sus rocas tienen nidos).
        BuildWildCaps(t, ice, snow, rock);

        // Los petreles de las nieves: parejas, volando desde sus nidos. Son pocos y no usan NavMesh.
        GameObject petrel = LoadAnimalPrefab("SnowPetrel");
        var nests = t.GetComponentsInChildren<SnowPetrelNest>();
        if (petrel != null && nests.Length > 0)
            for (int i = 0, n = PopulationPlan.Target("SnowPetrel"); i < n; i++)
                Adult(petrel, t, nests[(i / 2) % nests.Length].transform.position + Vector3.up * 3f + Vector3.right * (i % 2), i % 2 == 0 ? Sex.female : Sex.male, $"Petrel_nieves_{i + 1}");

        BuildFastIce(t, MakeMaterial("Hielo_fijo_MAT", new Color(0.8f, 0.9f, 0.96f)), MakeMaterial("Grieta_marea_MAT", new Color(0.12f, 0.2f, 0.26f)));

        // ── Puntos de la isla para la prueba ──
        var points = new List<Vector3>();
        foreach (var lobe in PolarLandscapeBuilder.Islands)
        {
            if (!lobe.name.StartsWith("Isla:") && lobe.name != "Sur:Rotacion") continue;
            Vector3 p = new Vector3(lobe.x, 0f, lobe.z);
            if (PolarLandscapeBuilder.LandAt(p.x, p.z)) points.Add(p);
        }
        ter.landPoints = points.ToArray();

        Debug.Log($"[Territorio] Tierra {ter.landArea:0} m² · hielo {ter.iceArea:0} m² ({100f * ter.landArea / Mathf.Max(1f, ter.landArea + ter.iceArea):0} % tierra); {ter.cliffs} bloques de barrera y acantilado ({birds} de aves); {caves.Count} cuevas.");
        return ter;
    }

    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Cadena alimenticia (evaluar)")]
    static void FoodChainMenu() => FoodChain.Log();

    /// <summary>EL HIELO FIJO (docs/hielo-y-respiraderos.md): láminas tan finas como las placas (`SeaRule.PlateThickness`) unidas a su frente, sobre la laguna, con su grieta de marea y un agujero de
    /// verdad para las focas. Son los únicos respiraderos: las placas, que llevan los edificios, son pared para la fauna (P20).</summary>
    static int BuildFastIce(Transform parent, Material thin, Material crack)
    {
        const float W = 5f, Back = 4f, Lip = 0.5f, Hole = 0.8f, HoleAt = 2f, Th = SeaRule.PlateThickness;
        int n = 0;
        Vector3 lagoon = PolarLandscapeBuilder.Lagoon; lagoon.y = 0f;
        for (float a = 0f; a < 360f; a += 30f)
        {
            Vector3 dir = TreeSpec.Dir(a), shore = Vector3.zero; bool found = false;
            for (float r = 8f; r < 80f && !found; r += 1f) { Vector3 p = lagoon + dir * r; if (PolarLandscapeBuilder.IceAt(p.x, p.z)) { shore = p; found = true; } }
            if (!found) continue;
            Quaternion rot = Quaternion.LookRotation(dir);
            if (!Clear(shore - dir * 2f + Vector3.down * 1f, new Vector3(W, 1f, Back), rot)) continue;   // (el submarino atracado, un muelle…)
            Transform sheet = new GameObject($"Lamina_hielo_fijo_{n}").transform; sheet.SetParent(parent, false); sheet.SetPositionAndRotation(shore, rot);
            // Cuatro piezas alrededor del agujero, en coordenadas de la orilla: +Z, hacia la placa.
            void Piece(float x0, float x1, float z0, float z1) => Block(sheet, MarineLife.ThinIce, sheet.TransformPoint(new Vector3((x0 + x1) * 0.5f, -Th * 0.5f, (z0 + z1) * 0.5f)), new Vector3(x1 - x0, Th, z1 - z0), thin, a, true);
            Piece(-W, -Hole, -Back, Lip); Piece(Hole, W, -Back, Lip);
            Piece(-Hole, Hole, -Back, -HoleAt - Hole); Piece(-Hole, Hole, -HoleAt + Hole, Lip);
            Block(sheet, "Grieta_de_marea", sheet.TransformPoint(new Vector3(0f, 0.012f, -0.2f)), new Vector3(2f * W, 0.02f, 0.25f), crack, a, false);
            var hole = new GameObject($"Respiradero_{n}"); hole.transform.SetParent(sheet, false); hole.transform.localPosition = new Vector3(0f, 0f, -HoleAt);
            Affordance.Put(hole, AffordanceKind.BreathingHole, 1);
            n++;
        }
        Debug.Log($"[Territorio] {n} láminas de hielo fijo con su respiradero junto a la laguna.");
        return n;
    }

    // Un bloque con o sin collider, en coordenadas del mundo.
    static Transform Block(Transform parent, string name, Vector3 center, Vector3 size, Material m, float yaw, bool solid)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, true);
        g.transform.SetPositionAndRotation(center, Quaternion.Euler(0f, yaw, 0f)); g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        g.isStatic = true;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        return g.transform;
    }

    static bool NearTrunk(Vector3 p, float margin)
    {
        foreach (Helix hx in _helixes)
            if (hx.root != null && Vector2.Distance(new Vector2(p.x, p.z), new Vector2(hx.root.position.x, hx.root.position.z)) < margin) return true;
        return false;
    }

    // ¿Hay sitio? Nada sólido en esa caja, salvo el propio suelo (ni árboles ni edificios: a pie de hielo aún no hay alfombras, que los troncos ya reservan).
    static bool Clear(Vector3 center, Vector3 half, Quaternion rot)
    {
        foreach (Collider c in Physics.OverlapBox(center, half, rot, ~0, QueryTriggerInteraction.Ignore))
            if (c.name != Territorio.Ice && !Territorio.IsLand(c) && c.name != "Sea_Placeholder" && !c.name.StartsWith("Ground")) return false;
        return true;
    }

    /// <summary>Un FRENTE (de hielo o de roca) por el borde de un lóbulo entre dos ángulos: bloques altos pegados a la orilla, sobre su propio
    /// suelo, que esquivan lo que haya y los pies de los árboles. Devuelve cuántos puso.</summary>
    static int CliffArc(Transform parent, string lobeName, float a0, float a1, float h0, float h1, Material body, Material cap, bool ice)
    {
        var lobe = System.Array.Find(PolarLandscapeBuilder.Islands, l => l.name == lobeName);
        if (lobe.name == null) { Debug.LogWarning($"[Territorio] No hay lóbulo {lobeName}."); return 0; }
        var rnd = new System.Random(lobeName.Length * 977 + (int)a0);
        Transform arc = new GameObject("Frente_" + lobeName.Replace(':', '_')).transform; arc.SetParent(parent, false);
        float step = 7.2f / lobe.r * Mathf.Rad2Deg;
        int n = 0;
        for (float a = a0; a <= a1; a += step)
        {
            Vector3 dir = TreeSpec.Dir(a);
            // Lo más afuera posible que siga siendo su suelo (la orilla es irregular).
            Vector3 at = Vector3.zero; bool ok = false;
            for (float r = lobe.r * 1.08f; r > lobe.r * 0.7f && !ok; r -= 1f)
            {
                Vector3 p = new Vector3(lobe.x, 0f, lobe.z) + dir * r, q = p - dir * 3f;
                bool own = ice ? PolarLandscapeBuilder.IceAt(p.x, p.z) && PolarLandscapeBuilder.IceAt(q.x, q.z) : PolarLandscapeBuilder.LandAt(p.x, p.z) && PolarLandscapeBuilder.LandAt(q.x, q.z);
                if (!own) continue;
                at = p - dir * 2.5f; ok = true;
            }
            if (!ok || NearTrunk(at, TrunkMargin)) continue;
            float h = h0 + (float)rnd.NextDouble() * (h1 - h0), w = 7.6f, d = 5f;
            Quaternion rot = Quaternion.Euler(0f, a, 0f);
            if (!Clear(at + Vector3.up * (h * 0.5f + 0.2f), new Vector3(w, h, d) * 0.5f, rot)) continue;
            Block(arc, ice ? "Barrera" : "Acantilado", at + Vector3.up * h * 0.5f, new Vector3(w, h, d), body, a, true);
            // Encima: la nieve del hielo o el guano y las repisas del acantilado de aves.
            Block(arc, ice ? "Barrera_nieve" : "Repisa", at + Vector3.up * (h + 0.15f), new Vector3(w + 0.2f, 0.3f, d + 0.2f), cap, a, false);
            if (!ice)
            {
                Block(arc, "Repisa", at + rot * new Vector3(0f, h * 0.6f, d * 0.5f + 0.25f), new Vector3(w * 0.9f, 0.25f, 0.5f), cap, a, false);
                if (n % 2 == 0)   // un nido de petrel de las nieves cada dos bloques: repisas sin nieve (ver SnowPetrelLife)
                {
                    var nest = new GameObject("Nido_petrel"); nest.transform.SetParent(arc, false);
                    nest.transform.position = at + rot * new Vector3(0f, h * 0.6f + 0.2f, d * 0.5f + 0.25f);
                    nest.AddComponent<SnowPetrelNest>();
                }
            }
            n++;
        }
        return n;
    }

    /// <summary>Una CUEVA en la que se entra andando: boca de 4,4 × 3,2 m, 11 m de fondo, paredes y techo con collider y suelo de su casquete (el
    /// NavMesh entra solo). La boca mira al centro del lóbulo y el cuerpo va hacia la orilla. Con «Boca» y «Fondo» para la prueba.</summary>
    static Transform Cave(Transform parent, string lobeName, string name, Material m, Material inner, string denSpecies, string label)
    {
        var lobe = System.Array.Find(PolarLandscapeBuilder.Islands, l => l.name == lobeName);
        if (lobe.name == null) return null;
        const float W = 4.4f, H = 3.2f, D = 11f, T = 1.4f;
        bool ice = !PolarLandscapeBuilder.LandAt(lobe.x, lobe.z);
        for (float r = lobe.r * 0.65f; r > lobe.r * 0.15f; r -= 2f)
            for (float a = 0f; a < 360f; a += 15f)
            {
                Vector3 dir = TreeSpec.Dir(a), mouth = new Vector3(lobe.x, 0f, lobe.z) + dir * r;
                Quaternion rot = Quaternion.LookRotation(dir);
                Vector3 mid = mouth + dir * (D * 0.5f);
                bool ok = !NearTrunk(mid, TrunkMargin);
                for (int i = -1; i <= 1 && ok; i++)
                    for (float z = -6f; z <= D + T; z += 2.5f)
                    {
                        Vector3 p = mouth + rot * new Vector3(i * (W * 0.5f + T), 0f, z);
                        if (!PolarLandscapeBuilder.NaturalIce(p.x, p.z) || PolarLandscapeBuilder.IceAt(p.x, p.z) != ice) { ok = false; break; }
                    }
                if (!ok || !Clear(mid + Vector3.up * (H * 0.5f + 0.9f), new Vector3(W + 2f * T, H + 2f * T, D + 8f) * 0.5f, rot)) continue;

                Transform c = new GameObject(name).transform; c.SetParent(parent, false); c.SetPositionAndRotation(mouth, rot);
                void Piece(string n, Vector3 local, Vector3 size, Material mm, bool solid)
                {
                    Block(c, n, c.TransformPoint(local), size, mm, a, solid);
                }
                Piece("Pared_cueva", new Vector3(-(W + T) * 0.5f, H * 0.5f, D * 0.5f), new Vector3(T, H, D), m, true);
                Piece("Pared_cueva", new Vector3((W + T) * 0.5f, H * 0.5f, D * 0.5f), new Vector3(T, H, D), m, true);
                Piece("Fondo_cueva", new Vector3(0f, H * 0.5f, D + T * 0.5f), new Vector3(W + 2f * T, H, T), m, true);
                Piece("Techo_cueva", new Vector3(0f, H + T * 0.5f, D * 0.5f + T * 0.25f), new Vector3(W + 2f * T, T, D + T), m, true);
                Piece("Domo_cueva", new Vector3(0f, H + T + 0.7f, D * 0.5f), new Vector3(W + T, 1.4f, D - 2f), m, true);
                Piece("Forro_cueva", new Vector3(0f, H - 0.04f, D * 0.5f), new Vector3(W, 0.08f, D), inner, false);
                Piece("Forro_cueva", new Vector3(0f, 1.6f, D - 0.04f), new Vector3(W, 3.1f, 0.08f), inner, false);
                for (int i = 0; i < 6; i++)   // carámbanos (o puntas de roca) del techo: por encima de la cabeza de un oso
                    Piece("Caramba", new Vector3(-1.5f + i * 0.6f, H - 0.25f, 2f + i * 1.4f), new Vector3(0.14f, 0.45f, 0.14f), inner, false);
                var boca = new GameObject("Boca").transform; boca.SetParent(c, false); boca.localPosition = new Vector3(0f, 0f, -2f);
                var fondo = new GameObject("Fondo").transform; fondo.SetParent(c, false); fondo.localPosition = new Vector3(0f, 0f, D - 2.5f);
                Affordance.Put(fondo.gameObject, AffordanceKind.Shelter, 3);
                if (!string.IsNullOrEmpty(denSpecies))
                {
                    Den den = fondo.gameObject.AddComponent<Den>(); den.species = denSpecies;
                    Affordance.Put(fondo.gameObject, AffordanceKind.Den, 1);
                }
                WorldLabel wl = boca.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = H + 0.8f;
                return c;
            }
        return null;
    }

    // Una LOMA de tundra: tres terrazas bajas de 25 cm (el NavMesh sube por ellas) del mismo suelo, «Tierra»: encima crece lo mismo.
    static void Hill(Transform parent, Vector3 at, float size, Material soil)
    {
        for (int i = 0; i < 3; i++)
        {
            float s = size * (1f - i * 0.3f);
            Transform b = Block(parent, Territorio.Land, at + Vector3.up * (0.125f + i * 0.25f), new Vector3(s, 0.25f, s * 0.8f), soil, 0f, true);
            b.position += new Vector3(i * 1.5f, 0f, -i * 1f);
        }
    }
}
