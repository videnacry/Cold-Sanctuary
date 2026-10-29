using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL CUERPO SE AJUSTA A LA MÁQUINA (Berón 2026-10-29: «todas las máquinas deben poder usarse»). Las posturas A/B de
/// <see cref="GymPoses"/> están pensadas para 1,85 m, pero el personal mide de 1,55 a 1,8 m y el equipo no cambia: una barra
/// a 2,4 m no baja para quien mide 1,55. Cada fotograma de uso (<see cref="WorkdayDirector.GymRep"/>), tras aplicar la
/// postura:
///  · ALTURA: si la postura se apoya en los pies, el pie más bajo toca lo que pisa (la banda de la cinta, el cajón, el
///    bloque…); si cuelga de las manos, el cuerpo sube o baja hasta que las manos llegan al agarre; y ningún pie queda
///    metido en el suelo.
///  · MANOS: las que agarran van al punto más cercano de su agarre (IK de dos huesos, como el alcance del yoga).
/// También sabe qué piezas de cada estación son agarres, macizas o superficies (<see cref="Of"/>), para que
/// <see cref="GymUsabilityTest"/> mida con las mismas reglas.
/// </summary>
public static class GymFit
{
    public class Equipment
    {
        public readonly List<Transform> solid = new List<Transform>(), grip = new List<Transform>(), surface = new List<Transform>();
        public Equipment() { }
        public Equipment(Transform only) { surface.Add(only); }
    }

    /// <summary>Puntos a lo largo del eje largo de un segmento del cuerpo (cubo unidad escalado): centro y ±0,2/±0,4 de su largo.</summary>
    public static IEnumerable<Vector3> AxisSamples(Transform seg)
    {
        Vector3 ls = seg.lossyScale; Vector3 ax = Vector3.right; float len = Mathf.Abs(ls.x);
        if (Mathf.Abs(ls.y) > len) { ax = Vector3.up; len = Mathf.Abs(ls.y); }
        if (Mathf.Abs(ls.z) > len) ax = Vector3.forward;
        foreach (float f in new[] { -0.4f, -0.2f, 0f, 0.2f, 0.4f }) yield return seg.TransformPoint(ax * f);
    }

    /// <summary>Medio grosor (la dimensión más fina) de un segmento del cuerpo.</summary>
    public static float HalfThickness(Transform seg) { Vector3 s = seg.lossyScale; return Mathf.Min(Mathf.Abs(s.x), Mathf.Min(Mathf.Abs(s.y), Mathf.Abs(s.z))) * 0.5f; }

    static readonly Dictionary<GymStation, Equipment> _eq = new Dictionary<GymStation, Equipment>();
    static readonly Dictionary<GymStation, float> _lastDy = new Dictionary<GymStation, float>();

    static readonly string[] GripNames = { "Agarre", "Asa", "Asidero", "Barra", "Mango", "Tirador", "Manillar", "Pasamanos", "Anilla", "Peldano",
                                          "Poste_empuje", "Cuerda", "Maneta", "Tubo", "Barandilla" };
    static readonly string[] Ignored = { "Cable", "Cinta_anilla", "Cadena", "Marca", "Diana", "Tejido", "Dibujo", "Flecos", "Zona", "Borde" };

    /// <summary>¿La pieza <paramref name="n"/> se agarra en la estación de tipo <paramref name="t"/>?</summary>
    public static bool IsGrip(GymMachineType t, string n)
    {
        if (n == "Brazo" && t != GymMachineType.SillaRomana) return true;   // brazos de la elíptica y del assault bike (en la silla romana son soportes)
        if (t == GymMachineType.MuroSubida && n.StartsWith("Muro")) return true;     // el borde del muro
        if (t == GymMachineType.KongVault && (n.StartsWith("Cajon") || n.StartsWith("Tapa"))) return true;   // las manos sobre el cajón
        foreach (string g in GripNames) if (n.StartsWith(g)) return true;
        return false;
    }

