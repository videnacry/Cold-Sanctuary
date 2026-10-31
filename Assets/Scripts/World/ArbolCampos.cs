using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL ÁRBOL DE LOS CAMPOS en juego (docs/edificio-arbol.md, v2): sus niveles y la RUTA por el tronco. Entre pisos no hay
/// escaleras ni alfombras (solo en el techo y en el suelo): se va a la puerta del piso, se entra al agua (el campo magnético
/// la sujeta), se nada por el tronco [y por la rama y la columna de la punta] hasta la puerta del nivel de destino y se sale
/// andando. Nada de teletransportes: el cuerpo nada cada metro (<see cref="SwimBody.SwimTo"/>). <see cref="PathMover"/> lo usa
/// con <see cref="Route"/>. Hay DOS ENTRADAS: la puerta del suelo y la trampilla del techo (con las alfombras); desde fuera se
/// llega en alfombra al techo (los campos están a 64–84 m: es la más cercana) y se baja nadando.
/// </summary>
public class ArbolCampos : MonoBehaviour
{
    /// <summary>TODOS los edificios en hélice de la escena (el Árbol de los Campos y las torres migradas al modelo del árbol:
    /// Servicios, Entrenamiento, Ciencias, Producción; docs/edificio-arbol.md §Migración).</summary>
    public static readonly List<ArbolCampos> All = new List<ArbolCampos>();
    /// <summary>El Árbol de los Campos (el que gira y tiene los cultivos); si no está, el primero que haya.</summary>
    public static ArbolCampos Instance { get { ArbolCampos f = null; foreach (ArbolCampos t in All) { if (t == null) continue; if (t.building == TreeSpec.Building) return t; if (f == null) f = t; } return f; } }
    [Tooltip("Nombre del edificio (red de sus alfombras; «Árbol de los Campos», «Servicios»…).")]
    public string building = TreeSpec.Building;
    [Tooltip("Medio lado del agua del tronco (m).")]
    public float waterHalf = 10f;
    [Tooltip("Alto del tronco (la cara del techo, m).")]
    public float rootHeight = 92f;
    [Tooltip("Alcance en planta (lo más lejano del eje, m): hasta dónde es «el suelo» del árbol.")]
    public float reach = 56f;
    public List<TreeLevel> levels = new List<TreeLevel>();
    public Transform water;          // el agua del tronco (renderer)
    public Transform wall;           // la pared transparente (malla con los huecos de las puertas)
    public Transform top;            // el techo (plataforma de alfombras con la trampilla)
    [Tooltip("Velocidad de nado por el tronco (m/s): más que nadar en el mar, para no quedarse sin aire.")]
    public float routeSwimSpeed = 2.5f;
    [Tooltip("La CORRIENTE del tronco (m/s): el agua sube por un lado y baja por el otro, y lleva a quien nada a lo largo de él (el tramo vertical va a nado + corriente = 3,9 m/s, por debajo de los 4 m/s que las pruebas toman por un salto).")]
    public float routeCurrent = 1.4f;

    readonly List<WaterZone> _waters = new List<WaterZone>();

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public TreeLevel Ground => levels.Find(l => l.kind == TreeLevel.Kind.Ground);
    public TreeLevel Top => levels.Find(l => l.kind == TreeLevel.Kind.Top);
    public TreeLevel Floor(int k) => levels.Find(l => l.kind == TreeLevel.Kind.Floor && l.index == k);
    public TreeLevel Ring(int k) => levels.Find(l => l.kind == TreeLevel.Kind.Ring && l.index == k);
    public Vector3 World(Vector3 local) => transform.TransformPoint(local);

    /// <summary>¿El punto está dentro del agua del árbol (el tronco, una rama o la columna de una punta)?</summary>
    public bool InRootWater(Vector3 world)
    {
        if (_waters.Count == 0)
        {
            GetComponentsInChildren(true, _waters);
            _waters.RemoveAll(z => z.GetComponentInParent<TreeFloor>(true) != null);   // (el agua de un tanque de virtualización de un piso no es la del árbol)
        }
        foreach (WaterZone z in _waters) if (z != null && z.isActiveAndEnabled && z.Contains(world)) return true;
        return false;
    }

