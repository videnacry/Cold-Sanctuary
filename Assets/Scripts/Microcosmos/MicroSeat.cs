using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA ESCALA DEL MOBILIARIO CÍVICO DE LOS MICROCOSMOS (docs/microcosmos-suelo-y-elementos.md §Escala; Berón 2026-11-04: «en el
/// microcosmos de Bolívar he ido a la plaza y sus estructuras parecían muy pequeñas para el uso de los insectos-humanos, se supone que
/// todas las sillas deben ser utilizables … los platos y ollas parecían demasiado pequeños para poder alimentar a un ciudadano … el
/// jugador era mucho más grande que el comedor … que la estatua y la pileta … quizás se deberían agrandar todas las estructuras y
/// utensilios del microcosmos x2?»).
/// DECISIÓN: NO un ×2 global, sino MEDIDAS POR PIEZA desde el cuerpo (<see cref="MicroBody"/>):
/// - lo que el cuerpo USA se mide con el cuerpo: el asiento a la altura de su vientre, la mesa a la de sus manos, la cama a su largo
///   (un ×2 dejaría el asiento al doble del vientre: nadie se sienta; la mesa a la altura de los ojos: nadie come);
/// - lo que ALIMENTA se mide por raciones: el plato con la superficie de una comida (<see cref="PlateFor"/>), la olla con 2 L por
///   comensal (<see cref="PotFor"/>);
/// - lo que se MIRA (estatua, fuente, plaza) es MONUMENTAL: múltiplos de la estatura del ciudadano (<see cref="StatueTotal"/>,
///   <see cref="FountainSide"/>), nunca más bajo que quien lo mira.
/// </summary>
public static class MicroCivicScale
{
    /// <summary>La estatura de un ciudadano insecto-humano tipo (m).</summary>
    public const float Citizen = 1.75f;
    /// <summary>Una estatua mide, con su pedestal, al menos esto × la estatura del ciudadano (las estatuas de plaza: 1,5–2,5×).</summary>
    public const float StatueTimes = 2.1f;
    /// <summary>El vaso de una fuente de plaza mide de lado al menos esto × la estatura del ciudadano.</summary>
    public const float FountainTimes = 2.4f;
    /// <summary>El pretil de la fuente: a la altura de un asiento (se sientan en él).</summary>
    public const float RimSeat = 0.45f;

    public static float StatueTotal(float height = Citizen) => StatueTimes * height;
    public static float FountainSide(float height = Citizen) => FountainTimes * height;

    /// <summary>El plato de UNA comida para un cuerpo de <paramref name="height"/> m: ≥ 0,26 m de diámetro a 1,75 m (una ración de
    /// ~600 g sobre ~0,05 m²), proporcional a la estatura.</summary>
    public static float PlateFor(float height = Citizen) => Mathf.Max(0.26f, 0.26f * height / Citizen);
    /// <summary>La olla de una cocina que da de comer a <paramref name="diners"/>: 2 L por comensal; devuelve el diámetro (m) de una
    /// olla de alto = 0,7 × diámetro.</summary>
    public static float PotFor(int diners) { float litres = Mathf.Max(4f, 2f * diners); return Mathf.Pow(litres / 1000f / (0.7f * Mathf.PI * 0.25f), 1f / 3f); }
    /// <summary>Litros de una olla de <paramref name="diameter"/> m (alto = 0,7 × diámetro).</summary>
    public static float PotLitres(float diameter) => Mathf.PI * 0.25f * diameter * diameter * 0.7f * diameter * 1000f;
}

/// <summary>
/// UN ASIENTO UTILIZABLE (silla, banco, taburete, el pretil de una fuente): sus puntos donde sentarse (locales) y hacia dónde mira quien
/// se sienta. Lo pone MicroFurniture en cada asiento (también los de las mesas de los restaurantes y los bancos de la plaza). Quien se
/// sienta usa <see cref="PublicSeatSpell"/> (o la mesa de su casa, <see cref="HomeNeedSpell"/>): <see cref="HomeMove.Sit"/>.
/// </summary>
public class MicroSeat : MonoBehaviour
{
    [Tooltip("Dónde se sienta cada uno (locales). Un banco de plaza tiene tres; una silla, uno.")]
    public Vector3[] points = { Vector3.zero };
    [Tooltip("Hacia dónde mira (la mesa, la plaza); null = su frente (+z local).")]
    public Transform faceTo;
    [Tooltip("¿Es de un hogar (lo usa su familia, no los paseantes)?")]
    public bool home;

    static readonly List<MicroSeat> _all = new List<MicroSeat>();
    static readonly Dictionary<(MicroSeat, int), GameObject> _taken = new Dictionary<(MicroSeat, int), GameObject>();
    public static IReadOnlyList<MicroSeat> All => _all;

    void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
    void OnDisable() { _all.Remove(this); }

    public Vector3 World(int i) => transform.TransformPoint(points[Mathf.Clamp(i, 0, points.Length - 1)]);

    /// <summary>¿Está libre el sitio <paramref name="i"/>?</summary>
    public bool Free(int i) => !_taken.TryGetValue((this, i), out GameObject g) || g == null;

    /// <summary>EL ASIENTO LIBRE más cercano a <paramref name="from"/> (a menos de <paramref name="radius"/> m), sin los de los
    /// hogares: el asiento y el sitio. null si no hay.</summary>
    public static MicroSeat Nearest(Vector3 from, float radius, out int slot)
    {
        slot = -1; MicroSeat best = null; float bestSq = radius * radius;
        foreach (MicroSeat s in _all)
        {
            if (s == null || s.home || !s.isActiveAndEnabled || s.GetComponentInParent<Household>(true) != null) continue;   // los de casa son de su familia
            for (int i = 0; i < s.points.Length; i++)
            {
                if (!s.Free(i)) continue;
                float sq = (s.World(i) - from).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = s; slot = i; }
            }
        }
        return best;
    }

    public bool Take(int i, GameObject who) { if (!Free(i)) return false; _taken[(this, i)] = who; return true; }
    public void Leave(int i, GameObject who) { if (_taken.TryGetValue((this, i), out GameObject g) && (g == who || g == null)) _taken.Remove((this, i)); }
}