    /// <summary>Las piezas (cubos) de la estación cerca de su puesto: agarres, macizas y todo lo que se pisa (con el suelo del piso).</summary>
    public static Equipment Of(GymStation st)
    {
        if (st == null) return new Equipment();
        if (_eq.TryGetValue(st, out Equipment e) && e.surface.All(t => t != null)) return e;
        e = new Equipment();
        Transform root = st.name.StartsWith("Sitio_") && st.transform.parent != null ? st.transform.parent : st.transform;
        Vector3 c = (st.SeatPoint(0f) + st.SeatPoint(1f)) * 0.5f + Vector3.up * 0.9f;
        float reach = 2.2f + st.shiftB.magnitude;
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null || !mf.sharedMesh.name.StartsWith("Cube")) continue;
            Renderer r = mf.GetComponent<Renderer>(); if (r == null || !r.enabled) continue;
            Transform t = mf.transform;
            if (t.GetComponentInParent<Anima>() != null || t.GetComponentInParent<FlyingCarpet>() != null) continue;
            if (Ignored.Any(n => t.name.StartsWith(n))) continue;
            if (Dist(c, t) > reach) continue;
            e.surface.Add(t);
            if (IsGrip(st.type, t.name)) e.grip.Add(t); else e.solid.Add(t);
        }
        // El suelo del piso (caucho, losetas, césped): lo que pisan.
        for (Transform a = root; a != null; a = a.parent)
        {
            bool floor = false;
            foreach (Transform ch in a) if (ch.name.StartsWith("Suelo_caucho")) { floor = true; break; }
            if (!floor) continue;
            foreach (Transform ch in a)
                if ((ch.name.StartsWith("Suelo") || ch.name.StartsWith("Loseta") || ch.name.StartsWith("Pista")) && !e.surface.Contains(ch)) e.surface.Add(ch);
            break;
        }
        _eq[st] = e;
        return e;
    }

    // ── Geometría de cubos ─────────────────────────────────────────────────────────────────────

    /// <summary>Cuánto está <paramref name="p"/> DENTRO de la pieza (cubo unidad escalado), en metros; ≤ 0 = fuera.</summary>
    public static float Depth(Vector3 p, Transform box)
    {
        Vector3 l = box.InverseTransformPoint(p), s = box.lossyScale;
        return Mathf.Min((0.5f - Mathf.Abs(l.x)) * Mathf.Abs(s.x), Mathf.Min((0.5f - Mathf.Abs(l.y)) * Mathf.Abs(s.y), (0.5f - Mathf.Abs(l.z)) * Mathf.Abs(s.z)));
    }

    /// <summary>El punto de la pieza más cercano a <paramref name="p"/>.</summary>
    public static Vector3 Closest(Vector3 p, Transform box)
    {
        Vector3 l = box.InverseTransformPoint(p);
        return box.TransformPoint(new Vector3(Mathf.Clamp(l.x, -0.5f, 0.5f), Mathf.Clamp(l.y, -0.5f, 0.5f), Mathf.Clamp(l.z, -0.5f, 0.5f)));
    }

    /// <summary>Distancia de <paramref name="p"/> a la pieza (0 si está dentro).</summary>
    public static float Dist(Vector3 p, Transform box) => Vector3.Distance(p, Closest(p, box));

    /// <summary>Lo alto de lo que hay justo debajo de <paramref name="from"/> (piezas de la estación con el suelo del piso o,
    /// si no hay, un collider que no sea de una máquina ni de alguien). −∞ si nada.</summary>
    public static float SurfaceBelow(Vector3 from, Equipment eq, Transform self, out string on)
    {
        float best = float.NegativeInfinity; on = "nada";
        foreach (Transform piece in eq.surface)
        {
            if (piece == null) continue;
            Vector3 o = piece.InverseTransformPoint(from), d = piece.InverseTransformVector(Vector3.down);
            float tmin = float.NegativeInfinity, tmax = float.PositiveInfinity;
            bool miss = false;
            for (int i = 0; i < 3 && !miss; i++)
            {
                if (Mathf.Abs(d[i]) < 1e-6f) { if (Mathf.Abs(o[i]) > 0.5f) miss = true; continue; }
                float t1 = (-0.5f - o[i]) / d[i], t2 = (0.5f - o[i]) / d[i];
                tmin = Mathf.Max(tmin, Mathf.Min(t1, t2)); tmax = Mathf.Min(tmax, Mathf.Max(t1, t2));
            }
            if (miss || tmax < Mathf.Max(0f, tmin)) continue;
            float y = from.y - Mathf.Max(0f, tmin);   // (si ya está dentro, la superficie está al menos a la altura de partida)
            if (y > best) { best = y; on = piece.name; }
        }
        if (float.IsNegativeInfinity(best))
            foreach (RaycastHit h in Physics.RaycastAll(from, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore))
                if ((self == null || !h.transform.IsChildOf(self)) && h.collider.GetComponentInParent<GymStation>() == null && h.collider.GetComponentInParent<Anima>() == null && h.point.y > best)
                { best = h.point.y; on = h.collider.name; }
        return best;
    }

    /// <summary>Hueco entre un pie y lo que pisa (m): NEGATIVO = hundido (algún punto del pie —talón, centro, punta o la
    /// planta— dentro de una pieza: el bloque, el peldaño, el reposapiés, el suelo…; lo que falta hasta su parte de arriba);
    /// si no, lo que la planta flota sobre lo más cercano que tiene debajo el talón, el centro o la punta.</summary>
    public static float FootGap(Transform footSeg, Equipment eq, Transform self, out string on)
    {
        float bottom = LowestY(footSeg);
        float sunk = 0f; on = "nada";
        var soles = new List<Vector3>();
        foreach (Vector3 a in AxisSamples(footSeg))   // el talón, el centro y la punta
        {
            Vector3 s = new Vector3(a.x, bottom + 0.01f, a.z);
            soles.Add(s);
            foreach (Vector3 q in new[] { a, s })
                foreach (Transform piece in eq.surface)
                {
                    if (piece == null || Depth(q, piece) <= 0f) continue;
                    float top = SurfaceBelow(new Vector3(q.x, q.y + 3f, q.z), new Equipment(piece), null, out _);
                    if (top - bottom > sunk) { sunk = top - bottom; on = piece.name; }
                }
        }
        if (sunk > 0f) return -sunk;
        float gap = float.PositiveInfinity;
        foreach (Vector3 s in soles)
        {
            float top = SurfaceBelow(s, eq, self, out string o);
            if (!float.IsNegativeInfinity(top) && bottom - top < gap) { gap = bottom - top; on = o; }
        }
        return gap;
    }

    public static float LowestY(Transform cube)
    {
        float y = float.MaxValue;
        for (int c = 0; c < 8; c++) y = Mathf.Min(y, cube.TransformPoint(new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? -0.5f : 0.5f, (c & 4) == 0 ? -0.5f : 0.5f)).y);
        return y;
    }

    public static Transform Segment(CreatureRig rig, BodyPart part)
    {
        Transform b = rig.Get(part); if (b == null) return null;
        CubeJoint j = b.GetComponent<CubeJoint>();
        return j != null ? j.segment : null;
    }

    // ── Qué agarra y en qué se apoya cada postura ───────────────────────────────────────────────

    /// <summary>Manos que AGARRAN algo de la máquina en cada fase: 2, 1, 0; −2 = las dos, en A o en B (palancas que se
    /// mueven: la pieza del agarre está fija en una de las dos posiciones).</summary>
    public static int Hands(GymMachineType t, bool b)
    {
        switch (t)
        {
            case GymMachineType.PressPecho: case GymMachineType.PressHombros: case GymMachineType.Remo: case GymMachineType.BicepsTriceps:
            case GymMachineType.PressBancaPlano: case GymMachineType.PressBancaInclinado: case GymMachineType.Eliptica: case GymMachineType.AssaultBike:
            case GymMachineType.Jalon: case GymMachineType.SentadillaJaula: case GymMachineType.SentadillaSmith: case GymMachineType.PesoMuerto:
            case GymMachineType.Cargada: case GymMachineType.RemoErgometro: case GymMachineType.SkiErg:
                return 2;   // (palancas, cables y barras van con las manos: GymFit.MoveGrips; los brazos de la elíptica, por IK)
            case GymMachineType.Prensa: case GymMachineType.ExtensionCuadriceps: case GymMachineType.CurlFemoral:
            case GymMachineType.BiciVertical: case GymMachineType.BiciReclinada: case GymMachineType.Escaladora: case GymMachineType.SillaRomana:
            case GymMachineType.Dominadas: case GymMachineType.Fondos: case GymMachineType.FondosAnillas: case GymMachineType.RemoAustraliano:
            case GymMachineType.FrontLever: case GymMachineType.ElevacionPiernas: case GymMachineType.LSit: case GymMachineType.BanderaHumana:
            case GymMachineType.MuroSubida: case GymMachineType.Lache: case GymMachineType.Kipping: case GymMachineType.Trineo: case GymMachineType.Cuerda:
                return 2;
            case GymMachineType.MonkeyBars: return 1;
            case GymMachineType.KongVault: return b ? 2 : 0;
            default: return 0;   // cinta, esterillas, banco declinado, hiperextensiones, saco, barandilla, saltos, wall ball, swing, GHD, hip thrust, gemelos, remo con mancuerna
        }
    }

    /// <summary>Pies que APOYAN en algo en cada fase (los que deben tocar su superficie).</summary>
    public static int Feet(GymMachineType t, bool b)
    {
        switch (t)
        {
            case GymMachineType.PressPecho: case GymMachineType.PressHombros: case GymMachineType.Jalon: case GymMachineType.Remo:
            case GymMachineType.HipThrust: case GymMachineType.Gemelos: case GymMachineType.BicepsTriceps:
            case GymMachineType.SentadillaJaula: case GymMachineType.PressBancaPlano: case GymMachineType.PressBancaInclinado:
            case GymMachineType.SentadillaSmith: case GymMachineType.PesoMuerto: case GymMachineType.Crunch: case GymMachineType.EstiramientoIsquios:
            case GymMachineType.SacoBoxeo: case GymMachineType.RemoAustraliano: case GymMachineType.SaltoPrecision: case GymMachineType.Cargada:
            case GymMachineType.SkiErg: case GymMachineType.WallBall: case GymMachineType.SaltoCajon: case GymMachineType.Swing:
                return 2;
            case GymMachineType.RemoMancuerna: case GymMachineType.Cinta: case GymMachineType.Escaladora: case GymMachineType.Trineo: return 1;
            case GymMachineType.KongVault: return b ? 0 : 1;
            case GymMachineType.Barandilla: return b ? 2 : 1;
            default: return 0;   // colgados, sentados con las piernas libres, pedales (fijos), tumbados con los pies enganchados
        }
    }

    /// <summary>¿La postura CUELGA de las manos (o se sostiene en ellas)? Entonces el cuerpo se coloca a la altura del agarre.</summary>
    public static bool Hangs(GymMachineType t, bool b)
    {
        switch (t)
        {
            case GymMachineType.Dominadas: case GymMachineType.MonkeyBars: case GymMachineType.Fondos: case GymMachineType.FondosAnillas:
            case GymMachineType.FrontLever: case GymMachineType.ElevacionPiernas: case GymMachineType.LSit: case GymMachineType.BanderaHumana:
            case GymMachineType.Lache: case GymMachineType.Kipping: case GymMachineType.Cuerda: case GymMachineType.SillaRomana:
                return true;
            case GymMachineType.MuroSubida: return !b;
            default: return false;
        }
    }

    /// <summary>Lo que <see cref="Fit"/> subió o bajó el cuerpo la última vez en esa estación (m).</summary>
    public static bool Debug_;
    public static float LastDy(GymStation st) => st != null && _lastDy.TryGetValue(st, out float d) ? d : 0f;

    // ── El ajuste ────────────────────────────────────────────────────────────────────────────────

    /// <summary>Ajusta el cuerpo de <paramref name="g"/> (ya colocado en el puesto y con la postura aplicada) a la máquina:
    /// altura por los pies o por las manos y manos a sus agarres. Las 10 máquinas de placas (posturas apoyadas en el
    /// suelo del cuerpo) solo ajustan las manos.</summary>
    public static void Fit(GymStation st, GameObject g, bool b)
    {
        if (st == null || g == null) return;
        CreatureRig rig = g.GetComponent<CreatureRig>(); if (rig == null) return;
        Equipment eq = Of(st);
        int feet = Feet(st.type, b), hands = Hands(st.type, b);
        bool legacy = (int)st.type <= (int)GymMachineType.BicepsTriceps;
        float dy = 0f;
        {
            float minGap = float.MaxValue;
            foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
            {
                Transform seg = Segment(rig, f); if (seg == null) continue;
                float gap = FootGap(seg, eq, g.transform, out _);
                if (!float.IsInfinity(gap)) minGap = Mathf.Min(minGap, gap);
            }
            if (legacy) { if (minGap < 0f) dy = -minGap; }   // placas (postura apoyada): solo sacar un pie hundido (en el reposapiés, el bastidor…)
            else if (feet > 0 && minGap < float.MaxValue) dy = -minGap;   // el pie más bajo, sobre lo que pisa
            else
            {
                if (Hangs(st.type, b) && hands > 0)
                {
                    float sum = 0f; int n = 0;
                    foreach (var (hand, target) in HandTargets(rig, eq, hands, 0.9f)) { sum += target.y - hand.position.y; n++; }
                    if (n > 0) dy = sum / n;
                }
                if (minGap < float.MaxValue && minGap + dy < 0f) dy = -minGap;   // ningún pie dentro del suelo
            }
            dy = Mathf.Clamp(dy, -0.8f, 0.8f);
            if (Debug_) Debug.Log($"[GymFitDbg] f{Time.frameCount} {st.name} {g.name} b={b} feet={feet} minGap={minGap:0.000} dy={dy:0.000} legacy={legacy} planta={LowestY(Segment(rig, BodyPart.FootLeft)) - st.SeatPoint(b ? 1f : 0f).y:0.000}");
            if (Mathf.Abs(dy) > 1e-4f) g.transform.position += Vector3.up * dy;
            // Al subir o bajar, el pie puede dar con otra pieza (el reposapiés justo encima del bastidor): se repite hasta que
            // ningún pie quede hundido.
            for (int it = 0; it < 4; it++)
            {
                float sunk = 0f;
                foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
                {
                    Transform seg = Segment(rig, f); if (seg == null) continue;
                    float gap = FootGap(seg, eq, g.transform, out _);
                    if (gap < -sunk) sunk = -gap;
                }
                if (sunk < 0.005f) break;
                g.transform.position += Vector3.up * sunk; dy += sunk;
            }
        }
        // SENTADO SOBRE el asiento o el banco, no dentro: la pelvis y los muslos salen por arriba de lo que atraviesan (en
        // las máquinas de placas, quien es más bajo que 1,85 m quedaba con la cadera metida en el asiento).
        if (legacy) AdjustSeat(st, rig, eq);   // las de placas tienen el asiento REGULABLE: baja o sube a la medida de quien se sienta
        else
        {
            float up = 0f;
            foreach (BodyPart p in new[] { BodyPart.Hips, BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.Back, BodyPart.Chest })
            {
                Transform seg = Segment(rig, p); if (seg == null) continue;
                foreach (Vector3 pt in AxisSamples(seg))
                    foreach (Transform piece in eq.solid)
                    {
                        if (piece == null) continue;
                        float d = Depth(pt, piece); if (d <= 0f) continue;
                        float top = SurfaceBelow(new Vector3(pt.x, pt.y + 0.3f, pt.z), new Equipment(piece), null, out _);
                        if (!float.IsNegativeInfinity(top) && top - pt.y < 0.15f) up = Mathf.Max(up, top - pt.y + HalfThickness(seg));   // (solo lo que tiene la cara de arriba cerca: un banco, no un poste)
                    }
            }
            up = Mathf.Min(up, 0.2f);
            if (up > 0f) { g.transform.position += Vector3.up * up; dy += up; }
        }
        // PIES AL SUELO: con los dos pies apoyados, el que no llega (piernas cortas en un banco alto) baja hasta lo que pisa.
        if (feet >= 2)
            foreach (bool left in new[] { true, false })
            {
                Transform foot = rig.Get(left ? BodyPart.FootLeft : BodyPart.FootRight), seg = Segment(rig, left ? BodyPart.FootLeft : BodyPart.FootRight);
                Transform th = rig.Get(left ? BodyPart.ThighLeft : BodyPart.ThighRight), sh = rig.Get(left ? BodyPart.ShinLeft : BodyPart.ShinRight);
                if (foot == null || seg == null || th == null || sh == null) continue;
                float gap = FootGap(seg, eq, g.transform, out _);
                if (gap <= 0.01f || gap > 0.3f) continue;
                TwoBoneIK.Solve(th, sh, foot, foot.position - Vector3.up * gap, sh.position + g.transform.forward * 0.3f, 1f);
                rig.LockPose(left ? BodyPart.ThighLeft : BodyPart.ThighRight); rig.LockPose(left ? BodyPart.ShinLeft : BodyPart.ShinRight);
            }
        _lastDy[st] = dy;
        // LA ALMOHADILLA DEL HIP THRUST va encima de la cadera (es la palanca que se empuja con ella).
        if (st.type == GymMachineType.HipThrust) RideOnHips(st, rig, eq);
        // AGARRES QUE SE MUEVEN (palancas, cables, barras): van con las manos.
        if (MoveGrips(st, rig, eq)) return;
        // MANOS al agarre fijo (las que agarran en esta fase; en las de palancas, solo si el agarre está a mano en esta fase).
        if (hands == 0) return;
        float reach = hands == -2 ? 0.3f : 0.9f;
        foreach (var (hand, target) in HandTargets(rig, eq, hands == -2 ? 2 : hands, reach))
        {
            bool left = hand == rig.Get(BodyPart.HandLeft);
            Transform sh = rig.Get(left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight), el = rig.Get(left ? BodyPart.ForearmLeft : BodyPart.ForearmRight);
            if (sh == null || el == null) continue;
            Vector3 mid = (sh.position + hand.position) * 0.5f, bend = el.position - mid;
            if (bend.sqrMagnitude < 1e-4f) bend = -g.transform.up * 0.1f + g.transform.right * (left ? -0.1f : 0.1f);
            TwoBoneIK.Solve(sh, el, hand, target, el.position + bend.normalized * 0.3f, 1f);
            rig.LockPose(left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight); rig.LockPose(left ? BodyPart.ForearmLeft : BodyPart.ForearmRight);
        }
    }

    // ── Piezas que se mueven a la medida de quien usa la máquina ────────────────────────────────

    static readonly Dictionary<Transform, Vector3> _orig = new Dictionary<Transform, Vector3>();   // posición local original de cada pieza movida
    static readonly Dictionary<GymStation, List<Transform>> _moved = new Dictionary<GymStation, List<Transform>>();
    static readonly Dictionary<GymStation, float> _travel = new Dictionary<GymStation, float>();

    /// <summary>Cuánto se desplazó (m) el agarre móvil de la estación la última vez (las palancas y cables tienen recorrido).</summary>
    public static float LastTravel(GymStation st) => st != null && _travel.TryGetValue(st, out float t) ? t : 0f;

    /// <summary>Piezas que van con las manos en la estación de tipo <paramref name="t"/> (null si sus agarres son fijos) y si
    /// van por lados (un agarre por mano) o juntas (una barra).</summary>
    static string[] Moving(GymMachineType t, out bool perSide)
    {
        perSide = false;
        switch (t)
        {
            case GymMachineType.PressPecho: case GymMachineType.PressHombros: case GymMachineType.Remo: perSide = true; return new[] { "Agarre" };
            case GymMachineType.SkiErg: perSide = true; return new[] { "Tirador" };
            case GymMachineType.Jalon: return new[] { "Barra_jalon" };
            case GymMachineType.BicepsTriceps: return new[] { "Barra_agarre" };
            case GymMachineType.RemoErgometro: return new[] { "Mango" };
            case GymMachineType.SentadillaJaula: case GymMachineType.SentadillaSmith: case GymMachineType.PressBancaPlano: case GymMachineType.PressBancaInclinado:
            case GymMachineType.PesoMuerto: case GymMachineType.Cargada:
                return new[] { "Barra_olimpica", "Manguito", "Disco" };
            default: return null;
        }
    }

    static Vector3 Orig(Transform t) => _orig.TryGetValue(t, out Vector3 o) && t.parent != null ? t.parent.TransformPoint(o) : t.position;

    static void Shift(GymStation st, Transform piece, Vector3 worldOffset)
    {
        if (!_orig.ContainsKey(piece)) _orig[piece] = piece.localPosition;
        if (!_moved.TryGetValue(st, out List<Transform> l)) _moved[st] = l = new List<Transform>();
        if (!l.Contains(piece)) l.Add(piece);
        Vector3 w = Orig(piece) + worldOffset;
        piece.localPosition = piece.parent != null ? piece.parent.InverseTransformPoint(w) : w;
    }

    /// <summary>Devuelve a su sitio lo que se movió en la estación (al levantarse).</summary>
    public static void Reset(GymStation st)
    {
        if (st == null || !_moved.TryGetValue(st, out List<Transform> l)) return;
        foreach (Transform t in l) if (t != null && _orig.TryGetValue(t, out Vector3 p)) t.localPosition = p;
        l.Clear(); _travel.Remove(st);
    }

    /// <summary>Lo que va con las manos: palancas, cables y barras se desplazan (desde su sitio) hasta las manos.
    /// true = la estación tiene agarres móviles (las manos no necesitan IK).</summary>
    static bool MoveGrips(GymStation st, CreatureRig rig, Equipment eq)
    {
        string[] names = Moving(st.type, out bool perSide);
        if (names == null) return false;
        Transform hl = Segment(rig, BodyPart.HandLeft), hr = Segment(rig, BodyPart.HandRight);
        if (hl == null || hr == null) return true;
        var group = new List<Transform>();
        foreach (Transform t in eq.surface) if (t != null && names.Any(n => t.name.StartsWith(n))) group.Add(t);
        if (group.Count == 0) return true;
        float travel = 0f;
        if (!perSide)
        {
            Transform primary = group.FirstOrDefault(t => t.name.StartsWith(names[0])) ?? group[0];
            Vector3 hands = (hl.position + hr.position) * 0.5f;
            // El punto de la barra (en su sitio original) más cercano a las manos → a las manos.
            Vector3 back = Orig(primary) - primary.position;
            Vector3 c = Closest(hands - back, primary) + back;
            Vector3 off = hands - c;
            foreach (Transform t in group) Shift(st, t, off);
            travel = off.magnitude;
        }
        else
        {
            foreach (bool plus in new[] { false, true })
            {
                var side = group.Where(t => (st.transform.InverseTransformPoint(Orig(t)).x > 0f) == plus).ToList();
                if (side.Count == 0) continue;
                Transform primary = side.FirstOrDefault(t => Mathf.Abs(t.lossyScale.y) >= Mathf.Abs(t.lossyScale.x)) ?? side[0];
                Vector3 o = Orig(primary);
                Transform hand = Vector3.Distance(hl.position, o) <= Vector3.Distance(hr.position, o) ? hl : hr;
                Vector3 off = hand.position - o;
                foreach (Transform t in side) Shift(st, t, off);
                travel = Mathf.Max(travel, off.magnitude);
            }
        }
        _travel[st] = travel;
        return true;
    }

    /// <summary>La almohadilla de la palanca del hip thrust, apoyada sobre la pelvis (sube y baja con ella).</summary>
    static void RideOnHips(GymStation st, CreatureRig rig, Equipment eq)
    {
        Transform hips = Segment(rig, BodyPart.Hips); if (hips == null) return;
        Transform pad = eq.solid.FirstOrDefault(t => t != null && t.name == "Almohadilla_cadera");
        if (pad == null) return;
        float top = float.MinValue;
        for (int c = 0; c < 8; c++) top = Mathf.Max(top, hips.TransformPoint(new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? -0.5f : 0.5f, (c & 4) == 0 ? -0.5f : 0.5f)).y);
        float bottom0 = Orig(pad).y - Mathf.Abs(pad.lossyScale.y) * 0.5f;
        Shift(st, pad, Vector3.up * Mathf.Clamp(top - bottom0, -0.3f, 0.5f));
    }

    /// <summary>ASIENTO REGULABLE de las máquinas de placas: su parte de arriba, justo bajo la pelvis de quien se sienta.</summary>
    static void AdjustSeat(GymStation st, CreatureRig rig, Equipment eq)
    {
        Transform hips = Segment(rig, BodyPart.Hips); if (hips == null) return;
        Transform seat = eq.solid.FirstOrDefault(t => t != null && t.name == "Asiento");
        if (seat == null) return;
        float top0 = Orig(seat).y + Mathf.Abs(seat.lossyScale.y) * 0.5f;
        float dy = Mathf.Clamp(LowestY(hips) - top0, -0.25f, 0.1f);
        foreach (Transform t in eq.solid)
            if (t != null && (t == seat || t.name == "Columna_asiento" || t.name == "Soporte_asiento") && Mathf.Abs(Orig(t).x - Orig(seat).x) < 0.3f)
                Shift(st, t, Vector3.up * dy);
    }

    /// <summary>Las <paramref name="n"/> manos más cerca de un agarre (a ≤ <paramref name="reach"/> m) y el punto del agarre
    /// al que van (la muñeca: el cubo de la mano queda alrededor).</summary>
    static List<(Transform hand, Vector3 target)> HandTargets(CreatureRig rig, Equipment eq, int n, float reach)
    {
        var l = new List<(Transform, Vector3, float)>();
        foreach (BodyPart h in new[] { BodyPart.HandLeft, BodyPart.HandRight })
        {
            Transform hand = rig.Get(h); Transform seg = Segment(rig, h);
            if (hand == null || seg == null) continue;
            float best = float.MaxValue; Vector3 bp = Vector3.zero;
            foreach (Transform gr in eq.grip)
            {
                if (gr == null) continue;
                Vector3 p = Closest(seg.position, gr); float d = Vector3.Distance(p, seg.position);
                if (d < best) { best = d; bp = p; }
            }
            if (best <= reach) l.Add((hand, bp + (hand.position - seg.position), best));   // la muñeca, desplazada lo que va del centro de la mano a ella
        }
        return l.OrderBy(x => x.Item3).Take(Mathf.Max(0, n)).Select(x => (x.Item1, x.Item2)).ToList();
    }
}
