using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// LA MAQUINARIA DE LAS ÁREAS DE PRODUCCIÓN (Berón 2026-11-05: «la maquinaria debe actualizarse para poder abastecer la producción requerida y esto es una regla de
/// las áreas de producción»; manual, regla P19; docs/produccion-maquinaria.md). Lo que un área produce sale de sus MÁQUINAS, no de un número: cada línea dice qué
/// necesita el santuario al día y qué da cada máquina de cada etapa, cuenta las que hay en la escena y su capacidad es la de la etapa más corta (el cuello de botella).
/// Se comprueba al final del Blockout y en el grupo 103; toda área con <see cref="AreaProducer"/> debe tener línea o estar declarada como exenta y por qué.
/// </summary>
public static class ProductionAudit
{
    public class Stage
    {
        public string label, prefix; public float perUnit;
        /// <summary>(2026-11-11) Etiqueta y capacidad que dependen de la maquinaria construida (los litros de los biorreactores).</summary>
        public System.Func<string> labelF; public System.Func<float> perF;
        public Stage(string label, string prefix, float perUnit) { this.label = label; this.prefix = prefix; this.perUnit = perUnit; }
        public Stage(System.Func<string> label, string prefix, System.Func<float> perUnit) { labelF = label; this.prefix = prefix; perF = perUnit; }
        public string Label => labelF != null ? labelF() : label;
        public float PerUnit => perF != null ? perF() : perUnit;
    }

    public class Line
    {
        public string area, product, unit;
        public System.Func<float> demand;
        public Stage[] stages;
        /// <summary>Capacidad medida de otra forma que contando piezas (la superficie de los bancales).</summary>
        public System.Func<float> custom;
        public string customLabel = "superficie";
    }

    public class Result
    {
        public string area, product, unit, bottleneck;
        public float demand, capacity;
        public bool Ok => capacity >= demand;
        public override string ToString() => $"{area}: {product}: pide {demand:0.#} {unit}/día, da {capacity:0.#} ({bottleneck}) {(Ok ? "bien" : "FALTA MAQUINARIA")}";
    }

    /// <summary>La carne al día del PLAN de depredadores de tierra (PopulationPlan.Cap, que escala con K × humanos) × lo que come cada uno.</summary>
    public static float PlannedPredatorKg()
    {
        float plan = 0f;
        foreach (string sp in new[] { "Bear", "Wolf", "ArcticFox" }) plan += PopulationPlan.Cap(sp) * (FoodChain.Intake.TryGetValue(sp, out float i) ? i : 1f);
        return plan;
    }

    /// <summary>Lo que necesitan al día los depredadores de tierra: el censo real en Play y, al construir, el plan.</summary>
    public static float PredatorKgPerDay()
    {
        // Para el techo de reproducción (1,5×), no para los que hay hoy: la maquinaria tiene que aguantar lo que pueden llegar a ser. Si el censo real lo pasa, manda el censo.
        float plan = PlannedPredatorKg();
        if (!Application.isPlaying) return plan;
        float kg = 0f;
        foreach (Animal a in Object.FindObjectsByType<Animal>(FindObjectsSortMode.None))
            if (PredatorRound.Served(a) && !PredatorRound.Orphan(a)) kg +=   // (2026-11-11) las crías huérfanas toman biberón, no carne: contadas, la demanda subió a 76,5 kg (ronda 49)
                FoodChain.Intake.TryGetValue(a.SpeciesName, out float i) ? i : 1f;
        return Mathf.Max(plan, kg);
    }

