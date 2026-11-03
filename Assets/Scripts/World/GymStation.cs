using System.Collections.Generic;
using UnityEngine;

/// <summary>Los 10 tipos de máquina del gimnasio (docs/gimnasio-maquinas.md): 0–4 abajo, 5–9 en la entreplanta.</summary>
public enum GymMachineType
{
    PressPecho, PressHombros, Jalon, Remo, Prensa, ExtensionCuadriceps, CurlFemoral, HipThrust, Gemelos, BicepsTriceps
}

/// <summary>
/// MÁQUINA DEL GIMNASIO EN USO (docs/gimnasio-maquinas.md, §13): cada <c>Maquina_*</c> lleva una. Sabe su tipo, su
/// <see cref="puesto"/> (hijo vacío «Puesto»: dónde va la raíz del usuario y hacia dónde mira), la altura de su pelvis
/// (<see cref="rootHeight"/>, para 1,85 m) y las dos posturas de una repetición (A ↔ B, «Postura de uso»). Como
/// <see cref="HospitalStation"/>, hay un registro estático y cada máquina la ocupa una sola persona a la vez
/// (<see cref="Claim"/> / <see cref="Release"/>).
/// </summary>
public class GymStation : MonoBehaviour
{
    public GymMachineType type;
    [Tooltip("Copia de este tipo (0, 1 o 2).")]
    public int copy;
    [Tooltip("Hijo «Puesto»: posición de la raíz del usuario (los pies, o la vertical de la pelvis) y su giro.")]
    public Transform puesto;
    [Tooltip("Altura de la pelvis en unidades del cuerpo nativo (de pie = 0,95), para 1,85 m de estatura.")]
    public float rootHeight = 0.6f;
    [Tooltip("Desplazamiento local del puesto en la postura B (solo el hip thrust: la pelvis avanza 0,10 m).")]
    public Vector3 shiftB;

    public string Occupant { get; private set; }

    static readonly List<GymStation> _all = new List<GymStation>();
    static readonly Dictionary<string, GymStation> _byWho = new Dictionary<string, GymStation>();

    void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
    void OnDisable()
    {
        _all.Remove(this);
        if (Occupant != null && _byWho.TryGetValue(Occupant, out GymStation s) && s == this) _byWho.Remove(Occupant);
        Occupant = null;
    }

    public static IReadOnlyList<GymStation> All => _all;
    public bool Free => string.IsNullOrEmpty(Occupant);
    /// <summary>Encima de la entreplanta (tipos 5–9).</summary>
    public bool Upper => (int)type >= 5;

    Transform P => puesto != null ? puesto : transform;
    /// <summary>Dónde van los pies / la vertical de la pelvis (en el mundo); <paramref name="b"/> = fase B.</summary>
    public Vector3 SeatPoint(float b = 0f) => P.position + P.rotation * (shiftB * b);
    public Quaternion Facing => P.rotation;
    /// <summary>Punto del pasillo junto a la máquina (a su izquierda, a la altura del puesto): ahí llega andando y se sienta.</summary>
    public Vector3 ApproachPoint => transform.TransformPoint(new Vector3(approachSide * 1.05f, 0f, P.localPosition.z));
    [Tooltip("Lado del pasillo por el que se llega: −1 a la izquierda (X−), +1 a la derecha (la primera columna, junto al borde).")]
    public float approachSide = -1f;

    /// <summary>La máquina que ocupa <paramref name="who"/> (null si ninguna).</summary>
    public static GymStation Of(string who) => who != null && _byWho.TryGetValue(who, out GymStation s) && s != null ? s : null;

    /// <summary>Reserva la máquina para <paramref name="who"/> (suelta la que tuviera). false si la ocupa otro.</summary>
    public bool Claim(string who)
    {
        if (string.IsNullOrEmpty(who)) return false;
        if (Occupant == who) return true;
        if (!Free) return false;
        Release(who);
        Occupant = who; _byWho[who] = this;
        return true;
    }

    /// <summary>Deja libre la máquina que ocupaba <paramref name="who"/>.</summary>
    public static void Release(string who)
    {
        if (who == null || !_byWho.TryGetValue(who, out GymStation s)) return;
        _byWho.Remove(who);
        if (s != null && s.Occupant == who) s.Occupant = null;
    }

    /// <summary>La copia <paramref name="copy"/> del tipo <paramref name="t"/> (null si no existe).</summary>
    public static GymStation Find(GymMachineType t, int copy)
    {
        foreach (GymStation s in _all) if (s != null && s.type == t && s.copy == copy) return s;
        return null;
    }

    /// <summary>¿Está <paramref name="g"/> colocado en su máquina? (GroundGuard no lo «baja» del banco de la prensa.)</summary>
    public static bool Seated(GameObject g)
    {
        GymStation s = g != null ? Of(g.name) : null;
        if (s == null) return false;
        Vector3 d = g.transform.position - s.SeatPoint(); d.y = 0f;
        return d.sqrMagnitude < 0.3f * 0.3f;
    }

    public AsanaPoseSpell PoseA() => GymPoses.Pose(type, false, rootHeight);
    public AsanaPoseSpell PoseB() => GymPoses.Pose(type, true, rootHeight);

    public static string Label(GymMachineType t)
    {
        switch (t)
        {
            case GymMachineType.PressPecho: return "Press de pecho";
            case GymMachineType.PressHombros: return "Press de hombros";
            case GymMachineType.Jalon: return "Jalón al pecho";
            case GymMachineType.Remo: return "Remo sentado";
            case GymMachineType.Prensa: return "Prensa de piernas 45°";
            case GymMachineType.ExtensionCuadriceps: return "Extensión de cuádriceps";
            case GymMachineType.CurlFemoral: return "Curl femoral tumbado";
            case GymMachineType.HipThrust: return "Hip thrust";
            case GymMachineType.Gemelos: return "Gemelos sentado";
            default: return "Curl de bíceps y tríceps";
        }
    }
}

