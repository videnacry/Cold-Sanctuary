using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LOS MAPAS A MANO (regla M9; petición 75; docs/microcosmos-mapas-a-mano.md): las historias de esta tanda tienen su mapa HECHO, como Angostura:
/// - su lugar, su fecha y sus FUENTES;
/// - la flora (≥ 40 plantas) y la fauna (≥ 8 bichos) en sus sitios reales, ninguna sobre el escenario de la propia historia (≤ 30 m del origen);
/// - ≥ 2 órdenes sobre las piezas del piso y ≥ 4 edificios o lugares del plano;
/// - sin la tarea «colocar a mano» de los esbozos.
/// Datos puros (sin escena): que caigan desde el aire y en suelo libre lo mira el registro «[Mapa] …» al construir el microcosmos.
/// </summary>
public class MapasAManoTest : MonoBehaviour, ITestUnit
{
    public int Group => 115;
    public bool ParallelSafe => true;

    /// <summary>Las historias de esta tanda (2026-11-12).</summary>
    public static readonly string[] Targets = { "Kapilavastu", "Woolsthorpe", "Berkeley", "Berna", "París 1898", "Colorado Springs" , "Pasadena" , "Dayton" , "Alejandría" , "Bagdad" , "Escútari" , "Egina" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MapasAManoTest>() == null) tr.gameObject.AddComponent<MapasAManoTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Mapas a mano: las historias de la tanda tienen su mapa hecho, con fuentes, flora y fauna en su sitio y el piso ordenado");
        foreach (string d in Targets)
        {
            MicroStoryMaps.Map m = MicroStoryMaps.For(d);
            TestProbe.Check($"«{d}»: su mapa está HECHO", m != null && MicroStoryMaps.Authored(d), m != null ? m.status.ToString() : "sin mapa");
            if (m == null) continue;
            TestProbe.Check($"«{d}»: lugar, fecha y fuentes", !string.IsNullOrEmpty(m.place) && !string.IsNullOrEmpty(m.source) && m.source.Length >= 40, m.source);
            TestProbe.Check($"«{d}»: ≥ 40 plantas, ≥ 8 bichos, ≥ 2 órdenes sobre el piso y ≥ 4 edificios o lugares", m.PlantCount >= 40 && m.AnimalCount >= 8 && m.floor.Length >= 2 && m.buildings.Length >= 4,
                $"{m.PlantCount} plantas, {m.AnimalCount} bichos, {m.floor.Length} órdenes, {m.buildings.Length} edificios");
            var onStage = m.flora.Where(p => p.at.magnitude - p.spread < 30f).Select(p => p.plant).Concat(m.fauna.Where(a => a.at.magnitude - a.spread < 30f).Select(a => a.name)).ToList();
            TestProbe.Check($"«{d}»: nada sembrado sobre el escenario de la historia (a ≥ 30 m del origen)", onStage.Count == 0, string.Join(", ", onStage.Take(5)));
            TestProbe.Check($"«{d}»: cada orden del piso dice por qué", m.floor.All(p => !string.IsNullOrEmpty(p.why)));
            TestProbe.Check($"«{d}»: ya no es un esbozo (sin «colocar a mano» pendiente)", !m.todo.Any(t => t.StartsWith("colocar a mano")), string.Join(" · ", m.todo));
        }
        int done = MicroStoryMaps.All.Count(x => x.status == MicroStoryMaps.Status.Hecho), stubs = MicroStoryMaps.All.Count(x => x.status == MicroStoryMaps.Status.Esbozo);
        var imagined = MicroStoryMaps.All.Where(x => x.status == MicroStoryMaps.Status.Imaginado).ToList();
        // Ambrosio y Sakshi: escenario imaginado (Berón, 2026-10-27), sin mapa real: no son un esbozo pendiente.
        TestProbe.Check("Ambrosio y Sakshi: escenario imaginado (Berón, 2026-10-27), sin mapa real", new[] { "Ambrosio (nivel 1)", "Sakshi (el río)" }.All(d => MicroStoryMaps.For(d) != null && MicroStoryMaps.For(d).status == MicroStoryMaps.Status.Imaginado));
        Debug.Log($"[Mapa] Tabla principal: {done} hechos, {imagined.Count} imaginados ({string.Join(", ", imagined.Select(x => x.display))}: escenario imaginado, sin mapa real) y {stubs} esbozos: {string.Join(", ", MicroStoryMaps.All.Where(x => x.status == MicroStoryMaps.Status.Esbozo).Select(x => x.display))}.");
        yield return null;
        TestProbe.End();
    }
}
