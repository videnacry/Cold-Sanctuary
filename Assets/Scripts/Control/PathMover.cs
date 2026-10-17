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
    float _repathAt;
    const float WaterY = -0.25f;

    public static PathMover Of(Component c) => c.GetComponent<PathMover>() ?? c.gameObject.AddComponent<PathMover>();

    /// <summary>Un paso hacia <paramref name="target"/> a <paramref name="speed"/> m/s. Devuelve la distancia que queda.</summary>
    public float StepToward(Vector3 target, float speed)
    {
        if (_path == null) _path = new NavMeshPath();
        if (float.IsNaN(_goal.x) || (target - _goal).sqrMagnitude > 1f || Time.time >= _repathAt) Repath(target);

        Vector3 pos = transform.position;
        Vector3 next = target;
        if (_path.status != NavMeshPathStatus.PathInvalid && _path.corners.Length > 1)
        {
            while (_corner < _path.corners.Length - 1 && Flat(_path.corners[_corner] - pos) < 0.35f) _corner++;
            next = _path.corners[Mathf.Min(_corner, _path.corners.Length - 1)];
        }
        Vector3 d = next - pos; d.y = 0f;
        float step = Mathf.Min(speed * Time.deltaTime, d.magnitude);
        if (d.sqrMagnitude > 1e-6f)
        {
            pos += d.normalized * step;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 8f * Time.deltaTime);
        }
        pos.y = GroundY(pos, transform.position.y);
        transform.position = pos;
        return Flat(target - pos);
    }

    void Repath(Vector3 target)
    {
        _goal = target; _repathAt = Time.time + 1.5f; _corner = 1;
        Vector3 from = transform.position, to = target;
        if (NavMesh.SamplePosition(from, out NavMeshHit a, 4f, NavMesh.AllAreas)) from = a.position;
        if (NavMesh.SamplePosition(to, out NavMeshHit b, 4f, NavMesh.AllAreas)) to = b.position;
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, _path)) _path.ClearCorners();
    }

    // Suelo bajo los pies (pivote del personaje a 1 m sobre el suelo, como la cápsula); sobre el agua, nadando.
    float GroundY(Vector3 p, float current)
    {
        Vector3 o = new Vector3(p.x, current + 2f, p.z);
        foreach (RaycastHit h in Physics.RaycastAll(o, Vector3.down, 30f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(transform) && h.point.y > WaterY - 0.2f) return Mathf.Max(h.point.y, WaterY) + PivotHeight;
        return WaterY + PivotHeight * 0.6f;   // en el agua: medio cuerpo sumergido
    }

    float PivotHeight
    {
        get
        {
            CapsuleCollider c = GetComponent<CapsuleCollider>();
            return c != null ? c.height * 0.5f * transform.lossyScale.y - c.center.y * transform.lossyScale.y : 1f;
        }
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
}
