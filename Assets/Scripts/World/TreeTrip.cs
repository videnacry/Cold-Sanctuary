using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UN VIAJE A NADO POR LA RAÍZ: recorre el camino de <see cref="ArbolCampos.SwimPath"/> (puntos locales del árbol, que gira)
/// paso a paso: andando hasta la puerta, nadando (<see cref="SwimBody.SwimTo"/>) de puerta a puerta y andando al salir.
/// Tiene un límite de tiempo (si se atasca, se queda donde está y avisa): nunca teletransporta.
/// </summary>
public class TreeTrip : MonoBehaviour
{
    static readonly HashSet<GameObject> _busy = new HashSet<GameObject>();
    public static bool Busy(GameObject g) => g != null && _busy.Contains(g);
    public static int Trips { get; private set; }
    public static int Stuck { get; private set; }
    /// <summary>Registro de cada tramo (las pruebas).</summary>
    public static bool Verbose;
    float _t0;

    ArbolCampos _tree;
    List<Vector3> _path;
    int _i;
    float _until, _ph;
    TreeLevel _to;
    SwimBody _swim;
    public TreeLevel Destination => _to;

    public static void Start(GameObject g, ArbolCampos tree, List<Vector3> path, TreeLevel to, bool wetStart = false)
    {
        TreeTrip t = g.GetOrAdd<TreeTrip>(); t._wetStart = wetStart;
        t._tree = tree; t._path = path; t._i = 1; t._to = to; t.enabled = true;
        t._ph = PathMover.Of(g.transform).PivotHeight;
        t._swim = SwimBody.Of(g.transform);
        float len = 0f; for (int i = 1; i < path.Count; i++) len += Vector3.Distance(path[i - 1], path[i]);
        t._t0 = Time.time;
        t._until = Time.time + len / Mathf.Max(0.5f, tree.routeSwimSpeed) * 3f + 20f;
        // El jugador: se le sujeta el cuerpo (su control no lo mueve a la vez) y, como a todos, se le lleva sin chocar con los
        // cantos de las puertas: el CharacterController se apaga durante el viaje (GroundGuard tampoco lo toca).
        if (g.TryGetComponent(out PlayerController pc) && !t._held) { pc.HoldBody(true); t._held = true; }
        if (g.TryGetComponent(out CharacterController cc) && cc.enabled) { cc.enabled = false; t._ccOff = true; }
        _busy.Add(g); Trips++;
        Debug.Log($"[Árbol] «{g.name}» entra en la raíz hacia {to.name}: {len:0} m a nado y andando ({path.Count} tramos).");
    }

    // ¿El tramo i (del punto i−1 al i) va por el agua? Entra por la puerta (punto 2) y sale por la otra (antepenúltimo).
    bool _wetStart, _held, _ccOff;
    bool Wet(int i) => (i >= 2 || (_wetStart && i >= 1)) && i <= _path.Count - 2;

    void Update()
    {
        if (_tree == null || _path == null) { Finish(false); return; }
        if (Time.time > _until) { Stuck++; Debug.LogWarning($"[Árbol] «{name}» no llegó a {_to.name} a tiempo: se queda donde está (tramo {_i}/{_path.Count - 1})."); Finish(false); return; }
        float budget = (Wet(_i) ? _tree.routeSwimSpeed : 1.8f) * Time.deltaTime;
        for (int guard = 0; guard < 4 && _i < _path.Count && budget > 1e-4f; guard++)
        {
            Vector3 target = _tree.World(_path[_i]) + Vector3.up * _ph;
            Vector3 d = target - transform.position;
            float dist = d.magnitude;
            // (Nadando solo hacia un punto DENTRO del agua: hacia el hueco de una puerta, que es aire, SwimTo lo bajaría a la
            // superficie del mar, que es el agua que queda «debajo» de ese punto; ahí se avanza en línea recta, como al andar.)
            if (Wet(_i) && WaterZone.Underwater(target))
            {
                Vector3 was = transform.position;
                float left = _swim.SwimTo(target, budget / Mathf.Max(1e-4f, Time.deltaTime));
                budget -= Mathf.Max(0.02f * budget, Vector3.Distance(was, transform.position));   // (lo de verdad recorrido: nunca más que la velocidad)
                if (left > 0.05f) break;
            }
            else
            {
                if (Wet(_i)) _swim.Begin(); else _swim.End();
                float step = Mathf.Min(budget, dist);
                if (dist > 1e-4f) transform.position += d / dist * step;
                Vector3 flat = new Vector3(d.x, 0f, d.z);
                if (flat.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 1f - Mathf.Exp(-8f * Time.deltaTime));
                budget -= step;
                if (dist - step > 0.05f) break;
            }
            if (Verbose) Debug.Log($"[Árbol] «{name}» tramo {_i}/{_path.Count - 1} hecho a los {Time.time - _t0:0.0} s en {transform.position} ({(Wet(_i) ? "nadando" : "andando")}).");
            _i++;
        }
        if (_i >= _path.Count) Finish(true);
    }

    void Finish(bool ok)
    {
        if (_swim != null) _swim.End();
        if (_ccOff && TryGetComponent(out CharacterController cc)) cc.enabled = true;
        if (_held && TryGetComponent(out PlayerController pc)) pc.HoldBody(false);
        _ccOff = _held = false;
        _busy.Remove(gameObject);
        if (ok) Debug.Log($"[Árbol] «{name}» sale por la puerta de {_to?.name}.");
        _path = null; enabled = false;
    }

    void OnDisable()
    {
        if (_ccOff && TryGetComponent(out CharacterController cc)) cc.enabled = true;
        if (_held && TryGetComponent(out PlayerController pc)) pc.HoldBody(false);
        _ccOff = _held = false;
        _busy.Remove(gameObject);
    }
}

