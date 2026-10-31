using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS TÉCNICAS DE BRUCE LEE en el cuerpo de cubos (docs/microcosmos-bruce-lee.md §3): las posturas y secuencias que enseña
/// en su microcosmos del Dojo. Como el yoga (<see cref="SunSalutation"/>), cada postura es un <see cref="AsanaPoseSpell"/>
/// (rotación local por <see cref="BodyPart"/>, las no nombradas vuelven a 0) y las MANOS van por IK (<c>reaches</c>, en
/// coordenadas del cuerpo nativo: pies y≈0, cadera y≈0,95, hombros en (±0,26; 1,51; 0), +Z delante). El brazo mide ~0,57
/// hasta la muñeca: ningún objetivo pasa de ahí (la prueba <c>BruceLeeStoryTest</c> lo comprueba).
/// Ejes: X+ flexiona hacia delante (columna) / lleva el muslo atrás / dobla la rodilla; X− lleva el miembro delante; en los
/// muslos, Z+ mete la rodilla izquierda hacia dentro (Z− la derecha); Hips Y− adelanta el lado derecho (guardia de diestro
/// adelantado, como la de Lee).
/// Fuentes de la técnica: Lee, B. (1975) «Tao of Jeet Kune Do» (guardia, juego de pies, golpe recto); Lee, B. y Uyehara, M.
/// (1977) «Bruce Lee's Fighting Method» vol. 2 (juego de pies: paso y deslizamiento); Yip Chun y Connor, D. (1993) «Wing
/// Chun Martial Arts: Principles & Techniques» (yee ji kim yeung ma, golpes en cadena, muñeco de madera).
/// </summary>
public static class JeetKuneDo
{
    static readonly BodyPart[] All =
    {
        BodyPart.Hips, BodyPart.Back, BodyPart.Chest, BodyPart.Neck, BodyPart.Head,
        BodyPart.ShoulderLeft, BodyPart.ShoulderRight, BodyPart.ForearmLeft, BodyPart.ForearmRight, BodyPart.HandLeft, BodyPart.HandRight,
        BodyPart.ThighLeft, BodyPart.ThighRight, BodyPart.ShinLeft, BodyPart.ShinRight, BodyPart.FootLeft, BodyPart.FootRight,
    };

    /// <summary>Hombro (pivote) en el cuerpo nativo y largo del brazo hasta la muñeca (0,30 + 0,27): para la prueba de alcance.</summary>
    public static readonly Vector3 ShoulderL = new Vector3(-0.26f, 1.51f, 0f), ShoulderR = new Vector3(0.26f, 1.51f, 0f);
    public const float ArmReach = 0.57f;

    static (BodyPart, Vector3) E(BodyPart p, float x, float y = 0f, float z = 0f) => (p, new Vector3(x, y, z));

    static AsanaPoseSpell Make(string name, (BodyPart, Vector3)[] set, Vector3 handL, Vector3 handR, float stance = 0.3f)
    {
        var parts = new (BodyPart, Vector3)[All.Length];
        for (int i = 0; i < All.Length; i++)
        {
            Vector3 e = Vector3.zero;
            foreach (var s in set) if (s.Item1 == All[i]) e = s.Item2;
            parts[i] = (All[i], e);
        }
        return new AsanaPoseSpell
        {
            asanaName = name, parts = parts, duration = 1f, reachFraction = 0.5f, rootHeight = -1f, stanceWidth = stance,
            reaches = new[] { (BodyPart.HandLeft, handL), (BodyPart.HandRight, handR) },
        };
    }

