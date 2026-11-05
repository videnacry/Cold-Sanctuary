using System.Collections;
using UnityEngine;

/// <summary>
/// PRUEBA DEL CICLO DE VIDA en su isla (grupo 58; Berón 2026-11-01: «una escena por animal del santuario y así se comprobaría
/// que realiza todos sus procesos naturales»). Corre DENTRO de una escena Assets/Scenes/CicloVida/CicloVida_*.unity (con
/// «test» del buzón del editor con esa escena abierta, o el menú «Pruebas/Ciclo de vida (sola)» en Play): mira el mismo
/// panel que ve Berón (<see cref="CicloVidaPanel"/>) y espera cada proceso, en orden, con la vida ACELERADA:
///   el tiempo a ×8 y el año a 0,05 días de juego. TODO sigue ese año (docs/ciclo-de-vida.md §Calendario): la gestación, el
///   huevo, la guarida, la colmena y también crecer y envejecer — así que a los padres se les alarga la vida adulta durante
///   la prueba (a 0,05 días el año, una foca vieja moriría en minutos) y, cuando nace la cría, su infancia se fija en
///   ~90 s de juego (le da tiempo a mamar, a seguir a su madre y a salir de la guarida) y su juventud en ~30 s.
///   Al final, el macho (o la reina) llega al último día de su vida.
/// Cada proceso de la especie (beber o sacar el agua de la comida, guarida, huevo, crianza, vuelo nupcial…) se COMPRUEBA:
/// ya no hay SKIP de lo «no modelado». Cada espera, acotada en tiempo REAL.
/// </summary>
public class CicloVidaTest : MonoBehaviour, ITestUnit
{
    public int Group => 58;   // tras los edificios-árbol (57); solo existe en las escenas de ciclo de vida
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (FindAnyObjectByType<CicloVidaPanel>() == null) return;
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<CicloVidaTest>() == null) tr.gameObject.AddComponent<CicloVidaTest>();
    }

    // ¿Su escena le da de comer lo suyo? Un banco de su clase, una planta de su etograma, un cuenco de carne o el panal.
    static bool FoodNearby(string sp)
    {
        if (sp == "SnowBee" || sp == "SnowSpider") return Hive.All.Count > 0;
        GameObject probe = new GameObject("Dieta_prueba"); probe.SetActive(false);
        Forager f = probe.AddComponent<Forager>(); f.ConfigureForSpecies(sp);
        bool ok = (f.eatsFish && Swarm.Nearest(Vector3.zero, SwarmKind.Fish) != null) || (f.eatsKrill && Swarm.Nearest(Vector3.zero, SwarmKind.Krill) != null)
               || (f.eatsClams && Swarm.Nearest(Vector3.zero, SwarmKind.Clam) != null) || (f.eatsPrey && FoodItem.GetPopulation(OrganicMaterial.Meat).Count > 0);
        foreach (EthoAct e in Ethogram.Of(sp)) ok |= Ethogram.Eats(sp, e.kind) && Affordance.Of(e.kind).Count > 0;
        Destroy(probe);
        return ok;
    }

    CicloVidaPanel _p;
    const short LongLife = 30000;   // días de vida: los padres no envejecen durante la prueba

    public IEnumerator Run()
    {
        _p = FindAnyObjectByType<CicloVidaPanel>();
        TestProbe.Begin($"Ciclo de vida en su isla: {(_p != null ? _p.displayName : "?")}");
        if (_p == null) { Debug.Log("[TEST] SKIP · Ciclo de vida — esta escena no es una isla de ciclo de vida"); TestProbe.End(); yield break; }
        // Regla E15 (comprobada también aquí): en su isla hay lo que su ficha dice que come.
        TestProbe.Check("en su isla hay lo que come", FoodNearby(_p.species));
        TimeController tc = TimeController.timeController;
        float oldScale = Time.timeScale, oldYear = tc.DaysPerYear;
        Time.timeScale = 8f;
        tc.DaysPerYear = 0.05f;
        foreach (Hive h in Hive.All) if (h != null) h.secondsPerDay = 0f;   // la colmena, con el mismo calendario
        bool bee = _p.species == "SnowBee";
        LifeHistory life = _p.Life;

        TestProbe.Check("la pareja está en la isla (hembra y macho adultos)", Pair(), Census());
        // Los padres viven toda la prueba (su vejez se prueba al final, a propósito); y empiezan con sed (beber, sin esperar horas).
        foreach (Animal a in _p.Living())
        {
            float r0 = Time.realtimeSinceStartup;
            while (a != null && a.AdultStage == null && Time.realtimeSinceStartup - r0 < 5f) yield return null;
            if (a != null && a.AdultStage != null) a.AdultStage.stageDays = LongLife;
            if (a != null && a.TryGetComponent(out Hydration hy)) hy.thirst = 1.2f;
        }
        TestProbe.Check($"{_p.displayName}: un día de vida dura lo que marca el año del santuario (año de {tc.DaysPerYear:0.##} días de juego)",
            Mathf.Abs(LifeStage.SecondsPerLifeDay(tc) - tc.SecondsForMonths(12f) / 365f) < 1e-3f, $"{LifeStage.SecondsPerLifeDay(tc):0.000} s");

        yield return Wait("comer", 150f);
        Expect("comer", "come");
        yield return Wait("beber", 90f);
        Expect("beber", life.waterLabel);
        // La noche: el reloj a las 22:30 (los nocturnos duermen de día: a las 12:00).
        bool night = SleepCycle.IsNocturnal(_p.species);
        tc.SetClock(tc.Day, night ? 12f : 22.5f);
        yield return Wait("dormir", 40f);
        Expect("dormir", night ? "duerme de día" : "duerme de noche");
        tc.SetClock(tc.Day + 1, night ? 21f : 7.5f);
        yield return Wait("bano", 90f);
        Expect("bano", "va al baño");

        if (bee)
        {
            // VUELO NUPCIAL en las horas cálidas: la reina virgen sale y el zángano la alcanza en la zona de congregación.
            tc.SetClock(tc.Day, 12f);
            yield return Wait("nupcial", 120f);
            Expect("nupcial", "vuelo nupcial: la reina se aparea con el zángano (que muere)");
            foreach (string k in new[] { "puesta", "larva", "pupa", "nace" }) { yield return Wait(k, 120f); Expect(k, _p.Get(k).label); }
            yield return Wait("zangano", 150f);
            Expect("zangano", "nace un zángano de un huevo sin fecundar");
            yield return Wait("crece", 60f);
            Expect("crece", "las obreras cambian de oficio con la edad");
        }
        else
        {
            yield return Wait("celo", 90f);
            Expect("celo", "la hembra entra en celo");
            yield return Wait("gestacion", 240f);
            Expect("cortejo", "la pareja se junta (cortejo/cópula)");
            Expect("gestacion", _p.Get("gestacion").label);
            if (life.eggs)
            {
                yield return Wait("puesta", 120f);
                Expect("puesta", life.layLabel);
                yield return Wait("incuba", 120f);
                Expect("incuba", life.incubateLabel);
            }
            yield return Wait("parto", 180f);
            if (life.den) Expect("guarida", life.denLabel);
            Expect("parto", _p.Get("parto").label);
            Animal cub = _p.born.Count > 0 ? _p.born[0] : null;
            if (cub != null)
            {
                float r0 = Time.realtimeSinceStartup;
                while (cub != null && cub.ChildStage == null && Time.realtimeSinceStartup - r0 < 5f) yield return null;   // su Init
                if (cub != null && cub.ChildStage != null)
                {
                    float day = LifeStage.SecondsPerLifeDay(tc);
                    cub.ChildStage.stageDays = (short)Mathf.Clamp(cub.ChildStage.livedDays + Mathf.CeilToInt(90f / day), 1, short.MaxValue);
                    cub.TeenStage.stageDays = (short)Mathf.Clamp(Mathf.CeilToInt(30f / day), 1, short.MaxValue);
                }
            }
            if (_p.Get("crianza") != null) { yield return Wait("crianza", 60f); Expect("crianza", life.careLabel); }
            yield return Wait("junto", 60f);
            Expect("junto", _p.Get("junto").label);
            if (life.den) { yield return Wait("sale", 90f); Expect("sale", life.denExitLabel); }
            yield return Wait("joven", 120f);
            Expect("joven", "la cría crece a joven");
            yield return Wait("adulto", 120f);
            Expect("adulto", "el joven crece a adulto");
        }

        // VEJEZ: el macho (o la reina) llega al último día de su etapa adulta.
        Animal old = null;
        foreach (Animal a in _p.Living()) if (a.lifeStage == LifeStage.adult && (bee ? a.TryGetComponent(out IceInsectLife il) && il.isQueen : a.sex == Sex.male)) { old = a; break; }
        if (old == null) foreach (Animal a in _p.Living()) if (a.lifeStage == LifeStage.adult) { old = a; break; }
        if (old != null && old.AdultStage != null) old.AdultStage.stageDays = (short)(old.AdultStage.livedDays + 2);
        yield return Wait("vejez", 60f);
        Expect("vejez", "envejece y muere de viejo");
        TestProbe.Check("nadie muere de otra cosa (hambre, ahogo…)", _p.deathsOther == 0, $"{_p.deathsOther} muerte(s)");

        Time.timeScale = oldScale;
        tc.DaysPerYear = oldYear;
        _p.ApplyCalendar();
        TestProbe.End();
    }

    bool Pair()
    {
        bool f = false, m = false;
        foreach (Animal a in _p.Living()) if (a.lifeStage == LifeStage.adult) { if (a.sex == Sex.female) f = true; else m = true; }
        return f && m;
    }

    string Census()
    {
        var sb = new System.Text.StringBuilder();
        foreach (Animal a in _p.Living())
        {
            sb.Append($"{a.name}({a.sex},{a.lifeStage}");
            if (a.TryGetComponent(out Volition vo) && vo.LastDesire.Length > 0) sb.Append($",«{vo.LastDesire}»");
            if (a.TryGetComponent(out IceInsectLife il)) sb.Append($",{il.Doing}");
            sb.Append(") ");
        }
        return sb.ToString();
    }

    // Espera (tope en tiempo REAL) a que el panel vea el proceso.
    IEnumerator Wait(string key, float realSeconds)
    {
        CicloVidaPanel.Row r = _p.Get(key);
        if (r == null) yield break;
        float t0 = Time.realtimeSinceStartup;
        while (!r.seen && Time.realtimeSinceStartup - t0 < realSeconds) yield return null;
    }

    void Expect(string key, string what)
    {
        CicloVidaPanel.Row r = _p.Get(key);
        if (r == null) return;
        TestProbe.Check($"{_p.displayName}: {what}", r.seen, r.seen ? $"a los {r.at / 60f:0.0} min de juego" : $"no se vio · {Census()}");
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Ciclo de vida (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloCicloVida");
        CicloVidaTest t = go.AddComponent<CicloVidaTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(CicloVidaTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
