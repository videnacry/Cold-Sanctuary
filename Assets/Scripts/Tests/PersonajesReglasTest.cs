using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LAS REGLAS DE PERSONAJES (grupo 95; docs/reglas-personajes.md, Berón 2026-11-05):
///  · cada persona del mesocosmos tiene su ficha docs/personajes/&lt;nombre&gt;.md. El repo y el proyecto de Unity son carpetas distintas: si desde
///    aquí no se ven los docs (COLD_SANCTUARY_REPO), lo dice y lo comprueba tools/check-personajes.sh, que no necesita Unity;
///  · cada puesto tiene su base documentada (<see cref="Professions.Basis"/>), y nadie tiene un puesto de yoga sin ella;
///  · Consol y Josedrilo son los dos maestros jefes, y Panterilia ya no da yoga;
///  · la clase de Consol sostiene más que la Primaria, lleva variaciones y acaba en la relajación.
/// </summary>
public class PersonajesReglasTest : MonoBehaviour, ITestUnit
{
    public int Group => 95;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<PersonajesReglasTest>() == null) tr.gameObject.AddComponent<PersonajesReglasTest>();
    }

    static string DocsDir()
    {
        var bases = new List<string> { Path.Combine(Application.dataPath, "..") };
        string env = System.Environment.GetEnvironmentVariable("COLD_SANCTUARY_REPO");
        if (!string.IsNullOrEmpty(env)) bases.Insert(0, env);
        foreach (string b in bases) { string d = Path.Combine(b, "docs", "personajes"); if (Directory.Exists(d)) return d; }
        return null;
    }

    static int Holds(List<AsanaPoseSpell> poses, string poseName) => poses.Count(p => p.asanaName.StartsWith(poseName) && p.asanaName.Contains("mantén"));

    public IEnumerator Run()
    {
        TestProbe.Begin("Reglas de personajes: fichas, puestos con base y clase de Consol");

        // ── Cada persona con su ficha ──
        var people = Professions.Mesocosmos.ToList();
        TestProbe.Check("todo el personal de la jornada está en la lista de personas del mesocosmos", WorkdayDirector.Staff.All(people.Contains), string.Join(", ", WorkdayDirector.Staff.Where(s => !people.Contains(s))));
        string docs = DocsDir();
        if (docs != null)
        {
            var missing = people.Where(p => !File.Exists(Path.Combine(docs, Professions.DocName(p) + ".md"))).ToList();
            TestProbe.Check($"cada persona tiene su ficha ({people.Count})", missing.Count == 0, string.Join(", ", missing));
        }
        else Debug.Log("[TEST] Los docs no se ven desde el proyecto de Unity (sin COLD_SANCTUARY_REPO): las fichas las comprueba tools/check-personajes.sh.");

        // ── Cada puesto con su base ──
        var noBasis = Professions.All.Where(e => !Professions.Basis.TryGetValue(e.who, out string b) || string.IsNullOrWhiteSpace(b)).Select(e => e.who).ToList();
        TestProbe.Check($"cada puesto tiene su base documentada ({Professions.All.Length})", noBasis.Count == 0, string.Join(", ", noBasis));
        foreach (KeyValuePair<string, string> kv in Professions.Unconfirmed) Debug.Log($"[TEST] Pendiente de Berón · {kv.Key}: {kv.Value}");
        var yogaPosts = Professions.All.Where(e => e.title.ToLowerInvariant().Contains("yoga") || e.title.Contains("Hatha") || e.title.Contains("Ashtanga")).Select(e => e.who).ToList();
        TestProbe.Check("los únicos con puesto de yoga son Josedrilo (Hatha) y Consol (Ashtanga)", yogaPosts.Count == 2 && yogaPosts.Contains(WorkdayDirector.YogaTeacher) && yogaPosts.Contains(WorkdayDirector.AshtangaTeacher), string.Join(", ", yogaPosts));
        TestProbe.Check("Consol no dirige la clase de Hatha ni Josedrilo se queda sin ella", !Professions.Practises(WorkdayDirector.AshtangaTeacher, "Sala de Yoga", "yog_guia") && Professions.Practises(WorkdayDirector.YogaTeacher, "Sala de Yoga", "yog_guia"));
        TestProbe.Check("Consol tiene su misión de preparar la clase", Professions.Practises(WorkdayDirector.AshtangaTeacher, "Sala de Yoga", "yog_ashtanga") && AreaWork.Catalog.Any(m => m.id == "yog_ashtanga"));
        TestProbe.Check("Consol da la clase de las 14 h", Professions.LeadOn("Sala de Yoga", 3) == WorkdayDirector.AshtangaTeacher);
        TestProbe.Check("Fernando es de la Energía (docs/energia.md)", Professions.Practises("Fernando", "Energía", "sol_mantenimiento"));

        // ── Las dos clases ──
        var primary = YogaLibrary.Find(WorkdayDirector.AshtangaSession).Build().OfType<AsanaPoseSpell>().ToList();
        YogaSession deep = YogaLibrary.Find(YogaClass.DeepSession);
        TestProbe.NotNull("la clase profunda está en la biblioteca", deep);
        if (deep != null)
        {
            var poses = deep.Build().OfType<AsanaPoseSpell>().ToList();
            const string paschi = "Paschimottanasana (pinza sentada)";
            TestProbe.Check("sostiene más que la Primaria", Holds(poses, paschi) > Holds(primary, paschi), $"{Holds(poses, paschi)} fases frente a {Holds(primary, paschi)}");
            TestProbe.Check("cada postura lleva su variación", poses.Where(p => p.asanaName.Contains("variación")).Select(p => p.asanaName.Split('·')[0]).Distinct().Count() == 3);
            TestProbe.Check("acaba en la relajación final", poses.Last().asanaName.StartsWith("Savasana"));
            int open = SunSalutation.AshtangaA().Count;
            TestProbe.Check("la secuencia cambia con el día", AshtangaPrimary.Deep(0).OfType<AsanaPoseSpell>().Skip(open).First().asanaName != AshtangaPrimary.Deep(1).OfType<AsanaPoseSpell>().Skip(open).First().asanaName);
        }
        yield return null;
        TestProbe.End();
    }
}