    /// <summary>El nivel en que está <paramref name="world"/> (los pies o el pivote), o null si no es del árbol (o está en su agua).</summary>
    public TreeLevel LevelAt(Vector3 world)
    {
        if (InRootWater(world)) return null;
        Vector3 l = transform.InverseTransformPoint(world);
        foreach (TreeLevel lv in levels)
        {
            if (lv.deck == null) continue;
            Vector3 d = lv.deck.InverseTransformPoint(world);
            if (d.y < -0.6f || d.y > 3f) continue;
            if (Mathf.Abs(d.x) <= lv.half + 0.3f && Mathf.Abs(d.z) <= lv.half + 0.3f) return lv;
        }
        float m = Mathf.Max(Mathf.Abs(l.x), Mathf.Abs(l.z));
        if (l.y < 3f && l.y > -2f && m > waterHalf + 1f && new Vector2(l.x, l.z).magnitude < Mathf.Max(30f, reach + 10f)) return Ground;
        return null;
    }

    /// <summary>Nivel por nombre corto: «0»/«suelo», «1»…«8», «anillo3», «copa»/«techo».</summary>
    public TreeLevel Find(string what)
    {
        what = (what ?? "").Trim().ToLowerInvariant().Replace("piso", "").Replace(" ", "");
        if (what == "0" || what == "suelo" || what == "ground") return Ground;
        if (what == "copa" || what == "top" || what == "techo") return Top;
        if (what.StartsWith("anillo") && int.TryParse(what.Substring(6), out int ri)) return Ring(ri);
        if (int.TryParse(what, out int fi)) return Floor(fi);
        return null;
    }

    /// <summary>El camino a nado (local del árbol, a ras de los pies) de la puerta de <paramref name="a"/> a la de
    /// <paramref name="b"/>: puerta → agua → [rama] → por el tronco (a 6 m del eje) → [rama y columna] → puerta → fuera.</summary>
    public List<Vector3> SwimPath(TreeLevel a, TreeLevel b, Vector3? fromLocal = null)
    {
        var p = new List<Vector3>();
        if (fromLocal.HasValue)
        {
            p.Add(fromLocal.Value);
            // Ya en el agua de una RAMA o de la COLUMNA de una punta (fuera del tronco): primero, por su camino de vuelta (o, si
            // va a esa misma punta o a su anillo, hacia delante por él).
            Vector3 f = fromLocal.Value;
            if (Mathf.Max(Mathf.Abs(f.x), Mathf.Abs(f.z)) >= waterHalf)
            {
                TreeLevel own = null; int seg = 0; float best = float.MaxValue;
                foreach (TreeLevel lv in levels)
                {
                    if (lv.via.Length == 0) continue;
                    for (int i = 0; i < lv.via.Length; i++)
                    {
                        Vector3 q0 = lv.via[i], q1 = i + 1 < lv.via.Length ? lv.via[i + 1] : lv.inside;
                        float dd = SegDist(f, q0, q1);
                        if (dd < best - 0.01f) { best = dd; own = lv; seg = i; }
                    }
                }
                if (own != null)
                {
                    bool sameBranch = b.via.Length > 0 && b.side == own.side && b.level == own.level && (b.kind == TreeLevel.Kind.Floor || b.kind == TreeLevel.Kind.Ring);
                    if (sameBranch)
                    {
                        for (int i = seg + 1; i < b.via.Length; i++) p.Add(b.via[i]);
                        p.Add(b.inside); p.Add(b.door); p.Add(b.outside);
                        return p;
                    }
                    for (int i = seg; i >= 0; i--) p.Add(own.via[i]);
                }
            }
        }
        else if (a.via.Length > 0 && b.via.Length > 0 && a.side == b.side && a.level == b.level)
        {
            // Del campo de punta a su anillo (o al revés): por la misma columna, sin bajar a la rama.
            p.Add(a.outside); p.Add(a.door); p.Add(a.inside); p.Add(b.inside); p.Add(b.door); p.Add(b.outside);
            return p;
        }
        else { p.Add(a.outside); p.Add(a.door); p.Add(a.inside); for (int i = a.via.Length - 1; i >= 0; i--) p.Add(a.via[i]); }
        Vector3 last = p[p.Count - 1];
        Vector3 bFirst = b.via.Length > 0 ? b.via[0] : b.inside;
        // Dentro del tronco (no desde una rama o una columna): por la columna central, a 6 m del eje.
        if (Mathf.Max(Mathf.Abs(last.x), Mathf.Abs(last.z)) < waterHalf)
        {
            float aAng = TreeSpec.Angle(new Vector3(last.x, 0f, last.z)), bAng = TreeSpec.Angle(new Vector3(bFirst.x, 0f, bFirst.z));
            p.Add(TreeSpec.Dir(aAng) * 6f + Vector3.up * last.y);
            p.Add(TreeSpec.Dir(bAng) * 6f + Vector3.up * bFirst.y);
        }
        p.AddRange(b.via); p.Add(b.inside); p.Add(b.door); p.Add(b.outside);
        return p;
    }

