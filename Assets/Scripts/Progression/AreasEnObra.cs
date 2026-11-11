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

    /// <summary>Lo que se sigue haciendo aunque su área esté en obra: el O₂ (fotobiorreactores, «bio_fotobio»), el material de obra («mat_») y la
    /// SEGURIDAD de la lanzadera (el diluvio y la trinchera, el remolque de un iceberg y el vaciado de combustible: Control de misión, 2026-11-09).</summary>
    public static bool KeepsWhileEnObra(WorkRoster.Mission m) => m != null && m.id != null && (m.id.StartsWith("mat_") || m.id.StartsWith(FotobioFacade.MissionId) || SafetyIds.Contains(m.id));
    static readonly HashSet<string> SafetyIds = new HashSet<string> { "lanz_seguridad", "lanz_vaciado", Iceberg.LaunchTow };

    /// <summary>Dónde se hace la obra de un área: en la tierra, en Construcción (su equipo); en el Marino o el Aéreo, en el área misma, para que la
    /// hagan los que pasan allí el día (Control de misión: los de s3).</summary>
    public static string ObraArea(string title) => SanctuaryPlacement.SanctuaryOfArea(title) == SanctuaryPlacement.Land ? "Construcción" : title;

    /// <summary>LAS ÁREAS DE OTRAS ESCENAS que también vuelven a obra (Berón 2026-11-09): Control de misión vive en la escena aérea, que no está cargada;
    /// su obra se lleva desde aquí, en un objeto propio, y su forma en construcción se enseña al cargar esa escena (<see cref="SyncLoaded"/>).</summary>
    public static readonly string[] RemoteAreas = { LaunchSite.ControlArea };

    /// <summary>Las tareas de hoy sin las de las áreas en obra (<paramref name="enObra"/>: áreas enteras), salvo <see cref="KeepsWhileEnObra"/>.</summary>
    public static List<WorkRoster.Mission> Filter(IEnumerable<WorkRoster.Mission> todays, ICollection<string> enObra)
        => todays.Where(m => !enObra.Contains(WorkRoster.AreaGroup(m.area)) || KeepsWhileEnObra(m)).ToList();

    /// <summary>(2026-11-11) ¿Cae <paramref name="p"/> sobre un área EN OBRA (una obra sin acabar)? Ahí no va nadie más que la obra: ni rondas ni
    /// actividades (Goluis, David y Gohageneis daban vueltas en «Sala de asambleas», ya en obra). El agua del tronco de un árbol es de todos, no del piso.</summary>
    public static bool InObra(Vector3 p)
    {
        foreach (ArbolCampos t in ArbolCampos.All) if (t != null && t.InRootWater(p)) return false;
        foreach (ConstructionProject cp in ConstructionProject.All)
            if (cp != null && !cp.Finished && cp.TryGetComponent(out SanctuaryArea a) && a.Contains(p, 0.5f)) return true;
        return false;
    }

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
        foreach (string remote in RemoteAreas)
        {
            if (IsBasic(remote) || ConstructionProject.All.Any(p => p != null && p.title == remote)) continue;
            var go = new GameObject($"Obra_{remote}");
            if (AereoLink.Instance != null) go.transform.SetParent(AereoLink.Instance.transform, false);   // (en el muelle aéreo: desde allí se vuela a su obra)
            ConstructionProject rp = go.AddComponent<ConstructionProject>();
            rp.title = remote; rp.reform = true; rp.daysRequired = Days; rp.crewNeeded = CrewFor(AreaHours(remote));
            n++; names.Add($"{remote} (en otra escena; equipo de {rp.crewNeeded})");
        }
        if (n > 0) Debug.Log($"[Obras] Áreas no básicas en obra (Berón 2026-11-09): {string.Join(", ", names)}.");
        return n;
    }

    /// <summary>Al cargar otra escena (la aérea): las áreas que allí vuelven a obra enseñan su forma en construcción mientras su obra (la de aquí) no
    /// esté terminada. Si el área no tiene <see cref="FloorForms"/>, se dice.</summary>
    public static void SyncLoaded()
    {
        foreach (SanctuaryArea a in Object.FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            if (a == null || System.Array.IndexOf(RemoteAreas, a.displayName) < 0) continue;
            ConstructionProject cp = ConstructionProject.All.FirstOrDefault(p => p != null && p.reform && p.title == a.displayName);
            if (cp == null) continue;
            if (a.TryGetComponent(out FloorForms ff)) ff.ShowConstruction(!cp.Finished);
            else Debug.LogWarning($"[Obras] «{a.displayName}» está en obra pero su piso no tiene FloorForms: no se puede enseñar en obras.");
        }
    }
}
