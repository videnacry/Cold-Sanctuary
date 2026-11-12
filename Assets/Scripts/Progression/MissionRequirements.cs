using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS REQUISITOS DE CADA MISIÓN (Berón 2026-11-05; docs/jornada-y-rotacion.md §Requisitos): antes de asignar una misión se mira si se puede hacer. Dos clases:
/// - DEL MUNDO (<see cref="Blocked"/>): herramientas, materiales, estado del área y seguridad. Si falla, HOY no se le da a nadie, y queda anotada con su motivo.
/// - DE LA PERSONA (<see cref="NeedsLesson"/>): las tareas con riesgo o técnica piden haberlas aprendido (o ser de su profesión). Las demás se aprenden haciéndolas, como siempre.
/// La regla de oro no cambia: todos acaban sabiendo hacer de todo y ninguna tarea es de nadie. El conocimiento es una etapa, y la Magnate (que lo sabe todo) cubre las obligatorias mientras tanto.
/// </summary>
public static class MissionRequirements
{
    public enum Kind { Conocimiento, Herramientas, Materiales, EstadoDelArea, Seguridad }

    public sealed class Req
    {
        public Kind kind; public string text; public System.Func<bool> ok;
        public Req(Kind k, string t, System.Func<bool> f) { kind = k; text = t; ok = f; }
    }

    /// <summary>Prefijos de las tareas que piden lección: un error ahí hace daño a alguien o rompe algo caro.</summary>
    static readonly string[] Lesson =
    {
        "hos_quirofano", "hos_uci", "hos_urgencias", "hos_veterinaria", "hos_aves", "enf_visita", "enf_iniciacion", "enf_animales",
        "pla_", "nuc_", "fis_", "qui_", "bio_", "lab_", "ifo_", "lanz_", "lan_", "sol_", "ene_", "mec_", "gua_",
    };

    public static bool NeedsLesson(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        foreach (string p in Lesson) if (id.StartsWith(p)) return true;
        return false;
    }

    /// <summary>Qué se aprende: las misiones de una misma familia (una por obra, por elaboración, por turno de corrector…) se aprenden de una vez.</summary>
    public static string KnowledgeKey(string id)
    {
        if (string.IsNullOrEmpty(id)) return "";
        foreach (string f in new[] { "obra_material_", "obra_trabajo_", "prep_", "rep_", "jar_frutales", "yog_corrige" }) if (id.StartsWith(f)) return f;
        return id;
    }

    /// <summary>Los requisitos del mundo de una misión. Las que no salen aquí solo piden que su área no esté en obras.</summary>
    public static List<Req> Of(WorkRoster.Mission m)
    {
        var l = new List<Req>();
        string id = m.id ?? "";
        // ESTADO DEL ÁREA (regla J29): nada en un piso en obras, salvo la propia obra.
        if (m.site == null && !id.StartsWith("obra_"))
            l.Add(new Req(Kind.EstadoDelArea, $"«{m.area}» no está en obras", () => !UnderWorks(m.area)));
        // SEGURIDAD: con el sitio nuclear en emergencia no se opera ningún reactor ni el ciclotrón; la vigilancia y lo pasivo, sí.
        if (id == "pla_operar" || id == "pla_isotopos" || id == "pla_ciclotron" || id == "nuc_turno")
            l.Add(new Req(Kind.Seguridad, "el sitio nuclear no está en emergencia", () => NuclearSite.Instance == null || NuclearSite.Instance.State == NuclearSite.Alert.Normal));
        // HERRAMIENTAS: el equipo sin el que no hay nada que hacer.
        if (id.StartsWith("ifo_")) l.Add(new Req(Kind.Herramientas, "el clúster de IA está instalado", () => AiCluster.Instance != null));
        if (id.StartsWith("lab_")) l.Add(new Req(Kind.Herramientas, "hay un biorreactor", () => CulturedMeatLab.All.Count > 0 || !Scene()));
        // MATERIALES.
        if (id == "sol_instalacion") l.Add(new Req(Kind.Materiales, "quedan plazas y mesas por montar", () => EnergySite.Instance == null || EnergySite.Instance.Pending > 0));
        return l;
    }

    /// <summary>Por qué no se puede hacer hoy (null si se puede).</summary>
    public static string Blocked(WorkRoster.Mission m)
    {
        foreach (Req r in Of(m)) if (!r.ok()) return $"{r.kind}: hace falta que {r.text}";
        return null;
    }

    static bool Scene() => WorkdayDirector.AreaTransform("Carne Cultivada") != null;

    static bool UnderWorks(string area)
    {
        Transform t = WorkdayDirector.AreaTransform(area);
        if (t == null) return false;
        ConstructionProject cp = t.GetComponent<ConstructionProject>();
        return cp != null && !cp.Finished;
    }
}
