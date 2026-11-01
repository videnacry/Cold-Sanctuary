using System.Collections;
using UnityEngine;

/// <summary>
/// PRUEBA DEL CICLO DE VIDA en su isla (grupo 58; Berón 2026-11-01: «una escena por animal del santuario y así se comprobaría
/// que realiza todos sus procesos naturales»). Corre DENTRO de una escena Assets/Scenes/CicloVida/CicloVida_*.unity (con
/// «test» del buzón del editor con esa escena abierta, o el menú «Pruebas/Ciclo de vida (sola)» en Play): mira el mismo
/// panel que ve Berón (<see cref="CicloVidaPanel"/>) y espera cada proceso, en orden, con la vida ACELERADA:
///   el tiempo a ×8, el año a 0,05 días (una gestación de meses dura segundos), el día de la colmena a 2 s y, cuando nace la
///   cría, sus etapas de cría y joven acortadas a 3 días; al final, el macho (o la reina) llega al último día de su vida.
/// Cada proceso que el juego NO modela para la especie (beber; la crianza de quien no se vincula con su cría…) sale como
/// SKIP con la razón, no como FAIL: el FAIL es «debería pasar y no pasa». Cada espera, acotada en tiempo REAL.
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

    CicloVidaPanel _p;

    public IEnumerator Run()
    {
        _p = FindAnyObjectByType<CicloVidaPanel>();
        TestProbe.Begin($"Ciclo de vida en su isla: {(_p != null ? _p.displayName : "?")}");
        if (_p == null) { Debug.Log("[TEST] SKIP · Ciclo de vida — esta escena no es una isla de ciclo de vida"); TestProbe.End(); yield break; }
        TimeController tc = TimeController.timeController;
        float oldScale = Time.timeScale, oldYear = tc.DaysPerYear;
        Time.timeScale = 8f;
        tc.DaysPerYear = 0.05f;
        foreach (Hive h in Hive.All) if (h != null) h.secondsPerDay = 2f;
        bool bee = _p.species == "SnowBee";

        TestProbe.Check("la pareja está en la isla (hembra y macho adultos)", Pair(), Census());

        yield return Wait("comer", 150f);
        Expect("comer", "come de su cuenco");
        Skip("beber");
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
            Skip("celo");
            foreach (string k in new[] { "puesta", "larva", "pupa", "nace" }) { yield return Wait(k, 120f); Expect(k, _p.Get(k).label); }
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
            yield return Wait("parto", 120f);
            Expect("parto", _p.Get("parto").label);
            Animal cub = _p.born.Count > 0 ? _p.born[0] : null;
            if (cub != null)
            {
                float r0 = Time.realtimeSinceStartup;
                while (cub != null && cub.ChildStage == null && Time.realtimeSinceStartup - r0 < 5f) yield return null;   // su Init
                if (cub != null && cub.ChildStage != null)
                {
                    cub.ChildStage.stageDays = (short)(cub.ChildStage.livedDays + 3);
                    cub.TeenStage.stageDays = 3;
                }
            }
            yield return Wait("crianza", 60f);
            Expect("crianza", "la madre alimenta a la cría");
            yield return Wait("junto", 30f);
            Expect("junto", _p.Get("junto").label);
            yield return Wait("joven", 120f);
            Expect("joven", "la cría crece a joven");
            yield return Wait("adulto", 120f);
            Expect("adulto", "el joven crece a adulto");
        }

        // VEJEZ: el macho (o la reina) llega al último día de su etapa adulta.
        Animal old = null;
        foreach (Animal a in _p.Living()) if (a.lifeStage == LifeStage.adult && (a.sex == Sex.male || bee)) { old = a; break; }
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
        foreach (Animal a in _p.Living()) sb.Append($"{a.name}({a.sex},{a.lifeStage}) ");
        return sb.ToString();
    }

    // Espera (tope en tiempo REAL) a que el panel vea el proceso.
    IEnumerator Wait(string key, float realSeconds)
    {
        CicloVidaPanel.Row r = _p.Get(key);
        if (r == null || !r.modelled) yield break;
        float t0 = Time.realtimeSinceStartup;
        while (!r.seen && Time.realtimeSinceStartup - t0 < realSeconds) yield return null;
    }

    void Expect(string key, string what)
    {
        CicloVidaPanel.Row r = _p.Get(key);
        if (r == null) return;
        if (!r.modelled && !r.seen) { Skip(key); return; }
        TestProbe.Check($"{_p.displayName}: {what}", r.seen, r.seen ? $"a los {r.at / 60f:0.0} min de juego" : $"no se vio · {Census()}");
    }

    void Skip(string key)
    {
        CicloVidaPanel.Row r = _p.Get(key);
        if (r == null) return;
        Debug.Log($"[TEST] SKIP · {_p.displayName}: {r.label} — {r.note}");
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
