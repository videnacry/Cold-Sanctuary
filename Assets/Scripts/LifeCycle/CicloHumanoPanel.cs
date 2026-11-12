using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESCENAS DE CICLO DE VIDA DE UNA PERSONA (Berón 2026-11-02: «¿el ciclo de vida está probado para los humanos? El ciclo de vida
/// de los humanos y de los insectos-humanos es el mismo»). Dos escenas como las islas de los animales (<see cref="CicloVidaPanel"/>):
/// una pareja HUMANA en una casita con despensa inagotable y una pareja de INSECTOS-HUMANOS en un solar de un pueblo de su era.
/// Este panel OBSERVA (no empuja a nadie) y marca con ✓ cada paso en cuanto lo anuncia <see cref="PersonLife.OnEvent"/>: comer,
/// dormir, ir al baño y lavarse en casa; cortejo, pareja, embarazo, parto; el niño junto a su madre, comiendo y creciendo a joven
/// y a adulto; la vejez y la muerte de viejo. Las filas son LAS MISMAS en las dos escenas, y la prueba también
/// (<see cref="CicloHumanoTest"/>, grupo 59). El año se comprime como en las islas (<see cref="CicloVidaPanel.CompressedYear"/>).
/// </summary>
public class CicloHumanoPanel : MonoBehaviour
{
    public string displayName = "Pareja humana";
    [Tooltip("La escena de los insectos-humanos (si no, la de los humanos). Solo cambia el rótulo: el ciclo y la prueba son los mismos.")]
    public bool insects;

    public class Row { public string key, label; public bool seen; public float at; public string who = ""; }
    public readonly List<Row> rows = new List<Row>();
    readonly Dictionary<string, Row> _byKey = new Dictionary<string, Row>();
    public int deathsOther { get; private set; }
    bool _show = true, _compressed = true;
    float _start;
    Vector2 _scroll;
    GUIStyle _st, _small;

    public Row Get(string key) => _byKey.TryGetValue(key, out Row r) ? r : null;
    public bool Seen(string key) => _byKey.TryGetValue(key, out Row r) && r.seen;

    /// <summary>Las filas, en el orden del ciclo (las mismas para humanos e insectos-humanos).</summary>
    public static readonly (string key, string label)[] Steps =
    {
        ("comer", "Come en casa (de la despensa, sentado a la mesa)"),
        ("bano", "Va al baño (el retrete de casa)"),
        ("lavarse", "Se lava (el lavabo de casa)"),
        ("dormir", "Duerme en su cama"),
        ("cortejo", "Cortejo: una mujer y un hombre adultos que comparten casa"),
        ("pareja", "Forman pareja"),
        ("embarazo", "Ella se queda embarazada (de noche, juntos en casa)"),
        ("parto", "Nace un hijo (a los 9 meses del calendario)"),
        ("junto", "El niño va con su madre"),
        ("hijo_come", "El niño come en casa"),
        ("joven", "El niño crece a joven"),
        ("adulto", "El joven crece a adulto"),
        ("anciano", "Llega a la vejez"),
        ("vejez", "Muere de viejo"),
    };

    void Awake()
    {
        foreach (var (k, l) in Steps) { var r = new Row { key = k, label = l }; rows.Add(r); _byKey[k] = r; }
    }

    void OnEnable() => PersonLife.OnEvent += OnLife;
    void OnDisable() => PersonLife.OnEvent -= OnLife;

    void Start() { _start = Time.time; ApplyCalendar(); }

    /// <summary>El año comprimido (o el natural) en el reloj.</summary>
    public void ApplyCalendar()
    {
        TimeController tc = TimeController.timeController;
        if (tc != null) tc.DaysPerYear = _compressed ? CicloVidaPanel.CompressedYear : CicloVidaPanel.NaturalYear;
    }

    void OnLife(GameObject who, string key)
    {
        if (who == null || who.scene != gameObject.scene) return;
        PersonLife pl = who.GetComponent<PersonLife>();
        bool kid = pl != null && pl.mother != null;
        if (key == "muerte") { deathsOther++; return; }
        if (key == "comer" && kid) Mark("hijo_come", who);
        if ((key == "joven" || key == "adulto") && !kid) return;   // solo el crecimiento del hijo que nace aquí
        Mark(key, who);
    }

    void Mark(string key, GameObject who)
    {
        if (!_byKey.TryGetValue(key, out Row r) || r.seen) return;
        r.seen = true; r.at = Time.time; r.who = who.name;
        Debug.Log($"[CicloHumano] ✓ {r.label} ({who.name}).");
    }

    /// <summary>Las personas vivas de esta escena.</summary>
    public List<PersonLife> Living()
    {
        var l = new List<PersonLife>();
        foreach (PersonLife p in FindObjectsByType<PersonLife>(FindObjectsSortMode.None))
            if (p != null && !p.Dead && p.gameObject.scene == gameObject.scene) l.Add(p);
        return l;
    }

    static string Stage(PersonLife p)
    {
        char s = p.Stage;
        string st = s == LifeStage.child ? "niño/a" : s == LifeStage.teen ? "joven" : s == LifeStage.adult ? "adulto/a" : "muerto/a";
        return p.Aging != null && p.Aging.IsElder ? "anciano/a" : st;
    }

    void OnGUI()
    {
        if (ColdSanctuary.TestRunner.Running && !_show) return;
        if (_st == null)
        {
            _st = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, wordWrap = true };
            _small = new GUIStyle(GUI.skin.label) { fontSize = 12, richText = true, wordWrap = true };
        }
        if (!_show) { if (GUI.Button(new Rect(10f, 10f, 190f, 26f), "Ciclo de vida (mostrar)")) _show = true; return; }
        float w = 450f, h = Mathf.Min(Screen.height - 20f, 780f);
        GUI.Box(new Rect(10f, 10f, w, h), GUIContent.none);
        GUI.Box(new Rect(10f, 10f, w, h), GUIContent.none);
        GUILayout.BeginArea(new Rect(20f, 16f, w - 20f, h - 12f));
        GUILayout.BeginHorizontal();
        GUILayout.Label($"<b><size=17>Ciclo de vida · {displayName}</size></b>", _st);
        if (GUILayout.Button("ocultar", GUILayout.Width(64f))) _show = false;
        GUILayout.EndHorizontal();
        TimeController tc = TimeController.timeController;
        int hh = Mathf.FloorToInt(tc.Hour), mm = Mathf.FloorToInt((tc.Hour - hh) * 60f);
        GUILayout.Label($"Día {tc.Day} · {hh:00}:{mm:00} ({(tc.IsDay ? "de día" : "de noche")}) · un día dura {tc.RealMinutesPerDay:0} min reales a ×1", _small);

        _scroll = GUILayout.BeginScrollView(_scroll);
        foreach (Row r in rows)
        {
            string mark = r.seen ? "<color=#7CFC8A>✓</color>" : "<color=#BBBBBB>○</color>";
            string when = r.seen ? $"  <color=#9AB>({(r.at - _start) / 60f:0.0} min de juego · {r.who})</color>" : "";
            GUILayout.Label($"{mark}  {r.label}{when}", _st);
        }
        if (deathsOther > 0) GUILayout.Label($"<color=#E9A>Muertes por otras causas (no vejez): {deathsOther}</color>", _small);

        GUILayout.Space(6f);
        GUILayout.Label($"<b>En {(insects ? "el solar" : "la casa")}</b>", _st);
        foreach (PersonLife p in Living())
        {
            BodyMetabolism b = p.GetComponent<BodyMetabolism>(); Anima a = p.GetComponent<Anima>();
            string body = b != null ? $" · hambre {b.hunger:0.0} · baño {b.toilet:0.0} · suciedad {b.dirt:0.0}" : "";
            string sleep = a != null ? (a.asleep ? " · duerme" : $" · sueño {a.sleepiness:0.0}") : "";
            string age = p.Aging != null ? $" · crecimiento {p.Aging.growth:0.00} · desgaste {p.Aging.wear:0.00}" : "";
            string fam = p.partner != null ? $" · pareja de {p.partner.name}" : p.courtship > 0f ? $" · cortejo {p.courtship * 100f:0} %" : "";
            if (p.pregnant) fam += $" · embarazada ({p.GestationDays / Mathf.Max(1e-4f, p.GestationTotalDays) * 100f:0} %)";
            ActionQueue q = p.GetComponent<ActionQueue>();
            string doing = q != null && q.Own.Count > 0 ? $" · «{q.Own[0].Label}»" : "";
            GUILayout.Label($"{(p.female ? "♀" : "♂")} {p.name} · {Stage(p)}{age}{body}{sleep}{fam}{doing}", _small);
        }

        GUILayout.Space(6f);
        GUILayout.Label("<b>Velocidad del tiempo</b>", _st);
        Rect br = GUILayoutUtility.GetRect(w - 40f, 28f);
        if (GameSpeed.Buttons(br, Time.timeScale, out float sp)) GameSpeed.Set(sp);
        bool c = GUILayout.Toggle(_compressed, $" Año comprimido: {CicloVidaPanel.CompressedYear:0} días (natural: {CicloVidaPanel.NaturalYear:0})");
        if (c != _compressed) { _compressed = c; ApplyCalendar(); }
        GUILayout.Label($"<color=#999999>Todo va con el mismo año ({tc.DaysPerYear:0.##} días de juego): el embarazo (9 meses), crecer (18 años) y envejecer. Las necesidades van por horas del día.</color>", _small);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