    static float SegDist(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a; float t = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
        return Vector3.Distance(p, a + ab * t);
    }

    public enum Hop { None, Walk, Swimming }

    /// <summary>Por dónde se sale del árbol hacia <paramref name="to"/> (fuera de él): por el suelo si el destino está en el
    /// hielo cerca del pie; si no, por el techo (sus alfombras).</summary>
    public TreeLevel ExitFor(Vector3 to)
    {
        Vector3 l = transform.InverseTransformPoint(to);
        return l.y < 6f && new Vector2(l.x, l.z).magnitude < 150f ? Ground : Top;
    }

    /// <summary>El árbol (y su nivel) en que está <paramref name="p"/>: un piso, un anillo, el techo o su agua mandan; si solo es
    /// «el suelo» (el hielo al pie), el del árbol más cercano.</summary>
    public static ArbolCampos At(Vector3 p, out TreeLevel level, out bool wet, bool water = true)
    {
        level = null; wet = false;
        ArbolCampos ground = null; float gd = float.MaxValue; TreeLevel gl = null;
        foreach (ArbolCampos t in All)
        {
            if (t == null) continue;
            TreeLevel lv = t.LevelAt(p);
            if (lv != null && lv.kind != TreeLevel.Kind.Ground) { level = lv; return t; }
            if (lv == null && water && t.InRootWater(p)) { wet = true; return t; }
            if (lv != null) { Vector3 d = p - t.transform.position; d.y = 0f; if (d.sqrMagnitude < gd) { gd = d.sqrMagnitude; ground = t; gl = lv; } }
        }
        level = gl;
        return ground;
    }

