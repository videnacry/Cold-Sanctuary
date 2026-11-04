using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA REGLA DE ALTURA DE LA LANZADERA DE COHETES (Berón 2026-11-04; docs/lanzadera-de-cohetes.md):
/// «siempre que se agrega un elemento elevado se valide y se ajuste la altura del lanza-cohetes para que no le afecte».
///   altura de la plataforma ≥ máx(lo más alto del santuario, techo de vuelo de las aves) + margen.
/// - Lo más alto del santuario: el techo de cada edificio-árbol con la columna de su puerta (<see cref="ArbolCampos"/>) y todo lo que
///   lleve un <see cref="ElevatedElement"/> (el árbol de la guardia, torres, anillos, lo que se añada).
/// - Techo de vuelo de las aves: lo más alto que vuelan las aves del santuario (<see cref="BirdBehavior"/>: min + 3 × max) y, como
///   mínimo, <see cref="RealBirdBand"/> (152 m = 500 pies: el ~72 % de los choques con aves en aviación son por debajo).
/// - Margen: <see cref="Margin"/> (30 m: el chorro de los propulsores, la nube del diluvio y los cascotes no bajan a esa franja).
/// Lo calcula <see cref="RequiredPadHeight"/> con lo que hay en la escena; el constructor lo usa al hacer la lanzadera y lo VALIDA al
/// final del Blockout (si algo creció, sube la plataforma); en juego, cada <see cref="ElevatedElement"/> que aparece vuelve a validar.
/// </summary>
public static class LaunchHeightRule
{
    public const string Building = "Cohetes";
    /// <summary>Franja de vuelo de la mayoría de las aves (152 m = 500 pies: el ~72 % de los choques con aves, por debajo; FAA/USDA).</summary>
    public const float RealBirdBand = 152f;
    public const float Margin = 30f;
    /// <summary>Distancia mínima (m) de la plataforma a cualquier edificio o isla del santuario: la de edificio habitado (IBD,
    /// K40 → 1,2 psi) de 2 t de TNT equivalente (el 20 % de ~10 t de LOX/RP-1 de un cohete de la clase Electron), 201 m, con un
    /// 50 % más por los cascotes.</summary>
    public const float MinDistance = 300f;
    /// <summary>Equivalente en TNT (kg) y su distancia de edificio habitado (m): 15,9 m·kg^-1/3 (K40 en pies/libras^1/3).</summary>
    public const float PropellantKg = 10200f, TntFraction = 0.2f;
    public static float TntKg => PropellantKg * TntFraction;
    public static float InhabitedBuildingDistance => 15.87f * Mathf.Pow(TntKg, 1f / 3f);

    /// <summary>Lo más alto del santuario (sin la lanzadera) y qué es.</summary>
    public static float TallestTop(out string what)
    {
        float top = 0f; what = "nada";
        foreach (ArbolCampos t in Object.FindObjectsByType<ArbolCampos>(FindObjectsSortMode.None))
        {
            if (t == null || t.building == Building) continue;
            float y = t.transform.position.y + t.rootHeight + TreeSpec.DoorHeight + 1.6f;   // el techo con la columna de su puerta
            if (y > top) { top = y; what = $"el techo de {t.building}"; }
        }
        foreach (ElevatedElement e in Object.FindObjectsByType<ElevatedElement>(FindObjectsSortMode.None))
        {
            if (e == null || e.GetComponentInParent<LaunchSite>() != null) continue;
            float y = e.Top;
            if (y > top) { top = y; what = e.label.Length > 0 ? e.label : e.name; }
        }
        return top;
    }

    /// <summary>Lo más alto que vuelan las aves del santuario (y, como mínimo, la franja de la mayoría de las aves).</summary>
    public static float BirdCeiling(out string what)
    {
        float game = 0f;
        foreach (BirdBehavior b in Object.FindObjectsByType<BirdBehavior>(FindObjectsSortMode.None))
            if (b != null) game = Mathf.Max(game, b.min + 3f * b.max);
        what = game > RealBirdBand ? $"las aves del santuario ({game:0} m)" : $"la franja de vuelo de la mayoría de las aves ({RealBirdBand:0} m; las del santuario, {game:0} m)";
        return Mathf.Max(game, RealBirdBand);
    }

    /// <summary>LA REGLA: la cara de la plataforma, como mínimo.</summary>
    public static float RequiredPadHeight(out string why)
    {
        float top = TallestTop(out string tw), birds = BirdCeiling(out string bw);
        float req = Mathf.Max(top, birds) + Margin;
        why = $"máx({tw} {top:0} m, {bw}) + {Margin:0} m = {req:0} m";
        return req;
    }
}

