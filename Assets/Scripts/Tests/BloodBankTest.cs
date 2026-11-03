using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL BANCO DE SANGRE (Berón 2026-11-03; docs/hospital.md §Banco de sangre):
/// 1. está en el hospital, con su equipo: nevera de hematíes, agitador de plaquetas, congelador de plasma, descongelador, nevera
///    veterinaria y sillón de donante; cada producto a su temperatura;
/// 2. compatibilidad ABO/Rh de hematíes y de plasma, perros DEA 1 y gatos sin donante universal;
/// 3. caducidad (la ronda retira lo caducado);
/// 4. el puesto del banco (F / el personal) cruza y transfunde al paciente de al lado; al lobo, sangre de perro DEA 1−;
/// 5. donar (y no dos veces antes de 56 días); la ronda diaria (dentro de «inventario del hospital») repone el submarino con O−;
/// 6. sin sangre compatible, no se transfunde y lo dice (banco de sangre andante).
/// </summary>
public class BloodBankTest : MonoBehaviour, ITestUnit
{
    public int Group => 83;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<BloodBankTest>() == null) tr.gameObject.AddComponent<BloodBankTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Banco de sangre del hospital: almacén, compatibilidad, caducidad, transfusión, donación y ronda diaria");
        BloodBank bank = BloodBank.Instance;
        HospitalStation st = bank != null ? bank.GetComponent<HospitalStation>() : null;
        TestProbe.Check("hay banco de sangre en el puesto del hospital", bank != null && st != null && st.service == HospitalService.BancoDeSangre);
        if (bank == null) { TestProbe.End(); yield break; }
        Transform line = bank.transform.parent;
        string[] eq = { "Banco_de_sangre", "Agitador_plaquetas", "Congelador_plasma", "Descongelador_plasma", "Nevera_veterinaria", "Sillon_donante" };
        var lack = eq.Where(n => line == null || line.Find(n) == null).ToList();
        TestProbe.Check("su equipo: neveras, agitador, congelador, descongelador y sillón de donante (un tramo corrido)", lack.Count == 0 && line.GetComponent<FurnitureLine>() != null, lack.Count == 0 ? line.name : "falta " + string.Join(", ", lack));
        yield return null;   // (las existencias se ponen en Start)
        bool temps = bank.stock.Any() && bank.stock.All(u => u.product == BloodBank.Product.Plasma ? u.StorageC <= -25f : u.product == BloodBank.Product.Plaquetas ? u.StorageC >= 20f && u.StorageC <= 24f : u.StorageC >= 2f && u.StorageC <= 6f);
        TestProbe.Check("cada producto a su temperatura (hematíes 2–6 °C, plasma ≤ −25 °C, plaquetas 20–24 °C)", temps, bank.Summary());

        // ── 2. COMPATIBILIDAD ──
        bool abo = BloodBank.RedCellsCompatible("O−", "AB+") && !BloodBank.RedCellsCompatible("A+", "O−") && BloodBank.RedCellsCompatible("A−", "A+") && !BloodBank.RedCellsCompatible("B+", "A+")
                   && BloodBank.RedCellsCompatible("O−", "O−") && !BloodBank.RedCellsCompatible("AB+", "A+");
        bool plasma = BloodBank.PlasmaCompatible("AB", "O") && !BloodBank.PlasmaCompatible("O", "A") && BloodBank.PlasmaCompatible("A", "O");
        TestProbe.Check("ABO/Rh: O− a todos, Rh+ no a Rh−; plasma AB a todos", abo && plasma);

        // ── 3. CADUCIDAD ──
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        var old = bank.Add(BloodBank.Product.Hematies, "A+", "humano", "caducada (prueba)", day - 50);
        int gone = bank.Expire(day);
        TestProbe.Check("retira los hematíes de más de 42 días", gone >= 1 && !bank.stock.Contains(old), $"{gone} retiradas");

        // ── 4. TRANSFUSIÓN desde el puesto ──
        GameObject body = WorkdayDirector.SpawnBodyCopy("Irosene", "Paciente de transfusión (prueba)");
        body.transform.position = st.transform.TransformPoint(new Vector3(0f, 0f, -1.6f)) + Vector3.up * 0.05f;
        ChecklistProbeAnima pa = body.AddComponent<ChecklistProbeAnima>();
        BloodLoss bl = body.AddComponent<BloodLoss>(); bl.lost = 0.36f;
        yield return null;
        int before = bank.stock.Count;
        string grp = BloodBank.GroupOf(body.name);
        Anima treated = st.Treat(null);
        TestProbe.Check($"el puesto cruza y transfunde (grupo {grp}): sale de la clase III", treated == pa && bl.lost < 0.3f && bank.stock.Count < before, $"pérdida 36 % → {bl.lost * 100f:0} % ({bl.unitsReceived} u.: {bl.lastProduct})");
        Destroy(body);
        Animal wolf = FindObjectsByType<Animal>(FindObjectsSortMode.None).FirstOrDefault(a => a != null && !a.death && a.SpeciesName == "Wolf");
        if (wolf != null)
        {
            BloodLoss wb = wolf.gameObject.AddComponent<BloodLoss>(); wb.lost = 0.25f;
            int u = bank.Transfuse(wolf, "veterinaria (prueba)");
            TestProbe.Check("al lobo, sangre de perro DEA 1− (la de la nevera veterinaria)", u >= 1 && wb.lastProduct != null && wb.lastProduct.Contains("perro"), wb.lastProduct ?? "nada");
            Destroy(wb);
        }

        // ── 5. DONAR Y LA RONDA ──
        GameObject donor = WorkdayDirector.SpawnBodyCopy("Irosene", "Donante (prueba)");
        int n0 = bank.stock.Count;
        string g1 = bank.Donate(donor), g2 = bank.Donate(donor);
        TestProbe.Check("donar: hematíes a la nevera y plasma al congelador; no otra vez antes de 56 días", g1 != null && g2 == null && bank.stock.Count == n0 + 2, $"grupo {g1}; existencias {n0} → {bank.stock.Count}");
        Destroy(donor);
        HospitalSubmarine sub = HospitalSubmarine.All.FirstOrDefault(s => s != null);
        if (sub != null)
        {
            sub.bloodUnits = 1;
            int oNeg = bank.Count(BloodBank.Product.Hematies, "O−");
            bool ran = AreaFunctions.Run("enf_inventario", "Enfermera (prueba)");
            TestProbe.Check("la ronda (dentro del inventario del hospital) repone el submarino con O−", ran && sub.bloodUnits == Mathf.Min(HospitalSubmarine.BloodCapacity, 1 + oNeg), $"submarino {sub.bloodUnits}/{HospitalSubmarine.BloodCapacity} O− (había {oNeg} O− en el banco)");
        }

        // ── 6. SIN SANGRE COMPATIBLE ──
        var saved = bank.stock.ToList();
        bank.stock.Clear();
        GameObject b2 = WorkdayDirector.SpawnBodyCopy("Irosene", "Paciente sin sangre (prueba)");
        b2.AddComponent<ChecklistProbeAnima>(); BloodLoss bl2 = b2.AddComponent<BloodLoss>(); bl2.lost = 0.42f;
        int none = bank.Transfuse(b2.GetComponent<Anima>(), "prueba");
        TestProbe.Check("sin sangre compatible no transfunde (y avisa: banco de sangre andante)", none == 0 && bl2.lost >= 0.42f);
        bank.stock.AddRange(saved);
        Destroy(b2);
        TestProbe.End();
    }
}
