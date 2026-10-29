using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ANDAR DE VERDAD hacia un punto (para personajes que se mueven por código: Linduarda, la Magnate…). Antes avanzaban en
/// LÍNEA RECTA ignorando el terreno: cruzaban el agua entre islas «flotando» y atravesaban árboles (parecía que volaban).
/// Ahora siguen el camino del NavMesh (incluidos los enlaces de nado) y se pegan al suelo; sobre el agua, a la altura del
/// agua (nadando). Recalcula el camino cada poco o cuando el destino se mueve.
/// </summary>
[DisallowMultipleComponent]
public class PathMover : MonoBehaviour
{
    NavMeshPath _path;
    int _corner;
    Vector3 _goal = new Vector3(float.NaN, 0f, 0f);
    float _repathAt, _cliffLog, _cliffSince;
    const float WaterY = -0.25f;

    public static PathMover Of(Component c) => c.GetOrAdd<PathMover>();

    /// <summary>Cuándo dio su último paso (Time.time). Mientras algo lo conduce con PathMover, nadie más debe moverlo: el
    /// «merodear» de un anima libre lo empujaba a la vez hacia otro lado y daban vueltas en círculo (Berón 2026-10-29).</summary>
    public float LastStep { get; private set; } = -999f;
    public static bool DrivenRecently(GameObject g, float within = 1f) => g != null && g.TryGetComponent(out PathMover pm) && Time.time - pm.LastStep < within;
    /// <summary>Cuántos pasos lleva dados (para que quien conduce sepa si otro le ha dado un paso entre medias).</summary>
    public int Steps { get; private set; }

    /// <summary>Olvida el camino calculado: el próximo paso recalcula hacia el destino nuevo (la orquesta lo llama al empezar
    /// cada «ir a»; si no, con un destino a menos de 3 m del anterior seguía hasta el viejo y esperaba 4 s para corregir).</summary>
    public void Restart() { _goal = new Vector3(float.NaN, 0f, 0f); _cliffSince = 0f; }

    /// <summary>Dónde queda el pivote con los pies en el suelo que haya en <paramref name="p"/> (p = el punto de los pies,
    /// p. ej. uno del NavMesh): sobre lo más alto a un escalón de ahí.</summary>
    public Vector3 OnGround(Vector3 p) { p.y = GroundY(p, p.y + PivotHeight); return p; }