/// <summary>Un ELEMENTO ELEVADO (una torre, un árbol que crece, un anillo, lo que se añada): la regla de la lanzadera lo cuenta. Al
/// aparecer en juego, vuelve a validar la altura de la plataforma (y la sube si hace falta).</summary>
public class ElevatedElement : MonoBehaviour
{
    public string label = "";
    [Tooltip("Lo más alto (y); si es 0, se mide con sus dibujos.")]
    public float top;

    public float Top
    {
        get
        {
            if (top > 0f) return top;
            float y = transform.position.y; bool any = false;
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer || r.bounds.size.y > 1000f) continue;
                y = any ? Mathf.Max(y, r.bounds.max.y) : r.bounds.max.y; any = true;
            }
            return y;
        }
    }

    void OnEnable() { if (Application.isPlaying && LaunchSite.Instance != null) LaunchSite.Instance.Validate($"apareció «{name}»"); }
}

/// <summary>
/// LA PLATAFORMA DE LANZAMIENTO (docs/lanzadera-de-cohetes.md): la corona sobre el techo del árbol-lanzadera, con su cohete, el
/// deflector, la trinchera de llamas, el diluvio y el escudo. <see cref="Validate"/> aplica la regla de altura: si queda por debajo de
/// lo que pide, SUBE la corona (y alarga sus pilares).
/// </summary>
public class LaunchSite : MonoBehaviour
{
    public static LaunchSite Instance { get; private set; }
    [Tooltip("La corona (la plataforma con todo lo de encima): su y es la cara de la plataforma.")]
    public Transform deck;
    [Tooltip("Los pilares de la corona: van del techo del árbol a la plataforma.")]
    public List<Transform> pillars = new List<Transform>();
    [Tooltip("Cara del techo del árbol (donde apoyan los pilares).")]
    public float roofY;
    public int Raises { get; private set; }
    public string LastWhy { get; private set; } = "";

    public float PadY => deck != null ? deck.position.y : transform.position.y;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }
    void Start() { Validate("al empezar"); }

    /// <summary>¿Cumple la regla? Si no, sube la corona a la altura que pide. Devuelve si tuvo que subirla.</summary>
    public bool Validate(string reason)
    {
        float need = LaunchHeightRule.RequiredPadHeight(out string why);
        LastWhy = why;
        if (PadY >= need - 0.01f) return false;
        RaiseTo(need);
        Debug.LogWarning($"[Lanzadera] La plataforma estaba a {PadY:0} m y la regla pide {why} ({reason}): sube.");
        return true;
    }

    /// <summary>Sube la corona a <paramref name="y"/> y alarga sus pilares desde el techo.</summary>
    public void RaiseTo(float y)
    {
        if (deck == null) return;
        Vector3 p = deck.position; p.y = y; deck.position = p;
        foreach (Transform pl in pillars)
        {
            if (pl == null) continue;
            float h = Mathf.Max(0.5f, y - roofY);
            Vector3 lp = pl.position; lp.y = roofY + h * 0.5f; pl.position = lp;
            Vector3 s = pl.localScale; s.y = h; pl.localScale = s;
        }
        Raises++;
        Physics.SyncTransforms();
    }

    /// <summary>Las misiones de la jornada en la lanzadera (si existe): control de misión y la revisión de la seguridad de la plataforma.</summary>
    public static List<WorkRoster.Mission> Missions()
    {
        var l = new List<WorkRoster.Mission>();
        if (Object.FindAnyObjectByType<LaunchSite>() == null) return l;
        l.Add(new WorkRoster.Mission(ControlArea, "lanz_control", "Control de misión: telemetría, meteorología (viento en altura, techo de nubes) y cuenta atrás de la próxima ventana", 1f));
        l.Add(new WorkRoster.Mission(ControlArea, "lanz_seguridad", "Seguridad de la plataforma: probar el diluvio de agua, limpiar la trinchera de llamas y revisar el escudo y la cubeta de contención", 1f) { roams = true });
        l.Add(new WorkRoster.Mission(ControlArea, "lanz_fabrica", "Fábrica de cohetes: integrar etapas y la carga útil (la trae el submarino de obra)", 2f));
        return l;
    }
    public const string ControlArea = "Control de misión", FactoryFloor = "Fábrica de cohetes";
}
