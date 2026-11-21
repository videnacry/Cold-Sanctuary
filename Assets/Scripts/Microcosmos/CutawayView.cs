using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// LA VISTA DE CORTE de un edificio de microcosmos (docs/microcosmos/plan-microcosmos-realismo.md §3.3, P2; Berón 2026-11-18: vuelven los muros
/// y los techos, «con vista de corte cuando se está dentro»). Desde fuera el edificio se ve entero; cuando el jugador (o la cámara)
/// está DENTRO de su planta, el TECHO se desvanece y también los MUROS que quedan entre la cámara y el jugador, para ver el interior
/// sin perder la fachada del otro lado. «Desvanecer» = el renderer queda solo para sombras (<see cref="ShadowCastingMode.ShadowsOnly"/>):
/// la luz del interior no cambia y al salir todo vuelve como estaba.
/// Reutilizable: se pone en la raíz del edificio (<see cref="Ensure"/>, que reúne sus muros y techos por nombre o por forma con las
/// mismas reglas de <see cref="MicroBuildRules"/>) y lo pide la regla P2 de <see cref="MicroBuildRules.Validate"/>. La lógica es pura
/// (<see cref="Refresh"/>), para probarla sin cámara.
/// </summary>
public class CutawayView : MonoBehaviour
{
    [Tooltip("Muros del edificio (se ocultan los que quedan entre la cámara y el jugador cuando se está dentro).")]
    public List<Renderer> walls = new List<Renderer>();
    [Tooltip("Techos, cubiertas, faldones y chapiteles (se ocultan enteros cuando se está dentro).")]
    public List<Renderer> roofs = new List<Renderer>();
    [Tooltip("La planta del edificio en locales (x, z; y = del suelo al techo): «dentro» es dentro de esta caja.")]
    public Bounds interior;
    [Tooltip("Margen (m) alrededor de la planta que ya cuenta como dentro (la cámara pegada a la fachada, el umbral de la puerta).")]
    public float margin = 0.6f;
    [Tooltip("Quién mira: el jugador. Vacío = el objeto con la etiqueta Player.")]
    public Transform viewer;
    [Tooltip("Cada cuánto (s) se recalcula en juego (el PC de pruebas es lento: no hace falta cada fotograma).")]
    public float interval = 0.1f;
    [Tooltip("(2026-11-19) Corte FORZADO: el techo y los muros del lado de la cámara se desvanecen aunque nadie esté dentro (una foto desde arriba, una prueba).")]
    public bool force;
    /// <summary>Corte forzado en TODOS los edificios (fotos de las pruebas).</summary>
    public static bool ForceAll;

    readonly Dictionary<Renderer, ShadowCastingMode> _saved = new Dictionary<Renderer, ShadowCastingMode>();
    float _next;

    /// <summary>¿Está el jugador (o la cámara) dentro ahora?</summary>
    public bool InsideNow { get; private set; }
    /// <summary>Cuántos renderers están desvanecidos ahora (pruebas).</summary>
    public int HiddenCount => _saved.Count;

    /// <summary>¿Está <paramref name="r"/> desvanecido?</summary>
    public bool IsHidden(Renderer r) => r != null && _saved.ContainsKey(r);

    /// <summary>¿Está el punto <paramref name="world"/> dentro de la planta (con su margen)?</summary>
    public bool Inside(Vector3 world)
    {
        Vector3 p = transform.InverseTransformPoint(world);
        Bounds b = interior; b.Expand(new Vector3(margin * 2f, 0f, margin * 2f));
        return p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z && p.y >= b.min.y - margin && p.y <= b.max.y + margin;
    }

