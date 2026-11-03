using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MÁSCARAS PARA RESPIRAR EN LOS EDIFICIOS-ÁRBOL (Berón 2026-11-03; docs/mascaras-de-oxigeno.md). «Máscaras para poder respirar de
/// modo que los personajes puedan nadar indefinidamente: están en las entradas y también hay estaciones dentro». Dos clases de
/// puesto (<see cref="MaskStation"/>):
///   · PERCHERO de la entrada (<see cref="MaskStation.Kind.Rack"/>): fuera, junto a cada puerta del tronco, de las columnas de las
///     puntas y del techo. Se coge al entrar al agua y se deja en el de la puerta por la que se sale.
///   · ESTACIÓN interior (<see cref="MaskStation.Kind.Station"/>): dentro del agua, junto a cada puerta y a lo largo del tronco cada
///     ~16 m (y en el fondo abierto). Quien acaba en el agua sin máscara nada a la más cercana y se la pone.
/// Puesta (<see cref="Worn"/>), el aliento no se gasta (<see cref="SwimBody"/>): oxígeno ilimitado (un rebreather con su botella).
/// </summary>
[DisallowMultipleComponent]
public class BreathingMask : MonoBehaviour
{
    /// <summary>¿Lleva la máscara puesta?</summary>
    public bool Worn { get; private set; }
    /// <summary>Desde dónde la cogió (un perchero o una estación).</summary>
    public MaskStation TakenFrom { get; private set; }
    public static int Taken { get; private set; }
    public static int Returned { get; private set; }
    public static int Rescues { get; private set; }
    /// <summary>Lo más lejos (m) que se nada a por una estación; más lejos (el mar abierto) se sube a respirar como siempre.</summary>
    public const float RescueRange = 45f;
    /// <summary>Segundos (de juego) en coger o dejar la máscara y ajustársela.</summary>
    public const float FitSeconds = 1.2f;

    public static BreathingMask Of(Component c) => c.GetOrAdd<BreathingMask>();
    public static bool Has(GameObject g) => g != null && g.TryGetComponent(out BreathingMask m) && m.Worn;

    Transform _visual;
    bool _wetSinceEquip, _isPlayer, _rescuing;
    MaskStation _rescueTo;
    float _fitUntil;

    void Awake() { _isPlayer = CompareTag("Player"); }

    /// <summary>Coge una máscara de <paramref name="from"/> y se la pone. false si allí no quedan.</summary>
    public bool Equip(MaskStation from)
    {
        if (Worn) return true;
        if (from == null || !from.TryTake()) return false;
        Worn = true; TakenFrom = from; _wetSinceEquip = false; Taken++;
        ShowVisual(true);
        Debug.Log($"[Mascaras] «{name}» coge una máscara de {from.name} y se la pone: oxígeno ilimitado.");
        return true;
    }

    /// <summary>Deja la máscara en <paramref name="to"/> (si es null, en el puesto más cercano a 8 m; si no hay, se la queda).</summary>
    public bool Return(MaskStation to = null)
    {
        if (!Worn) return true;
        if (to == null) to = MaskStation.Nearest(transform.position, null, 8f, false);
        if (to == null) return false;
        to.Put();
        Worn = false; TakenFrom = null; Returned++;
        ShowVisual(false);
        Debug.Log($"[Mascaras] «{name}» deja la máscara en {to.name}.");
        return true;
    }

    /// <summary>Quita la máscara sin dejarla en ningún puesto (pruebas: empezar sin máscara).</summary>
    public void Strip() { Worn = false; TakenFrom = null; ShowVisual(false); _rescuing = false; _rescueTo = null; }

    SwimBody Swim => TryGetComponent(out SwimBody s) ? s : null;

