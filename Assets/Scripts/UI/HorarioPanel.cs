using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PANEL DEL HORARIO LIBRE (docs/horario-libre.md): la semana de ocio y cuidado de cada personaje de la escena, como el panel de
/// Progreso (centrado, con su rueda) y el itinerario de la jornada. Filas: los personajes; columnas: las franjas libres del día
/// (16, 17, 18, 20 y 21 h); la franja de ahora, resaltada, con lo que hace cada uno (✓ ya la hace, → va hacia allí, ‖ espera a
/// una necesidad). Debajo, el reparto de la semana (uniformidad: diferencia entre quien más y quien menos hizo cada actividad) y el
/// catálogo con sus plazas. Se abre con el botón «Horario libre» (abajo a la izquierda) o con la consola: <c>horario</c>.
/// </summary>
public class HorarioPanel : MonoBehaviour
{
    public static bool Show;
    static readonly string[] DayNames = { "lunes", "martes", "miércoles", "jueves", "viernes", "sábado", "domingo" };
    int _dayOffset = -1, _groupIdx;
    Vector2 _scroll;
    GUIStyle _st, _small, _cell;

    /// <summary>Abre o cierra el panel (consola <c>horario</c>).</summary>
    public static string Toggle()
    {
        Show = !Show;
        if (LeisureDirector.Instance != null) LeisureDirector.Instance.Scan(true);
        return Show ? "Horario libre: abierto (vuelve a escribir «horario» para cerrarlo)." : "Horario libre: cerrado.";
    }

    /// <summary>Nombre del día de la semana (el día 0 es lunes).</summary>
    public static string DayName(int day) => DayNames[((day % 7) + 7) % 7];

    void OnGUI()
    {
        LeisureDirector ld = LeisureDirector.Instance;
        if (ld == null) return;
        if (_st == null)
        {
            _st = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true, wordWrap = true };
            _small = new GUIStyle(GUI.skin.label) { fontSize = 11, richText = true, wordWrap = true };
            _cell = new GUIStyle(GUI.skin.label) { fontSize = 11, richText = true, wordWrap = true, alignment = TextAnchor.UpperLeft };
        }
        if (!Show)
        {
            if (!ColdSanctuary.TestRunner.Running && ld.groups.Count > 0 && GUI.Button(new Rect(10f, Screen.height - 34f, 120f, 24f), "Horario libre")) { Show = true; ld.Scan(true); }
            return;
        }
        if (ld.groups.Count == 0) ld.Scan(true);
        if (ld.groups.Count == 0) return;
        _groupIdx = Mathf.Clamp(_groupIdx, 0, ld.groups.Count - 1);
        LeisureDirector.Group gr = ld.groups[_groupIdx];
        if (gr.plan == null) return;
        TimeController tc = TimeController.timeController;
        int today = tc.Day;
        if (_dayOffset < 0 || _dayOffset >= LeisureRoster.DaysPerWeek) _dayOffset = Mathf.Clamp(today - gr.plan.firstDay, 0, LeisureRoster.DaysPerWeek - 1);
        int day = gr.plan.firstDay + _dayOffset;
        int nowSlot = day == today ? LeisureRoster.SlotAt(tc.Hour) : -1;

