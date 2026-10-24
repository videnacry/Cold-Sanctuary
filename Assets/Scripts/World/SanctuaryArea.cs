using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A zone inside Cold Sanctuary. Each area has:
///   - Entry requirements (minimum stats to be placed here)
///   - A set of tasks characters do autonomously
///   - Spawn points where characters are positioned on arrival
///   - A resident list maintained at runtime
///
/// SanctuaryDirector reads progressionTier to determine promotion order.
/// </summary>
public class SanctuaryArea : MonoBehaviour
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public SanctuaryAreaType areaType;
    public string displayName;
    [TextArea(2, 3)]
    public string description;

    [Header("Progression")]
    [Tooltip("Lower number = earlier in the progression chain. " +
             "Director promotes characters to the next-lowest tier they qualify for.")]
    public int progressionTier;

    // ── Entry requirements ────────────────────────────────────────────────────

    [Header("Entry Requirements (0 = no requirement)")]
    [Tooltip("Minimum physicalResistance / strength.")]
    public float minStrength;

    [Tooltip("Minimum satisfaction level (0–1).")]
    public float minSatisfaction;

    [Tooltip("Minimum observationRadius (in world units).")]
    public float minObservation;

    // ── Tasks ─────────────────────────────────────────────────────────────────

    [Header("Tasks")]
    [Tooltip("Pool of tasks available in this area. Characters pick randomly from tasks " +
             "whose minProgressionLevel they meet.")]
    public AreaTask[] availableTasks;

    // ── Spawn points ──────────────────────────────────────────────────────────

    [Header("Spawn Points")]
    [Tooltip("Where characters are placed when assigned here. " +
             "Director cycles through these so characters spread out.")]
    public Transform[] spawnPoints;

    // ── Residents (runtime) ───────────────────────────────────────────────────

    readonly List<WorldCharacter> _residents = new List<WorldCharacter>();

    /// <summary>Characters currently assigned to this area.</summary>
    public IReadOnlyList<WorldCharacter> Residents => _residents;

    public void AddResident(WorldCharacter c)    { if (!_residents.Contains(c)) _residents.Add(c); }
    public void RemoveResident(WorldCharacter c) { _residents.Remove(c); }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the given stat values meet this area's entry requirements.
    /// </summary>
    public bool MeetsRequirements(float strength, float satisfaction, float observation)
        => strength     >= minStrength
        && satisfaction >= minSatisfaction
        && observation  >= minObservation;

    /// <summary>
    /// Returns a random task appropriate for a character at the given progression level.
    /// Falls back to any task if none match the level filter.
    /// </summary>
    public AreaTask GetTask(int progressionLevel)
    {
        if (availableTasks == null || availableTasks.Length == 0) return null;

        // Collect tasks the character is qualified for
        var eligible = System.Array.FindAll(availableTasks,
            t => t.minProgressionLevel <= progressionLevel && !t.requiresVehicle);

        if (eligible.Length == 0)
            eligible = availableTasks;   // fallback: any task

        return eligible[Random.Range(0, eligible.Length)];
    }

    /// <summary>
    /// World-space position of the spawn point at the given index (round-robin safe).
    /// Falls back to this transform's position if no spawn points are configured.
    /// </summary>
    public Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints[index % spawnPoints.Length].position;
        // Sin puntos de aparición: alrededor del centro del área y SOBRE el suelo. Antes devolvía el pivote del área (y = 0),
        // y quien era asignado (el jugador, los compañeros) quedaba con medio cuerpo bajo tierra.
        float ang = index * 2.399f;   // ángulo áureo: no se apilan
        Vector3 p = transform.position + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * (1.2f + 0.4f * (index % 4));
        if (Physics.Raycast(p + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 20f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point + Vector3.up * 1.1f;
        return p + Vector3.up * 1.1f;
    }

    // ── ¿Dentro del área? ────────────────────────────────────────────────────

    float _top = -1f;

    /// <summary>¿Está <paramref name="p"/> dentro del área? Cuenta la PLANTA entera (la alfombra «Mat» más
    /// <paramref name="margin"/>) y en altura todo lo que el área tiene encima: entreplantas (camas, mesas, máquinas a
    /// 4 m) y pisos superiores propios (la planta veterinaria del hospital). Un área apilada que sea OTRA área (el dojo
    /// sobre el gimnasio) no cuenta: para el puesto de trabajo se usa <see cref="InBuilding"/>.</summary>
    public bool Contains(Vector3 p, float margin = 1f)
    {
        Transform mat = transform.Find("Mat");
        Vector3 half = mat != null ? mat.localScale * 0.5f : new Vector3(12f, 0f, 12f);
        Vector3 l = transform.InverseTransformPoint(p);
        float ring = RingReach();
        bool onMat = Mathf.Abs(l.x) <= half.x + margin && Mathf.Abs(l.z) <= half.z + margin;
        if (!onMat && (ring <= 0f || l.y < _ringMinY - 0.5f || Mathf.Abs(l.x) > half.x + margin + ring || Mathf.Abs(l.z) > half.z + margin + ring)) return false;   // fuera de la planta: solo sobre el anillo
        return l.y > -2f && l.y < Top() + 3f;
    }

    // ANILLOS (RingDeck, Berón 2026-10-23): las plataformas hijas en anillo vuelan ~8 m fuera del borde y son del área.
    float _ringReach = -1f, _ringMinY = float.MaxValue;
    float RingReach()
    {
        if (_ringReach >= 0f) return _ringReach;
        _ringReach = 0f;
        Transform mat = transform.Find("Mat");
        Vector3 half = mat != null ? mat.localScale * 0.5f : new Vector3(12f, 0f, 12f);
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("Anillo_")) continue;
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true))
            {
                Vector3 c = transform.InverseTransformPoint(r.bounds.center), e = r.bounds.extents;
                _ringMinY = Mathf.Min(_ringMinY, transform.InverseTransformPoint(r.bounds.min).y);
                _ringReach = Mathf.Max(_ringReach, Mathf.Abs(c.x) + e.x - half.x, Mathf.Abs(c.z) + e.z - half.z);
            }
        }
        return _ringReach;
    }

    /// <summary>¿Está <paramref name="p"/> en el EDIFICIO de esta área? Es toda la columna de plataformas apiladas sobre su
    /// planta: el piso de abajo y el de arriba aunque sean otra área (gimnasio + dojo, yoga + ballet), sus entreplantas y la
    /// pista de alfombras de arriba. Berón 2026-10-23: quien
    /// trabaja en un área puede moverse por todo su edificio; el suelo de debajo NO cuenta.</summary>
    public bool InBuilding(Vector3 p)
    {
        float lo = float.MaxValue, hi = float.MinValue;
        foreach (SanctuaryArea a in Building) { lo = Mathf.Min(lo, a.transform.position.y); hi = Mathf.Max(hi, a.transform.position.y + a.Top()); }
        if (p.y < lo - 2f || p.y > hi + 15f) return false;   // del piso de abajo a la pista de arriba (+11 m y su gente)
        foreach (SanctuaryArea a in Building) if (a.InFootprint(p, 1.5f)) return true;
        return false;
    }

    List<SanctuaryArea> _building;

    /// <summary>Las áreas del mismo edificio: esta y las apiladas encima o debajo (sus plantas se solapan).</summary>
    public List<SanctuaryArea> Building
    {
        get
        {
            if (_building != null) return _building;
            _building = new List<SanctuaryArea> { this };
            foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
                if (a != this && Mathf.Abs(a.transform.position.y - transform.position.y) > 3f && (InFootprint(a.transform.position, 0f, false) || a.InFootprint(transform.position, 0f, false)))   // sin el anillo: no une edificios vecinos
                    _building.Add(a);
            return _building;
        }
    }

    bool InFootprint(Vector3 p, float margin, bool withRing = true)
    {
        Transform mat = transform.Find("Mat");
        Vector3 half = mat != null ? mat.localScale * 0.5f : new Vector3(12f, 0f, 12f);
        Vector3 l = transform.InverseTransformPoint(p);
        float ring = withRing ? RingReach() : 0f;
        return Mathf.Abs(l.x) <= half.x + margin + ring && Mathf.Abs(l.z) <= half.z + margin + ring;
    }

    // Altura local de lo más alto del área (sin seres ni otras áreas apiladas); se calcula una vez.
    float Top()
    {
        if (_top >= 0f) return _top;
        _top = 4f;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.GetComponentInParent<Anima>() != null || r.GetComponentInParent<SanctuaryArea>() != this) continue;
            _top = Mathf.Max(_top, transform.InverseTransformPoint(r.bounds.max).y);
        }
        return _top;
    }
}
