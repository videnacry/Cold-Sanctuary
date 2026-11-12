using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DEL NADO (<see cref="SwimBody"/>, Berón 2026-10-29: «no he visto a nadie nadar, mucho menos bucear; en el mar Kushal
/// parece que camina o da saltitos»). En dos piscinas de prueba construidas lejos de todo (aire libre a 300 m de altura,
/// con su <see cref="WaterZone"/> y su fondo), con un nadador de prueba (CharacterController + PlayerController con entrada
/// simulada, cuerpo de cubos) y un personaje sin CharacterController:
///   (1) agua por debajo del pecho → VADEA de pie: no nada, va más despacio que andando y sin saltitos (± pocos cm);
///   (2) agua honda → NADA: cuerpo horizontal, brazadas (hombros que giran, ciclos completos), la cabeza fuera para respirar
///       y el andar sin tocar las piernas;
///   (3) BUCEA (abajo + adelante): baja de verdad, de cabeza, y gasta aliento; al volver arriba, el aliento se recupera;
///   (4) un personaje nada 15 m con <see cref="SwimBody.SwimTo"/> y vuelve con <see cref="PathMover"/>, siempre con el cuerpo
///       de nado (nunca de pie andando).
/// Cada espera, acotada. Deshace todo al acabar. Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class SwimTest : MonoBehaviour, ITestUnit
{
    public int Group => 54;   // tras la voz de Kushal (52): sus piscinas están lejos de todo y no mueve a nadie del santuario
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<SwimTest>() == null) tr.gameObject.AddComponent<SwimTest>();
    }

    static readonly Vector3 Origin = new Vector3(-900f, 300f, -900f);
    const float Surface = 306f;   // Origin.y + 6

    public IEnumerator Run()
    {
        TestProbe.Begin("Nadar y bucear (cuerpo horizontal, brazadas, vadear sin saltos, aliento, personajes que nadan)");
        GameObject root = new GameObject("PruebaNado");
        // Piscina honda (12 m) y piscina baja (0,8 m), las dos con la superficie a 306 m.
        Pool(root.transform, "Honda", Origin, 12f);
        Pool(root.transform, "Baja", Origin + new Vector3(100f, 0f, 0f), 0.8f);
        Physics.SyncTransforms();

        GameObject swimmer = Body(root.transform, "PruebaNadador", true, Origin + new Vector3(100f, 0f, -10f));
        PlayerController pc = swimmer.GetComponent<PlayerController>();
        SwimBody sb = SwimBody.Of(swimmer.transform);
        CreatureRig rig = swimmer.GetComponent<CreatureRig>();
        Transform hips = Joint(swimmer, BodyPart.Hips), shoulder = Joint(swimmer, BodyPart.ShoulderLeft);
        yield return null; yield return null;

        // ── 1. Agua baja: vadea ──
        Place(swimmer, new Vector3(Origin.x + 100f, Surface - 0.8f + 1.02f, Origin.z - 10f));
        yield return Frames(0.4f);
        pc.scriptedInput = new Vector3(0f, 0f, 1f);
        swimmer.transform.rotation = Quaternion.identity;
        Vector3 start = swimmer.transform.position;
        float yMin = float.MaxValue, yMax = float.MinValue, t0 = Time.time, end = Time.realtimeSinceStartup + 6f;
        bool everSwam = false, upright = true, waded = true;
        yield return Frames(0.3f);
        Vector3 from = swimmer.transform.position; float tFrom = Time.time;
        while (Time.time - t0 < 2.8f && Time.realtimeSinceStartup < end)
        {
            float y = swimmer.transform.position.y; yMin = Mathf.Min(yMin, y); yMax = Mathf.Max(yMax, y);
            everSwam |= pc.IsSwimming; waded &= pc.WadeDepth > 0.3f;
            if (Vector3.Angle(hips.up, Vector3.up) > 20f) upright = false;
            yield return null;
        }
        float wadeSpeed = Flat(swimmer.transform.position - from) / Mathf.Max(0.01f, Time.time - tFrom);
        TestProbe.Check("agua por debajo del pecho: vadea de pie (no nada)", !everSwam && upright && waded, $"nadó {everSwam}, erguido {upright}, agua {pc.WadeDepth:0.00} m");
        TestProbe.Check("vadeando avanza, más despacio que andando", wadeSpeed > 0.5f && wadeSpeed < pc.walkSpeed * 0.9f, $"{wadeSpeed:0.00} m/s (andar {pc.walkSpeed:0.0})");
        TestProbe.Check("vadeando no da saltitos (el pivote no sube ni baja más de 5 cm)", yMax - yMin < 0.05f, $"oscila {(yMax - yMin) * 100f:0.0} cm");
        pc.scriptedInput = Vector3.zero;

        // ── 2. Agua honda: nada horizontal con brazadas ──
        Place(swimmer, new Vector3(Origin.x - 8f, Surface - 0.3f, Origin.z - 8f));
        swimmer.transform.rotation = Quaternion.identity;
        yield return Frames(0.5f);
        TestProbe.Check("en agua honda empieza a nadar", pc.IsSwimming && sb.Active, $"nadando {pc.IsSwimming}");
        pc.scriptedInput = new Vector3(0f, 0f, 1f);
        yield return Frames(0.8f);
        float aMin = float.MaxValue, aMax = float.MinValue, fy0 = swimmer.transform.position.y, fyMin = fy0, fyMax = fy0;
        int frames = 0, horizontal = 0, headUp = 0, locked = 0, strokes0 = sb.Strokes;
        end = Time.realtimeSinceStartup + 6f; t0 = Time.time;
        while (Time.time - t0 < 3f && Time.realtimeSinceStartup < end)
        {
            frames++;
            float tilt = Vector3.Angle(hips.up, Vector3.up);
            if (tilt > 60f && tilt < 110f) horizontal++;
            if (sb.HeadAboveWater) headUp++;
            if (rig != null && rig.IsPoseLocked(BodyPart.ThighLeft) && rig.IsPoseLocked(BodyPart.ShoulderLeft)) locked++;
            float a = Vector3.SignedAngle(Vector3.up, shoulder.localRotation * Vector3.up, Vector3.right);
            aMin = Mathf.Min(aMin, a); aMax = Mathf.Max(aMax, a);
            float y = swimmer.transform.position.y; fyMin = Mathf.Min(fyMin, y); fyMax = Mathf.Max(fyMax, y);
            yield return null;
        }
        frames = Mathf.Max(1, frames);
        TestProbe.Check("nadando, el cuerpo va horizontal (boca abajo)", horizontal >= frames * 0.85f, $"{horizontal}/{frames} frames, estilo {sb.Style}, inclinación {sb.BodyPitch:0}°");
        TestProbe.Check("da brazadas (el hombro gira y completa ciclos)", aMax - aMin > 90f && sb.Strokes - strokes0 >= 2, $"hombro {aMin:0}…{aMax:0}°, {sb.Strokes - strokes0} ciclos a {sb.StrokeRate:0.00}/s");
        TestProbe.Check("en la superficie la cabeza queda fuera para respirar", headUp >= frames * 0.8f && sb.Breath01 > 0.97f, $"{headUp}/{frames} frames, aliento {sb.Breath01:0.00}");
        TestProbe.Check("nadando, el andar no mueve piernas ni brazos (el nado manda en sus huesos)", locked >= frames * 0.95f, $"{locked}/{frames}");
        TestProbe.Check("flota en la superficie (sin hundirse ni saltar)", fyMax - fyMin < 0.25f, $"oscila {(fyMax - fyMin) * 100f:0} cm");

        // ── 3. Bucear: baja de cabeza y gasta aliento; arriba lo recupera ──
        float yStart = swimmer.transform.position.y;
        pc.scriptedInput = new Vector3(0f, -1f, 1f);
        float maxPitch = 0f, breathMid = 1f; end = Time.realtimeSinceStartup + 7f; t0 = Time.time;
        while (Time.time - t0 < 3f && Time.realtimeSinceStartup < end)
        {
            maxPitch = Mathf.Max(maxPitch, Vector3.Angle(hips.up, Vector3.up));
            if (Time.time - t0 > 1.5f && breathMid >= 1f) breathMid = sb.Breath01;
            yield return null;
        }
        float dived = yStart - swimmer.transform.position.y, breathLow = sb.Breath01;
        TestProbe.Check("bucear baja de verdad (más de 2 m)", dived > 2f, $"bajó {dived:0.0} m");
        TestProbe.Check("al bucear el cuerpo va de cabeza (más allá de la horizontal)", maxPitch > 110f, $"inclinación máx. {maxPitch:0}°");
        TestProbe.Check("bajo el agua se gasta el aliento", !sb.HeadAboveWater && breathLow < 0.99f && breathLow < breathMid, $"aliento {breathMid:0.000} → {breathLow:0.000}");
        pc.scriptedInput = new Vector3(0f, 1f, 0f);
        end = Time.realtimeSinceStartup + 10f; t0 = Time.time;
        while (!sb.HeadAboveWater && Time.time - t0 < 8f && Time.realtimeSinceStartup < end) yield return null;
        pc.scriptedInput = Vector3.zero;
        end = Time.realtimeSinceStartup + 9f; t0 = Time.time;
        while (sb.Breath01 < 0.99f && Time.time - t0 < 7f && Time.realtimeSinceStartup < end) yield return null;
        TestProbe.Check("al salir a la superficie recupera el aliento", sb.HeadAboveWater && sb.Breath01 >= 0.99f, $"aliento {sb.Breath01:0.00}, cabeza fuera {sb.HeadAboveWater}");
        yield return Frames(1.5f);
        TestProbe.Check("parado en agua honda se mantiene a flote, vertical", sb.Style == SwimStyle.Tread && Vector3.Angle(hips.up, Vector3.up) < 35f, $"estilo {sb.Style}, inclinación {Vector3.Angle(hips.up, Vector3.up):0}°");
        pc.scriptedInput = null;
        Place(swimmer, new Vector3(Origin.x + 15f, Surface - 0.3f, Origin.z - 15f));   // fuera del camino del otro

        // ── 4. Un personaje nada 15 m (SwimTo) y vuelve (PathMover), siempre con el cuerpo de nado ──
        GameObject npc = Body(root.transform, "PruebaNadadorPNJ", false, Origin + new Vector3(-10f, 0f, 8f));
        SwimBody ns = SwimBody.Of(npc.transform);
        Transform nh = Joint(npc, BodyPart.Hips);
        CreatureRig nrig = npc.GetComponent<CreatureRig>();
        yield return null;
        Vector3 a0 = new Vector3(Origin.x - 10f, Surface - 0.3f, Origin.z + 8f);
        npc.transform.position = a0; npc.transform.rotation = Quaternion.LookRotation(Vector3.right);
        a0.y = ns.FloatPivotYFor(Surface);
        Vector3 b0 = a0 + new Vector3(15f, 0f, 0f);
        float left = 99f; int nf = 0, nh_ok = 0, nlock = 0; int ns0 = ns.Strokes;
        end = Time.realtimeSinceStartup + 30f; t0 = Time.time;
        while (left > 0.3f && Time.time - t0 < 25f && Time.realtimeSinceStartup < end)
        {
            left = ns.SwimTo(b0);
            if (Time.time - t0 > 1f)
            {
                nf++;
                float tilt = Vector3.Angle(nh.up, Vector3.up);
                if (tilt > 50f && tilt < 130f) nh_ok++;
                if (nrig != null && nrig.IsPoseLocked(BodyPart.ThighLeft)) nlock++;
            }
            yield return null;
        }
        nf = Mathf.Max(1, nf);
        TestProbe.Check("un personaje nada 15 m con SwimTo", left <= 0.3f, $"quedan {left:0.0} m");
        TestProbe.Check("…con el cuerpo de nado, nunca de pie andando", ns.Active && nh_ok >= nf * 0.9f && nlock >= nf * 0.95f && ns.Strokes - ns0 >= 3, $"horizontal {nh_ok}/{nf}, huesos del nado {nlock}/{nf}, {ns.Strokes - ns0} ciclos");

        PathMover pm = PathMover.Of(npc.transform); pm.Restart();
        left = 99f; nf = 0; nh_ok = 0; nlock = 0;
        Vector3 back = a0; back.y = Surface;   // un destino de los de siempre: el punto en el agua
        end = Time.realtimeSinceStartup + 30f; t0 = Time.time;
        while (left > 0.5f && Time.time - t0 < 25f && Time.realtimeSinceStartup < end)
        {
            left = pm.StepToward(back, 3f);
            if (Time.time - t0 > 1f)
            {
                nf++;
                float tilt = Vector3.Angle(nh.up, Vector3.up);
                if (tilt > 50f && tilt < 130f) nh_ok++;
                if (nrig != null && nrig.IsPoseLocked(BodyPart.ThighLeft)) nlock++;
            }
            yield return null;
        }
        nf = Mathf.Max(1, nf);
        TestProbe.Check("con PathMover, el tramo de agua también se nada (15 m de vuelta)", left <= 0.5f && ns.Active, $"quedan {left:0.0} m, nadando {ns.Active}");
        TestProbe.Check("…horizontal y a flote, sin andar por el fondo", nh_ok >= nf * 0.9f && nlock >= nf * 0.95f && Mathf.Abs(npc.transform.position.y - ns.FloatPivotYFor(Surface)) < 0.3f,
            $"horizontal {nh_ok}/{nf}, huesos del nado {nlock}/{nf}, altura {npc.transform.position.y - ns.FloatPivotYFor(Surface):+0.00;-0.00} m de la de flote");

        // La API de agua: dentro, encima y fuera de la piscina.
        TestProbe.Check("WaterZone sabe dónde hay agua (dentro, encima y fuera de la piscina)",
            WaterZone.Underwater(Origin) && WaterZone.SurfaceAt(Origin + Vector3.up * 20f, out float s) && Mathf.Abs(s - Surface) < 0.01f && !WaterZone.SurfaceAt(Origin + new Vector3(50f, 0f, 0f), out _));

        Destroy(root);
        yield return null;
        TestProbe.End();
    }

    // ── Escenario ─────────────────────────────────────────────────────────────────────────────────

    static void Pool(Transform parent, string name, Vector3 center, float depth)
    {
        var water = new GameObject("Agua_" + name);
        water.transform.SetParent(parent, false);
        water.transform.position = center;
        BoxCollider box = water.AddComponent<BoxCollider>();
        box.isTrigger = true; box.size = new Vector3(40f, 12f, 40f);
        water.AddComponent<WaterZone>();
        var floor = new GameObject("Fondo_" + name);
        floor.transform.SetParent(parent, false);
        floor.transform.position = new Vector3(center.x, Surface - depth - 0.5f, center.z);
        floor.AddComponent<BoxCollider>().size = new Vector3(40f, 1f, 40f);
    }

    // Un cuerpo de cubos de 1,75 m con los pies 1 m bajo el pivote. Con CharacterController y PlayerController (entrada
    // simulada) para el «jugador» de prueba; sin él, como los compañeros (cápsula).
    static GameObject Body(Transform parent, string name, bool player, Vector3 at)
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.position = at;
        GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Capsule);   // solo para medir la altura y los pies
        Object.DestroyImmediate(shape.GetComponent<Collider>());
        shape.transform.SetParent(g.transform, false);
        if (player)
        {
            CharacterController cc = g.AddComponent<CharacterController>();
            cc.height = 2f; cc.radius = 0.35f; cc.center = Vector3.zero;
        }
        else
        {
            CapsuleCollider c = g.AddComponent<CapsuleCollider>(); c.height = 2f; c.radius = 0.3f;
        }
        CubeHumanoid.Dress(g, new Color(0.2f, 0.4f, 0.6f), new Color(0.8f, 0.65f, 0.5f), 1.75f);
        if (player) g.AddComponent<PlayerController>();
        return g;
    }

    static Transform Joint(GameObject g, BodyPart p)
    {
        foreach (CubeJoint j in g.GetComponentsInChildren<CubeJoint>(true)) if (j.part == p && j.name == p.ToString()) return j.transform;
        return g.transform;
    }

    static void Place(GameObject g, Vector3 p)
    {
        CharacterController cc = g.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        g.transform.position = p;
        if (cc != null) cc.enabled = true;
    }

    static IEnumerator Frames(float seconds)
    {
        float t = Time.time, end = Time.realtimeSinceStartup + seconds * 3f + 1f;
        while (Time.time - t < seconds && Time.realtimeSinceStartup < end) yield return null;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
}
