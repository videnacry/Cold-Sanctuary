using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL PARQUE ENERGÉTICO en juego (Berón 2026-11-05; docs/energia.md): el campo solar, los aerogeneradores, la turbina mareal piloto y el almacén, y el balance con la
/// demanda de todos los pisos. El reactor cubre lo que falte hasta su tope; lo que sobra carga las baterías y luego el silo de arena. Si para el reactor,
/// las baterías sostienen lo esencial. La jornada lo cuida: limpiar, instalar, reparar, revisar. Vive una vez por segundo: nada espera a nada.
/// </summary>
public class EnergySite : MonoBehaviour
{
    public static EnergySite Instance { get; private set; }

    [Tooltip("Todas las plazas de mesa solar, en orden: las primeras 'installed' están montadas; las demás, ocultas hasta que Fernando las instale.")]
    public List<Transform> tables = new List<Transform>();
    public int installed;
    public List<WindTurbineUnit> turbines = new List<WindTurbineUnit>();
    public List<TidalTurbineUnit> tidal = new List<TidalTurbineUnit>();
    [Tooltip("Los contenedores de baterías y el silo (sitios de la misión del almacén).")]
    public Transform storage;
    [Tooltip("Las figuras aterradoras (ocultas hasta que salta un detector) y sus sitios: se cambian de sitio para que las aves no se acostumbren.")]
    public List<Transform> figures = new List<Transform>();
    public List<Vector3> figureSpots = new List<Vector3>();
    public DroneDragon dragon;
    [Tooltip("Lo acostumbradas que están las aves a las figuras y a los drones (0–1).")]
    public float habituation;
    public int injuredWaiting, carcassesWaiting;

    /// <summary>La suciedad del campo (escarcha y nieve): de 0 a <see cref="EnergyRule.MaxSoil"/>.</summary>
    public float soil;
    public float batteryMWh = EnergyRule.BatteryMWh, sandMWh;
    /// <summary>(Pruebas y emergencias) El reactor no entrega.</summary>
    public bool reactorForcedOff;

    public float SolarKW { get; private set; }
    public float WindKW { get; private set; }
    public float TidalKW { get; private set; }
    public float ReactorKW { get; private set; }
    public float DemandKW { get; private set; }
    public float ShedKW { get; private set; }
    public int Maintained { get; private set; }
    public int Installs { get; private set; }
    public int Repairs { get; private set; }
    public EnergyRule.Report Last { get; private set; }
    public int Scares { get; private set; }
    public int Carcasses { get; private set; }
    public int Moves { get; private set; }
    /// <summary>Fuerza de la disuasión ahora: baja con la habituación. NO cuenta para la seguridad: la turbina para igual.</summary>
    public float DeterrentStrength => 1f - habituation;

    /// <summary>(Pruebas) Puntos que el sitio trata como un ave y como un animal en el agua.</summary>
    public static readonly List<Transform> TestBirds = new List<Transform>(), TestSwimmers = new List<Transform>();

    readonly HashSet<int> _faults = new HashSet<int>();
    float _next;
    int _day = int.MinValue;
    int _figureSpot;
    float _figuresUntil = -1f, _scareAt = -999f;

