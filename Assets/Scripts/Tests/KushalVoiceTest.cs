using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE LA VOZ DE KUSHAL (<see cref="KushalVoice"/>, Berón 2026-10-29): cuando otro personaje le habla, Kushal contesta,
/// con humildad y aceptando ayudar, en su propio panel ARRIBA, sin tapar la frase que le llega (abajo).
/// Comprueba: (1) una frase dirigida a Kushal (<see cref="KushalVoice.Hear"/>: subtítulo abajo + respuesta) hace aparecer su
/// respuesta; (2) la respuesta es del repertorio humilde de ese contexto; (3) el rectángulo de su panel queda ENCIMA del panel
/// entrante (<see cref="StorySubtitle"/>) sin solaparse — medido en el OnGUI real si se dibuja y, si no (sin vista de juego),
/// con la maquetación pura para varias resoluciones; (4) un pensamiento sale en cursiva y entre paréntesis; (5) el repertorio
/// no repite la misma frase dos veces seguidas. Solo toca la UI (no mueve a nadie). Cada espera, acotada.
/// Se añade solo al orquestador cuando se piden las pruebas (como <see cref="KitchenOriginStoryTest"/>).
/// </summary>
public class KushalVoiceTest : MonoBehaviour, ITestUnit
{
    public int Group => 52;   // tras la clase de yoga (51): solo UI, no pisa a nadie
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<KushalVoiceTest>() == null) tr.gameObject.AddComponent<KushalVoiceTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("La voz de Kushal: contesta, humilde, arriba y sin tapar lo que le dicen");
        KushalVoice.Clear(); StorySubtitle.Clear();

        // ── 1–2. Le hablan: contesta, del repertorio humilde ──
        int before = KushalVoice.Spoken;
        KushalVoice.Hear("Ambrosio (prueba)", "¡Eh, tú! ¡Ayúdanos, por favor! Salva a mi familia.", KushalVoice.Ctx.Help, 8f, 0.3f);
        yield return UntilReal(() => KushalVoice.Spoken > before && KushalVoice.Visible, 5f);
        TestProbe.Check("una frase dirigida a Kushal hace aparecer su respuesta", KushalVoice.Spoken > before && KushalVoice.Visible,
            KushalVoice.LastSpoken ?? "sin respuesta");
        TestProbe.Check("la respuesta es humilde y acepta ayudar (repertorio de «ayuda»)", KushalVoice.IsHumble(KushalVoice.LastSpoken, KushalVoice.Ctx.Help),
            $"«{KushalVoice.LastSpoken}»");

        // ── 3. Su panel, encima del entrante y sin solaparse ──
        yield return UntilReal(() => Time.frameCount - KushalVoice.LastDrawFrame <= 1 && Time.frameCount - StorySubtitle.LastDrawFrame <= 1, 2f);
        bool drawn = Time.frameCount - KushalVoice.LastDrawFrame <= 2 && Time.frameCount - StorySubtitle.LastDrawFrame <= 2;
        if (drawn)
        {
            Rect k = KushalVoice.LastRect, s = StorySubtitle.LastRect;
            string d = $"Kushal y {k.yMin:0}–{k.yMax:0} · entrante y {s.yMin:0}–{s.yMax:0} (pantalla {Screen.width}×{Screen.height})";
            TestProbe.Check("el panel de Kushal está arriba (en la mitad superior)", k.center.y < Screen.height * 0.5f, d);
            TestProbe.Check("el panel de Kushal queda encima del entrante, sin solaparse", k.yMax <= s.yMin && !k.Overlaps(s), d);
            if (Time.frameCount - LessonPrompt.LastDrawFrame <= 2)
                TestProbe.Check("…ni con el aviso de lección", !k.Overlaps(LessonPrompt.LastRect), $"aviso y {LessonPrompt.LastRect.yMin:0}");
        }
        else
        {
            // Sin OnGUI (sin vista de juego): la maquetación pura, con el entrante más alto posible (pila de 3 subtítulos).
            bool ok = true; string why = "";
            foreach (Vector2 scr in new[] { new Vector2(1920, 1080), new Vector2(1280, 720), new Vector2(1024, 600), new Vector2(800, 600) })
            {
                float incomingTop = scr.y - 230f - 3f * 66f;
                Rect k = KushalVoice.Layout(scr.x, scr.y, 2f * 44f + 6f, incomingTop);
                var s = new Rect(scr.x * 0.1f, incomingTop, scr.x * 0.8f, scr.y - 230f - incomingTop);
                bool good = k.height > 0f && k.yMax <= s.yMin && !k.Overlaps(s) && k.center.y < scr.y * 0.5f;
                if (!good) { ok = false; why += $"{scr.x}×{scr.y}: Kushal {k.yMin:0}–{k.yMax:0}, entrante {s.yMin:0}; "; }
            }
            TestProbe.Check("(maquetación) el panel de Kushal queda arriba y encima del entrante, sin solaparse", ok, why == "" ? "4 resoluciones" : why);
        }

        // ── 4. Un pensamiento: cursiva y entre paréntesis ──
        string thought = KushalVoice.Think(KushalVoice.Thought.EnterMicrocosmos, null, 0.1f);
        yield return UntilReal(() => KushalVoice.LastThought == thought, 3f);
        string shown = KushalVoice.Format(thought, true);
        TestProbe.Check("un pensamiento sale en cursiva y entre paréntesis", KushalVoice.LastThought == thought && shown.Contains("<i>") && shown.Contains("(" + thought + ")"), shown);

        // ── 5. Sin repetir la misma frase dos veces seguidas, y todo el repertorio, humilde ──
        bool noRepeat = true; string prev = null;
        for (int i = 0; i < 40; i++) { string p = KushalVoice.Pick(KushalVoice.Ctx.Instruction); if (p == prev) noRepeat = false; prev = p; }
        TestProbe.Check("el repertorio no repite la misma frase dos veces seguidas", noRepeat);
        bool allHumble = true;
        foreach (KushalVoice.Ctx c in System.Enum.GetValues(typeof(KushalVoice.Ctx)))
            foreach (string l in KushalVoice.Pool(c)) if (!KushalVoice.IsHumble(l, c) || string.IsNullOrEmpty(l)) allHumble = false;
        TestProbe.Check("cada contexto tiene su repertorio", allHumble);

        KushalVoice.Clear(); StorySubtitle.Clear();
        TestProbe.End();
    }

    static IEnumerator UntilReal(System.Func<bool> cond, float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (!cond() && Time.realtimeSinceStartup < end) yield return null;
    }
}
