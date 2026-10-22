using UnityEngine;

/// <summary>
/// HÁBITATS DEL SANTUARIO DE HIELO (docs/natural-interactions.md): los elementos del entorno con los que la fauna polar
/// interactúa de verdad, como <see cref="Affordance"/> en vez de decorado:
///   · DESCANSADEROS en la orilla de la laguna: donde las focas salen a descansar/mudar (y la orca patrulla).
///   · RESPIRADEROS en el hielo junto a la orilla: la foca sube a respirar y los mantiene abiertos; el oso acecha inmóvil
///     a su lado (still-hunting) y la caza si asoma.
///   · PLAYA DE GUIJARROS de la colonia de pingüinos: cada uno lleva piedras a su nido (y las roba del vecino).
///   · GUARIDAS de nieve (ventisqueros) en el territorio de los osos.
///   · BANCOS DE GRAVA en el fondo de la laguna: las ballenas se frotan para mudar la piel.
/// Se genera al arrancar sobre el hielo real (rayos), como <see cref="PolarUnderstory"/>; el constructor de la escena fija
/// dónde están la colonia y las guaridas.
/// </summary>
public class PolarHabitats : MonoBehaviour
{
    public Vector3 pebbleBeach = new Vector3(60f, 0f, -10f);
    public Vector3[] denSites = new Vector3[0];
    [Tooltip("Separación angular (grados) entre descansaderos de la orilla de la laguna.")]
    public float haulOutStep = 15f;

    public int HaulOuts { get; private set; }
    public int Holes { get; private set; }

    void Start() { Generate(); }

    public void Generate()
    {
        Material dark = Mat("Respiradero", new Color(0.05f, 0.12f, 0.18f)), pebble = Mat("Guijarro", new Color(0.42f, 0.42f, 0.45f));
        Material snow = Mat("Ventisquero", new Color(0.95f, 0.97f, 1f)), gravel = Mat("Grava", new Color(0.45f, 0.38f, 0.3f));
        Vector3 lagoon = MarineLife.Lagoon;
        int i = 0;
        for (float ang = 0f; ang < 360f; ang += haulOutStep, i++)
        {
            Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
            if (!Shore(lagoon, dir, out Vector3 shore)) continue;
            var h = new GameObject($"Descansadero_{i}"); h.transform.SetParent(transform, false); h.transform.position = shore + dir * 2f;
            Affordance.Put(h, AffordanceKind.HaulOut, 2); HaulOuts++;
            if (i % 2 != 0 || !Ice(shore + dir * 6f, out Vector3 holeAt)) continue;
            Transform hole = Box($"Respiradero_{i}", holeAt + Vector3.up * 0.02f, new Vector3(0.9f, 0.03f, 0.9f), dark);
            Affordance.Put(hole.gameObject, AffordanceKind.BreathingHole, 1); Holes++;
        }

        // Playa de guijarros de la colonia (el mar los trae muy despacio).
        var beach = new GameObject("Playa_guijarros"); beach.transform.SetParent(transform, false);
        beach.transform.position = Ice(pebbleBeach, out Vector3 pb) ? pb : pebbleBeach;
        var rnd = new System.Random(77);
        for (int k = 0; k < 16; k++)
        {
            Transform g = Box("Guijarro", beach.transform.position + new Vector3((float)rnd.NextDouble() * 3f - 1.5f, 0.05f, (float)rnd.NextDouble() * 3f - 1.5f), Vector3.one * 0.12f, pebble);
            g.SetParent(beach.transform, true);
        }
        Affordance.Put(beach, AffordanceKind.Pebble, 3, 40f, 0.0005f);

        int dens = 0;
        foreach (Vector3 d in denSites)
        {
            if (!Ice(d, out Vector3 at)) continue;
            Transform den = Box($"Guarida_nieve_{dens}", at + Vector3.up * 0.6f, new Vector3(3f, 1.2f, 3.5f), snow);
            Affordance.Put(den.gameObject, AffordanceKind.Den, 1); dens++;
        }

        for (int k = 0; k < 2; k++)
        {
            Vector3 at = lagoon + Quaternion.Euler(0f, 70f + k * 150f, 0f) * Vector3.forward * 16f; at.y = -3.2f;
            Transform bank = Box($"Banco_grava_{k}", at, new Vector3(6f, 0.6f, 4f), gravel);
            Affordance.Put(bank.gameObject, AffordanceKind.RubbingGravel, 1);
        }
        Debug.Log($"[Hábitats] {HaulOuts} descansaderos, {Holes} respiraderos, playa de guijarros, {dens} guaridas de nieve y 2 bancos de grava en la laguna.");
    }

    // Primer punto de hielo firme saliendo de la laguna en esa dirección (la orilla).
    static bool Shore(Vector3 from, Vector3 dir, out Vector3 at)
    {
        for (float r = 8f; r < 80f; r += 1.5f)
            if (Ice(from + dir * r, out at)) return true;
        at = Vector3.zero; return false;
    }

    static bool Ice(Vector3 p, out Vector3 at)
    {
        at = p;
        if (!Physics.Raycast(new Vector3(p.x, 8f, p.z), Vector3.down, out RaycastHit h, 14f, ~0, QueryTriggerInteraction.Ignore)) return false;
        if (h.point.y < -0.3f || h.collider.name != "Casquete") return false;   // hielo de verdad (no un árbol, un edificio o un animal)
        at = h.point; return true;
    }

    Transform Box(string name, Vector3 pos, Vector3 size, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; Destroy(g.GetComponent<Collider>());   // no estorban el paso: se usan, no se chocan
        g.transform.SetParent(transform, false); g.transform.position = pos; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }

    static Material Mat(string name, Color c)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
}