        int slots = LeisureRoster.SlotHours.Length;
        float nameW = 150f, cellW = Mathf.Clamp((Screen.width - 80f - nameW) / slots, 110f, 170f);
        float w = nameW + cellW * slots + 30f, rowH = 34f;
        float h = Mathf.Min(Screen.height - 80f, 150f + gr.plan.people.Count * rowH + 40f + gr.catalog.Count * 16f);
        var r = new Rect((Screen.width - w) * 0.5f, 50f, w, h);
        GUI.Box(r, ""); GUI.Box(r, "");
        int hh = Mathf.FloorToInt(tc.Hour), mm = Mathf.FloorToInt((tc.Hour - hh) * 60f);
        GUI.Label(new Rect(r.x + 12f, r.y + 6f, w - 120f, 22f), $"<b><size=15>✦ Horario libre · {(gr.micro ? "microcosmos" : "santuario")} «{gr.name}»</size></b>  <size=11>semana {gr.plan.week} · hoy {DayName(today)} (día {today}) {hh:00}:{mm:00}</size>", _st);
        if (GUI.Button(new Rect(r.xMax - 80f, r.y + 6f, 68f, 22f), "cerrar")) Show = false;
        // Día de la semana y escena.
        float y = r.y + 32f;
        if (GUI.Button(new Rect(r.x + 12f, y, 26f, 22f), "◀")) _dayOffset = (_dayOffset + LeisureRoster.DaysPerWeek - 1) % LeisureRoster.DaysPerWeek;
        GUI.Label(new Rect(r.x + 42f, y + 2f, 190f, 22f), $"<b>{DayName(day)}</b> · día {day}{(day == today ? " (hoy)" : "")}", _st);
        if (GUI.Button(new Rect(r.x + 236f, y, 26f, 22f), "▶")) _dayOffset = (_dayOffset + 1) % LeisureRoster.DaysPerWeek;
        if (ld.groups.Count > 1 && GUI.Button(new Rect(r.x + 280f, y, 150f, 22f), "otra escena")) { _groupIdx = (_groupIdx + 1) % ld.groups.Count; _dayOffset = -1; }
        GUI.Label(new Rect(r.x + 440f, y + 3f, w - 450f, 22f), "<color=#9AA0A6>8–16 jornada · 19–20 cena · 22–7 sueño · el resto, libre</color>", _small);
        y += 28f;
        // Cabecera de las franjas.
        GUI.Label(new Rect(r.x + 12f, y, nameW, 20f), "<b>Personaje</b>", _st);
        for (int s = 0; s < slots; s++)
        {
            string hd = $"{LeisureRoster.SlotHours[s]:00}:00–{LeisureRoster.SlotHours[s] + 1f:00}:00";
            GUI.Label(new Rect(r.x + 12f + nameW + s * cellW, y, cellW, 20f), s == nowSlot ? $"<b><color=#FFD27A>▸ {hd}</color></b>" : $"<b>{hd}</b>", _st);
        }
        y += 22f;
        var view = new Rect(0f, 0f, w - 40f, gr.plan.people.Count * rowH + 24f + gr.catalog.Count * 16f + 40f);
        _scroll = GUI.BeginScrollView(new Rect(r.x + 8f, y, w - 16f, r.yMax - y - 8f), _scroll, view);
        float ry = 0f;
        foreach (LeisureRoster.Person p in gr.plan.people)
        {
            GameObject body = gr.BodyOf(p);
            GUI.Label(new Rect(4f, ry, nameW, rowH), $"<b>{p.name}</b>", _st);
            for (int s = 0; s < slots; s++)
            {
                LeisureRoster.Entry e = gr.plan.At(p, day, s);
                string txt = e != null ? e.activity.title : "<color=#E88>— sin actividad</color>";
                if (s == nowSlot && e != null && body != null)
                {
                    LeisureDirector.Run run = ld.RunOf(body);
                    string mark = run == null ? "" : run.blockedBy != null ? $"<color=#E9C46A>‖ {run.blockedBy}</color> " : run.Arrived ? "<color=#7CFC8A>✓</color> " : "<color=#8EC9FF>→</color> ";
                    txt = $"<b>{mark}{e.activity.title}</b>";
                }
                GUI.Label(new Rect(4f + nameW + s * cellW, ry, cellW - 6f, rowH), txt, _cell);
            }
            ry += rowH;
        }
        ry += 6f;
        var over = LeisureRoster.OverCapacity(gr.plan);
        GUI.Label(new Rect(4f, ry, view.width, 20f), $"<b>Reparto de la semana</b> · {gr.plan.entries.Count} franjas · sin llenar {gr.plan.unfilled.Count} · capacidad {(over.Count == 0 ? "<color=#7CFC8A>respetada</color>" : $"<color=#E88>superada {over.Count} vez/veces</color>")} · " +
                                             $"uniformidad: cada actividad Δ={gr.spread} <color=#9AA0A6>({gr.worst})</color> · por persona Δ={gr.spreadPerPerson}", _small);
        ry += 22f;
        foreach (LeisureRoster.Activity a in gr.catalog)
        {
            int n = gr.plan.entries.Count(e => e.activity == a);
            GUI.Label(new Rect(4f, ry, view.width, 16f), $"· {a.title} <color=#9AA0A6>— {KindName(a.kind)}, en {a.venue}, {(a.Unlimited ? "cada uno en la suya" : $"{a.capacity} plazas")} · {n} franjas esta semana</color>", _small);
            ry += 16f;
        }
        GUI.EndScrollView();
    }

    static string KindName(LeisureRoster.Kind k) => k == LeisureRoster.Kind.Ocio ? "ocio" : k == LeisureRoster.Kind.Cuidado ? "cuidado" : k == LeisureRoster.Kind.Descanso ? "descanso" : "recuperación";
}