    /// <summary>
    /// La parte del ÁRBOL de una ruta (la llama <see cref="PathMover.StepToward"/> antes que las alfombras):
    /// - <see cref="Hop.Swimming"/>: va nadando por el tronco (no hay que moverlo);
    /// - <see cref="Hop.Walk"/>: anda por su piso hacia <paramref name="to"/> (quizá cambiado por la puerta), sin alfombras;
    /// - <see cref="Hop.None"/>: nada que ver con el árbol (o se llega en alfombra: al suelo o al techo).
    /// </summary>
    public static Hop Route(GameObject g, ref Vector3 to)
    {
        if (g == null || All.Count == 0) return Hop.None;
        if (TreeTrip.Busy(g)) return Hop.Swimming;
        if (FlyingCarpet.Riding(g)) return Hop.None;   // volando: las alfombras mandan
        ArbolCampos ta = At(g.transform.position, out TreeLevel a, out bool wet), tb = At(to, out TreeLevel b, out _, false);
        if (ta == null && tb == null) return Hop.None;   // nada que ver con ningún árbol
        const TreeLevel.Kind G = TreeLevel.Kind.Ground, T = TreeLevel.Kind.Top;
        // DE UN ÁRBOL A OTRO (Berón 2026-10-31: todos los edificios son árboles): primero se sale de este (nadando al techo) y,
        // desde su techo o su suelo, las alfombras llevan a la entrada del otro (su techo, si el destino es un piso).
        if (ta != null && tb != null && ta != tb)
        {
            if (!wet && a != null && (a.kind == G || a.kind == T))
            {
                if (b.kind != G && b.kind != T) to = tb.World(tb.Top.outside);
                return Hop.None;
            }
            b = null;
        }
        ArbolCampos t = ta != null ? ta : tb;
        if (ta == null) a = null;
        if (tb != t) b = null;
        if (a != null && a == b) return a.kind == G || a.kind == T ? Hop.None : Hop.Walk;   // mismo nivel: andando
        // Sale del árbol: desde el suelo o el techo, a pie o en alfombra; desde un piso, un anillo o el agua, primero nadando a
        // la salida (el techo, salvo que vaya al hielo junto al pie).
        if (b == null) { if (!wet && a != null && (a.kind == G || a.kind == T)) return Hop.None; b = t.ExitFor(to); }
        // Entre el suelo y el techo: en alfombra (las dos pistas son del árbol).
        if (a != null && ((a.kind == G && b.kind == T) || (a.kind == T && b.kind == G))) return Hop.None;
        // Desde FUERA del árbol a un nivel: las alfombras le llevan a la entrada (al suelo, si va al suelo; si no, al techo: el
        // destino que ven es esa pista, nunca un piso del árbol, que no tiene alfombras y confundiría su red).
        if (a == null && !wet)
        {
            if (b.kind == G || b.kind == T) return Hop.None;
            to = t.World(t.Top.outside);
            return Hop.None;
        }
        PathMover pm = PathMover.Of(g.transform);
        float ph = pm.PivotHeight;
        if (wet) { TreeTrip.Start(g, t, t.SwimPath(null, b, t.transform.InverseTransformPoint(g.transform.position) - Vector3.up * ph), b, true); return Hop.Swimming; }
        Vector3 door = t.World(a.outside) + Vector3.up * ph;
        Vector3 d = door - g.transform.position;
        if (new Vector2(d.x, d.z).magnitude > 0.9f || Mathf.Abs(d.y) > 1.5f) { to = door; return Hop.Walk; }   // primero, a su puerta
        TreeTrip.Start(g, t, t.SwimPath(a, b), b);
        return Hop.Swimming;
    }

    /// <summary>Hechizo «piso»: lleva a <paramref name="who"/> al nivel <paramref name="what"/> andando y nadando por el tronco.</summary>
    public static string GoTo(GameObject who, string what)
    {
        // El árbol en que está (o el más cercano; si no, el Árbol de los Campos). «Servicios:3» elige el edificio.
        ArbolCampos t = null;
        int colon = (what ?? "").IndexOf(':');
        if (colon > 0) { string bn = what.Substring(0, colon).Trim(); what = what.Substring(colon + 1); t = All.Find(x => x != null && x.building.StartsWith(bn, System.StringComparison.OrdinalIgnoreCase)); }
        if (t == null && who != null) t = At(who.transform.position, out _, out _);
        if (t == null) t = Instance;
        if (t == null) return "No hay ningún edificio-árbol en esta escena.";
        TreeLevel lv = t.Find(what);
        if (lv == null) return $"El árbol no tiene «{what}» (piso 0–8, anillo1–4 o techo).";
        TreeWalk.Go(who, t, lv);
        return $"«{who.name}» va a {lv.name} de {t.building} (por el tronco, nadando).";
    }
}