    /// <summary>Un paso hacia <paramref name="target"/> a <paramref name="speed"/> m/s. Devuelve la distancia que queda.</summary>
    public float StepToward(Vector3 target, float speed)
    {
        LastStep = Time.time; Steps++;
        // Otro piso u otro edificio (ya no hay escaleras ni puentes): por la cadena de alfombras voladoras. Así lo hace
        // cualquiera que ande con PathMover (la jornada, Linduarda, la que inicia, los guardias…).
        Vector3 final = target;
        if (CarpetPad.Route(gameObject, ref target)) return Flat(final - transform.position);
        if (_path == null) _path = new NavMeshPath();
        // Mientras cruza el agua (fuera del NavMesh) NO recalcula: el camino nuevo saldría de la orilla de atrás y daría la
        // vuelta (se quedaba yendo y viniendo en la orilla). Recalcula al pisar tierra si el destino se movió o pasó el tiempo.
        bool onMesh = NavMesh.SamplePosition(transform.position, out _, 1.5f, NavMesh.AllAreas);
        if (float.IsNaN(_goal.x) || (onMesh && ((target - _goal).sqrMagnitude > 9f || Time.time >= _repathAt))) Repath(target);

        // NADAR (Berón 2026-10-29): en agua más honda que el pecho, el mismo cuerpo de nado que el jugador (horizontal, a
        // braza, a flote), más despacio; en agua baja vadea andando. SwimBody decide por la profundidad donde está.
        SwimBody swim = SwimBody.Of(this);
        bool swimming = swim.UpdateWater();
        if (swimming) speed = Mathf.Min(speed, swim.npcSwimSpeed);

        Vector3 pos = transform.position;
        Vector3 next = target;
        if (_path.status != NavMeshPathStatus.PathInvalid && _path.corners.Length > 1)
        {
            while (_corner < _path.corners.Length - 1 && Flat(_path.corners[_corner] - pos) < 0.35f) _corner++;
            next = _path.corners[Mathf.Min(_corner, _path.corners.Length - 1)];
        }
        Vector3 d = next - pos; d.y = 0f;
        float step = Mathf.Min(speed * Time.deltaTime, d.magnitude);
        // Sin camino (destino inalcanzable por el NavMesh): no seguir en línea recta si eso le saca del NavMesh por un
        // borde (antes Linduarda salía de la enfermería elevada atravesando la barandilla y caía 6 m).
        bool noPath = _path.status == NavMeshPathStatus.PathInvalid || _path.corners.Length <= 1;
        if (noPath && onMesh && d.sqrMagnitude > 1e-6f && !StaysOnMesh(pos + d.normalized * 0.6f, pos.y - PivotHeight)) return Flat(final - pos);
        if (d.sqrMagnitude > 1e-6f)
        {
            pos += d.normalized * step;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 8f * Time.deltaTime);
        }
        if (swimming)
        {
            pos.y = Mathf.MoveTowards(transform.position.y, swim.FloatPivotY, 1.5f * Time.deltaTime);   // a flote, sin «acantilados»
            _cliffSince = 0f;
            transform.position = pos;
            return Flat(final - pos);
        }
        pos.y = GroundY(pos, transform.position.y);
        // ACANTILADO: si el paso le haría caer más de 1,2 m (borde de una plataforma elevada), no lo da. Las escaleras bajan
        // de peldaño en peldaño; el NavMesh no ve las barandillas invisibles y la ruta a veces pasaba por el borde.
        // ACANTILADO, salvo que el camino completo baje por ahí y lleve un rato parado al borde: sin barandillas, desde un
        // anillo o una plataforma hija se SALTA al piso de abajo (Berón 2026-10-27: «que sea fácil saltar»), como haría cualquiera.
        bool jump = _path.status == NavMeshPathStatus.PathComplete && next.y < transform.position.y - 1f && _cliffSince > 0f && Time.time - _cliffSince > 2.5f && transform.position.y - pos.y < 6f;
        if (pos.y >= transform.position.y - 1.2f || jump) _cliffSince = 0f;
        else if (_cliffSince <= 0f) _cliffSince = Time.time;
        if (pos.y < transform.position.y - 1.2f && !jump)
        {
            if (Time.time > _cliffLog) { _cliffLog = Time.time + 10f; Debug.Log($"[Ruta] «{name}» se detiene al borde en ({transform.position.x:0},{transform.position.y:0},{transform.position.z:0}) camino {_path.status} {_path.corners.Length} esquinas, hacia ({target.x:0},{target.z:0})."); }
            return Flat(final - transform.position);
        }
        transform.position = pos;
        return Flat(final - pos);
    }

    // ¿Ese punto sigue sobre el NavMesh, a la misma altura (no el suelo 6 m más abajo ni un borde a medio metro)?
    static bool StaysOnMesh(Vector3 p, float feetY)
        => NavMesh.SamplePosition(p, out NavMeshHit h, 1.5f, NavMesh.AllAreas) && Flat(h.position - p) < 0.2f && Mathf.Abs(h.position.y - feetY) < 1.2f;

    void Repath(Vector3 target)
    {
        _goal = target; _repathAt = Time.time + 4f; _corner = 1;
        Vector3 from = transform.position, to = target;
        if (NavMesh.SamplePosition(from, out NavMeshHit a, 4f, NavMesh.AllAreas)) from = a.position;
        if (NavMesh.SamplePosition(to, out NavMeshHit b, 4f, NavMesh.AllAreas)) to = b.position;
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, _path)) { _path.ClearCorners(); return; }
        // Si ya pasó la primera esquina (p. ej. recalculó en la orilla tras cruzar un tramo de agua), no da media vuelta:
        // empieza por la esquina que de verdad tiene delante.
        Vector3[] c = _path.corners;
        Vector3 here = transform.position;
        while (_corner < c.Length - 1 && Flat(c[_corner + 1] - here) < Flat(c[_corner + 1] - c[_corner])) _corner++;
    }

    // Suelo bajo los pies (pivote del personaje a 1 m sobre el suelo, como la cápsula); sobre el agua, nadando.
    float GroundY(Vector3 p, float current)
    {
        Vector3 o = new Vector3(p.x, current + 2f, p.z);
        // El MÁS ALTO que quede a un escalón de los pies (RaycastAll no viene ordenado: antes tomaba uno cualquiera y podía
        // subirse a una rama o a un tejado que tenía encima).
        float feet = current - PivotHeight, best = float.NegativeInfinity;
        foreach (RaycastHit h in Physics.RaycastAll(o, Vector3.down, 30f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(transform) && h.point.y > WaterY - 0.2f && h.point.y <= feet + 0.8f && h.point.y > best) best = h.point.y;
        if (!float.IsNegativeInfinity(best)) return Mathf.Max(best, WaterY) + PivotHeight;
        return WaterY + PivotHeight * 0.6f;   // en el agua: medio cuerpo sumergido
    }

    /// <summary>Del pivote a los pies: la cápsula de los compañeros o, en el jugador, su CharacterController (como GroundGuard).
    /// Sin esto el jugador que anda solo en una orquesta quedaba con los pies a otra altura.</summary>
    public float PivotHeight
    {
        get
        {
            CapsuleCollider c = GetComponent<CapsuleCollider>();
            if (c != null) return c.height * 0.5f * transform.lossyScale.y - c.center.y * transform.lossyScale.y;
            CharacterController cc = GetComponent<CharacterController>();
            return cc != null ? (cc.height * 0.5f - cc.center.y) * transform.lossyScale.y : 1f;
        }
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
}