    /// <summary>
    /// Recalcula qué se ve con el jugador en <paramref name="viewerPos"/> y la cámara en <paramref name="camPos"/>: si alguno está
    /// dentro, se desvanecen los techos y los muros entre la cámara y el jugador (los que corta la línea cámara → jugador, o los que
    /// quedan del lado de la cámara); si no, todo se ve.
    /// </summary>
    public void Refresh(Vector3 viewerPos, Vector3 camPos)
    {
        InsideNow = force || ForceAll || Inside(viewerPos) || Inside(camPos);
        foreach (Renderer r in roofs) Set(r, InsideNow);
        Vector3 toCam = camPos - viewerPos; toCam.y = 0f;
        float dist = (camPos - viewerPos).magnitude;
        foreach (Renderer w in walls)
        {
            if (w == null) continue;
            bool hide = false;
            if (InsideNow)
            {
                Bounds b = w.bounds;
                if (dist > 1e-3f && b.IntersectRay(new Ray(camPos, (viewerPos - camPos) / dist), out float d) && d <= dist + 0.05f) hide = true;
                else
                {   // del lado de la cámara: el muro está más cerca de la cámara que del jugador, por delante del jugador mirando hacia ella
                    Vector3 c = b.center; c.y = viewerPos.y;
                    Vector3 toWall = c - viewerPos;
                    hide = toCam.sqrMagnitude > 1e-4f && Vector3.Dot(toWall, toCam) > 0f && Vector3.Dot(toWall.normalized, toCam.normalized) > 0.35f;
                }
            }
            Set(w, hide);
        }
    }

    /// <summary>Todo visible otra vez (al desactivarse, al salir del microcosmos).</summary>
    public void ShowAll()
    {
        foreach (var kv in _saved) if (kv.Key != null) kv.Key.shadowCastingMode = kv.Value;
        _saved.Clear();
        InsideNow = false;
    }

    void Set(Renderer r, bool hide)
    {
        if (r == null) return;
        if (hide) { if (!_saved.ContainsKey(r)) { _saved[r] = r.shadowCastingMode; r.shadowCastingMode = ShadowCastingMode.ShadowsOnly; } }
        else if (_saved.TryGetValue(r, out ShadowCastingMode m)) { r.shadowCastingMode = m; _saved.Remove(r); }
    }

    void LateUpdate()
    {
        if (Time.time < _next) return;
        _next = Time.time + Mathf.Max(0f, interval);
        if (viewer == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) viewer = p.transform; }
        Camera cam = Camera.main;
        if (viewer == null && cam == null) return;
        Vector3 v = viewer != null ? viewer.position : cam.transform.position;
        Refresh(v, cam != null ? cam.transform.position : v);
    }

    void OnDisable() => ShowAll();

    /// <summary>
    /// Pone (o rehace) la vista de corte en <paramref name="building"/>: reúne sus muros y techos (por nombre —«Muro_…», «Cubierta»,
    /// «Tejado»…— o por forma, como <see cref="MicroBuildRules.IsWall"/> e <see cref="MicroBuildRules.IsRoof"/>; sin muebles, puestos,
    /// gente ni cristal) y calcula su planta. Devuelve el componente.
    /// </summary>
    public static CutawayView Ensure(Transform building)
    {
        if (building == null) return null;
        CutawayView cv = building.GetComponent<CutawayView>() ?? building.gameObject.AddComponent<CutawayView>();
        cv.Collect();
        return cv;
    }

    /// <summary>Vuelve a reunir los muros y techos y la planta.</summary>
    public void Collect()
    {
        ShowAll();
        walls.Clear(); roofs.Clear();
        float gy = transform.position.y;
        bool any = false; Bounds local = default;
        foreach (Renderer r in MicroBuildRules.ShellRenderers(transform))
        {
            bool isRoof = MicroBuildRules.IsRoof(r, gy), isWall = !isRoof && MicroBuildRules.IsWall(r, gy);
            if (isRoof) roofs.Add(r); else if (isWall) walls.Add(r); else continue;
            Bounds wb = r.bounds;
            foreach (Vector3 corner in Corners(wb))
            {
                Vector3 p = transform.InverseTransformPoint(corner);
                if (!any) { local = new Bounds(p, Vector3.zero); any = true; } else local.Encapsulate(p);
            }
        }
        interior = any ? local : new Bounds(Vector3.up, Vector3.one * 2f);
        if (interior.min.y > 0f) interior.Encapsulate(new Vector3(interior.center.x, 0f, interior.center.z));   // del suelo al techo
    }

    static IEnumerable<Vector3> Corners(Bounds b)
    {
        for (int i = 0; i < 8; i++)
            yield return new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
    }
}
