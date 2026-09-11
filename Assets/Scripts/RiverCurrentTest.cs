using System.Collections;
using UnityEngine;

/// <summary>
/// Test del RÍO (docs/microcosmos-sakshi-origin.md §4, testing-checklist §42), por `TestProbe`: la corriente
/// ARRASTRA a la cría pequeña (masa baja) aguas abajo pero la hormiga GRANDE (masa alta) AGUANTA (combate de stats),
/// y el arrastre DEBILITA (drena ATP). Determinista: llama a `RiverCurrent.ApplyTo` directo (no depende del barrido
/// por física ni de NavMesh). Las ánimas se crean LEJOS de la zona para que el `Update` del río no interfiera.
/// </summary>
public class RiverCurrentTest : MonoBehaviour, ITestUnit
{
    public int Group => 9;
    public bool ParallelSafe => false;

    public IEnumerator Run()
    {
        TestProbe.Begin("Rio (corriente: arrastra a la cria, la grande aguanta, debilita)");
        yield return null;

        // Zona del río (con collider, exigido por RiverCurrent). Flujo +Z, fuerza 3, resistencia por masa.
        GameObject riverGO = new GameObject("River_TEST");
        riverGO.AddComponent<BoxCollider>().size = new Vector3(2f, 2f, 2f);
        RiverCurrent river = riverGO.AddComponent<RiverCurrent>();
        river.flowDirection = Vector3.forward; river.pushPower = 3f; river.massResist = 1f;
        river.dragEnergyPerSecond = 2f; river.driftSpeed = 1.5f;

        // Cría pequeña (masa 0.2 → net 2.8 > 0 → arrastrada) y hormiga grande (masa 5 → net -2 → aguanta). LEJOS del río.
        GameObject cria  = MakeBeing("Cria_TEST",  new Vector3(100f, 0f, 0f), 0.2f);
        GameObject grande = MakeBeing("Grande_TEST", new Vector3(120f, 0f, 0f), 5f);
        yield return null;   // deja correr Awake (CharacterLevel fija su energía)

        Anima criaA = cria.GetComponent<Anima>();
        Anima grandeA = grande.GetComponent<Anima>();
        CharacterLevel criaCL = cria.GetComponent<CharacterLevel>();
        criaCL.currentEnergy = 10f;   // energía conocida para medir el drenaje

        float criaZ0 = cria.transform.position.z;
        float grandeZ0 = grande.transform.position.z;
        float energy0 = criaCL.currentEnergy;

        TestProbe.Greater("la corriente vence a la cria (empuje neto > 0)", river.NetPush(criaA), 0f);
        TestProbe.Check("la hormiga grande AGUANTA (empuje neto <= 0)", river.NetPush(grandeA) <= 0f);

        for (int i = 0; i < 10; i++) { river.ApplyTo(criaA, 0.1f); river.ApplyTo(grandeA, 0.1f); }

        TestProbe.Greater("la cria es ARRASTRADA aguas abajo (+Z)", cria.transform.position.z - criaZ0, 0.05f);
        TestProbe.Check("la hormiga grande NO se mueve", Mathf.Abs(grande.transform.position.z - grandeZ0) < 0.001f);
        TestProbe.Greater("el arrastre DEBILITA la cria (drena ATP)", energy0 - criaCL.currentEnergy, 0f);

        Object.Destroy(riverGO); Object.Destroy(cria); Object.Destroy(grande);
        TestProbe.End();
    }

    static GameObject MakeBeing(string name, Vector3 pos, float mass)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.position = pos;
        SimpleAnima a = go.AddComponent<SimpleAnima>();
        a.bodyMass = mass;
        go.AddComponent<CharacterLevel>();   // para el drenaje de ATP (sin ImpulseController → ruta directa de arrastre)
        return go;
    }
}
