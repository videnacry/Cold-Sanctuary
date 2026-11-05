using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEST DE DATOS DE LA TANDA 3 DE HISTORIAS (docs/microcosmos-historias-3.md; grupo 78). Datos puros, sin cargar escenas: por cada una de las doce,
/// su piso en <see cref="MicroFloors.Table"/> (representativo o «pendiente»), su fecha y lugar (<see cref="Eras.OfMicrocosmos"/>), su región y
/// su entrada de estilo propia, su mapa a mano HECHO con flora, fauna, órdenes sobre el piso y edificios, su lugar de entrenar de la época con
/// tradición y sesiones, sus frutales y la luz (<see cref="SolarPosition.GeoOf"/>). Y los juicios: Onondaga sin manzanos, Madrid sin nada militar.
/// </summary>
public class HistoriasDatosTest : MonoBehaviour, ITestUnit
{
    public int Group => 78;
    public bool ParallelSafe => true;

    public static readonly (string display, string platform, bool rep, int year, string region, string entry, string training)[] Cases =
    {
        ("Chimbacalle", "VehicleWorkshop_Area", false, 1908, "Andes", "andes_republicano", "pelota_nacional"),
        ("Lagunillas", "Infirmary_Area", true, 1557, "Mapuche", "mapuche", "palin"),
        ("Cajamarca", "TextileStudio_Area", false, 1532, "Inca", "inca", "warachikuy"),
        ("Onondaga", "Classroom_Area", false, 1450, "Haudenosaunee", "haudenosaunee", "tewaarathon"),
        ("Alejandría de Cleopatra", "Kitchen_Area", true, -42, "EgiptoPtolemaico", "egipto_ptolemaico", "gimnasio_griego"),
        ("Langley", "Informatics_Area", true, 1962, "VirginiaTidewater", "virginia_1960", "gimnasio_moderno"),
        ("Viena 1824", "Ballet_Area", false, 1824, "Viena", "viena_biedermeier", "sala_esgrima"),
        ("Leiden", "Classroom_Area", true, 1637, "ProvinciasUnidas", "provincias_1600", "sala_esgrima"),
        ("San Pablo", "Infirmary_Area", true, 1952, "Amazonia", "amazonia_1950", "estadio"),
        ("Madrid 2018", "Classroom_Area", false, 2018, "EspanaActual", "espana_actual", "gimnasio_moderno"),
        ("Gettysburg", "Classroom_Area", false, 1863, "Norteamerica", "norteamerica_victoriana", "turnhalle"),
        ("Feria de las Hazañas", "Classroom_Area", false, 2026, "Exposicion", "exposicion_mundial", "gimnasio_moderno"),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<HistoriasDatosTest>() == null) tr.gameObject.AddComponent<HistoriasDatosTest>();
    }

    public IEnumerator Run()
    {
        foreach (var c in Cases)
        {
            TestProbe.Begin($"Historias 3 (datos): {c.display}");
            MicroFloors.Match f = MicroFloors.Of(c.display);
            TestProbe.Check($"su piso es {c.platform}, {(c.rep ? "representativo" : "pendiente")}", f != null && f.platform == c.platform && f.representative == c.rep, f != null ? $"{f.platform}: «{f.pending}»" : "sin fila");

            var era = Eras.OfMicrocosmos(c.display);
            TestProbe.Check($"su año es {Eras.Format(c.year)}", era.HasValue && era.Value.year == c.year, era.HasValue ? $"{era.Value.year}" : "sin fecha");
            MicroStyles.Region region = MicroStyles.RegionOf(c.display);
            TestProbe.Check($"su región es {c.region}", region.ToString() == c.region, region.ToString());
            MicroStyles.Entry e = MicroStyles.For(c.year, region);
            TestProbe.Check($"su estilo es {c.entry}", e != null && e.id == c.entry, e != null ? e.id : "sin entrada");

            MicroStoryMaps.Map m = MicroStoryMaps.For(c.display);
            TestProbe.Check("su mapa a mano está HECHO, con ≥ 40 plantas, ≥ 8 bichos, ≥ 2 órdenes sobre el piso y ≥ 4 edificios", MicroStoryMaps.Authored(c.display) && m.PlantCount >= 40 && m.AnimalCount >= 8 && m.floor.Length >= 2 && m.buildings.Length >= 4,
                m != null ? $"{m.PlantCount} plantas, {m.AnimalCount} bichos, {m.floor.Length} órdenes, {m.buildings.Length} edificios" : "sin mapa");

            TestProbe.Check("sus cuatro categorías tienen nombre de época: ocio, entrenamiento y recuperación", e != null && MicroStyles.CivicName(MicroStyles.Civic.Plaza, e, c.year) != null
                && MicroStyles.CivicName(MicroStyles.Civic.Entrenamiento, e, c.year) != null && (MicroStyles.CivicName(MicroStyles.Civic.Sanacion, e, c.year) ?? MicroStyles.CivicName(MicroStyles.Civic.Piscina, e, c.year)) != null,
                e != null ? MicroStyles.CivicName(MicroStyles.Civic.Entrenamiento, e, c.year) : "");
            string kind = e != null ? MicroStyles.TrainingKind(e, c.year) : null;
            TestProbe.Check($"entrena en {c.training}", kind == c.training, kind);
            var tradition = HistoricTraining.ForMicrocosmos(c.display, out int y, out string k2);
            TestProbe.Check("su lugar de entrenar tiene tradición y sesiones de época", tradition != null && HistoricTraining.Sessions(tradition, y, k2).Count > 0, tradition != null ? tradition.id : "sin tradición");
            TestProbe.Check("tiene frutales de su lugar", e != null && MicroStyles.Fruits(e).Length >= 2);
            if (c.display != "Feria de las Hazañas") TestProbe.Check("su luz sale de su lugar y su fecha", SolarPosition.GeoOf(c.display, out _, out _, out _));
            TestProbe.End();
            yield return null;
        }

        TestProbe.Begin("Historias 3 (datos): los juicios");
        var onon = new List<string>(); foreach (var fr in MicroStyles.Fruits(MicroStyles.For(1450, MicroStyles.Region.Haudenosaunee))) onon.Add(fr.name);
        TestProbe.Check("en Onondaga no hay frutales europeos (manzanos, perales)", !onon.Exists(x => x.Contains("Manzano") || x.Contains("Peral")), string.Join(", ", onon));
        TestProbe.Check("ninguna de las entradas nuevas levanta un cuartel (historias civiles)", System.Array.TrueForAll(Cases, c => { var e = MicroStyles.For(c.year, MicroStyles.RegionOf(c.display)); return e.id == "norteamerica_victoriana" || MicroStyles.CivicName(MicroStyles.Civic.Milicia, e, c.year) == null; }));
        var pend = MicroFloors.Pending();
        TestProbe.Check("la sala de asambleas la esperan al menos cuatro de esta tanda", pend.TryGetValue("Sala de asambleas (parlamento)", out var ls) && ls.Count >= 5, ls != null ? string.Join(" · ", ls) : "");
        TestProbe.End();
    }
}
