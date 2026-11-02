using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PRUEBA DEL CICLO DE VIDA DE UNA PERSONA (grupo 59; Berón 2026-11-02: «¿el ciclo de vida está probado para los humanos? El ciclo
/// de vida de los humanos y de los insectos-humanos es el mismo»). LA MISMA prueba corre en las dos escenas
/// (Assets/Scenes/CicloVida/CicloVida_Humano y CicloVida_InsectoHumano): mira el panel (<see cref="CicloHumanoPanel"/>) y espera
/// cada paso, en orden, con la vida ACELERADA como en el grupo 58: el tiempo a ×8 y el año a 0,05 días de juego.
///   · La pareja: una mujer y un hombre adultos, en la misma casa (cama para cada uno, retrete, lavabo, mesa con sillas, despensa),
///     todavía sin pareja; con el cuerpo de una persona (BodyMetabolism, NeedsDriver, BodyAging, PersonLife).
///   · Necesidades: con hambre comen en casa, con ganas van al baño, sucios se lavan y de noche duermen en su cama.
///   · Cortejo → pareja → embarazo (de noche, juntos en casa) → parto a los 9 meses → el niño va con su madre, come y crece a
///     joven y a adulto (a su crecimiento se le dan ~20 s reales, como a la cría del grupo 58).
///   · Vejez: al padre se le acorta la vida (como el grupo 58 a su macho): llega a anciano y muere de viejo; nadie muere de otra cosa.
/// Cada espera, acotada en tiempo REAL.
/// </summary>
public class CicloHumanoTest : MonoBehaviour, ITestUnit
{
    public int Group => 59;   // tras el ciclo de vida de los animales (58); solo existe en las escenas de ciclo de vida de una persona
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (FindAnyObjectByType<CicloHumanoPanel>() == null) return;
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<CicloHumanoTest>() == null) tr.gameObject.AddComponent<CicloHumanoTest>();
    }

    CicloHumanoPanel _p;
    const float LongLife = 1000f;   // años: los padres no envejecen durante la prueba

    public IEnumerator Run()
    {
        _p = FindAnyObjectByType<CicloHumanoPanel>();
        TestProbe.Begin($"Ciclo de vida de una persona: {(_p != null ? _p.displayName : "?")}");
        if (_p == null) { Debug.Log("[TEST] SKIP · Ciclo humano — esta escena no es de ciclo de vida de una persona"); TestProbe.End(); yield break; }
        TimeController tc = TimeController.timeController;
        float oldScale = Time.timeScale, oldYear = tc.DaysPerYear;
        Time.timeScale = 8f;
        tc.DaysPerYear = 0.05f;

        PersonLife she = null, he = null;
        foreach (PersonLife p in _p.Living()) if (p.mother == null) { if (p.female && she == null) she = p; else if (!p.female && he == null) he = p; }
        bool pair = she != null && he != null && she.Adult && he.Adult;
        TestProbe.Check($"{_p.displayName}: una mujer y un hombre adultos", pair, Census());
        if (!pair) { Restore(oldScale, oldYear); TestProbe.End(); yield break; }
        TestProbe.Check($"{_p.displayName}: con el cuerpo de una persona (BodyMetabolism, NeedsDriver, BodyAging, PersonLife)",
            Body(she) && Body(he));
        var homeBad = new List<string>();
        foreach (GameObject root in _p.gameObject.scene.GetRootGameObjects()) homeBad.AddRange(Household.Check(root.transform));
        TestProbe.Check($"{_p.displayName}: comparten casa, con cama para cada uno, retrete, lavabo, mesa con sillas y despensa",
            she.home != null && she.home == he.home && homeBad.Count == 0, string.Join(" · ", homeBad));
        TestProbe.Check($"{_p.displayName}: todavía sin pareja (el cortejo se ve aquí)", she.partner == null && he.partner == null);
        foreach (PersonLife p in new[] { she, he }) { p.lifeYears = LongLife; p.ApplyCalendar(); }

        // ── NECESIDADES ──
        tc.SetClock(tc.Day, 9f);
        Set(she, hunger: 0.9f); Set(he, hunger: 0.9f);
        yield return Wait("comer", 90f); Expect("comer");
        Set(she, toilet: 0.95f); Set(he, toilet: 0.95f);
        yield return Wait("bano", 60f); Expect("bano");
        Set(she, dirt: 0.95f); Set(he, dirt: 0.95f);
        yield return Wait("lavarse", 60f); Expect("lavarse");
        tc.SetClock(tc.Day, 22.5f);
        foreach (PersonLife p in new[] { she, he }) Sleepy(p, 0.9f);
        yield return Wait("dormir", 60f); Expect("dormir");
        foreach (PersonLife p in new[] { she, he }) Sleepy(p, 0.02f);   // descansados: se despiertan (dormir 8 h no cabe en la prueba)
        tc.SetClock(tc.Day + 1, 8f);

        // ── CORTEJO, PAREJA, EMBARAZO, PARTO ──
        yield return Wait("cortejo", 60f); Expect("cortejo");
        yield return Wait("pareja", 120f); Expect("pareja");
        TestProbe.Check($"{_p.displayName}: la pareja sigue en la misma casa", she.partner == he && she.home == he.home);
        tc.SetClock(tc.Day, 22.5f);   // de noche, juntos en casa
        yield return Wait("embarazo", 90f); Expect("embarazo");
        yield return Wait("parto", 120f); Expect("parto");

        // ── EL HIJO CRECE ──
        PersonLife kid = null;
        foreach (PersonLife p in _p.Living()) if (p.mother == she) kid = p;
        TestProbe.Check($"{_p.displayName}: el hijo es de la familia (su madre, su padre y su casa)", kid != null && kid.father == he && kid.home == she.home && kid.Stage == LifeStage.child,
            kid != null ? $"{kid.name}: etapa {kid.Stage}" : "no nació");
        if (kid != null)
        {
            kid.growYears = 1f; kid.ApplyCalendar();   // ~20 s reales de niño a adulto (como la cría del grupo 58)
            Set(kid, hunger: 0.7f);
            tc.SetClock(tc.Day + 1, 9f);
        }
        yield return Wait("junto", 60f); Expect("junto");
        yield return Wait("hijo_come", 90f); Expect("hijo_come");
        yield return Wait("joven", 120f); Expect("joven");
        yield return Wait("adulto", 120f); Expect("adulto");

        // ── VEJEZ: el padre llega al final de su vida ──
        he.lifeYears = 0.5f; he.ApplyCalendar();
        if (he.Aging != null) he.Aging.wear = Mathf.Max(he.Aging.wear, 0.66f);
        yield return Wait("anciano", 60f); Expect("anciano");
        yield return Wait("vejez", 60f); Expect("vejez");
        TestProbe.Check($"{_p.displayName}: nadie muere de otra cosa", _p.deathsOther == 0, $"{_p.deathsOther} muerte(s)");

        Restore(oldScale, oldYear);
        TestProbe.End();
    }

    void Restore(float scale, float year)
    {
        Time.timeScale = scale;
        TimeController.timeController.DaysPerYear = year;
        if (_p != null) _p.ApplyCalendar();
    }

    static bool Body(PersonLife p) => p.GetComponent<BodyMetabolism>() != null && p.GetComponent<NeedsDriver>() != null && p.GetComponent<BodyAging>() != null && p.GetComponent<Anima>() != null;

    static void Set(PersonLife p, float hunger = -1f, float toilet = -1f, float dirt = -1f)
    {
        if (p == null || !p.TryGetComponent(out BodyMetabolism b)) return;
        if (hunger >= 0f) b.hunger = hunger;
        if (toilet >= 0f) b.toilet = toilet;
        if (dirt >= 0f) b.dirt = dirt;
    }

    static void Sleepy(PersonLife p, float s) { if (p != null && p.TryGetComponent(out Anima a)) a.sleepiness = s; }

    string Census()
    {
        var sb = new System.Text.StringBuilder();
        foreach (PersonLife p in _p.Living())
        {
            ActionQueue q = p.GetComponent<ActionQueue>();
            sb.Append($"{p.name}({(p.female ? "♀" : "♂")},{p.Stage}{(p.partner != null ? ",en pareja" : "")}{(p.pregnant ? ",embarazada" : "")}");
            if (q != null && q.Own.Count > 0) sb.Append($",«{q.Own[0].Label}»");
            sb.Append(") ");
        }
        return sb.ToString();
    }

    IEnumerator Wait(string key, float realSeconds)
    {
        CicloHumanoPanel.Row r = _p.Get(key);
        if (r == null) yield break;
        float t0 = Time.realtimeSinceStartup;
        while (!r.seen && Time.realtimeSinceStartup - t0 < realSeconds) yield return null;
    }

    void Expect(string key)
    {
        CicloHumanoPanel.Row r = _p.Get(key);
        if (r == null) return;
        TestProbe.Check($"{_p.displayName}: {r.label}", r.seen, r.seen ? $"{r.who}, a los {r.at / 60f:0.0} min de juego" : $"no se vio · {Census()}");
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Ciclo de vida de una persona (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloCicloHumano");
        CicloHumanoTest t = go.AddComponent<CicloHumanoTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(CicloHumanoTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