/// <summary>
/// SENTARSE EN UN ASIENTO PÚBLICO (un banco de la plaza, una silla de un comedor, el pretil de la fuente): va andando al asiento libre
/// más cercano, se sienta (<see cref="HomeMove.Sit"/>: el insecto-humano a horcajadas en su asiento de abdomen, la persona con la
/// postura sentada) un rato y se levanta. Todo con tope: si en 45 s reales no llega, lo deja (no se le teletransporta a la plaza).
/// Lo encola <see cref="NeedsDriver"/> en el tiempo libre de quien tiene horario (no en su turno).
/// </summary>
public class PublicSeatSpell : QueuedSpell
{
    public float minutes = 15f, radius = 60f;
    const float WalkTimeout = 45f;
    MicroSeat _seat; int _slot = -1; int _phase; float _since, _until;

    /// <summary>Cuántas veces alguien se sentó en un asiento público (pruebas).</summary>
    public static int Sat;

    public override string Label => _phase == 0 ? "ir a sentarse (asiento público)" : "sentado en un asiento público";

    static float MinuteSecs => TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 1f;

    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        Transform t = self.transform;
        float h = PersonLife.HeightOf(t);
        switch (_phase)
        {
            case 0:
                if (_seat == null)
                {
                    _seat = MicroSeat.Nearest(t.position, radius, out _slot);
                    if (_seat == null || !_seat.Take(_slot, self.gameObject)) return true;   // ningún asiento libre cerca: nada
                    _since = Time.time;
                }
                Vector3 to = _seat.World(_slot);
                if (!MicroMove.Step(t, to, 1.3f * h / 1.75f, 0.5f * h))
                {
                    if (Time.time - _since > WalkTimeout) { _seat.Leave(_slot, self.gameObject); return true; }
                    return false;
                }
                HomeMove.Sit(self, SeatPoint(), _seat.faceTo != null ? _seat.faceTo : null);
                if (_seat.faceTo == null) t.rotation = Quaternion.LookRotation(_seat.transform.forward);
                _until = Time.time + minutes * MinuteSecs;
                Sat++;
                Debug.Log($"[Asiento] «{self.name}» se sienta en «{_seat.name}».");
                _phase = 1;
                return false;
            case 1:
                HomeMove.KeepSitting(self);
                if (Time.time < _until) return false;
                HomeMove.StandUp(self, _seat.World(_slot) + _seat.transform.forward * 0.6f);
                _seat.Leave(_slot, self.gameObject);
                return true;
            default: return true;
        }
    }

    // HomeMove.Sit pone a quien se sienta en el sitio del asiento (x, z) y a la altura de la raíz del mueble: un punto vacío como hijo.
    Transform SeatPoint()
    {
        string n = $"Sitio_{_slot}";
        Transform p = _seat.transform.Find(n);
        if (p == null) { p = new GameObject(n).transform; p.SetParent(_seat.transform, false); p.localPosition = _seat.points[_slot]; }
        return p;
    }
}
