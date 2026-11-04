using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// RING DE ALMOHADAS de la arboleda residencial (Berón 2026-11-04: «se deben crear guerras de almohadas en un ring que se debe crear en
/// la residencia, el ring debe ser creado con la regla para que tenga las dimensiones necesarias para que se enfrenten todos los
/// residentes del santuario y debe estar lleno de almohadas y la base es una cama colosal»; docs/guerra-de-almohadas.md).
///
/// LA REGLA DEL TAMAÑO (manual J15): cada luchador necesita una CELDA de <see cref="CellSize"/> × <see cref="CellSize"/> m. Un brazo
/// mide ~0,7 m y una almohada ~0,6: cada uno barre ~1,3 m a su alrededor, y dos que se pegan almohadazos de frente quedan a ~2,5 m
/// (la celda). El colchón es una rejilla de n × n celdas con n = ⌈√residentes⌉: caben n² luchadores a la vez (<see cref="Capacity"/>).
/// Alrededor, un BORDE BLANDO de <see cref="Bumper"/> m (la cama colosal) y 2 m de paso (manual S2) hasta el borde de la plataforma
/// y hasta la pista de alfombras. Nada de escalones: el colchón sube 0,2 m sobre la plataforma.
/// Si llegan más residentes de los que caben, <see cref="Validate"/> falla (y al reconstruir la escena el ring crece solo).
/// </summary>
public class PillowRing : MonoBehaviour
{
    /// <summary>Lado de la celda de cada luchador (m): ~1,3 m de barrido por luchador (brazo + almohada), dos de frente a 2,5 m.</summary>
    public const float CellSize = 2.5f;
    /// <summary>Borde blando (acolchado) de la cama colosal alrededor del colchón (m).</summary>
    public const float Bumper = 0.6f;
    /// <summary>Paso libre alrededor de la cama y hasta la pista de alfombras (m; manual S2).</summary>
    public const float Walkway = 2f;
    /// <summary>Almohadas sueltas por luchador (una en la mano y otra de repuesto en el colchón).</summary>
    public const int PillowsPerFighter = 2;

    [Tooltip("Celdas por lado del colchón (n): caben n × n luchadores.")]
    public int cells = 1;
    [Tooltip("Para cuántos residentes se construyó (los de la arboleda al construir la escena).")]
    public int builtFor;
    [Tooltip("El colchón de la cama colosal (su cara de arriba es el suelo del ring).")]
    public Transform mattress;
    [Tooltip("Las almohadas sueltas del ring (se cogen al empezar la guerra y se dejan al acabar).")]
    public Transform pillows;

