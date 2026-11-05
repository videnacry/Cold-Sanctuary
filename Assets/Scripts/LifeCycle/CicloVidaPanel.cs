using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESCENAS DE CICLO DE VIDA (Berón 2026-11-01: «una escena por animal del santuario y así se comprobaría que realiza todos
/// sus procesos naturales»): el panel que OBSERVA a la pareja de la isla y marca con ✓ cada proceso natural en cuanto lo ve.
/// No empuja a nadie: solo mira los componentes y eventos que ya llevan los animales (AnimalVitals, Hydration, EstrusState,
/// Volition, Reproduction, MaternityDen, Egg, LifeStage; en la abeja, la Hive y el vuelo nupcial de IceInsectLife). Las
/// filas salen de la biología de cada especie (<see cref="LifeHistory"/>): la osa entra en su guarida, el pingüino pone un
/// huevo que incuba el macho, la abeja reina sale en vuelo nupcial… Lo usa también la prueba (<see cref="CicloVidaTest"/>,
/// grupo 58). Ver docs/ciclo-de-vida.md.
///
/// CALENDARIO: en estas escenas el año dura <see cref="CompressedYear"/> días de juego (en el santuario, 28), para que una
/// gestación se vea en minutos y no en horas; el botón lo vuelve al natural. TODO va con ese año: gestación, incubación,
/// lactancia, guarida, la colmena, crecer y envejecer (LifeStage cuenta días reales de vida en ese calendario).
/// </summary>
public class CicloVidaPanel : MonoBehaviour
{
    public enum Habitat { Hielo, Mar, Costa, Arbol, Tierra }

    /// <summary>Las especies del santuario (las que pone SampleScene: la fauna de hielo, la laguna, el bosque de árboles
    /// carnívoros y los recintos de la Cría), con su escena y su hábitat. Una sola fuente para el constructor y el panel.
    /// La especie es la de su CICLO DE VIDA (<see cref="Animal.LifeSpecies"/>: la araña de las nieves tiene la suya).</summary>
    public static readonly (string species, string scene, string name, Habitat habitat)[] Species =
    {
        ("Bear", "OsoPolar", "Oso polar", Habitat.Hielo),
        ("Seal", "Foca", "Foca", Habitat.Mar),
        ("Whale", "Ballena", "Ballena", Habitat.Mar),
        ("Penguin", "Pinguino", "Pingüino de Adelia", Habitat.Costa),
        ("EmperorPenguin", "PinguinoEmperador", "Pingüino emperador", Habitat.Costa),
        ("SnowPetrel", "PetrelNieves", "Petrel de las nieves", Habitat.Costa),
        ("Walrus", "Morsa", "Morsa", Habitat.Mar),
        ("Reindeer", "Reno", "Reno", Habitat.Tierra),
        ("Wolf", "LoboArtico", "Lobo ártico", Habitat.Tierra),
        ("ArcticHare", "LiebreArtica", "Liebre ártica", Habitat.Tierra),
        ("ArcticFox", "ZorroArtico", "Zorro ártico", Habitat.Tierra),
        ("MuskOx", "BueyAlmizclero", "Buey almizclero", Habitat.Tierra),
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
    LifeHistory _life;

    public bool Seen(string key) => _byKey.TryGetValue(key, out Row r) && r.seen;
    public Row Get(string key) => _byKey.TryGetValue(key, out Row r) ? r : null;
    public LifeHistory Life => _life ?? (_life = LifeHistory.Of(species));

    public bool Bee => species == "SnowBee";
    public bool Marine => species == "Seal" || species == "Walrus" || species == "Whale" || species == "Orca";

    void Add(string key, string label, bool modelled = true, string note = "")
    {
        var r = new Row { key = key, label = label, modelled = modelled, note = note };
        rows.Add(r); _byKey[key] = r;
    }

    void Awake()
    {
        LifeHistory life = Life;
        Add("comer", "Come (de su cuenco, su banco o su panal)");
        Add("beber", string.IsNullOrEmpty(life.waterLabel) ? "Bebe" : life.waterLabel);
        Add("dormir", "Duerme" + (SleepCycle.IsNocturnal(species) ? " (de día: nocturno)" : " (de noche)"));
        Add("bano", "Va al baño");
        if (Bee)
        {
            // La abeja nace del panal (Hive): la reina virgen se aparea en el aire y luego pone; las obreras crían.
            Add("nupcial", "Vuelo nupcial: la reina se aparea con un zángano en el aire (y el zángano muere)");
            Add("puesta", "La reina pone un huevo en una celdilla");
            Add("larva", "Eclosiona: larva (la alimentan las nodrizas)");
            Add("pupa", "Pupa: celdilla operculada");
            Add("nace", "Nace una abeja nueva");
            Add("zangano", "Nace un zángano (de un huevo sin fecundar, a los 24 días)");
            Add("crece", "La obrera cambia de oficio con la edad");
        }
        else
        {
            Add("celo", "Celo (la hembra adulta)");
            Add("cortejo", species == "SnowSpider" ? "Cortejo: el macho pulsa la tela de la hembra" : "Cortejo: la pareja junta");
            Add("gestacion", life.eggs ? life.eggLabel : "Gestación");
            if (life.den) Add("guarida", life.denLabel);
            if (life.eggs) { Add("puesta", life.layLabel); Add("incuba", life.incubateLabel); }
            Add("parto", life.eggs ? life.hatchLabel : "Parto (nace la cría)");
            if (BroodRide.ParentalCare(species)) Add("crianza", life.careLabel);
            Add("junto", Marine ? "La cría nada junto a su madre" : string.IsNullOrEmpty(life.followLabel) ? "La cría va junto a su madre" : life.followLabel);
            if (life.den) Add("sale", life.denExitLabel);
            Add("joven", "La cría crece: joven");
            Add("adulto", "El joven crece: adulto");
        }
        Add("vejez", "Envejece y muere de viejo");
    }

    void Start()
    {
        _start = Time.time;
        Reproduction.Born += OnBorn;
        Hydration.Drank += OnDrank;
        DesireCatalog.CubFed += OnFed;
        MaternityDen.Emerged += OnEmerged;
        Egg.Laid += OnLaid;
        Egg.Transferred += OnTransferred;
        IceInsectLife.Mated += OnMated;
        Hive.Emerged += OnHiveEmerged;
        ApplyCalendar();
    }

    void OnDestroy()
    {
        Reproduction.Born -= OnBorn;
        Hydration.Drank -= OnDrank;
        DesireCatalog.CubFed -= OnFed;
        MaternityDen.Emerged -= OnEmerged;
        Egg.Laid -= OnLaid;
        Egg.Transferred -= OnTransferred;
        IceInsectLife.Mated -= OnMated;
        Hive.Emerged -= OnHiveEmerged;
    }

    bool Mine(Animal a) => a != null && a.LifeSpecies == species;

    void OnBorn(Animal mother, Animal baby)
    {
        if (!Mine(mother) || baby == null) return;
        born.Add(baby);
        if (mother.TryGetComponent(out MaternityDen md) && md.InDen) Mark("guarida");   // ha parido DENTRO de su guarida
        Mark("parto");
    }
    void OnDrank(Animal a, string how) { if (Mine(a)) Mark("beber"); }
    void OnFed(Animal parent, Animal cub) { if (Mine(parent)) Mark("crianza"); }
    void OnEmerged(Animal mother) { if (Mine(mother)) Mark("sale"); }
    void OnLaid(Egg e) { if (e != null && e.species == species) Mark("puesta"); }
    void OnTransferred(Egg e) { if (e != null && e.species == species) Mark("incuba"); }
    void OnMated(IceInsectLife queen, IceInsectLife drone) { if (Bee) Mark("nupcial"); }
    void OnHiveEmerged(Hive h, IceInsectLife bee, bool drone) { if (Bee && drone) Mark("zangano"); }

    public void Mark(string key)
    {
        if (!_byKey.TryGetValue(key, out Row r) || r.seen) return;
        r.seen = true; r.at = Time.time;
        Debug.Log($"[CicloVida] {displayName}: ✓ {r.label} (a los {(Time.time - _start) / 60f:0.0} min de juego)");
    }

    /// <summary>Año comprimido (<see cref="CompressedYear"/> días) o natural (28). La colmena sigue el mismo calendario.</summary>
    public void ApplyCalendar()
    {
        TimeController tc = TimeController.timeController;
        tc.DaysPerYear = _compressed ? CompressedYear : NaturalYear;
        foreach (Hive h in Hive.All) if (h != null) h.secondsPerDay = 0f;   // el «día de abeja» = un día de vida del calendario
    }

    /// <summary>Todos los de la especie (vivos y muertos) de la escena.</summary>
    public List<Animal> All()
    {
        var l = new List<Animal>();
        foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None)) if (Mine(a)) l.Add(a);
        return l;
    }

    public List<Animal> Living()
    {
        var l = All();
        l.RemoveAll(a => a == null || a.death);
        return l;
    }

    void Update()
    {
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + 0.25f;
        List<Animal> all = All();
        foreach (Animal a in all)
        {
            if (a.death)
            {
                if (_dead.Add(a))
                {
                    AdultStageEnded(a, out bool old);
                    // El zángano muere al aparearse: muerte natural de su ciclo, no un accidente.
                    bool mating = a.TryGetComponent(out IceInsectLife il) && il.diedMating;
                    if (old) Mark("vejez"); else if (!mating) deathsOther++;
                }
                continue;
            }
            if (a.TryGetComponent(out AnimalVitals v))
            {
                if (v.meals > 0) Mark("comer");
                if (v.sleptSeconds > 0f) Mark("dormir");
                if (v.defecations > 0) Mark("bano");
            }
            if (a.TryGetComponent(out Hydration hy) && hy.drinks > 0) Mark("beber");
            if (a.asleep) Mark("dormir");
            if (a.TryGetComponent(out EstrusState e) && e.InEstrus && a.sex == Sex.female) Mark("celo");
            Volition vo = a.GetComponent<Volition>();
            string desire = vo != null ? vo.LastDesire : "";
            if (a.TryGetComponent(out Reproduction rp) && rp.Pregnant) { Mark("cortejo"); Mark("gestacion"); }
            if (desire == "mate" && a.lifeStage == LifeStage.adult && Partner(a, all, 3f) != null) Mark("cortejo");
            if (born.Contains(a))
            {
                if (a.lifeStage == LifeStage.teen) Mark("joven");
                if (a.lifeStage == LifeStage.adult) { Mark("joven"); Mark("adulto"); }
                if (a.lifeStage == LifeStage.child)
                {
                    Animal mom = Parent(a, all);
                    float t = _nearMother.TryGetValue(a, out float s) ? s : 0f;
                    t = mom != null && Vector3.Distance(mom.transform.position, a.transform.position) < 4f ? t + 0.25f * Mathf.Max(0.01f, Time.timeScale) : 0f;
                    _nearMother[a] = t;
                    if (t >= 10f) Mark("junto");
                }
            }
        }
        // El saco de la araña: la madre lo guarda (a su lado) unos segundos.
        foreach (Egg egg in Egg.All)
            if (egg != null && egg.species == species && egg.Life != null && egg.Life.incubator == LifeHistory.Incubator.Female && egg.attendedSeconds > 5f) Mark("incuba");
        if (Bee)
        {
            foreach (Hive h in Hive.All)
            {
                if (h == null) continue;
                if (h.Count(Hive.Cell.Egg) > 0) Mark("puesta");
                if (h.Count(Hive.Cell.Larva) > 0) Mark("larva");
                if (h.Count(Hive.Cell.CappedBrood) > 0) Mark("pupa");
                if (h.born > 0) Mark("nace");
                if (h.dronesBorn > 0) Mark("zangano");
            }
            foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None))
                if (b != null && b.kind == IceInsectLife.Kind.Bee && !b.isQueen && !b.drone && _roles.TryGetValue(b, out IceInsectLife.Role r0) && r0 != b.role) Mark("crece");
                else if (b != null) _roles[b] = b.role;
        }
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

    // El progenitor más cercano: la madre o, en los que crían en pareja (el pingüino), también el padre.
    Animal Parent(Animal cub, List<Animal> all)
    {
        Animal best = null; float bd = float.MaxValue;
        foreach (Animal o in all)
        {
            if (o == cub || o.death || o.lifeStage != LifeStage.adult || (o.sex != Sex.female && !Life.biparental)) continue;
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
        float w = 430f, h = Mathf.Min(Screen.height - 20f, 760f);
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
                extra = $" · {(Life.eggs ? "forma el huevo" : "preñada")}: {(Life.eggs ? "lo pone" : "nace")} en ~{Mathf.Max(0f, rp.BirthAt - Time.time) / 60f / Mathf.Max(0.01f, Time.timeScale):0.0} min reales";
            if (a.TryGetComponent(out EstrusState e) && e.InEstrus) extra += " · en celo";
            if (a.TryGetComponent(out MaternityDen md) && md.InDen) extra += " · en la guarida";
            foreach (Egg egg in Egg.All) if (egg != null && egg.incubator == a && egg.Attended) extra += $" · incuba ({egg.progress * 100f:0} %)";
            if (a.TryGetComponent(out Hydration hy)) extra += $" · sed {hy.thirst:0.0}";
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
        GUILayout.Label($"<color=#999999>Todo va con el mismo año ({tc.DaysPerYear:0.##} días de juego): gestación, incubación, lactancia, guarida, la colmena, crecer y envejecer (un día de vida = {LifeStage.SecondsPerLifeDay(tc):0.#} s a ×1).</color>", _small);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
