using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL LOTE ESTÁTICO (docs/rendimiento.md):
/// 1. al cargar, las piezas quietas están en lotes, con su GameObject, su collider y su MeshRenderer encendido (solo
///    forceRenderingOff): se siguen encontrando por nombre;
/// 2. el lote cubre de verdad a la pieza (su malla la contiene);
/// 3. lo que se mueve o tiene vida propia no entra (cuerpos de animas, grupos de animales, colmenas);
/// 4. AUTOCURACIÓN: una máscara que se coge del perchero y una pieza que se mueve salen del lote y vuelven a dibujarse solas;
/// 5. apagar y encender en vivo (la medida A/B) deja las piezas como estaban.
/// </summary>
public class LoteEstaticoTest : MonoBehaviour, ITestUnit
{
    public int Group => 112;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<LoteEstaticoTest>() == null) tr.gameObject.AddComponent<LoteEstaticoTest>();
    }

    // Una vuelta de la barrida: acotada en tiempo real (a 3 FPS una vuelta entera tarda).
    static IEnumerator Until(System.Func<bool> done, float seconds = 40f)
    {
        float t0 = Time.realtimeSinceStartup;
        while (!done() && Time.realtimeSinceStartup - t0 < seconds) yield return null;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Lote estático: las piezas quietas se dibujan juntas y siguen ahí");
        yield return Until(() => LoteEstatico.Pieces > 0, 8f);
        TestProbe.Check("hay piezas en lotes", LoteEstatico.Pieces > 0 && LoteEstatico.Groups > 0, LoteEstatico.LastReport);
        TestProbe.Check("muchas menos mallas que piezas", LoteEstatico.Groups * 5 < LoteEstatico.Pieces, $"{LoteEstatico.Pieces} piezas en {LoteEstatico.Groups} lotes");

        MeshRenderer[] all = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        MeshRenderer[] batched = all.Where(r => r.forceRenderingOff && r.enabled).ToArray();
        TestProbe.Check("las piezas siguen encendidas (solo forceRenderingOff)", batched.Length >= LoteEstatico.Pieces / 2, $"{batched.Length} de {all.Length}");

        MaskStation st = FindObjectsByType<MaskStation>(FindObjectsSortMode.None).FirstOrDefault(s => s.GetComponentsInChildren<MeshRenderer>().Any(r => r.forceRenderingOff && r.name == "Visor"));
        Transform mask = null;
        if (st != null) foreach (Transform c in st.transform) if (c.name.StartsWith("Mascara") && c.gameObject.activeSelf) mask = c;
        TestProbe.Check("un perchero de máscaras está en lotes y su máscara se encuentra por nombre", mask != null && mask.Find("Marco") != null);

        Anima[] animas = FindObjectsByType<Anima>(FindObjectsSortMode.None);
        bool bodies = animas.Take(20).All(a => a.GetComponentsInChildren<MeshRenderer>().All(r => !r.name.StartsWith("Lote_")));
        TestProbe.Check("los cuerpos de las animas no van en lotes", bodies);
        bool swarms = Swarm.All.All(s => s == null || s.GetComponentsInChildren<MeshRenderer>().All(r => !r.name.StartsWith("Lote_")));
        TestProbe.Check("los grupos de animales no van en lotes", swarms);

        if (mask != null)
        {
            MeshRenderer piece = mask.Find("Marco").GetComponent<MeshRenderer>();
            Bounds pb = piece.bounds;
            bool covered = all.Any(r => r != null && r.enabled && r.name.StartsWith("Lote_") && r.bounds.Intersects(pb));
            TestProbe.Check("hay un lote donde está la pieza", covered);

            int before = LoteEstatico.Ejected;
            mask.gameObject.SetActive(false);
            yield return Until(() => !piece.forceRenderingOff);
            TestProbe.Check("una máscara cogida sale del lote", !piece.forceRenderingOff && LoteEstatico.Ejected > before, $"devueltas {LoteEstatico.Ejected - before}");
            mask.gameObject.SetActive(true);
            yield return null;
            TestProbe.Check("devuelta al perchero, se dibuja por sí misma", piece.enabled && !piece.forceRenderingOff && piece.gameObject.activeInHierarchy);
        }

        MeshRenderer loose = batched.FirstOrDefault(r => r != null && r.forceRenderingOff && r.GetComponentInParent<FlyingCarpet>() == null && r.GetComponentInParent<TreeRotator>() == null);
        if (loose != null)
        {
            loose.transform.position += Vector3.up * 0.25f;
            yield return Until(() => !loose.forceRenderingOff);
            TestProbe.Check("una pieza que se mueve sale del lote y se ve donde está", !loose.forceRenderingOff, loose.name);
            loose.transform.position -= Vector3.up * 0.25f;
        }

        LoteEstatico.SetLive(false);
        yield return null;
        MeshRenderer one = batched.FirstOrDefault(r => r != null && r.enabled);
        TestProbe.Check("en vivo, sin lotes: ninguna pieza queda oculta", batched.All(r => r == null || !r.forceRenderingOff), one != null ? one.name : "");
        LoteEstatico.SetLive(true);
        yield return null;
        TestProbe.Check("y al volver, las piezas vuelven a sus lotes", batched.Count(r => r != null && r.forceRenderingOff) > 0 && LoteEstatico.Live);
        TestProbe.End();
    }
}