    public static readonly List<PillowRing> All = new List<PillowRing>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    /// <summary>Celdas por lado para <paramref name="fighters"/> luchadores: ⌈√n⌉ (al menos 1).</summary>
    public static int CellsFor(int fighters) => Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, fighters)) - 1e-4f));
    /// <summary>Lado del colchón (m) para <paramref name="fighters"/> luchadores.</summary>
    public static float SideFor(int fighters) => CellsFor(fighters) * CellSize;
    /// <summary>Luchadores que caben a la vez.</summary>
    public int Capacity => cells * cells;
    public float Side => cells * CellSize;

    /// <summary>El ring de una escena (null si no tiene).</summary>
    public static PillowRing In(Scene s) { foreach (PillowRing r in All) if (r != null && r.gameObject.scene == s) return r; return null; }

    /// <summary>El centro de la celda <paramref name="seat"/> sobre el colchón (mundo; a la altura de los pies).</summary>
    public Vector3 Spot(int seat)
    {
        int n = Mathf.Max(1, cells), i = ((seat % (n * n)) + n * n) % (n * n);
        float x = (i % n - (n - 1) * 0.5f) * CellSize, z = (i / n - (n - 1) * 0.5f) * CellSize;
        if (mattress == null) return transform.TransformPoint(new Vector3(x, 0f, z));
        return mattress.parent.TransformPoint(mattress.localPosition + new Vector3(x, mattress.localScale.y * 0.5f, z));   // (el colchón cuelga del ring, sin girar)
    }

    /// <summary>¿Está el punto encima del colchón (con un margen)?</summary>
    public bool OnMattress(Vector3 world, float margin = 0.5f)
    {
        if (mattress == null) return false;
        Vector3 l = mattress.parent.InverseTransformPoint(world) - mattress.localPosition;
        float h = Side * 0.5f + margin;
        return Mathf.Abs(l.x) <= h && Mathf.Abs(l.z) <= h && l.y > -0.5f && l.y < 3f;
    }

    /// <summary>Los RESIDENTES de la escena: los dueños de las casas del árbol y sus parejas (los que duermen en la arboleda).</summary>
    public static int Residents(Scene s)
    {
        int n = 0;
        foreach (Bedroom b in Bedroom.All)
        {
            if (b == null || b.gameObject.scene != s) continue;
            if (!string.IsNullOrEmpty(b.owner)) n++;
            if (!string.IsNullOrEmpty(b.partner)) n++;
        }
        return n;
    }

    /// <summary>LA REGLA (manual J15): el ring da cabida a TODOS a la vez: a los residentes de la arboleda y a todo el personal del
    /// santuario (con la Magnate). Falla si no caben, si el colchón no mide lo que manda la regla o si faltan almohadas.</summary>
    public bool Validate(out string why)
    {
        Scene s = gameObject.scene;
        int residents = Residents(s);
        int members = 1;   // la Magnate
        if (WorkdayDirector.Instance != null) members += WorkdayDirector.Instance.staff.Length;
        int need = Mathf.Max(residents, members);
        int loose = pillows != null ? pillows.childCount : 0;
        var bad = new List<string>();
        if (Capacity < need) bad.Add($"caben {Capacity} y son {need} (residentes {residents}, personal {members}): reconstruir la escena para que crezca a {CellsFor(need)} × {CellsFor(need)} celdas");
        if (cells != CellsFor(Mathf.Max(1, builtFor))) bad.Add($"el colchón tiene {cells} celdas por lado y la regla pide {CellsFor(builtFor)} para {builtFor}");
        if (mattress != null && Mathf.Abs(mattress.localScale.x - Side) > 0.05f) bad.Add($"el colchón mide {mattress.localScale.x:0.0} m y la regla pide {Side:0.0}");
        if (loose < PillowsPerFighter * Capacity) bad.Add($"{loose} almohadas sueltas, menos de {PillowsPerFighter} por luchador ({PillowsPerFighter * Capacity})");
        why = bad.Count == 0 ? $"ring de {cells} × {cells} celdas ({Side:0.0} × {Side:0.0} m): caben {Capacity}, son {need}; {loose} almohadas" : string.Join("; ", bad);
        return bad.Count == 0;
    }

    // ── En el horario libre (etiqueta de actividad: ocio para todos a la vez) ─────────────────────────────────────────────

    public const string ActivityId = "almohadas";
    /// <summary>Día de la semana (0–6) y franja (índice de <see cref="LeisureRoster.SlotHours"/>) de la guerra: el sábado a las 20 h.</summary>
    public const int Day = 5, Slot = 3;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (_registered) return; _registered = true;
        LeisureTags.Providers.Add((gr, sites) =>
        {
            if (gr.micro) return;
            PillowRing ring = In(gr.scene);
            if (ring == null) return;
            var act = new LeisureRoster.Activity(ActivityId, "Guerra de almohadas en el ring", LeisureRoster.Kind.Ocio, "el ring de almohadas", ring.Capacity, "almohadas")
            { together = true, togetherDay = Day, togetherSlot = Slot };
            gr.catalog.Add(act);
            gr.venues[act] = new LeisureDirector.Venue { title = "el ring de almohadas", anchor = ring.transform, capacity = ring.Capacity, index = gr.catalog.Count - 1 };
        });
    }
    static bool _registered;
}