    // ── Las piernas ──────────────────────────────────────────────────────────────────────────────────────────────────
    // YEE JI KIM YEUNG MA (la guardia del Wing Chun, «la postura de la cabra que sujeta el carácter dos»): pies a la anchura
    // de los hombros con las puntas hacia dentro, rodillas dobladas y metidas hacia dentro, pelvis adelantada, peso repartido.
    static (BodyPart, Vector3)[] WingChunLegs => new[]
    {
        E(BodyPart.ThighLeft, -25f, 0f, 6f), E(BodyPart.ThighRight, -25f, 0f, -6f),
        E(BodyPart.ShinLeft, 35f, 0f, -6f), E(BodyPart.ShinRight, 35f, 0f, 6f),
        E(BodyPart.FootLeft, -10f, 12f), E(BodyPart.FootRight, -10f, -12f),   // puntas hacia dentro
    };
    // BAI JONG (la guardia del Jeet Kune Do): el lado FUERTE (derecho) delante, de perfil a medias, rodillas algo dobladas,
    // el talón de atrás levantado (sobre la planta, listo para salir), el peso un poco más en la pierna de atrás.
    static (BodyPart, Vector3)[] BaiJongLegs(float leadThigh = -20f, float leadShin = 20f, float rearThigh = 18f, float rearShin = 15f, float rearFoot = 35f) => new[]
    {
        E(BodyPart.Hips, 0f, -30f), E(BodyPart.Chest, 0f, 15f), E(BodyPart.Head, 0f, 15f),   // el torso gira hacia el rival; la mirada, al frente
        E(BodyPart.ThighRight, leadThigh), E(BodyPart.ShinRight, leadShin), E(BodyPart.FootRight, -(leadThigh + leadShin)),
        E(BodyPart.ThighLeft, rearThigh), E(BodyPart.ShinLeft, rearShin), E(BodyPart.FootLeft, rearFoot),
    };
    static (BodyPart, Vector3)[] Join(params (BodyPart, Vector3)[][] g) { var l = new List<(BodyPart, Vector3)>(); foreach (var x in g) l.AddRange(x); return l.ToArray(); }

    // ── Las posturas ─────────────────────────────────────────────────────────────────────────────────────────────────
    /// <summary>Guardia del Wing Chun: la mano de delante (man sau) en la línea central y la de atrás (wu sau) protegiendo.</summary>
    public static AsanaPoseSpell WingChunStance() => Make("Yee ji kim yeung ma (guardia de Wing Chun)", WingChunLegs,
        new Vector3(-0.03f, 1.33f, 0.25f), new Vector3(0.03f, 1.38f, 0.45f));
    /// <summary>Golpe en cadena (lin wan kuen): puño vertical por la línea central; el otro vuelve al centro, listo para salir.</summary>
    public static AsanaPoseSpell ChainPunch(bool right) => Make(right ? "Golpe en cadena (derecha)" : "Golpe en cadena (izquierda)", WingChunLegs,
        right ? new Vector3(-0.04f, 1.30f, 0.22f) : new Vector3(-0.02f, 1.36f, 0.49f),
        right ? new Vector3(0.02f, 1.36f, 0.49f) : new Vector3(0.04f, 1.30f, 0.22f));
    /// <summary>Muñeco de madera (muk yan jong), 1: tan sau (palma arriba, desvía) y golpe con la otra.</summary>
    public static AsanaPoseSpell DummyTan() => Make("Muñeco: tan sau y golpe", WingChunLegs, new Vector3(-0.06f, 1.28f, 0.42f), new Vector3(0.03f, 1.38f, 0.47f));
    /// <summary>Muñeco de madera, 2: pak sau (la palma aparta el brazo del muñeco) y golpe con la otra.</summary>
    public static AsanaPoseSpell DummyPak() => Make("Muñeco: pak sau y golpe", WingChunLegs, new Vector3(-0.02f, 1.38f, 0.49f), new Vector3(-0.02f, 1.40f, 0.38f));
    /// <summary>Bai jong: la guardia del Jeet Kune Do (mano adelantada a la altura del hombro, la de atrás junto a la barbilla).</summary>
    public static AsanaPoseSpell OnGuard() => Make("Bai jong (guardia de Jeet Kune Do)", BaiJongLegs(), new Vector3(-0.08f, 1.45f, 0.2f), new Vector3(0.1f, 1.38f, 0.42f), 0.28f);
    /// <summary>Juego de pies, paso adelante: el pie de delante avanza; luego el de atrás se desliza y vuelve la guardia.</summary>
    public static AsanaPoseSpell StepForward() => Make("Paso adelante", BaiJongLegs(-35f, 25f, 25f, 12f, 40f), new Vector3(-0.08f, 1.45f, 0.2f), new Vector3(0.1f, 1.38f, 0.42f), 0.28f);
    /// <summary>Paso atrás: el pie de atrás retrocede primero y la pierna de delante, recogida, lo sigue.</summary>
    public static AsanaPoseSpell StepBack() => Make("Paso atrás", BaiJongLegs(-12f, 30f, 30f, 20f, 45f), new Vector3(-0.08f, 1.45f, 0.2f), new Vector3(0.1f, 1.38f, 0.42f), 0.28f);
    /// <summary>Golpe de una pulgada, cargado: el puño ya toca el blanco (brazo casi estirado), la cadera girada hacia atrás y
    /// las rodillas dobladas.</summary>
    public static AsanaPoseSpell OneInchLoaded() => Make("Golpe de una pulgada (cargado)",
        Join(BaiJongLegs(-25f, 35f, 22f, 30f, 40f), new[] { E(BodyPart.Hips, 0f, -40f), E(BodyPart.Chest, 0f, 20f), E(BodyPart.Head, 0f, 20f) }),
        new Vector3(-0.08f, 1.45f, 0.2f), new Vector3(0.12f, 1.36f, 0.38f), 0.3f);
    /// <summary>Golpe de una pulgada, impacto: la pierna de atrás se estira, la cadera gira hacia delante y el brazo termina
    /// recto; la fuerza sale del suelo y pasa por la cadera, no del brazo solo.</summary>
    public static AsanaPoseSpell OneInchImpact() => Make("Golpe de una pulgada (impacto)",
        Join(BaiJongLegs(-25f, 30f, 25f, 5f, 45f), new[] { E(BodyPart.Hips, 0f, -10f), E(BodyPart.Chest, 4f, 0f), E(BodyPart.Head, 0f, 5f) }),
        new Vector3(-0.08f, 1.45f, 0.2f), new Vector3(0.05f, 1.37f, 0.5f), 0.3f);

