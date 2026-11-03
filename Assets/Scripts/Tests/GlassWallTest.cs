using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LAS PAREDES DE CRISTAL (Berón 2026-11-03; docs/hospital.md §Paredes de cristal):
/// 1. las salas que deben aislarse de los hospitales del santuario tienen su sala de cristal: los 4 quirófanos, la UCI, los 4
///    aislamientos y su esclusa (hospital de personas) y los 4 quirófanos veterinarios, la sala fría de los pingüinos y el
///    aislamiento veterinario;
/// 2. el cristal es transparente (no tapa la vista) y cada sala tiene su puerta (hueco de ≥ 1,8 m);
/// 3. GUARDAN LOS 2 m: ningún mueble (de dentro ni de fuera) a menos de 2 m de un paño (FurnitureSpacing, en todo el hospital);
/// 4. las reglas de los microcosmos (MicroBuildRules) dejan pasar el cristal (no es «muro») pero le miden el paso de 2 m.
/// </summary>
public class GlassWallTest : MonoBehaviour, ITestUnit
{
    public int Group => 84;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<GlassWallTest>() == null) tr.gameObject.AddComponent<GlassWallTest>();
    }

    static readonly string[] Rooms =
    {
        "Quirofano_1", "Quirofano_2", "Quirofano_3", "Quirofano_4", "UCI", "Aislamiento_1", "Aislamiento_2", "Aislamiento_3", "Aislamiento_4", "Esclusa_aislamiento",
        "Quirofano_vet_1", "Quirofano_vet_2", "Quirofano_vet_3", "Quirofano_vet_4", "Sala_fria_pinguinos", "Aislamiento_veterinario",
    };

    public IEnumerator Run()
    {
        TestProbe.Begin("Paredes de cristal en las salas aisladas de los hospitales: transparentes, con puerta y con sus 2 m");
        GameObject hosp = GameObject.Find("Infirmary_Area");
        TestProbe.Check("hay hospital", hosp != null);
        if (hosp == null) { TestProbe.End(); yield break; }
        var rooms = hosp.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Sala_cristal_")).ToDictionary(t => t.name.Substring("Sala_cristal_".Length), t => t);
        var lack = Rooms.Where(r => !rooms.ContainsKey(r)).ToList();
        TestProbe.Check("las 16 salas que deben aislarse (quirófanos, UCI, aislamientos, esclusa, sala fría) son de cristal", lack.Count == 0, lack.Count == 0 ? $"{rooms.Count} salas" : "faltan " + string.Join(", ", lack));

        // ── 2. TRANSPARENTE Y CON PUERTA ──
        int clear = 0, doors = 0, panes = 0;
        foreach (var kv in rooms)
        {
            var ps = kv.Value.GetComponentsInChildren<GlassWall>().Where(g => g.name.StartsWith(GlassWall.Prefix)).ToList();
            panes += ps.Count;
            if (ps.Count > 0 && ps.All(g => g.GetComponent<Renderer>().sharedMaterial.color.a < 0.5f)) clear++;
            if (kv.Value.Find("Dintel_cristal") != null || kv.Key == "Esclusa_aislamiento" && ps.Count >= 3) doors++;
        }
        TestProbe.Check("el cristal es transparente (alfa < 0,5) en todas", clear == rooms.Count, $"{clear}/{rooms.Count} salas, {panes} paños");
        TestProbe.Check("cada sala tiene su puerta (hueco de 1,8 m con dintel)", doors == rooms.Count, $"{doors}/{rooms.Count}");

        // ── 3. LOS 2 m ──
        var bad = FurnitureSpacing.Violations(hosp.transform).Where(v => v.a.name.StartsWith(GlassWall.Prefix) || v.b.name.StartsWith(GlassWall.Prefix)).ToList();
        TestProbe.Check("ningún mueble a menos de 2 m de un cristal (dentro y fuera de su sala)", bad.Count == 0, bad.Count == 0 ? "0 pares" : $"{bad.Count} pares: {FurnitureSpacing.Worst(bad)}");
        // Dentro de cada sala, lo que hay: al menos 2 m hasta su cristal por los cuatro lados.
        float worst = float.MaxValue; string worstRoom = "";
        foreach (var kv in rooms)
        {
            Transform room = kv.Value;
            var ps = room.GetComponentsInChildren<GlassWall>().Where(g => g.name.StartsWith(GlassWall.Prefix)).Select(g => g.GetComponent<Renderer>().bounds).ToList();
            if (ps.Count == 0) continue;
            Bounds inside = ps[0]; foreach (Bounds b in ps) inside.Encapsulate(b);
            foreach (Transform f in room.parent.GetComponentsInChildren<Transform>())
            {
                if (!FurnitureSpacing.IsFurniture(f) || f.name.StartsWith(GlassWall.Prefix)) continue;
                Bounds fb = Box(f); if (fb.size == Vector3.zero) continue;
                if (fb.center.x < inside.min.x || fb.center.x > inside.max.x || fb.center.z < inside.min.z || fb.center.z > inside.max.z) continue;
                if (Mathf.Abs(fb.min.y - inside.min.y) > 1.5f) continue;
                foreach (Bounds pb in ps) { float g = Gap(fb, pb); if (g < worst) { worst = g; worstRoom = $"«{f.name}» en {kv.Key} a {g:0.00} m"; } }
            }
        }
        TestProbe.Check("dentro de cada sala, los muebles a ≥ 2 m de su cristal", worst >= 1.99f, worst == float.MaxValue ? "salas vacías" : $"el más justo: {worstRoom}");

        // ── 4. LAS REGLAS DE LOS MICROCOSMOS ──
        var scope = new GameObject("Prueba_reglas_cristal").transform;
        scope.position = new Vector3(0f, -500f, 0f);
        GlassWall.Box(scope, "prueba", Vector3.zero, new Vector2(6f, 6f), 0f, 0);
        Transform piece = new GameObject("Cama_prueba").transform; piece.SetParent(scope, false); piece.localPosition = new Vector3(0f, 0f, 1.6f);
        piece.gameObject.AddComponent<MicroPiece>();
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube); Destroy(cube.GetComponent<Collider>()); cube.transform.SetParent(piece, false); cube.transform.localScale = new Vector3(1f, 0.6f, 2f); cube.transform.localPosition = Vector3.up * 0.3f;
        yield return null;
        var r1 = MicroBuildRules.Validate(scope, "prueba de cristal", needs: false, log: false);
        bool noWall = !r1.Any(s => s.Contains("muro") || s.Contains("techo")), passage = r1.Any(s => s.Contains("paso"));
        piece.localPosition = Vector3.zero; cube.transform.localScale = new Vector3(1f, 0.6f, 1.6f);
        Physics.SyncTransforms();
        var r2 = MicroBuildRules.Validate(scope, "prueba de cristal", needs: false, log: false);
        TestProbe.Check("MicroBuildRules: el cristal no cuenta como muro, pero un mueble a < 2 m de él sí incumple el paso; a 2 m, nada", noWall && passage && r2.Count == 0,
            $"cama a ~0,6 m: [{string.Join("; ", r1.Take(3))}] · en el centro (2,2 m): [{string.Join("; ", r2.Take(3))}]");
        Destroy(scope.gameObject);
        TestProbe.End();
    }

    static Bounds Box(Transform t)
    {
        Bounds b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r.GetComponent<TMPro.TMP_Text>() != null) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return any ? b : new Bounds(t.position, Vector3.zero);
    }

    static float Gap(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
