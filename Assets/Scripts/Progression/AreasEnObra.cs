using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ÁREAS EN OBRA (Berón 2026-11-09; docs/areas-en-obra.md): para que haya menos clones, solo las áreas BÁSICAS quedan terminadas. Son los servicios
/// básicos de humanos y animales (hospital, cocina, guardia, huerto, cría, energía y lo que sostiene la vida), la construcción misma, el gimnasio y el yoga.
/// Las demás vuelven a ser OBRAS (<see cref="ConstructionProject"/> con <see cref="ConstructionProject.reform"/>, con su forma en construcción a la vista: F1):
///  · no dan tareas de área (<see cref="Filter"/>), salvo lo que sostiene la vida o la obra (el O₂ de los fotobiorreactores, el material de obra);
///  · sus horas pasan a la obra: 1 h de material y un equipo de 2 h por turno, tantos turnos como sus horas pidan (<see cref="CrewFor"/>);
///  · esas tareas de obra son OPCIONALES: las hace quien tenga tiempo en el reparto por partes iguales; nunca piden un clon.
/// La lista se decide por «necesidad básica de humanos o animales». Lógica pura salvo <see cref="Apply"/> (la escena).
/// </summary>
public static class AreasEnObra
{
    /// <summary>LAS BÁSICAS (por área entera, <see cref="WorkRoster.AreaGroup"/>). Carne Cultivada: alimenta a los depredadores. Mecánica: las alfombras,
    /// el transporte de todos (INFERIDO). La planta nuclear y la central: la energía. La sala de guardia y la línea: la seguridad.</summary>
    public static readonly HashSet<string> Basic = new HashSet<string>
    {
        AreaWork.Hospital, "Cocina", AreaWork.Guard, AreaWork.GuardRoom, "Huerto", "Cría", "Carne Cultivada", "Mecánica", "Construcción",
        "Gimnasio", "Sala de Yoga", EnergyRule.Area, NuclearSite.PlantArea, NuclearSite.PowerArea, SiteLine.AreaName,
        VerticalFarm.AreaName,   // (ronda 31) el Huerto vertical: las estanterías LED que eligió Berón, comida
    };
    public static bool IsBasic(string area) => area != null && Basic.Contains(WorkRoster.AreaGroup(area));

    /// <summary>Lo que se sigue haciendo aunque su área esté en obra: el O₂ (fotobiorreactores, «bio_fotobio») y el material de obra («mat_»).</summary>
    public static bool KeepsWhileEnObra(WorkRoster.Mission m) => m != null && m.id != null && (m.id.StartsWith("mat_") || m.id.StartsWith(FotobioFacade.MissionId));

    /// <summary>Las tareas de hoy sin las de las áreas en obra (<paramref name="enObra"/>: áreas enteras), salvo <see cref="KeepsWhileEnObra"/>.</summary>
    public static List<WorkRoster.Mission> Filter(IEnumerable<WorkRoster.Mission> todays, ICollection<string> enObra)
        => todays.Where(m => !enObra.Contains(WorkRoster.AreaGroup(m.area)) || KeepsWhileEnObra(m)).ToList();

    /// <summary>Turnos de 2 h del equipo de la obra para un área de <paramref name="areaHours"/> h al día: con la hora de material, tantas horas como el área.</summary>
    public static int CrewFor(float areaHours) => System.Math.Max(1, Mathf.RoundToInt((areaHours - 1f) / 2f));

    /// <summary>Las horas obligadas de un área en el catálogo fijo (las que pasan a la obra).</summary>
    public static float AreaHours(string area) => AreaWork.Catalog.Where(m => WorkRoster.AreaGroup(m.area) == WorkRoster.AreaGroup(area) && !m.optional).Sum(m => m.hours);

    /// <summary>Días de obra de una reforma (INFERIDO: los del Dojo y el Ballet).</summary>
    public const int Days = 6;

    /// <summary>Las pruebas lo encienden para convertir la escena; si no, con las pruebas no se toca nada.</summary>
    public static bool InTests;

    /// <summary>EN LA ESCENA: cada área no básica sin obra se vuelve obra (reforma): su forma en construcción a la vista y sus días guardados. Las salas de
    /// entrenamiento (gimnasio, ballet, dojo, calistenia) y el yoga no se tocan. Devuelve cuántas.</summary>
    public static int Apply()
    {
        if ((ColdSanctuary.TestRunner.Running || ColdSanctuary.TestRunner.Requested) && !InTests) return 0;
        int n = 0;
        var names = new List<string>();
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            if (a == null || IsBasic(a.displayName) || a.areaType == SanctuaryAreaType.YogaRoom || a.areaType == SanctuaryAreaType.Gym || a.areaType == SanctuaryAreaType.Ballet
                || a.areaType == SanctuaryAreaType.MartialArts || a.areaType == SanctuaryAreaType.Calisthenics || a.GetComponent<ConstructionProject>() != null) continue;
            ConstructionProject cp = a.gameObject.AddComponent<ConstructionProject>();
            cp.title = a.displayName; cp.reform = true; cp.daysRequired = Days; cp.crewNeeded = CrewFor(AreaHours(a.displayName));
            n++; names.Add($"{a.displayName} (equipo de {cp.crewNeeded})");
        }
        if (n > 0) Debug.Log($"[Obras] Áreas no básicas en obra (Berón 2026-11-09): {string.Join(", ", names)}.");
        return n;
    }
}
