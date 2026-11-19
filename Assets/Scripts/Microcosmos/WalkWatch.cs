using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// (2026-11-18, piloto4: MIC-93 «jornada 163, hogar 6») EL TOPE DE ANDAR DE UN INSECTO-HUMANO, A LA MEDIDA DEL CAMINO. Antes era un tope
/// fijo (60 s de jornada, 45 s de casa, × 2 en los encogidos): a 1,2 m/s son ~140 m, y en Hawthorne la casa, la nave, los bomberos y el
/// aeropuerto están a cientos de metros: en cada cambio de bloque, medio pueblo «no llegaba» y se le ponía allí. Ahora:
/// · el TIEMPO que se le da sale del camino del NavMesh (su largo entre su velocidad, × 3, + 30 s; nunca menos que el tope de antes);
/// · mientras se ACERCA, sigue andando (solo se rinde si lleva un rato sin ganar terreno, o pasado el triple del tiempo);
/// · el DESTINO se posa en el NavMesh si el punto pedido cae fuera (dentro del borde que deja el radio del agente junto al mueble):
///   antes llegaba al borde, a un palmo, y no «llegaba» nunca.
/// Cuando se rinde, dice POR QUÉ (los 20 primeros, en <see cref="Log"/>): tiempo, atasco, sin camino o camino parcial, con el agente
/// (fuera del NavMesh, apagado…) y el destino (fuera del NavMesh).
/// </summary>
public sealed class WalkWatch
{
    float _since = -1f, _budget, _best, _bestAt, _min;
    Vector3 _raw, _snap, _to; bool _hasSnap;

    /// <summary>Los 20 primeros teletransportes (quién, de dónde, a dónde, por qué): pruebas y registro.</summary>
    public static readonly List<string> Log = new List<string>();
    public const int LogMax = 20;

    /// <summary>Empieza de cero (otro bloque, otro destino, o ya llegó).</summary>
    public void Reset() { _since = -1f; }

    /// <summary>El destino que se anda: <paramref name="raw"/>, o su punto del NavMesh más cercano si raw cae fuera (≤ 3 m en planta —los muebles ×2 de Hawthorne dejan su
    /// punto de uso más hondo—, ≤ 0,6 m de altura: no la tapa de un mueble).</summary>
    public Vector3 Target(Vector3 raw)
    {
        if (_hasSnap && (raw - _raw).sqrMagnitude < 0.0625f) return _snap;
        _raw = raw; _snap = raw; _hasSnap = true;
        if (NavMesh.SamplePosition(raw, out NavMeshHit near, 0.15f, NavMesh.AllAreas)) return _snap;   // ya está en el NavMesh
        if (NavMesh.SamplePosition(raw, out NavMeshHit h, 3f, NavMesh.AllAreas) && Mathf.Abs(h.position.y - raw.y) <= 0.6f) _snap = h.position;
        return _snap;
    }

    /// <summary>(2026-11-19, ronda 3 de Hawthorne, MIC-93) ¿Está ya JUNTO al mueble? A ≤ <paramref name="reach"/> m en planta de su caja
    /// (sus colisores) y en su misma planta. Con los muebles ×2, el radio del agente lo deja a un metro del punto de uso, rozando el
    /// mueble, y a 0,2 m/s no «llegaba» nunca: se le teletransportaba. Junto al mueble ya puede usarlo.</summary>
    public static bool AtPiece(Transform agent, Transform piece, float reach)
    {
        if (agent == null || piece == null) return false;
        bool any = false; Bounds b = default;
        foreach (Collider c in piece.GetComponentsInChildren<Collider>())
        {
            if (!c.enabled || c.isTrigger) continue;
            if (!any) { b = c.bounds; any = true; } else b.Encapsulate(c.bounds);
        }
        if (!any) return false;
        Vector3 p = agent.position;
        if (p.y < b.min.y - 1.2f || p.y > b.max.y + 0.6f) return false;   // otra planta
        float dx = Mathf.Max(0f, Mathf.Max(b.min.x - p.x, p.x - b.max.x)), dz = Mathf.Max(0f, Mathf.Max(b.min.z - p.z, p.z - b.max.z));
        return dx * dx + dz * dz <= reach * reach;
    }

