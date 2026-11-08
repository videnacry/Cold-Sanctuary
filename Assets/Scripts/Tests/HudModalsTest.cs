using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA PILA DE BOTONES Y SUS VENTANAS (grupo 93; docs/interfaz-botones.md, Berón 2026-11-05):
///  · la pila va a la derecha y encima del botón de F10, sin pisarlo ni pisarse, y lejos del lanzador de la izquierda;
///  · las teclas no chocan con F1 ni con F10;
///  · las ventanas son de una en una; Esc, el botón y su tecla (<see cref="HudModals.Toggle"/>) las cierran;
///  · el horario sigue abriéndose por <see cref="HorarioPanel"/>, como lo usan sus pruebas y la consola;
///  · están sincronizadas: los recursos y la jornada salen de sus fuentes, en el momento.
/// </summary>
public class HudModalsTest : MonoBehaviour, ITestUnit
{
    public int Group => 93;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<HudModalsTest>() == null) tr.gameObject.AddComponent<HudModalsTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Interfaz: la pila de botones y sus ventanas");

        var pause = new Rect(Screen.width - 190f, Screen.height - 72f, 185f, 24f);   // el botón de F10 (SceneLauncher)
        Rect[] s = { HudModals.Slot(0), HudModals.Slot(1), HudModals.Slot(2) };
        TestProbe.Check("la pila, a la derecha y encima del botón de F10, con su ancho", s.All(r => r.x == pause.x && r.width == pause.width && r.yMax <= pause.y), $"{s[0]} · F10 {pause}");
        TestProbe.Check("los botones no se pisan", !s[0].Overlaps(s[1]) && !s[1].Overlaps(s[2]) && !s[0].Overlaps(pause));
        TestProbe.Check("nada a la izquierda: el lanzador de hechizos queda libre", s.All(r => r.x > Screen.width * 0.5f), $"{s[0].x} de {Screen.width}");
        KeyCode[] keys = { HudModals.HorarioKey, HudModals.RecursosKey, HudModals.MisionesKey };
        TestProbe.Check("las teclas son distintas y no chocan con F1 ni con F10", keys.Distinct().Count() == 3 && !keys.Contains(HudGate.ToggleKey) && !keys.Contains(SceneLauncher.PauseKey), string.Join(", ", keys));

        // Esc cede a un diálogo abierto (es lo que se quiere): sin ninguno, para probar la pila. Uno que dejó otra prueba se cierra y se dice cuál.
        string left = GameKeys.OpenDialogs;
        if (GameKeys.DialogOpen) { GameKeys.CloseDialogs(); float w0 = Time.realtimeSinceStartup; while (GameKeys.DialogOpen && Time.realtimeSinceStartup - w0 < 2f) yield return null; }
        if (left.Length > 0) Debug.Log($"[TEST] (antes de la pila de ventanas había abierto: {left}; cerrado)");
        HudModals.Close();
        HudModals.Toggle(HudModals.Modal.Horario);
        TestProbe.Check("el horario se abre en la pila y HorarioPanel lo ve", HorarioPanel.Show && HudModals.Current == HudModals.Modal.Horario);
        HudModals.Toggle(HudModals.Modal.Recursos);
        TestProbe.Check("abrir otra ventana cierra la anterior", !HorarioPanel.Show && HudModals.Current == HudModals.Modal.Recursos);
        HudModals.Toggle(HudModals.Modal.Recursos);
        TestProbe.Check("su botón o su tecla la cierra", HudModals.Current == HudModals.Modal.None);
        HorarioPanel.Show = true;
        TestProbe.Check("HorarioPanel.Show abre la ventana (sus pruebas y la consola)", HudModals.Current == HudModals.Modal.Horario);
        TestProbe.Check("Esc la cierra", HudModals.Escape() && HudModals.Current == HudModals.Modal.None && !HorarioPanel.Show);
        TestProbe.Check("Esc sin ventana abierta no hace nada", !HudModals.Escape());

        // ── Sincronizadas con sus fuentes ──
        if (SanctuaryResourceHUD.Instance != null && SanctuaryResources.HasInstance)
        {
            SanctuaryResources res = SanctuaryResources.Instance;
            float before = res.Get(SanctuaryResource.Food);
            res.Add(SanctuaryResource.Food, 137f);
            string text = SanctuaryResourceHUD.Instance.Text();
            TestProbe.Check("la ventana de recursos enseña el libro de recursos en el momento", text.Contains($"{SanctuaryResource.Food}: {Mathf.FloorToInt(before + 137f)}"), text.Split('\n')[0]);
            res.TrySpend(SanctuaryResource.Food, 137f);
            TestProbe.Check("en esta escena sale el botón de recursos", HudModals.Available(HudModals.Modal.Recursos));
        }
        else Debug.Log("[TEST] Esta escena no tiene recursos del santuario: se salta esa parte.");

        var lines = HudModals.MisionesLines();
        TestProbe.Check("la ventana de misiones tiene sus dos apartados", lines.Exists(l => l.Contains("Misiones aceptadas")) && lines.Exists(l => l.Contains("Jornada de hoy")));
        WorkdayDirector wd = WorkdayDirector.Instance;
        var day = wd != null ? wd.Itinerary() : null;
        if (day != null) TestProbe.Check("la jornada de la ventana es la del recuadro de la jornada", day.All(d => lines.Exists(l => l.Contains(d.Length > 12 ? d.Substring(d.IndexOf(' ') + 1) : d) || d.StartsWith("Jornada") || d.StartsWith("Ahora"))), string.Join(" | ", day));
        yield return null;
        TestProbe.End();
    }
}
