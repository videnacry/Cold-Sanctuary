using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL ALIMENTADOR (Berón 2026-11-05; docs/alimentador.md): no hay comedero; la lista es compartida y nadie mira dos veces al mismo; quien tiene
/// hambre recibe su ración desde lejos, quien no, queda marcado; misión por rotación, con E, gesto y pasos virtuales. Y la cadena alimenticia lo cuenta.
/// </summary>
public class AlimentadorTest : MonoBehaviour, ITestUnit
{
    public int Group => 105;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<AlimentadorTest>() == null) tr.gameObject.AddComponent<AlimentadorTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Alimentador: sin comedero, lista compartida, distancia segura y rotación");
        if (FindAnyObjectByType<Territorio>() == null) { Debug.Log("[TEST] SKIP · Alimentador — esta escena no es el santuario"); TestProbe.End(); yield break; }

        TestProbe.Check("ya no hay comedero", GameObject.Find("Comedero_depredadores") == null);
        TestProbe.Check("hay almacén de raciones junto al laboratorio", PredatorRound.Store != null && PredatorRound.Store.name == "Almacen_raciones");

        // ── La misión: para cualquiera, por rotación ──
        foreach (string id in new[] { "gua_alimentador", "gua_alimentador_2" })
        {
            AreaFunctions.AreaFunction f = AreaFunctions.Get(id);
            TestProbe.Check($"{id}: en el catálogo, sin profesión exigida", f != null && f.sanctuary && f.roles.Length == 0);
            TestProbe.Check($"{id}: con pasos de E", PlayerTasks.Authored(id));
            TestProbe.Check($"{id}: con pasos virtuales", VirtualMissions.Steps(id).Length >= 3);
            TestProbe.Check($"{id}: con gesto de jornada", Performances.ForMission(id) != null);
        }

        // Un clon de la Magnate con la misión la hace de verdad (ronda 24: con el reparto las dos plazas eran de clones, que iban a su sitio
        // sin la misión, y nadie daba de comer: los dos osos adultos morían de hambre, 43, en la jornada).
        foreach (string id in new[] { "gua_alimentador", "gua_alimentador_2", "gua_carnivoros" })
            TestProbe.Check($"[CV-11] un clon con «{id}» la lleva a cabo", WorkdayDirector.CloneDuty(id) == id);
        TestProbe.Check("un clon con otra misión solo va a su sitio", WorkdayDirector.CloneDuty("coc_servicio") == null);
        // …y su cuerpo puede llevarla: un clon es una copia del cuerpo SIN Anima, y el alimentador solo se le ponía a quien la tenía (ronda 25:
        // ni una ración de un clon en toda la jornada; los osos, muertos de hambre).
        { GameObject clon = WorkdayDirector.SpawnMagnateBody("Magnate (clon de prueba)"); TestProbe.Check("el cuerpo de un clon puede llevar una misión con conducta propia", WorkdayDirector.IsPerson(clon)); Destroy(clon); }

        // El primer día ya hay raciones: el primer lote no está antes del turno de las 12 (ronda 26: «recoge 0 raciones»).
        TestProbe.Check("el almacén empieza con un día de raciones de los depredadores de tierra", CulturedMeatLab.FirstDayStock() >= 3 * (PredatorRound.RationsFor("Bear") + PredatorRound.RationsFor("Wolf")), $"{CulturedMeatLab.FirstDayStock()} raciones");

        // ── La lista compartida ──
        PredatorRound.NewRound();
        var list = PredatorRound.Today();
        // (2026-11-11) Con las reglas de la lista de hoy: TODOS los servidos vivos están (los osos, lobos y zorros adultos de tierra y las crías
        // huérfanas de oso, lobo, zorro y foca) y ninguno del mar ni ingresado. Antes exigía que hubiera oso Y lobo: en la suite 58 los dos osos
        // adultos habían muerto antes de esta prueba y la lista (5) fallaba aunque estuviera bien.
        var shouldBe = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None).Where(PredatorRound.Served).ToList();
        var missing = shouldBe.Where(x => !list.Any(e => e.animal == x)).Select(x => x.name).ToList();
        TestProbe.Check("la lista tiene a todos los depredadores de tierra servidos (y ni uno del mar ni ingresado)", shouldBe.Count > 0 && missing.Count == 0
            && !list.Any(e => e.animal.GetComponent<MarineLife>() != null || VetCare.Admitted(e.animal)),
            $"{list.Count} en la lista: {string.Join(", ", list.GroupBy(e => e.animal.SpeciesName + (PredatorRound.Orphan(e.animal) ? " (cría)" : "")).Select(g => $"{g.Key} {g.Count()}"))}{(missing.Count > 0 ? " · faltan: " + string.Join(", ", missing) : "")}");
        // (2026-11-11) Un ingresado lo alimenta la veterinaria: en la lista de la ronda no se le alcanza nunca y, como la lista solo se renueva
        // completa, nadie más volvía a comer (tras 12 h de juego: lobos 4 → 1, el oso adulto muerto de inanición; «Lobo_paciente» 20 veces pendiente).
        var admitted = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None).Where(x => x != null && !x.death && x.Forage != null && x.Forage.eatsPrey && VetCare.Admitted(x)).ToList();
        TestProbe.Check("un depredador ingresado no entra en la ronda del alimentador (le da de comer la veterinaria)", admitted.All(x => !PredatorRound.Served(x)) && !list.Any(e => VetCare.Admitted(e.animal)),
            admitted.Count == 0 ? "sin ingresados en la escena" : string.Join(", ", admitted.Select(x => x.name)));
        // (2026-11-11) CRÍAS HUÉRFANAS: una cría que aún no forrajea y no tiene madre (ni padre) viva no come nunca (tras 12 h: los dos oseznos
        // sin la osa). Entra en la ronda y el alimentador le da el biberón en la mano; con su madre viva, no.
        Animal adultLand = list.Select(e => e.animal).FirstOrDefault(x => x != null && x.lifeStage == LifeStage.adult);
        if (adultLand != null)
        {
            GameObject cubGo = Instantiate(adultLand.gameObject, adultLand.transform.position + Vector3.right * 3f, adultLand.transform.rotation);
            cubGo.name = "Cria_huerfana_TEST";
            Animal cub = cubGo.GetComponent<Animal>(); cub.lifeStage = LifeStage.child;
            yield return null;
            cub.lifeStage = LifeStage.child; cub.bonds.Clear();
            TestProbe.Check("una cría huérfana (sin madre viva) entra en la ronda del alimentador", PredatorRound.Orphan(cub) && PredatorRound.Served(cub), cub.SpeciesName);
            TestProbe.Check("a la cría huérfana se la atiende de cerca (biberón), no a la distancia del adulto", PredatorRound.SafeDistanceFor(cub) < 2f && PredatorRound.SafeDistanceFor(adultLand) >= 5f);
            cub.hungry = 0.5f;
            TestProbe.Check("con hambre se le da aunque no haya raciones de carne en la mano", PredatorRound.ShouldFeed(cub));
            PredatorRound.BottleFeed(cub, "prueba");
            TestProbe.Check("…y el biberón la sacia", cub.hungry < 0f, $"hambre {cub.hungry:0.00}");
            Reproduction.Imprint(cub, adultLand);
            TestProbe.Check("con su madre viva no es huérfana (la amamanta ella)", !PredatorRound.Orphan(cub) && !PredatorRound.Served(cub));
            // (2026-11-11) URGENCIA: el adulto con hambre va antes que la cría huérfana (aunque la cría tenga más), y entre adultos el más hambriento
            // antes que el más cercano. Vivo49: 37 en la lista, 33 pendientes en 3 días, y dos osas muertas de hambre esperando detrás de polluelos.
            cub.bonds.Clear(); cub.hungry = 3f; float oldAd = adultLand.hungry; adultLand.hungry = 0.6f;
            TestProbe.Check("el depredador adulto con hambre va antes que la cría huérfana (biberón)", PredatorRound.Priority(adultLand, 0, cub.transform.position) < PredatorRound.Priority(cub, 0, cub.transform.position));
            Animal other = list.Select(e => e.animal).FirstOrDefault(x => x != null && x != adultLand && !PredatorRound.Orphan(x));
            if (other != null)
            {
                float oldO = other.hungry; other.hungry = -0.5f;
                Vector3 nearOther = other.transform.position;
                TestProbe.Check("el más hambriento antes que el más cercano", PredatorRound.Priority(adultLand, 0, nearOther) < PredatorRound.Priority(other, 0, nearOther));
                TestProbe.Check("…pero el que no se alcanzó vuelve detrás aunque tenga hambre (no se pierde el turno con él)", PredatorRound.Priority(adultLand, 1, nearOther) > PredatorRound.Priority(other, 0, nearOther));
                other.hungry = oldO;
            }
            adultLand.hungry = oldAd;
            Destroy(cubGo);
            // HUÉRFANAS SOLO DONDE LA CRÍA NO SOBREVIVE SIN SUS PADRES: oseznos, lobeznos, zorreznos y cachorros de foca; no los polluelos de pingüino.
            Animal pg = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None).FirstOrDefault(x => x != null && !x.death && x.SpeciesName == "Penguin" && x.lifeStage == LifeStage.adult);
            if (pg != null)
            {
                GameObject chickGo = Instantiate(pg.gameObject, pg.transform.position + Vector3.right * 2f, pg.transform.rotation);
                chickGo.name = "Polluelo_huerfano_TEST"; Animal chick = chickGo.GetComponent<Animal>(); chick.lifeStage = LifeStage.child;
                yield return null;
                chick.lifeStage = LifeStage.child; chick.bonds.Clear();
                TestProbe.Check("un polluelo de pingüino sin padres no entra en la ronda del biberón", !PredatorRound.Orphan(chick) && !PredatorRound.Served(chick));
                Destroy(chickGo);
            }
            TestProbe.Check("las especies del biberón: oso, lobo, zorro ártico y foca", PredatorRound.OrphanSpecies.SetEquals(new[] { "Bear", "Wolf", "ArcticFox", "Seal" }));
            PredatorRound.NewRound(); list = PredatorRound.Today();
        }
        // (2026-11-11) EL QUE SE MUERE DE HAMBRE FUERA DE TURNO: el almacén le deja la ración junto a él al momento (suite 63: una osa adulta murió de
        // inanición al empezar, con hambre 46, antes del primer turno del alimentador).
        Animal starving = list.Select(e => e.animal).FirstOrDefault(x => x != null && !PredatorRound.Orphan(x) && !x.death);
        if (starving != null)
        {
            float oldH = starving.hungry; int before = PredatorRound.EmergencyFed;
            starving.hungry = PredatorRound.StarvingAt + 1f;
            CulturedMeatLab labE = CulturedMeatLab.All.FirstOrDefault(l => l != null); int stockWas = labE != null ? labE.rationsStored : 0;
            if (labE != null && labE.rationsStored < 8) labE.rationsStored = 8;
            PredatorRound.EmergencyTick(true);
            TestProbe.Check("un depredador que se muere de hambre fuera de turno recibe su ración del almacén al momento", PredatorRound.EmergencyFed >= before + 1 && !PredatorRound.ShouldFeed(starving), $"{starving.name}, hambre {starving.hungry:0.0}");
            starving.hungry = oldH; if (labE != null) labE.rationsStored = stockWas;
            foreach (FoodItem fi in Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None)) if (fi != null && fi.name == "Racion_carne_cultivada" && Vector3.Distance(fi.transform.position, starving.transform.position) < 8f) Destroy(fi.gameObject);
            PredatorRound.NewRound(); list = PredatorRound.Today();
        }
        var a = PredatorRound.Claim("Uno", Vector3.zero); var b = PredatorRound.Claim("Dos", Vector3.zero);
        TestProbe.Check("dos alimentadores no se reservan al mismo", a != null && b != null && a.animal != b.animal);

        // ── Con hambre: ración desde lejos; sin hambre: marcado ──
        // (2026-11-11) Con un DEPREDADOR ADULTO: desde 229c412 la lista lleva también a las crías huérfanas (biberón a 1,5 m y a demanda), y la
        // reserva más cercana al origen era un polluelo de pingüino: «actual=1,5 esperado=5» y «recién alimentado…» (ronda 49) miraban la cría.
        PredatorRound.Entry ad = a != null && !PredatorRound.Orphan(a.animal) ? a : b != null && !PredatorRound.Orphan(b.animal) ? b : null;
        for (int k = 0; ad == null && k < 80; k++) { PredatorRound.Entry c = PredatorRound.Claim("Tres", Vector3.zero); if (c == null) break; if (!PredatorRound.Orphan(c.animal)) ad = c; }
        TestProbe.Check("hay un depredador adulto en la ronda para probar la ración", ad != null);
        if (ad != null)
        {
            Animal x = ad.animal; float old = x.hungry;
            x.hungry = 0.5f;
            Vector3 from = PredatorRound.StandOff(x, x.transform.position + Vector3.right * 40f);
            TestProbe.Near("se queda a su distancia segura", Vector3.Distance(from, x.transform.position), PredatorRound.SafeDistance(x.SpeciesName), 0.3f);
            FoodItem fi = PredatorRound.Throw(x, 2, from);
            TestProbe.Check("la ración cae a unos metros del animal, no en la mano", Vector2.Distance(new Vector2(fi.transform.position.x, fi.transform.position.z), new Vector2(x.transform.position.x, x.transform.position.z)) > 2f);
            TestProbe.Check("la ración es comida de verdad", fi.material == OrganicMaterial.Meat && fi.grams >= 1000f);
            PredatorRound.Report(ad, PredatorRound.State.Fed);
            // (2026-11-11) Al dar de comer al ÚLTIMO de la lista, la lista se renueva y el mismo animal vuelve como pendiente; 2 s después, con
            // la ración aún en el suelo (sigue con hambre), se le lanzaba otra: «Lobo_paciente … Quedan 4» y «… Quedan 3» seguidas.
            TestProbe.Check("recién alimentado (la ración aún sin comer) no se le lanza otra aunque siga con hambre", PredatorRound.IsHungry(x) && !PredatorRound.ShouldFeed(x));
            x.hungry = -0.5f; Destroy(fi.gameObject);
            TestProbe.Check("sin hambre no se le da", !PredatorRound.IsHungry(x));
            x.hungry = old;
            TestProbe.Check("marcado en la lista compartida: nadie vuelve a él", !PredatorRound.Today().Any(e => e.animal == x && e.state == PredatorRound.State.Pending));
        }
        PredatorRound.NewRound();

        // ── Nadie sale de la lista sin estar hecho (Berón: «eso no es admisible») ──
        {
            var all = PredatorRound.Today();
            int total = all.Count;
            var first = PredatorRound.Claim("Uno", Vector3.zero);
            if (first != null)
            {
                PredatorRound.Requeue(first, "prueba");
                TestProbe.Check("el no alcanzado vuelve a la cola", first.state == PredatorRound.State.Pending && first.misses == 1 && PredatorRound.Left == total);
                var next = PredatorRound.Claim("Dos", first.animal.transform.position);
                TestProbe.Check("…detrás de los demás", total < 2 || next != first);
                if (next != null) PredatorRound.Requeue(next, "prueba");
                // Con hambre y sin raciones sigue pendiente.
                int stored = 0; foreach (CulturedMeatLab l in CulturedMeatLab.All) { stored += l.rationsStored; l.rationsStored = 0; }
                float old = first.animal.hungry; first.animal.hungry = 0.5f;
                var p = new GameObject("Jugador_prueba"); p.transform.position = first.animal.transform.position;
                PredatorRound.PlayerVisit(p); Destroy(p);
                first.animal.hungry = old;
                TestProbe.Check("con hambre y sin raciones no se da por hecho", !PredatorRound.Today().Any(e => e.animal == first.animal && PredatorRound.Done(e)));
                if (CulturedMeatLab.All.Count > 0) CulturedMeatLab.All[0].rationsStored = stored;
            }
            // Se renueva solo cuando está completa: con uno sin hacer, la misma lista.
            var keep = PredatorRound.Today();
            for (int i = 1; i < keep.Count; i++) keep[i].state = PredatorRound.State.Checked;
            keep[0].state = PredatorRound.State.Pending;
            yield return new WaitForSeconds(0.5f);
            TestProbe.Check("mientras falte uno, la lista no se renueva", PredatorRound.Today().Count(PredatorRound.Done) == keep.Count - 1);
            keep[0].state = PredatorRound.State.Fed;
            TestProbe.Check("completa, se renueva", PredatorRound.Today().All(e => e.state == PredatorRound.State.Pending));
        }
        PredatorRound.NewRound();

        // ── Un alimentador de verdad ──
        var runner = new GameObject("Alimentador_prueba");
        PredatorFeeder pf = runner.AddComponent<PredatorFeeder>();
        TestProbe.Check("el componente se puede poner a cualquiera", pf != null);
        Destroy(runner);

        // ── Cadena alimenticia ──
        TestProbe.Check("la cadena no supone el 80 %: lo calcula del laboratorio", FoodChain.LabShare > 0.05f && FoodChain.LabShare <= 1f, $"{FoodChain.LabShare:P0}");
        yield return null;
        TestProbe.End();
    }
}
