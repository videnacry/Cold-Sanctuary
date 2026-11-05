using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// LA TANDA 3 DE MICROCOSMOS DE HISTORIAS (docs/microcosmos-historias-3.md; Berón 2026-11-05). Cada historia vive en SU archivo de esta
/// carpeta, con sus tres partes: la escena (<see cref="StorySceneBuilder"/>), el pueblo con su plano (<see cref="CivicBuilder"/>) y su flora y
/// fauna (<see cref="MicroBiome"/>); el mapa a mano va en <c>MicroStoryMaps.Historias.cs</c> (código de juego). Aquí, los menús (uno por
/// escena) y el reparto por nombre. Figuras reales: escenas civiles y humanas, sin violencia gloriosa ni juicio político; lo discutido,
/// en una línea de la ficha.
/// </summary>
public static partial class StorySceneBuilder
{
    internal static readonly string[] Historias3Scenes =
    {
        "Microcosmos_EloyAlfaro", "Microcosmos_Galvarino", "Microcosmos_Atahualpa", "Microcosmos_GranLeyDeLaPaz", "Microcosmos_Cleopatra", "Microcosmos_Langley",
        "Microcosmos_Beethoven", "Microcosmos_Descartes", "Microcosmos_CheGuevara", "Microcosmos_PedroSanchez", "Microcosmos_Lincoln", "Microcosmos_Hazanas",
    };

    /// <summary>Genera las doce, una tras otra (con la escena principal abierta). Con poca memoria, mejor una a una con su menú.</summary>
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3 (todos, uno tras otro)")]
    public static void BuildHistorias3() { foreach (string sc in Historias3Scenes) { try { BuildOne(sc); } catch (System.Exception e) { Debug.LogWarning($"[Historias] {sc}: {e.Message}"); } } }

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Eloy Alfaro (Chimbacalle, 1908)")] public static void BuildEloyAlfaro() => BuildOne("Microcosmos_EloyAlfaro");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Galvarino (Arauco, 1557)")] public static void BuildGalvarino() => BuildOne("Microcosmos_Galvarino");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Atahualpa (Cajamarca, 1533)")] public static void BuildAtahualpa() => BuildOne("Microcosmos_Atahualpa");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Gran Ley de la Paz (Onondaga)")] public static void BuildGranLey() => BuildOne("Microcosmos_GranLeyDeLaPaz");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Cleopatra (Alejandría, 42 a. C.)")] public static void BuildCleopatra() => BuildOne("Microcosmos_Cleopatra");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Las calculadoras de Langley (1962)")] public static void BuildLangley() => BuildOne("Microcosmos_Langley");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Beethoven (Viena, 1824)")] public static void BuildBeethoven() => BuildOne("Microcosmos_Beethoven");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Descartes (Leiden, 1637)")] public static void BuildDescartes() => BuildOne("Microcosmos_Descartes");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Ernesto Guevara (San Pablo, 1952)")] public static void BuildCheGuevara() => BuildOne("Microcosmos_CheGuevara");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Pedro Sánchez (Madrid, 2018)")] public static void BuildPedroSanchez() => BuildOne("Microcosmos_PedroSanchez");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Lincoln (Gettysburg, 1863)")] public static void BuildLincoln() => BuildOne("Microcosmos_Lincoln");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias 3/Grandes hazañas de presidentes (feria)")] public static void BuildHazanas() => BuildOne("Microcosmos_Hazanas");
}

internal static partial class CivicBuilder
{
    /// <summary>Los planos de la tanda 3 (mismo formato que <c>PlanText</c>; cada uno en el archivo de su historia).</summary>
    static readonly Dictionary<string, string> HistoPlans = new Dictionary<string, string>
    {
        ["Chimbacalle"] = PlanChimbacalle, ["Lagunillas"] = PlanLagunillas, ["Cajamarca"] = PlanCajamarca, ["Onondaga"] = PlanOnondaga, ["Alejandría de Cleopatra"] = PlanAlejandria,
        ["Langley"] = PlanLangley, ["Viena 1824"] = PlanViena, ["Leiden"] = PlanLeiden, ["San Pablo"] = PlanSanPablo, ["Madrid 2018"] = PlanMadrid, ["Gettysburg"] = PlanGettysburg, ["Feria de las Hazañas"] = PlanFeria,
    };

    /// <summary>El pueblo de cada historia de la tanda 3 (0 si <paramref name="display"/> no es una de ellas).</summary>
    static int HistoTown(Transform t, string display)
    {
        switch (display)
        {
            case "Chimbacalle": return ChimbacalleTown(t);
            case "Lagunillas": return LagunillasTown(t);
            case "Cajamarca": return CajamarcaTown(t);
            case "Onondaga": return OnondagaTown(t);
            case "Alejandría de Cleopatra": return AlejandriaTown(t);
            case "Langley": return LangleyTown(t);
            case "Viena 1824": return VienaTown(t);
            case "Leiden": return LeidenTown(t);
            case "San Pablo": return SanPabloTown(t);
            case "Madrid 2018": return MadridTown(t);
            case "Gettysburg": return GettysburgTown(t);
            case "Feria de las Hazañas": return FeriaTown(t);
            default: return 0;
        }
    }
}

public static partial class MicroBiome
{
    /// <summary>La flora y la fauna de cada historia de la tanda 3 (null si no es una de ellas).</summary>
    static Biome HistoBiome(string display)
    {
        switch (display)
        {
            case "Chimbacalle": return ChimbacalleBiome();
            case "Lagunillas": return LagunillasBiome();
            case "Cajamarca": return CajamarcaBiome();
            case "Onondaga": return OnondagaBiome();
            case "Alejandría de Cleopatra": return AlejandriaBiome();
            case "Langley": return LangleyBiome();
            case "Viena 1824": return VienaBiome();
            case "Leiden": return LeidenBiome();
            case "San Pablo": return SanPabloBiome();
            case "Madrid 2018": return MadridBiome();
            case "Gettysburg": return GettysburgBiome();
            case "Feria de las Hazañas": return FeriaBiome();
            default: return null;
        }
    }
}
