using System.Collections;
using UnityEngine;

/// <summary>
/// PRUEBA DEL CUERPO ENCOGIDO DE LOS INSECTOS-HUMANOS (docs/plan-microcosmos-realismo.md §3.1 y fase 0b; Berón 2026-11-18: «la mitad de
/// grandes» y «el ancho es importante»), sin cargar escenas. Un vecino hormiga (<see cref="CubeInsect.DressFolk"/>) a la estatura
/// tipo × s (<see cref="MicroBuildRules.FolkStature"/>, 0,875 m):
/// 1. mide eso de alto (± 8 cm, con las antenas);
/// 2. con la POSTURA RECOGIDA (<see cref="CubeInsect.FolkTuckKnee"/>), su ancho de pie es ≤ 0,95 m y, andando (el barrido de la coxa
///    de ±25° y el trocánter alzado), ≤ 1,05 m: cabe por una puerta de 1,2 m (<see cref="MicroBuildRules.DoorWidth"/>) y por debajo de
///    su alto mínimo; una hormiga de las mismas medidas con las patas abiertas (la fauna) es al menos un 20 % más ancha;
/// 3. su ficha de muebles (<see cref="MicroBody.Of"/>) no se recorta a los mínimos de antes (largo ≥ 1,2 m): el largo es el suyo, y
///    <see cref="MicroBody.OfSpecies"/> sin estatura da la del insecto-humano encogido;
/// 4. la MESA ALTA queda a sus manos ± 0,05 m (<see cref="MicroBody.StandingTable"/>, con las manos medidas en los cubos) y el
///    APOYO DE ABDOMEN, bajo su gáster (<see cref="MicroBody.AbdomenRest"/>, entre 6 cm por debajo del vientre y 2 cm por encima).
/// Escribe en la consola las medidas («[Escala] …») para la captura del piloto.
/// </summary>
public class FolkScaleTest : MonoBehaviour, ITestUnit
{
    public int Group => 121;   // (119 y 120 son de otras ramas: CodeShipLogicTest); no carga escenas
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<FolkScaleTest>() == null) tr.gameObject.AddComponent<FolkScaleTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Insectos-humanos a la mitad: estatura, ancho con las patas recogidas, puerta, mesa alta y apoyo de abdomen");
        float s = MicroBuildRules.FolkScale, h = MicroBuildRules.FolkStature;
        var g = new GameObject("Vecino_escala_prueba"); g.transform.position = new Vector3(9300f, -650f, -9300f);
        var wild = new GameObject("Hormiga_patas_abiertas_prueba"); wild.transform.position = new Vector3(9310f, -650f, -9300f);
        try
        {
            var cap = g.AddComponent<CapsuleCollider>(); cap.height = h; cap.center = Vector3.up * h * 0.5f;
            CubeInsect ci = CubeInsect.DressFolk(g, "Ant", h, "Vecino", true);
            yield return null;

            // 1 · La estatura.
            Bounds all = HostBounds(g);
            TestProbe.Near($"[MIC-84] mide la estatura tipo × s ({h:0.00} m, con las antenas)", all.size.y, h, 0.08f);

            // 2 · El ancho, de pie y andando, con las patas recogidas.
            Vector2 stand = ci.Footprint(false), walk = ci.Footprint(true);
            Debug.Log($"[Escala] Vecino hormiga con s = {s}: {all.size.y:0.00} m de alto; de pie {stand.x:0.00} × {stand.y:0.00} m, andando {walk.x:0.00} × {walk.y:0.00} m (patas recogidas).");
            TestProbe.Check("[MIC-84] de pie, con las patas recogidas, ≤ 0,95 m de ancho", stand.x > 0.2f && stand.x <= 0.95f, $"{stand.x:0.00} m");
            TestProbe.Check($"[MIC-84] andando (barrido de las patas), ≤ {MicroBuildRules.TuckedWalkWidth + 0.05f:0.00} m de ancho", walk.x > 0.2f && walk.x <= MicroBuildRules.TuckedWalkWidth + 0.05f, $"{walk.x:0.00} m (previsto {MicroBuildRules.TuckedWalkWidth:0.00})");
            TestProbe.Check($"[MIC-84] cabe andando por una puerta de {MicroBuildRules.DoorWidth:0.0} × {MicroBuildRules.DoorHeight:0.00} m", walk.x <= MicroBuildRules.DoorWidth - 0.1f && all.size.y <= MicroBuildRules.DoorHeight, $"{walk.x:0.00} × {all.size.y:0.00} m");
            // La misma hormiga con las patas abiertas (la de la fauna), al mismo largo L.
            Transform ins = g.transform.Find("Insecto");
            float L = ins != null ? Mathf.Abs(ins.lossyScale.z) : 0f;
            CubeInsect open = CubeInsect.Dress(wild, "Ant", InsectCast.ColorOf("Ant"), L);
            yield return null;
            Vector2 openStand = open.Footprint(false);
            TestProbe.Check("[MIC-84] las patas recogidas estrechan al menos un 20 % (frente a la hormiga de la fauna del mismo largo)", openStand.x > 0f && stand.x <= 0.8f * openStand.x, $"recogidas {stand.x:0.00} m · abiertas {openStand.x:0.00} m (L = {L:0.00} m)");

            // 3 · La ficha de muebles, sin los mínimos de antes.
            MicroBody.Fit f = MicroBody.Of(g);
            Debug.Log($"[Escala] Ficha del vecino: {f}");
            TestProbe.Check("[MIC-84] su largo no se recorta al mínimo de antes (1,2 m): es el suyo", f.length < 1.2f && f.length > 0.4f, $"{f.length:0.00} m");
            MicroBody.Fit sp = MicroBody.OfSpecies("Ant");
            TestProbe.Check("[MIC-84] OfSpecies sin estatura: la del insecto-humano encogido, y un largo parecido al medido", Mathf.Approximately(sp.height, h) && Mathf.Abs(sp.length - f.length) < 0.25f, $"{sp.height:0.00} m, largo {sp.length:0.00} (medido {f.length:0.00})");

            // 4 · La mesa alta a las manos y el apoyo de abdomen bajo el gáster.
            float hands = MeasuredHands(g), belly = MeasuredBelly(g);
            float table = MicroBody.StandingTable(f), rest = MicroBody.AbdomenRest(f);
            Debug.Log($"[Escala] Manos a {hands:0.00} m, mesa alta a {table:0.00} m; vientre a {belly:0.00} m, apoyo de abdomen a {rest:0.00} m.");
            TestProbe.Check("[MIC-84] la mesa alta queda a sus manos ± 0,05 m", hands > 0f && Mathf.Abs(table - hands) <= 0.05f, $"mesa {table:0.00} m, manos {hands:0.00} m");
            TestProbe.Check("[MIC-84] las manos, a la mitad de las de una persona sentada a la mesa (0,3–0,6 m)", hands >= 0.3f && hands <= 0.6f, $"{hands:0.00} m");
            TestProbe.Check("[MIC-84] el apoyo de abdomen queda bajo su gáster (entre −6 y +2 cm del vientre)", belly > 0f && rest >= belly - 0.06f && rest <= belly + 0.02f, $"apoyo {rest:0.00} m, vientre {belly:0.00} m");
            TestProbe.Check("[MIC-84] el apoyo de abdomen, de 1,5 × el ancho del cuerpo", Mathf.Abs(MicroBody.AbdomenRestWidth(f) - 1.5f * f.width) < 0.01f, $"{MicroBody.AbdomenRestWidth(f):0.00} m (cuerpo {f.width:0.00})");
        }
        finally { Destroy(g); Destroy(wild); }
        TestProbe.End();
    }

    // La caja de todo lo visible del cuerpo, en el marco del anfitrión.
    static Bounds HostBounds(GameObject g)
    {
        Bounds b = default; bool any = false;
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            Bounds w = r.bounds;
            Vector3 lo = g.transform.InverseTransformPoint(w.min), hi = g.transform.InverseTransformPoint(w.max);
            Bounds l = new Bounds((lo + hi) * 0.5f, new Vector3(Mathf.Abs(hi.x - lo.x), Mathf.Abs(hi.y - lo.y), Mathf.Abs(hi.z - lo.z)));
            if (!any) { b = l; any = true; } else b.Encapsulate(l);
        }
        return b;
    }

    // La altura media de las piezas «Mano» (el torso erguido), en el marco del anfitrión.
    static float MeasuredHands(GameObject g)
    {
        float y = 0f; int n = 0;
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>(true)) if (r.name == "Mano") { y += g.transform.InverseTransformPoint(r.bounds.center).y; n++; }
        return n > 0 ? y / n : -1f;
    }

    // El vientre: la cara de abajo del último segmento (el gáster de la hormiga).
    static float MeasuredBelly(GameObject g)
    {
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>(true)) if (r.name == "Gaster") return g.transform.InverseTransformPoint(r.bounds.min).y;
        return -1f;
    }
}
