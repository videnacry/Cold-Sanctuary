using System.Collections.Generic;
using UnityEngine;

/// <summary>Tipos de estación del gimnasio. Los 10 primeros son las máquinas de placas (docs/gimnasio-maquinas.md; el
/// circuito de 10 estaciones de <see cref="WorkdayDirector"/>). Detrás, sin cambiar el orden, el resto del edificio de
/// entrenamiento (docs/entrenamiento-areas.md §4): peso libre (piso 0), cardio y zona libre (piso 1), calistenia, parkour
/// y crossfit (piso 2).</summary>
public enum GymMachineType
{
    PressPecho, PressHombros, Jalon, Remo, Prensa, ExtensionCuadriceps, CurlFemoral, HipThrust, Gemelos, BicepsTriceps,
    // Piso 0 · peso libre (§4.1)
    SentadillaJaula, PressBancaPlano, PressBancaInclinado, SentadillaSmith, PesoMuerto, RemoMancuerna,
    // Piso 1 · cardio (§4.2) y zona libre (§4.3)
    Cinta, BiciVertical, BiciReclinada, Eliptica, Escaladora, RemoErgometro,
    Crunch, EstiramientoIsquios, SillaRomana, AbdominalDeclinado, Hiperextension, SacoBoxeo,
    // Piso 2 · calistenia (§4.4)
    Dominadas, MonkeyBars, Fondos, FondosAnillas, RemoAustraliano, FrontLever, ElevacionPiernas, LSit, BanderaHumana,
    // Piso 2 · parkour (§4.5)
    KongVault, MuroSubida, Barandilla, SaltoPrecision, Lache,
    // Piso 2 · crossfit (§4.6)
    Kipping, Cargada, AssaultBike, SkiErg, WallBall, SaltoCajon, Swing, Trineo, Cuerda, GHD
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
    [Tooltip("Copia de este tipo (0, 1, 2… por orden de construcción).")]
    public int copy;
    [Tooltip("Hijo «Puesto»: posición de la raíz del usuario (los pies, o la vertical de la pelvis) y su giro.")]
    public Transform puesto;
    [Tooltip("Altura de la pelvis en la postura A, en unidades del cuerpo nativo (de pie = 0,95), para 1,85 m de estatura. La B la saca GymPoses (misma diferencia A→B del doc).")]
    public float rootHeight = 0.6f;
    [Tooltip("Desplazamiento del puesto en la postura B, en ejes del Puesto (+Z = hacia donde mira): hip thrust, sentadillas, peso muerto, remo, saltos, cuerda…")]
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
    /// <summary>Segunda mitad del circuito de placas (tipos 5–9; antes iban en la entreplanta).</summary>
    public bool Upper => (int)type >= 5 && (int)type <= 9;

