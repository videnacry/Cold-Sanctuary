using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PÉRDIDA DE SANGRE de un paciente (docs/hospital.md §Banco de sangre): fracción del volumen de sangre perdida (0–1). Las
/// clases de hemorragia del ATLS (American College of Surgeons, <i>Advanced Trauma Life Support</i>, 10.ª ed., 2018):
/// I &lt; 15 %, II 15–30 %, III 30–40 % (hace falta sangre), IV &gt; 40 % (riesgo de muerte inmediato). Con más del 30 % el paciente
/// empeora solo (shock); cada unidad transfundida devuelve su volumen (<see cref="BloodBank.UnitFraction"/>).
/// </summary>
public class BloodLoss : MonoBehaviour
{
    [Range(0f, 1f)] public float lost;
    public int unitsReceived;
    public string lastProduct;

    public int AtlsClass => lost < 0.15f ? 1 : lost < 0.3f ? 2 : lost < 0.4f ? 3 : 4;
    public bool NeedsBlood => lost >= 0.3f;

    void Update()
    {
        if (lost < 0.3f) return;
        Anima a = GetComponent<Anima>();
        if (a == null || a.death) return;
        a.sickness = Mathf.Min(1f, a.sickness + (lost - 0.25f) * 0.02f * Time.deltaTime);   // shock: empeora mientras no se repone
        a.stress = Mathf.Min(1f, a.stress + 0.01f * Time.deltaTime);
    }
}

/// <summary>
/// EL BANCO DE SANGRE DEL HOSPITAL (Berón 2026-11-03: «en el hospital debe existir un banco de sangre»; docs/hospital.md
/// §Banco de sangre). Un hospital aislado a −77,85° no recibe sangre de fuera: la guarda y la saca de su gente.
/// - ALMACÉN, cada producto a su temperatura (OMS, <i>Clinical Transfusion Process and Patient Safety</i>, 2010; Council of
///   Europe, <i>Guide to the preparation, use and quality assurance of blood components</i>, 21.ª ed., 2023; AABB Standards):
///   hematíes en nevera a 2–6 °C (SAGM: 42 días; sangre total CPDA-1: 35 días), plasma fresco congelado a ≤ −25 °C (36 meses;
///   descongelado, 5 días a 2–6 °C) y plaquetas a 20–24 °C en agitación continua (5–7 días).
/// - COMPATIBILIDAD ABO/Rh (hematíes: O− es el donante universal; plasma: AB es el donante universal).
/// - BANCO DE SANGRE ANDANTE: como en las bases antárticas y en la medicina militar (<i>walking blood bank</i>; Joint Trauma
///   System CPG «Whole Blood Transfusion», 2018), los donantes son el propio santuario, con su grupo conocido; una donación es
///   una unidad de ~450 mL y el donante espera 56 días (8 semanas) para la siguiente (Cruz Roja Americana).
/// - ANIMALES: perros, grupo DEA 1 (los DEA 1-negativos son los donantes «universales» de la primera transfusión); gatos, grupos
///   A/B/AB SIN donante universal (hay que tipar siempre: los B tienen anti-A fuerte) (Davidow 2013, <i>Vet Clin North Am Small
///   Anim Pract</i> 43:735; Kisielewicz y Self 2014, <i>J Small Anim Pract</i>). En los demás (oso, lobo, foca), sangre del
///   mismo animal o de su especie y fluidoterapia mientras.
/// La interacción: el puesto «Banco de sangre» (HospitalStation) cruza y transfunde al paciente de al lado; el sillón de
/// donante saca una unidad a quien se sienta; la ronda diaria (misión <c>hos_banco_sangre</c>) retira lo caducado, repone las
/// neveras del submarino-ambulancia y llama a donantes si falta O−.
/// </summary>
public class BloodBank : MonoBehaviour
{
    public enum Product { Hematies, Plasma, Plaquetas, SangreTotal }

    public class Unit
    {
        public Product product;
        public string group;      // «O−», «A+»… (personas), «DEA1−»… (perros), «A», «B» (gatos)
        public string species;    // «humano», «perro», «gato»
        public string donor;
        public int collectedDay, expiresDay;
        public float StorageC => product == Product.Plasma ? -30f : product == Product.Plaquetas ? 22f : 4f;
        public override string ToString() => $"{product} {group} ({species}, de {donor}, caduca el día {expiresDay})";
    }