    /// <summary>Todas las posturas (la prueba las pone una a una en el cuerpo de Bruce Lee).</summary>
    public static List<AsanaPoseSpell> AllPoses() => new List<AsanaPoseSpell>
    {
        WingChunStance(), ChainPunch(true), ChainPunch(false), DummyTan(), DummyPak(), OnGuard(), StepForward(), StepBack(), OneInchLoaded(), OneInchImpact(),
    };

    // ── Las secuencias (formas) que hace Bruce Lee en cada paso de su historia ─────────────────────────────────────────
    /// <summary>Una forma: sus fotogramas y cuánto dura cada uno (s). Los golpes en cadena van rápidos (varios por segundo).</summary>
    public static (System.Func<AsanaPoseSpell> pose, float seconds)[] Form(string key)
    {
        switch (key)
        {
            case "wingchun": return new (System.Func<AsanaPoseSpell>, float)[] { (WingChunStance, 3f) };
            case "cadena":   return new (System.Func<AsanaPoseSpell>, float)[] { (() => ChainPunch(true), 0.3f), (() => ChainPunch(false), 0.3f) };
            case "muneco":   return new (System.Func<AsanaPoseSpell>, float)[] { (DummyTan, 0.7f), (DummyPak, 0.7f), (() => ChainPunch(true), 0.3f), (() => ChainPunch(false), 0.3f) };
            case "guardia":  return new (System.Func<AsanaPoseSpell>, float)[] { (OnGuard, 3f) };
            case "pasos":    return new (System.Func<AsanaPoseSpell>, float)[] { (OnGuard, 0.8f), (StepForward, 0.45f), (OnGuard, 0.6f), (StepBack, 0.45f) };
            case "pulgada":  return new (System.Func<AsanaPoseSpell>, float)[] { (OneInchLoaded, 1.6f), (OneInchImpact, 0.35f), (OnGuard, 1.2f) };
            case "notas":    return new (System.Func<AsanaPoseSpell>, float)[] { (DailyPoses.Sit, 4f) };
            case "agua":     return new (System.Func<AsanaPoseSpell>, float)[] { (WingChunStance, 4f) };
            default:         return new (System.Func<AsanaPoseSpell>, float)[0];
        }
    }

    public static readonly string[] FormKeys = { "wingchun", "cadena", "muneco", "guardia", "pasos", "pulgada", "notas", "agua" };
}
