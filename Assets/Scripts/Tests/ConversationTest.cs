using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PRUEBA DE LAS CHARLAS ENTRE PERSONAS (grupo 88; docs/conversaciones.md; Berón 2026-11-04: «debería haber una escena específica
/// creada para poder probar esta lógica al detalle»). Corre en <c>Assets/Scenes/Pruebas/Prueba_Conversaciones</c>: una mesa con
/// bancos y seis personas (dos humanos, dos insectos-humanos de especies distintas y una pareja humano-insecto). Mira:
///   1. que surge una charla de cada clase (humano-humano, insecto-insecto, humano-insecto) en menos de <see cref="StartMinutes"/>
///      minutos de juego;
///   2. que hablan POR TURNOS (A, B, A, B…) y que cada frase se muestra (bocadillo con el tema; y que se pinta en pantalla);
///   3. que el vínculo sube y el estrés (la emoción) cambia al charlar;
///   4. que una necesidad urgente de uno corta la charla al momento;
///   5. que nunca hay más charlas a la vez que el tope (también con el tope bajado a 2, y que se llega a él);
///   6. que nadie se teletransporta ni salta en toda la prueba (las charlas no mueven a nadie).
/// Cada espera, acotada en tiempo real.
/// </summary>
public class ConversationTest : MonoBehaviour, ITestUnit
{
    public const string Scene = "Prueba_Conversaciones";
    public const float StartMinutes = 30f;   // minutos de juego para que surja cada clase de charla
    public int Group => 88;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (FindAnyObjectByType<ConversationSandbox>() == null) return;
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<ConversationTest>() == null) tr.gameObject.AddComponent<ConversationTest>();
    }

    readonly List<string> _jumps = new List<string>();
    int _overCap;
    bool _watch;

    public IEnumerator Run()
    {
        TestProbe.Begin("Charlas entre personas (humanos e insectos-humanos)");
        ConversationSandbox box = FindAnyObjectByType<ConversationSandbox>();
        if (box == null) { Debug.Log("[TEST] SKIP · Charlas — esta escena no es Prueba_Conversaciones"); TestProbe.End(); yield break; }
        ConversationDirector dir = ConversationDirector.Ensure();
        TestProbe.NotNull("hay director de charlas en la escena", dir);
        if (dir == null) { TestProbe.End(); yield break; }

        float oldScale = Time.timeScale;
        int oldCap = dir.maxConcurrent;
        ConversationDirector.AllowDuringTests = true;
        Time.timeScale = 2f;
        var cast = new List<Anima>();
        foreach (Anima a in box.cast) if (a != null) cast.Add(a);
        int humans = 0, insects = 0;
        foreach (Anima a in cast) { if (ConversationDirector.IsInsect(a)) insects++; else humans++; }
        TestProbe.Check("reparto: tres humanos y tres insectos-humanos, todos personas", humans == 3 && insects == 3 && cast.TrueForAll(ConversationDirector.IsPerson),
            $"{humans} humanos, {insects} insectos");
        var species = new HashSet<string>();
        foreach (Anima a in cast) if (a.TryGetComponent(out FolkLineage fl)) species.Add(fl.species);
        TestProbe.Check("los insectos-humanos son de especies distintas", species.Count >= 2, string.Join(", ", species));
        foreach (Anima a in cast) { a.stress = 0.4f; a.sleepiness = 0f; }
        dir.EndAll("prueba"); dir.ResetCooldowns();
        int recFrom = ConversationDirector.Records.Count;
        int drawn0 = ConversationDirector.BubblesDrawn;

        _watch = true;
        StartCoroutine(Watch(cast, dir));

        // ── 1. Una charla de cada clase en menos de N minutos de juego ──
        var firstAt = new Dictionary<string, float>();
        float t0 = Time.time, limit = StartMinutes * ConversationDirector.MinuteSecs;
        System.Action<Conversation, string> onEv = (c, ev) => { if (ev == "inicio" && !firstAt.ContainsKey(c.kind)) firstAt[c.kind] = (Time.time - t0) / ConversationDirector.MinuteSecs; };
        ConversationDirector.OnEvent += onEv;
        float realEnd = Time.realtimeSinceStartup + 120f;
        while (firstAt.Count < 3 && Time.time - t0 < limit && Time.realtimeSinceStartup < realEnd)
        {
            if (dir.Active.Count >= dir.maxConcurrent) dir.ResetCooldowns();
            yield return null;
        }
        foreach (string k in new[] { "humano-humano", "insecto-insecto", "humano-insecto" })
            TestProbe.Check($"surge una charla {k} en menos de {StartMinutes:0} min de juego", firstAt.ContainsKey(k),
                firstAt.TryGetValue(k, out float m) ? $"a los {m:0.0} min" : "no surgió");

        // ── 2–3. Dejar que terminen algunas por su guion ──
        realEnd = Time.realtimeSinceStartup + 90f;
        while (Finished(recFrom, false) < 3 && Time.realtimeSinceStartup < realEnd) yield return null;
        var done = new List<Conversation>();
        for (int i = recFrom; i < ConversationDirector.Records.Count; i++) if (!ConversationDirector.Records[i].interrupted) done.Add(ConversationDirector.Records[i]);
        TestProbe.Check("al menos tres charlas terminan su guion", done.Count >= 3, $"{done.Count}");

        bool turns = done.Count > 0, shown = done.Count > 0, lines = done.Count > 0, bond = done.Count > 0, mood = done.Count > 0;
        var bad = new List<string>();
        foreach (Conversation c in done)
        {
            for (int i = 0; i < c.said.Count; i++)
            {
                Anima expect = i % 2 == 0 ? c.a : c.b;
                if (c.said[i].who != expect || (i > 0 && c.said[i].who == c.said[i - 1].who)) { turns = false; bad.Add($"turno {i} de {c.topicTitle}"); }
                if (string.IsNullOrEmpty(c.said[i].text)) { shown = false; bad.Add($"frase vacía en {c.topicTitle}"); }
            }
            if (c.said.Count < 4 || c.said.Count != c.script.Count) { lines = false; bad.Add($"{c.topicTitle}: {c.said.Count}/{c.script.Count} frases"); }
            if (!(c.bondAB1 > c.bondAB0 && c.bondBA1 > c.bondBA0)) { bond = false; bad.Add($"vínculo {c.topicTitle} {c.bondAB0:0.00}→{c.bondAB1:0.00}"); }
            if (Mathf.Approximately(c.stressA1, c.stressA0) && Mathf.Approximately(c.stressB1, c.stressB0)) { mood = false; bad.Add($"estrés sin cambio en {c.topicTitle}"); }
        }
        TestProbe.Check("hablan por turnos (A, B, A, B…)", turns, string.Join(" · ", bad));
        TestProbe.Check("cada charla dice todo su guion (4–6 frases)", lines, string.Join(" · ", bad));
        TestProbe.Check("cada frase tiene texto y se muestra con su tema", shown && done.TrueForAll(c => !string.IsNullOrEmpty(c.topicTitle)));
        TestProbe.Check("los bocadillos se pintan en pantalla (OnGUI)", ConversationDirector.BubblesDrawn > drawn0, $"{ConversationDirector.BubblesDrawn - drawn0} bocadillos");
        TestProbe.Check("el vínculo sube en los dos sentidos", bond, string.Join(" · ", bad));
        TestProbe.Check("la emoción cambia (estrés de los dos)", mood, string.Join(" · ", bad));
        var topics = new HashSet<string>(); foreach (Conversation c in done) topics.Add(c.topic);
        Debug.Log($"[TEST] Charlas: temas vistos: {string.Join(", ", topics)}");

        // ── 4. Una necesidad urgente corta la charla ──
        dir.ResetCooldowns();
        Conversation victim = null;
        realEnd = Time.realtimeSinceStartup + 60f;
        while (victim == null && Time.realtimeSinceStartup < realEnd)
        {
            foreach (Conversation c in dir.Active) if (c.said.Count >= 1) { victim = c; break; }
            if (victim == null) { dir.ResetCooldowns(); yield return null; }
        }
        TestProbe.Check("hay una charla en marcha para la prueba de la necesidad", victim != null);
        if (victim != null)
        {
            box.sustainNeeds = false;
            Anima who = ConversationDirector.IsInsect(victim.a) ? victim.a : ConversationDirector.IsInsect(victim.b) ? victim.b : victim.a;
            BodyMetabolism m = who.GetComponent<BodyMetabolism>();
            bool hungerWay = m != null && ConversationDirector.IsInsect(who);   // un insecto-humano sin casa no se va a comer: solo la charla reacciona
            if (hungerWay) m.hunger = 0.95f; else who.sleepiness = 0.95f;
            float cut = Time.realtimeSinceStartup + 5f;
            while (!victim.over && Time.realtimeSinceStartup < cut) yield return null;
            TestProbe.Check($"la {(hungerWay ? "hambre" : "sueño")} urgente de «{ConversationDirector.Name(who)}» corta la charla",
                victim.over && victim.interrupted && victim.endReason != null && (victim.endReason.Contains("necesidad") || victim.endReason.Contains("dormido")),
                victim.endReason ?? "sigue hablando");
            TestProbe.Check("con la necesidad urgente no vuelve a charlar", !dir.InConversation(who));
            if (m != null) m.hunger = 0f;
            who.sleepiness = 0f;
            box.sustainNeeds = true;
            yield return new WaitForSeconds(1.5f);
        }

        // ── 5. El tope de charlas a la vez ──
        dir.maxConcurrent = 2;
        dir.EndAll("prueba"); dir.ResetCooldowns();
        int peak = 0, over = 0;
        realEnd = Time.realtimeSinceStartup + 40f;
        while (Time.realtimeSinceStartup < realEnd)
        {
            peak = Mathf.Max(peak, dir.Active.Count);
            if (dir.Active.Count > 2) over++;
            if (dir.Active.Count < 2) dir.ResetCooldowns();
            if (peak >= 2 && Time.realtimeSinceStartup > realEnd - 30f) break;
            yield return null;
        }
        TestProbe.Check("con el tope en 2 nunca hay más de 2 charlas a la vez", over == 0, $"{over} fotogramas por encima");
        TestProbe.Check("y se llega al tope (las tres parejas quieren hablar)", peak == 2, $"máximo {peak}");
        dir.maxConcurrent = oldCap;
        TestProbe.Check($"en toda la prueba nunca más charlas que el tope ({oldCap})", _overCap == 0, $"{_overCap} fotogramas por encima");

        // ── 6. Nadie se teletransporta ni salta ──
        _watch = false;
        yield return null;
        TestProbe.Check("nadie se teletransporta ni salta (> 1 m o > 0,6 m hacia arriba en un fotograma)", _jumps.Count == 0, string.Join(" · ", _jumps));

        ConversationDirector.OnEvent -= onEv;
        ConversationDirector.AllowDuringTests = false;
        Time.timeScale = oldScale;
        TestProbe.End();
    }

    static int Finished(int from, bool interrupted)
    {
        int n = 0;
        for (int i = from; i < ConversationDirector.Records.Count; i++) if (ConversationDirector.Records[i].interrupted == interrupted) n++;
        return n;
    }

    // Vigila cada fotograma: posiciones (saltos y teletransportes) y el tope de charlas.
    IEnumerator Watch(List<Anima> cast, ConversationDirector dir)
    {
        var last = new Dictionary<Anima, Vector3>();
        foreach (Anima a in cast) last[a] = a.transform.position;
        while (_watch)
        {
            yield return null;
            if (dir.Active.Count > dir.maxConcurrent) _overCap++;
            foreach (Anima a in cast)
            {
                if (a == null) continue;
                Vector3 p = a.transform.position, d = p - last[a];
                if ((d.magnitude > 1f || d.y > 0.6f) && _jumps.Count < 10) _jumps.Add($"«{ConversationDirector.Name(a)}» {d.magnitude:0.00} m (Δy {d.y:0.00})");
                last[a] = p;
            }
        }
    }
}
