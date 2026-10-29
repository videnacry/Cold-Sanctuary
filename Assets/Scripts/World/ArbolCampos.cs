using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL ÁRBOL DE LOS CAMPOS en juego (docs/edificio-arbol.md): sus niveles y la RUTA por la raíz. Entre pisos no hay
/// escaleras ni alfombras (solo en la copa y en el suelo): se va a la puerta del piso, se entra al agua (el campo magnético
/// la sujeta), se nada por la columna hasta la puerta del piso de destino y se sale andando. Nada de teletransportes: el
/// cuerpo nada cada metro (<see cref="SwimBody.SwimTo"/>). <see cref="PathMover"/> lo usa con <see cref="Route"/>.
/// </summary>
public class ArbolCampos : MonoBehaviour
{
    public static ArbolCampos Instance { get; private set; }
    public List<TreeLevel> levels = new List<TreeLevel>();
    public Transform water;          // el agua de la raíz (renderer)
    public Transform wall;           // la pared transparente (malla con los huecos de las puertas)
    public Transform top;            // la copa (plataforma de alfombras)
    [Tooltip("Velocidad de nado por la raíz (m/s): más que nadar en el mar, para no quedarse sin aire (100 m en 40 s).")]
    public float routeSwimSpeed = 2.5f;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    public TreeLevel Ground => levels.Find(l => l.kind == TreeLevel.Kind.Ground);
    public TreeLevel Floor(int k) => levels.Find(l => l.kind == TreeLevel.Kind.Floor && l.index == k);
    public TreeLevel Ring(int k) => levels.Find(l => l.kind == TreeLevel.Kind.Ring && l.index == k);
    public Vector3 World(Vector3 local) => transform.TransformPoint(local);

    /// <summary>¿El punto está dentro del agua de la raíz?</summary>
    public bool InRootWater(Vector3 world)
    {
        Vector3 l = transform.InverseTransformPoint(world);
        return l.y > TreeSpec.WaterBottom - 0.5f && l.y < TreeSpec.WaterTop && new Vector2(l.x, l.z).magnitude < TreeSpec.WaterRadius;
    }

    /// <summary>El nivel en que está <paramref name="world"/> (los pies o el pivote), o null si no es del árbol.</summary>
    public TreeLevel LevelAt(Vector3 world)
    {
        Vector3 l = transform.InverseTransformPoint(world);
        float r = new Vector2(l.x, l.z).magnitude;
        foreach (TreeLevel lv in levels)
        {
            if (lv.deck == null) continue;
            Vector3 d = lv.deck.InverseTransformPoint(world);
            if (d.y < -0.6f || d.y > 3f) continue;
            if (lv.kind == TreeLevel.Kind.Floor && r > TreeSpec.WaterRadius && Mathf.Abs(d.x) <= TreeSpec.FloorHalf + TreeSpec.FloorEdge && Mathf.Abs(d.z) <= TreeSpec.FloorHalf + TreeSpec.FloorEdge) return lv;
            if (lv.kind == TreeLevel.Kind.Ring && Mathf.Abs(d.x) <= 7.5f && Mathf.Abs(d.z) <= 7.5f) return lv;
            if (lv.kind == TreeLevel.Kind.Top && r <= TreeSpec.TopWidth * 0.5f + 0.5f) return lv;
        }
        if (l.y < 3f && l.y > -2f && r > TreeSpec.WaterRadius && r < 45f) return Ground;
        return null;
    }

    /// <summary>Nivel por nombre corto: «0»/«suelo», «1»…«8», «anillo3», «copa».</summary>
    public TreeLevel Find(string what)
    {
        what = (what ?? "").Trim().ToLowerInvariant().Replace("piso", "").Replace(" ", "");
        if (what == "0" || what == "suelo" || what == "ground") return Ground;
        if (what == "copa" || what == "top") return levels.Find(l => l.kind == TreeLevel.Kind.Top);
        if (what.StartsWith("anillo") && int.TryParse(what.Substring(6), out int ri)) return Ring(ri);
        if (int.TryParse(what, out int fi)) return Floor(fi);
        return null;
    }

