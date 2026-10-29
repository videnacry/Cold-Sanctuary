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
        /// <summary>Piezas que van con los pies (los pedales giran con la biela): un pie en ellas no está «hundido».</summary>
        public readonly HashSet<Transform> withFeet = new HashSet<Transform>();
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
        if (n.StartsWith("Peldano") && t != GymMachineType.MonkeyBars && t != GymMachineType.ElevacionPiernas) return false;   // los peldaños de los postes y de la escaladora se pisan, no se agarran
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
            if (Pedals(st.type) && t.name == "Pedal") e.withFeet.Add(t);
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
                    if (piece == null || eq.withFeet.Contains(piece) || Depth(q, piece) <= 0f) continue;
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
            case GymMachineType.RemoAustraliano:   // (con los talones en el suelo: los pies bajan por IK)
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
        // Ningún pie metido en nada: si lo está, el cuerpo sube lo que haga falta (hasta 4 veces: al subir, el pie puede dar
        // con otra pieza, el reposapiés justo encima del bastidor).
        void Unsink()
        {
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
        // ANTES DE MEDIR LOS PIES, lo que se adapta a la pierna de esta postura (si no, un pie metido en la pieza del
        // fotograma anterior levantaba el cuerpo entero):
        //  · bicis: quien es alto llega con el pie estirado al volante; la rodilla se dobla más hasta que el pie sale.
        //  · banco declinado: los rodillos, a los tobillos.
        //  · hiperextensiones: la almohadilla, en su sitio (luego se regula a la medida: HipPad).
        Transform rail = st.type == GymMachineType.Barandilla ? eq.grip.FirstOrDefault(t => t != null && t.name.StartsWith("Tubo")) : null;
        if (Pedals(st.type)) RetractFeet(rig, eq, g.transform);
        if (st.type == GymMachineType.AbdominalDeclinado) FeetRollers(st, rig, eq);
        if (st.type == GymMachineType.Hiperextension) { Transform pad = eq.solid.FirstOrDefault(t => t != null && t.name == "Almohadilla_cadera"); if (pad != null) Shift(st, pad, Vector3.zero); }
        {
            float minGap = float.MaxValue;
            foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
            {
                Transform seg = Segment(rig, f); if (seg == null) continue;
                float gap = FootGap(seg, eq, g.transform, out _);
                if (!float.IsInfinity(gap)) minGap = Mathf.Min(minGap, gap);
            }
            if (legacy) { if (minGap < 0f) dy = -minGap; }   // placas (postura apoyada): solo sacar un pie hundido (en el reposapiés, el bastidor…)
            else if (rail != null)   // la barandilla: el pie más bajo, sobre el TUBO (5 cm: si el pie no cae justo encima, el suelo engañaba)
            {
                float low = float.MaxValue;
                foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight }) { Transform seg = Segment(rig, f); if (seg != null) low = Mathf.Min(low, LowestY(seg)); }
                if (low < float.MaxValue) dy = rail.position.y + Mathf.Abs(rail.lossyScale.y) * 0.5f - low;
            }
            else if (Hangs(st.type, b) && hands > 0)   // cuelga (o se sostiene) de las manos: a la altura del agarre; los pies, por IK
            {
                float sum = 0f; int n = 0;
                foreach (var (hand, target) in HandTargets(rig, eq, hands, 0.9f)) { sum += target.y - hand.position.y; n++; }
                if (n > 0) dy = sum / n;
                if (minGap < float.MaxValue && minGap + dy < 0f) dy = -minGap;   // ningún pie dentro del suelo
            }
            else if (feet > 0 && minGap < float.MaxValue)   // el pie más bajo, sobre lo que pisa
            {
                dy = -minGap;
                if (st.type == GymMachineType.Escaladora) dy = StepChoice(rig, eq, g.transform, dy);   // (o el de arriba, en su peldaño)
            }
            else if (minGap < float.MaxValue && minGap < 0f) dy = -minGap;   // ningún pie dentro de nada
            dy = Mathf.Clamp(dy, -0.8f, 0.8f);
            if (Debug_) Debug.Log($"[GymFitDbg] f{Time.frameCount} {st.name} {g.name} b={b} feet={feet} minGap={minGap:0.000} dy={dy:0.000} legacy={legacy} planta={LowestY(Segment(rig, BodyPart.FootLeft)) - st.SeatPoint(b ? 1f : 0f).y:0.000}");
            if (Mathf.Abs(dy) > 1e-4f) g.transform.position += Vector3.up * dy;
            // CURL FEMORAL (tumbado boca abajo): la postura apoya en el puesto su punto más bajo, que son las MANOS colgando,
            // y el cuerpo flotaba sobre el banco con las manos a la altura del tablero (a 0,2 m de los asideros, que están
            // más abajo). El tronco y las piernas se apoyan en el banco; las manos bajan luego a los asideros por IK.
            if (st.type == GymMachineType.CurlFemoral)
            {
                float low = float.MaxValue;
                foreach (BodyPart p in Trunk) { Transform seg = Segment(rig, p); if (seg != null) low = Mathf.Min(low, LowestY(seg)); }
                float down = low - st.SeatPoint(b ? 1f : 0f).y;
                if (low < float.MaxValue && down > 0.005f) { g.transform.position -= Vector3.up * down; dy -= down; }
            }
            Unsink();
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
        // Solo si el IK lo deja mejor: con la pierna casi recta (remo australiano) un pie podía acabar dentro del suelo.
        if (feet >= 2 && rail == null)
            foreach (bool left in new[] { true, false })
            {
                Transform foot = rig.Get(left ? BodyPart.FootLeft : BodyPart.FootRight), seg = Segment(rig, left ? BodyPart.FootLeft : BodyPart.FootRight);
                Transform th = rig.Get(left ? BodyPart.ThighLeft : BodyPart.ThighRight), sh = rig.Get(left ? BodyPart.ShinLeft : BodyPart.ShinRight);
                if (foot == null || seg == null || th == null || sh == null) continue;
                float gap = FootGap(seg, eq, g.transform, out _);
                if (gap <= 0.01f || gap > 0.3f) continue;
                // Si justo debajo no hay dónde apoyar (los talones de quien es alto pasan del borde del caucho y debajo está
                // la losa, 13 cm más abajo: bajarlos los metía en el canto del caucho), la rodilla se dobla: el pie se acerca
                // a la cadera (de 3 en 3 cm, hasta 0,45 m) hasta que baja sobre algo sin hundirse.
                Quaternion qt = th.localRotation, qs = sh.localRotation;
                Vector3 f0 = foot.position, toHip = th.position - foot.position; toHip.y = 0f;
                toHip = toHip.sqrMagnitude > 1e-6f ? toHip.normalized : Vector3.zero;
                bool ok = false; float bestAfter = float.MaxValue; int bestK = -1;
                for (int k = 0; k <= 15 && !ok; k++)
                {
                    th.localRotation = qt; sh.localRotation = qs;
                    float down = gap;
                    if (k > 0)
                    {
                        if (toHip == Vector3.zero) break;
                        TwoBoneIK.Solve(th, sh, foot, f0 + toHip * (0.03f * k), sh.position + g.transform.forward * 0.3f, 1f);
                        down = FootGap(seg, eq, g.transform, out _);
                        if (float.IsInfinity(down) || down < -0.06f || down > 0.3f) continue;   // (sobre el canto del caucho: sube lo que se hunde)
                    }
                    if (Mathf.Abs(down) > 0.005f) TwoBoneIK.Solve(th, sh, foot, foot.position - Vector3.up * down, sh.position + g.transform.forward * 0.3f, 1f);
                    float after = FootGap(seg, eq, g.transform, out _);
                    ok = after >= -0.005f && after <= (k == 0 ? 0.045f : 0.03f);   // (sin doblar la rodilla vale lo que ya valía: ≤ 4,5 cm)
                    if (!ok && after >= -0.005f && after < bestAfter) { bestAfter = after; bestK = k; }
                }
                if (!ok)
                {
                    th.localRotation = qt; sh.localRotation = qs;
                    if (bestK < 0 || bestAfter >= gap) continue;
                    // (lo mejor que se encontró, aunque no llegue del todo)
                    if (bestK > 0) TwoBoneIK.Solve(th, sh, foot, f0 + toHip * (0.03f * bestK), sh.position + g.transform.forward * 0.3f, 1f);
                    float d2 = FootGap(seg, eq, g.transform, out _);
                    if (!float.IsInfinity(d2) && d2 > 0.005f) TwoBoneIK.Solve(th, sh, foot, foot.position - Vector3.up * d2, sh.position + g.transform.forward * 0.3f, 1f);
                }
                rig.LockPose(left ? BodyPart.ThighLeft : BodyPart.ThighRight); rig.LockPose(left ? BodyPart.ShinLeft : BodyPart.ShinRight);
            }
        // LA BARANDILLA: los pies que apoyan, SOBRE el tubo de 5 cm (antes caían a su lado, a 0,5–1 m del suelo).
        if (rail != null) FeetOnRail(rig, eq, rail, feet, g.transform);
        // LA BANDERA HUMANA: la cabeza, que va hacia el poste, no se mete en él (el cuerpo se aparta; las manos siguen al agarre).
        if (st.type == GymMachineType.BanderaHumana) PushOff(g, rig, eq, "Poste");
        // Y al final, ningún pie dentro de nada (lo de arriba mueve piernas y cuerpo).
        Unsink();
        _lastDy[st] = dy;
        // LA ALMOHADILLA DEL HIP THRUST va encima de la cadera (es la palanca que se empuja con ella).
        if (st.type == GymMachineType.HipThrust) RideOnHips(st, rig, eq);
        // LO REGULABLE a la medida de quien la usa: la almohadilla de las rodillas del gemelo, la de la cadera de las
        // hiperextensiones y los rodillos de los pies del banco declinado. Y los PEDALES, bajo los pies (giran con la biela).
        if (st.type == GymMachineType.Gemelos) RideOn(st, rig, eq, "Almohadilla_rodillas", true, BodyPart.ThighLeft, BodyPart.ThighRight);
        if (st.type == GymMachineType.Hiperextension) HipPad(st, rig, eq);
        if (st.type == GymMachineType.AbdominalDeclinado) FeetRollers(st, rig, eq);
        if (Pedals(st.type)) PedalsUnderFeet(st, rig, eq);
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
            case GymMachineType.AssaultBike: perSide = true; return new[] { "Brazo" };   // los brazos del assault bike empujan y tiran
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

    /// <summary>¿Tiene pedales que giran con los pies? (bicis y assault bike)</summary>
    public static bool Pedals(GymMachineType t) => t == GymMachineType.BiciVertical || t == GymMachineType.BiciReclinada || t == GymMachineType.AssaultBike;

    static float HighestY(Transform cube)
    {
        float y = float.MinValue;
        for (int c = 0; c < 8; c++) y = Mathf.Max(y, cube.TransformPoint(new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? -0.5f : 0.5f, (c & 4) == 0 ? -0.5f : 0.5f)).y);
        return y;
    }

    /// <summary>Pieza REGULABLE en altura: <paramref name="above"/> = encima de esas partes del cuerpo (su base, sobre lo más
    /// alto de ellas: la almohadilla de las rodillas); si no, debajo (su parte de arriba, bajo lo más bajo: la de la cadera).</summary>
    static void RideOn(GymStation st, CreatureRig rig, Equipment eq, string piece, bool above, params BodyPart[] parts)
    {
        Transform pad = eq.solid.FirstOrDefault(t => t != null && t.name == piece);
        if (pad == null) return;
        float y = above ? float.MinValue : float.MaxValue;
        foreach (BodyPart p in parts)
        {
            Transform seg = Segment(rig, p); if (seg == null) continue;
            y = above ? Mathf.Max(y, HighestY(seg)) : Mathf.Min(y, LowestY(seg));
        }
        if (y == float.MinValue || y == float.MaxValue) return;
        Vector3 back = Orig(pad) - pad.position;   // dónde estaría en su sitio
        float lo = LowestY(pad) + back.y, hi = HighestY(pad) + back.y;
        Shift(st, pad, Vector3.up * Mathf.Clamp(above ? y - lo : y - hi, -0.2f, 0.2f));
    }

    /// <summary>Los rodillos de los pies del banco declinado (con su brazo), a la altura y la distancia de los tobillos de quien
    /// se tumba. Centrados en los pies, el pie (inclinado) se metía en uno de los dos rodillos: se prueba alrededor
    /// (±0,3 m arriba/abajo, ±0,2 m a lo largo del banco) y se queda la posición más cercana en la que ningún pie entra en un
    /// rodillo y las piernas no lo atraviesan más de lo que cede el acolchado. Los PIES mandan: si no hay posición limpia,
    /// la que menos hunde el pie (lo que mide la prueba: la parte de arriba del rodillo menos la planta) y, a igualdad, la
    /// que menos se mete en las piernas.</summary>
    static void FeetRollers(GymStation st, CreatureRig rig, Equipment eq)
    {
        Transform fl = Segment(rig, BodyPart.FootLeft), fr = Segment(rig, BodyPart.FootRight);
        if (fl == null || fr == null) return;
        var rollers = eq.solid.Where(t => t != null && t.name == "Rodillo_pies").ToList();
        if (rollers.Count == 0) return;
        Vector3 mid = Vector3.zero; foreach (Transform r in rollers) mid += Orig(r); mid /= rollers.Count;
        Vector3 off = (fl.position + fr.position) * 0.5f - mid; off.x = 0f;
        off = Vector3.ClampMagnitude(off, 0.3f);
        var moved = eq.solid.Where(t => t != null && (t.name == "Rodillo_pies" || t.name == "Brazo_rodillos")).ToList();
        Vector3 fwd = st.Facing * Vector3.forward; fwd.y = 0f; fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
        var cands = new List<Vector3>();
        for (int i = -15; i <= 15; i++) for (int j = -5; j <= 5; j++) cands.Add(Vector3.up * (i * 0.02f) + fwd * (j * 0.04f));
        cands.Sort((x, y) => x.sqrMagnitude.CompareTo(y.sqrMagnitude));
        Vector3 best = off; float bestFeet = float.MaxValue, bestBody = float.MaxValue;
        foreach (Vector3 c in cands)
        {
            foreach (Transform t in moved) Shift(st, t, off + c);
            float feetSunk = FeetSunk(rig, moved);
            if (feetSunk > bestFeet + 1e-4f) continue;
            float body = BodyDepth(rig, moved);
            if (feetSunk < bestFeet - 1e-4f || body < bestBody) { bestFeet = feetSunk; bestBody = body; best = off + c; }
            if (feetSunk <= 0f && body <= 0.006f) break;
        }
        foreach (Transform t in moved) Shift(st, t, best);
    }

    // ── Medidas del cuerpo contra la máquina (las mismas reglas que GymUsabilityTest) ───────────────────────────────

    /// <summary>Segmentos del cuerpo que no deben atravesar la máquina (las manos no: agarran).</summary>
    static readonly BodyPart[] Measured =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head, BodyPart.ShoulderLeft, BodyPart.ShoulderRight,
        BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight,
        BodyPart.FootLeft, BodyPart.FootRight,
    };
    /// <summary>El tronco y las piernas (sin los brazos): lo que se apoya en un banco.</summary>
    static readonly BodyPart[] Trunk =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head, BodyPart.ThighLeft, BodyPart.ThighRight,
        BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };
    static readonly string[] SoftNames = { "Almohadilla", "Respaldo", "Asiento", "Banco", "Rodillo", "Sillin", "Apoy", "Esterilla", "Saco" };
    /// <summary>¿Es un acolchado? (cede 2 cm).</summary>
    public static bool Soft(string n) { foreach (string s in SoftNames) if (n.StartsWith(s)) return true; return false; }

    /// <summary>Lo que más se mete el cuerpo (puntos del eje de cada segmento, sin las manos) en esas piezas, en m; los
    /// acolchados ceden 2 cm. ≤ 0 = nada.</summary>
    static float BodyDepth(CreatureRig rig, IEnumerable<Transform> pieces, IEnumerable<BodyPart> parts = null)
    {
        float worst = 0f;
        foreach (BodyPart p in parts ?? Measured)
        {
            Transform seg = Segment(rig, p); if (seg == null) continue;
            foreach (Vector3 pt in AxisSamples(seg))
                foreach (Transform piece in pieces)
                {
                    if (piece == null) continue;
                    float d = Depth(pt, piece) - (Soft(piece.name) ? 0.02f : 0f);
                    if (d > worst) worst = d;
                }
        }
        return worst;
    }

    /// <summary>Lo que un pie queda «hundido» en esas piezas, como lo mide <see cref="FootGap"/>: si algún punto del pie
    /// (talón, centro, punta o planta) está dentro de una, la parte de arriba de esa pieza menos la planta. 0 = ninguno.</summary>
    static float FeetSunk(CreatureRig rig, List<Transform> pieces)
    {
        float s = 0f;
        foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
        {
            Transform seg = Segment(rig, f); if (seg == null) continue;
            float bottom = LowestY(seg);
            foreach (Vector3 a in AxisSamples(seg))
                foreach (Vector3 q in new[] { a, new Vector3(a.x, bottom + 0.01f, a.z) })
                    foreach (Transform piece in pieces)
                        if (piece != null && Depth(q, piece) > 0f) s = Mathf.Max(s, HighestY(piece) - bottom + 0.001f);
        }
        return s;
    }

    static float MinFootGap(CreatureRig rig, Equipment eq, Transform self)
    {
        float m = float.MaxValue;
        foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
        {
            Transform seg = Segment(rig, f); if (seg == null) continue;
            float gap = FootGap(seg, eq, self, out _);
            if (!float.IsInfinity(gap)) m = Mathf.Min(m, gap);
        }
        return m;
    }

    // ── Ajustes por máquina ────────────────────────────────────────────────────────────────────

    /// <summary>BICIS: con la pierna estirada, quien es alto metía la punta del pie en el volante (y el ajuste lo subía 0,3 m
    /// del asiento: las manos ya no llegaban a las asas). La rodilla se dobla más —el pie se acerca a la cadera, de 3 en 3 cm,
    /// hasta 0,3 m— hasta que ni el pie ni la pierna entran en nada (los pedales luego van bajo el pie).</summary>
    static void RetractFeet(CreatureRig rig, Equipment eq, Transform self)
    {
        var solids = eq.solid.Where(t => t != null && !eq.withFeet.Contains(t)).ToList();
        foreach (bool left in new[] { true, false })
        {
            BodyPart pt = left ? BodyPart.ThighLeft : BodyPart.ThighRight, ps = left ? BodyPart.ShinLeft : BodyPart.ShinRight, pf = left ? BodyPart.FootLeft : BodyPart.FootRight;
            Transform th = rig.Get(pt), sh = rig.Get(ps), foot = rig.Get(pf), seg = Segment(rig, pf);
            if (th == null || sh == null || foot == null || seg == null) continue;
            BodyPart[] leg = { pt, ps, pf };
            bool Bad() => FootGap(seg, eq, self, out _) < -0.01f || BodyDepth(rig, solids, leg) > 0.004f;
            if (!Bad()) continue;
            Quaternion qt = th.localRotation, qs = sh.localRotation;
            Vector3 f0 = foot.position, toHip = (th.position - foot.position).normalized;
            bool ok = false;
            for (int k = 1; k <= 10 && !ok; k++)
            {
                th.localRotation = qt; sh.localRotation = qs;
                TwoBoneIK.Solve(th, sh, foot, f0 + toHip * (0.03f * k), sh.position + self.forward * 0.3f, 1f);
                ok = !Bad();
            }
            if (!ok) { th.localRotation = qt; sh.localRotation = qs; continue; }
            rig.LockPose(pt); rig.LockPose(ps);
        }
    }

    /// <summary>ESCALADORA: el pie más bajo en su peldaño (<paramref name="dyLow"/>) o, si así una pierna atraviesa el peldaño de
    /// encima (quien es bajo deja el pie de delante entre dos peldaños y la espinilla dentro del siguiente), el pie de ARRIBA
    /// sobre el peldaño que tiene debajo (subiendo: el de atrás se despega). Se queda la que no atraviesa nada.</summary>
    static float StepChoice(CreatureRig rig, Equipment eq, Transform self, float dyLow)
    {
        float Score(float d)
        {
            self.position += Vector3.up * d;
            float gap = MinFootGap(rig, eq, self);
            float s = Mathf.Max(BodyDepth(rig, eq.solid) - 0.004f, gap < float.MaxValue ? -gap - 0.01f : 0f);
            self.position -= Vector3.up * d;
            return s;
        }
        float s0 = Score(dyLow);
        if (s0 <= 0f) return dyLow;
        Transform hi = null; float hiY = float.MinValue;
        foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
        {
            Transform seg = Segment(rig, f); if (seg == null) continue;
            float y = LowestY(seg); if (y > hiY) { hiY = y; hi = seg; }
        }
        if (hi == null) return dyLow;
        const float rise = 0.45f;
        float top = float.NegativeInfinity;
        foreach (Vector3 a in AxisSamples(hi))
        {
            float t = SurfaceBelow(new Vector3(a.x, hiY + rise, a.z), eq, self, out _);
            if (t < hiY + rise - 1e-3f) top = Mathf.Max(top, t);
        }
        if (float.IsNegativeInfinity(top)) return dyLow;
        float dyHigh = top + 0.003f - hiY;
        return Score(dyHigh) < s0 ? dyHigh : dyLow;
    }

    /// <summary>BARANDILLA: los pies que apoyan (el más bajo en A, los dos en B) van por IK SOBRE el tubo: el centro de la
    /// planta en la vertical del eje del tubo y la planta en su parte de arriba (+4 mm).</summary>
    static void FeetOnRail(CreatureRig rig, Equipment eq, Transform rail, int n, Transform self)
    {
        float top = rail.position.y + Mathf.Abs(rail.lossyScale.y) * 0.5f;
        Vector3 axis = rail.TransformDirection(Vector3.forward); axis.y = 0f;
        axis = axis.sqrMagnitude > 1e-6f ? axis.normalized : Vector3.forward;
        var feetList = new List<(BodyPart th, BodyPart sh, BodyPart f, float y)>();
        foreach (bool left in new[] { true, false })
        {
            Transform seg = Segment(rig, left ? BodyPart.FootLeft : BodyPart.FootRight); if (seg == null) continue;
            feetList.Add((left ? BodyPart.ThighLeft : BodyPart.ThighRight, left ? BodyPart.ShinLeft : BodyPart.ShinRight, left ? BodyPart.FootLeft : BodyPart.FootRight, LowestY(seg)));
        }
        foreach (var (pt, ps, pf, _) in feetList.OrderBy(x => x.y).Take(Mathf.Max(1, n)))
        {
            Transform th = rig.Get(pt), sh = rig.Get(ps), foot = rig.Get(pf), seg = Segment(rig, pf);
            if (th == null || sh == null || foot == null || seg == null) continue;
            for (int it = 0; it < 3; it++)
            {
                Vector3 c = seg.position;
                Vector3 on = rail.position + axis * Vector3.Dot(c - rail.position, axis);
                Vector3 d = new Vector3(on.x - c.x, top + 0.004f - LowestY(seg), on.z - c.z);
                if (d.magnitude < 0.004f) break;
                TwoBoneIK.Solve(th, sh, foot, foot.position + d, sh.position + self.forward * 0.3f, 1f);
            }
            rig.LockPose(pt); rig.LockPose(ps);
        }
    }

    /// <summary>Aparta el cuerpo (en horizontal) de las piezas macizas cuyo nombre empieza por <paramref name="prefix"/> en
    /// las que se mete (la cabeza en el poste de la bandera). Empujar desde el eje del poste no bastaba: la cabeza va de
    /// frente al poste y, al retirarla, el siguiente punto de su eje entraba. Se prueban desplazamientos de 1 en 1 cm (hasta
    /// 0,2 m) hacia delante, hacia atrás, alejándose del poste y en diagonal, y se queda el más corto que deja todo el cuerpo
    /// fuera (+5 mm de margen).</summary>
    static void PushOff(GameObject g, CreatureRig rig, Equipment eq, string prefix)
    {
        var posts = eq.solid.Where(t => t != null && t.name.StartsWith(prefix)).ToList();
        if (posts.Count == 0 || BodyDepth(rig, posts) <= 0f) return;
        Vector3 fwd = g.transform.forward; fwd.y = 0f; fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
        Transform hips = rig.Get(BodyPart.Hips);
        Vector3 away = (hips != null ? hips.position : g.transform.position) - posts[0].position; away.y = 0f;
        away = away.sqrMagnitude > 1e-6f ? away.normalized : -fwd;
        Vector3[] dirs = { fwd, -fwd, away, (fwd + away).normalized, (-fwd + away).normalized };
        Vector3 start = g.transform.position, best = start; float bestD = BodyDepth(rig, posts);
        for (int k = 1; k <= 20; k++)
        {
            foreach (Vector3 d in dirs)
            {
                g.transform.position = start + d * (0.01f * k);
                float depth = BodyDepth(rig, posts);
                if (depth < bestD) { bestD = depth; best = g.transform.position; }
                if (depth <= -0.005f || (depth <= 0f && AllClear(rig, posts, 0.005f))) { g.transform.position = start + d * (0.01f * k); return; }
            }
        }
        g.transform.position = best;
    }

    /// <summary>¿Ningún punto del eje de los segmentos (sin las manos) a menos de <paramref name="margin"/> m de esas piezas?</summary>
    static bool AllClear(CreatureRig rig, List<Transform> pieces, float margin)
    {
        foreach (BodyPart p in Measured)
        {
            Transform seg = Segment(rig, p); if (seg == null) continue;
            foreach (Vector3 pt in AxisSamples(seg))
                foreach (Transform piece in pieces)
                    if (piece != null && Dist(pt, piece) < margin) return false;
        }
        return true;
    }

    /// <summary>HIPEREXTENSIONES: la almohadilla de la cadera es regulable. Su borde de arriba queda ~0,3 m por delante de la
    /// cadera, así que subirla hasta la pelvis la metía en el hombro o el brazo del tronco que cuelga. Se prueban alturas
    /// (±0,2 m) y avances (±0,1 m) y se queda la más cercana a su sitio en la que el cuerpo no la atraviesa (cede 2 cm) y
    /// la cadera o los muslos la tocan.</summary>
    static void HipPad(GymStation st, CreatureRig rig, Equipment eq)
    {
        Transform pad = eq.solid.FirstOrDefault(t => t != null && t.name == "Almohadilla_cadera");
        if (pad == null) return;
        var one = new List<Transform> { pad };
        Vector3 fwd = st.Facing * Vector3.forward; fwd.y = 0f; fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
        var cands = new List<Vector3>();
        for (int i = -10; i <= 10; i++) for (int j = -2; j <= 2; j++) cands.Add(Vector3.up * (i * 0.02f) + fwd * (j * 0.05f));
        cands.Sort((x, y) => x.sqrMagnitude.CompareTo(y.sqrMagnitude));
        Vector3 best = Vector3.zero; float bestPen = float.MaxValue, bestGap = float.MaxValue;
        foreach (Vector3 c in cands)
        {
            Shift(st, pad, c);
            float pen = BodyDepth(rig, one);
            bool clean = pen <= 0.004f;
            float gap = float.MaxValue;
            if (clean)
                foreach (BodyPart p in new[] { BodyPart.Hips, BodyPart.ThighLeft, BodyPart.ThighRight })
                {
                    Transform seg = Segment(rig, p); if (seg == null) continue;
                    foreach (Vector3 pt in AxisSamples(seg)) gap = Mathf.Min(gap, Mathf.Max(0f, Dist(pt, pad) - HalfThickness(seg)));
                }
            bool bestClean = bestPen <= 0.004f;
            if (clean ? (!bestClean || gap < bestGap - 0.005f) : (!bestClean && pen < bestPen)) { best = c; bestPen = pen; bestGap = gap; }
        }
        Shift(st, pad, best);
    }

    /// <summary>Los pedales, bajo la planta de cada pie (giran con la biela).</summary>
    static void PedalsUnderFeet(GymStation st, CreatureRig rig, Equipment eq)
    {
        foreach (Transform p in eq.withFeet)
        {
            if (p == null) continue;
            Transform fl = Segment(rig, BodyPart.FootLeft), fr = Segment(rig, BodyPart.FootRight);
            if (fl == null || fr == null) continue;
            Vector3 o = Orig(p);
            Transform foot = Vector3.Distance(fl.position, o) <= Vector3.Distance(fr.position, o) ? fl : fr;
            Vector3 sole = new Vector3(foot.position.x, LowestY(foot), foot.position.z);
            Vector3 want = sole - Vector3.up * (Mathf.Abs(p.lossyScale.y) * 0.5f);
            Vector3 off = want - o;
            if (off.magnitude <= 0.6f) Shift(st, p, off);
        }
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
