using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PRUEBA DE LOS MANDOS TÁCTILES TIPO SWITCH Y EL GRIMORIO (grupo 141; docs/mandos-tactiles.md, Berón 2026-11-19: «recrear los mandos de la Nintendo
/// Switch sobre la pantalla… para mobile»). Lógica pura (no carga escenas ni mueve a nadie), va en la lista rápida:
///  · el mando tiene los 16 botones de un par de Joy-Con, 8 por lado, cada uno con su tecla y su nombre;
///  · la asignación es la de CodeShip (A Saltar, B Correr, + Misiones, − Áreas, captura Vista, inicio Menú);
///  · tocar, mantener y el interruptor (Correr) llegan al juego por GameKeys como el teclado; los diálogos los ven aunque el teclado del
///    juego esté mudo (UiKeyDown);
///  · el dibujo cabe y no se pisa en un móvil horizontal y vertical, una tableta 4:3 y un monitor;
///  · el tema de los menús (CsTheme) genera sus texturas: redondas por fuera, transparentes en las esquinas.
/// </summary>
public class JoyConTest : MonoBehaviour, ITestUnit
{
    public int Group => 141;
    public bool ParallelSafe => false;   // toca el estado estático de GameKeys

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<JoyConTest>() == null) tr.gameObject.AddComponent<JoyConTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Mandos táctiles tipo Switch y tema de los menús");
        Map();
        Fit();
        Theme();
        yield return Keys();
        yield return GrimorioChecks();
        TestProbe.End();
    }

    static void Map()
    {
        var seen = new HashSet<JoyConMap.Pad>();
        string bad = "";
        foreach (JoyConMap.Button b in JoyConMap.Default)
        {
            if (!seen.Add(b.pad)) bad += $"{b.pad} repetido; ";
            if (b.key == KeyCode.None) bad += $"{b.pad} sin tecla; ";
            bool arrow = b.pad == JoyConMap.Pad.Up || b.pad == JoyConMap.Pad.Down || b.pad == JoyConMap.Pad.Left || b.pad == JoyConMap.Pad.Right;
            if (!arrow && string.IsNullOrEmpty(b.label)) bad += $"{b.pad} sin nombre; ";
        }
        TestProbe.Check("[UI-02] el mando tiene los 16 botones de la Switch, 8 por lado, cada uno con tecla y nombre",
            seen.Count == 16 && JoyConMap.LeftSide.Length == 8 && JoyConMap.RightSide.Length == 8 && bad.Length == 0, bad.Length > 0 ? bad : $"{seen.Count} botones");

        bool codeship = JoyConMap.Get(JoyConMap.Pad.A).label == "Saltar" && JoyConMap.Get(JoyConMap.Pad.A).key == KeyCode.Space
            && JoyConMap.Get(JoyConMap.Pad.B).label == "Correr" && JoyConMap.Get(JoyConMap.Pad.B).mode == JoyConMap.Mode.Toggle
            && JoyConMap.Get(JoyConMap.Pad.Plus).label == "Misiones" && JoyConMap.Get(JoyConMap.Pad.Minus).label == "Áreas"
            && JoyConMap.Get(JoyConMap.Pad.Capture).label == "Vista" && JoyConMap.Get(JoyConMap.Pad.Home).label == "Menú";
        TestProbe.Check("[UI-03] la asignación es la de CodeShip (A Saltar, B Correr, + Misiones, − Áreas, captura Vista, inicio Menú)", codeship);
    }

    static void Fit()
    {
        var screens = new[] { new Vector2(2340, 1080), new Vector2(1080, 2340), new Vector2(1024, 768), new Vector2(1920, 1080), new Vector2(360, 740) };
        string bad = "";
        foreach (Vector2 s in screens)
        {
            TouchControls.Layout L = TouchControls.Compute(s.x, s.y);
            var screen = new Rect(0f, 0f, s.x, s.y);
            var all = new List<KeyValuePair<JoyConMap.Pad, Rect>>(L.rects);
            float R = L.stickRadius;
            Rect move = new Rect(L.moveRest.x - R, L.moveRest.y - R, 2f * R, 2f * R), look = new Rect(L.lookCenter.x - R, L.lookCenter.y - R, 2f * R, 2f * R);
            for (int i = 0; i < all.Count; i++)
            {
                Rect a = all[i].Value;
                if (a.xMin < 0f || a.yMin < 0f || a.xMax > screen.width || a.yMax > screen.height) bad += $"{s.x}×{s.y}: {all[i].Key} fuera; ";
                if (a.Overlaps(move) || a.Overlaps(look)) bad += $"{s.x}×{s.y}: {all[i].Key} pisa un joystick; ";
                for (int j = i + 1; j < all.Count; j++) if (a.Overlaps(all[j].Value)) bad += $"{s.x}×{s.y}: {all[i].Key} pisa {all[j].Key}; ";
            }
            if (L.rects.Count != 16) bad += $"{s.x}×{s.y}: {L.rects.Count} botones; ";
            if (L.unit < 36f) bad += $"{s.x}×{s.y}: botón de {L.unit:0} px; ";
        }
        TestProbe.Check("[UI-04] el mando cabe y nada se pisa (móvil horizontal y vertical, tableta 4:3, monitor, móvil pequeño)", bad.Length == 0, bad);
    }

    static void Theme()
    {
        Texture2D d = CsTheme.Disc(CsTheme.PadFill, CsTheme.PadRim, 64);
        Texture2D r = CsTheme.Rounded(CsTheme.Glass, CsTheme.CyanSoft, 10);
        Texture2D p = CsTheme.Pill(CsTheme.Orange, CsTheme.OrangeLight, 32);
        bool ok = d.GetPixel(0, 0).a < 0.05f && d.GetPixel(32, 32).a > 0.5f
            && r.GetPixel(0, 0).a < 0.05f && r.GetPixel(r.width / 2, r.height / 2).a > 0.5f
            && p.width == 2 * p.height && p.GetPixel(0, 0).a < 0.05f && p.GetPixel(p.width / 2, p.height / 2).a > 0.5f;
        TestProbe.Check("[UI-05] el tema de los menús genera sus texturas (redondas, esquinas transparentes)", ok);
        Object.DestroyImmediate(d); Object.DestroyImmediate(r); Object.DestroyImmediate(p);
    }

    static IEnumerator Keys()
    {
        GameKeys.CloseDialogs();
        var go = new GameObject("JoyConTest_pad");
        TouchControls pad = go.AddComponent<TouchControls>();
        pad.enabled = false;   // sin Update: solo se pulsan sus botones a mano

        // Tocar X (Usar): la F cuenta como pulsada solo este fotograma.
        pad.Press(JoyConMap.Pad.X, true);
        bool now = GameKeys.GetKeyDown(KeyCode.F);
        yield return null;
        bool after = GameKeys.GetKeyDown(KeyCode.F);
        TestProbe.Check("[UI-06] tocar X pulsa Usar (F) un fotograma, como el teclado", now && !after);

        // Mantener A (Saltar): espacio mantenido; al soltar, GetKeyUp.
        pad.Press(JoyConMap.Pad.A, true);
        bool down = GameKeys.GetKeyDown(KeyCode.Space) && GameKeys.GetKey(KeyCode.Space);
        yield return null;
        bool still = GameKeys.GetKey(KeyCode.Space);
        pad.Press(JoyConMap.Pad.A, false);
        bool up = GameKeys.GetKeyUp(KeyCode.Space) && !GameKeys.GetKey(KeyCode.Space);
        TestProbe.Check("[UI-07] mantener A mantiene Saltar (espacio) y soltarlo lo suelta", down && still && up);

        // B (Correr) es un interruptor: se queda puesto hasta el segundo toque.
        pad.Press(JoyConMap.Pad.B, true); pad.Press(JoyConMap.Pad.B, false);
        yield return null;
        bool running = GameKeys.GetKey(KeyCode.LeftShift) && pad.IsOn(JoyConMap.Pad.B);
        pad.Press(JoyConMap.Pad.B, true); pad.Press(JoyConMap.Pad.B, false);
        bool stopped = !GameKeys.GetKey(KeyCode.LeftShift) && !pad.IsOn(JoyConMap.Pad.B);
        TestProbe.Check("[UI-08] B (Correr) se queda puesto y el segundo toque lo quita", running && stopped);

        // Con un menú que enmudece el teclado del juego, ZL (Cerrar) y la cruceta siguen llegando a los diálogos.
        yield return null;
        GameKeys.MenuOpen = true;
        pad.Press(JoyConMap.Pad.ZL, true);
        pad.Press(JoyConMap.Pad.Down, true);
        bool muted = !GameKeys.GetKeyDown(KeyCode.Escape);
        bool ui = GameKeys.UiKeyDown(KeyCode.Escape) && GameKeys.UiKeyDown(KeyCode.DownArrow);
        GameKeys.MenuOpen = false;
        TestProbe.Check("[UI-09] con un menú abierto, el juego no ve ZL pero los diálogos sí (Cerrar y la cruceta)", muted && ui);

        Object.DestroyImmediate(go);
        yield return null;
    }

    // ── EL GRIMORIO (2026-11-20): todos los hechizos, cada uno con su reto, y acertarlo lo lanza ──
    static IEnumerator GrimorioChecks()
    {
        string bad = "";
        int n = 0;
        foreach (var e in SpellConsole.Catalog)
        {
            n++;
            CodeExercise ex = SpellChallenge.For(e.name, e.template);
            string v = ex.Validate();
            if (v.Length > 0) bad += $"{e.name}: {v}; ";
            else if (!ex.IsCorrect(SpellChallenge.Answer(ex))) bad += $"{e.name}: la respuesta buena no acierta; ";
        }
        TestProbe.Check("[UI-10] cada hechizo del lanzador tiene su reto de código bien formado (ordenar si tiene parámetros, elegir si no)", bad.Length == 0 && n >= 20, bad.Length > 0 ? bad : $"{n} hechizos");
        TestProbe.Check("[UI-11] el botón R del mando abre el Grimorio (F8)", JoyConMap.Get(JoyConMap.Pad.R).key == Grimorio.ToggleKey && JoyConMap.Get(JoyConMap.Pad.R).label == "Grimorio");

        // Lanzar desde el Grimorio: fallar el reto no lanza nada; acertarlo lanza la línea (con un lanzador de prueba: nada se mueve).
        var cast = new List<string>();
        Grimorio.CastHook = l => { cast.Add(l); return "ok"; };
        GameKeys.CloseDialogs();
        Grimorio.Open();
        bool opened = Grimorio.IsOpen && GameKeys.MenuOpen;
        bool chosen = Grimorio.Choose("yoga") || Grimorio.Choose("step");
        CodeExercise cur = CodeChallengeSheet.Current;
        bool sheet = chosen && !Grimorio.IsOpen && cur != null && cur.id.StartsWith("hechizo-");
        bool wrongNoCast = false, rightCasts = false;
        if (sheet)
        {
            CodeAnswer wrong = cur.kind == CodeExercise.Kind.Choice ? CodeAnswer.Choice((cur.correct + 1) % cur.options.Length) : CodeAnswer.Order("trampa", "nombre", "cierre");
            CodeChallengeSheet.Submit(wrong);
            wrongNoCast = cast.Count == 0;
            CodeChallengeSheet.Submit(SpellChallenge.Answer(cur));
            rightCasts = cast.Count == 1 && Grimorio.LastLine == cast[0];
        }
        CodeChallengeSheet.Close();
        Grimorio.Close();
        Grimorio.CastHook = null;
        GameKeys.CloseDialogs();
        TestProbe.Check("[UI-12] en el Grimorio, elegir un hechizo abre su reto; fallar no lo lanza y acertar sí", opened && sheet && wrongNoCast && rightCasts,
            $"abierto {opened} · reto {sheet} · fallo sin lanzar {wrongNoCast} · acierto lanza {rightCasts} ({string.Join(" | ", cast)})");
        yield return null;
    }
}