    /// <summary>¿Se rinde ya (y hay que ponerle allí)? <paramref name="minSecs"/>: el tope de antes, el mínimo.
    /// Si se rinde, <paramref name="why"/> dice por qué.</summary>
    public bool GiveUp(Transform t, Vector3 to, float speed, float minSecs, out string why)
    {
        why = null;
        Vector3 d = to - t.position; d.y = 0f; float dist = d.magnitude;
        if (_since < 0f || (to - _to).sqrMagnitude > 1f)   // otro destino (de la bandeja a la mesa): otro tiempo
        {
            _to = to; _since = Time.time; _min = minSecs; _best = dist; _bestAt = Time.time;
            _budget = Mathf.Max(minSecs, 3f * PathLength(t, to) / Mathf.Max(0.1f, speed) + 30f);
        }
        if (dist < _best - 0.3f) { _best = dist; _bestAt = Time.time; }
        float now = Time.time, stall = Mathf.Max(20f, _min * 0.5f);
        bool stuck = now - _bestAt > stall, late = now - _since > _budget && now - _bestAt > 5f, cap = now - _since > 3f * _budget;
        if (!stuck && !late && !cap) return false;
        why = (stuck ? $"atasco ({now - _bestAt:0} s sin acercarse)" : cap ? $"tope ({3f * _budget:0} s)" : $"tiempo ({_budget:0} s)") + "; " + Diagnose(t, to);
        return true;
    }

    /// <summary>Lo apunta (los 20 primeros) y lo avisa en la consola.</summary>
    public static void Record(string tag, string who, Vector3 from, Vector3 to, string target, string why)
    {
        if (Log.Count >= LogMax) return;
        string line = $"«{who}» de {Fmt(from)} a {Fmt(to)} («{target}», a {Vector3.Distance(from, to):0} m): {why}";
        Log.Add(line);
        Debug.LogWarning($"[{tag}] teletransporte {Log.Count}: {line}");
    }

    static string Fmt(Vector3 p) => $"({p.x:0},{p.y:0.0},{p.z:0})";

    static int Mask(Transform t) => t.TryGetComponent(out NavMeshAgent ag) ? ag.areaMask : NavMesh.AllAreas;

    static Vector3 From(Transform t) => t.TryGetComponent(out NavMeshAgent ag) && ag.enabled && ag.isOnNavMesh ? ag.nextPosition : t.position;

    /// <summary>El largo del camino del NavMesh (o la recta × 1,5 si no hay camino completo).</summary>
    public static float PathLength(Transform t, Vector3 to)
    {
        var p = new NavMeshPath();
        Vector3 from = From(t);
        if (NavMesh.CalculatePath(from, to, Mask(t), p) && p.status == NavMeshPathStatus.PathComplete && p.corners.Length > 1) return Length(p);
        Vector3 d = to - from; d.y = 0f; return d.magnitude * 1.5f;
    }

    static float Length(NavMeshPath p) { float s = 0f; for (int i = 1; i < p.corners.Length; i++) s += Vector3.Distance(p.corners[i - 1], p.corners[i]); return s; }

    /// <summary>Por qué no llega: el agente, el destino y el camino.</summary>
    public static string Diagnose(Transform t, Vector3 to)
    {
        string a;
        if (!t.TryGetComponent(out NavMeshAgent ag)) a = "sin agente (en recta)";
        else if (!ag.enabled) a = "agente apagado (¿a mano?)";
        else if (!ag.isOnNavMesh) a = "agente FUERA del NavMesh";
        else a = $"agente a {ag.velocity.magnitude:0.0} m/s, {(ag.pathPending ? "camino pendiente" : ag.hasPath ? $"camino {ag.pathStatus}, faltan {ag.remainingDistance:0} m" : "sin camino en curso")}";
        string g = NavMesh.SamplePosition(to, out NavMeshHit h, 1f, NavMesh.AllAreas) ? $"destino en el NavMesh (a {Vector3.Distance(h.position, to):0.0} m)" : "destino FUERA del NavMesh (a más de 1 m)";
        var p = new NavMeshPath();
        Vector3 from = From(t);
        bool ok = NavMesh.CalculatePath(from, to, Mask(t), p);
        string c = !ok ? "SIN CAMINO" : p.status == NavMeshPathStatus.PathComplete ? $"camino completo de {Length(p):0} m"
                 : p.status == NavMeshPathStatus.PathPartial ? $"camino PARCIAL (acaba a {(p.corners.Length > 0 ? Vector3.Distance(p.corners[p.corners.Length - 1], to) : -1f):0.0} m)" : "camino inválido";
        return $"{c}; {a}; {g}";
    }
}
