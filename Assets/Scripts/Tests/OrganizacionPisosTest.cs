using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA FASE F0 DEL REDISEÑO DE PISOS (grupo 142; PB-170, docs/areas/rediseno-pisos-arboles.md; D1 de Berón 2026-11-25:
/// «uno por actividad, por plazas»). Sin escena:
/// 1. Las MARCAS de los pisos (puro/mixto, cuota) y las reglas de la reorganización por cálculo (<see cref="FloorOrganization"/>): dónde
///    cabe cada cuota, el mixto desbordado, la Residencia que sube a una cima y la cede a Ciencias, el desequilibrio.
/// 2. La J16 reescrita: el entrenamiento de las 8 y las 14 se reparte POR PLAZAS (<see cref="WorkRoster.SeatTraining"/>): nadie pasa
///    del tope de un centro, todos entrenan, los maestros conservan su clase y, sin plazas, la rotación es la de siempre.
/// 3. Los pisos por plazas (<see cref="TrainingCentres.FloorsBySeats"/>): uno por centro mientras la suma llegue.
/// </summary>
public class OrganizacionPisosTest : MonoBehaviour, ITestUnit
{
    public int Group => 142;
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<OrganizacionPisosTest>() == null) tr.gameObject.AddComponent<OrganizacionPisosTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Rediseño de pisos F0: marcas de cada piso y reorganización por cálculo");
        TestProbe.Check("cuota 1: un piso · 1,5: piso con anillo · 7,6: cima · 10: cima con anillo · 30: se reparte",
            FloorOrganization.Place(1f).where == FloorOrganization.Where.Floor && FloorOrganization.Place(1.5f).where == FloorOrganization.Where.FloorRing
            && FloorOrganization.Place(7.6f).where == FloorOrganization.Where.Crown && FloorOrganization.Place(10f).where == FloorOrganization.Where.CrownRing
            && FloorOrganization.Place(30f).where == FloorOrganization.Where.Split && FloorOrganization.Place(30f).floors == 16);

        List<FloorOrganization.Building> before = FloorOrganization.Proposed();
        var allFloors = before.SelectMany(b => b.floors).ToList();
        TestProbe.Check("cada piso lleva marcas: puro con un componente, mixto con varios, y ninguno pasa de su piso salvo la Residencia",
            allFloors.All(f => f.components.Count > 0 && f.Mixed == (f.components.Count > 1)) && allFloors.Where(f => f.id != "residencia-plaza").All(f => f.Occupancy <= 1f + 1e-4f),
            string.Join(" · ", allFloors.Where(f => f.Occupancy > 1f).Select(f => $"{f.id} {f.Occupancy:0.##}")));
        TestProbe.Check("la categoría de cada árbol es la de sus pisos", FloorOrganization.CategoryOf(before.First(b => b.id == "servicios")) == FloorOrganization.Category.Servicios
            && FloorOrganization.CategoryOf(before.First(b => b.id == "entrenamiento")) == FloorOrganization.Category.Entrenamiento
            && FloorOrganization.CategoryOf(before.First(b => b.id == "produccion")) == FloorOrganization.Category.Produccion);

        List<FloorOrganization.Building> after = FloorOrganization.Reorganize(before, out List<string> done);
        foreach (string d in done) Debug.Log($"[Organización] {d}");
        FloorOrganization.Building ciencias = after.FirstOrDefault(b => b.id == "ciencias");
        TestProbe.Check("la Residencia no cabe en un piso: sube a una cima, se queda sola con ella y la cede a Ciencias (el árbol con menos pisos), y desaparece",
            after.All(b => b.id != "residencia") && ciencias != null && ciencias.crown != null && ciencias.crown.components.Any(c => c.id == "residencia") && done.Count == 2,
            string.Join(" | ", done));
        TestProbe.Check("las reglas no tocan nada más: Servicios 5, Entrenamiento 4, Ciencias 2 + cima, Producción 3, Campos 4",
            string.Join(",", after.Select(b => $"{b.id}:{b.floors.Count}")) == "servicios:5,entrenamiento:4,ciencias:2,produccion:3,campos:4",
            string.Join(",", after.Select(b => $"{b.id}:{b.floors.Count}")));
        TestProbe.Check("los entrenamientos de un piso por actividad suman plazas para todos los miembros (J16 reescrita)",
            FloorOrganization.TrainingSeats(after) >= AreaCapacity.DefaultMembers, $"{FloorOrganization.TrainingSeats(after)} plazas, {AreaCapacity.DefaultMembers} miembros");
        TestProbe.Check("desequilibrio: Ciencias (2 pisos frente a 5) queda señalado",
            FloorOrganization.Review(after).Any(f => f.rule == FloorOrganization.Rule.Imbalance && f.building == "ciencias"));

        // Un mixto desbordado: el componente que crece se queda; el otro busca un piso de su categoría con sitio.
        var mixed = new List<FloorOrganization.Building>
        {
            new FloorOrganization.Building("a", "A",
                new FloorOrganization.Floor("cria-carne", new FloorOrganization.Component("cria", "Cría", FloorOrganization.Category.Servicios, 0.9f),
                                                          new FloorOrganization.Component("carne", "Carne", FloorOrganization.Category.Ciencias, 0.4f))),
            new FloorOrganization.Building("b", "B",
                new FloorOrganization.Floor("lab", new FloorOrganization.Component("quimica", "Química", FloorOrganization.Category.Ciencias, 0.5f)),
                new FloorOrganization.Floor("restaurante", new FloorOrganization.Component("restaurante", "Restaurante", FloorOrganization.Category.Servicios, 0.3f))),
        };
        var over = FloorOrganization.Review(mixed).Where(f => f.rule == FloorOrganization.Rule.Overflow).ToList();
        TestProbe.Check("mixto desbordado: la Cría se queda y la Carne va al laboratorio (su categoría), no al piso más vacío",
            over.Count == 1 && over[0].component == "carne" && over[0].grows == "cria" && over[0].to == "lab", string.Join(" · ", over));
        TestProbe.End();
        yield return null;

        TestProbe.Begin("Rediseño de pisos F0: el entrenamiento de las 8 y las 14, por plazas (J16 reescrita)");
        var r = new WorkRoster { initiationModel = true, useProfessions = false };
        r.areas.Add("A");
        r.trainingAreas.AddRange(new[] { "Combate", "Gimnasio", "Salas" });
        for (int i = 0; i < 30; i++) r.Add("P" + i);
        // Sin plazas: la rotación de siempre (persona i, día d → (i + d) mod n).
        WorkRoster.DayPlan free = r.Plan(3, new List<WorkRoster.Mission>());
        TestProbe.Check("sin plazas conocidas, la rotación de siempre: (i + d) mod n a las 14 y (i + d + 1) mod n a las 8",
            r.workers.Select((w, i) => (w, i)).All(x => free.training[x.w] == r.trainingAreas[(x.i + 3) % 3] && free.morning[x.w] == r.trainingAreas[(x.i + 4) % 3]));
        r.trainingSeats["Gimnasio"] = 6; r.trainingSeats["Combate"] = 10; r.trainingSeats["Salas"] = 32;
        bool fits = true, all = true, moves = false;
        for (int d = 0; d < 7; d++)
        {
            WorkRoster.DayPlan p = r.Plan(d, new List<WorkRoster.Mission>());
            foreach (var slot in new[] { p.training, p.morning })
            {
                all &= r.workers.All(slot.ContainsKey);
                foreach (string a in r.trainingAreas) fits &= WorkRoster.Seated(slot, a) <= r.SeatsOf(a);
            }
            moves |= r.workers.Select((w, i) => (w, i)).Any(x => p.training[x.w] != r.trainingAreas[(x.i + d) % 3]);
        }
        TestProbe.Check("una semana: todos entrenan a las 8 y a las 14 y ningún centro pasa de sus plazas", all && fits);
        TestProbe.Check("quien encuentra lleno su centro va al siguiente con plaza", moves);
        var gym = new Dictionary<WorkRoster.Worker, int>();
        for (int d = 0; d < 30; d++) foreach (var kv in r.Plan(d, new List<WorkRoster.Mission>()).training) if (kv.Value == "Gimnasio") { gym.TryGetValue(kv.Key, out int n); gym[kv.Key] = n + 1; }
        TestProbe.Check("en un mes, el gimnasio (6 plazas) le toca a casi todos: quien elige primero rota cada día", gym.Count >= 24, $"{gym.Count} de 30");
        // Sin plazas para todos (J16 incumplida): nadie se queda sin entrenar, va al centro menos lleno.
        r.trainingSeats["Salas"] = 8;
        WorkRoster.DayPlan shortP = r.Plan(0, new List<WorkRoster.Mission>());
        TestProbe.Check("si la suma no llega (24 plazas para 30), todos entrenan igualmente, en el menos lleno", r.workers.All(shortP.training.ContainsKey) && r.trainingAreas.All(a => WorkRoster.Seated(shortP.training, a) >= r.SeatsOf(a)));
        TestProbe.End();
        yield return null;

        TestProbe.Begin("Rediseño de pisos F0: pisos de cada centro por plazas");
        int sum = TrainingCentres.SeatsOnePerActivity();
        TestProbe.Check($"con {sum} plazas en un piso de cada centro, 30 miembros: un piso de cada uno", TrainingCentres.All.All(s => TrainingCentres.FloorsBySeats(s.name, 30) == 1));
        TestProbe.Check("si la suma no llega, el piso que falta va al centro con más plazas por piso (la Calistenia, 40)",
            TrainingCentres.FloorsBySeats("Calistenia", sum + 1) == 2 && TrainingCentres.FloorsBySeats("Gimnasio", sum + 1) == 1);
        TestProbe.Check("fase F1: los edificios «amplios» (cada centro para todos) quedan apagados y el constructor levanta uno por centro",
            !TrainingCentres.Amplios && TrainingCentres.All.All(s => TrainingCentres.FloorsFor(s.name, 30) == 1)
            && TrainingCentres.FloorsFor(16, 30) == 2);   // (la cuenta amplia se guarda: un centro de 16 plazas para 30, dos pisos)
        TestProbe.End();
    }
}
