using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// (2026-11-13, ronda 82) LAS CRÍAS DEL RECINTO DE LA CRÍA ANDAN SIN NAVMESH. Los recintos están en un piso del Árbol de los Campos, que gira:
/// las crías son hijas del recinto y giran con él, pero su NavMeshAgent vivía en el NavMesh del árbol, que el paquete vuelve a colocar a cada
/// paso de giro. El agente y el cuerpo se desfasaban, las crías quedaban en el borde del NavMesh («borde a 0,00 m», «deseada 0,00» o
/// velocidad 0 con camino) y salían «atascadas > 20 s» ronda tras ronda (Osezno_1/2, Polluelo_1/2/3, Cachorro_1/3, Cria_foca_1/2).
/// Un recinto es un rectángulo plano de 6–7 m con muros: la cría anda en las coordenadas del recinto (localPosition), hacia un sitio de
/// su suelo a 1,5–4 m, lejos de los muros y fuera de lo que abulta (la guarida, las rocas). Gira con el árbol sin hacer nada.
/// </summary>
public class PenWalker : MonoBehaviour
{
    /// <summary>Margen a los muros (m).</summary>
    public const float Margin = 0.6f;
    /// <summary>Lo que abulta más que esto (m, sobre el suelo del recinto) no se pisa.</summary>
    public const float TallObstacle = 0.28f;
    public static int Started, Arrived, Gave;

    Animal _a;
    Transform _pen;
    Vector2 _half;
    readonly List<Rect> _blocks = new List<Rect>();
    Vector3 _goal;           // en coordenadas del recinto
    float _until;
    bool _arrived;

    /// <summary>¿Va andando hacia su sitio?</summary>
    public bool Going { get; private set; }
    /// <summary>El sitio al que va, en el mundo.</summary>
    public Vector3 GoalWorld => _pen != null ? _pen.TransformPoint(_goal) : transform.position;

    /// <summary>El andador de la cría <paramref name="a"/> si vive en un recinto de la Cría (con su suelo «Suelo_recinto»); si no, null.
    /// Al crearlo apaga su NavMeshAgent.</summary>
    public static PenWalker For(Animal a)
    {
        if (a == null) return null;
        if (a.TryGetComponent(out PenWalker w)) return w._pen != null ? w : null;
        CriaFeeder f = a.GetComponentInParent<CriaFeeder>();
        if (f == null || a.transform.parent != f.transform) return null;
        Transform floor = f.transform.Find("Suelo_recinto");
        if (floor == null) return null;
        w = a.gameObject.AddComponent<PenWalker>();
        w.Init(a, f.transform, new Vector2(floor.localScale.x * 0.5f, floor.localScale.z * 0.5f));
        return w;
    }

    void Init(Animal a, Transform pen, Vector2 half)
    {
        _a = a; _pen = pen; _half = half;
        foreach (Transform c in pen)
        {
            if (c == transform || c.name == "Suelo_recinto" || c.name == "Muro" || c.name.StartsWith("Puerta") || c.name == "Puestos_cuidado" || c.GetComponentInChildren<Animal>() != null) continue;
            foreach (Renderer r in c.GetComponentsInChildren<Renderer>())
            {
                Bounds lb = r.localBounds; Vector3 mn = Vector3.positiveInfinity, mx = Vector3.negativeInfinity;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = lb.center + Vector3.Scale(lb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 p = pen.InverseTransformPoint(r.transform.TransformPoint(corner));
                    mn = Vector3.Min(mn, p); mx = Vector3.Max(mx, p);
                }
                if (mx.y > TallObstacle) _blocks.Add(Rect.MinMaxRect(mn.x, mn.z, mx.x, mx.z));
            }
        }
        if (a.nav != null) a.nav.enabled = false;
    }

    /// <summary>¿Vale <paramref name="local"/> (x, z del recinto) como sitio? Dentro del suelo con su margen y fuera de lo que abulta (con
    /// <paramref name="body"/> m de holgura).</summary>
    public static bool GoalOk(Vector2 local, Vector2 half, IList<Rect> blocks, float body)
    {
        if (Mathf.Abs(local.x) > half.x - Margin || Mathf.Abs(local.y) > half.y - Margin) return false;
        foreach (Rect b in blocks)
            if (local.x > b.xMin - body && local.x < b.xMax + body && local.y > b.yMin - body && local.y < b.yMax + body) return false;
        return true;
    }

    /// <summary>¿Cruza el tramo recto de <paramref name="from"/> a <paramref name="to"/> algo que abulta?</summary>
    public static bool Crosses(Vector2 from, Vector2 to, IList<Rect> blocks, float body)
    {
        for (int i = 1; i <= 8; i++) { Vector2 p = Vector2.Lerp(from, to, i / 8f); foreach (Rect b in blocks) if (p.x > b.xMin - body && p.x < b.xMax + body && p.y > b.yMin - body && p.y < b.yMax + body) return true; }
        return false;
    }

    float Body => 0.25f;

    /// <summary>Elige un sitio del suelo a 1,5–4 m (o lo más lejos posible si el recinto no da para tanto) y echa a andar. false si no hay.</summary>
    public bool PickGoal()
    {
        if (_pen == null) return false;
        Vector3 here = transform.localPosition; Vector2 h2 = new Vector2(here.x, here.z);
        for (int i = 0; i < 24; i++)
        {
            float rMin = i < 16 ? 1.5f : 0.8f;
            Vector2 p = h2 + Random.insideUnitCircle.normalized * Random.Range(rMin, 4f);
            if (!GoalOk(p, _half, _blocks, Body) || Crosses(h2, p, _blocks, Body)) continue;
            _goal = new Vector3(p.x, here.y, p.y); _until = Time.time + 12f; Going = true; _arrived = false; Started++;
            _a?.Loco?.SetGait(false, 1f);
            return true;
        }
        Gave++;
        return false;
    }

    /// <summary>¿Acaba de llegar (una vez por llegada)?</summary>
    public bool TakeArrival() { bool r = _arrived; _arrived = false; return r; }

    /// <summary>Se para donde está.</summary>
    public void Stop() { Going = false; }

    void Update()
    {
        if (_pen == null || _a == null) return;
        if (_a.nav != null && _a.nav.enabled) _a.nav.enabled = false;   // (OnCollisionEnter lo volvía a encender)
        if (!Going || _a.death || _a.asleep) return;
        Vector3 here = transform.localPosition, to = _goal; to.y = here.y;
        Vector3 d = to - here;
        float dist = d.magnitude;
        if (dist < 0.08f || Time.time > _until) { Going = false; _arrived = dist < 0.3f; if (_arrived) Arrived++; _a.Loco?.Idle(1f); return; }
        float v = Mathf.Clamp(_a.ActsPrep?.walk != null ? _a.ActsPrep.walk.navSpeed * 0.4f : 0.8f, 0.4f, 1.2f);
        transform.localPosition = here + d / dist * Mathf.Min(dist, v * Time.deltaTime);
        Vector3 flat = _pen.TransformDirection(new Vector3(d.x, 0f, d.z));
        if (flat.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat.normalized, _pen.up), 6f * Time.deltaTime);
    }

    /// <summary>(Pruebas) Los rectángulos de lo que abulta y el medio suelo.</summary>
    public IList<Rect> BlocksForTest => _blocks;
    public Vector2 HalfForTest => _half;
    public Vector3 GoalLocalForTest => _goal;
}
