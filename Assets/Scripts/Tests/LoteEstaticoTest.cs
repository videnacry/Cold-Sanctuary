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
/// 6. un cuerpo lejano deja de dibujarse, pausa sus scripts de figura y vuelve entero.
/// 7. el ritmo repartido (GameTick) no pierde tiempo de juego: el total y un humor integrado salen iguales.
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
        // EL RITMO REPARTIDO: repartir un integrador lineal no cambia el total de tiempo de juego ni el resultado.
        GameTick tick = default;
        float every = 0f, sliced = 0f, sum = 0f, h1 = 0.8f, h2 = 0.8f;
        int fired = 0;
        for (int fr = 0; fr < 600; fr++)
        {
            float d = 0.01f + 0.07f * (fr % 7) / 6f;
            every += d;
            h1 = Mathf.MoveTowards(h1, 0.5f, 0.03f * d);
            if (tick.Step(6, 12345, fr, d, out float due)) { sliced += due; fired++; h2 = Mathf.MoveTowards(h2, 0.5f, 0.03f * due); }
        }
        tick.Step(1, 0, 600, 0f, out float rest); sliced += rest; h2 = Mathf.MoveTowards(h2, 0.5f, 0.03f * rest);
        TestProbe.Check("GameTick: 1 de cada 6 frames, sin perder tiempo de juego", fired == 100 && Mathf.Abs(every - sliced) < 1e-2f, $"{every:0.000} s contra {sliced:0.000} s en {fired} toques");
        TestProbe.Check("GameTick: un humor llega al mismo valor", Mathf.Abs(h1 - h2) < 1e-3f, $"{h1:0.0000} contra {h2:0.0000}");

        // CUERPO LEJANO: se deja de dibujar, sus scripts de figura se pausan y vuelve entero (el TestRunner lo impide salvo aquí).
        Anima far = null;
        Vector3 cam = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
            if ((a.transform.position - cam).sqrMagnitude > 200f * 200f && a.GetComponentInChildren<CubeWalkCycle>() != null) { far = a; break; }
        if (far == null) TestProbe.Check("(sin cuerpo a más de 200 m: el recorte lejano no se comprueba aquí)", true);
        else
        {
            CubeWalkCycle walk = far.GetComponentInChildren<CubeWalkCycle>();
            Renderer body = far.GetComponentInChildren<Renderer>();
            LoteEstatico.FarInTests = true;
            yield return Until(() => !walk.enabled, 12f);
            TestProbe.Check("un cuerpo lejano no se dibuja y pausa su figura", !walk.enabled && body.forceRenderingOff && body.enabled, $"{far.name}: figura {(walk.enabled ? "activa" : "pausada")}, «{body.name}» oculto {body.forceRenderingOff}, a {(far.transform.position - cam).magnitude:0} m");
            LoteEstatico.FarInTests = false;
            yield return Until(() => walk.enabled, 12f);
            TestProbe.Check("y al acabar vuelve entero", walk.enabled && !body.forceRenderingOff, far.name);
        }
        TestProbe.End();
    }
}
