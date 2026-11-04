using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LAS MÁSCARAS, LAS PUERTAS GRANDES Y EL FONDO ABIERTO de los edificios-árbol (Berón 2026-11-03; docs/mascaras-de-oxigeno.md):
/// 1. PUESTOS: en cada puerta de cada árbol, un perchero de máscaras fuera y una estación dentro del agua.
/// 2. PUERTAS Y RAMAS CUADRADAS para 8 personas a la vez: 8 cuerpos (cápsula de 0,35 m) caben en fila por la puerta del suelo.
/// 3. SIN SUELO: bajo el tronco no hay nada sólido (se baja nadando al mar), el agua sigue y el hueco no se anda (NavMesh).
/// 4. UN PERSONAJE coge la máscara en la entrada, nada del suelo a un piso con aliento de 4 s sin gastarlo y la deja al salir.
/// 5. EL JUGADOR coge la máscara en el perchero, entra y bucea con aliento ilimitado, sale y la deja.
/// 6. RESCATE: un personaje en el agua sin máscara nada a la estación más cercana y se la pone.
/// 7. KUSHAL BAJO EL AGUA SIN MÁSCARA (la escena Prueba_Mascaras empieza así; en SampleScene se le pone ahí): va nadando a la
///    estación más cercana, se pone la máscara y tiene oxígeno ilimitado.
/// </summary>
public class TreeMaskTest : MonoBehaviour, ITestUnit
{
    public int Group => 70;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<TreeMaskTest>() == null) tr.gameObject.AddComponent<TreeMaskTest>();
    }

    static IEnumerator Wait(float s) { float t0 = Time.time, r0 = Time.realtimeSinceStartup; while (Time.time - t0 < s && Time.realtimeSinceStartup - r0 < s * 3f + 2f) yield return null; }

    public IEnumerator Run()
    {
        TestProbe.Begin("Edificios-árbol: máscaras de oxígeno, puertas de 8 personas y fondo abierto al mar");
        ArbolCampos t = ArbolCampos.Instance;
        if (t == null) { TestProbe.Check("hay un edificio-árbol en la escena", false); TestProbe.End(); yield break; }
        TreeRotator rot = TreeRotator.Instance; float oldHours = rot != null ? rot.hoursPerTurn : 0f;
        if (rot != null) rot.hoursPerTurn = 0f;
        float oldScale = Time.timeScale;
        Physics.SyncTransforms();

        // ── 1. PUESTOS DE MÁSCARAS ──
        int doors = 0, withRack = 0, withStation = 0; string miss = "";
        foreach (ArbolCampos tr in ArbolCampos.All)
            foreach (TreeDoorExit d in tr.GetComponentsInChildren<TreeDoorExit>())
            {
                doors++;
                MaskStation[] ms = d.GetComponentsInChildren<MaskStation>();
                MaskStation rack = ms.FirstOrDefault(m => m.kind == MaskStation.Kind.Rack), st = ms.FirstOrDefault(m => m.kind == MaskStation.Kind.Station);
                bool rOk = rack != null && !WaterZone.Underwater(rack.SwimPoint + Vector3.up) && rack.Masks > 0;
                bool sOk = st != null && st.Underwater && st.Masks > 0;
                if (rOk) withRack++; if (sOk) withStation++;
                if ((!rOk || !sOk) && miss.Length < 260) miss += $"{tr.building}/{d.name}: perchero {(rack == null ? "no" : rOk ? "sí" : $"{(WaterZone.Underwater(rack.SwimPoint + Vector3.up) ? "MOJADO (en " + WaterZone.At(rack.SwimPoint + Vector3.up)?.name + ")" : "seco")}, {rack.Masks} máscaras")}, estación {(st == null ? "no" : sOk ? "sí" : "fuera del agua o vacía")}; ";
            }
        TestProbe.Check("cada puerta de cada edificio-árbol tiene su perchero de máscaras fuera (en seco, con máscaras)", doors > 0 && withRack == doors, $"{withRack}/{doors} puertas en {ArbolCampos.All.Count} árboles; {miss}");
        TestProbe.Check("y su estación de máscaras dentro, en el agua junto a la puerta (una en cada nivel)", doors > 0 && withStation == doors, $"{withStation}/{doors}");

        // ── 2. PUERTAS Y RAMAS PARA 8 PERSONAS ──
        float need = TreeSpec.PeopleAtOnce * TreeSpec.PersonWidth + (TreeSpec.PeopleAtOnce - 1f) * TreeSpec.PersonGap;
        int sq = 0, all = 0; float minW = 99f;
        foreach (ArbolCampos tr in ArbolCampos.All)
            foreach (CampoMagnetico f in tr.GetComponentsInChildren<CampoMagnetico>())
            {
                all++; Vector3 s = f.transform.lossyScale; minW = Mathf.Min(minW, s.x);
                if (Mathf.Abs(s.x - s.y) < 0.05f && s.x >= need - 0.01f) sq++;
            }
        HelixTower h = TreeSpec.Helix;
        float tubeW = 2f * (h.tubeHalf - h.tubeWall);
        TestProbe.Check($"todas las puertas son CUADRADAS y caben 8 personas a la vez ({TreeSpec.PeopleAtOnce:0} × {TreeSpec.PersonWidth:0.00} m + 7 × {TreeSpec.PersonGap:0.00} m = {need:0.00} m)",
            all > 0 && sq == all, $"{sq}/{all} puertas; la más estrecha, {minW:0.0} m");
        TestProbe.Check("las ramas son cuadradas y más anchas que las puertas (8 nadadores a la vez)", Mathf.Abs(h.TubeInnerH - tubeW) < 0.01f && tubeW >= need && tubeW >= TreeSpec.DoorWidth,
            $"hueco de la rama {tubeW:0.0} × {h.TubeInnerH:0.0} m");
        {
            TreeLevel g = t.Ground;
            Vector3 c = t.World(g.door), across = t.transform.TransformDirection(HelixTower.FromSide(g.side, Vector3.right)), outw = t.transform.TransformDirection(HelixTower.FromSide(g.side, Vector3.forward));
            int free = 0;
            for (int i = 0; i < 8; i++)
            {
                float x = (i - 3.5f) * (TreeSpec.PersonWidth + TreeSpec.PersonGap);
                Vector3 p = c + across * x + Vector3.up * 0.05f;
                bool blocked = false;
                foreach (Collider col in Physics.OverlapCapsule(p + Vector3.up * 0.4f - outw * 0.6f, p + Vector3.up * 1.45f + outw * 0.6f, 0.34f, ~0, QueryTriggerInteraction.Ignore))
                    if (col.transform.IsChildOf(t.transform) && col.name != "Umbral_puerta") { blocked = true; break; }
                if (!blocked) free++;
            }
            TestProbe.Check("8 cuerpos de 0,7 m en fila cruzan a la vez el hueco de la puerta del suelo sin tocar el marco", free == 8, $"{free}/8 libres");
        }

        // ── 3. SIN SUELO ──
        {
            Vector3 c = t.transform.position;
            bool solid = Physics.SphereCast(c + Vector3.up * 4f, 0.6f, Vector3.down, out RaycastHit hit, 11f, ~0, QueryTriggerInteraction.Ignore);
            TestProbe.Check("el tronco no tiene suelo: desde dentro se baja nadando al mar (nada sólido bajo el agua del tronco)", t.bottomHole > 1f && !solid,
                solid ? $"choca con «{hit.collider.name}» a {hit.point.y:0.0} m" : $"hueco de {2f * t.bottomHole:0.#} m en el hielo");
            bool water = WaterZone.Underwater(c + Vector3.up * 2f) && WaterZone.Underwater(c + Vector3.down * 1f) && WaterZone.Underwater(c + Vector3.down * 6f) && t.InRootWater(c + Vector3.down * 1f);
            TestProbe.Check("el agua sigue del tronco al mar por el hueco", water);
            bool hole = !UnityEngine.AI.NavMesh.SamplePosition(c, out _, 1.5f, UnityEngine.AI.NavMesh.AllAreas);
            int outsideOk = 0, outsideN = 0;
            foreach (ArbolCampos tr in ArbolCampos.All)
            {
                TreeLevel gl = tr.Ground; if (gl == null) continue; outsideN++;
                Vector3 o = tr.World(gl.outside + TreeSpec.Dir(gl.doorAngle) * 1.5f);
                if (UnityEngine.AI.NavMesh.SamplePosition(o, out UnityEngine.AI.NavMeshHit nh, 1.5f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(nh.position.y - o.y) < 0.6f) outsideOk++;
            }
            TestProbe.Check("el NavMesh no pisa el hueco y sigue llegando a la puerta del suelo de cada árbol", hole && outsideOk == outsideN, $"hueco sin NavMesh {hole}; {outsideOk}/{outsideN} puertas del suelo con NavMesh");
        }

        // ── 4. UN PERSONAJE: máscara en la entrada, nado sin gastar aliento, la deja al salir ──
        {
            TreeLevel g0 = t.Ground, dest = t.Floor(5) ?? t.levels.FirstOrDefault(l => l.kind == TreeLevel.Kind.Floor);
            GameObject body = new GameObject("Mascara_NPC_TEST");
            CapsuleCollider cap = body.AddComponent<CapsuleCollider>(); cap.height = 1.8f; cap.radius = 0.3f; cap.center = new Vector3(0f, -0.1f, 0f);
            body.transform.position = PathMover.Of(body.transform).OnGround(t.World(g0.outside + TreeSpec.Dir(g0.doorAngle) * 4f) + Vector3.up);
            SwimBody sb = SwimBody.Of(body.transform); sb.breathHoldSeconds = 4f;   // 4 s de aliento: sin máscara se ahogaría en el viaje
            int taken0 = BreathingMask.Taken, ret0 = BreathingMask.Returned;
            Time.timeScale = 3f;
            float minBreath = 1f, wornWet = 0f, wet = 0f; bool tookAtDoor = false;
            Vector3 to = t.World(dest.outside + TreeSpec.Dir(dest.doorAngle) * 1.2f) + Vector3.up;
            float end = Time.time + 300f, endR = Time.realtimeSinceStartup + 150f;
            PathMover pm = PathMover.Of(body.transform);
            while (Time.time < end && Time.realtimeSinceStartup < endR)
            {
                Vector3 d = to - body.transform.position;
                if (new Vector2(d.x, d.z).magnitude < 1.2f && Mathf.Abs(d.y) < 1.6f && !TreeTrip.Busy(body)) break;
                pm.StepToward(to, 3f);
                yield return null;
                if (!tookAtDoor && BreathingMask.Has(body) && !t.InRootWater(body.transform.position)) tookAtDoor = true;
                if (t.InRootWater(body.transform.position)) { wet += Time.deltaTime; if (BreathingMask.Has(body)) wornWet += Time.deltaTime; }
                minBreath = Mathf.Min(minBreath, sb.Breath01);
            }
            Time.timeScale = oldScale;
            yield return Wait(0.5f);
            Vector3 left = to - body.transform.position;
            bool arrived = new Vector2(left.x, left.z).magnitude < 1.6f;
            TestProbe.Check("un personaje coge una máscara del perchero de la entrada ANTES de meterse en el agua", tookAtDoor && BreathingMask.Taken > taken0, $"cogidas {BreathingMask.Taken - taken0}");
            TestProbe.Check("y nada del suelo a un piso con la máscara puesta: con 4 s de aliento, no lo gasta (oxígeno ilimitado)", arrived && wet > 4f && wornWet >= wet - 0.2f && minBreath > 0.95f,
                $"llega {arrived}, {wet:0} s en el agua ({wornWet:0} con máscara), aliento mínimo {minBreath:0.00}");
            TestProbe.Check("al salir por la puerta deja la máscara en el perchero de esa entrada", !BreathingMask.Has(body) && BreathingMask.Returned > ret0, $"devueltas {BreathingMask.Returned - ret0}, la lleva {BreathingMask.Has(body)}");
            Destroy(body);
        }

        // ── 5. EL JUGADOR: perchero → agua → buceo con aliento ilimitado → sale y la deja ──
        {
            TreeLevel g0 = t.Ground;
            GameObject sw = new GameObject("Jugador_mascara_TEST");
            CharacterController scc = sw.AddComponent<CharacterController>(); scc.height = 1.8f; scc.radius = 0.35f; scc.center = Vector3.zero;
            PlayerController spc = sw.AddComponent<PlayerController>();
            sw.tag = "Player";
            yield return null; yield return null;
            BreathingMask bm = BreathingMask.Of(sw.transform);
            MaskStation rack = t.GetComponentsInChildren<MaskStation>().Where(m => m.kind == MaskStation.Kind.Rack).OrderBy(m => Vector3.Distance(m.SwimPoint, t.World(g0.outside))).FirstOrDefault();
            void Put(Vector3 at, Vector3 face) { scc.enabled = false; sw.transform.SetPositionAndRotation(at, Quaternion.LookRotation(face)); scc.enabled = true; Physics.SyncTransforms(); }
            Vector3 inward = -t.transform.TransformDirection(TreeSpec.Dir(g0.doorAngle));
            // Pasa junto al perchero sin entrar: NO la coge (2026-11-04: solo al meterse en el agua, sin rodeos).
            Put(rack != null ? rack.SwimPoint + Vector3.up * 1f : t.World(g0.outside) + Vector3.up, inward);
            yield return Wait(0.6f);
            bool dryTook = bm.Worn;
            // Entra: andando hacia la puerta desde fuera.
            Put(t.World(g0.outside) + Vector3.up * 1.0f, inward);
            spc.scriptedInput = new Vector3(0f, 0f, 1f);
            { float t0 = Time.time, r0 = Time.realtimeSinceStartup; while ((!spc.IsSwimming || !bm.Worn) && Time.time - t0 < 6f && Time.realtimeSinceStartup - r0 < 20f) yield return null; }
            TestProbe.Check("el jugador coge la máscara del perchero de la entrada AL METERSE en el agua (no al pasar junto a él)", !dryTook && bm.Worn && bm.TakenFrom != null && bm.TakenFrom.kind == MaskStation.Kind.Rack,
                $"al pasar en seco {dryTook}; nadando {spc.IsSwimming}, de {bm.TakenFrom?.name ?? "ninguno"}");
            // Bucea 12 s (aliento de 4 s).
            SwimBody psb = SwimBody.Of(sw.transform); psb.breathHoldSeconds = 4f;
            Put(t.World(new Vector3(0f, 12f, 0f)), inward);
            spc.scriptedInput = new Vector3(0f, -0.4f, 0f);
            float minB = 1f; bool under = false;
            { float t0 = Time.time, r0 = Time.realtimeSinceStartup; while (Time.time - t0 < 12f && Time.realtimeSinceStartup - r0 < 30f) { yield return null; minB = Mathf.Min(minB, psb.Breath01); if (!psb.HeadAboveWater) under = true; } }
            TestProbe.Check("el jugador con la máscara bucea 12 s en el tronco con 4 s de aliento y no lo gasta", under && minB > 0.95f && bm.Worn, $"bajo el agua {under}, aliento mínimo {minB:0.00}");
            // Sale por la puerta del suelo y deja la máscara.
            Vector3 gOut = t.transform.TransformDirection(g0.outside - g0.inside); gOut.y = 0f;
            Put(t.World(g0.inside) + Vector3.up * 1.2f, gOut);
            spc.scriptedInput = new Vector3(0f, 0f, 1f);
            { float t0 = Time.time, r0 = Time.realtimeSinceStartup; while ((spc.IsSwimming || bm.Worn) && Time.time - t0 < 8f && Time.realtimeSinceStartup - r0 < 25f) yield return null; }
            spc.scriptedInput = Vector3.zero;
            yield return Wait(0.6f);
            TestProbe.Check("al salir por la puerta, el jugador deja la máscara en el perchero", !spc.IsSwimming && !bm.Worn, $"nadando {spc.IsSwimming}, máscara {bm.Worn}");
            Destroy(sw);
        }

        // ── 6. RESCATE: un personaje en el agua sin máscara ──
        {
            GameObject body = new GameObject("Sin_mascara_TEST");
            CapsuleCollider cap = body.AddComponent<CapsuleCollider>(); cap.height = 1.8f; cap.radius = 0.3f;
            body.transform.position = t.World(new Vector3(2f, 30f, -3f));
            SwimBody sb = SwimBody.Of(body.transform); sb.breathHoldSeconds = 50f;
            BreathingMask bm = BreathingMask.Of(body.transform); bm.Strip();
            int r0 = BreathingMask.Rescues;
            float t0 = Time.time, rr = Time.realtimeSinceStartup;
            Time.timeScale = 2f;
            while (!bm.Worn && Time.time - t0 < 60f && Time.realtimeSinceStartup - rr < 40f) yield return null;
            Time.timeScale = oldScale;
            MaskStation from = bm.TakenFrom;
            TestProbe.Check("un personaje que acaba en el agua sin máscara nada a la estación más cercana y se la pone", bm.Worn && BreathingMask.Rescues > r0 && from != null && from.kind == MaskStation.Kind.Station,
                $"en {Time.time - t0:0} s, de {from?.name ?? "ninguna"} a {(from != null ? Vector3.Distance(from.SwimPoint, t.World(new Vector3(2f, 30f, -3f))) : 0f):0} m");
            Destroy(body);
        }

        // ── 7. KUSHAL BAJO EL AGUA SIN MÁSCARA ──
        yield return KushalRescue(t);

        if (rot != null) rot.hoursPerTurn = oldHours;
        Time.timeScale = oldScale;
        TestProbe.End();
    }

    /// <summary>Kushal (el jugador de la escena) empieza bajo el agua del tronco sin máscara (en Prueba_Mascaras ya empieza así;
    /// si no, se le pone ahí): nada hacia la estación más cercana (lo que haría quien juega siguiendo el aviso de la pantalla), la
    /// toca, se pone la máscara y desde entonces el aliento no baja.</summary>
    public static IEnumerator KushalRescue(ArbolCampos t, System.Action<GameObject, string> snap = null)
    {
        PlayerController pc = null;
        foreach (PlayerController p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None)) if (p.CompareTag("Player") && !p.name.Contains("TEST")) { pc = p; break; }
        if (pc == null) { TestProbe.Check("Kushal (el jugador) está en la escena", false); yield break; }
        GameObject k = pc.gameObject;
        CharacterController cc = k.GetComponent<CharacterController>();
        Vector3 back = k.transform.position; Quaternion backR = k.transform.rotation;
        BreathingMask bm = BreathingMask.Of(k.transform);
        if (bm.Worn) bm.Strip();
        SwimBody sb = SwimBody.Of(k.transform);
        bool startedUnder = t.InRootWater(sb.HeadPosition);
        // (2026-11-04) Que nada más lo mueva mientras dura la prueba: la jornada de Kushal (piloto automático) lo llevaba a su puesto
        // (un viaje por el tronco que lo sacaba del agua) y le SUJETABA el cuerpo, así que la entrada de la prueba no hacía nada
        // («puesto a 34 m … bajo el agua False», prueba 70).
        var paused = new List<Behaviour>();
        foreach (PlayerWorkday w in FindObjectsByType<PlayerWorkday>(FindObjectsSortMode.None)) if (w.enabled) { w.enabled = false; paused.Add(w); }
        if (k.TryGetComponent(out TreeTrip trip) && trip.enabled) trip.enabled = false;
        for (int i = 0; i < 8 && pc.BodyHeld; i++) pc.HoldBody(false);
        Vector3 at = t.World(new Vector3(-3f, 34f, 4f));
        for (int tries = 0; tries < 5 && !t.InRootWater(sb.HeadPosition); tries++)
        {
            if (cc != null) cc.enabled = false; k.transform.position = at; if (cc != null) cc.enabled = true; Physics.SyncTransforms();
            pc.SetSwimming(true);
            yield return null; yield return null;
        }
        bool placed = t.InRootWater(sb.HeadPosition);
        if (!placed) TestProbe.Check("Kushal queda bajo el agua del tronco para la prueba", false, $"en {k.transform.position}");
        yield return null;
        float breath0 = sb.Breath01;
        snap?.Invoke(k, "bajo_el_agua_sin_mascara");
        MaskStation target = MaskStation.Nearest(sb.HeadPosition, MaskStation.Kind.Station, BreathingMask.RescueRange, true, true);
        float dist0 = target != null ? Vector3.Distance(target.SwimPoint, sb.HeadPosition) : -1f;
        float t0 = Time.time, r0 = Time.realtimeSinceStartup, minB = 1f;
        while (!bm.Worn && target != null && Time.time - t0 < 60f && Time.realtimeSinceStartup - r0 < 60f)
        {
            Vector3 d = target.SwimPoint - sb.HeadPosition;
            Vector3 flat = new Vector3(d.x, 0f, d.z);
            if (flat.sqrMagnitude > 0.04f) k.transform.rotation = Quaternion.LookRotation(flat.normalized);
            if (!pc.IsSwimming && t.InRootWater(k.transform.position)) pc.SetSwimming(true);   // (dentro del tronco, nadando: lo movía la prueba)
            pc.scriptedInput = new Vector3(0f, Mathf.Clamp(d.y * 0.8f, -1f, 1f), flat.magnitude > 0.6f ? Mathf.Clamp01(flat.magnitude / 2f) : 0f);
            yield return null;
            minB = Mathf.Min(minB, sb.Breath01);
        }
        float took = Time.time - t0;
        snap?.Invoke(k, "se_pone_la_mascara");
        TestProbe.Check($"KUSHAL bajo el agua sin máscara ({(startedUnder ? "la escena empieza así" : "puesto a 34 m en el tronco")}) nada a la estación más cercana y se la pone",
            bm.Worn && bm.TakenFrom != null && bm.TakenFrom.kind == MaskStation.Kind.Station, $"estación a {dist0:0.0} m, puesta en {took:0} s, aliento {breath0:0.00} → mínimo {minB:0.00}");
        // Con la máscara: 20 s más bajo el agua, el aliento no baja.
        sb.breathHoldSeconds = 4f;
        pc.scriptedInput = new Vector3(0f, -0.3f, 0f);
        float minAfter = 1f; bool under = false;
        { float t1 = Time.time, r1 = Time.realtimeSinceStartup; while (Time.time - t1 < 20f && Time.realtimeSinceStartup - r1 < 40f) { if (!pc.IsSwimming && t.InRootWater(k.transform.position)) pc.SetSwimming(true); yield return null; if (WaterZone.Underwater(sb.HeadPosition)) under = true; if (Time.time - t1 > 2f) minAfter = Mathf.Min(minAfter, sb.Breath01); if (!bm.Worn) break; } }
        snap?.Invoke(k, "buceando_con_mascara");
        pc.scriptedInput = null;
        TestProbe.Check("con la máscara, Kushal tiene oxígeno ilimitado: 20 s bajo el agua con 4 s de aliento y no baja", bm.Worn && under && minAfter > 0.95f, $"bajo el agua {under}, aliento mínimo {minAfter:0.00}");
        sb.breathHoldSeconds = 50f;
        if (!startedUnder)
        {
            bm.Return(target);
            if (cc != null) cc.enabled = false; k.transform.SetPositionAndRotation(back, backR); if (cc != null) cc.enabled = true; Physics.SyncTransforms();
            pc.SetSwimming(false);
        }
        foreach (Behaviour b in paused) if (b != null) b.enabled = true;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Máscaras de los árboles (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloMascaras"); DontDestroyOnLoad(go);
        TreeMaskTest t = go.AddComponent<TreeMaskTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(TreeMaskTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}

/// <summary>La escena de prueba Prueba_Mascaras (Assets/Scenes/Pruebas; docs/escenas-de-prueba.md): el Árbol de los Campos solo, con
/// KUSHAL EMPEZANDO BAJO EL AGUA DEL TRONCO SIN MÁSCARA. Nada a la estación más cercana, se la pone y tiene oxígeno ilimitado.
/// Solo corre en esa escena (en SampleScene lo hace la 7.ª parte de <see cref="TreeMaskTest"/>).</summary>
public class KushalMaskSceneTest : MonoBehaviour, ITestUnit
{
    public const string Scene = "Prueba_Mascaras";
    public int Group => 72;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != Scene) return;
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<KushalMaskSceneTest>() == null) tr.gameObject.AddComponent<KushalMaskSceneTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Kushal empieza bajo el agua del tronco sin máscara: va a una estación y respira sin límite");
        ArbolCampos t = ArbolCampos.Instance;
        if (t == null) { TestProbe.Check("hay un edificio-árbol en la escena", false); TestProbe.End(); yield break; }
        yield return TreeMaskTest.KushalRescue(t);
        TestProbe.End();
    }
}
