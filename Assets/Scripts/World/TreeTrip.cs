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
    /// <summary>(2026-11-11) Quién se atascó (en orden), para que una prueba cuente solo a los suyos.</summary>
    public static readonly List<string> StuckWho = new List<string>();
    /// <summary>Registro de cada tramo (las pruebas).</summary>
    public static bool Verbose;
    float _t0;

    ArbolCampos _tree;
    List<Vector3> _path;
    int _i;
    float _until, _ph;
    Vector3 _left;   // donde lo dejó el viaje el último fotograma
    TreeLevel _to;
    SwimBody _swim;
    public TreeLevel Destination => _to;

    /// <summary>¿El viaje en curso de <paramref name="g"/> sigue llevando adonde va ahora (<paramref name="to"/>)? (2026-11-01) Un viaje
    /// empezado hacia otro sitio seguía hasta el final aunque le mandaran a otro: JefeConstruccion bajaba nadando 55 m al pie de
    /// Producción (iba al comedero) cuando ya le tocaba la clase de yoga en el techo de otro árbol, y no llegaba a su esterilla.
    /// Mira una vez por segundo: el nivel que querría ahora (el del destino en este árbol o, si está fuera, su salida) es el del viaje.</summary>
    public static bool StillFor(GameObject g, Vector3 to)
    {
        if (g == null || !g.TryGetComponent(out TreeTrip t) || t._tree == null || t._to == null) return true;
        if (Time.time < t._recheck) return true;
        t._recheck = Time.time + 1f;
        ArbolCampos tb = ArbolCampos.At(to, out TreeLevel b, out _, false);
        TreeLevel want = tb == t._tree && b != null ? b : t._tree.ExitFor(to);
        if (want == null || want == t._to) return true;
        Debug.Log($"[Árbol] «{g.name}» ya no va a {t._to.name} sino a {want.name}: deja ese viaje y nada hacia allí.");
        // (2026-11-11) NADA HACIA ALLÍ DE VERDAD: el viaje nuevo empieza ya, desde donde está y en el agua. Antes se dejaba el viaje y la ruta se
        // replanificaba «desde donde está»; a media rama o columna ArbolCampos.At no lo ve ni en un nivel ni en el agua del tronco, la ruta lo daba
        // por fuera del árbol, PathMover «se detiene al borde … PathInvalid» y GroundGuard, ya sin nado, lo bajaba de golpe: JefeConstruccion
        // 52 m y Gohageneis 17 m («no se teletransporta», clase de yoga, rondas 48 y 51).
        ArbolCampos tree = t._tree; float ph = t._ph;
        t.Finish(false);
        List<Vector3> path = tree.SwimPath(null, want, tree.transform.InverseTransformPoint(g.transform.position) - Vector3.up * ph);
        if (path != null && path.Count >= 2) { Start(g, tree, path, want, true); return true; }
        return false;
    }
    float _recheck;

    public static void Start(GameObject g, ArbolCampos tree, List<Vector3> path, TreeLevel to, bool wetStart = false)
    {
        TreeTrip t = g.GetOrAdd<TreeTrip>(); t._wetStart = wetStart; t._maskDone = false; t._maskUntil = 0f;
        t._tree = tree; t._path = path; t._i = 1; t._to = to; t.enabled = true;
        t._ph = PathMover.Of(g.transform).PivotHeight;
        t._swim = SwimBody.Of(g.transform);
        float len = 0f; for (int i = 1; i < path.Count; i++) len += Vector3.Distance(path[i - 1], path[i]);
        t._t0 = Time.time; t._left = g.transform.position; t._recheck = Time.time + 1f;
        t._until = Time.time + len / Mathf.Max(0.5f, tree.routeSwimSpeed) * 4f + 60f;   // (2026-11-04: más margen —4× el nado + 60 s—: con 4 a la vez por una rama, uno se daba por atascado, prueba 53; el aviso dice dónde)
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
    // ¿El tramo i sube o baja por la columna (la corriente de la raíz lo lleva)? Casi vertical: más alto que ancho ×3.
    bool AlongColumn(int i) { if (i < 1 || i >= _path.Count) return false; Vector3 d = _path[i] - _path[i - 1]; return Mathf.Abs(d.y) > 3f * new Vector2(d.x, d.z).magnitude; }
    // (2026-11-01) La corriente lleva también por el interior de las RAMAS, no solo por la columna: de rama a rama (137 m, 14
    // tramos) se tardaban 52 s bajo el agua, más que los 50 s de aliento de quien tiene resistencia 1 («aliento mínimo 0,00»,
    // prueba 53). Solo los tramos de las puertas (meterse en el agua y salir) van sin corriente. Nado + corriente = 3,9 m/s.
    bool Carried(int i) => AlongColumn(i) || (Wet(i) && i > 2 && i < _path.Count - 2);

    void Update()
    {
        if (_tree == null || _path == null) { Finish(false); return; }
        // Si OTRO lo ha movido lejos (un teletransporte: la jornada o una prueba lo ponen en su puesto, la vuelta de un
        // microcosmos…), el viaje ya no vale: seguía desde allí en línea recta hacia la raíz (Goluis salía volando de la
        // cocina hacia el árbol, 130 m, a 1,67 m por fotograma, en el servicio del restaurante).
        if ((transform.position - _left).sqrMagnitude > 16f)
        { Debug.Log($"[Árbol] «{name}» ya no está donde lo dejó el viaje ({Vector3.Distance(transform.position, _left):0.0} m): deja el viaje a {_to?.name}."); Finish(false); return; }
        if (Time.time > _until) { Stuck++; StuckWho.Add(name); Debug.LogWarning($"[Árbol] «{name}» no llegó a {_to.name} a tiempo: se queda donde está (tramo {_i}/{_path.Count - 1}, en {transform.position}, hacia {_tree.World(_path[Mathf.Min(_i, _path.Count - 1)])}, {Time.time - _t0:0} s; máscara {BreathingMask.Has(gameObject)}, rescate {BreathingMask.IsRescuing(gameObject)})."); Finish(false); return; }
        if (MaskStep()) { _left = transform.position; return; }
        float budget = (Wet(_i) ? _tree.routeSwimSpeed + (Carried(_i) ? _tree.routeCurrent : 0f) : 1.8f) * Time.deltaTime;
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
            if (_i == 2 && !_maskDone && !_wetStart) break;   // en la puerta: primero, la máscara
        }
        _left = transform.position;
        if (_i >= _path.Count) Finish(true);
    }

    // LA MÁSCARA (docs/mascaras-de-oxigeno.md): al llegar a la puerta (punto 1, fuera) coge una del perchero de la entrada y se la
    // pone antes de meterse en el agua (o, si el perchero está vacío, de la estación de dentro, junto a la puerta). Si el viaje
    // empieza ya en el agua y sin máscara, primero nada a la estación más cercana. true mientras está en ello.
    bool _maskDone; float _maskUntil;
    bool MaskStep()
    {
        if (_maskDone) return false;
        if (BreathingMask.Has(gameObject)) { if (Time.time < _maskUntil) { _until += Time.deltaTime; return true; } _maskDone = true; return false; }
        if (_wetStart) { if (BreathingMask.RescueStep(gameObject, true)) { _until += Time.deltaTime; return true; } _maskDone = true; return false; }
        if (_i < 2) return false;
        MaskStation r = MaskStation.Nearest(transform.position, MaskStation.Kind.Rack, 7f, false, true)
                        ?? MaskStation.Nearest(_tree.World(_path[Mathf.Min(2, _path.Count - 1)]), MaskStation.Kind.Station, 9f, true, true);
        // (2026-11-04) Sin pararse: la coge al pasar por la puerta, ya entrando (la pausa de 1,2 s por viaje, y los rodeos, sumaban en la
        // jornada: Kushal llegó a su ronda de la Planta con menos de la mitad del tiempo, prueba 50).
        if (r != null) BreathingMask.Of(transform).Equip(r);
        _maskDone = true; return false;
        _maskDone = true; return false;
    }

    void Finish(bool ok)
    {
        // Sale por una puerta: deja la máscara en el perchero de esa entrada.
        if (ok && BreathingMask.Has(gameObject) && !WaterZone.Underwater(transform.position + Vector3.up * 0.5f))
            BreathingMask.Of(transform).Return(MaskStation.Nearest(transform.position, MaskStation.Kind.Rack, 9f, false) ?? MaskStation.Nearest(transform.position, null, 9f, false));
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

