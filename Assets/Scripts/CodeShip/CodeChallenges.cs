using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL CATÁLOGO DE RETOS DE CÓDIGO DE LOS MICROCOSMOS (docs/plan-microcosmos-realismo.md §4.4; CodeShip reglas.md §4: «todo se hace con
/// retos de código»). Datos: cada reto dice en qué escena va, después de qué paso de su historia (<see cref="StoryMission"/>) y en qué
/// sitio se resuelve. <see cref="Inject"/> los mete en la historia al arrancar, como pasos <see cref="StoryMission.Kind.Code"/>, sin
/// reconstruir la escena (la escena del microcosmos no cambia; solo la lista de pasos en memoria). Acertar el reto cumple el paso.
/// Contenido con datos reales y su fuente (regla M39).
/// </summary>
public static class CodeChallenges
{
    /// <summary>Dónde va un reto: la escena, el paso de la historia tras el que va (un trozo de su objetivo), el sitio (nombre del objeto
    /// bajo la historia) y lo que se dice al acertarlo.</summary>
    public class Spec
    {
        public string scene, afterGoal, target, goal, speaker, line;
        public CodeExercise exercise;
    }

    static List<Spec> _specs;
    static List<CodeExercise> _all;

    public static List<Spec> Specs { get { if (_specs == null) Build(); return _specs; } }
    public static List<CodeExercise> All { get { if (_all == null) Build(); return _all; } }

    public static CodeExercise Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (CodeExercise e in All) if (e.id == id) return e;
        return null;
    }

    public static List<Spec> ForScene(string scene) => Specs.FindAll(s => s.scene == scene);

    /// <summary>Mete en la historia <paramref name="m"/> los retos de su escena, cada uno tras su paso (si ya están, no hace nada). Un reto
    /// cuyo paso o sitio no se encuentra se salta con un aviso (no rompe la historia). Devuelve cuántos metió.</summary>
    public static int Inject(StoryMission m)
    {
        if (m == null || m.steps == null) return 0;
        var steps = new List<StoryMission.StoryStep>(m.steps);
        int added = 0;
        foreach (Spec s in ForScene(m.gameObject.scene.name))
        {
            if (steps.Exists(x => x.kind == StoryMission.Kind.Code && x.challenge == s.exercise.id)) continue;
            int after = steps.FindIndex(x => x.goal != null && x.goal.Contains(s.afterGoal));
            Transform t = FindUnder(m.transform, s.target);
            if (after < 0 || t == null) { Debug.LogWarning($"[Retos] «{s.exercise.id}»: no encuentro {(after < 0 ? $"el paso «{s.afterGoal}»" : $"el sitio «{s.target}»")} en la historia de {m.gameObject.scene.name}; se salta."); continue; }
            if (after < m.Step) continue;   // ese paso ya se pasó: no se mete detrás de lo hecho
            steps.Insert(after + 1, new StoryMission.StoryStep
            {
                goal = s.goal, kind = StoryMission.Kind.Code, challenge = s.exercise.id, targets = new[] { t },
                speaker = s.speaker, line = s.line, truth = Veracity.Recreada, truthSource = s.exercise.source,
            });
            added++;
        }
        if (added > 0) m.steps = steps.ToArray();
        return added;
    }

    static Transform FindUnder(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    static CodeBlock B(string id, string pseudo, string js, int indent = 0) => new CodeBlock { id = id, pseudo = pseudo, js = js, indent = indent };

    static void Build()
    {
        _specs = new List<Spec>();
        _all = new List<CodeExercise>();

        // ══ ELON MUSK · Hawthorne, 2008 · el Falcon 1 ═══════════════════════════════════════════════════════════════════════════════════
        // Fuentes: Spaceflight Now «Falcon 1 flight 4» (cronología del vuelo: apagado del Merlin, separación, Kestrel, cofia, órbita);
        // SpaceX, nota del 2-8-2008 y Musk en su blog (el empuje residual del Merlin 1C regenerativo tras el apagado hizo que la primera
        // etapa alcanzara a la segunda; el arreglo, alargar la espera entre el apagado y la separación); Koenigsmann et al., SmallSat 2009.
        const string muskSrc = "Spaceflight Now, «Falcon 1 flight 4» (2008); SpaceX, actualización del vuelo 3 (agosto de 2008); Koenigsmann et al., SmallSat 2009.";
        Add(new Spec
        {
            scene = "Microcosmos_ElonMusk", afterGoal = "Mira con el equipo los datos del vuelo 3", target = "Consola de datos del vuelo 3",
            goal = "Reto: repara la secuencia del vuelo 3 en la consola de datos", speaker = "Ingeniera de propulsión",
            line = "Eso es: no hay que cambiar el motor, hay que esperar. Con esa línea, la primera etapa ya no alcanza a la segunda.",
            exercise = new CodeExercise
            {
                id = "musk-reparar-vuelo3", kind = CodeExercise.Kind.Choice, skill = CodeSkill.Reparar,
                idea = "Un fallo de tiempo se arregla en el orden y las esperas, no en la pieza.",
                question = "Vuelo 3 (2-8-2008): al apagarse, el Merlin 1C siguió empujando un poco y la primera etapa chocó con la segunda tras separarse. ¿Qué línea pones entre «apagar» y «separar»?",
                blocks = new[]
                {
                    B("l1", "apagar(merlin1C)            // MECO: fin de la primera etapa", "merlin1C.shutdown(); // MECO"),
                    B("l2", "// ← aquí falta algo", "// ← missing line"),
                    B("l3", "separar(etapas)", "stages.separate();"),
                    B("l4", "encender(kestrel)           // segunda etapa", "kestrel.ignite();"),
                },
                options = new[]
                {
                    "esperar(hasta que el empuje residual caiga)",
                    "encender(kestrel) antes de separar",
                    "apagar(merlin1C) otra vez",
                    "quitar separar(etapas)",
                },
                correct = 0, source = muskSrc,
                explain = "Para el vuelo 4 se alargó la espera entre el apagado del Merlin y la separación de etapas, para que el empuje residual se agotara antes de separar.",
            },
        });
        Add(new Spec
        {
            scene = "Microcosmos_ElonMusk", afterGoal = "Revisa las tres cosas", target = "Consola de datos del vuelo 3",
            goal = "Reto: ordena la secuencia de lanzamiento del vuelo 4", speaker = "Director de lanzamiento",
            line = "Secuencia cargada. Si esto sale como en la pantalla, el 28 de septiembre estamos en órbita.",
            exercise = new CodeExercise
            {
                id = "musk-secuencia-vuelo4", kind = CodeExercise.Kind.Order, skill = CodeSkill.Especificar,
                idea = "Un programa es una secuencia: cada paso solo puede ir cuando el anterior ha terminado.",
                question = "Ordena la secuencia de vuelo del Falcon 1 (vuelo 4, 28-9-2008). Sobra un bloque.",
                blocks = new[]
                {
                    B("encender1", "encender(merlin1C)          // en la plataforma de Omelek", "merlin1C.ignite();"),
                    B("despegar", "soltar(anclajes)            // despegue", "pad.release();"),
                    B("meco", "apagar(merlin1C)            // MECO", "merlin1C.shutdown();"),
                    B("esperar", "esperar(empujeResidual == 0)", "await until(() => residualThrust === 0);"),
                    B("separar", "separar(etapas)", "stages.separate();"),
                    B("kestrel", "encender(kestrel)           // segunda etapa", "kestrel.ignite();"),
                    B("cofia", "soltar(cofia)               // fuera de la atmósfera densa", "fairing.jettison();"),
                    B("seco", "apagar(kestrel)             // SECO: ¡en órbita!", "kestrel.shutdown(); // orbit"),
                    B("distraccion", "separar(etapas) con el Merlin aún encendido", "stages.separate(); // engine still on"),
                },
                solution = new[] { "encender1", "despegar", "meco", "esperar", "separar", "kestrel", "cofia", "seco" },
                source = muskSrc,
                explain = "El 28 de septiembre de 2008 el cuarto Falcon 1 siguió esta secuencia y puso en órbita su carga de prueba de 165 kg (RatSat): el primer cohete de combustible líquido de financiación privada en llegar a órbita.",
            },
        });
    }

    static void Add(Spec s) { _specs.Add(s); _all.Add(s.exercise); }
}
