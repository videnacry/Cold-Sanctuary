using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// PRUEBA DE LA VISTA DE CORTE (docs/plan-microcosmos-realismo.md §3.3, P2; regla 2 de docs/microcosmos-reglas-edificacion.md), sin
/// cargar escenas: una casa de 6 × 6 m con cuatro muros de 3 m (uno con su puerta de 1,2 m) y su tejado.
/// 1. <see cref="CutawayView.Ensure"/> reúne los cuatro muros y el tejado (por nombre y por forma) y su planta;
/// 2. con el jugador y la cámara fuera, todo se ve;
/// 3. con el jugador dentro y la cámara al sur, fuera: el tejado y el muro del sur se desvanecen (quedan solo para sombras) y los
///    del norte, el este y el oeste se ven;
/// 4. con la cámara al este, se desvanece el del este y vuelve el del sur;
/// 5. al salir, todo vuelve a su modo de sombras de antes; y <see cref="CutawayView.ShowAll"/> deja todo visible.
/// </summary>
public class CutawayTest : MonoBehaviour, ITestUnit
{
    public int Group => 122;   // tras la escala del cuerpo (121); no carga escenas
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<CutawayTest>() == null) tr.gameObject.AddComponent<CutawayTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Vista de corte: por dentro se desvanecen el techo y los muros entre la cámara y el jugador");
        var house = new GameObject("Casa_corte_prueba").transform; house.position = new Vector3(-9100f, -600f, 9100f);
        try
        {
            var site = house.gameObject.AddComponent<AreaFunctionSite>(); site.title = "Casa de prueba de la vista";   // (sin «corte» ni «puerta» en el título: el check de abajo busca esas palabras en las faltas)
            Renderer north = Wall(house, "Muro_N", new Vector3(0f, 1.5f, 3f), new Vector3(6f, 3f, 0.3f));
            Renderer south = Wall(house, "Muro_S", new Vector3(0f, 1.5f, -3f), new Vector3(6f, 3f, 0.3f));
            Renderer east = Wall(house, "Panel_E", new Vector3(3f, 1.5f, 0f), new Vector3(0.3f, 3f, 6f));   // muro por su forma, no por el nombre
            Renderer west = Wall(house, "Muro_O", new Vector3(-3f, 1.5f, 0f), new Vector3(0.3f, 3f, 6f));
            Renderer roof = Wall(house, "Tejado", new Vector3(0f, 3.2f, 0f), new Vector3(6.6f, 0.3f, 6.6f));
            MicroDoor.Place(house, "Puerta", new Vector3(0f, 0f, -3f), 0f, 1.2f, 2.1f);
            yield return null;

            CutawayView cv = CutawayView.Ensure(house);
            TestProbe.Check("[MIC-85] reúne los cuatro muros (por nombre y por forma) y el tejado", cv.walls.Count == 4 && cv.roofs.Count == 1 && cv.roofs.Contains(roof) && cv.walls.Contains(east), $"{cv.walls.Count} muros, {cv.roofs.Count} techos");
            TestProbe.Check("[MIC-85] su planta cubre la casa (6 × 6 m)", cv.interior.size.x >= 5.9f && cv.interior.size.z >= 5.9f && cv.Inside(house.position + Vector3.up) && !cv.Inside(house.position + new Vector3(0f, 1f, -8f)), $"{cv.interior.size}");
            TestProbe.Check("[MIC-85] la regla P2 la da por buena (sin falta de vista de corte ni de puerta)", !MicroBuildRules.Validate(house, null, false, false).Exists(s => s.Contains("vista de corte") || s.Contains("sin puerta") || s.Contains("puerta «")));

            Vector3 inside = house.position + new Vector3(0f, 0.5f, 0.5f), outside = house.position + new Vector3(0f, 0.5f, -10f);
            Vector3 camSouth = house.position + new Vector3(0f, 4f, -9f), camEast = house.position + new Vector3(9f, 4f, 0.5f);

            cv.Refresh(outside, camSouth);
            TestProbe.Check("[MIC-85] fuera: todo se ve", cv.HiddenCount == 0 && !cv.InsideNow && Visible(roof) && Visible(south));

            cv.Refresh(inside, camSouth);
            TestProbe.Check("[MIC-85] dentro, cámara al sur: se desvanecen el tejado y el muro del sur", cv.InsideNow && cv.IsHidden(roof) && cv.IsHidden(south) && !Visible(roof) && !Visible(south),
                $"ocultos {cv.HiddenCount}; tejado {roof.shadowCastingMode}, sur {south.shadowCastingMode}");
            TestProbe.Check("[MIC-85] … y los del norte, el este y el oeste se ven (la fachada del otro lado no se pierde)", Visible(north) && Visible(east) && Visible(west));

            cv.Refresh(inside, camEast);
            TestProbe.Check("[MIC-85] cámara al este: se desvanece el del este y vuelve el del sur", cv.IsHidden(east) && !cv.IsHidden(south) && Visible(south) && cv.IsHidden(roof));

            cv.Refresh(outside, camSouth);
            TestProbe.Check("[MIC-85] al salir, todo vuelve como estaba (sombras de antes)", cv.HiddenCount == 0 && Visible(roof) && Visible(east) && roof.shadowCastingMode == ShadowCastingMode.On);

            cv.Refresh(inside, camSouth); cv.ShowAll();
            TestProbe.Check("[MIC-85] ShowAll deja todo visible", cv.HiddenCount == 0 && Visible(roof) && Visible(south));
        }
        finally { Destroy(house.gameObject); }
        TestProbe.End();
    }

    static bool Visible(Renderer r) => r.enabled && r.shadowCastingMode != ShadowCastingMode.ShadowsOnly;

    static Renderer Wall(Transform parent, string name, Vector3 local, Vector3 size)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
        g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localScale = size;
        Renderer r = g.GetComponent<Renderer>(); r.shadowCastingMode = ShadowCastingMode.On;
        return r;
    }
}
