using System.Collections;
using UnityEngine;

/// <summary>
/// VIDA DEL SANTUARIO: construcciones elevadas con colchón de musgo (las caídas no matan), dormitorios para cada persona,
/// energía solar (nada de gas: cocinas de inducción), insectos de hielo a tamaño modificado y la energía que se recupera
/// del propio cuerpo (metabolismo).
/// </summary>
public class SanctuaryLifeTest : MonoBehaviour, ITestUnit
{
    public int Group => 28;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Vida del santuario (elevado, musgo, dormitorios, solar, insectos de hielo, energía)");
        yield return null;

        // Caídas: 7 m sobre hielo hacen daño; sobre el colchón de musgo, casi nada.
        GameObject probe = new GameObject("Caida_TEST");
        probe.AddComponent<CharacterController>();
        FallImpact fi = probe.AddComponent<FallImpact>();
        float hard = fi.Land(7f, 0f), soft = fi.Land(7f, 0.85f);
        TestProbe.Greater("una caída de 7 m sobre hielo hace daño", hard, 30f, $"{hard:0}");
        TestProbe.Check("sobre el colchón de musgo el golpe se amortigua", soft < hard * 0.2f, $"{soft:0} vs {hard:0}");
        TestProbe.Check("saltar un escalón no hace daño", fi.Land(1f, 0f) == 0f);
        Destroy(probe);
        TestProbe.Greater("hay colchones de musgo bajo lo construido", FindObjectsByType<SoftGround>(FindObjectsSortMode.None).Length, 6f);

        // Áreas elevadas.
        GameObject kitchen = GameObject.Find("Kitchen_Area");
        TestProbe.Check("la cocina está elevada (≥ 5 m)", kitchen != null && kitchen.transform.position.y >= 5f, kitchen != null ? $"{kitchen.transform.position.y:0.0} m" : "sin cocina");
        GameObject garden = GameObject.Find("Garden_Area");
        TestProbe.Check("el huerto sigue a ras de suelo", garden != null && garden.transform.position.y < 1f);

        // Dormitorios.
        TestProbe.Greater("hay un dormitorio por persona (12)", Bedroom.All.Count, 11f);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Bedroom mine = player != null ? Bedroom.Of(player.GetComponent<Anima>()) : null;
        TestProbe.Check("Kushal tiene su habitación", mine != null && mine.bed != null, mine != null ? mine.name : "ninguna");

        // Solar e inducción.
        TestProbe.Greater("hay paneles solares (uno por edificio + campo)", FindObjectsByType<SolarArray>(FindObjectsSortMode.None).Length, 6f);
        bool flame = false;
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (t.name == "Llama") { flame = true; break; }
        TestProbe.Check("las cocinas son de inducción (sin llama de gas)", !flame);

        // Insectos de hielo a tamaño modificado.
        int bees = 0; float maxBee = 0f; Vector3 firstScale = Vector3.zero;
        foreach (IceInsectLife b in FindObjectsByType<IceInsectLife>(FindObjectsSortMode.None))
        {
            if (b.kind != IceInsectLife.Kind.Bee) continue;
            bees++; if (bees == 1) firstScale = b.transform.lossyScale;
            Bounds bb = new Bounds(b.transform.position, Vector3.zero);
            foreach (Renderer r in b.GetComponentsInChildren<Renderer>()) if (r.enabled) bb.Encapsulate(r.bounds);
            maxBee = Mathf.Max(maxBee, Mathf.Max(bb.size.x, bb.size.z));
        }
        TestProbe.Greater("las abejas de las nieves viven (vuelan) en el bosque", bees, 0f);
        TestProbe.Check("la abeja de las nieves mide ~20 cm", bees == 0 || (maxBee > 0.1f && maxBee < 0.45f), $"{maxBee:0.00} m (escala {firstScale})");

        // Energía = metabolismo: sin grasa, se recupera gastando masa del cuerpo.
        GameObject g = new GameObject("Metabolismo_TEST");
        Anima an = g.AddComponent<Anima>(); an.fatReserves = 0f; an.bodyMass = 1f;
        CharacterLevel cl = g.AddComponent<CharacterLevel>();
        ATPRegenSpell regen = g.AddComponent<ATPRegenSpell>();
        yield return null;
        cl.currentEnergy = 0f;
        float t0 = Time.time;
        while (Time.time - t0 < 1f) yield return null;
        TestProbe.Greater("sin grasa, la energía se recupera…", cl.currentEnergy, 0.1f);
        TestProbe.Check("…pero gastando masa del propio cuerpo", an.bodyMass < 1f, $"masa {an.bodyMass:0.0000}");
        Destroy(g);

        // Tanque de virtualización: pecera con escotilla horizontal; al abrirla se enciende el campo que sujeta el agua.
        VirtualizationTank tank = FindAnyObjectByType<VirtualizationTank>();
        TestProbe.Check("hay tanque de virtualización (pecera)", tank != null);
        if (tank != null)
        {
            yield return tank.Hatch(true);
            TestProbe.Check("la escotilla horizontal se abre con el campo magnético encendido", tank.HatchOpen && tank.FieldOn);
            yield return tank.Hatch(false);
            TestProbe.Check("…y se cierra (campo apagado)", !tank.HatchOpen && !tank.FieldOn);
        }

        // Carne cultivada: el laboratorio pasa por todas las fases y deja raciones de carne en el comedero de los depredadores.
        CulturedMeatLab lab = CulturedMeatLab.All.Count > 0 ? CulturedMeatLab.All[0] : null;
        TestProbe.Check("hay laboratorio de carne cultivada", lab != null);
        if (lab != null)
        {
            float oldSpeed = lab.speed; lab.speed = 400f;
            bool sawProliferation = false;
            float t1 = Time.time;
            while (lab.portionsMade == 0 && Time.time - t1 < 8f) { sawProliferation |= lab.stage == CulturedMeatLab.Stage.Proliferacion; yield return null; }
            lab.speed = oldSpeed;
            TestProbe.Check("el lote pasa por la proliferación en biorreactor", sawProliferation);
            TestProbe.Greater("…y cosecha raciones de carne para los depredadores", lab.portionsMade, 0f, $"{lab.portionsMade} raciones");
        }
        TestProbe.End();
    }
}