    // Capacidades por máquina: [A] valores de referencia de diseño (docs/produccion-maquinaria.md). La carne sale de las cuentas del propio laboratorio.
    public static readonly Line[] Lines =
    {
        new Line { area = "Carne Cultivada", product = "carne cultivada", unit = "kg", demand = () => PredatorKgPerDay() + 2f, stages = new[] {
            new Stage(() => $"biorreactores de {CulturedMeatLab.ReactorLitres:0} L", "Biorreactor_produccion", () => CulturedMeatLab.ReactorLitres * CulturedMeatLab.GramsPerLitre / 1000f),
            new Stage(() => "perfusión con andamio", "Biorreactor_andamio", () => CulturedMeatLab.ScaffoldKg),
            new Stage("centrífugas", "Centrifuga", 40f), new Stage("formadoras", "Formadora", 40f) } },
        new Line { area = "Cocina", product = "comidas", unit = "comidas", demand = () => PopulationPlan.Humans * 3f, stages = new[] { new Stage("equipos de cocina", "Equipo_", 10f) } },
        // (2026-11-08) Para las 32 personas planeadas (PopulationPlan.People): los bancales del Árbol y los estantes LED del Huerto vertical (VerticalFarm).
        new Line { area = "Huerto", product = "verdura", unit = "kg", demand = () => VerticalFarm.DemandKgDay, custom = () => VerticalFarm.FieldsKgDay() + VerticalFarm.CapacityKgDay(), customLabel = "bancales y estantes LED" },
        new Line { area = "Estudio Textil", product = "prendas", unit = "prendas", demand = () => PopulationPlan.Humans * 6f / 365f, stages = new[] { new Stage("telares", "Telar_", 0.5f) } },
        // Lo que el reactor da todo el día contra lo que piden los pisos en su punta (FloorEnergy; la regla completa, con paneles y baterías, es EnergyRule.Check).
        new Line { area = "Energía", product = "electricidad en punta", unit = "kWh", demand = () => EnergyRule.DemandKW() * EnergyRule.PeakFactor * 24f, custom = () => EnergyRule.ReactorKW * 24f, customLabel = "reactor de 5 MWe" },
    };

    /// <summary>Áreas con <see cref="AreaProducer"/> que no hacen un producto físico que se pueda contar. Con su porqué.</summary>
    public static readonly Dictionary<string, string> Exempt = new Dictionary<string, string>
    {
        { "VehicleWorkshop_Area", "repara: da servicio, no un producto; sus puestos los mide la regla S13" },
        { "Infirmary_Area", "investigación y salud; los puestos los mide H2" },
        { "AtomicPlant_Area", "investigación" }, { "Chemistry_Area", "investigación" }, { "Biology_Area", "investigación" },
    };

    static int Count(string prefix) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Count(t => t.name.StartsWith(prefix) && t.GetComponentInParent<Animal>() == null);

    public static Result Check(Line l)
    {
        var r = new Result { area = l.area, product = l.product, unit = l.unit, demand = l.demand() };
        if (l.custom != null) { r.capacity = l.custom(); r.bottleneck = l.customLabel; return r; }
        r.capacity = float.MaxValue;
        foreach (Stage s in l.stages)
        {
            float cap = Count(s.prefix) * s.PerUnit;
            if (cap < r.capacity) { r.capacity = cap; r.bottleneck = s.Label; }
        }
        return r;
    }

    public static List<Result> Audit() => Lines.Select(Check).ToList();

    /// <summary>Áreas de producción sin línea ni exención: un área nueva no se queda sin revisar.</summary>
    public static List<string> Unlisted()
    {
        var known = new HashSet<string> { "Kitchen_Area", "Garden_Area", "TextileStudio_Area", "CulturedMeatLab_Area", "NuclearPower_Area" };
        var l = new List<string>();
        foreach (AreaProducer p in Object.FindObjectsByType<AreaProducer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!known.Contains(p.name) && !Exempt.ContainsKey(p.name)) l.Add(p.name);
        return l;
    }

    /// <summary>Lo que escribe el Blockout. Devuelve cuántas líneas fallan.</summary>
    public static int Log()
    {
        var sb = new StringBuilder("[Producción] La maquinaria de las áreas de producción (regla P19):\n");
        int bad = 0;
        foreach (Result r in Audit()) { if (!r.Ok) bad++; sb.AppendLine("  " + r); }
        foreach (string u in Unlisted()) { bad++; sb.AppendLine($"  {u}: área de producción SIN LÍNEA: añádela a ProductionAudit.Lines o a Exempt."); }
        if (bad > 0) Debug.LogWarning(sb.ToString()); else Debug.Log(sb.ToString());
        return bad;
    }
}
