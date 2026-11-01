using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESCENAS DE CICLO DE VIDA (Berón 2026-11-01: «una escena por animal del santuario y así se comprobaría que realiza todos
/// sus procesos naturales»): el panel que OBSERVA a la pareja de la isla y marca con ✓ cada proceso natural en cuanto lo ve
/// (comer, dormir, ir al baño, celo, cortejo, gestación/incubación, parto, crianza, crecer, envejecer y morir). No empuja a
/// nadie: solo mira los mismos componentes que ya llevan los animales (AnimalVitals, EstrusState, Volition, Reproduction,
/// LifeStage; en la abeja, la Hive). Lo que el juego NO modela sale en gris con la razón (p. ej. beber: no hay sed).
/// Lo usa también la prueba (<see cref="CicloVidaTest"/>, grupo 58).
///
/// CALENDARIO: en estas escenas el año dura <see cref="CompressedYear"/> días de juego (en el santuario, 28), para que una
/// gestación se vea en minutos y no en horas; el botón lo vuelve al natural. El crecimiento y la vejez no dependen del año
/// (LifeStage cuenta sus «días» en minutos de juego), así que no cambian.
/// </summary>
public class CicloVidaPanel : MonoBehaviour
{
    public enum Habitat { Hielo, Mar, Costa, Arbol }

    /// <summary>Las especies del santuario (las que pone SampleScene: la fauna de hielo, la laguna, el bosque de árboles
    /// carnívoros y los recintos de la Cría), con su escena y su hábitat. Una sola fuente para el constructor y el panel.</summary>
    public static readonly (string species, string scene, string name, Habitat habitat)[] Species =
    {
        ("Bear", "OsoPolar", "Oso polar", Habitat.Hielo),
        ("Seal", "Foca", "Foca", Habitat.Mar),
        ("Whale", "Ballena", "Ballena", Habitat.Mar),
        ("Penguin", "Pinguino", "Pingüino", Habitat.Costa),
        ("Orca", "Orca", "Orca", Habitat.Mar),
        ("Malamute", "Malamute", "Malamute (perro de trineo)", Habitat.Hielo),
        ("SnowBee", "AbejaNieves", "Abeja de las nieves", Habitat.Arbol),
        ("SnowSpider", "AranaNieves", "Araña de las nieves", Habitat.Arbol),
    };

    public const float CompressedYear = 2f, NaturalYear = 28f;

    public string species = "Bear";
    public string displayName = "Oso polar";

    public class Row
    {
        public string key, label, note;
        public bool modelled = true, seen;
        public float at;   // Time.time al verlo
    }
    public readonly List<Row> rows = new List<Row>();
    readonly Dictionary<string, Row> _byKey = new Dictionary<string, Row>();

    /// <summary>Crías nacidas aquí (para seguir su crecimiento).</summary>
    public readonly List<Animal> born = new List<Animal>();
    public int deathsOther { get; private set; }
    readonly HashSet<Animal> _dead = new HashSet<Animal>();
    readonly Dictionary<Animal, float> _nearMother = new Dictionary<Animal, float>();
    float _next, _start;
    bool _show = true, _compressed = true;
    Vector2 _scroll;
    GUIStyle _st, _small;

    public bool Seen(string key) => _byKey.TryGetValue(key, out Row r) && r.seen;
    public Row Get(string key) => _byKey.TryGetValue(key, out Row r) ? r : null;

    bool Bee => species == "SnowBee";

    void Add(string key, string label, bool modelled = true, string note = "")
    {
        var r = new Row { key = key, label = label, modelled = modelled, note = note };
        rows.Add(r); _byKey[key] = r;
    }

    void Awake()
    {
        bool marine = species == "Seal" || species == "Whale" || species == "Orca";
        Add("comer", "Come (de su cuenco)");
        Add("beber", "Bebe", false, "no modelado: en el juego no hay sed");
        Add("dormir", "Duerme" + (SleepCycle.IsNocturnal(species) ? " (de día: nocturno)" : " (de noche)"));
        Add("bano", "Va al baño");
        if (Bee)
        {
            // La abeja nace del panal (Reproduction.HiveBreeds): la reina pone y las obreras crían en las celdillas.
            Add("celo", "Celo / vuelo nupcial", false, "no modelado: la reina pone sin aparearse (no hay zánganos que cubran)");
            Add("puesta", "La reina pone un huevo en una celdilla");
            Add("larva", "Eclosiona: larva (la alimentan las nodrizas)");
            Add("pupa", "Pupa: celdilla operculada");
            Add("nace", "Nace una abeja nueva");
            Add("crece", "La obrera cambia de oficio con la edad");
        }
        else
        {
            Add("celo", "Celo (la hembra adulta)");
            Add("cortejo", "Cortejo: la pareja junta");
            bool egg = species == "Penguin";
            Add("gestacion", egg ? "Incubación" : "Gestación", true,
                egg ? "sin huevo visible: el juego lleva la incubación dentro de la madre, como una gestación" : "");
            Add("parto", egg ? "Eclosión (nace el polluelo)" : "Parto (nace la cría)");
            bool care = BroodRide.ParentalCare(species);
            Add("crianza", "Crianza: la madre alimenta a la cría", care,
                care ? "" : "no modelado para esta especie: la madre no se vincula con la cría que pare (BroodRide.ParentalCare)");
            Add("junto", marine ? "La cría nada junto a su madre" : "La cría va junto a su madre", marine,
                marine ? "" : "no modelado en tierra: la cría no sigue a la madre (solo los marinos, MarineLife)");
            Add("joven", "La cría crece: joven");
            Add("adulto", "El joven crece: adulto");
        }
        Add("vejez", "Envejece y muere de viejo");
    }

    void Start()
    {
        _start = Time.time;
        Reproduction.Born += OnBorn;
        ApplyCalendar();
    }

    void OnDestroy() { Reproduction.Born -= OnBorn; }

    void OnBorn(Animal mother, Animal baby)
    {
        if (mother == null || baby == null || mother.SpeciesName != species) return;
        born.Add(baby);
        Mark("parto");
    }

    public void Mark(string key)
    {
        if (!_byKey.TryGetValue(key, out Row r) || r.seen) return;
        r.seen = true; r.at = Time.time;
        Debug.Log($"[CicloVida] {displayName}: ✓ {r.label} (a los {(Time.time - _start) / 60f:0.0} min de juego)");
    }

    /// <summary>Año comprimido (<see cref="CompressedYear"/> días) o natural (28): gestaciones, intervalos y la colmena.</summary>
    public void ApplyCalendar()
    {
        TimeController tc = TimeController.timeController;
        tc.DaysPerYear = _compressed ? CompressedYear : NaturalYear;
        float f = tc.DaysPerYear / NaturalYear;
        foreach (Hive h in Hive.All) if (h != null) h.secondsPerDay = tc.SecondsPerGameDay * f;   // el «día de abeja», al mismo ritmo
    }

    public List<Animal> Living()
    {
        var l = new List<Animal>();
        foreach (GameObject g in AnimalPopulations.Of(species)) if (g != null && g.TryGetComponent(out Animal a) && !a.death) l.Add(a);
        return l;
    }

    void Update()
    {
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + 0.25f;
        var all = new List<Animal>();
        foreach (GameObject g in AnimalPopulations.Of(species)) if (g != null && g.TryGetComponent(out Animal a)) all.Add(a);
        foreach (Animal a in all)
        {
            if (a.death)
            {
                if (_dead.Add(a))
                {
                    AdultStageEnded(a, out bool old);
                    if (old) Mark("vejez"); else deathsOther++;
                }
                continue;
            }
            if (a.TryGetComponent(out AnimalVitals v))
            {
                if (v.meals > 0) Mark("comer");
                if (v.sleptSeconds > 0f) Mark("dormir");
                if (v.defecations > 0) Mark("bano");
            }
            if (a.asleep) Mark("dormir");
            if (a.TryGetComponent(out EstrusState e) && e.InEstrus && a.sex == Sex.female) Mark("celo");
            Volition vo = a.GetComponent<Volition>();
            string desire = vo != null ? vo.LastDesire : "";
            if (a.TryGetComponent(out Reproduction rp) && rp.Pregnant) { Mark("cortejo"); Mark("gestacion"); }
            if (desire == "mate" && a.lifeStage == LifeStage.adult && Partner(a, all, 3f) != null) Mark("cortejo");
            if (desire == "nurse" && a.lifeStage == LifeStage.adult) Mark("crianza");
            if (born.Contains(a))
            {
                if (a.lifeStage == LifeStage.teen) Mark("joven");
                if (a.lifeStage == LifeStage.adult) { Mark("joven"); Mark("adulto"); }
                if (a.lifeStage == LifeStage.child)
                {
                    Animal mom = Mother(a, all);
                    float t = _nearMother.TryGetValue(a, out float s) ? s : 0f;
                    t = mom != null && Vector3.Distance(mom.transform.position, a.transform.position) < 4f ? t + 0.25f * Mathf.Max(0.01f, Time.timeScale) : 0f;
                    _nearMother[a] = t;
                    if (t >= 10f) Mark("junto");
                }
            }
        }
        if (Bee)
            foreach (Hive h in Hive.All)
            {
                if (h == null) continue;
                if (h.Count(Hive.Cell.Egg) > 0) Mark("puesta");
                if (h.Count(Hive.Cell.Larva) > 0) Mark("larva");
                if (h.Count(Hive.Cell.CappedBrood) > 0) Mark("pupa");
                if (h.born > 0) Mark("nace");
            }
        if (Bee)
            foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None))
                if (b != null && b.kind == IceInsectLife.Kind.Bee && !b.isQueen && _roles.TryGetValue(b, out IceInsectLife.Role r0) && r0 != b.role) Mark("crece");
                else if (b != null) _roles[b] = b.role;
    }
    readonly Dictionary<IceInsectLife, IceInsectLife.Role> _roles = new Dictionary<IceInsectLife, IceInsectLife.Role>();

    /// <summary>¿Murió al acabar su etapa adulta (vejez)? LifeStage pone `death` al pasar a `soul` tras sus días de adulto.</summary>
    public static void AdultStageEnded(Animal a, out bool old)
    {
        old = a.AdultStage != null && a.AdultStage.livedDays >= a.AdultStage.stageDays && a.AdultStage.stageDays > 0;
    }

    static Animal Partner(Animal a, List<Animal> all, float within)
    {
        foreach (Animal o in all)
            if (o != a && !o.death && o.lifeStage == LifeStage.adult && o.sex != a.sex && Vector3.Distance(o.transform.position, a.transform.position) <= within) return o;
        return null;
    }

    static Animal Mother(Animal cub, List<Animal> all)
    {
        Animal best = null; float bd = float.MaxValue;
        foreach (Animal o in all)
        {
            if (o == cub || o.death || o.lifeStage != LifeStage.adult || o.sex != Sex.female) continue;
            float d = Vector3.Distance(o.transform.position, cub.transform.position);
            if (d < bd) { bd = d; best = o; }
        }
        return best;
    }

    static string Stage(char s) => s == LifeStage.child ? "cría" : s == LifeStage.teen ? "joven" : s == LifeStage.adult ? "adulto" : s == LifeStage.soul ? "muerto" : "?";

    // ── Panel ────────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        if (ColdSanctuary.TestRunner.Running && !_show) return;
        if (_st == null)
        {
            _st = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, wordWrap = true };
            _small = new GUIStyle(GUI.skin.label) { fontSize = 12, richText = true, wordWrap = true };
        }
        if (!_show)
        {
            if (GUI.Button(new Rect(10f, 10f, 190f, 26f), "Ciclo de vida (mostrar)")) _show = true;
            return;
        }
        float w = 430f, h = Mathf.Min(Screen.height - 20f, 720f);
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
            string mark = r.seen ? "<color=#7CFC8A>✓</color>" : r.modelled ? "<color=#BBBBBB>○</color>" : "<color=#888888>—</color>";
            string when = r.seen ? $"  <color=#9AB>({(r.at - _start) / 60f:0.0} min de juego)</color>" : "";
            string label = r.modelled || r.seen ? r.label : $"<color=#999999>{r.label}</color>";
            GUILayout.Label($"{mark}  {label}{when}", _st);
            if (!string.IsNullOrEmpty(r.note) && !r.seen) GUILayout.Label($"      <color=#999999><i>{r.note}</i></color>", _small);
        }
        if (deathsOther > 0) GUILayout.Label($"<color=#E9A>Muertes por otras causas (no vejez): {deathsOther}</color>", _small);

        GUILayout.Space(6f);
        GUILayout.Label("<b>En la isla</b>", _st);
        foreach (Animal a in Living())
        {
            Volition vo = a.GetComponent<Volition>();
            string extra = "";
            if (a.TryGetComponent(out Reproduction rp) && rp.Pregnant)
                extra = $" · {(species == "Penguin" ? "incubando" : "preñada")}: nace en ~{Mathf.Max(0f, rp.BirthAt - Time.time) / 60f / Mathf.Max(0.01f, Time.timeScale):0.0} min reales";
            if (a.TryGetComponent(out EstrusState e) && e.InEstrus) extra += " · en celo";
            LifeStage st = a.lifeStage == LifeStage.child ? (LifeStage)a.ChildStage : a.lifeStage == LifeStage.teen ? a.TeenStage : a.AdultStage;
            string age = st != null ? $" {st.livedDays}/{st.stageDays} d" : "";
            GUILayout.Label($"{(a.sex == Sex.female ? "♀" : "♂")} {a.name} · {Stage(a.lifeStage)}{age} · hambre {a.hungry:0.0}{(a.asleep ? " · duerme" : "")}" +
                            $"{(vo != null && vo.LastDesire.Length > 0 ? " · quiere «" + vo.LastDesire + "»" : "")}{extra}", _small);
        }

        GUILayout.Space(6f);
        GUILayout.Label("<b>Velocidad del tiempo</b>", _st);
        Rect br = GUILayoutUtility.GetRect(w - 40f, 28f);
        if (GameSpeed.Buttons(br, Time.timeScale, out float sp)) GameSpeed.Set(sp);
        bool c = GUILayout.Toggle(_compressed, $" Año comprimido: {CompressedYear:0} días (natural: {NaturalYear:0})");
        if (c != _compressed) { _compressed = c; ApplyCalendar(); }
        GUILayout.Label("<color=#999999>Crecer y envejecer no dependen del año: LifeStage cuenta sus días en minutos de juego.</color>", _small);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