    /// <summary>¿Tiene la cabeza bajo el agua de un edificio-árbol (con una estación a mano) y sin máscara? Entonces busca una.</summary>
    public bool NeedsRescue
    {
        get
        {
            if (Worn) return false;
            SwimBody s = Swim;
            Vector3 head = s != null ? s.HeadPosition : transform.position + Vector3.up * 0.7f;
            if (!WaterZone.Underwater(head)) return false;
            return MaskStation.Nearest(head, MaskStation.Kind.Station, RescueRange, true) != null;
        }
    }
    public bool Rescuing => _rescuing;
    public static bool IsRescuing(GameObject g) => g != null && g.TryGetComponent(out BreathingMask m) && m._rescuing;

    /// <summary>Un paso del RESCATE (personajes que no son el jugador): nadar a la estación interior más cercana y ponerse una
    /// máscara. true mientras está en ello (quien lo conduce cede el paso este fotograma). Lo llaman PathMover y TreeTrip; si nadie
    /// lo conduce, lo hace su propio Update.</summary>
    public static bool RescueStep(GameObject g, bool fromTrip = false)
        => g != null && !g.CompareTag("Player") && (fromTrip || !TreeTrip.Busy(g)) && Of(g.transform).Step();

    float _lastStep;
    bool Step()
    {
        if (Time.frameCount == _stepFrame) return _rescuing;   // una vez por fotograma
        _stepFrame = Time.frameCount;
        _lastStep = Time.time;
        if (Worn) { _rescuing = false; return false; }
        if (Time.time < _fitUntil) return true;   // ajustándosela
        if (!_rescuing)
        {
            if (!NeedsRescue) return false;
            _rescuing = true; Rescues++;
            Debug.Log($"[Mascaras] «{name}» está bajo el agua sin máscara: nada a la estación más cercana.");
        }
        SwimBody s = SwimBody.Of(this);
        if (_rescueTo == null || _rescueTo.Masks <= 0) _rescueTo = MaskStation.Nearest(s.HeadPosition, MaskStation.Kind.Station, RescueRange, true, true);
        if (_rescueTo == null) { _rescuing = false; return false; }   // ninguna con máscaras: sube a respirar como siempre
        Vector3 at = _rescueTo.SwimPoint + Vector3.down * Mathf.Max(0f, s.HeadPosition.y - transform.position.y);
        float left = s.SwimTo(at, Mathf.Max(1.6f, s.npcSwimSpeed));
        if (left < 0.9f || Vector3.Distance(s.HeadPosition, _rescueTo.SwimPoint) < 1.4f)
        {
            if (Equip(_rescueTo)) { _fitUntil = Time.time + FitSeconds; _rescuing = false; _rescueTo = null; return true; }
            _rescueTo = null;
        }
        return true;
    }
    int _stepFrame = -1; float _nextCheck;

    void Update()
    {
        SwimBody s = Swim;
        if (Worn && s != null && s.Active) _wetSinceEquip = true;
        if (_isPlayer) { PlayerUpdate(s); return; }
        // Sin nadie que lo conduzca (ni la jornada, ni un viaje por el tronco): el rescate lo hace él solo.
        if (!TreeTrip.Busy(gameObject) && !PathMover.DrivenRecently(gameObject, 0.3f) && (_rescuing || Time.time < _fitUntil || Time.time >= _nextCheck)) { _nextCheck = Time.time + 0.25f; Step(); }
    }

    // EL JUGADOR: coge la máscara al pasar junto a un perchero de la entrada (o al tocar una estación dentro del agua) y la deja en el
    // perchero de la puerta por la que sale (tras haber nadado con ella).
    float _nextScan;
    void PlayerUpdate(SwimBody s)
    {
        if (Time.time < _nextScan) return;
        _nextScan = Time.time + 0.2f;
        bool wet = s != null && s.Active;
        if (!Worn)
        {
            MaskStation near = MaskStation.Nearest(wet ? s.HeadPosition : transform.position, wet ? MaskStation.Kind.Station : MaskStation.Kind.Rack, wet ? 1.8f : 4.5f, wet, true);
            if (near != null) Equip(near);
        }
        else if (!wet && _wetSinceEquip)
        {
            MaskStation rack = MaskStation.Nearest(transform.position, MaskStation.Kind.Rack, 5f, false);
            if (rack != null) Return(rack);
        }
        else if (!wet && TakenFrom != null && Vector3.Distance(transform.position, TakenFrom.SwimPoint) > 6f)
            Return(TakenFrom);   // la cogió al pasar junto al perchero y no entró al agua: la deja donde estaba
    }

