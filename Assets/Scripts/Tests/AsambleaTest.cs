using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// TEST DE LA SALA DE ASAMBLEAS, DEL DISEÑO DE LAS ÁREAS Y DE LA VERACIDAD (docs/piso-asambleas.md, docs/veracidad-frases.md; grupo 77). Datos, y el piso si está en la escena:
/// - las 5 historias que la esperaban están en «Assembly_Area», representadas; su misión, con pasos y uniforme; la asamblea es de todos;
/// - el piso (si existe): en Servicios, con plazas para todos, sus piezas del mundo y sin muebles a menos de 2 m (S7, lo mira FurnitureSpacing en 28);
/// - AreaDesign: cada fila Cumple con ≥ 3 culturas y su doc;
/// - LineTruth: la tabla clasifica bien y una misión de prueba queda sin huecos.
/// </summary>
public class AsambleaTest : MonoBehaviour, ITestUnit
{
    public int Group => 77;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<AsambleaTest>() == null) tr.gameObject.AddComponent<AsambleaTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Sala de asambleas: datos");
        foreach (string d in new[] { "Angostura", "Onondaga", "Madrid 2018", "Gettysburg", "Feria de las Hazañas" })
        {
            MicroFloors.Match m = MicroFloors.Of(d);
            TestProbe.Check($"{d} está en la Sala de asambleas, representada", m != null && m.platform == "Assembly_Area" && m.representative, m != null ? $"{m.platform} «{m.pending}»" : "sin fila");
        }
        TestProbe.Check("ningún microcosmos espera ya la Sala de asambleas", !MicroFloors.Pending().ContainsKey("Sala de asambleas (parlamento)"));
        TestProbe.Check("la asamblea es para todos los miembros (J16)", LeisureDirector.AllMembers.Contains("asamblea"));
        foreach (string id in new[] { "asa_orden", "asa_acta", "asa_mediar" })
            TestProbe.Check($"«{id}» lleva el uniforme de la asamblea", Uniforms.ForWork(id, "Sala de asambleas", "") == Uniforms.Kind.Asamblea);
        TestProbe.End();
        yield return null;

        GameObject area = GameObject.Find("Assembly_Area");
        if (area != null)
        {
            TestProbe.Begin("Sala de asambleas: el piso");
            int seats = area.GetComponentsInChildren<AreaSeat>(true).Where(s => s.activity == "asamblea").Sum(s => s.seats);
            TestProbe.Check("caben todos los miembros a la vez", seats >= WorkdayDirector.Staff.Length, $"{seats} plazas, {WorkdayDirector.Staff.Length} miembros");
            var low = AreaCapacity.Check(new[] { area.transform }, AreaCapacity.Members(), out _).Where(l => !l.ok).Select(l => l.ToString()).ToList();
            TestProbe.Check("ningún puesto por debajo de la regla de capacidad (S13)", low.Count == 0, string.Join(" · ", low));
            string names = string.Join(" ", area.GetComponentsInChildren<Transform>(true).Select(t => t.name));
            foreach (string piece in new[] { "Piedra de la palabra", "Toguna", "Kleroterion", "Cabina de intérpretes", "Archivo de actas", "Marcador de votos", "Retoño del roble" })
                TestProbe.Check($"tiene {piece}", names.Contains(piece));
            TestProbe.End();
        }

        TestProbe.Begin("Diseño de las áreas (S14, F14)");
        foreach (AreaDesign.Row r in AreaDesign.All.Where(x => x.state == AreaDesign.State.Cumple))
            TestProbe.Check($"{r.area}: ≥ 3 culturas y su doc", r.cultures.Length >= 3 && r.doc.Length > 0, string.Join(", ", r.cultures));
        TestProbe.Check("la Sala de asambleas cumple", AreaDesign.Of("Sala de asambleas")?.state == AreaDesign.State.Cumple);
        TestProbe.Check("el Huerto y Construcción cumplen", AreaDesign.Of("Huerto")?.state == AreaDesign.State.Cumple && AreaDesign.Of("Construcción")?.state == AreaDesign.State.Cumple);
        GameObject garden = GameObject.Find("Garden_Area"), works = GameObject.Find("Construction_Area");
        if (garden != null && garden.transform.parent != null)
        {
            string plots = string.Join(" ", garden.transform.parent.GetComponentsInChildren<Transform>(true).Select(t => t.name));
            foreach (string piece in new[] { "Chinampa", "Zai", "Anden", "Hugelkultur", "Batdam", "Subak" }) TestProbe.Check($"el piso del Huerto tiene {piece}", plots.Contains("Metodo_" + piece));
        }
        if (works != null)
        {
            string samples = string.Join(" ", works.GetComponentsInChildren<Transform>(true).Select(t => t.name));
            foreach (string piece in new[] { "Guadua", "Tapia", "Kigumi", "Boveda_nubia", "Silleria_inca" }) TestProbe.Check($"Construcción tiene {piece}", samples.Contains("Tecnica_" + piece));
        }
        TestProbe.End();

        TestProbe.Begin("Veracidad de las frases (M39)");
        TestProbe.Check("la promesa del cargo es documentada", LineTruth.Judge("Pedro Sánchez", "Prometo por mi conciencia y honor cumplir", new string[0]).v == Veracity.Documentada);
        TestProbe.Check("la frase de juego de una persona real es recreada, con fuente", LineTruth.Judge("Elon Musk", "Vamos a intentarlo", new[] { "Space.com" }) is var a && a.v == Veracity.Recreada && a.source.Contains("Space.com"));
        TestProbe.Check("la cita de la música tiene su base", LineTruth.Judge("John Lennon", "Si pudiera vender la paz como se vende un jabón, lo haría.", new string[0]).v == Veracity.Atestiguada);
        TestProbe.Check("las historias guionizadas tienen su fila", new[] { "Compasión", "Egina", "Sakshi", "Ambrosio", "Cocina" }.All(s => LineTruth.ForStory(s).Length == 1));
        TestProbe.Check("la de un personaje de juego es inventada", LineTruth.Judge("Ujier", "Hoy está esto lleno", new string[0]).v == Veracity.Inventada);
        var probe = new GameObject("Prueba_veracidad"); probe.SetActive(false);
        var card = probe.AddComponent<HistoryCard>(); card.sources = new[] { "Fuente" };
        var mission = probe.AddComponent<StoryMission>();
        mission.steps = new[] { new StoryMission.StoryStep { goal = "a", speaker = "Abraham Lincoln", line = "Hola" }, new StoryMission.StoryStep { goal = "b", speaker = "Ujier", line = "Hola" } };
        mission.finalSpeaker = "Ujier"; mission.finalLine = "Adiós";
        LineTruth.Fill(mission, card);
        TestProbe.Check("Fill no deja frases sin marcar y las escribe en la ficha", LineTruth.Missing(mission).Count == 0 && card.lineTruth.Length == 3, string.Join(" · ", LineTruth.Missing(mission)));
        Destroy(probe);
        TestProbe.End();
    }
}
