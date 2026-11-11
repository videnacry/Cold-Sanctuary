using System.Collections.Generic;

/// <summary>
/// LA LISTA RÁPIDA, COMO DATOS EN C# (el repositorio solo guarda .cs y documentos). Una unidad por entrada: clase, grupo y segundos en la
/// última batería completa. La reescribe «Tools/Cold Sanctuary/Pruebas/Rehacer la lista rápida» (TestImpactTool.RefreshRapida).
/// </summary>
public static class TestPresetsData
{
    // Prueba rápida: unidades de menos de 60 s en la última batería completa (units38.txt): 45 unidades, 123 s.
    public static readonly (string unit, int group, float seconds)[] Rapida =
    {
        ("AereoTest", 110, 28f),
        ("AlertaPisoNuevoTest", 76, 1f),
        ("AlimentadorTest", 105, 0f),
        ("AsambleaTest", 77, 0f),
        ("BodyMetabolismTest", 16, 6f),
        ("BodyShapeTest", 26, 0f),
        ("BruceLeeStoryTest", 60, 27f),
        ("CadenaAlimenticiaTest", 85, 0f),
        ("CapacidadPisosTest", 75, 0f),
        ("ChecklistAuto", 20, 0f),
        ("ChecklistMagicTest", 21, 0f),
        ("ChecklistProgressTest", 24, 0f),
        ("ChecklistSoulTest", 22, 1f),
        ("DietPreferenceTest", 10, 0f),
        ("EmergenceAuto", 2, 0f),
        ("EstrusTest", 4, 0f),
        ("FaunaChecks", 1, 0f),
        ("FaunaFriaTest", 111, 0f),
        ("GardenVisitorTest", 27, 11f),
        ("GrimoireTest", 3, 0f),
        ("GuardRoomTest", 91, 0f),
        ("HistoriasDatosTest", 78, 0f),
        ("HudModalsTest", 93, 0f),
        ("IcebergVidaTest", 108, 0f),
        ("IdealStateTest", 13, 0f),
        ("InformaticaAulaTest", 74, 0f),
        ("IslandsTest", 25, 0f),
        ("KushalVoiceTest", 52, 0f),
        ("LifeCycleAuto", 5, 0f),
        ("LoteEstaticoTest", 112, 11f),
        ("MicroBuildRulesTest", 63, 0f),
        ("ObservationTest", 11, 0f),
        ("PakalStoryTest", 66, 16f),
        ("PersonajesReglasTest", 95, 0f),
        ("PetrelTest", 109, 5f),
        ("ProduccionTest", 103, 5f),
        ("RiverCurrentTest", 9, 0f),
        ("SensesTest", 14, 0f),
        ("SicknessTest", 6, 0f),
        ("SleepTest", 8, 0f),
        ("StarvationTest", 7, 0f),
        ("StateSpellsTest", 12, 0f),
        ("TerritorioTest", 99, 6f),
        ("TraceFieldTest", 0, 6f),
        ("YogaTest", 15, 0f),
        ("TestPipelineTest", 17, 0f),
    };
}
