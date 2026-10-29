using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUERTO POR GREMIO (policultivo): una parcela repartida por el mapa donde crecen JUNTAS las plantas que se ayudan
/// (docs/huertos-y-cultivos.md): la milpa de las Tres Hermanas, la chacra andina, el arrozal con soja en los bordes,
/// las hortalizas asociadas (tomate+albahaca, zanahoria+cebolla…), el campo de rotación de legumbres y granos, y el
/// bosque comestible por capas. Con SETO CORTAVIENTOS al norte (de donde sopla el viento polar).
/// Las plantas se generan al arrancar (no se guardan en la escena); la parcela (suelo con collider) la pone el editor.
/// </summary>
public class GardenPlot : MonoBehaviour
{
    public enum Guild { Hortalizas, Milpa, ChacraAndina, Arrozal, Rotacion, BosqueComestible, Bambusal }

    public Guild guild;
    public Vector2 size = new Vector2(20f, 16f);
    public int seed = 1;
    [Tooltip("Bosque comestible: por qué especie del dosel y de los árboles bajos empieza (dos bancales del mismo gremio, p. ej. los dos pisos del Árbol de los Campos, plantan especies distintas).")]
    public int speciesOffset;

    public static readonly List<GardenPlot> All = new List<GardenPlot>();
    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public int Plants { get; private set; }

    // ── EL SUELO (docs/garden-simulation.md §8, receta base: abonar · arar): lo prepara la iniciación del huerto ──
    /// <summary>¿El suelo está listo para sembrar (abonado, labrado, con surcos y rastrillado)?</summary>
    public bool SoilPrepared { get; private set; }
    /// <summary>Cuánto se ha preparado (0 = sin tocar, 1 = listo).</summary>
    public float SoilProgress { get; private set; }

    /// <summary>Marca el paso <paramref name="done"/> de <paramref name="total"/> de preparar el suelo y lo deja VER: la capa
    /// de abono (más oscura) al esparcirlo y los surcos al labrar.</summary>
    public void PrepareSoil(int done, int total)
    {
        SoilProgress = Mathf.Clamp01(done / (float)Mathf.Max(1, total));
        if (done >= 2 && transform.Find("Abono") == null)   // abono esparcido: una capa oscura sobre el bancal
            SoilPiece("Abono", new Vector3(0f, 0.07f, 0f), new Vector3(size.x, 0.02f, size.y), new Color(0.24f, 0.17f, 0.11f));
        if (done >= 4 && transform.Find("Surcos") == null)   // surcos: caballones a un palmo y medio, a lo largo del bancal
        {
            Transform s = new GameObject("Surcos").transform; s.SetParent(transform, false);
            for (float x = -size.x * 0.5f + 0.4f; x <= size.x * 0.5f - 0.4f; x += 0.6f)
                SoilPiece("Caballón", new Vector3(x, 0.1f, 0f), new Vector3(0.22f, 0.06f, size.y - 0.4f), new Color(0.3f, 0.21f, 0.14f), s);
        }
        if (done >= total)
        {
            SoilPrepared = true;
            Debug.Log($"[Huerto] el suelo de «{name}» está preparado: abonado, labrado y con surcos.");
        }
    }

