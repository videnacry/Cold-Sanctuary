using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEST DE «CÓMO NACIÓ LA COCINA» (<see cref="KitchenOriginStory"/>, docs/novela.md): la historia jugable del microcosmos de la
/// Cocina. Entra al microcosmos como lo hace el jugador al final de la iniciación de la cocina (MobWorldLoader.EnterMobWorld,
/// si «Microcosmos_Cocina» está en Build Settings); si no, monta la historia suelta, lejos, sobre un suelo de prueba (la
/// historia crea las piezas que falten). Juega los beats como el jugador: se acerca a cada resto y pulsa E, lo lleva al hogar y
/// pulsa E; en la vigilia alimenta la llama con E; los beats guionizados se saltan (<see cref="KitchenOriginStory.SkipWait"/>).
/// Comprueba: (1) los beats avanzan en orden (Intro → Carry → Accident → Vigil → Circle → Farewell → Done); (2) el jugador
/// ayuda de verdad (entrega <see cref="KitchenOriginStory.ScrapsNeeded"/> restos y alimenta la llama sin que se acabe el
/// tiempo); (3) Nasatya también lleva restos; (4) la historia termina y pide la vuelta al mesocosmos, y el jugador vuelve.
/// Cada espera, acotada. Se añade solo al orquestador cuando se piden las pruebas (como <see cref="KitchenInitiationTest"/>).
/// </summary>
public class KitchenOriginStoryTest : MonoBehaviour, ITestUnit
{
    public int Group => 44;   // tras el piso de los microcosmos (43): entra y sale de un microcosmos
    public bool ParallelSafe => false;
    public float speed = 6f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<KitchenOriginStoryTest>() == null) tr.gameObject.AddComponent<KitchenOriginStoryTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Cómo nació la cocina: historia jugable del microcosmos de la Cocina → vuelta al mesocosmos");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { TestProbe.Check("hay jugador", false); TestProbe.End(); yield break; }

        // La Magnate, quieta (no se lo lleve a otra parte mientras juega la historia).
        Magnate[] mags = FindObjectsByType<Magnate>(FindObjectsSortMode.None);
        foreach (Magnate m in mags) m.enabled = false;
        MobWorldLoader mwl = MobWorldLoader.Instance;
        if (mwl.IsInMobWorld) { mwl.ExitMobWorld(); yield return Until(() => !mwl.IsInMobWorld && !mwl.IsBusy, 30f); }
        yield return Until(() => !mwl.IsBusy, 15f);
        Vector3 home = player.transform.position;
        // (En un piso del Árbol de los Campos, que gira, «donde estaba» es el mismo sitio DEL PISO: se mide en sus coordenadas.)
        Transform homeFloor = MobWorldLoader.MovingFloorUnder(home);
        Vector3 homeLocal = homeFloor != null ? homeFloor.InverseTransformPoint(home) : home;
        float oldScale = Time.timeScale;
        string exitedScene = null;
        System.Action<string> onExit = s => exitedScene = s;
        MobWorldLoader.Exited += onExit;

        // ── Entrar: por el tanque (la escena real) o, sin ella, la historia suelta ──
        GameObject temp = null;
        bool viaScene = Application.CanStreamedLevelBeLoaded(AreaInitiator.KitchenMicrocosmos);
        if (viaScene)
        {
            mwl.EnterMobWorld(AreaInitiator.KitchenMicrocosmos);
            yield return Until(() => mwl.IsInMobWorld && !mwl.IsBusy, 60f);
            viaScene = mwl.IsInMobWorld && mwl.ActiveScene == AreaInitiator.KitchenMicrocosmos;
            TestProbe.Check("entra al microcosmos de la Cocina", viaScene, mwl.ActiveScene ?? "fuera");
        }
        else Debug.Log("[Prueba] «Microcosmos_Cocina» no está en Build Settings: la historia se juega suelta, sobre un suelo de prueba.");
        if (!viaScene)
        {
            temp = new GameObject("Prueba_origen_cocina"); temp.transform.position = new Vector3(9000f, 0f, 9000f);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name = "Suelo_prueba";
            floor.transform.SetParent(temp.transform, false); floor.transform.localPosition = Vector3.down * 0.5f; floor.transform.localScale = new Vector3(120f, 1f, 120f);
            var cast = new GameObject("Elenco"); cast.transform.SetParent(temp.transform, false);
            cast.AddComponent<KitchenOriginStory>();
            WorkdayDirector.Teleport(player.transform, temp.transform.position + new Vector3(0f, 1f, -14f));
        }

        yield return Until(() => KitchenOriginStory.Instance != null && KitchenOriginStory.Instance.History.Count > 0, 15f);
        KitchenOriginStory st = KitchenOriginStory.Instance;
        if (st == null)
        {
            TestProbe.Check("el microcosmos de la Cocina tiene su historia (KitchenOriginStory)", false, "¿escena de antes? Reconstruye los microcosmos");
            yield return Leave(player, home, temp, viaScene, onExit, oldScale, mags);
            TestProbe.End(); yield break;
        }
        TestProbe.Check("la historia empieza en la Intro, con el hogar y Nasatya", st.Now == KitchenOriginStory.Beat.Intro && st.Hearth != null && st.Nasatya != null, st.Now.ToString());
        Time.timeScale = speed;

        // ── 1. Intro → Carry ──
        st.SkipWait();
        yield return Until(() => st.Now == KitchenOriginStory.Beat.Carry, 20f);
        TestProbe.Check("tras la intro, Nasatya pide ayuda (Carry)", st.Now == KitchenOriginStory.Beat.Carry, st.Now.ToString());

        // ── 2. Llevar restos al hogar: E junto al resto, E junto al fuego ──
        for (int i = 0; i < KitchenOriginStory.ScrapsNeeded + 6 && st.Now == KitchenOriginStory.Beat.Carry && st.ScrapsDelivered < KitchenOriginStory.ScrapsNeeded; i++)
        {
            if (!st.Carrying)
            {
                Transform s = st.NextScrap(player.transform.position);
                if (s == null) { yield return null; continue; }
                Vector3 at = s.position + Vector3.back; at.y = s.position.y + 0.6f;
                WorkdayDirector.Teleport(player.transform, at);
                yield return Frames(3);
                string r = st.Act();
                if (!st.Carrying) { Debug.Log($"[Prueba] (E) junto al resto: {r}"); continue; }
            }
            Vector3 h = st.Hearth.position + Vector3.back * 2.5f; h.y = st.Hearth.position.y + 0.6f;
            WorkdayDirector.Teleport(player.transform, h);
            yield return Frames(3);
            st.Act();
            yield return Frames(2);
        }
        TestProbe.Check("el jugador ayuda: lleva los restos al hogar", st.ScrapsDelivered >= KitchenOriginStory.ScrapsNeeded && !st.TimedOut, $"{st.ScrapsDelivered}/{KitchenOriginStory.ScrapsNeeded}");
        yield return Until(() => st.Now != KitchenOriginStory.Beat.Carry, 10f);
        TestProbe.Check("con los restos en el hogar, el grano cae junto a la brasa (Accident)", st.Now == KitchenOriginStory.Beat.Accident, st.Now.ToString());

        // ── 3. La vigilia del fuego: alimentar la llama ──
        st.SkipWait();
        yield return Until(() => st.Now == KitchenOriginStory.Beat.Vigil, 10f);
        TestProbe.Check("empieza la vigilia del fuego", st.Now == KitchenOriginStory.Beat.Vigil, st.Now.ToString());
        Vector3 hv = st.Hearth.position + Vector3.back * 2.5f; hv.y = st.Hearth.position.y + 0.6f;
        WorkdayDirector.Teleport(player.transform, hv);
        yield return Frames(3);
        for (int k = 0; k < KitchenOriginStory.FeedsNeeded + 4 && st.Now == KitchenOriginStory.Beat.Vigil; k++)
        {
            yield return Until(() => st.CanFeed, 5f);
            st.Act();
            yield return Frames(2);
        }
        yield return Until(() => st.Now != KitchenOriginStory.Beat.Vigil, 10f);
        TestProbe.Check("el jugador mantiene viva la llama", st.Feeds >= KitchenOriginStory.FeedsNeeded && !st.TimedOut, $"{st.Feeds}/{KitchenOriginStory.FeedsNeeded}, llama {st.Flame:0.00}");
        TestProbe.Check("Nasatya también lleva restos al hogar", st.NasatyaDelivered > 0, $"{st.NasatyaDelivered}");

        // ── 4. El círculo, la despedida y la vuelta ──
        st.SkipWait();
        yield return Until(() => st.Now == KitchenOriginStory.Beat.Farewell, 10f);
        st.SkipWait();
        yield return Until(() => st.Now == KitchenOriginStory.Beat.Done, 10f);
        var want = new List<KitchenOriginStory.Beat> { KitchenOriginStory.Beat.Intro, KitchenOriginStory.Beat.Carry, KitchenOriginStory.Beat.Accident, KitchenOriginStory.Beat.Vigil, KitchenOriginStory.Beat.Circle, KitchenOriginStory.Beat.Farewell, KitchenOriginStory.Beat.Done };
        bool inOrder = st.History.Count == want.Count;
        for (int i = 0; inOrder && i < want.Count; i++) inOrder = st.History[i] == want[i];
        TestProbe.Check("los beats avanzan en orden, sin saltarse ninguno", inOrder, string.Join(" → ", st.History));
        TestProbe.Check("la historia termina y pide volver al mesocosmos", st.Now == KitchenOriginStory.Beat.Done && st.ExitRequested);
        if (viaScene)
        {
            yield return Until(() => !mwl.IsInMobWorld && !mwl.IsBusy, 30f);
            TestProbe.Check("el jugador vuelve al mesocosmos (sale del microcosmos de la Cocina)", !mwl.IsInMobWorld && exitedScene == AreaInitiator.KitchenMicrocosmos, exitedScene ?? "sigue dentro");
            yield return Until(() => false, 1.5f);   // (y se queda: si reaparece en el aire, ya ha caído)
            Vector3 homeNow = homeFloor != null ? homeFloor.TransformPoint(homeLocal) : home;
            TestProbe.Check("reaparece donde estaba antes de entrar", Vector3.Distance(player.transform.position, homeNow) < 5f, $"{Vector3.Distance(player.transform.position, homeNow):0.0} m{(homeFloor != null ? " (en su piso del árbol, que gira)" : "")}");
        }
        yield return Leave(player, home, temp, viaScene, onExit, oldScale, mags);
        TestProbe.End();
    }

    IEnumerator Leave(GameObject player, Vector3 home, GameObject temp, bool viaScene, System.Action<string> onExit, float scale, Magnate[] mags)
    {
        Time.timeScale = scale;
        MobWorldLoader.Exited -= onExit;
        if (viaScene && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 30f); }
        if (temp != null) { WorkdayDirector.Teleport(player.transform, home); Destroy(temp); }
        LessonPrompt.Show("", null, 0.1f);
        foreach (Magnate m in mags) if (m != null) m.enabled = true;
    }

    static IEnumerator Frames(int n) { for (int i = 0; i < n; i++) yield return null; }

    static IEnumerator Until(System.Func<bool> cond, float seconds)
    {
        float end = Time.time + seconds;
        while (!cond() && Time.time < end) yield return null;
    }
}