    Transform P => puesto != null ? puesto : transform;
    /// <summary>Dónde van los pies / la vertical de la pelvis (en el mundo); <paramref name="b"/> = fase B.</summary>
    public Vector3 SeatPoint(float b = 0f) => P.position + P.rotation * (shiftB * b);
    public Quaternion Facing => P.rotation;
    /// <summary>Punto del pasillo junto a la máquina (a su izquierda, a la altura del puesto): ahí llega andando y se sienta.</summary>
    public Vector3 ApproachPoint
    {
        get
        {
            Vector3 p = transform.TransformPoint(new Vector3(approachSide * 1.05f, 0f, P.localPosition.z));
            if (_approachFrom == p) return _approach;
            // Sobre el NavMesh (a donde se puede llegar andando): si el punto del pasillo cae junto a un poste o fuera de la
            // malla, el camino acababa a 0,8 m y no se sentaba nunca.
            _approachFrom = p; _approach = p;
            if (UnityEngine.AI.NavMesh.SamplePosition(p, out UnityEngine.AI.NavMeshHit h, 1.2f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(h.position.y - p.y) < 0.5f) _approach = h.position;
            return _approach;
        }
    }
    Vector3 _approach, _approachFrom = new Vector3(float.NaN, 0f, 0f);
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
        // En cualquier punto entre el puesto de A y el de B (el salto al cajón, el kong, el trineo… desplazan el puesto en B).
        Vector3 a = s.SeatPoint(0f), b = s.SeatPoint(1f), p = g.transform.position; a.y = b.y = p.y = 0f;
        Vector3 ab = b - a; float t = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
        return (p - (a + ab * t)).sqrMagnitude < 0.3f * 0.3f;
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
            case GymMachineType.BicepsTriceps: return "Curl de bíceps y tríceps";
            case GymMachineType.SentadillaJaula: return "Sentadilla en jaula";
            case GymMachineType.PressBancaPlano: return "Press de banca plano";
            case GymMachineType.PressBancaInclinado: return "Press de banca inclinado";
            case GymMachineType.SentadillaSmith: return "Sentadilla en Smith";
            case GymMachineType.PesoMuerto: return "Peso muerto";
            case GymMachineType.RemoMancuerna: return "Remo con mancuerna";
            case GymMachineType.Cinta: return "Cinta de correr";
            case GymMachineType.BiciVertical: return "Bici estática";
            case GymMachineType.BiciReclinada: return "Bici reclinada";
            case GymMachineType.Eliptica: return "Elíptica";
            case GymMachineType.Escaladora: return "Escaladora";
            case GymMachineType.RemoErgometro: return "Remo (ergómetro)";
            case GymMachineType.Crunch: return "Abdominales en esterilla";
            case GymMachineType.EstiramientoIsquios: return "Estiramiento de isquios";
            case GymMachineType.SillaRomana: return "Silla romana";
            case GymMachineType.AbdominalDeclinado: return "Abdominales en banco declinado";
            case GymMachineType.Hiperextension: return "Hiperextensiones";
            case GymMachineType.SacoBoxeo: return "Saco de boxeo";
            case GymMachineType.Dominadas: return "Dominadas";
            case GymMachineType.MonkeyBars: return "Escalera horizontal";
            case GymMachineType.Fondos: return "Fondos";
            case GymMachineType.FondosAnillas: return "Fondos en anillas";
            case GymMachineType.RemoAustraliano: return "Remo australiano";
            case GymMachineType.FrontLever: return "Front lever agrupado";
            case GymMachineType.ElevacionPiernas: return "Elevación de piernas en espalderas";
            case GymMachineType.LSit: return "L-sit";
            case GymMachineType.BanderaHumana: return "Bandera humana";
            case GymMachineType.KongVault: return "Kong vault";
            case GymMachineType.MuroSubida: return "Subida al muro";
            case GymMachineType.Barandilla: return "Equilibrio en barandilla";
            case GymMachineType.SaltoPrecision: return "Salto de precisión";
            case GymMachineType.Lache: return "Balanceo en barra de lâché";
            case GymMachineType.Kipping: return "Dominadas con kipping";
            case GymMachineType.Cargada: return "Cargada";
            case GymMachineType.AssaultBike: return "Assault bike";
            case GymMachineType.SkiErg: return "SkiErg";
            case GymMachineType.WallBall: return "Wall ball";
            case GymMachineType.SaltoCajon: return "Salto al cajón";
            case GymMachineType.Swing: return "Swing con kettlebell";
            case GymMachineType.Trineo: return "Empuje de trineo";
            case GymMachineType.Cuerda: return "Trepa de cuerda";
            default: return "GHD (abdominal)";
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

    static AsanaPoseSpell Build(string name, float root, List<(BodyPart, Vector3)> set, float duration = 1.5f, bool ground = true)
    {
        AsanaPoseSpell s = DailyPoses.Make(name, root, set.ToArray());
        s.duration = duration; s.reachFraction = 0.6f; s.ground = ground;
        return s;
    }

    /// <summary>Segundos de cada mitad de la repetición (A → B o B → A) a ritmo real (docs/entrenamiento-areas.md §4): la
    /// cinta da un paso cada ~0,35 s, la bici una vuelta cada ~0,8 s; las máquinas de placas y el resto, 1,5 s.</summary>
    public static float RepHalf(GymMachineType t)
    {
        switch (t)
        {
            case GymMachineType.Cinta: return 0.35f;
            case GymMachineType.BiciVertical: case GymMachineType.BiciReclinada: case GymMachineType.AssaultBike: return 0.4f;
            case GymMachineType.Eliptica: case GymMachineType.Escaladora: case GymMachineType.Trineo: case GymMachineType.MonkeyBars: return 0.5f;
            case GymMachineType.SacoBoxeo: case GymMachineType.Kipping: case GymMachineType.Lache: case GymMachineType.Swing: return 0.7f;
            case GymMachineType.RemoErgometro: case GymMachineType.SkiErg: case GymMachineType.WallBall: case GymMachineType.KongVault:
            case GymMachineType.SaltoPrecision: case GymMachineType.SaltoCajon: return 1f;
            default: return 1.5f;
        }
    }

    /// <summary>Postura A (<paramref name="b"/> = false) o B de la máquina <paramref name="t"/>. <paramref name="root"/> es la
    /// altura de la pelvis en A; cuando la B la tiene distinta se le suma la diferencia del doc (así las barras a otras
    /// alturas solo cambian el <see cref="GymStation.rootHeight"/>).</summary>
    public static AsanaPoseSpell Pose(GymMachineType t, bool b, float root)
    {
        var s = new List<(BodyPart, Vector3)>();
        void Arms(float sx, float sz, float fx) { s.AddRange(Sym(BodyPart.ShoulderLeft, BodyPart.ShoulderRight, sx, sz)); s.AddRange(Sym(BodyPart.ForearmLeft, BodyPart.ForearmRight, fx)); }
        void Arms0(float fx) => s.AddRange(Sym(BodyPart.ForearmLeft, BodyPart.ForearmRight, fx));   // solo los antebrazos
        void Legs(float tx, float tz, float sh) { s.AddRange(Sym(BodyPart.ThighLeft, BodyPart.ThighRight, tx, tz)); s.AddRange(Sym(BodyPart.ShinLeft, BodyPart.ShinRight, sh)); }
        void Add(BodyPart p, float x, float y = 0f, float z = 0f) => s.Add(P(p, x, y, z));
        // Barra a la espalda (sentadillas): brazos abiertos y codos abajo, antebrazos hacia arriba, manos a la anchura de la barra.
        void BarOnBack() { float w = t == GymMachineType.SentadillaSmith ? 40f : 50f; Add(BodyPart.ShoulderLeft, 0, 0, -w); Add(BodyPart.ShoulderRight, 0, 0, w); Add(BodyPart.ForearmLeft, 0, 0, -2f * w); Add(BodyPart.ForearmRight, 0, 0, 2f * w); }   // (en la Smith, más cerrado: la barra es más corta y los discos quedan más cerca)
        // Lado contra lado (zancadas, pedales, brazos cruzados): en A la izquierda va a xa y la derecha a xb; en B, al revés.
        void Swap(BodyPart l, BodyPart r, float xa, float xb, float z = 0f) { s.Add(P(l, b ? xb : xa, 0f, z)); s.Add(P(r, b ? xa : xb, 0f, -z)); }
        void SwapLegs(float ta, float tb, float sa, float sb) { Swap(BodyPart.ThighLeft, BodyPart.ThighRight, ta, tb); Swap(BodyPart.ShinLeft, BodyPart.ShinRight, sa, sb); }
        float rb = 0f;   // diferencia de la pelvis de A a B
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
            case GymMachineType.BicepsTriceps:   // el brazo quieto sobre el apoyo, solo se mueve el codo
                s.Add(P(BodyPart.Back, 15)); s.Add(P(BodyPart.Chest, 5)); s.Add(P(BodyPart.Head, 5));
                Arms(-30, 10, b ? -125 : -15);
                Legs(-90, 8, 90); break;

            // ── Piso 0 · peso libre (§4.1) ──
            case GymMachineType.SentadillaJaula:   // A 0,95 → B 0,48
                Add(BodyPart.Hips, b ? 40 : 5); if (b) { Add(BodyPart.Neck, -20); Add(BodyPart.Head, -10); }
                BarOnBack();
                if (b) Legs(-130, 15, 125); else Legs(-5, 8, 0);
                rb = -0.47f; break;
            case GymMachineType.PressBancaPlano:   // tumbado boca arriba, cabeza hacia +Z
                Add(BodyPart.Hips, -90); Add(BodyPart.Chest, -5);
                if (b) Arms(-88, 10, -5); else Arms(-10, 55, -100);
                Legs(15, 20, 75); break;
            case GymMachineType.PressBancaInclinado:   // respaldo a 30° de la horizontal
                Add(BodyPart.Hips, -60); Add(BodyPart.Neck, 10);
                if (b) Arms(-120, 10, -5); else Arms(-20, 55, -110);
                Legs(-30, 15, 90); break;
            case GymMachineType.SentadillaSmith:   // A 0,95 → B 0,53
                Add(BodyPart.Hips, b ? 25 : 0); if (b) Add(BodyPart.Neck, -15);
                BarOnBack();
                if (b) Legs(-115, 12, 95); else Legs(-5, 8, 0);
                rb = -0.42f; break;
            case GymMachineType.PesoMuerto:   // A 0,64 (abajo) → B 0,95 (bloqueo)
            case GymMachineType.Cargada:      // la A es la del peso muerto; B = recepción en sentadilla frontal (0,50)
                if (!b) { Add(BodyPart.Hips, 55); Add(BodyPart.Back, 5); Add(BodyPart.Neck, -30); Add(BodyPart.Head, -10); Arms(-55, 5, 0); Legs(-125, 10, 95); }
                else if (t == GymMachineType.PesoMuerto) { Add(BodyPart.Hips, 0); Add(BodyPart.Chest, -5); Arms(0, 5, 0); Legs(0, 8, 0); }
                else { Add(BodyPart.Hips, 25); Add(BodyPart.Neck, -20); Arms(-90, 25, -150); Legs(-120, 15, 120); }
                rb = t == GymMachineType.PesoMuerto ? 0.31f : -0.14f; break;
            case GymMachineType.RemoMancuerna:   // rodilla y mano izquierdas en el banco, rema la derecha
                Add(BodyPart.Hips, 80); if (b) Add(BodyPart.Chest, 0, -10); Add(BodyPart.Neck, -20);
                Add(BodyPart.ShoulderLeft, -75, 0, 5); Add(BodyPart.ForearmLeft, -20);
                if (b) { Add(BodyPart.ShoulderRight, 15, 0, -10); Add(BodyPart.ForearmRight, -90); } else Add(BodyPart.ShoulderRight, -80, 0, -5);
                Add(BodyPart.ThighLeft, -80); Add(BodyPart.ShinLeft, 90); Add(BodyPart.ThighRight, -80, 0, -10); break;

            // ── Piso 1 · cardio (§4.2) ──
            case GymMachineType.Cinta:
                Add(BodyPart.Hips, 8); Swap(BodyPart.ShoulderLeft, BodyPart.ShoulderRight, 25, -40, 8); Arms0(-90);
                SwapLegs(-35, 20, 15, 80); break;
            case GymMachineType.BiciVertical:
                Add(BodyPart.Hips, 20); Add(BodyPart.Neck, -15); Arms(-60, 10, -20);
                SwapLegs(-90, -40, 110, 30); break;
            case GymMachineType.BiciReclinada:
                Add(BodyPart.Hips, -25); Arms(-10, 20, -40);
                SwapLegs(-85, -55, 100, 25); break;
            case GymMachineType.Eliptica:
                Add(BodyPart.Hips, 5); Swap(BodyPart.ShoulderLeft, BodyPart.ShoulderRight, -20, -55, 10); Swap(BodyPart.ForearmLeft, BodyPart.ForearmRight, -90, -45);
                SwapLegs(-30, 10, 40, 20); break;
            case GymMachineType.Escaladora:
                Add(BodyPart.Hips, 10); Arms(-30, 15, -50);
                SwapLegs(-50, 0, 60, 5); break;
            case GymMachineType.RemoErgometro:   // ataque → final (el carro va 0,6 m atrás: shiftB)
                Add(BodyPart.Hips, b ? -25 : 25);
                if (b) { Arms(35, 12, -120); Legs(-60, 5, 5); } else { Arms(-85, 8, 0); Legs(-150, 10, 125); }
                break;

            // ── Piso 1 · zona libre (§4.3) ──
            case GymMachineType.Crunch:
                Add(BodyPart.Hips, -90); if (b) { Add(BodyPart.Back, 30); Add(BodyPart.Chest, 20); Add(BodyPart.Neck, 15); }
                Arms(-150, 60, -130); Legs(-45, 10, 90); break;
            case GymMachineType.EstiramientoIsquios:
                if (b) { Add(BodyPart.Hips, 30); Add(BodyPart.Back, 25); Add(BodyPart.Chest, 20); Add(BodyPart.Neck, 15); Arms(-120, 8, 0); Legs(-120, 5, 0); }
                else { Arms(-60, 10, 0); Legs(-90, 5, 0); }
                break;
            case GymMachineType.SillaRomana:
                if (b) { Add(BodyPart.Hips, -10); Add(BodyPart.Back, 5); }
                Arms(-10, 10, -90);
                if (b) Legs(-100, 8, 100); else Legs(0, 5, 0);
                break;
            case GymMachineType.AbdominalDeclinado:   // pies enganchados en el extremo alto
                Add(BodyPart.Hips, b ? -45 : -120); if (b) Add(BodyPart.Back, 10);
                Arms(-40, 20, -130); Legs(b ? -80 : -5, 5, 100); break;   // muslos a lo largo del banco, tobillos entre los rodillos
            case GymMachineType.Hiperextension:
                Add(BodyPart.Hips, b ? 45 : 125); if (b) Add(BodyPart.Neck, -20);
                Arms(-40, 20, -130); Legs(b ? 0 : -80, 5, 0); break;
            case GymMachineType.SacoBoxeo:   // de perfil, pie izquierdo delante: guardia → directo de izquierda
                Add(BodyPart.Hips, b ? 8 : 5, b ? -30 : -20);
                if (b) { Add(BodyPart.ShoulderLeft, -90, 0, 5); Add(BodyPart.ForearmLeft, -5); } else { Add(BodyPart.ShoulderLeft, -40, 0, 15); Add(BodyPart.ForearmLeft, -120); }
                Add(BodyPart.ShoulderRight, -30, 0, -15); Add(BodyPart.ForearmRight, -130);
                Add(BodyPart.ThighLeft, b ? -20 : -15, 0, 8); Add(BodyPart.ShinLeft, b ? 15 : 10);
                Add(BodyPart.ThighRight, 10, 0, -8); Add(BodyPart.ShinRight, 15); break;

            // ── Piso 2 · calistenia (§4.4) ──
            case GymMachineType.Dominadas:   // barra a 2,4 m: A 1,33 (manos en la barra) → B 1,89 (barbilla sobre la barra)
                Add(BodyPart.Hips, b ? -5 : 0); Add(BodyPart.Neck, b ? -15 : -10);
                if (b) Arms(-30, 25, -140); else Arms(-175, 10, 0);   // (agarre algo más cerrado: con alguien en la barra de al lado no se tocan los antebrazos)
                Legs(b ? -15 : -10, 0, b ? 70 : 60); rb = 0.56f; break;
            case GymMachineType.MonkeyBars:
                Add(BodyPart.Hips, b ? -5 : 5); Swap(BodyPart.ShoulderLeft, BodyPart.ShoulderRight, -175, -150, 10); Arms0(0);
                SwapLegs(-20, 10, 30, 30); break;
            case GymMachineType.Fondos:          // A 1,39 → B 1,04 (las manos no se mueven de las barras)
            case GymMachineType.FondosAnillas:   // igual, con los brazos más abiertos (anillas a ±0,25 m)
            {
                float z = t == GymMachineType.FondosAnillas ? -2f : (b ? 8f : 5f);
                Add(BodyPart.Hips, b ? 20 : 5);
                if (b) Arms(85, z, -90); else Arms(0, z, 0);
                Legs(b ? -15 : -10, 0, b ? 80 : 70); rb = t == GymMachineType.FondosAnillas ? -0.33f : -0.35f; break;
            }
            case GymMachineType.RemoAustraliano:   // A 0,30 → B 0,55 (las manos siguen en la barra: el pecho sube hacia ella)
                Add(BodyPart.Hips, b ? -50 : -72);
                if (b) Arms(-100, 30, -100); else Arms(-108, 10, 0);
                Legs(0, 5, 0); rb = 0.25f; break;
            case GymMachineType.FrontLever:   // A 0,62 → B 1,18
                Add(BodyPart.Hips, b ? -90 : 0); Arms(b ? -90 : -175, 12, 0);
                Legs(b ? -120 : -130, 0, 130); rb = 0.56f; break;   // en B la raíz avanza 0,52 m (shiftB): los hombros siguen bajo la barra
            case GymMachineType.ElevacionPiernas:   // de espaldas a las espalderas: brazos algo hacia atrás, al peldaño de 2,24 m
                Add(BodyPart.Hips, b ? -5 : 0); Arms(-190, 20, 0); Legs(b ? -90 : 0, 0, 0); break;
            case GymMachineType.LSit:
                Add(BodyPart.Hips, b ? -5 : 0); Add(BodyPart.Back, b ? 5 : 10); Arms(5, 8, 0);
                if (b) Legs(-90, 0, 0); else Legs(-110, 0, 120);
                break;
            case GymMachineType.BanderaHumana:   // poste a 0,75 m a la izquierda; A 1,50 (agrupada) → B 1,55 (horizontal), cabeza hacia el poste
                Add(BodyPart.Hips, 0, 0, b ? 90 : 75);
                Add(BodyPart.ShoulderLeft, 0, 0, 106); Add(BodyPart.ShoulderRight, 0, 0, -103);   // manos en los agarres de 1,3 y 1,7 m
                if (b) Legs(0, 12, 0); else Legs(-100, 5, 110);
                rb = 0.05f; break;

            // ── Piso 2 · parkour (§4.5) ──
            case GymMachineType.KongVault:   // batida → sobre el cajón; A 0,80 → B 1,65
                if (b) { Add(BodyPart.Hips, 80); Add(BodyPart.Neck, -40); Arms(-80, 15, 0); Legs(-110, 0, 120); }
                else { Add(BodyPart.Hips, 15); Arms(-90, 15, -10); Add(BodyPart.ThighLeft, -50); Add(BodyPart.ShinLeft, 70); Add(BodyPart.ThighRight, 25); Add(BodyPart.ShinRight, 20); }
                rb = 0.85f; break;
            case GymMachineType.MuroSubida:   // colgado del borde (2,4 m) → empuje (mantle) con la rodilla en lo alto; A 1,59 → B 2,74
                if (b) { Add(BodyPart.Hips, 40); Arms(-40, 15, 0); Add(BodyPart.ThighLeft, -110); Add(BodyPart.ShinLeft, 130); Add(BodyPart.ThighRight, 10); Add(BodyPart.ShinRight, 20); }
                else { Arms(-155, 20, -10); Legs(-15, 0, 45); }   // manos sobre el borde, piernas contra el muro
                rb = 1.15f; break;
            case GymMachineType.Barandilla:   // andar con los brazos abiertos → cuclillas de precisión; A 1,46 → B 1,00
                if (b) { Add(BodyPart.Hips, 40); Arms(-70, 10, -10); Legs(-130, -9, 125); }   // (muslos hacia dentro: los dos pies sobre el tubo de 5 cm)
                else { Arms(0, 80, -10); Add(BodyPart.ThighLeft, -20, 0, -9); Add(BodyPart.ShinLeft, 10); Add(BodyPart.ThighRight, 10, 0, 9); Add(BodyPart.ShinRight, 15); }
                rb = -0.46f; break;
            case GymMachineType.SaltoPrecision:   // batida → aterrizaje; A 1,11 → B 1,16
                Add(BodyPart.Hips, b ? 35 : 45); Arms(b ? -80 : 60, 10, -10); Legs(b ? -115 : -110, 8, b ? 110 : 100);
                rb = 0.05f; break;
            case GymMachineType.Lache:   // balanceo en X (el puesto mira hacia ±X)
                Add(BodyPart.Hips, b ? -35 : 20); Add(BodyPart.Back, b ? 10 : -10); Arms(-175, 12, 0);
                Legs(b ? -70 : -30, 0, b ? 30 : 90); rb = 0.06f; break;   // en B la raíz va 0,65 m hacia delante (shiftB): las manos no dejan la barra

            // ── Piso 2 · crossfit (§4.6) ──
            case GymMachineType.Kipping:   // arco ↔ hueco
                Add(BodyPart.Hips, b ? -20 : 15); Add(BodyPart.Back, b ? 10 : -15); Arms(b ? -160 : -175, 15, 0);
                Legs(b ? -20 : 10, 0, b ? 0 : 10); break;
            case GymMachineType.AssaultBike:
                Add(BodyPart.Hips, 15);
                Swap(BodyPart.ShoulderLeft, BodyPart.ShoulderRight, -30, -70, 10); Swap(BodyPart.ForearmLeft, BodyPart.ForearmRight, -90, -20);
                SwapLegs(-95, -65, 105, 70); break;   // (la pierna de delante no tan estirada: el pie, en el pedal y no en el ventilador)
            case GymMachineType.SkiErg:   // A 0,95 → B 0,89
                Add(BodyPart.Hips, b ? 45 : 0);
                if (b) { Arms(-35, 10, -5); Legs(-65, 5, 35); } else { Arms(-160, 15, -10); Legs(0, 5, 0); }
                rb = -0.06f; break;
            case GymMachineType.WallBall:   // sentadilla con el balón → lanzamiento; A 0,49 → B 0,95
                Add(BodyPart.Hips, b ? 0 : 30); Add(BodyPart.Neck, b ? -15 : -20); if (b) Add(BodyPart.Head, -10);
                if (b) { Arms(-165, 15, -10); Legs(0, 8, 0); } else { Arms(-40, 35, -130); Legs(-125, 15, 120); }
                rb = 0.46f; break;
            case GymMachineType.SaltoCajon:   // A 0,60 → B 1,24 (la raíz avanza 0,5 m: shiftB)
                Add(BodyPart.Hips, b ? 30 : 45); Arms(b ? -80 : 50, 10, 0); Legs(-110, 10, b ? 105 : 100);
                rb = 0.64f; break;
            case GymMachineType.Swing:   // A 0,88 → B 0,95
                Add(BodyPart.Hips, b ? 0 : 60); if (!b) Add(BodyPart.Neck, -30);
                Arms(b ? -90 : -40, 5, 0); Legs(b ? 0 : -80, 10, b ? 0 : 30);
                rb = 0.07f; break;
            case GymMachineType.Trineo:   // manos en los postes (1,0 m)
                Add(BodyPart.Hips, 55); Arms(-115, 10, -20);
                SwapLegs(-110, -30, 100, 20); break;
            case GymMachineType.Cuerda:   // la raíz sube 0,6 m por ciclo (shiftB)
                if (b) { Add(BodyPart.ShoulderLeft, -40, 0, 45); Add(BodyPart.ShoulderRight, -60, 0, -45); Arms0(-130); Legs(-20, 0, 20); }   // manos juntas en la cuerda
                else { Add(BodyPart.ShoulderLeft, -170, 0, 24); Add(BodyPart.ShoulderRight, -150, 0, -24); Arms0(-10); Legs(-100, 0, 100); }
                break;
            case GymMachineType.GHD:   // cadera en la almohadilla (~1,0 m), pies enganchados
                Add(BodyPart.Hips, b ? -10 : -110); Add(BodyPart.Back, b ? 10 : -10);
                Arms(b ? -90 : -170, b ? 10 : 15, 0); Legs(b ? -80 : 20, 0, 50); break;   // rodillas dobladas: tobillos bajo los rodillos
        }
        // Los equipos nuevos (tras el curl de bíceps) dan la pelvis exacta (colgado, sobre un banco, un pedal o un cajón): sin
        // apoyar el punto más bajo en el suelo, que bajaba al que colgaba de la barra hasta sentarlo en el piso.
        return Build(n, b ? root + rb : root, s, Mathf.Min(1.5f, RepHalf(t)), (int)t <= (int)GymMachineType.BicepsTriceps);
    }
}