    public int Pending { get { Report(); return Mathf.Clamp(Last.tablesRequired - installed, 0, tables.Count - installed); } }
    public int Faults => _faults.Count;
    public float FieldKWp => installed * EnergyRule.TableKWp;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        EnergyRule.DeclareMissing();
        for (int i = 0; i < tables.Count; i++) if (tables[i] != null) tables[i].gameObject.SetActive(i < installed);
        Validate("al empezar");
    }

    /// <summary>El balance ahora, sin registrar nada.</summary>
    public EnergyRule.Report Report()
    {
        int free = Mathf.Max(0, tables.Count - installed);
        return Last = EnergyRule.Check(FieldKWp + EnergyRule.RooftopKWp(), installed, free);
    }

    /// <summary>LA REGLA en juego: declara lo que falte y comprueba. Un fallo por paneles no es un error: es trabajo para <c>sol_instalacion</c>.</summary>
    public EnergyRule.Report Validate(string when)
    {
        EnergyRule.DeclareMissing();
        EnergyRule.Report r = Report();
        if (r.Ok) Debug.Log($"[Energía] Regla ({when}): {r}.");
        else Debug.LogWarning($"[Energía] Regla ({when}): {r}.");
        return r;
    }

    void Update()
    {
        if (Time.time < _next) return;
        float dt = Time.time - (_next - 1f); _next = Time.time + 1f;
        TimeController tc = TimeController.timeController;
        if (tc == null) return;
        float hours = Mathf.Clamp(dt, 0f, 5f) * 24f / Mathf.Max(1f, tc.SecondsPerGameDay);
        NewDay(tc.Day);
        if (_figuresUntil > 0f && Time.time >= _figuresUntil) ShowFigures(false);
        Scan();
        Step(tc.Day * 24f + tc.Hour, SolarPosition.SanctuaryDayOfYear(tc), tc.Hour, hours);
    }

    void NewDay(int day)
    {
        if (day == _day) return;
        bool first = _day == int.MinValue; _day = day;
        if (first) return;
        soil = Mathf.Min(EnergyRule.MaxSoil, soil + EnergyRule.SoilPerDay);
        habituation = Mathf.Max(0f, habituation - EnergyRule.HabituationDecay);
        int d9 = ((day % 9) + 9) % 9;
        if (d9 == 4) injuredWaiting++;   // aves heridas y restos al pie de lo que se levanta: torres, mástiles y mesas
        if (d9 == 7) carcassesWaiting++;
        if (((day % 5) + 5) % 5 == 2 && installed > 0) Fail(Mathf.Abs(day * 37) % installed);
        Validate($"día {day}");
    }

    /// <summary>Se avería la mesa <paramref name="i"/>: baliza encendida y deja de producir.</summary>
    public void Fail(int i)
    {
        if (i < 0 || i >= installed || !_faults.Add(i)) return;
        Beacon(i, true);
        Debug.Log($"[Energía] Avería en la mesa solar {i}.");
    }

    void Beacon(int i, bool on) { Transform b = tables[i] != null ? tables[i].Find("Aviso_averia") : null; if (b != null) b.gameObject.SetActive(on); }

    /// <summary>Lo que dan el sol, el viento y la marea en ese momento, y el reparto: reactor, baterías, arena.</summary>
    public void Step(float t, float doy, float hour, float dtHours)
    {
        float wind = EnergyRule.WindSpeed(t), current = EnergyRule.TidalCurrent(t);
        SolarKW = (installed - _faults.Count) * EnergyRule.TableKWp * EnergyRule.SolarAt(doy, hour) * (1f - soil);
        WindKW = 0f; foreach (WindTurbineUnit u in turbines) if (u != null) { u.Tick(wind); WindKW += u.OutputKW(wind); }
        TidalKW = 0f; foreach (TidalTurbineUnit u in tidal) if (u != null) { u.Tick(current); TidalKW += u.OutputKW(current); }
        DemandKW = EnergyRule.DemandKW();
        float gen = SolarKW + WindKW + TidalKW;
        bool reactor = !reactorForcedOff && (NuclearSite.Instance == null || NuclearSite.Instance.State == NuclearSite.Alert.Normal);
        // Sin reactor se apaga lo no esencial: las baterías solo sostienen lo esencial.
        float need = reactor ? DemandKW : Mathf.Min(DemandKW, EnergyRule.EssentialKW());
        ShedKW = DemandKW - need;
        ReactorKW = reactor ? Mathf.Clamp(need - gen, 0f, EnergyRule.ReactorKW) : 0f;
        float balance = gen + ReactorKW - need;
        if (balance >= 0f)
        {
            float toBat = Mathf.Min(balance * dtHours * EnergyRule.BatteryEfficiency / 1000f, EnergyRule.BatteryMWh - batteryMWh);
            batteryMWh += toBat;
            float left = balance * dtHours / 1000f - toBat / EnergyRule.BatteryEfficiency;
            sandMWh = Mathf.Min(EnergyRule.SandMWh, sandMWh + Mathf.Max(0f, left));
        }
        else
        {
            float want = -balance * dtHours / 1000f / EnergyRule.BatteryEfficiency;
            float got = Mathf.Min(want, batteryMWh);
            batteryMWh -= got;
            if (got < want) ShedKW += (want - got) * EnergyRule.BatteryEfficiency * 1000f / Mathf.Max(1e-5f, dtHours);
        }
        sandMWh = Mathf.Max(0f, sandMWh - dtHours * 0.1f);   // el calor de distrito tira del silo (~100 kW)
    }

    // ── LA FAUNA: una pasada por segundo, por aves y por animales en el agua ─────────────────────────────────────────

    void Scan()
    {
        foreach (Transform b in TestBirds) if (b != null) Bird(b.position, "ave cerca (prueba)");
        foreach (Transform s in TestSwimmers) if (s != null) Swimmer(s.position, "animal cerca (prueba)");
        foreach (GameObject g in BirdBehavior.population) if (g != null) Bird(g.transform.position, "ave cerca");
        foreach (Anima a in SceneCache.All<Anima>(2f))
        {
            if (a == null) continue;
            Vector3 p = a.transform.position;
            if (a is Animal)
            {
                if (p.y < PolarSeaLevel - 0.5f) Swimmer(p, $"{a.name} en el agua");
                else if (p.y > 3f) Bird(p, $"{a.name} en el aire");
                continue;
            }
            foreach (WindTurbineUnit u in turbines)
                if (u != null && Flat(p, u.transform.position) <= EnergyRule.TowerLockout) u.Hold("alguien al pie de la torre");
        }
    }

    const float PolarSeaLevel = -0.25f;

    void Bird(Vector3 p, string why)
    {
        foreach (WindTurbineUnit u in turbines)
        {
            if (u == null || Flat(p, u.transform.position) > EnergyRule.BirdStopRadius) continue;
            bool first = !u.Stopped;
            u.Hold(why);   // LA SEGURIDAD: para siempre, haya o no disuasión
            if (first) Scare(u.transform.position);
        }
    }

    // LA DISUASIÓN: solo cuando salta un detector (no enseñarla siempre es lo que apunta la habituación). Las figuras y el dragón son del juego: docs/energia.md §7.
    void Scare(Vector3 at)
    {
        if (Time.time - _scareAt < 60f) return;
        _scareAt = Time.time;
        if (DeterrentStrength < EnergyRule.MinDeterrent) { Debug.Log("[Energía] Las aves ya no hacen caso a las figuras: toca ene_disuasion."); return; }
        Scares++;
        habituation = Mathf.Min(1f, habituation + EnergyRule.HabituationPerScare);
        ShowFigures(true);
        _figuresUntil = Time.time + EnergyRule.ResumeSeconds;
        if (dragon != null) dragon.Launch(at);
    }

    void ShowFigures(bool on)
    {
        foreach (Transform f in figures) if (f != null) f.gameObject.SetActive(on);
        if (!on) _figuresUntil = -1f;
    }

    void Swimmer(Vector3 p, string why) { foreach (TidalTurbineUnit u in tidal) if (u != null && Flat(p, u.transform.position) <= EnergyRule.MarineStopRadius) u.Hold(why); }
    static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    /// <summary>(Pruebas) La pasada de fauna ahora mismo, sin esperar al segundo.</summary>
    public void ScanNow() => Scan();

    // ── EL TRABAJO DE LA JORNADA ─────────────────────────────────────────────────────────────────────────────────────

    /// <summary>«sol_mantenimiento»: quita la escarcha y la nieve del campo.</summary>
    public void Maintain(string who) { soil = 0f; Maintained++; Debug.Log($"[Energía] {who} limpia el campo solar: {installed} mesas."); }

    /// <summary>«sol_instalacion»: monta las siguientes mesas pendientes (las de los pisos nuevos). Devuelve cuántas.</summary>
    public int Install(string who)
    {
        int n = Mathf.Min(EnergyRule.TablesPerShift, Pending);
        for (int i = 0; i < n; i++) { if (tables[installed] != null) tables[installed].gameObject.SetActive(true); installed++; }
        if (n > 0) { Installs += n; Validate($"{who} instala {n} mesas"); }
        return n;
    }

    /// <summary>«sol_reparacion»: repone las mesas averiadas.</summary>
    public int Repair(string who)
    {
        int n = _faults.Count;
        foreach (int i in _faults) Beacon(i, false);
        _faults.Clear(); Repairs += n;
        if (n > 0) Debug.Log($"[Energía] {who} repara {n} mesas.");
        return n;
    }

    /// <summary>«ene_ronda_aves»: busca aves heridas y restos al pie de torres y mesas. Las heridas van al refugio del Hospital; los restos se retiran para no atraer a los carroñeros.</summary>
    public void Patrol(string who)
    {
        Carcasses += carcassesWaiting; carcassesWaiting = 0;
        BirdRehabPoint rp = BirdRehabPoint.Instance;
        while (injuredWaiting > 0 && rp != null && rp.Admit(injuredWaiting % 2 == 0 ? "petrel de las nieves" : "págalo")) injuredWaiting--;   // (acotado: baja o sale)
        Debug.Log($"[Energía] {who} hace la ronda de aves: {Carcasses} restos retirados en total.");
    }

    /// <summary>«ene_disuasion»: cambia las figuras de sitio y revisa los drones: lo nuevo vuelve a contar. Baja la habituación, no la borra.</summary>
    public void MoveDeterrents(string who)
    {
        habituation = Mathf.Max(0f, habituation - EnergyRule.HabituationReset); Moves++;
        if (figureSpots.Count == 0) return;
        _figureSpot = (_figureSpot + 1) % figureSpots.Count;
        for (int i = 0; i < figures.Count; i++)
            if (figures[i] != null) figures[i].position = figureSpots[(_figureSpot + i * figureSpots.Count / Mathf.Max(1, figures.Count)) % figureSpots.Count];
    }

    /// <summary>Las misiones de la jornada en el parque (si existe). Instalar y reparar, solo si hay qué.</summary>
    public static List<WorkRoster.Mission> Missions(int day)
    {
        var l = new List<WorkRoster.Mission>();
        EnergySite s = Object.FindAnyObjectByType<EnergySite>();
        if (s == null) return l;
        int d = ((day % 3) + 3) % 3;
        l.Add(new WorkRoster.Mission(EnergyRule.Area, "sol_mantenimiento", "Mantenimiento de los paneles solares: quitar escarcha y nieve y revisar las mesas", 1f));
        l.Add(new WorkRoster.Mission(EnergyRule.Area, "ene_control", "Control de la energía: reparto de cargas y registro de paradas por fauna", 0.5f));
        if (s.Pending > 0) l.Add(new WorkRoster.Mission(EnergyRule.Area, "sol_instalacion", $"Instalar paneles solares: {s.Pending} mesas pendientes de los pisos nuevos", 2f));
        if (s.Faults > 0) l.Add(new WorkRoster.Mission(EnergyRule.Area, "sol_reparacion", $"Reparar paneles solares: {s.Faults} mesas averiadas", 1f));
        if (d == 0) l.Add(new WorkRoster.Mission(EnergyRule.Area, "ene_eolica", "Revisar los aerogeneradores con la turbina parada: palas, cámaras y radar", 1f));
        if (d == 1) l.Add(new WorkRoster.Mission(EnergyRule.Area, "ene_mareal", "Revisar la turbina mareal: sonar, anillo y fugas", 1f));
        if (d == 2) l.Add(new WorkRoster.Mission(EnergyRule.Area, "ene_almacen", "Revisar el almacén: calefacción de las baterías y silo de arena", 0.5f));
        l.Add(new WorkRoster.Mission(EnergyRule.Area, "ene_ronda_aves", "Ronda de aves: buscar heridas y retirar restos al pie de torres, mástiles y mesas", 0.5f));
        if (s.DeterrentStrength < 0.7f) l.Add(new WorkRoster.Mission(EnergyRule.Area, "ene_disuasion", "Cambiar de sitio las figuras y revisar los drones, para que las aves no se acostumbren", 1f));
        BirdRehabPoint rp = BirdRehabPoint.Instance;
        if (rp != null && rp.patients.Count > 0) l.Add(new WorkRoster.Mission(AreaWork.Hospital, "hos_aves", $"Refugio de aves: curar a {rp.patients.Count} y soltar a las que estén listas, lejos de las turbinas", 0.5f));
        return l;
    }

    /// <summary>¿Se hace en el casquete y no en el piso de control?</summary>
    /// <summary>¿Está en el casquete del parque (y sus pontonas)? Para la jornada, es su puesto: la Magnate no lo devuelve.</summary>
    public static bool OnCap(Vector3 p) => Mathf.Abs(p.x - EnergyRule.Center.x) <= EnergyRule.CapHalf + 16f && Mathf.Abs(p.z - EnergyRule.Center.z) <= EnergyRule.CapHalf + 2f;

    public static bool IsField(string missionId) => missionId != null && (missionId.StartsWith("sol_") || missionId == "ene_eolica" || missionId == "ene_mareal" || missionId == "ene_almacen" || missionId == "ene_ronda_aves" || missionId == "ene_disuasion");

    /// <summary>Dónde se pone a trabajar <paramref name="who"/> en esa misión: junto a la mesa que toca, la torre, la pontona o el almacén. Null si no es de campo.</summary>
    public static Vector3? FieldPoint(string missionId, string who)
    {
        EnergySite s = Instance;
        if (s == null || !IsField(missionId)) return null;
        Transform t = null;
        int hash = Mathf.Abs((who ?? "").GetHashCode());
        if ((missionId == "ene_eolica" || missionId == "ene_ronda_aves") && s.turbines.Count > 0) t = s.turbines[hash % s.turbines.Count].transform;
        else if (missionId == "ene_mareal" && s.tidal.Count > 0) t = s.tidal[hash % s.tidal.Count].transform.parent;
        else if (missionId == "ene_almacen" || missionId == "ene_disuasion") t = s.storage;
        else if (missionId == "sol_instalacion" && s.installed < s.tables.Count) t = s.tables[s.installed];
        else if (missionId == "sol_reparacion" && s._faults.Count > 0) foreach (int i in s._faults) { t = s.tables[i]; break; }
        else if (s.installed > 0) t = s.tables[hash % s.installed];
        if (t == null) return null;
        Vector3 toCentre = EnergyRule.Center - t.position; toCentre.y = 0f;
        Vector3 p = t.position + toCentre.normalized * 6f + Vector3.forward * 2f; p.y = 0f;
        return p + Vector3.up;
    }
}