    void SoilPiece(string n, Vector3 local, Vector3 scale, Color c, Transform parent = null)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(g.GetComponent<Collider>());   // se ve, no se pisa (el NavMesh es el del suelo)
        g.name = n; g.transform.SetParent(parent != null ? parent : transform, false);
        g.transform.localPosition = local; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
    }

    System.Random _rnd;
    Transform _root;
    float R(float a, float b) => a + (float)_rnd.NextDouble() * (b - a);

    void Start() { Generate(); }

    public static string Title(Guild g)
    {
        switch (g)
        {
            case Guild.Hortalizas: return "Huerto de hortalizas asociadas";
            case Guild.Milpa: return "Milpa (las Tres Hermanas)";
            case Guild.ChacraAndina: return "Chacra andina";
            case Guild.Arrozal: return "Arrozal";
            case Guild.Rotacion: return "Campo de rotación: legumbres y granos";
            case Guild.Bambusal: return "Bambusal (guadua y cáñamo para construir)";
            default: return "Bosque comestible";
        }
    }

    public void Generate()
    {
        if (_root != null) return;
        _rnd = new System.Random(seed);
        _root = new GameObject("Plantas").transform;
        _root.SetParent(transform, false);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;

        switch (guild)
        {
            case Guild.Hortalizas:
                // Filas de parejas que se protegen: el aroma de la albahaca confunde a los insectos del tomate; la cebolla y
                // la zanahoria se espantan mutuamente la mosca; las aromáticas en flor atraen a los depredadores de plagas.
                Rows(hx, hz, 1.8f, new[]
                {
                    // (~6 filas: cada fila alterna 2–3 especies que se llevan bien)
                    new[] { GardenCrop.Kind.Tomate, GardenCrop.Kind.Albahaca, GardenCrop.Kind.Pepino },
                    new[] { GardenCrop.Kind.Zanahoria, GardenCrop.Kind.Cebolla, GardenCrop.Kind.Perejil },
                    new[] { GardenCrop.Kind.Col, GardenCrop.Kind.Cilantro, GardenCrop.Kind.Frutilla },
                    new[] { GardenCrop.Kind.Espinaca, GardenCrop.Kind.Ajo, GardenCrop.Kind.Romero },
                    new[] { GardenCrop.Kind.Pimiento, GardenCrop.Kind.Chile, GardenCrop.Kind.Comino },
                    new[] { GardenCrop.Kind.Berenjena, GardenCrop.Kind.Albahaca, GardenCrop.Kind.Papa },
                    new[] { GardenCrop.Kind.Cebollin, GardenCrop.Kind.Zanahoria },
                }, 0.7f);
                Border(hx, hz, GardenCrop.Kind.Lino, 1.2f);   // flores para polinizadores en el borde sur
                break;

            case Guild.Milpa:
                // Montículos: el maíz es el tutor, el frijol trepa por él y fija nitrógeno, la calabaza cubre el suelo (sombra,
                // humedad, menos malas hierbas). Amaranto y chile en los bordes; nopal como cerca viva.
                for (float x = -hx + 1.5f; x <= hx - 1.5f; x += 2.6f)
                    for (float z = -hz + 1.5f; z <= hz - 2.5f; z += 2.6f)
                    {
                        Plant(GardenCrop.Kind.Maiz, new Vector3(x, 0f, z));
                        Plant(GardenCrop.Kind.Frijol, new Vector3(x + 0.35f, 0f, z + 0.2f));
                        if (((int)(x + z)) % 2 == 0) Plant(GardenCrop.Kind.Calabaza, new Vector3(x + 1.3f, 0f, z + 1.3f));
                    }
                for (float x = -hx + 1f; x <= hx - 1f; x += 1.4f)
                    Plant(((int)x) % 2 == 0 ? GardenCrop.Kind.Amaranto : GardenCrop.Kind.Chile, new Vector3(x, 0f, hz - 1f));
                for (float x = -hx + 1f; x <= hx - 1f; x += 2f) Plant(GardenCrop.Kind.Nopal, new Vector3(x, 0f, -hz - 0.8f));
                for (float x = -hx + 1.6f; x <= hx - 1.6f; x += 2.4f) Plant(((int)x) % 2 == 0 ? GardenCrop.Kind.Achiote : GardenCrop.Kind.Azafran, new Vector3(x, 0f, hz + 0.1f));   // achiote y azafrán (color y aroma de la carta)
                break;

            case Guild.ChacraAndina:
                // Franjas alternas de quinoa, tarwi (lupino que abona el suelo) y choclo; huacatay y ají ahuyentan plagas.
                Rows(hx, hz, 1.6f, new[]
                {
                    new[] { GardenCrop.Kind.Quinoa, GardenCrop.Kind.Haba }, new[] { GardenCrop.Kind.Tarwi, GardenCrop.Kind.Melloco },
                    new[] { GardenCrop.Kind.Maiz, GardenCrop.Kind.MaizMorado }, new[] { GardenCrop.Kind.Huacatay, GardenCrop.Kind.Chile, GardenCrop.Kind.Hierbaluisa },
                    new[] { GardenCrop.Kind.Quinoa, GardenCrop.Kind.Papa }, new[] { GardenCrop.Kind.Tarwi, GardenCrop.Kind.Arveja },
                    new[] { GardenCrop.Kind.Amaranto, GardenCrop.Kind.Cedron }, new[] { GardenCrop.Kind.MaizMorado },
                }, 0.9f);
                break;

            case Guild.Arrozal:
                // Bancales inundados de arroz; en los caballones (bordes secos) soja y cebollín, como en los arrozales
                // asiáticos; el alga se cultiva en el canal de agua más cercano.
                for (float z = -hz + 1.5f; z <= hz - 1.5f; z += 4f)
                {
                    for (float x = -hx + 0.8f; x <= hx - 0.8f; x += 1.4f)
                    {
                        Plant(GardenCrop.Kind.Arroz, new Vector3(x, 0f, z));
                        Plant(GardenCrop.Kind.Arroz, new Vector3(x, 0f, z + 1.4f));
                    }
                    PaddyWater(new Vector3(0f, 0f, z + 0.7f), size.x - 1f, 2.6f);   // la terraza INUNDADA donde crece el arroz
                    Bed(new Vector3(0f, 0f, z + 2.6f), new Vector3(size.x - 1f, 0.25f, 0.7f), new Color(0.52f, 0.44f, 0.36f));   // caballón de tierra
                    for (float x = -hx + 0.8f; x <= hx - 0.8f; x += 1f)
                        Plant(((int)(x * 2f)) % 3 == 0 ? GardenCrop.Kind.Cebollin : GardenCrop.Kind.Soja, new Vector3(x, 0.25f, z + 2.6f));
                }
                for (float x = -hx + 1f; x <= hx - 1f; x += 3f) Plant(GardenCrop.Kind.Wasabi, new Vector3(x, 0f, -hz + 0.2f));   // wasabi en la orilla húmeda y fresca
                SeaweedInNearestWater(18);
                break;

            case Guild.Rotacion:
                // Franjas que ROTAN cada temporada: legumbre (fija nitrógeno) → cereal (lo aprovecha) → otra legumbre…
                // Así el suelo no se agota y las plagas de un cultivo no encuentran el mismo sitio al año siguiente.
                Rows(hx, hz, 2.2f, new[]
                {
                    new[] { GardenCrop.Kind.Trigo }, new[] { GardenCrop.Kind.Lenteja, GardenCrop.Kind.Arveja }, new[] { GardenCrop.Kind.Trigo, GardenCrop.Kind.Avena },
                    new[] { GardenCrop.Kind.Garbanzo, GardenCrop.Kind.Haba }, new[] { GardenCrop.Kind.Teff, GardenCrop.Kind.Sesamo },
                    new[] { GardenCrop.Kind.Caupi, GardenCrop.Kind.Mani }, new[] { GardenCrop.Kind.Soja },
                }, 0.7f);
                Border(hx, hz, GardenCrop.Kind.Lino, 1f);
                break;

            case Guild.Bambusal:
                // BAMBUSAL: matas de guadua (cada mata, un rizoma con varias cañas de distintas edades) separadas para cortar
                // las maduras sin dañar el rizoma, y un borde de cáñamo (fibra para cuerda y cañamiza para el micelio).
                for (float x = -hx + 2f; x <= hx - 2f; x += 3.5f)
                    for (float z = -hz + 2f; z <= hz - 3f; z += 3.5f)
                        Plant(GardenCrop.Kind.Bambu, new Vector3(x + R(-0.4f, 0.4f), 0f, z + R(-0.4f, 0.4f)));
                for (float x = -hx + 0.8f; x <= hx - 0.8f; x += 1.1f) Plant(GardenCrop.Kind.Canamo, new Vector3(x, 0f, hz - 1f));
                break;

            default:   // BOSQUE COMESTIBLE por capas: dosel, árboles bajos, arbustos, herbáceas, trepadoras y hongos a la sombra
                GardenCrop.Kind[] canopy = { GardenCrop.Kind.Mango, GardenCrop.Kind.Coco, GardenCrop.Kind.Guanabana, GardenCrop.Kind.Datil, GardenCrop.Kind.Almendro, GardenCrop.Kind.Olivo, GardenCrop.Kind.Aguacate, GardenCrop.Kind.Clavo };
                GardenCrop.Kind[] small = { GardenCrop.Kind.Limon, GardenCrop.Kind.Naranja, GardenCrop.Kind.Cacao, GardenCrop.Kind.Lima, GardenCrop.Kind.Canela, GardenCrop.Kind.Laurel, GardenCrop.Kind.Cafe, GardenCrop.Kind.Babaco };
                int c = speciesOffset, s = speciesOffset;
                for (float x = -hx + 4f; x <= hx - 3f; x += 7.5f)
                    for (float z = -hz + 4f; z <= hz - 3f; z += 7.5f)
                    {
                        Plant(canopy[c++ % canopy.Length], new Vector3(x, 0f, z));
                        Plant(GardenCrop.Kind.Pimienta, new Vector3(x + 1f, 0f, z + 0.6f));   // trepa por el tronco
                        Plant(small[s++ % small.Length], new Vector3(x + 3.7f, 0f, z + 3.2f));
                        Plant(GardenCrop.Kind.Banana, new Vector3(x - 2.5f, 0f, z + 2.8f));
                        Plant(GardenCrop.Kind.Shiitake, new Vector3(x + 1.6f, 0f, z - 1.4f));   // troncos a la sombra
                        for (int k = 0; k < 5; k++)
                            Plant(k % 3 == 0 ? GardenCrop.Kind.Pina : k % 3 == 1 ? GardenCrop.Kind.Jengibre : GardenCrop.Kind.Curcuma,
                                  new Vector3(x + R(-3f, 3f), 0f, z + R(-3f, 3f)));
                    }
                Border(hx, hz, GardenCrop.Kind.Sandia, 2.2f);
                for (float x = -hx + 1f; x <= hx - 1f; x += 2.2f) Plant(((int)x) % 3 == 0 ? GardenCrop.Kind.Cana : ((int)x) % 3 == 1 ? GardenCrop.Kind.Naranjilla : GardenCrop.Kind.Mortino, new Vector3(x, 0f, hz - 0.8f));
                break;
        }
        if (guild != Guild.BosqueComestible) Windbreak(hx, hz);
        Debug.Log($"[Huerto] {Title(guild)}: {Plants} plantas en {size.x:0}×{size.y:0} m.");
    }

    // Filas este-oeste; cada fila alterna las especies de su grupo (policultivo dentro de la fila).
    void Rows(float hx, float hz, float rowGap, GardenCrop.Kind[][] rows, float step)
    {
        int r = 0;
        for (float z = -hz + 1.2f; z <= hz - 1.2f; z += rowGap, r++)
        {
            GardenCrop.Kind[] row = rows[r % rows.Length];
            int i = 0;
            for (float x = -hx + 1.2f; x <= hx - 1.2f; x += step * Spacing(row[0]), i++)
                Plant(row[i % row.Length], new Vector3(x, 0f, z + R(-0.1f, 0.1f)));
        }
    }

    static float Spacing(GardenCrop.Kind k) =>
        k == GardenCrop.Kind.Tomate || k == GardenCrop.Kind.Berenjena || k == GardenCrop.Kind.Col ? 1.4f :
        k == GardenCrop.Kind.Maiz || k == GardenCrop.Kind.Quinoa || k == GardenCrop.Kind.Amaranto || k == GardenCrop.Kind.Tarwi ? 1.2f : 1f;

    void Border(float hx, float hz, GardenCrop.Kind k, float step)
    {
        for (float x = -hx + 0.8f; x <= hx - 0.8f; x += step) Plant(k, new Vector3(x, 0f, -hz + 0.3f));
    }

    // Seto cortavientos al NORTE: matas densas de arbusto de hielo (el viento polar entra por ahí).
    void Windbreak(float hx, float hz)
    {
        for (float x = -hx; x <= hx; x += 1.1f)
        {
            Bed(new Vector3(x, 0.7f + R(-0.1f, 0.15f), hz + 0.9f), new Vector3(1.3f, 1.4f + R(0f, 0.4f), 1f), new Color(0.70f, 0.82f, 0.74f));
            Bed(new Vector3(x + 0.4f, 1.5f, hz + 0.9f), new Vector3(0.8f, 0.7f, 0.8f), new Color(0.78f, 0.88f, 0.80f));
        }
    }

    void SeaweedInNearestWater(int n)
    {
        // Busca agua (el rayo no toca hielo: cae al fondo marino) en anillos alrededor de la parcela. Antes plantaba solo
        // alrededor del PRIMER punto de agua y se rendía: si era un canal estrecho (o algo nuevo tapaba el agua junto a la
        // orilla), no quedaba ni un alga y la carta perdía nori y kombu. Ahora sigue buscando hasta plantar las n.
        int placed = 0;
        for (float rad = size.x * 0.6f; rad < 120f && placed < n; rad += 3f)
            for (int a = 0; a < 24 && placed < n; a++)
            {
                float ang = a * Mathf.PI / 12f;
                Vector3 p = transform.position + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * rad;
                if (!IsWater(p)) continue;
                for (int k = 0; k < n * 3 && placed < n; k++)
                {
                    Vector3 q = p + new Vector3(R(-4f, 4f), 0f, R(-4f, 4f));
                    if (!IsWater(q)) continue;
                    GardenCrop.Build(_root, GardenCrop.Kind.Alga, new Vector3(q.x, -0.35f, q.z), seed * 31 + placed);
                    placed++; Plants++;
                }
            }
        if (placed == 0) Debug.LogWarning($"[Huerto] {name}: no encuentro agua a menos de 120 m para cultivar el alga (nori/kombu).");
    }

    static bool IsWater(Vector3 p) =>
        Physics.Raycast(p + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 40f, ~0, QueryTriggerInteraction.Ignore) && hit.point.y < -2f;

    void Plant(GardenCrop.Kind k, Vector3 local)
    {
        Vector3 world = transform.TransformPoint(local + new Vector3(R(-0.08f, 0.08f), 0f, R(-0.08f, 0.08f)));
        GardenCrop.Build(_root, k, world, seed * 1000 + Plants);
        Plants++;
    }


    // (Sin bancales elevados: eran solo adorno y no se veían bien — Berón, 2026-10-22. Las filas van directas en el suelo.)

    // Lámina de agua somera de una terraza de arroz (se ve el agua entre las matas).
    void PaddyWater(Vector3 local, float length, float depth)
    {
        Bed(local, new Vector3(length, 0.05f, depth), new Color(0.55f, 0.72f, 0.80f));
    }
    void Bed(Vector3 local, Vector3 size3, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(g.GetComponent<Collider>());
        g.name = "Bancal";
        g.transform.SetParent(_root, false);
        g.transform.localPosition = local + Vector3.up * size3.y * 0.5f;
        g.transform.localScale = size3;
        Renderer r = g.GetComponent<Renderer>();
        r.sharedMaterial = BedMat(c);
    }

    static readonly Dictionary<Color, Material> _bedMats = new Dictionary<Color, Material>();
    static Material BedMat(Color c)
    {
        if (_bedMats.TryGetValue(c, out Material m) && m != null) return m;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        m = new Material(sh) { color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return _bedMats[c] = m;
    }
}