    public static readonly int[] ShelfDays = { 42, 1095, 5, 35 };   // hematíes SAGM, plasma congelado, plaquetas, sangre total CPDA-1
    public const float UnitLitres = 0.45f;
    public const int DonorWaitDays = 56;

    public readonly List<Unit> stock = new List<Unit>();
    public readonly Dictionary<string, int> lastDonation = new Dictionary<string, int>();
    public int transfusions, donations, expired;

    public static BloodBank Instance { get; private set; }
    void Awake() { if (Instance == null) Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    void Start()
    {
        if (stock.Count > 0) return;
        // EXISTENCIAS DE PARTIDA: unas pocas de cada grupo frecuente y O− de sobra (urgencias, submarino); plasma AB; plaquetas.
        string[] g = { "O−", "O−", "O−", "O−", "O+", "O+", "O+", "A+", "A+", "A−", "B+", "AB+" };
        int d = Today;
        foreach (string x in g) Add(Product.Hematies, x, "humano", "reserva inicial", d - Random.Range(0, 10));
        Add(Product.Plasma, "AB", "humano", "reserva inicial", d - 30); Add(Product.Plasma, "AB", "humano", "reserva inicial", d - 30);
        Add(Product.Plaquetas, "O+", "humano", "reserva inicial", d);
        Add(Product.SangreTotal, "DEA1−", "perro", "Malamute donante", d - 3); Add(Product.SangreTotal, "DEA1−", "perro", "Malamute donante", d - 3);
        Add(Product.SangreTotal, "A", "gato", "gata donante", d - 2);
    }

    public Unit Add(Product p, string group, string species, string donor, int day)
    {
        var u = new Unit { product = p, group = group, species = species, donor = donor, collectedDay = day, expiresDay = day + ShelfDays[(int)p] };
        stock.Add(u);
        return u;
    }

    public int Count(Product p, string group = null, string species = "humano") => stock.FindAll(u => u.product == p && u.species == species && (group == null || u.group == group)).Count;

    /// <summary>Retira lo caducado (la ronda diaria). Devuelve cuántas unidades.</summary>
    public int Expire(int day)
    {
        int n = stock.RemoveAll(u => u.expiresDay < day);
        expired += n;
        if (n > 0) Debug.Log($"[Banco de sangre] Día {day}: {n} unidades caducadas retiradas; quedan {stock.Count}.");
        return n;
    }

    // ── GRUPOS ──────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Grupo ABO/Rh de una persona del santuario, estable por su nombre, con la frecuencia real (Cruz Roja Americana):
    /// O+ 38 %, A+ 34 %, B+ 9 %, O− 7 %, A− 6 %, AB+ 3 %, B− 2 %, AB− 1 %.</summary>
    public static string GroupOf(string who)
    {
        int h = 0; foreach (char c in who ?? "") h = h * 31 + c;
        int r = Mathf.Abs(h) % 100;
        return r < 38 ? "O+" : r < 72 ? "A+" : r < 81 ? "B+" : r < 88 ? "O−" : r < 94 ? "A−" : r < 97 ? "AB+" : r < 99 ? "B−" : "AB−";
    }

    /// <summary>Grupo de un animal: perros DEA 1 (un 40 % DEA 1-negativo), gatos A (la mayoría), B o AB; los demás, su especie.</summary>
    public static (string species, string group) AnimalGroup(Anima a)
    {
        string sp = a is Animal an ? an.SpeciesName : "humano";
        int h = 0; foreach (char c in a.name) h = h * 31 + c; int r = Mathf.Abs(h) % 100;
        switch (sp)
        {
            case "Wolf": case "Dog": case "Malamute": case "Husky": return ("perro", r < 40 ? "DEA1−" : "DEA1+");
            case "Cat": case "Fox": case "ArcticFox": return ("gato", r < 85 ? "A" : r < 97 ? "B" : "AB");
            default: return (sp, "su especie");
        }
    }

    /// <summary>¿Puede recibir <paramref name="recipient"/> hematíes de <paramref name="donor"/>? (ABO y Rh)</summary>
    public static bool RedCellsCompatible(string donor, string recipient)
    {
        if (donor == null || recipient == null) return false;
        bool dNeg = donor.EndsWith("−"), rNeg = recipient.EndsWith("−");
        if (dNeg == false && rNeg) return false;   // Rh+ a un Rh−: no
        string dA = donor.TrimEnd('+', '−'), rA = recipient.TrimEnd('+', '−');
        if (dA == "O") return true;
        if (dA == "AB") return rA == "AB";
        return rA == dA || rA == "AB";
    }

    /// <summary>¿Puede recibir <paramref name="recipient"/> plasma de <paramref name="donor"/>? (al revés: AB es el universal)</summary>
    public static bool PlasmaCompatible(string donor, string recipient)
    {
        string d = donor.TrimEnd('+', '−'), r = recipient.TrimEnd('+', '−');
        if (d == "AB") return true;
        if (r == "O") return true;
        return d == r;
    }

    static bool AnimalCompatible(Unit u, string species, string group)
    {
        if (u.species != species) return false;
        if (species == "perro") return u.group == "DEA1−" || u.group == group;   // DEA 1-negativo, a cualquiera (primera transfusión)
        if (species == "gato") return u.group == group;                         // sin universal: mismo grupo
        return true;
    }

    /// <summary>Volumen de sangre (L) de un paciente: 70 mL/kg en personas (ATLS) y aproximado por especie en animales.</summary>
    public static float BloodVolume(Anima a)
    {
        if (!(a is Animal an)) return 5f;
        float kg;
        switch (an.SpeciesName)
        {
            case "Bear": kg = 350f; break; case "PolarBear": kg = 450f; break; case "Wolf": kg = 40f; break; case "Fox": kg = 6f; break;
            case "Reindeer": kg = 140f; break; case "Seal": kg = 150f; break; case "Penguin": kg = 4.5f; break; case "EmperorPenguin": kg = 30f; break;
            case "SnowPetrel": kg = 0.32f; break; case "Walrus": kg = 1000f; break; case "MuskOx": kg = 300f; break; case "ArcticHare": kg = 4f; break; case "ArcticFox": kg = 3.3f; break;
            case "Orca": kg = 4000f; break; case "Whale": kg = 1200f; break; default: kg = 40f; break;
        }
        return kg * 0.07f;
    }

    /// <summary>Fracción del volumen que repone una unidad (~450 mL).</summary>
    public static float UnitFraction(Anima a) => UnitLitres / Mathf.Max(0.5f, BloodVolume(a));

    /// <summary>CRUZAR Y TRANSFUNDIR: hematíes (o sangre total en animales) compatibles hasta bajar de la clase II, y plasma 1:1 en
    /// la hemorragia masiva (≥ 40 %). Devuelve las unidades puestas; registra lo que no hay.</summary>
    public int Transfuse(Anima patient, string by, int maxUnits = 4)
    {
        if (patient == null) return 0;
        BloodLoss bl = patient.GetComponent<BloodLoss>();
        if (bl == null || bl.lost < 0.15f) { Debug.Log($"[Banco de sangre] «{patient.name}» no necesita sangre (pérdida {(bl != null ? bl.lost : 0f) * 100f:0} %)."); return 0; }
        bool human = !(patient is Animal);
        string group = human ? GroupOf(patient.name) : AnimalGroup(patient).group, species = human ? "humano" : AnimalGroup(patient).species;
        bool massive = bl.lost >= 0.4f;
        int given = 0; float per = UnitFraction(patient);
        while (given < maxUnits && bl.lost >= 0.15f)
        {
            Unit u = null;
            // El MISMO grupo primero; O− (o DEA 1−) solo si no hay (se guarda para las urgencias sin tipar).
            foreach (Unit x in stock) if ((x.product == Product.Hematies || x.product == Product.SangreTotal) && (human ? x.species == "humano" && x.group == group : AnimalCompatible(x, species, group) && x.group == group)) { u = x; break; }
            if (u == null) foreach (Unit x in stock) if ((x.product == Product.Hematies || x.product == Product.SangreTotal) && (human ? x.species == "humano" && RedCellsCompatible(x.group, group) : AnimalCompatible(x, species, group))) { u = x; break; }
            if (u == null) break;
            stock.Remove(u); given++; transfusions++;
            bl.lost = Mathf.Max(0f, bl.lost - per); bl.unitsReceived++; bl.lastProduct = u.ToString();
            if (massive && human)
            {
                Unit pl = stock.Find(x => x.product == Product.Plasma && x.species == "humano" && PlasmaCompatible(x.group, group));
                if (pl != null) { stock.Remove(pl); transfusions++; bl.unitsReceived++; }
            }
        }
        if (given == 0) Debug.LogWarning($"[Banco de sangre] No hay sangre compatible para «{patient.name}» ({species} {group}): banco de sangre andante — llamar a un donante de su grupo.");
        else Debug.Log($"[Banco de sangre] {by} cruza y transfunde {given} unidad(es) a «{patient.name}» ({species} {group}{(massive ? ", hemorragia masiva: plasma 1:1" : "")}): pérdida ahora {bl.lost * 100f:0} % (clase {bl.AtlsClass}).");
        return given;
    }

    /// <summary>DONAR: una unidad de sangre total de <paramref name="donor"/> (persona: se fracciona en hematíes y plasma), si han
    /// pasado 56 días desde su última donación. Devuelve el grupo o null.</summary>
    public string Donate(GameObject donor)
    {
        if (donor == null) return null;
        int d = Today;
        if (lastDonation.TryGetValue(donor.name, out int last) && d - last < DonorWaitDays) { Debug.Log($"[Banco de sangre] «{donor.name}» donó el día {last}: le faltan {DonorWaitDays - (d - last)} días."); return null; }
        Anima a = donor.GetComponent<Anima>();
        string group; string species;
        if (a is Animal) { var g = AnimalGroup(a); species = g.species; group = g.group; Add(Product.SangreTotal, group, species, donor.name, d); }
        else { species = "humano"; group = GroupOf(donor.name); Add(Product.Hematies, group, species, donor.name, d); Add(Product.Plasma, group.TrimEnd('+', '−'), species, donor.name, d); }
        lastDonation[donor.name] = d; donations++;
        Debug.Log($"[Banco de sangre] «{donor.name}» dona una unidad ({species} {group}): hematíes a la nevera (2–6 °C){(species == "humano" ? " y plasma al congelador (≤ −25 °C)" : "")}. Existencias: {stock.Count}.");
        return group;
    }

    /// <summary>LA RONDA DIARIA del banco (misión hos_banco_sangre): retira lo caducado, repone el submarino-ambulancia con O− y, si
    /// quedan menos de 4 O−, llama a un donante O− del santuario (el banco de sangre andante).</summary>
    public string DailyRound(string who)
    {
        int d = Today, gone = Expire(d);
        int refill = 0;
        foreach (HospitalSubmarine sub in HospitalSubmarine.All)
            while (sub != null && sub.bloodUnits < HospitalSubmarine.BloodCapacity)
            {
                Unit u = stock.Find(x => x.product == Product.Hematies && x.species == "humano" && x.group == "O−");
                if (u == null) break;
                stock.Remove(u); sub.bloodUnits++; refill++;
            }
        string called = null;
        if (Count(Product.Hematies, "O−") < 4)
            foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
            {
                if (a is Animal || a.death || a.CompareTag("Player") || GroupOf(a.name) != "O−") continue;
                if (lastDonation.TryGetValue(a.name, out int last) && d - last < DonorWaitDays) continue;
                called = a.name; Donate(a.gameObject); break;
            }
        string s = $"{who}: banco de sangre revisado: {gone} caducadas fuera, {refill} O− al submarino{(called != null ? $", dona {called} (O−)" : "")}; {stock.Count} unidades ({Count(Product.Hematies, "O−")} O−).";
        Debug.Log("[Banco de sangre] " + s);
        return s;
    }

    public string Summary()
    {
        var groups = new SortedDictionary<string, int>();
        foreach (Unit u in stock) { string k = $"{u.product} {u.group}{(u.species != "humano" ? " " + u.species : "")}"; groups[k] = groups.TryGetValue(k, out int n) ? n + 1 : 1; }
        var parts = new List<string>(); foreach (var kv in groups) parts.Add($"{kv.Key}: {kv.Value}");
        return string.Join(" · ", parts);
    }
}