    void OnGUI()
    {
        if (!_isPlayer) return;
        SwimBody s = Swim;
        if (s == null || !s.Active) return;
        float w = 320f, x = (Screen.width - w) * 0.5f, y = Screen.height - 150f;
        if (Worn) { GUI.color = new Color(0.6f, 1f, 0.75f); GUI.Label(new Rect(x, y, w, 20f), "Máscara puesta: oxígeno ilimitado"); GUI.color = Color.white; return; }
        if (s.HeadAboveWater) return;
        MaskStation st = MaskStation.Nearest(s.HeadPosition, MaskStation.Kind.Station, RescueRange, true, true);
        if (st == null) return;
        Vector3 d = st.SwimPoint - s.HeadPosition;
        string dir = d.y > 2f ? "arriba" : d.y < -2f ? "abajo" : "a tu altura";
        GUI.color = new Color(1f, 0.85f, 0.4f);
        GUI.Label(new Rect(x, y, w, 20f), $"Sin máscara: estación de máscaras a {d.magnitude:0} m ({dir})");
        GUI.color = Color.white;
    }

    // ── La máscara en la cara: visor, boquilla, correa y la botellita del rebreather ──
    void ShowVisual(bool on)
    {
        if (!on) { if (_visual != null) Destroy(_visual.gameObject); _visual = null; return; }
        if (_visual != null) return;
        Transform head = FindHead();
        Transform v = new GameObject("Mascara_puesta").transform;
        if (head != null) { v.SetParent(head, false); v.position = head.TransformPoint(new Vector3(0f, 0.13f, 0f)) + head.forward * 0.11f; }
        else { v.SetParent(transform, false); v.localPosition = new Vector3(0f, 0.75f, 0.2f); }
        v.localRotation = Quaternion.identity;
        Vector3 inv = new Vector3(1f / Mathf.Max(1e-3f, v.lossyScale.x), 1f / Mathf.Max(1e-3f, v.lossyScale.y), 1f / Mathf.Max(1e-3f, v.lossyScale.z));
        void P(Vector3 p, Vector3 s, Color c, string n)
        {
            Transform t = KitchenProps.C(v, Vector3.Scale(p, inv), Vector3.Scale(s, inv), c, 0f, n);
            if (t.TryGetComponent(out Renderer r)) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        P(new Vector3(0f, 0.04f, 0.02f), new Vector3(0.2f, 0.09f, 0.03f), new Color(0.7f, 0.9f, 1f, 0.45f), "Visor");
        P(new Vector3(0f, 0.04f, 0.035f), new Vector3(0.22f, 0.11f, 0.02f), new Color(0.95f, 0.75f, 0.1f), "Marco_visor");
        P(new Vector3(0f, -0.05f, 0.05f), new Vector3(0.07f, 0.06f, 0.06f), new Color(0.15f, 0.15f, 0.17f), "Boquilla");
        P(new Vector3(0f, 0.04f, -0.12f), new Vector3(0.24f, 0.025f, 0.24f), new Color(0.1f, 0.1f, 0.12f), "Correa");
        P(new Vector3(0.12f, -0.12f, -0.02f), new Vector3(0.05f, 0.16f, 0.05f), new Color(0.9f, 0.9f, 0.92f), "Botella_O2");
        _visual = v;
    }

    Transform FindHead()
    {
        Transform body = transform.Find(CubeHumanoid.BodyName);
        if (body == null) return null;
        foreach (CubeJoint j in body.GetComponentsInChildren<CubeJoint>(true)) if (j.part == BodyPart.Head && j.name == BodyPart.Head.ToString()) return j.transform;
        return null;
    }

    void OnDisable() { _rescuing = false; }
}

/// <summary>Un PUESTO DE MÁSCARAS de un edificio-árbol: perchero de la entrada o estación interior (dentro del agua). Lleva la
/// cuenta de sus máscaras (se ven colgadas: <see cref="Masks"/> de <see cref="capacity"/>) y su ESTADO (las juntas, la válvula y
/// la carga de las botellas: baja un poco cada día; lo repone quien cuida los jardines del edificio, ver FruitGardener).</summary>
public class MaskStation : MonoBehaviour
{
    public enum Kind { Rack, Station }
    public Kind kind = Kind.Rack;
    public string building = "";
    public int capacity = 8;
    [SerializeField] int masks = 8;
    /// <summary>Estado de las máscaras del puesto (1 = revisadas hoy). Baja <see cref="DecayPerDay"/> al día.</summary>
    [Range(0f, 1f)] public float condition = 1f;
    public const float DecayPerDay = 0.25f;
    /// <summary>Minutos de juego en revisar el puesto (las juntas y el visor, la válvula y la carga de cada botella).</summary>
    public const float CheckMinutes = 1f;
    /// <summary>Donde se pone quien la coge (dentro del agua, delante del panel; o fuera, delante del perchero).</summary>
    public Transform standPoint;

