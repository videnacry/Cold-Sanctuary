using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PRUEBA DE LOS MANDOS TÁCTILES TIPO SWITCH Y EL GRIMORIO (grupo 141; (PB-169) medidas del santuario 1 de CodeShip y ocultarse con los modales; docs/sistemas/mandos-tactiles.md, Berón 2026-11-19: «recrear los mandos de la Nintendo
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
        yield return HideWithModals();
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
        var screens = new[] { new Vector2(2340, 1080), new Vector2(1080, 2340), new Vector2(1024, 768), new Vector2(1920, 1080), new Vector2(360, 740), new Vector2(585, 996), new Vector2(1266, 585) };
        string bad = "";
        foreach (Vector2 s in screens)
        {
            TouchControls.Layout L = TouchControls.Compute(s.x, s.y);
            var screen = new Rect(0f, 0f, s.x, s.y);
            var all = new List<KeyValuePair<JoyConMap.Pad, Rect>>(L.rects);
            float R = L.stickRadius;
            Rect move = new Rect(L.moveCenter.x - R, L.moveCenter.y - R, 2f * R, 2f * R), look = new Rect(L.lookCenter.x - R, L.lookCenter.y - R, 2f * R, 2f * R);
            if (move.xMin < 0f || move.yMax > s.y || look.xMax > s.x || move.Overlaps(look)) bad += $"{s.x}×{s.y}: joysticks fuera o pisándose; ";
            for (int i = 0; i < all.Count; i++)
            {
                Rect a = all[i].Value;
                if (a.xMin < 0f || a.yMin < 0f || a.xMax > screen.width || a.yMax > screen.height) bad += $"{s.x}×{s.y}: {all[i].Key} fuera; ";
                if (a.Overlaps(move) || a.Overlaps(look)) bad += $"{s.x}×{s.y}: {all[i].Key} pisa un joystick; ";
                // (Como en CodeShip, la cruceta y el rombo van a 49 px con botones de 50: las cajas se tocan 1 px en la esquina, los dibujos no.)
                for (int j = i + 1; j < all.Count; j++) if (Shrunk(a).Overlaps(Shrunk(all[j].Value))) bad += $"{s.x}×{s.y}: {all[i].Key} pisa {all[j].Key}; ";
            }
            if (L.rects.Count != 16) bad += $"{s.x}×{s.y}: {L.rects.Count} botones; ";
            if (L.rects[JoyConMap.Pad.Plus].height < 44f * L.dp - 0.01f) bad += $"{s.x}×{s.y}: «+» de menos de 44 px CSS; ";
        }
        TestProbe.Check("[UI-04] el mando cabe y nada se pisa (móvil horizontal y vertical, tableta 4:3, monitor, móvil pequeño)", bad.Length == 0, bad);

        // (PB-169) Como en el santuario 1 de CodeShip, medido en vivo (Playwright, iPhone 13 en vertical, 390×664 px CSS).
        var codeship = new Dictionary<JoyConMap.Pad, Rect>
        {
            { JoyConMap.Pad.Up, new Rect(63, 356, 50, 50) }, { JoyConMap.Pad.Down, new Rect(63, 454, 50, 50) }, { JoyConMap.Pad.Left, new Rect(14, 405, 50, 50) }, { JoyConMap.Pad.Right, new Rect(112, 405, 50, 50) },
            { JoyConMap.Pad.L, new Rect(14, 306, 64, 44) }, { JoyConMap.Pad.ZL, new Rect(14, 256, 64, 44) }, { JoyConMap.Pad.Minus, new Rect(118, 306, 44, 44) }, { JoyConMap.Pad.Capture, new Rect(118, 256, 44, 44) },
            { JoyConMap.Pad.X, new Rect(277, 356, 50, 50) }, { JoyConMap.Pad.A, new Rect(326, 405, 50, 50) }, { JoyConMap.Pad.B, new Rect(277, 454, 50, 50) }, { JoyConMap.Pad.Y, new Rect(228, 405, 50, 50) },
            { JoyConMap.Pad.R, new Rect(312, 306, 64, 44) }, { JoyConMap.Pad.ZR, new Rect(312, 256, 64, 44) }, { JoyConMap.Pad.Plus, new Rect(228, 306, 44, 44) }, { JoyConMap.Pad.Home, new Rect(228, 256, 44, 44) },
        };
        string off = "";
        TouchControls.Layout C = TouchControls.Compute(390f, 664f);
        foreach (var kv in codeship)
        {
            Rect m = C.rects[kv.Key], c = kv.Value;
            if (Mathf.Abs(m.x - c.x) > 1f || Mathf.Abs(m.y - c.y) > 1f || Mathf.Abs(m.width - c.width) > 0.5f || Mathf.Abs(m.height - c.height) > 0.5f) off += $"{kv.Key} en {m} y no en {c}; ";
        }
        // Y al doble de densidad, el mismo dibujo al doble (la misma parte de la pantalla): un botón del rombo, 50/390 del ancho.
        TouchControls.Layout D = TouchControls.Compute(780f, 1328f);
        if (Mathf.Abs(D.rects[JoyConMap.Pad.A].width / 780f - 50f / 390f) > 0.002f) off += $"a 780×1328 el botón A mide {D.rects[JoyConMap.Pad.A].width:0} px; ";
        if (Mathf.Abs(C.stickRadius - 60f) > 0.01f || Mathf.Abs(C.moveCenter.x - 82f) > 0.5f || Mathf.Abs(C.moveCenter.y - 578f) > 0.5f) off += $"joystick de andar en {C.moveCenter} (r {C.stickRadius}); ";
        // (revisión PB-169) El botón de pausa del móvil no cae sobre ningún botón ni joystick del mando, en vertical ni en horizontal.
        foreach (Vector2 scr in new[] { new Vector2(390f, 664f), new Vector2(750f, 342f), new Vector2(1266f, 585f) })
        {
            TouchControls.Layout P = TouchControls.Compute(scr.x, scr.y);
            Rect pr = SceneLauncher.MobilePauseRect(scr.x, scr.y);
            foreach (var kv in P.rects) if (pr.Overlaps(kv.Value)) off += $"a {scr.x}×{scr.y} la pausa pisa {kv.Key}; ";
            foreach (Vector2 c in new[] { P.moveCenter, P.lookCenter })
                if (new Rect(c.x - P.stickRadius, c.y - P.stickRadius, 2f * P.stickRadius, 2f * P.stickRadius).Overlaps(pr)) off += $"a {scr.x}×{scr.y} la pausa pisa un joystick; ";
            if (pr.y < 0f || pr.xMax > scr.x) off += $"a {scr.x}×{scr.y} la pausa se sale ({pr}); ";
        }
        TestProbe.Check("[UI-13] el mando mide y se coloca como en el santuario 1 de CodeShip (iPhone 13), y ocupa la misma parte de cualquier pantalla", off.Length == 0, off);

        // La hoja (retos, Grimorio, ajustes): abajo desde el 38 % en vertical (el reto, desde el 30 % en un móvil bajo); a la derecha y 46 % en horizontal.
        Rect sv = CsUi.SheetRectFor(390f, 664f, 1f), sr = CsUi.SheetRectFor(390f, 664f, 1f, true), sh = CsUi.SheetRectFor(750f, 342f, 1f), sw = CsUi.SheetRectFor(1920f, 1080f, 1f);
        bool sheet = Mathf.Abs(sv.y - 252.32f) < 0.5f && sv.width == 390f && Mathf.Abs(sr.y - 199.2f) < 0.5f
            && Mathf.Abs(sh.width - 345f) < 0.5f && Mathf.Abs(sh.x - 405f) < 0.5f && sh.height == 342f && Mathf.Abs(sw.width - 480f) < 0.5f;
        TestProbe.Check("[UI-15] las hojas van donde en CodeShip (vertical abajo, horizontal a la derecha)", sheet, $"{sv} · {sr} · {sh} · {sw}");
    }

    static Rect Shrunk(Rect r) { float k = r.height * 0.05f; return new Rect(r.x + k, r.y + k, r.width - 2f * k, r.height - 2f * k); }

    static void Theme()
    {
        Texture2D d = CsTheme.Shape(64, 64, 32f, CsTheme.Radial(0.4f, 0.35f, CsTheme.Cyan, CsTheme.Glass, 0.7f), CsTheme.OrangeLight, 2f);
        Texture2D r = CsTheme.Rounded(CsTheme.Glass, CsTheme.CyanSoft, 10);
        Texture2D p = CsTheme.Shape(64, 32, 16f, (u, v) => CsTheme.Orange, CsTheme.OrangeLight, 2f);
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

    // ── (PB-169) El mando se oculta con cualquier modal y vuelve al cerrarlo ──
    static IEnumerator HideWithModals()
    {
        GameKeys.CloseDialogs();
        HudModals.Close();
        var go = new GameObject("JoyConTest_hide");
        go.AddComponent<TouchControls>();
        yield return null; yield return null; yield return null;
        bool shownFirst = TouchControls.Showing;
        string missed = "";
        HudModals.Current = HudModals.Modal.Misiones;
        yield return null;
        if (TouchControls.Showing) missed += "Misiones; ";
        HudModals.Close();
        Grimorio.Open();
        yield return null;
        if (TouchControls.Showing) missed += "Grimorio; ";
        Grimorio.Close();
        ChoiceDialog.Show("prueba", new[] { "a", "b" }, _ => { });
        yield return null;
        if (TouchControls.Showing) missed += "diálogo de opciones; ";
        ChoiceDialog.Close();
        MicroRootMenu.Toggle(true);   // (revisión PB-169) el desplegable ☰ crece hacia abajo, sobre los botones
        yield return null;
        if (TouchControls.Showing) missed += "menú ☰; ";
        MicroRootMenu.Toggle(false);
        yield return null; yield return null; yield return null;
        bool back = TouchControls.Showing;
        Object.Destroy(go);
        yield return null;
        TestProbe.Check("[UI-14] el mando se oculta con un modal abierto (Misiones, Grimorio, diálogo, menú ☰) y vuelve al cerrarlo", shownFirst && missed.Length == 0 && back,
            $"antes {shownFirst} · no se ocultó con: {missed} · después {back} · abiertos: {string.Join(", ", CsUi.OpenModals)}");
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