/// <summary>
/// POSTURAS DE USO de cada máquina (docs/gimnasio-maquinas.md, tablas «Postura de uso»): A = inicio, B = final de la
/// repetición. Euler locales del rig de cubos; en los pares izquierda/derecha, Z (y Y) cambia de signo en la derecha.
/// </summary>
public static class GymPoses
{
    static (BodyPart, Vector3) P(BodyPart b, float x, float y = 0f, float z = 0f) => (b, new Vector3(x, y, z));

    // Pares simétricos: izquierda (x, y, z), derecha (x, −y, −z).
    static IEnumerable<(BodyPart, Vector3)> Sym(BodyPart l, BodyPart r, float x, float z = 0f)
    {
        yield return (l, new Vector3(x, 0f, z));
        yield return (r, new Vector3(x, 0f, -z));
    }

    static AsanaPoseSpell Build(string name, float root, List<(BodyPart, Vector3)> set)
    {
        AsanaPoseSpell s = DailyPoses.Make(name, root, set.ToArray());
        s.duration = 1.5f; s.reachFraction = 0.6f;
        return s;
    }

    /// <summary>Postura A (<paramref name="b"/> = false) o B de la máquina <paramref name="t"/>.</summary>
    public static AsanaPoseSpell Pose(GymMachineType t, bool b, float root)
    {
        var s = new List<(BodyPart, Vector3)>();
        void Arms(float sx, float sz, float fx) { s.AddRange(Sym(BodyPart.ShoulderLeft, BodyPart.ShoulderRight, sx, sz)); s.AddRange(Sym(BodyPart.ForearmLeft, BodyPart.ForearmRight, fx)); }
        void Legs(float tx, float tz, float sh) { s.AddRange(Sym(BodyPart.ThighLeft, BodyPart.ThighRight, tx, tz)); s.AddRange(Sym(BodyPart.ShinLeft, BodyPart.ShinRight, sh)); }
        string n = GymStation.Label(t) + (b ? " · B" : " · A");
        switch (t)
        {
            case GymMachineType.PressPecho:
                s.Add(P(BodyPart.Hips, -10)); if (b) s.Add(P(BodyPart.Chest, 3));
                if (b) Arms(-80, 12, -10); else Arms(15, 45, -95);
                Legs(-80, 8, 90); break;
            case GymMachineType.PressHombros:
                s.Add(P(BodyPart.Hips, -8)); if (b) s.Add(P(BodyPart.Head, -5));
                if (b) Arms(-165, 12, -10); else Arms(-60, 30, -120);
                Legs(-82, 8, 90); break;
            case GymMachineType.Jalon:
                s.Add(P(BodyPart.Hips, b ? -18 : -12));
                if (b) s.Add(P(BodyPart.Chest, -10)); else s.Add(P(BodyPart.Head, -10));
                if (b) Arms(-15, 20, -135); else Arms(-170, 12, 0);
                Legs(b ? -72 : -78, 8, 95); break;
            case GymMachineType.Remo:
                if (b) s.Add(P(BodyPart.Chest, -5)); else s.Add(P(BodyPart.Back, 5));
                if (b) Arms(35, 12, -95); else Arms(-85, 8, -5);
                Legs(-90, 10, 80); break;
            case GymMachineType.Prensa:
                s.Add(P(BodyPart.Hips, -50)); s.Add(P(BodyPart.Neck, 15)); s.Add(P(BodyPart.Head, 10));
                Arms(-20, 20, -30);
                if (b) Legs(-80, 8, 10); else Legs(-115, 12, 95);
                break;
            case GymMachineType.ExtensionCuadriceps:
                s.Add(P(BodyPart.Hips, -12)); Arms(-10, 18, -20);
                Legs(-78, 6, b ? 5 : 90); break;
            case GymMachineType.CurlFemoral:
                s.Add(P(BodyPart.Hips, 105)); s.Add(P(BodyPart.Neck, -30)); s.Add(P(BodyPart.Head, -15));
                Arms(-90, 15, -30);
                Legs(-15, 5, b ? 105 : 5); break;
            case GymMachineType.HipThrust:
                if (b) { s.Add(P(BodyPart.Hips, -90)); s.Add(P(BodyPart.Neck, 35)); Arms(5, 25, -20); Legs(0, 12, 90); }
                else { s.Add(P(BodyPart.Hips, -35)); s.Add(P(BodyPart.Back, 10)); s.Add(P(BodyPart.Neck, 20)); Arms(10, 25, -20); Legs(-85, 12, 110); }
                root = b ? 0.48f : 0.25f;   // la pelvis sube ~0,22 m
                break;
            case GymMachineType.Gemelos:
                s.Add(P(BodyPart.Back, 15)); Arms(-35, 10, -55);
                Legs(b ? -96 : -90, 6, b ? 96 : 90);
                s.AddRange(Sym(BodyPart.FootLeft, BodyPart.FootRight, b ? 25 : -15)); break;
            default:   // bíceps y tríceps: el brazo quieto sobre el apoyo, solo se mueve el codo
                s.Add(P(BodyPart.Back, 15)); s.Add(P(BodyPart.Chest, 5)); s.Add(P(BodyPart.Head, 5));
                Arms(-30, 10, b ? -125 : -15);
                Legs(-90, 8, 90); break;
        }
        return Build(n, root, s);
    }
}