    /// <summary>El camino a nado (local del árbol, a ras de los pies) de la puerta de <paramref name="a"/> a la de
    /// <paramref name="b"/>: puerta → agua → por la columna (a 6 m del eje) → [por la rama] → puerta → fuera.</summary>
    public List<Vector3> SwimPath(TreeLevel a, TreeLevel b, Vector3? fromLocal = null)
    {
        var p = new List<Vector3>();
        if (fromLocal.HasValue) p.Add(fromLocal.Value);
        else { p.Add(a.outside); p.Add(a.door); p.Add(a.inside); for (int i = a.via.Length - 1; i >= 0; i--) p.Add(a.via[i]); }
        Vector3 last = p[p.Count - 1];
        Vector3 bFirst = b.via.Length > 0 ? b.via[0] : b.inside;
        float aAng = TreeSpec.Angle(new Vector3(last.x, 0f, last.z)), bAng = TreeSpec.Angle(new Vector3(bFirst.x, 0f, bFirst.z));
        p.Add(TreeSpec.Dir(aAng) * 6f + Vector3.up * last.y);
        p.Add(TreeSpec.Dir(bAng) * 6f + Vector3.up * bFirst.y);
        p.AddRange(b.via); p.Add(b.inside); p.Add(b.door); p.Add(b.outside);
        return p;
    }

    public enum Hop { None, Walk, Swimming }

    /// <summary>
    /// La parte del ÁRBOL de una ruta (la llama <see cref="PathMover.StepToward"/> antes que las alfombras):
    /// - <see cref="Hop.Swimming"/>: va nadando por la raíz (no hay que moverlo);
    /// - <see cref="Hop.Walk"/>: anda por su piso hacia <paramref name="to"/> (quizá cambiado por la puerta), sin alfombras;
    /// - <see cref="Hop.None"/>: nada que ver con el árbol (o se llega en alfombra: al suelo o a la copa).
    /// </summary>
    public static Hop Route(GameObject g, ref Vector3 to)
    {
        ArbolCampos t = Instance;
        if (t == null || g == null) return Hop.None;
        if (TreeTrip.Busy(g)) return Hop.Swimming;
        TreeLevel a = t.LevelAt(g.transform.position), b = t.LevelAt(to);
        bool wet = a == null && t.InRootWater(g.transform.position);
        if (a == null && b == null && !wet) return Hop.None;   // nada que ver con el árbol
        const TreeLevel.Kind G = TreeLevel.Kind.Ground, T = TreeLevel.Kind.Top;
        if (a != null && a == b) return a.kind == G || a.kind == T ? Hop.None : Hop.Walk;   // mismo nivel: andando
        // Sale del árbol: desde el suelo o la copa, en alfombra; desde un piso o un anillo, primero nadando al suelo.
        if (b == null) { if (a != null && (a.kind == G || a.kind == T)) return Hop.None; b = t.Ground; }
        // A la COPA: desde fuera o desde el suelo, en alfombra; desde un piso o un anillo, nadando al suelo y luego en alfombra.
        if (b.kind == T) { if ((a == null && !wet) || (a != null && a.kind == G)) return Hop.None; b = t.Ground; }
        // Desde FUERA del árbol (o desde la copa) a un piso o un anillo: a la puerta del suelo (las alfombras le llevan al pie
        // del árbol: el destino que ven es el pie, nunca un piso del árbol, que no tiene alfombras y confundiría su red).
        if ((a == null && !wet) || (a != null && a.kind == T))
        {
            if (b.kind == G && a == null) return Hop.None;
            to = t.World(t.Ground.outside);
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

    /// <summary>Hechizo «piso»: lleva a <paramref name="who"/> al nivel <paramref name="what"/> andando y nadando por la raíz.</summary>
    public static string GoTo(GameObject who, string what)
    {
        ArbolCampos t = Instance;
        if (t == null) return "No hay Árbol de los Campos en esta escena.";
        TreeLevel lv = t.Find(what);
        if (lv == null) return $"El árbol no tiene «{what}» (piso 0–8, anillo1–6 o copa).";
        TreeWalk.Go(who, t, lv);
        return $"«{who.name}» va a {lv.name} del Árbol de los Campos (por la raíz, nadando).";
    }
}