    public static readonly List<MaskStation> All = new List<MaskStation>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() => All.Remove(this);

    public int Masks => masks;
    public Vector3 SwimPoint => standPoint != null ? standPoint.position : transform.position;
    public bool Underwater => WaterZone.Underwater(SwimPoint);

    int _lastDay = -1;
    void Update()
    {
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        if (_lastDay < 0) _lastDay = day;
        if (day != _lastDay) { condition = Mathf.Max(0f, condition - DecayPerDay * (day - _lastDay)); _lastDay = day; }
    }

    public bool TryTake()
    {
        if (masks <= 0) return false;
        masks--; Refresh();
        return true;
    }

    /// <summary>Deja una máscara (si está lleno, la guarda en el cajón de reserva: hasta el doble de su capacidad).</summary>
    public void Put() { masks = Mathf.Min(capacity * 2, masks + 1); Refresh(); }

    /// <summary>Revisión de quien cuida el edificio: juntas, visor, válvula y carga de las botellas; repone hasta la capacidad.</summary>
    public void Check(string who)
    {
        float before = condition;
        condition = 1f;
        if (masks < capacity) masks = capacity;
        Refresh();
        Debug.Log($"[Mascaras] «{who}» revisa {name}: estado {before * 100f:0}% → 100%, {masks} máscaras.");
    }

    // Las máscaras colgadas (hijas llamadas «Mascara»): se ven tantas como quedan.
    void Refresh()
    {
        int i = 0;
        foreach (Transform t in transform)
        {
            if (!t.name.StartsWith("Mascara")) continue;
            t.gameObject.SetActive(i < masks);
            i++;
        }
    }

    void Start() { Refresh(); }

    /// <summary>El puesto más cercano a <paramref name="p"/> (de una clase, o de cualquiera si null), a menos de
    /// <paramref name="max"/> m; <paramref name="underwater"/>: solo los que están dentro del agua (y solo los que no, si false
    /// y es un perchero). <paramref name="withMasks"/>: solo los que tienen alguna.</summary>
    public static MaskStation Nearest(Vector3 p, Kind? kind, float max, bool underwater, bool withMasks = false)
    {
        MaskStation best = null; float bd = max * max;
        foreach (MaskStation m in All)
        {
            if (m == null || !m.isActiveAndEnabled) continue;
            if (kind.HasValue && m.kind != kind.Value) continue;
            if (withMasks && m.masks <= 0) continue;
            if (underwater && m.kind == Kind.Rack) continue;
            float d = (m.SwimPoint - p).sqrMagnitude;
            if (d < bd) { bd = d; best = m; }
        }
        return best;
    }

    public static IEnumerable<MaskStation> Of(string building)
    {
        foreach (MaskStation m in All) if (m != null && m.building == building) yield return m;
    }
}
